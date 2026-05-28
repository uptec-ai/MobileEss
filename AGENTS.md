# AGENTS.md

## Project

- Root: `C:\Project\2. ESS\EMS_PJT v1.4\EMS_PJT_Hamburger`
- IDE: Visual Studio 2019
- Framework: .NET Framework 4.8
- Application: WPF
- UI Library: DevExpress v23.1.5
- Chart Library: SciChart 8.6
- Shell: PowerShell

## Required Harness

1. Start every task with `.\scripts\harness\start-task.ps1 -Title "..."`.
2. Fill the generated plan with goal, scope, impacted files, test strategy, and rollback.
3. Do not edit app code until `.\scripts\harness\guard-before-edit.ps1` passes.
4. Log implementation decisions with `.\scripts\harness\write-log.ps1 -Message "..."`.
5. Run quality gates in order: unit, integration, static analysis, build, E2E.
6. Suggest a commit message with `.\scripts\harness\suggest-commit-message.ps1`.
7. Complete the task with `.\scripts\harness\complete-task.ps1`.

## Mandatory Workflow Enforcement

All code change requests must follow the Harness Workflow automatically,
even if the user does not explicitly mention the workflow steps.

Simple user requests such as:
- "Add a button"
- "Modify a feature"
- "Fix a bug"

must still follow the full Harness Workflow.

Before modifying any application code, the agent must:

1. Read:
   - `docs/harness/workflow.md`

2. Execute:
   - `.\scripts\harness\start-task.ps1 -Title "..."`

3. Generate and complete:
   - an active plan document
   - an active log document

4. Execute:
   - `.\scripts\harness\guard-before-edit.ps1`

Application code modification is strictly prohibited without:
- an active plan document
- an active log file
- a successful guard validation

## Local Rules

- Stay inside the workspace. Do not modify, save, or delete files outside it.
- Do not refactor unless explicitly requested.
- Do not revert user changes.
- Preserve the existing WPF/MVVM structure: `Views`, `ViewModels`, `Models`, `Models/Managers`, `Models/Client`.
- Use MSBuild for this legacy .NET Framework solution.
- Keep DevExpress/SciChart license and DB secrets out of source files.

## Current Architecture Notes

- `App.xaml.cs` creates shared managers, views, and view models.
- DevExpress `ViewModelBase`, `BindableBase`, and `DelegateCommand` are the local MVVM baseline.
- SciChart license is expected from `EMS_SCICHART_LICENSE_KEY`.
- DB connection is expected from `EMS_DB_CONN` or `App.config`.
- There are currently no test projects; the harness warns and passes until tests are added.
