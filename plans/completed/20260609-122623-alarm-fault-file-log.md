# alarm-fault-file-log

- Task ID: 20260609-122623-alarm-fault-file-log
- Status: active
- Created: 2026-06-09 12:26:23

## Goal

Add DB-independent file logging for PCS/BMS fault and alarm occurrence, and confirm what the History PCS/BMS trend charts represent.

## Scope

Included:
- Add a small alarm/fault file logger under Models/Managers.
- Write PCS fault occurrence to the file log before DB insert.
- Write BMS fault occurrence to the file log before DB insert.
- Keep existing DB alarm behavior unchanged.

Excluded:
- Changing alarm DB schema.
- Loading or changing History trend chart semantics.
- Refactoring unrelated logging or alarm code.

## Impacted Files

- `EMS_PJT_Hamburger/Models/Managers/AlarmFileLogger.cs`
- `EMS_PJT_Hamburger/Models/Client/PCS/PcsModel.cs`
- `EMS_PJT_Hamburger/ViewModels/BMSViewModel.cs`
- `EMS_PJT_Hamburger/EMS_PJT_Hamburger.csproj`
- Harness plan/log files.

## Test Strategy

- Static: ensure the new helper is included in the csproj.
- Build: Run harness quality gates/MSBuild.
- E2E: Harness release artifact verification.

## Rollback

Remove the new AlarmFileLogger file, remove the two call sites, and remove the csproj Compile entry.

## Notes

- Harness guard must pass before app code edits.
