# Harness Rules

## Core Principles

- All tasks must start with `scripts/harness/start-task.ps1`.
- `scripts/harness/guard-before-edit.ps1` must pass before modifying application code.
- All implementation decisions and code changes must be continuously recorded using `scripts/harness/write-log.ps1`.
- The required quality gate order before completion is:
  1. Unit Tests
  2. Integration Tests
  3. Static Analysis
  4. Build
  5. E2E Validation
- Refactoring is allowed only when explicitly requested.
- Files outside the workspace must not be modified, saved, or deleted.

## Project-Specific Rules

- The target framework must be treated as `.NET Framework 4.8`.
- Build operations must use Visual Studio/MSBuild-based workflows.
- NuGet package management must prioritize `packages.config` and the `packages/` folder structure.
- DevExpress must be validated against version `v23.1.5`.
- SciChart must be validated against version `8.6.0.28199`.
- Preserve the existing folder structure for:
  - `Views`
  - `ViewModels`
  - `Models`
  - `Managers`
- Do not change the current pattern where WPF View `DataContext` instances are directly injected in `App.xaml.cs`.
- Database changes must go through `DbManager`.
- SQL statements must prioritize parameter binding over inline string concatenation.
- Device communication addresses, database connection strings, and SciChart license keys must never be hardcoded in source files.

## Prohibited / Warning

- Application code modification without the Harness workflow is prohibited.
- Commands that revert user changes are prohibited, including:
  - `git reset --hard`
  - `git checkout --`
- DevExpress/SciChart version upgrades, migration to SDK-style .NET projects, and DI introduction must be handled as separate refactoring tasks.
- When modifying equipment/device control logic, the task plan document must include:
  - rollback procedures
  - manual validation procedures