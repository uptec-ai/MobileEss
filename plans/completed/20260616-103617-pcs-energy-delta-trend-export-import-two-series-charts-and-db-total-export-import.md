# PCS energy delta trend export import two series charts and db total export import

- Task ID: 20260616-103617-pcs-energy-delta-trend-export-import-two-series-charts-and-db-total-export-import
- Status: active
- Created: 2026-06-16 10:36:17

## Goal

PCS Active Power Trend을 Total Export/Import Energy의 변화량(Δ) 2개 시리즈로 변경.
- 실시간: 누적 export/import(kWh)를 인터벌 버킷팅, 버킷 간 증가분(Δ)을 2개 시리즈로 표출.
- DB: pcs_total_export_kwh / pcs_total_import_kwh(누적) 저장. pcs_active_power_kw는
  컬럼 유지하되 사용 중단(INSERT/조회 제거, DROP 안 함).
- History PCS trend도 버킷별 Δ(max-min) 2개 시리즈.
- export=초록(#76F7A8), import=파랑(#4EA5FF).

## Scope

- DbManager: 신규 컬럼 ALTER ADD, InsertCompressedRawData에서 active_power 제거 +
  export/import 추가, Resolve 헬퍼 교체.
- PcsModel: PowerTrendSample(export/import), UpdateDailyPowerTrend(에너지 읽기),
  RebuildDailyPowerTrendSeries(2 시리즈 Δ), DailyExport/ImportTrendSeries 속성.
- PCSView.xaml/.cs: 2 시리즈 차트(초록/파랑) + 범례, Y축 auto-range, 제목/단위.
- HistoryViewModel: LoadPcsRows 쿼리(버킷 Δ), HistoryDataRow Export/ImportDelta,
  PcsExport/ImportTrendSeries.
- HistoryView.xaml: PCS 차트 2 시리즈(초록/파랑) + 범례.

## Impacted Files

- EMS_PJT_Hamburger/Models/Managers/DbManager.cs
- EMS_PJT_Hamburger/Models/Client/PCS/PcsModel.cs
- EMS_PJT_Hamburger/Views/PCSView.xaml (+ .xaml.cs)
- EMS_PJT_Hamburger/ViewModels/HistoryViewModel.cs
- EMS_PJT_Hamburger/Views/HistoryView.xaml

## Test Strategy

- Build: 별도 OutputPath 컴파일 검증.
- E2E(수동): 실시간 export/import Δ 2색 표출, History Today/Week Δ 표출, DB 신규 컬럼 적재.

## Rollback

- 위 5개 파일 원복. 신규 DB 컬럼은 nullable라 남아도 무해(원하면 수동 DROP).

## Notes

- 변화량 = 버킷별 누적값 증가분. 음수(카운터 리셋)는 0으로 클램프.
- pcs_active_power_kw 컬럼은 보존(사용자 선택: 사용만 중단).
