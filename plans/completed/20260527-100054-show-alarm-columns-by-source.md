# Show alarm columns by source

- Task ID: 20260527-100054-show-alarm-columns-by-source
- Status: active
- Created: 2026-05-27 10:00:54

## Goal

Use the shared alarm popup while hiding PCS-only columns for BMS and hiding BMS-only columns for PCS.

## Scope

Included:
- Add source-specific column visibility properties to `AlarmDetailWindowViewModel`.
- Bind `AlarmDetailWindow` columns to those properties.

Excluded:
- No data model or DB schema changes.
- No popup routing changes.

## Impacted Files

- `EMS_PJT_Hamburger/ViewModels/AlarmDetailWindowViewModel.cs`
- `EMS_PJT_Hamburger/Views/AlarmDetailWindow.xaml`

## Test Strategy

- Unit: Run harness unit gate; no test projects are currently expected.
- Integration: Run harness integration gate; no integration projects are currently expected.
- Static analysis: Run harness static analysis.
- Build: Run Release MSBuild.
- E2E: Run harness WPF artifact check.

## Rollback

Remove the column visibility bindings and show all unified alarm columns again.

## Notes

- Harness guard must pass before app code edits.
