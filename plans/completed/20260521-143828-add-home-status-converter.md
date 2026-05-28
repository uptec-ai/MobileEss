# Add Home status converter

- Task ID: 20260521-143828-add-home-status-converter
- Status: active
- Created: 2026-05-21 14:38:28

## Goal

Add one reusable HomeView converter that returns FAULT when the matching fault count is non-zero, otherwise returns RUN for a run value of 1/true and STOP for 0/false.

## Scope

Included:
- Add a shared converter in the existing manager converter file.
- Apply it to the HomeView PCS and BMS status labels using MultiBinding.
- Use PCS `PcsVm.PanelData.AlarmCnt` and BMS `BmsVm.Alarms` as fault inputs.

Excluded:
- No refactoring of HomeViewModel polling.
- No changes to PCS/BMS communication logic.

## Impacted Files

- `EMS_PJT_Hamburger/Models/Managers/ConvertManager.cs`
- `EMS_PJT_Hamburger/Views/HomeView.xaml`

## Test Strategy

- Unit: Run harness unit gate; no test projects are expected in this solution.
- Integration: Run harness integration gate; no integration test projects are expected.
- Static analysis: Run harness static analysis.
- Build: Run Release MSBuild through harness.
- E2E: Run harness WPF artifact verification.

## Rollback

Revert the converter addition and restore the PCS/BMS HomeView labels to their previous `ConnectPCS` and `ConnectBMS` bindings.

## Notes

- Harness guard must pass before app code edits.
