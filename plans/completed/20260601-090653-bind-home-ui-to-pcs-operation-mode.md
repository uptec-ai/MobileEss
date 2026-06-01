# Bind Home UI To PCS Operation Mode

- Task ID: 20260601-090653-bind-home-ui-to-pcs-operation-mode
- Status: active
- Created: 2026-06-01 09:06:53

## Goal

Bind HomeView UI mode to actual PCS charge/discharge operation state instead of the demo-only WaitChangeAsync cycle.

## Scope

Included:
- Expose current PCS charge/discharge mode from the PCS ViewModel.
- Update HomeViewModel loop to apply Charging UI when PCS charge is active, Discharging UI when PCS discharge is active, and Waiting UI otherwise.
- Keep existing load target behavior unchanged until a real ON/OFF/Vehicle selection source exists.

Excluded:
- No HomeView XAML redesign.
- No broad refactoring of PCS/BMS control logic.
- No load destination selection implementation.

## Impacted Files

- `EMS_PJT_Hamburger/ViewModels/PcsViewModel.cs`
- `EMS_PJT_Hamburger/ViewModels/HomeViewModel.cs`

## Test Strategy

- Unit: run harness unit script; current repository has no unit test projects.
- Integration: run harness integration script; current repository has no integration test projects.
- Static analysis: run harness static analysis.
- Build: run MSBuild Release|Any CPU via harness.
- E2E: run harness WPF release artifact check.

## Rollback

Revert the changes in `PcsViewModel.cs` and `HomeViewModel.cs`, then rerun the harness quality gates.

## Notes

- Harness guard must pass before app code edits.
