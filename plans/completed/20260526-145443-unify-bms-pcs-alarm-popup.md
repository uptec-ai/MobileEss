# Unify BMS PCS alarm popup

- Task ID: 20260526-145443-unify-bms-pcs-alarm-popup
- Status: active
- Created: 2026-05-26 14:54:43

## Goal

Unify the PCS and BMS alarm popup workflow so both use the same filter controls, similar table shape, load/export actions, and a shared future DB table contract.

## Scope

Included:
- Extend the existing BMS alarm popup/view model so it can show BMS and PCS alarms.
- Route PCS fault popup command to the unified alarm popup.
- Move the BMS alarm popup button into the BMS control panel.
- Add a shared `db_ems_alarm` table creation/select/insert contract in `DbManager`.
- Implement mutual-exclusive filters and TXT export for the unified popup.

Excluded:
- No actual historical DB migration.
- No periodic PCS/BMS alarm persistence loop beyond adding the DB contract methods.
- No unrelated WPF layout refactor.

## Impacted Files

- `EMS_PJT_Hamburger/Models/Client/BMS/AlarmService.cs`
- `EMS_PJT_Hamburger/ViewModels/AlarmDetailWindowViewModel.cs`
- `EMS_PJT_Hamburger/ViewModels/PcsViewModel.cs`
- `EMS_PJT_Hamburger/Views/AlarmDetailWindow.xaml`
- `EMS_PJT_Hamburger/Views/BMSView.xaml`
- `EMS_PJT_Hamburger/Models/Managers/DbManager.cs`

## Test Strategy

- Unit: Run harness unit gate; no test projects are currently expected.
- Integration: Run harness integration gate; no integration projects are currently expected.
- Static analysis: Run harness static analysis.
- Build: Run Release MSBuild.
- E2E: Run harness WPF artifact check.

## Rollback

Revert the popup routing/layout changes, the unified alarm item/view model changes, and the added `DbManager` alarm table methods.

## Notes

- Harness guard must pass before app code edits.
