# Home Load Target And PCS 32Bit Read Mode

- Task ID: 20260602-091216-home-load-target-and-pcs-32bit-read-mode
- Status: active
- Created: 2026-06-02 09:12:16

## Goal

HomeView에 ON/OFF/Vehicle 중 하나만 선택되는 로드 선택 토글 UI를 추가하고, PCSView에서 32bit 레지스터 Read 워드 순서를 기본/리틀엔디언으로 선택할 수 있게 한다. BMS Pack Ready 랜덤 생성 동작은 현재 구현을 확인해 실제 BMS 연결 시 영향과 위험을 답변한다.

## Scope

Included:
- HomeView 로드 선택 UI 및 ViewModel 선택 명령/상태 반영
- PCS 32bit Read 워드 순서 설정 UI 및 ReadControlU32/일반 Modbus parse 반영
- 현재 BMS Ready 상태 산출 로직 검토

Excluded:
- PCS 프로토콜 주소 추가 또는 출력 대상과 PCS 제어 연결
- BMS 통신 구조 리팩토링
- 대규모 UI 레이아웃 재설계

## Impacted Files

- EMS_PJT_Hamburger/Views/HomeView.xaml
- EMS_PJT_Hamburger/ViewModels/HomeViewModel.cs
- EMS_PJT_Hamburger/Models/HomeModel.cs
- EMS_PJT_Hamburger/Views/PCSView.xaml
- EMS_PJT_Hamburger/ViewModels/PcsViewModel.cs
- EMS_PJT_Hamburger/Models/Client/PCS/ModbusParse.cs
- EMS_PJT_Hamburger/Models/Client/PCS/PcsModel.cs

## Test Strategy

- Unit: harness unit gate; 현재 테스트 프로젝트 부재 시 하네스 경고 확인
- Integration: harness integration gate; 현재 테스트 프로젝트 부재 시 하네스 경고 확인
- Static analysis: harness static analysis gate
- Build: MSBuild Release Any CPU
- E2E: harness WPF artifact validation

## Rollback

이번 태스크에서 변경한 파일만 되돌리면 기존 HomeView 표시 및 PCS 32bit Read 방식으로 복귀한다. 기존 사용자 변경이 섞인 HomeView.xaml 라벨 변경은 되돌리지 않는다.

## Notes

- Harness guard must pass before app code edits.
