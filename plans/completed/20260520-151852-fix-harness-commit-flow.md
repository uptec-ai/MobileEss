# Fix harness commit flow

- Task ID: 20260520-151852-fix-harness-commit-flow
- Status: active
- Created: 2026-05-20 15:18:52

## Goal

Fix the Harness commit flow so `pre-commit` can run quality gates after task completion.

## Scope

Included:

- Allow `run-quality-gates.ps1` to run from Git hooks when the latest completed plan/log exists.
- Keep `guard-before-edit.ps1` strict: app code edits still require an active task.
- Document the commit-time behavior.

Excluded:

- Application code changes
- Refactoring

## Impacted Files

- scripts/harness/Harness.Common.ps1
- scripts/harness/run-quality-gates.ps1
- scripts/harness/run-unit-tests.ps1
- scripts/harness/run-integration-tests.ps1
- scripts/harness/run-static-analysis.ps1
- scripts/harness/run-build.ps1
- scripts/harness/run-e2e.ps1
- docs/harness/workflow.md
- docs/harness/qc.md

## Test Strategy

- Unit: run through `run-quality-gates.ps1`.
- Integration: run through `run-quality-gates.ps1`.
- Static analysis: verify mandatory artifacts and AGENTS.md line count.
- Build: MSBuild Release|Any CPU.
- E2E: verify release executable.
- Hook simulation: run `pre-commit` with no `current-task.json`.

## Rollback

Revert the Harness-only script/documentation changes.

## Notes

- Harness guard must pass before app code edits.
