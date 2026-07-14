# pcs-trend-point-marker

- Task ID: 20260608-155732-pcs-trend-point-marker
- Status: active
- Created: 2026-06-08 15:57:32

## Goal

Show PCS Power Trend vertex points as 1x1 ellipse markers.

## Scope

Included:
- Add an ellipse point marker to the PCS Power Trend SciChart series.

Excluded:
- Trend data logic changes.
- Refactoring unrelated PCS view code.

## Impacted Files

- `EMS_PJT_Hamburger/Views/PCSView.xaml`
- Harness plan/log files.

## Test Strategy

- XML: Parse PCSView.xaml.
- Build: Run harness quality gates/MSBuild.

## Rollback

Remove the point marker from the PCS Power Trend series.

## Notes

- Harness guard must pass before app code edits.
