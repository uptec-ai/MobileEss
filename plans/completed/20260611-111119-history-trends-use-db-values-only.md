# history-trends-use-db-values-only

- Task ID: 20260611-111119-history-trends-use-db-values-only
- Status: active
- Created: 2026-06-11 11:11:19

## Goal

History View trend charts should show only DB-backed PCS active power and BMS SOC points. Remove the previous zero-fill behavior for hours with no data.

## Scope

Included:
- Update HistoryViewModel trend row creation to use actual hourly DB values only.
- Keep PCS trend based on `pcs_active_power_kw`.
- Keep BMS trend based on `bms_soc`.

Excluded:
- No DB schema drops or destructive migrations.
- No unrelated UI/style changes.
- No refactoring outside the requested logic.

## Impacted Files

- `EMS_PJT_Hamburger/ViewModels/HistoryViewModel.cs`

## Test Strategy

- Unit: run harness quality gates; no test projects are expected.
- Integration: run harness quality gates; no integration projects are expected.
- Static analysis: harness static analysis.
- Build: harness MSBuild validation if local permissions allow it.
- E2E: not applicable for this WPF change in the current harness.

## Rollback

Revert this task's changes in `HistoryViewModel.cs` to restore zero-filled hourly trend buckets.

## Notes

- Harness guard must pass before app code edits.
