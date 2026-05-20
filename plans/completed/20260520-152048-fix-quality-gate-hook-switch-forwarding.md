# Fix quality gate hook switch forwarding

- Task ID: 20260520-152048-fix-quality-gate-hook-switch-forwarding
- Status: active
- Created: 2026-05-20 15:20:48

## Goal

Fix `run-quality-gates.ps1` so `-AllowCompletedTask` is forwarded reliably to child quality gate scripts.

## Scope

Included:

- Replace helper-based switch forwarding with explicit argument splatting.
- Validate quality gates with active context and completed context.

Excluded:

- Application code changes

## Impacted Files

- scripts/harness/run-quality-gates.ps1

## Test Strategy

- Unit: run through `run-quality-gates.ps1`.
- Integration: run through `run-quality-gates.ps1`.
- Static analysis: run through `run-quality-gates.ps1`.
- Build: run through `run-quality-gates.ps1`.
- E2E: run through `run-quality-gates.ps1`.
- Hook simulation: run `run-quality-gates.ps1 -AllowCompletedTask` after completion.

## Rollback

Revert the `run-quality-gates.ps1` forwarding change.

## Notes

- Harness guard must pass before app code edits.
