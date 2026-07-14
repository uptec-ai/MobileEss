# pcs-trend-realtime-tooltip

- Task ID: 20260608-151231-pcs-trend-realtime-tooltip
- Status: active
- Created: 2026-06-08 15:12:31

## Goal

Make PCS Power Trend update visibly in real time without double-clicking the chart, and show nearest interval-point values when the mouse hovers over the chart.

## Scope

Included:
- Enable automatic axis range updates for the PCS Power Trend SciChart surface.
- Add a SciChart hover/rollover modifier so 1m/1h/1d interval vertex values are displayed.
- Keep the existing interval toggle and data aggregation structure.

Excluded:
- Refactoring unrelated PCS view/model code.
- Changes outside the workspace.

## Impacted Files

- `EMS_PJT_Hamburger/Views/PCSView.xaml`
- Harness plan/log files.

## Test Strategy

- XML: Parse PCSView.xaml.
- Build: Run harness quality gates/MSBuild.
- E2E: Verify WPF release artifact through harness.

## Rollback

Revert only the PCSView.xaml SciChart modifier and axis range changes from this task.

## Notes

- Harness guard must pass before app code edits.
