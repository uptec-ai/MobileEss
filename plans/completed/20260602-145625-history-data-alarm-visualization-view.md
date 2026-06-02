# History Data Alarm Visualization View

- Task ID: 20260602-145625-history-data-alarm-visualization-view
- Status: active
- Created: 2026-06-02 14:56:25

## Goal

HistoryView에서 PCS/BMS 데이터 및 알람을 조회하고, 주요 값을 그래프 형태로 확인할 수 있는 테마 일관 UI를 구현한다.

## Scope

Included:
- HistoryView 화면 레이아웃 개선
- HistoryViewModel 추가 및 App DataContext 연결
- PCS/BMS 최근 데이터, EMS 알람 조회 기능
- 간단한 추세 그래프 표시용 데이터 구성

Excluded:
- DB schema 변경
- 대규모 DbManager 리팩토링
- 실제 데이터 저장 주기 변경

## Impacted Files

- EMS_PJT_Hamburger/Views/HistoryView.xaml
- EMS_PJT_Hamburger/ViewModels/HistoryViewModel.cs
- EMS_PJT_Hamburger/App.xaml.cs
- EMS_PJT_Hamburger/EMS_PJT_Hamburger.csproj

## Test Strategy

- Unit: harness unit gate; 테스트 프로젝트 부재 시 경고 확인
- Integration: harness integration gate; 테스트 프로젝트 부재 시 경고 확인
- Static analysis: harness static analysis gate
- Build: MSBuild Release Any CPU
- E2E: WPF release artifact validation

## Rollback

추가된 HistoryViewModel과 HistoryView.xaml 변경, App.xaml.cs DataContext 연결, csproj compile include를 되돌리면 기존 HistoryView 상태로 복귀한다.

## Notes

- Harness guard must pass before app code edits.
