# history-hourly-count-trend

- Task ID: 20260609-181256-history-hourly-count-trend
- Status: active
- Created: 2026-06-09 18:12:56

## Goal

Keep the existing History period rules and change PCS/BMS trend charts from hourly latest payload size to hourly saved data counts.

## Scope

Included:
- Keep Today/Week/period max 30 days and 720-row cap.
- Query hourly count of saved raw rows for PCS/BMS.
- Use the hourly count as chart value and row display value.
- Update chart subtitle/axis label from payload size to saved count.

Excluded:
- Database schema changes.
- Changing alarm/system-state queries.
- Refactoring unrelated History UI.

## Impacted Files

- `EMS_PJT_Hamburger/ViewModels/HistoryViewModel.cs`
- `EMS_PJT_Hamburger/Views/HistoryView.xaml`
- Harness plan/log files.

## Test Strategy

- XML: Parse HistoryView.xaml.
- Build: Run harness quality gates/MSBuild.
- E2E: Harness release artifact verification.

## Rollback

Revert this task's History hourly count query and label changes.

## Notes

- Harness guard must pass before app code edits.
