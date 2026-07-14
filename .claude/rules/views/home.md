---
paths:
  - "EMS_PJT_Hamburger/Views/HomeView.xaml"
  - "EMS_PJT_Hamburger/Views/HomeView.xaml.cs"
  - "EMS_PJT_Hamburger/ViewModels/HomeViewModel.cs"
---
# View: Home

## 목적
ESS 전체 개요 화면. 운전모드(충전/방전/대기)·에너지 흐름 애니메이션, PCS/BMS 빠른 제어(충전·방전·정지·릴레이), 목표 부하 선택, 그리고 **GPS 위치 + 오프라인 지도**를 표시.

## 상태
구현 완료. 지도는 GMap.NET → DevExpress 오프라인 타일로 이전 완료(GPS+지도 작업 브랜치 `feature/gps`).

## 담당 ViewModel / 소유
`HomeViewModel`(= `App.HomeVm`, App.xaml.cs `InitViews`에서 생성·DataContext 지정). 종료 시 `App.OnExit → HomeVm.Dispose()`로 GPS 재연결 루프 + SyncSystemModeAsync 루프 + 시리얼 해제.

## 데이터 & 외부 I/O
- GPS: `Models/Client/GPS/*`, App.config `GpsPort`(COM19)/`GpsBaud`(4800).
- PCS/BMS 상태는 `App.PcsVm`/`App.BmsVm`에 바인딩(`Source={x:Static Application.Current}`).
- 지도 타일 `Maps/tiles/{z}/{x}/{y}.png`, 시/도/군 `Maps/skorea_muni.json`.

## UI 표면
- GPS 패널(Row0): `GpsRegion`(시/도/군)·`GpsFixStatus`·`GpsLatitude/Longitude`·`GpsSatelliteCount`·`GpsIsValid`.
- 지도(Row1): `dxm:MapControl x:Name="Map"` + `MarkerStorage`(중앙 마커=GpsRegion) + 우하단 배율 콤보(12/10/8).
- 상단: 운전모드, PCS 충/방/정지, BMS 릴레이, Touch Keyboard/BMS Guard 토글, 목표 Load(OffGrid/Vehicle).
- 장비 클릭 네비게이션: PCS/BMS Border → PCSView/BMSView.

## 주의사항 / 규칙
- 지도/GPS 세부 규칙은 `.claude/rules/gps-map.md` 참조.
- 마커 라벨/센터는 `GpsRegionChanged`/`GpsPositionChanged`(코드비하인드 구독, Loaded/Unloaded에서 관리, VM은 App 소유라 여기서 Dispose 금지).
- 한글 주석/문자열 → UTF-8 BOM 유지.

## 관련 문서
`.claude/rules/gps-map.md`, `.claude/docs/README.md`
