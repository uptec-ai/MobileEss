# pcs-trend-interval-hover

- Task ID: 20260608-105201-pcs-trend-interval-hover
- Status: active
- Created: 2026-06-08 10:52:01

## Goal

Update PCS Control panel action-button hover styling to match the current theme, and make Power Trend refresh in real time with mutually exclusive 1-minute/1-hour/1-day interval selection buttons.

## Scope

Included:
- Add theme-matched hover/pressed colors for PCS Control action buttons.
- Add Power Trend interval selection UI beside the chart title.
- Add ViewModel state/commands for one selected trend interval at a time.
- Fix trend series synchronization so appended data is visible in the chart in real time.

Excluded:
- Broad refactoring unrelated to PCS View.
- Changes outside the workspace.

## Impacted Files

- `EMS_PJT_Hamburger/Views/PCSView.xaml`
- `EMS_PJT_Hamburger/ViewModels/PcsViewModel.cs`
- `EMS_PJT_Hamburger/Models/Client/PCS/PcsModel.cs`
- Harness plan/log files.

## Test Strategy

- Unit: Run harness unit test script; current solution may warn/pass if no test projects exist.
- Integration: Run harness integration script.
- Static analysis: Run harness static analysis.
- Build: Run harness MSBuild quality gate.
- E2E: Run harness artifact verification.

## Rollback

Revert PCS trend interval and hover-style changes from this task only.

## Notes

- Harness guard must pass before app code edits.
