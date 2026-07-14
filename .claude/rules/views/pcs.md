---
paths:
  - "EMS_PJT_Hamburger/Views/PCSView.xaml"
  - "EMS_PJT_Hamburger/Views/PCSView.xaml.cs"
  - "EMS_PJT_Hamburger/Views/PcsFaultMessageWindow.xaml*"
  - "EMS_PJT_Hamburger/ViewModels/PcsViewModel.cs"
---
# View: PCS

## 목적
PCS(전력변환장치) 상태 감시 및 제어. AC 전압/전류/주파수, 충전·방전·정지, 릴레이, 인버터 항목(InvItems) 및 결함(Fault) 표시.

## 상태
구현 완료(세부는 코드 확인).

## 담당 ViewModel / 소유
`PcsViewModel`(= `App.PcsVm`). Modbus 통신은 `Models/Client/PCS/ModbusService`.

## 데이터 & 외부 I/O
- Modbus TCP: App.config `PcsHost`/`PcsPort`/`PcsTimeoutMs`.
- 태그 정의: `Models/Client/PCS/{ModbusFieldSpec,ModbusParse,PcsSpecs}.cs`.
- 주요 바인딩: `InvAveAcVoltage`, `InvAveCurrent`, `InvItems[..].Value`, `OccurredFault`, `IsConnected`, 충전/방전/정지 커맨드.

## UI 표면
결함 팝업 `PcsFaultMessageWindow`. 홈 화면 PCS 카드도 같은 `App.PcsVm`에 바인딩.

## 주의사항 / 규칙
- 통신/스레딩/제어안전 규칙은 `.claude/rules/pcs-modbus.md` 참조.
- 제어 확인 흐름(`ControlConfirmationService`, BMS Guard) 우회 금지.

## 관련 문서
`.claude/rules/pcs-modbus.md`, `docs/PCSView_Button_Sequence.txt`
