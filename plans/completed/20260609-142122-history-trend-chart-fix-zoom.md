# history-trend-chart-fix-zoom

- Task ID: 20260609-142122-history-trend-chart-fix-zoom
- Status: active
- Created: 2026-06-09 14:21:22

## Goal

Fix History PCS/BMS trend charts so existing Today/Week raw payload data is plotted, week view uses date-based X-axis labels, and drag-selection zoom is available.

## Scope

Included:
- Use actual collected_at DateTime values for trend X values instead of reparsing formatted display text.
- Build trend series from raw payload rows only, excluding system-state rows from payload charts.
- Switch X-axis text formatting between time and date by selected period.
- Add SciChart drag-selection zoom to PCS/BMS History trend charts.

Excluded:
- Changing database schema.
- Changing raw data collection/storage behavior.
- Refactoring unrelated History view code.

## Impacted Files

- `EMS_PJT_Hamburger/ViewModels/HistoryViewModel.cs`
- `EMS_PJT_Hamburger/Views/HistoryView.xaml`
- Harness plan/log files.

## Test Strategy

- XML: Parse HistoryView.xaml.
- Build: Run harness quality gates/MSBuild.
- E2E: Harness release artifact verification.

## Rollback

Revert this task's HistoryViewModel trend properties and HistoryView chart modifier/axis changes.

## Notes

- Harness guard must pass before app code edits.
