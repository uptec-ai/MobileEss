# Add control button confirmation dialogs

- Task ID: 20260527-113416-add-control-button-confirmation-dialogs
- Status: active
- Created: 2026-05-27 11:34:16

## Goal

Add a one-more-confirmation safety step to HOME, PCS, and BMS control buttons so potentially dangerous control actions are not executed by an accidental click.

## Scope

Included:
- Identify HOME, PCS, and BMS control button command handlers.
- Add a consistent WPF confirmation dialog before each control action continues.
- Keep the existing MVVM/DevExpress command structure and avoid unrelated refactoring.

Excluded:
- Protocol, database, or alarm logic changes.
- Visual redesign outside the confirmation behavior.

## Impacted Files

- `EMS_PJT_Hamburger/ViewModels/PcsViewModel.cs`
- `EMS_PJT_Hamburger/ViewModels/BmsViewModel.cs`
- `EMS_PJT_Hamburger/ViewModels/HomeViewModel.cs` if HOME control commands are defined there
- Related model files only if command handlers live outside the view models

## Test Strategy

- Unit: Run harness unit gate; current project may warn/pass if no test projects exist.
- Integration: Run harness integration gate.
- Static analysis: Run harness static analysis gate.
- Build: Run harness build gate with MSBuild for the legacy WPF solution.
- E2E: Run harness E2E gate; manually reason that canceling confirmation prevents command execution and accepting continues.

## Rollback

Revert the confirmation helper and the command handler changes in the impacted view model/model files. No schema or persistent data changes are planned.

## Notes

- Harness guard must pass before app code edits.
