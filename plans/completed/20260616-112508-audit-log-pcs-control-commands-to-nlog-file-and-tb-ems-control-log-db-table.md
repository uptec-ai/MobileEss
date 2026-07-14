# Audit log PCS control commands to nlog file and tb_ems_control_log db table

- Task ID: 20260616-112508-audit-log-pcs-control-commands-to-nlog-file-and-tb-ems-control-log-db-table
- Status: active
- Created: 2026-06-16 11:25:08

## Goal

PCS 제어 명령(충전/방전/정지/Fault Reset/비상정지)을 파일 로그(nlog)와
DB(tb_ems_control_log)에 감사 기록한다. 단일 choke point ExecuteControlSequenceAsync에서
Start/Complete/Canceled/Failed 전이를 기록.

## Scope

- DbManager: tb_ems_control_log 테이블/인덱스 신설(EnsureEssHistoryTables) +
  InsertControlLog(source, command, result, message, occurredAt).
- PcsViewModel: ExecuteControlSequenceAsync에 로깅 추가
  - Start: nlog.Info
  - Complete/Canceled/Failed: nlog + DB(LogControlCommand)
- 제외: 제어 판정/시퀀스 로직 변경 없음. BMS 명령은 범위 밖(PCS 한정).

## Impacted Files

- EMS_PJT_Hamburger/Models/Managers/DbManager.cs
- EMS_PJT_Hamburger/ViewModels/PcsViewModel.cs

## Test Strategy

- Build: 별도 OutputPath 컴파일 검증.
- E2E(수동): 충전/방전/정지/취소/실패 시 nlog 파일 기록 + tb_ems_control_log 행 적재.

## Rollback

- 두 파일 원복. 신규 테이블은 남아도 무해.

## Notes

- DB insert는 종료상태(Complete/Canceled/Failed) 1행/명령. Start는 nlog만.
- 실패/예외 시에도 SystemMsg 동작 유지, 로깅 실패는 삼켜서 제어 흐름 방해 안 함.
