# PCS view connection status border ellipse color by Conn_State Status like BMS

- Task ID: 20260616-140751-pcs-view-connection-status-border-ellipse-color-by-conn-state-status-like-bms
- Status: active
- Created: 2026-06-16 14:07:51

## Goal

PCS View 헤더의 연결 상태 패널을 BMS View처럼 연결/끊김에 따라 색이 바뀌게 한다.
- Conn_State.Status == "Connected" -> 녹색(#15391F/#2C7F41, ellipse #76F7A8)
- 그 외(Disconnected 등) -> 적색(#4A1515/#8C3A3A, ellipse #FF6B6B)

## Scope

- PCSView.xaml 헤더 연결 Border/Ellipse를 DataTrigger(Conn_State.Status)로 색 전환.
  텍스트 Foreground White로(양 상태 가독).
- 제외: ViewModel/연결 로직 변경 없음. Conn_State.Status는 이미 알림 발생 속성.

## Impacted Files

- EMS_PJT_Hamburger/Views/PCSView.xaml

## Test Strategy

- Build: 별도 OutputPath 컴파일 검증.
- E2E(수동): 연결 시 녹색, 끊김 시 적색으로 Border/Ellipse 전환.

## Rollback

- PCSView.xaml 원복.

## Notes

- IsConnected(computed)는 알림이 없어 트리거는 Conn_State.Status 문자열로 건다(BMS는 IsBmsConnected bool).
