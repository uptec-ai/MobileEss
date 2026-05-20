# Restore harness baseline

- Task ID: 20260520-150754-restore-harness-baseline
- Status: active
- Created: 2026-05-20 15:07:54

## Goal

Restore the Harness baseline files after `git reset --hard origin/feature/pcs` and update `AGENTS.md` with mandatory workflow enforcement.

## Scope

Included:

- Restore `docs/harness/*.md`
- Restore `scripts/harness/*.ps1`
- Restore `.githooks/*`
- Restore `AGENTS.md` with the user-provided content
- Reinstall Git hook path
- Validate guard and quality gates

Excluded:

- Application code changes
- Refactoring
- Reverting existing branch changes

## Impacted Files

- AGENTS.md
- docs/harness/analysis.md
- docs/harness/rules.md
- docs/harness/workflow.md
- docs/harness/qc.md
- docs/harness/wpf-rules.md
- docs/harness/database.md
- scripts/harness/*.ps1
- .githooks/pre-commit
- .githooks/commit-msg
- .githooks/pre-push

## Test Strategy

- Unit: run `scripts/harness/run-unit-tests.ps1`; current repo has no test projects, so warning pass is expected.
- Integration: run `scripts/harness/run-integration-tests.ps1`; current repo has no integration test projects, so warning pass is expected.
- Static analysis: verify mandatory artifacts, `AGENTS.md` line count, active plan/log, and secret patterns.
- Build: run MSBuild for `Release|Any CPU`.
- E2E: verify release executable and runtime environment warnings.

## Rollback

Remove the restored Harness-only files and reset `core.hooksPath` if needed.

## Notes

- Harness guard must pass before app code edits.
- Existing modified application files from the current branch are not changed by this restore.
