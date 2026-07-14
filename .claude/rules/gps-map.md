---
paths:
  - "EMS_PJT_Hamburger/Models/Client/GPS/**"
  - "EMS_PJT_Hamburger/Views/HomeView.xaml*"
  - "EMS_PJT_Hamburger/Converters/**"
  - "EMS_PJT_Hamburger/Maps/**"
---
# GPS + 오프라인 지도 규칙

매칭 파일 편집 시에만 로드.

- **지도 스택**: DevExpress `MapControl`(`DevExpress.Xpf.Map`, 이미 참조됨). GMap.NET/SQLite 아님. 완전 오프라인 래스터.
- **타일**: `Maps/tiles/{z}/{x}/{y}.png`. 빌드 시 출력폴더로 복사하지 않음(대용량). 런타임에 `HomeView.xaml.cs:ResolveTilesDir()`가 exe옆 `Maps\tiles` 또는 개발 시 `..\..\Maps\tiles`(= 프로젝트 소스)를 직접 참조. 배율 레벨 8/10/12/15 준비됨(z15 폴더는 비어있음).
- **중심 follow**: `HomeViewModel.MapCenterLatitude/Longitude` → XAML `MapControl.CenterPoint`(LatLngToGeoConverter) 바인딩. 첫 fix 전 기본 중심은 `DefaultCenterLat/Lng`(익산).
- **마커/시/도/군**: 마커 라벨 = `GpsRegion`. `KoreaRegionLookup`(오프라인 point-in-polygon, `Maps/skorea_muni.json`, System.Text.Json). `skorea_muni.json`은 csproj `Content`로 출력폴더 복사됨.
- **역지오코딩 로딩 경합 주의**: 17MB GeoJSON은 백그라운드 로드. `EnsureLoaded()`는 lock 내부에서 로드(로딩 중 조회는 대기). 앵커는 조회 성공 시에만 확정(정지 상태 재시도 보장).
- **마우스 좌표 패널**: `MapControl.CoordinatesPanelOptions Visible="False"`로 숨김(사용 안 함).
- **GPS 수신**: `Models/Client/GPS/GpsService.cs`(Serial NMEA, App.config `GpsPort`/`GpsBaud`). 파서 `NmeaParser`. 재연결/타이머는 `HomeViewModel`이 소유하고 `Dispose()`에서 해제.
- **한글 인코딩**: 이 폴더 .cs는 한글 주석/문자열 포함 → **UTF-8 BOM** 유지(과거 GpsService 주석 mojibake 이력).
