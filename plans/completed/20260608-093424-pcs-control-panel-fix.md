# pcs-control-panel-fix

- Task ID: 20260608-093424-pcs-control-panel-fix
- Status: active
- Created: 2026-06-08 09:34:24

## Goal

Fix PCSView XAML invalid character/encoding issues and replace the Control section's simple list with a touch-friendly control panel containing TextEdit/ComboBox inputs and action buttons.

## Scope

Included:
- Diagnose the invalid XML character near the reported PCSView line.
- Remove or safely encode problematic text characters.
- Update the PCS Control section to render the previous-style control input panel when Control is selected.
- Preserve current PCS detail layout, SciChart trend, and existing command bindings.

Excluded:
- Broad refactoring outside PCS View/ViewModel.
- Changes outside the workspace.

## Impacted Files

- `EMS_PJT_Hamburger/Views/PCSView.xaml`
- Harness plan/log files.

## Test Strategy

- Unit: Harness unit test script; solution may warn/pass without test projects.
- Integration: Harness integration script.
- Static analysis: Harness static analysis.
- Build: MSBuild quality gate if environment allows; otherwise report limitation.
- E2E: Harness artifact verification if build can run.

## Rollback

Revert this task's PCSView XAML changes only, preserving prior DB/History/PCS edits.

## Notes

- Harness guard must pass before app code edits.
