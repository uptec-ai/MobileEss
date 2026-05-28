# Add PCS BMS Guard Policy

- Task ID: 20260528-095718-add-pcs-bms-guard-policy
- Status: active
- Created: 2026-05-28 09:57:18

## Goal

Add a single PCS control guard function that reflects BMS ready/fault/SOC state before PCS charge/discharge commands and can also enforce SOC-based stop policy during polling.

## Scope

Included:
- Gate PCS charge/discharge commands using BMS state.
- Centralize BMS state checks and SOC stop policy in one function so it can be commented out for PCS-only communication tests.
- Keep current PCS Modbus command order unchanged.

Excluded:
- No UI redesign.
- No broad refactoring.
- No changes to unrelated BMS/Home view edits already present in the worktree.

## Impacted Files

- `EMS_PJT_Hamburger/ViewModels/PcsViewModel.cs`

## Test Strategy

- Unit: run harness unit script; repository currently has no dedicated test project.
- Integration: run harness integration script.
- Static analysis: run harness static analysis script.
- Build: run harness build script.
- E2E: run harness E2E script; desktop app manual PCS/BMS hardware test remains external.

## Rollback

Revert the changes in `PcsViewModel.cs` and rerun the build gate. Existing user changes in unrelated XAML files should be preserved.

## Notes

- Harness guard must pass before app code edits.
