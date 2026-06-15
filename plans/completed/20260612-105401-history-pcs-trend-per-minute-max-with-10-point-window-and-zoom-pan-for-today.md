# History PCS Trend per-minute max with 10-point window and zoom pan for Today

- Task ID: 20260612-105401-history-pcs-trend-per-minute-max-with-10-point-window-and-zoom-pan-for-today
- Status: active
- Created: 2026-06-12 10:54:01

## Goal

History View의 PCS Trend 차트를 PCS View의 Active Power Trend 차트처럼 동작시킨다.
- Today(단일 일자) 조회 시 PCS Trend를 분단위(date_trunc 'minute') 최대값으로 조회.
- X축에 한 번에 최대 10개 포인트만 보이도록 기본 가시 범위 제한.
- 휠 줌 인/아웃 + 드래그 팬으로 이전 시간대 값 조회.
- 더블클릭 시 기본 범위(마지막 10개 윈도)로 리셋, Y축은 편집기 값(기본 0~100) 유지.

## Scope

- 포함:
  - HistoryViewModel: 단일 일자 -> 분단위, 다중 일자 -> 시간단위 조회 분기.
  - PcsTrendDefaultVisibleRange(마지막 10개 버킷) 계산/노출.
  - HistoryView.xaml: PCS XAxis 이름 부여 + AutoRange=Never, 모디파이어를
    Rollover + MouseWheelZoom + ZoomPan으로 변경.
  - HistoryView.xaml.cs: 데이터 로드/더블클릭 시 기본 범위 리셋(PCS View 패턴 차용).
- 제외:
  - BMS Trend(요청 대상 아님, 기존 시간단위 유지).
  - DB 스키마/ViewModel 외 로직 변경 없음.

## Impacted Files

- EMS_PJT_Hamburger/ViewModels/HistoryViewModel.cs
- EMS_PJT_Hamburger/Views/HistoryView.xaml
- EMS_PJT_Hamburger/Views/HistoryView.xaml.cs

## Test Strategy

- Unit: 해당 없음(테스트 프로젝트 없음).
- Integration: 해당 없음.
- Static analysis: 하네스 스텁 통과.
- Build: MSBuild Debug 성공.
- E2E(수동): Today 조회 시 분단위 10개 표시, 휠/드래그로 이전 값 조회, 더블클릭 리셋.

## Rollback

- 위 3개 파일을 변경 이전 상태로 되돌린다.

## Notes

- truncUnit('minute'/'hour')은 내부 상수라 SQL 주입 위험 없음.
- 가시 윈도 = interval * 10 (PCS View의 _powerTrendVisibleWindow 패턴과 동일).
