# History PCS Trend lock Y axis 0-100 and add Y range editor

- Task ID: 20260611-133032-history-pcs-trend-lock-y-axis-0-100-and-add-y-range-editor
- Status: active
- Created: 2026-06-11 13:30:32

## Goal

PCS Trend 차트 더블클릭 시 Y축이 "데이터 min~max → 0~100" 2단계로 바뀌는 문제를 없애고,
항상 0~100으로 고정한다. 추가로 PCS Trend 패널 우측 상단에 Y축 범위(Min~Max)를
조절할 수 있는 TextEdit를 제공한다.

## Scope

- 포함:
  - 중복된 `ZoomExtentsModifier`(전축 줌) 제거, X축 전용만 유지.
  - 더블클릭 시 Y축을 편집기 값(기본 0~100)으로 결정적으로 재적용.
  - PCS Trend 헤더 우측에 Y Min/Max `dxe:TextEdit` 추가 및 코드비하인드 연결.
- 제외:
  - BMS Trend 차트(요청 대상 아님).
  - ViewModel/DB 로직 변경 없음(View 계층 한정).

## Impacted Files

- EMS_PJT_Hamburger/Views/HistoryView.xaml
- EMS_PJT_Hamburger/Views/HistoryView.xaml.cs

## Test Strategy

- Unit: 해당 없음(테스트 프로젝트 없음, 하네스가 경고 후 통과).
- Integration: 해당 없음.
- Static analysis: 하네스 스텁 통과.
- Build: MSBuild Debug/Release 빌드 성공.
- E2E: 수동 확인 - 더블클릭 시 Y 0~100 유지, Min/Max 편집 시 Y축 반영.

## Rollback

- HistoryView.xaml / HistoryView.xaml.cs 두 파일을 변경 이전 상태로 되돌린다.

## Notes

- Harness guard must pass before app code edits.
- Y축 편집기는 Min/Max 두 개의 TextEdit로 구성(범위 조절은 두 경계 필요).
