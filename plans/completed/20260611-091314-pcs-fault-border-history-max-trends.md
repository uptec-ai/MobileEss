# pcs-fault-border-history-max-trends

- Task ID: 20260611-091314-pcs-fault-border-history-max-trends
- Status: active
- Created: 2026-06-11 09:13:14

## Goal

Update PCS View fault card styling on active alarms/faults and change History trend charts to show hourly maximum PCS active power and BMS SOC, with empty hours rendered as zero.

## Scope

Included:
- Add a PCS fault/alarm active view property and bind Fault card colors to it.
- Keep raw compressed payload storage columns because they are still needed to reconstruct historical values.
- Build History PCS trend from hourly max GridActivePower in stored PCS raw payloads.
- Build History BMS trend from hourly max BMS_SOC/BMS_Disp_SOC in stored BMS raw payloads.
- Fill missing hourly buckets in the selected period with zero values.
- Keep existing 30-day and 720-hour constraints.

Excluded:
- Dropping payload_length/compressed_length columns or destructive DB migrations.
- Changing raw data collection cadence.
- Broad refactoring unrelated to these views.

## Impacted Files

- `EMS_PJT_Hamburger/Models/Client/PCS/PcsModel.cs`
- `EMS_PJT_Hamburger/ViewModels/HistoryViewModel.cs`
- `EMS_PJT_Hamburger/Views/PCSView.xaml`
- `EMS_PJT_Hamburger/Views/HistoryView.xaml`
- Harness plan/log files.

## Test Strategy

- XML: Parse PCSView.xaml and HistoryView.xaml.
- Build: Run harness quality gates/MSBuild.
- E2E: Harness release artifact verification.

## Rollback

Revert this task's PCS alarm styling and History trend payload parsing changes.

## Notes

- Harness guard must pass before app code edits.
