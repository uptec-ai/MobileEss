# Restore daily cumulative energy display

- Task ID: 20260804-171025-restore-daily-cumulative-energy-display
- Status: active
- Created: 2026-08-04 17:10:25

## Goal

주석 상태인 "수전/송전 누적 전력량(일간)" 표시를 복원하고 실데이터로 연결한다.
(KongBoard 남은 목표 #4 "일일 누적 전력량 조회(주석상태)")

일간값 = 현재 총 누적 전력량(GridTotalImport/ExportedActivePower) − 전일 마감 누적값.
전일 마감값은 이미 운영 중인 `tb_pcs_grid`(하루 1행, `UpsertPcsGridDailyTotals`로 당일 행 갱신)에서 조회한다.

## Scope

- 포함
  - `PcsViewModel`: LoadItems의 주석 2행("수전/송전 누적 전력량(일간)")을 끝(인덱스 17/18)에 복원 —
    기존 인덱스 참조(0~16)를 깨지 않는 위치.
  - `PcsModel`: `ChangeInformation`의 UI 디스패치 블록(주석 호출부 1166~1169 자리)에서
    일간값 계산·표시 갱신. 전일 마감 기준값은 `tb_pcs_grid`에서 Task.Run으로 1회/일 로드(날짜 변경 시 재로드).
    전일 행이 없으면(최초 가동일) 현재 총량을 기준값으로 사용(그 시점부터 0 집계) 후 로그 기록.
- 제외
  - `Daily*EnergySummarySeries` 차트 인프라 주석 블록(PcsModel 245~470행대) — 바인딩할 XAML UI가
    존재하지 않아(활성 차트는 Δ 트렌드 `Daily*TrendSeries`뿐) 복원 시 데드 코드. 주석 유지.
  - DbManager 등 공유 파일 수정 없음(기존 공개 API `GetDataSetByQuery`만 사용).

## Impacted Files

- `EMS_PJT_Hamburger/ViewModels/PcsViewModel.cs` (LoadItems 2행 복원)
- `EMS_PJT_Hamburger/Models/Client/PCS/PcsModel.cs` (일간값 계산/갱신 로직, 주석 호출부 대체)

## Test Strategy

- Unit: 테스트 프로젝트 없음(하네스 경고 후 통과).
- Integration: 없음(스크립트 스켈레톤 통과).
- Static analysis: `run-static-analysis.ps1`.
- Build: `run-build.ps1` (MSBuild) 통과 필수.
- E2E: `run-e2e.ps1`. 수동 검증 — PCS 연결(루프백 시뮬레이터 가능) 상태에서 Load 섹션에
  일간 수전/송전 항목 표시 및 tb_pcs_grid 전일 행 유무에 따른 기준값 동작 확인.

## Rollback

- 커밋 전: `git checkout -- EMS_PJT_Hamburger/ViewModels/PcsViewModel.cs "EMS_PJT_Hamburger/Models/Client/PCS/PcsModel.cs"`
- 커밋 후: 해당 커밋 `git revert`. DB 스키마/데이터 변경 없음(조회만).

## Notes

- Harness guard must pass before app code edits.
- 한글 주석 포함 .cs는 UTF-8 BOM 저장.
- LoadItems 표시 갱신은 UI 스레드(디스패치 블록)에서, 기준값 DB 조회는 Task.Run에서 수행.
