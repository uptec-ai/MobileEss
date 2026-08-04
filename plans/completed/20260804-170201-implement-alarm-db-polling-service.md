# Implement alarm DB polling service

- Task ID: 20260804-170201-implement-alarm-db-polling-service
- Status: active
- Created: 2026-08-04 17:02:01

## Goal

`AlarmService`(현재 전체 주석 스텁)를 실구현해 알람 DB(`tb_ems_alarm`)를 주기 폴링하고,
알람 상세 창이 열려 있는 동안 신규 알람이 수동 새로고침 없이 목록에 실시간 반영되게 한다.
(KongBoard 남은 목표 #3 "Alarm Service (알람 DB 폴링) 구현")

## Scope

- 포함
  - `AlarmService`: DispatcherTimer 기반 주기 폴링(5초), 마지막 `alarm_id` 이후 신규 행 조회,
    `AlarmsArrived` 이벤트 발행. DB 접근은 기존 공개 API(`DbManager.GetDataSetByQuery`)만 사용.
  - `BMSViewModel.OpenAlarmsWindow`: 서비스 이벤트를 구독해 열린 알람 창 VM의 `Alarms` 컬렉션에
    신규 알람을 선두 삽입(occurred_at desc 순서 유지). 창 닫힘/Dispose 시 구독 해제.
- 제외
  - 공유 파일(`Models/Managers/DbManager.cs`, `ViewModels/AlarmDetailWindowViewModel.cs`) 수정 —
    worktree 라우팅 규칙상 메인 저장소 소관이며, 이번 구현은 기존 공개 API만으로 가능.
  - PCS 측 알람 폴링(현재 PCS는 폴링 중 실시간 fault 목록을 자체 보유) — 필요 시 후속 태스크.

## Impacted Files

- `EMS_PJT_Hamburger/Models/Client/BMS/AlarmService.cs` (폴링 실구현)
- `EMS_PJT_Hamburger/ViewModels/BMSViewModel.cs` (이벤트 구독/해제 배선)

## Test Strategy

- Unit: 테스트 프로젝트 없음(하네스 경고 후 통과) — KongBoard 목표 #6 보류 상태.
- Integration: 없음(스크립트 스켈레톤 통과).
- Static analysis: `run-static-analysis.ps1`.
- Build: `run-build.ps1` (MSBuild Debug|AnyCPU) 통과 필수.
- E2E: `run-e2e.ps1`. 수동 검증 절차 — 앱 실행 → BMS 알람 창 열기 →
  `insert into tb_ems_alarm(source, alarm_code, alarm_name) values('BMS', 99, '수동 테스트')`
  실행 → 5초 내 목록 선두에 표시 확인.

## Rollback

- 커밋 전: `git checkout -- EMS_PJT_Hamburger/Models/Client/BMS/AlarmService.cs EMS_PJT_Hamburger/ViewModels/BMSViewModel.cs`
- 커밋 후: 해당 커밋 `git revert`. DB 스키마 변경 없음(기존 테이블만 조회)이라 데이터 롤백 불필요.

## Notes

- Harness guard must pass before app code edits.
- 한글 주석 포함 .cs는 UTF-8 BOM 저장(csc CP949 오독 방지).
- DispatcherTimer Tick은 UI 스레드에서 돌므로 DB 조회는 Task.Run으로 백그라운드 실행,
  결과 반영만 Dispatcher로 마샬링. 재진입 방지 플래그 사용.
