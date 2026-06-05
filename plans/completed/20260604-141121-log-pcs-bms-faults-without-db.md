# Log PCS BMS Faults Without DB

- Task ID: 20260604-141121-log-pcs-bms-faults-without-db
- Status: active
- Created: 2026-06-04 14:11:21

## Goal

Record PCS and BMS fault events to NLog files as well as the existing alarm database path, so fault evidence remains when DB writes fail.

## Scope

Included:
- Add file log output for newly detected PCS faults.
- Add file log output for newly detected BMS faults.
- Keep existing DB alarm insert behavior and restore BMS fault persistence where the existing call is currently disabled.

Excluded:
- Changing DB schema or connection settings.
- Refactoring fault parsing or alarm window UI.

## Impacted Files

- `EMS_PJT_Hamburger/Models/Client/PCS/PcsModel.cs`
- `EMS_PJT_Hamburger/Models/BmsDataModel.cs`

## Test Strategy

- Unit: Run harness unit gate; no test projects are expected.
- Integration: Run harness integration gate; no test projects are expected.
- Static analysis: Run harness static analysis.
- Build: Run Release MSBuild through the harness.
- E2E: Run harness E2E artifact check if available.

## Rollback

Revert the changes to PCS and BMS fault logging, then rebuild Release.

## Notes

- Harness guard must pass before app code edits.
