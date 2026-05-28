# Quality Gates

## Unit Test

- `*Test*.csproj`, `*Tests*.csproj`를 자동 검색한다.
- 현재 솔루션에는 테스트 프로젝트가 없으므로 경고 통과한다.
- 테스트 프로젝트가 추가되면 자동 실행한다.

## Integration Test

- `*IntegrationTest*.csproj`, `*IntegrationTests*.csproj`를 자동 검색한다.
- DB 통합 검증은 `EMS_RUN_DB_INTEGRATION=1`일 때 `EMS_DB_CONN`이 없으면 실패한다.
- 장비 연동 테스트는 실제 PCS/BMS 장비 또는 시뮬레이터가 준비된 작업에서 별도 계획에 적는다.

## Static Analysis

- 필수 Harness 산출물 존재 여부를 확인한다.
- `AGENTS.md`가 200줄 이하인지 확인한다.
- `current-task.json`, 계획 파일, 로그 파일이 유효한지 확인한다.
- App.config/NLog.config에 SciChart 라이선스나 DB 비밀번호가 직접 들어가지 않았는지 확인한다.

## Build

- `MSBuild.exe`를 탐색한다.
- `EMS_PJT_Hamburger.sln`을 `Release|Any CPU`로 빌드한다.

## E2E

- WPF 앱 특성상 기본 E2E는 산출물 존재와 주요 런타임 설정을 검증한다.
- `EMS_SCICHART_LICENSE_KEY`가 없으면 경고를 기록한다.
- 실제 UI 실행/장비 연결/DB 확인은 작업 계획에 명시된 경우에만 확장한다.

## Commit/Push 차단

- `pre-commit`: 전체 품질 게이트 실행
- `commit-msg`: 커밋 메시지 규칙 검증
- `pre-push`: 통합 테스트, 빌드, E2E 검증 실행
- Commit/push hooks may use the latest completed task context when `current-task.json` has already been removed by `complete-task.ps1`.
- App code edits still require an active task and must not rely on completed context.
