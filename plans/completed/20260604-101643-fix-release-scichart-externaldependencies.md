# Fix Release SciChart ExternalDependencies

- Task ID: 20260604-101643-fix-release-scichart-externaldependencies
- Status: active
- Created: 2026-06-04 10:16:44

## Goal

Fix the Release runtime XAML load failure caused by the missing `SciChart.Examples.ExternalDependencies` assembly.

## Scope

Included:
- Remove the application startup dependency on SciChart example resource dictionaries.
- Keep the existing global CheckBox style reference valid with a local WPF style.
- Validate with the harness gates, especially Release MSBuild.

Excluded:
- SciChart version upgrade or broader chart styling refactor.
- Unrelated existing project changes.

## Impacted Files

- `EMS_PJT_Hamburger/App.xaml`
- `EMS_PJT_Hamburger/StaticResources.xaml`

## Test Strategy

- Unit: Run harness unit gate; no test projects are expected.
- Integration: Run harness integration gate; no test projects are expected.
- Static analysis: Run harness static analysis.
- Build: Run Release MSBuild through the harness.
- E2E: Run harness E2E gate if available.

## Rollback

Revert the App.xaml and StaticResources.xaml changes for this task, then rebuild Release.

## Notes

- Harness guard must pass before app code edits.
