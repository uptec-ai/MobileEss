# Harness Workflow

## Task Start

```powershell
.\scripts\harness\start-task.ps1 -Title "Mobile Ess"
```

Generated artifacts:

- `current-task.json`
- `plans/active/<task-id>.md`
- `logs/harness/<task-id>.log`

## Plan Creation

Fill the generated plan document with the following items:

- Goal
- Scope
- Impacted files
- Test strategy
- Rollback strategy

## Automatic Harness Task Enforcement

Even if the user gives only a simple feature request,
the Agent must treat it as a Harness task.

Examples:
- "Add a button"
- "Add a search filter"
- "Modify a chart"
- "Fix a bug"

All of the above requests must enforce the following workflow:

1. Review `AGENTS.md`
2. Review related documents under `docs/harness`
3. Execute `.\scripts\harness\start-task.ps1`
4. Create the task plan document
5. Pass `.\scripts\harness\guard-before-edit.ps1`
6. Implement the feature
7. Record logs
8. Execute tests
9. Execute build validation
10. Recommend a commit message

Application code modification is prohibited without:
- an active plan document
- an active log file

## Guard Before Code Modification

```powershell
.\scripts\harness\guard-before-edit.ps1
```

The guard must fail if any of the following are missing:

- `current-task.json`
- active plan file
- active log file

## Implementation Logging

```powershell
.\scripts\harness\write-log.ps1 -Message "Implementation details and reasoning"
```

## Quality Gates

```powershell
.\scripts\harness\run-quality-gates.ps1
```

Execution order:

1. Unit Tests
2. Integration Tests
3. Lint / Static Analysis
4. Build
5. E2E Validation

## Completion

```powershell
.\scripts\harness\suggest-commit-message.ps1
.\scripts\harness\complete-task.ps1
```

When completed:
- the plan document is moved to `plans/completed`
- `current-task.json` is removed