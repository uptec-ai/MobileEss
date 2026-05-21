# Fix completed task pre-push gates

- Task ID: 20260521-180735-fix-completed-task-pre-push-gates
- Status: active
- Created: 2026-05-21 18:07:35

## Goal

Fix pre-push quality gates so scripts invoked with `-AllowCompletedTask` can log build/test process steps against the latest completed harness task after `current-task.json` has been removed.

## Scope

Included:
- Keep active-task behavior unchanged.
- Allow `Invoke-HarnessProcess` callers to pass the already-resolved harness state.
- Update gate scripts that call `Invoke-HarnessProcess` to pass their resolved state.
- Fix the compile error exposed by the gate by using DevExpress property notification in `BMSViewModel`.

Excluded:
- No application code changes.
- No changes to Git remote or branch history.

## Impacted Files

- `scripts/harness/Harness.Common.ps1`
- `scripts/harness/run-build.ps1`
- `scripts/harness/run-unit-tests.ps1`
- `scripts/harness/run-integration-tests.ps1`
- `EMS_PJT_Hamburger/ViewModels/BMSViewModel.cs`

## Test Strategy

- Unit: Run harness unit gate.
- Integration: Run harness integration gate.
- Static analysis: Run harness static analysis.
- Build: Run harness build gate.
- E2E: Run harness E2E gate.
- Manual: Run `git push --dry-run --porcelain origin feature/pcs` after task completion to verify pre-push can use completed task context.

## Rollback

Revert the harness script changes to restore the previous active-task-only process logging behavior, and restore the previous BMS alarm notification call if the removed base helper is brought back.

## Notes

- Harness guard must pass before app code edits.
