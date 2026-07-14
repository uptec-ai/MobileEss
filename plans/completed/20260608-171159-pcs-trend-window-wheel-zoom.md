# pcs-trend-window-wheel-zoom

- Task ID: 20260608-171159-pcs-trend-window-wheel-zoom
- Status: active
- Created: 2026-06-08 17:11:59

## Goal

Show PCS Power Trend as the most recent 10 intervals for 1m/1h/1d selections and enable mouse wheel zoom in/out on the chart.

## Scope

Included:
- Keep runtime trend samples for up to 10 days.
- Display only the selected recent window: 10 minutes, 10 hours, or 10 days.
- Add SciChart mouse wheel zoom modifier.

Excluded:
- Loading historical trend data from DB.
- Refactoring unrelated PCS code.
- Changes outside the workspace.

## Impacted Files

- `EMS_PJT_Hamburger/Models/Client/PCS/PcsModel.cs`
- `EMS_PJT_Hamburger/Views/PCSView.xaml`
- Harness plan/log files.

## Test Strategy

- XML: Parse PCSView.xaml.
- Build: Run harness quality gates/MSBuild.

## Rollback

Revert this task's display-window and mouse-wheel modifier changes.

## Notes

- Harness guard must pass before app code edits.
