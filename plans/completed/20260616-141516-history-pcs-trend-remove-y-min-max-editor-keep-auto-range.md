# History PCS trend remove Y min max editor keep auto range

- Task ID: 20260616-141516-history-pcs-trend-remove-y-min-max-editor-keep-auto-range
- Status: active
- Created: 2026-06-16 14:15:16

## Goal

History PCS Trend(변화량 Δ kWh)의 Y축을 auto-range(이미 적용)로 두고, 더 이상
의미 없는 Y Min/Max 편집기를 제거한다. BMS Trend(SOC 0~100) 편집기는 유지.

## Scope

- HistoryView.xaml: PCS Trend 헤더의 Y Min/Max 편집기 StackPanel 제거.
- HistoryView.xaml.cs: PcsYRange_EditValueChanged, ApplyPcsYRange 제거,
  ResetPcsTrendRange에서 ApplyPcsYRange 호출 제거(X만 리셋).
- 유지: BMS 편집기(ApplyBmsYRange/BmsYRange_EditValueChanged), ToDouble, DefaultPcsYMin/Max(BMS가 사용).

## Impacted Files

- EMS_PJT_Hamburger/Views/HistoryView.xaml
- EMS_PJT_Hamburger/Views/HistoryView.xaml.cs

## Test Strategy

- Build: 별도 OutputPath 컴파일 검증.
- E2E(수동): PCS 차트 Y가 Δ 데이터에 맞춰 자동, 편집기 없음. BMS 편집기 정상.

## Rollback

- 두 파일 원복.

## Notes

- PCS Y축은 XAML에서 이미 AutoRange=Always, AxisTitle=kWh. 편집기/코드만 정리.
