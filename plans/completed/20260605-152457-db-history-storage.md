# db-history-storage

- Task ID: 20260605-152457-db-history-storage
- Status: active
- Created: 2026-06-05 15:24:57

## Goal

Build the database-backed storage foundation for ESS runtime data and connect the History view to DB data:
system ready states, compressed raw PCS/BMS payloads, improved alarm records, and history binding.

## Scope

Included:
- Inspect existing WPF/MVVM, PostgreSQL, alarm, PCS/BMS data, and History view code.
- Add or adjust database schema and access code inside the workspace.
- Store PCS/BMS ready states, selected compressed raw data, and improved alarms using `EMS_DB_CONN`.
- Bind the History view to DB-backed data and remove only clearly obsolete tables created or superseded by this task.

Excluded:
- Broad refactoring unrelated to DB/history storage.
- Changes outside `C:\Project\2. ESS`.
- Source-controlled DB secrets or license keys.

## Impacted Files

- `EMS_PJT_Hamburger` project files under `Models`, `ViewModels`, and `Views`.
- Any local SQL/schema files if the project already uses them or if a focused schema artifact is needed.
- Harness plan/log files.

## Test Strategy

- Unit: Run project harness unit test script; expect warning/pass if no test project exists.
- Integration: Run harness integration script; use local DB only if configured and reachable.
- Static analysis: Run harness static analysis.
- Build: Run harness build script/MSBuild for the WPF project.
- E2E: Run harness E2E script; report if not available for this desktop app.

## Rollback

Revert the files changed by this task and restore any DB schema changes using the generated SQL rollback/drop statements where applicable. Do not revert unrelated user changes.

## Notes

- Harness guard must pass before app code edits.
