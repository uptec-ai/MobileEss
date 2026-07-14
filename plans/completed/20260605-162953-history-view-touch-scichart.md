# history-view-touch-scichart

- Task ID: 20260605-162953-history-view-touch-scichart
- Status: active
- Created: 2026-06-05 16:29:53

## Goal

Update the History view for touch-PC readability using the provided visual direction, theme-matched button hover styling, and SciChart-based PCS/BMS trend charts.

## Scope

Included:
- Adjust History view layout, typography, cards, alarm list, and touch-friendly spacing.
- Replace the current Canvas/Polyline trend visuals with SciChart surfaces.
- Add or adjust ViewModel chart data series needed for SciChart binding.
- Keep existing DB-backed History data flow from the prior task.

Excluded:
- Broad refactoring outside History UI/ViewModel.
- Changes outside the workspace.
- DB schema or storage behavior changes unless needed for History binding.

## Impacted Files

- `EMS_PJT_Hamburger/Views/HistoryView.xaml`
- `EMS_PJT_Hamburger/ViewModels/HistoryViewModel.cs`
- Harness plan/log files.

## Test Strategy

- Unit: Run harness unit test script; current solution may warn/pass if no test projects exist.
- Integration: Run harness integration script; no DB mutations expected.
- Static analysis: Run harness static analysis.
- Build: Run harness MSBuild quality gate for the WPF solution.
- E2E: Run harness E2E artifact verification.

## Rollback

Revert the History XAML/ViewModel changes from this task only, preserving unrelated user and DB worktree changes.

## Notes

- Harness guard must pass before app code edits.
