# Create GMapControl at runtime and set OSM UserAgent to fix designer error interop timing and Access blocked

- Task ID: 20260706-140902-create-gmapcontrol-at-runtime-and-set-osm-useragent-to-fix-designer-error-interop-timing-and-access-blocked
- Status: active
- Created: 2026-07-06 14:09:02

## Goal

세 증상 동시 해결:
1) XAML 디자이너 "GMapControl 인스턴스를 만들 수 없습니다".
2) 런타임 "SQLite.Interop.dll 로드 실패(0x8007007E)" 여전.
3) 실행 시 지도에 "Access blocked / App is not following the tile usage policy of OpenStreetMap".

## 원인

- XAML에 GMapControl이 있으면 디자이너와 InitializeComponent(=Loaded 이전)에서 컨트롤이 생성됨.
  그 정적 초기화가 GMaps.Instance/SQLite 캐시를 건드려, Loaded에서 도는 interop 선로드보다 먼저 실패.
  디자이너 오류도 동일(디자이너 프로세스 base dir엔 interop 없음).
- OSM는 식별 User-Agent가 없거나 기본값이면 타일을 차단(정책 위반 응답).

## Scope

- HomeView.xaml: <gmap:GMapControl> 제거, 호스트를 빈 Border(x:Name="MapHost")로. 미사용 xmlns:gmap 제거.
- HomeView.xaml.cs: InitializeMap에서 (1)EnsureSQLiteInteropLoaded() 선행,
  (2)GMapProvider.UserAgent 설정, (3)GMaps.Instance.Mode 설정, (4)new GMapControl() 생성·구성 후
  MapHost.Child에 부착. MoveMap 등은 필드 _map 사용.
- 제외: csproj/ViewModel 변경 없음. 리버스지오코딩·프리캐시(후속).

## Impacted Files

- EMS_PJT_Hamburger/Views/HomeView.xaml
- EMS_PJT_Hamburger/Views/HomeView.xaml.cs

## Test Strategy

- Unit: N/A
- Integration: N/A
- Static analysis: N/A
- Build: 별도 OutputPath 컴파일(ExitCode=0) + 실제 bin\Debug 정식 빌드.
- E2E: 앱 실행 시 OSM 타일 표시·follow·마커 정상(하드웨어+실행 필요, 헤드리스 검증 불가).

## Rollback

- HomeView.xaml/.xaml.cs를 이전(런타임 생성 이전) 상태로 원복.

## Notes

- 런타임 생성이라 디자이너는 GMap 타입을 인스턴스화하지 않음 → 디자이너 오류 제거 + interop 타이밍 보장.
- GMap 첫 접근(GMapProvider/GMaps.Instance/new GMapControl) 전에 반드시 EnsureSQLiteInteropLoaded 호출.
- UA는 저용량 단말 식별용: "ESS-HMI/1.0 (uptec-netzeroai.com)".
