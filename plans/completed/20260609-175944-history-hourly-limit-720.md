# history-hourly-limit-720

- Task ID: 20260609-175944-history-hourly-limit-720
- Status: active
- Created: 2026-06-09 17:59:44

## Goal

Change History PCS/BMS raw payload loading to hourly latest values, capped at 720 rows, and block period searches over 30 days with an error message.

## Scope

Included:
- Today: hourly latest payload values, up to 24 hours.
- Week: hourly latest payload values, up to 168 hours.
- Period search: hourly latest payload values, capped at 720 rows.
- Show an error/status message and skip loading when requested period exceeds 30 days.

Excluded:
- Database schema changes.
- Changing alarm/system-state queries unless needed for the period validation.
- Refactoring unrelated History UI.

## Impacted Files

- `EMS_PJT_Hamburger/ViewModels/HistoryViewModel.cs`
- Harness plan/log files.

## Test Strategy

- Build: Run harness quality gates/MSBuild.
- E2E: Harness release artifact verification.

## Rollback

Revert HistoryViewModel query and range-validation changes from this task.

## Notes

- Harness guard must pass before app code edits.
