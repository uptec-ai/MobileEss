# Add MSBuildMajorVersion safeguard and integrate GMap.NET map into HomeView (option A)

- Task ID: 20260706-132352-add-msbuildmajorversion-safeguard-and-integrate-gmap-net-map-into-homeview-option-a
- Status: active

## Goal

1) 모던 NuGet 패키지(.targets가 SDK 전용 $(MSBuildMajorVersion)을 숫자 비교)를 old-style
   packages.config/.NET FW48 프로젝트에 추가할 때 나는 MSB4086을 방지.
2) (후속) 안 A: GMap.NET 라스터 지도를 HomeView에 넣어 GPS lat/lon 중심 고정(follow)+마커.

## Scope (이번 단계)

- csproj 상단에 MSBuildMajorVersion 안전 정의 추가:
  값이 비어있으면 $(MSBuildVersion)의 Major로 채움 → 모던 패키지 targets의 숫자비교 안전.
- 빌드 검증(현재 그린 유지).
- 제외(다음 단계): GMap.NET DLL 참조 + HomeView 지도/마커/follow, NetTopologySuite+GeoJSON 리버스지오코딩.

## Impacted Files

- EMS_PJT_Hamburger/EMS_PJT_Hamburger.csproj

## Test Strategy

- Build: 별도 OutputPath 컴파일(ExitCode=0 유지).

## Rollback

- csproj의 추가 PropertyGroup 제거.

## Notes

- MSBuildMajorVersion은 내장 예약 속성이 아니라 SDK-style 빌드에서만 채워짐 → old-style에선 빈값.
- 다음: GMap.NET.Core+WindowsPresentation 버전쌍 확정 후 DLL 직접 참조(패키지 targets 회피).
