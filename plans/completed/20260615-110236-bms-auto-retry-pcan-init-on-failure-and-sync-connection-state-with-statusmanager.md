# BMS auto retry PCAN init on failure and sync connection state with StatusManager

- Task ID: 20260615-110236-bms-auto-retry-pcan-init-on-failure-and-sync-connection-state-with-statusmanager
- Status: active
- Created: 2026-06-15 11:02:36

## Goal

BMS PCAN 초기화(_rx.Start())가 실패해 미연결(_isPcanStarted=false)일 때
일정 간격으로 자동 재시도하고, 모든 연결/재시도 상태가 StatusManager에 연동되게 한다.

## Scope

- 포함:
  - BMSViewModel: BmsReconnectInterval(5s) + _lastStartAttemptUtc 추가.
  - StartBmsReceiver를 재호출 안전하게 정리(시도시각 기록, null/dispose 가드).
  - BmsStatusTimer_Tick: _isPcanStarted=false면 일정 간격으로 StartBmsReceiver 재시도.
  - 상태 연동: 모든 전이가 UpdateBmsConnectionStatus를 거쳐 StatusManager.CurrentBMS_Status
    (Fody 알림 -> Update_BMS_Status -> StatusManager.BMS)로 반영되도록 유지/보장.
- 제외:
  - 시작 성공 후 일시 끊김(ReadLoop 자가복구)은 기존 동작 유지(재초기화 안 함).
  - StatusManager 클래스 자체 변경 없음(이미 Fody로 알림 동작).

## Impacted Files

- EMS_PJT_Hamburger/ViewModels/BMSViewModel.cs

## Test Strategy

- Build: 별도 OutputPath 컴파일 검증.
- E2E(수동): PCAN 미연결로 시작 -> 5초마다 재시도 로그/Connecting 표시,
  장치 연결되면 Connected로 전환, StatusManager.BMS 문자열 동기화.

## Rollback

- BMSViewModel.cs를 변경 이전 상태로 되돌린다.

## Notes

- 재시도는 _isPcanStarted=false(초기화 실패)일 때만. 성공 후 _isPcanStarted=true 고정.
- UpdateBmsConnectionStatus가 StatusManager 연동의 단일 choke point.
