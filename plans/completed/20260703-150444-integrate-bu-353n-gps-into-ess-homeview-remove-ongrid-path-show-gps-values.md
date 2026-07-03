# Integrate BU-353N GPS into ESS HomeView remove OnGrid path show gps values

- Task ID: 20260703-150444-integrate-bu-353n-gps-into-ess-homeview-remove-ongrid-path-show-gps-values
- Status: active
- Created: 2026-07-03 15:04:44

## Goal

GPSTester의 GPS 파이프라인(GpsService/NmeaParser/GpsData/SatelliteInfo)을 ESS로 이식하고
HomeView에서 Discharge_OnGrid Path를 제거, 그 빈 공간에 GPS값(위도/경도/Fix/위성수)을 표시.
포트는 App.config 고정(GpsPort/GpsBaud, 기본 115200). 미설정 시 '--' 표시.

## Scope

- 신규 파일(Models/Client/GPS/): GpsData, SatelliteInfo, NmeaParser, GpsService
  (네임스페이스 EMS_PJT_Hamburger.Models.Client.GPS). csproj Compile Include 등록.
- App.config: GpsPort(빈값)/GpsBaud(115200) 추가.
- HomeViewModel: GPS 서비스/파서 + Gps* 표시 속성 + config 연결 + Dispose.
- HomeView.xaml: Discharge_OnGrid Path 제거 + GPS 패널 Border 추가.

## Impacted Files

- (new) EMS_PJT_Hamburger/Models/Client/GPS/*.cs
- EMS_PJT_Hamburger/EMS_PJT_Hamburger.csproj
- EMS_PJT_Hamburger/App.config
- EMS_PJT_Hamburger/ViewModels/HomeViewModel.cs
- EMS_PJT_Hamburger/Views/HomeView.xaml

## Test Strategy

- Build: 별도 OutputPath 컴파일 검증(하드웨어 없이 라이브 검증 불가).
- E2E(수동, 장치 연결 후): App.config에 GpsPort 설정 -> HomeView에 위도/경도/Fix/위성수 표시.

## Rollback

- 신규 파일 삭제 + csproj/App.config/HomeViewModel/HomeView 원복.

## Notes

- BU-353N: 115200 8N1 NMEA. SentenceReceived(백그라운드)->Dispatcher로 UI 반영.
- 자동 COM 스캔은 안 함(블루투스 COM 보호). GpsPort 미설정 시 연결 안 함.
- 향후 4번(지도)에서 재사용하려면 App 레벨 매니저로 승격 가능.
