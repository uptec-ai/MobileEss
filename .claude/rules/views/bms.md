---
paths:
  - "EMS_PJT_Hamburger/Views/BMSView.xaml"
  - "EMS_PJT_Hamburger/Views/BMSView.xaml.cs"
  - "EMS_PJT_Hamburger/Views/AlarmDetailWindow.xaml*"
  - "EMS_PJT_Hamburger/ViewModels/BMSViewModel.cs"
  - "EMS_PJT_Hamburger/ViewModels/AlarmDetailWindowViewModel.cs"
---
# View: BMS

## 목적
BMS(배터리관리시스템) 상태 감시. 전압/전류/SoC, Ready/Relay 상태, 셀·온도, 알람(결함) 표시.

## 상태
구현 완료(세부는 코드 확인).

## 담당 ViewModel / 소유
`BMSViewModel`(= `App.BmsVm`). CAN 수신은 `Models/Client/BMS/PcanRxService`.

## 데이터 & 외부 I/O
- CAN(PCAN). 프레임 정의: `Models/Client/BMS/{CanFieldSpec,BmsSpecs}.cs`. CAN DB: Document `CAN_DB_XPack_ESV_*.xlsx`.
- 주요 바인딩: `StatusMsg01`(Ready/RelayStatus/TotalVoltage/TotalCurrent), `OccurredFault`, 릴레이 커맨드 `Cmd_RelayBtn`.
- 알람: `AlarmService` + `AlarmFileLogger` → `AlarmDetailWindow`.

## UI 표면
홈 화면 Battery 카드도 같은 `App.BmsVm`에 바인딩(SoC/Volt/Curr).

## 주의사항 / 규칙
- 통신/스레딩 규칙은 `.claude/rules/bms-can.md` 참조.
- 릴레이/제어는 BMS Guard 설정과 연동(홈 화면 토글 `PcsVm.IsBmsGuardEnabled`).

## 관련 문서
`.claude/rules/bms-can.md`
