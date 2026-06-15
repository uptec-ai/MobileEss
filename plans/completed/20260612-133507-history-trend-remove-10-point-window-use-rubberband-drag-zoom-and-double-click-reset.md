# History trend remove 10-point window use rubberband drag zoom and double-click reset

- Task ID: 20260612-133507-history-trend-remove-10-point-window-use-rubberband-drag-zoom-and-double-click-reset
- Status: active
- Created: 2026-06-12 13:35:07

## Goal

History View의 PCS/BMS Trend 차트 상호작용을 변경한다.
- X축 "최근 10개만 표시" 기본 가시 윈도 삭제 -> 전체 데이터 범위 표시.
- 마우스 좌클릭 드래그로 드래그한 영역 확대(RubberBand 박스 줌).
- 더블클릭으로 초기화(전체 범위 + Y는 편집기 값).

## Scope

- 포함:
  - HistoryViewModel: BuildTrendDefaultVisibleRange를 전체 범위(첫~마지막 버킷)로 변경,
    MaxPcsTrendVisiblePoints 상수 및 관련 생성자 초기화 제거/단순화.
  - HistoryView.xaml: PCS/BMS 모디파이어를 Rollover + RubberBandXyZoomModifier로 변경
    (MouseWheelZoom/ZoomPan 제거).
- 제외:
  - 분단위/시간단위 조회 granularity는 유지(요청 대상 아님).
  - DB/Y편집기 로직 변경 없음.

## Impacted Files

- EMS_PJT_Hamburger/ViewModels/HistoryViewModel.cs
- EMS_PJT_Hamburger/Views/HistoryView.xaml

## Test Strategy

- Build: 별도 OutputPath 컴파일 검증.
- E2E(수동): 로드 시 전체 범위 표시, 드래그 영역 확대, 더블클릭 시 전체 범위로 초기화.

## Rollback

- 위 2개 파일을 변경 이전 상태로 되돌린다.

## Notes

- 코드비하인드 리셋 로직(ResetPcs/BmsTrendRange)은 그대로 재사용(기본 범위만 전체로 바뀜).
- RubberBand는 X/Y 박스 줌, 더블클릭 리셋이 Y를 편집기 값으로 복원.
