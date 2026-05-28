# Fix PCS occurred alarm active list

- Task ID: 20260526-162901-fix-pcs-occurred-alarm-active-list
- Status: active
- Created: 2026-05-26 16:29:01

## Goal

Make PCS popup `Occurred Alarm` show only currently active PCS faults, so faults whose bit returned to 0 disappear from the occurred list while remaining in history.

## Scope

Included:
- Add a current-active PCS fault collection.
- Remove faults from the current collection when the corresponding PCS fault bit is 0.
- Pass the current collection to the PCS alarm popup for `Occurred Alarm`.

Excluded:
- No reset command behavior changes.
- No DB historical query changes.
- No unrelated alarm popup layout changes.

## Impacted Files

- `EMS_PJT_Hamburger/Models/Client/PCS/PcsModel.cs`
- `EMS_PJT_Hamburger/ViewModels/PcsViewModel.cs`

## Test Strategy

- Unit: Run harness unit gate; no test projects are currently expected.
- Integration: Run harness integration gate; no integration projects are currently expected.
- Static analysis: Run harness static analysis.
- Build: Run Release MSBuild.
- E2E: Run harness WPF artifact check.

## Rollback

Revert the current-active collection and restore the PCS alarm popup to use `PcsFaultMessages`.

## Notes

- Harness guard must pass before app code edits.
