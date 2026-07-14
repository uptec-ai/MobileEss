---
paths:
  - "EMS_PJT_Hamburger/Models/Client/BMS/**"
  - "EMS_PJT_Hamburger/ViewModels/BMSViewModel.cs"
  - "EMS_PJT_Hamburger/Views/BMSView.xaml*"
---
# BMS / CAN 통신 규칙

매칭 파일 편집 시에만 로드.

- **프로토콜**: CAN (Peak `PCANBasic.NET`). 수신 `PcanRxService.cs`. CAN DB 참고: `docs/`의 `CAN_DB_XPack_ESV_*.xlsx`(Document 폴더).
- **필드 정의**: `Models/Client/BMS/CanFieldSpec.cs` / `BmsSpecs.cs`. 프레임 파싱은 spec 기반, 매직넘버 금지.
- **알람**: `AlarmService.cs` + `Models/Managers/AlarmFileLogger.cs`. 알람 상세는 `AlarmDetailWindow`.
- **서비스/VM**: 뷰모델 `BMSViewModel`(전역 `App.BmsVm`). `StatusMsg01`(Ready/RelayStatus/TotalVoltage/TotalCurrent) 등.
- **스레딩**: CAN 수신은 백그라운드 → Dispatcher 마샬링. 수신/타이머 자원은 앱 종료 시 해제.
