# pcs-trend-zoom-reset-window

- Task ID: 20260609-092047-pcs-trend-zoom-reset-window
- Status: active
- Created: 2026-06-09 09:20:47

## Goal

Allow PCS Power Trend zoom-out to reveal older retained X values while double-click restores the default view: latest 10 interval points and Y range 0-100.

## Scope

Included:
- Keep all retained trend buckets in the series instead of trimming the series to the visible window.
- Expose the selected default visible X window from the model.
- Handle chart double-click to restore X visible range and Y 0-100.
- Keep mouse wheel zoom enabled.

Excluded:
- Loading historical trend data from DB.
- Refactoring unrelated PCS view code.
- Changes outside the workspace.

## Impacted Files

- `EMS_PJT_Hamburger/Models/Client/PCS/PcsModel.cs`
- `EMS_PJT_Hamburger/Views/PCSView.xaml`
- `EMS_PJT_Hamburger/Views/PCSView.xaml.cs`
- Harness plan/log files.

## Test Strategy

- XML: Parse PCSView.xaml.
- Build: Run harness quality gates/MSBuild.
- E2E: Harness release artifact verification.

## Rollback

Revert this task's PcsModel visible range properties and PCSView chart double-click/reset changes.

## Notes

- Harness guard must pass before app code edits.
