# Allow completed plan commit message suggestions

- Task ID: 20260521-113226-allow-completed-plan-commit-message-suggestions
- Status: active
- Created: 2026-05-21 11:32:26

## Goal

Allow `scripts/harness/suggest-commit-message.ps1` to recommend a commit message even after `complete-task.ps1` has moved the active plan to `plans/completed` and removed `current-task.json`.

## Scope

Included:
- Update only the commit message suggestion harness script.
- Reuse existing completed-task fallback helpers from `Harness.Common.ps1`.
- Preserve active task behavior when `current-task.json` exists.

Excluded:
- No application code changes.
- No workflow or hook behavior changes.

## Impacted Files

- `scripts/harness/suggest-commit-message.ps1`

## Test Strategy

- Unit: Run harness unit gate; current solution has no test projects, so warning/pass is expected.
- Integration: Run harness integration gate; current solution has no integration test projects, so warning/pass is expected.
- Static analysis: Run harness static analysis.
- Build: Run Release MSBuild through harness.
- E2E: Run harness WPF artifact verification.
- Manual: Execute `suggest-commit-message.ps1` with active task context and verify output.

## Rollback

Revert `scripts/harness/suggest-commit-message.ps1` to active-task-only behavior.

## Notes

- Harness guard must pass before app code edits.
