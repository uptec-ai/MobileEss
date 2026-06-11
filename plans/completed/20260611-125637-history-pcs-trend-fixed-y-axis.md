# history-pcs-trend-fixed-y-axis

- Task ID: 20260611-125637-history-pcs-trend-fixed-y-axis
- Status: active
- Created: 2026-06-11 12:56:37

## Goal

Keep the History View PCS Trend Y axis fixed at 0 to 100 even after chart double-click zoom/fit actions.

## Scope

Included:
- Update the PCS Trend SciChart Y axis range behavior.

Excluded:
- No BMS Trend changes unless required.
- No data query, DB, or refactoring changes.

## Impacted Files

- `EMS_PJT_Hamburger/Views/HistoryView.xaml`

## Test Strategy

- Unit: harness quality gates; no unit projects expected.
- Integration: harness quality gates; no integration projects expected.
- Static analysis: harness static analysis.
- Build: harness MSBuild validation if local files are not locked.
- E2E: not applicable for this scoped WPF XAML change.

## Rollback

Revert the PCS Trend Y axis range changes in `HistoryView.xaml`.

## Notes

- Harness guard must pass before app code edits.
