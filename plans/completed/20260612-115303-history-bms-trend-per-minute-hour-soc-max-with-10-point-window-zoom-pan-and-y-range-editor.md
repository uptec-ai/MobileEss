# History BMS Trend per-minute/hour SOC max with 10-point window zoom pan and Y range editor

- Task ID: 20260612-115303-history-bms-trend-per-minute-hour-soc-max-with-10-point-window-zoom-pan-and-y-range-editor
- Status: active
- Created: 2026-06-12 11:53:03

## Goal

History View의 BMS Trend 차트를 PCS Trend와 동일 규칙으로 맞춘다.
- Today(단일 일자): 1분 간격 SOC 분당 최대값 표시, 휠 줌/드래그 팬으로 이전 값 조회.
- Week(다중 일자): 1시간 간격 SOC 시간당 최대값 표시, 휠 줌/드래그 팬으로 이전 값 조회.
- BMS Trend 우측 상단에 Y축 범위(Min~Max) 설정용 TextEdit 추가.
- 더블클릭 시 기본 범위(마지막 10개 윈도)로 리셋.

## Scope

- 포함:
  - HistoryViewModel: LoadBmsRows 단일/다중 일자 granularity 분기, _bmsTrendInterval,
    BmsTrendDefaultVisibleRange(마지막 10개 버킷) 계산/노출/초기화.
  - HistoryView.xaml: BMS XAxis/YAxis 이름 + AutoRange=Never(+Y 0~100), 모디파이어
    Rollover+MouseWheelZoom+ZoomPan, Y Min/Max TextEdit, MouseDoubleClick.
  - HistoryView.xaml.cs: BMS 리셋/Y적용 핸들러, 속성변경/로드/더블클릭 처리.
  - 부수: 두 차트 부제목에서 "Hourly" 문구 제거(분/시 혼용이라 부정확).
- 제외: DB 스키마 변경, PCS 동작 변경.

## Impacted Files

- EMS_PJT_Hamburger/ViewModels/HistoryViewModel.cs
- EMS_PJT_Hamburger/Views/HistoryView.xaml
- EMS_PJT_Hamburger/Views/HistoryView.xaml.cs

## Test Strategy

- Unit/Integration: 해당 없음.
- Build: 별도 OutputPath로 컴파일 검증(실행 중 앱이 bin 잠금 가능).
- E2E(수동): Today 분단위 10개/Week 시간단위 10개 표시, 휠·드래그 이전값 조회,
  Y Min/Max 편집 반영, 더블클릭 리셋.

## Rollback

- 위 3개 파일을 변경 이전 상태로 되돌린다.

## Notes

- PCS Trend와 동일 패턴(_pcsTrendInterval/PcsTrendDefaultVisibleRange) 재사용.
