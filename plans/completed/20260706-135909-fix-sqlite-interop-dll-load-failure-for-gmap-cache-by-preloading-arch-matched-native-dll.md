# Fix SQLite.Interop.dll load failure for GMap cache by preloading arch-matched native DLL

- Task ID: 20260706-135909-fix-sqlite-interop-dll-load-failure-for-gmap-cache-by-preloading-arch-matched-native-dll
- Status: active
- Created: 2026-07-06 13:59:09

## Goal

런타임 오류 "DLL 'SQLite.Interop.dll'을(를) 로드할 수 없습니다 (0x8007007E, ERROR_MOD_NOT_FOUND)" 해결.
GMap.NET 오프라인 캐시(System.Data.SQLite)가 네이티브 interop을 찾도록 한다.

## 원인(실측)

- interop은 bin\Debug\x86, x64 하위폴더에 정상 복사됨. DLL 자체 의존성도 표준 시스템 DLL뿐(ADVAPI32/KERNEL32/USER32/WINTRUST/mscoree) → 로드 가능.
- 그러나 System.Data.SQLite의 [DllImport("SQLite.Interop.dll")] 기본 탐색은 앱 루트(bin\Debug)+PATH만 보고 아키텍처 하위폴더를 못 찾음 → ERROR_MOD_NOT_FOUND.
- 실측: 전체 경로 LoadLibrary는 성공(핸들 유효), 이미 로드되면 bare-name도 매칭됨.

## Scope

- HomeView.xaml.cs: GMap 캐시 사용 전에 Environment.Is64BitProcess로 x86/x64 선택,
  AppDomain.BaseDirectory\{arch}\SQLite.Interop.dll을 kernel32 LoadLibrary(전체경로)로 선로드.
  이후 System.Data.SQLite의 bare-name DllImport가 이미 로드된 모듈을 사용.
- 제외: csproj/XAML/ViewModel 변경 없음. 리버스지오코딩·프리캐시(후속).

## Impacted Files

- EMS_PJT_Hamburger/Views/HomeView.xaml.cs

## Test Strategy

- Unit: N/A
- Integration: N/A
- Static analysis: N/A
- Build: 별도 OutputPath 컴파일(ExitCode=0).
- E2E: 앱 실행 시 지도 타일 로드/캐시 정상(하드웨어+실행 필요, 헤드리스 검증 불가).

## Rollback

- HomeView.xaml.cs의 EnsureSQLiteInteropLoaded 호출·메서드·P/Invoke 제거.

## Notes

- 전체 경로 선로드는 System.Data.SQLite 자체 pre-loader와 무관하게 결정적으로 동작.
- 배포 PC는 64-bit Windows 11(AnyCPU, Prefer32Bit 미설정 -> x64 실행)이지만 두 비트 모두 대응.
