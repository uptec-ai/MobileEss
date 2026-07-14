---
paths:
  - "EMS_PJT_Hamburger/Models/Client/PCS/**"
  - "EMS_PJT_Hamburger/ViewModels/PcsViewModel.cs"
  - "EMS_PJT_Hamburger/Views/PCSView.xaml*"
---
# PCS / Modbus 통신 규칙

매칭 파일 편집 시에만 로드.

- **프로토콜**: Modbus TCP (NModbus4). 접속 정보는 App.config `PcsHost`/`PcsPort`/`PcsTimeoutMs`. 하드코딩 금지.
- **레지스터 맵**: `Models/Client/PCS/ModbusFieldSpec.cs` / `ModbusParse.cs` / `PcsSpecs.cs` 로 태그↔스케일 정의. 새 태그는 spec에 추가, 매직넘버 흩뿌리지 말 것.
- **서비스/VM**: `ModbusService.cs`(폴링), `PcsModel.cs`, 뷰모델 `PcsViewModel`(앱 전역 인스턴스 `App.PcsVm`). 충전/방전/정지 커맨드 + 릴레이.
- **스레딩**: 폴링은 백그라운드. UI 반영은 Dispatcher 마샬링. 폴링 CTS는 앱 종료 시 해제.
- **제어 안전**: 제어 커맨드는 `ControlConfirmationService`/BMS Guard 등 기존 확인 흐름을 우회하지 말 것.
