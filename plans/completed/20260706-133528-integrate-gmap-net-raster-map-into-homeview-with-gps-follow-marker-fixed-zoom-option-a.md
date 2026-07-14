# Integrate GMap.NET raster map into HomeView with GPS follow marker fixed zoom (option A)

- Task ID: 20260706-133528-integrate-gmap-net-raster-map-into-homeview-with-gps-follow-marker-fixed-zoom-option-a
- Status: active
- Created: 2026-07-06 13:35:28

## Goal

HomeView GPS 패널(Border 338,65 · 633x184) 안에 GMap.NET 라스터 지도를 1열 배치.
OSM 타일, 줌 고정, 중심=GPS lat/lon(follow), 중앙 마커. 오프라인은 SQLite 캐시(네이티브 interop).

## Scope

- libs\ 에 DLL 직접 참조(패키지 targets 회피): GMap.NET.Core(net48),
  GMap.NET.WindowsPresentation(net48), Newtonsoft.Json(net45), System.Data.SQLite(managed)
  + 네이티브 SQLite.Interop.dll(x86/x64).
- csproj: 4개 Reference + SQLite.Interop을 $(OutDir)x86|x64로 복사하는 target.
- HomeView.xaml: GPS Border 내부를 1열 Grid(행0 GPS 텍스트, 행1 GMapControl)로.
- HomeView.xaml.cs: 지도 초기화(OSM, 고정줌, 캐시), GPS 갱신 시 Position 이동 + 중앙 마커.
- 제외(다음): NetTopologySuite + 전국 시/도/구 GeoJSON 리버스지오코딩 텍스트, 오프라인 타일 프리캐시.

## Impacted Files

- (new) EMS_PJT_Hamburger/libs/*.dll (+ SQLite.Interop x86/x64)
- EMS_PJT_Hamburger/EMS_PJT_Hamburger.csproj
- EMS_PJT_Hamburger/Views/HomeView.xaml (+ .xaml.cs)

## Test Strategy

- Build: 별도 OutputPath 컴파일(ExitCode=0). 런타임 렌더/오프라인 캐시는 하드웨어+실행 필요(헤드리스 검증 불가).

## Rollback

- libs 삭제 + csproj/HomeView/HomeView.xaml.cs 원복.

## Notes

- GMap.NET.Core는 Newtonsoft.Json + System.Data.SQLite를 하드 참조(런타임 필수).
- 네이티브 SQLite.Interop은 오프라인 SQLite 캐시에만 필요(온라인 렌더는 관리 DLL만으로 동작).
- MSBuildMajorVersion safeguard 선적용됨(모던 패키지 추가 대비).
