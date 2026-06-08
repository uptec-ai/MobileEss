# pcs-view-touch-detail

- Task ID: 20260605-165147-pcs-view-touch-detail
- Status: active
- Created: 2026-06-05 16:51:47

## Goal

Update PCS View for touch-PC readability using the provided PCS detail visual direction, section buttons, SciChart power trend, and a Back button returning to Home.

## Scope

Included:
- Redesign PCS View layout and styles to match the supplied dark detail dashboard.
- Add mutually exclusive Grid/Inverter/Battery/Load/Control buttons that switch the inner detail content.
- Bind existing PCS data collections/control panel data to the selected section.
- Add a SciChart-based same-day power trend.
- Add a Back button that returns to Home.

Excluded:
- Export CSV and Full Screen buttons.
- Broad refactoring outside PCS View/ViewModel/navigation support.
- Changes outside the workspace.

## Impacted Files

- `EMS_PJT_Hamburger/Views/PCSView.xaml`
- `EMS_PJT_Hamburger/Views/PCSView.xaml.cs`
- `EMS_PJT_Hamburger/ViewModels/PcsViewModel.cs`
- Possibly `EMS_PJT_Hamburger/MainWindow.xaml.cs` if navigation support is needed.
- Harness plan/log files.

## Test Strategy

- Unit: Run harness unit test script; current solution may warn/pass if no test projects exist.
- Integration: Run harness integration script.
- Static analysis: Run harness static analysis.
- Build: Run harness MSBuild quality gate for the WPF solution.
- E2E: Run harness E2E artifact verification.

## Rollback

Revert PCS View/ViewModel/navigation changes from this task only, preserving unrelated user and prior DB/History changes.

## Notes

- Harness guard must pass before app code edits.
