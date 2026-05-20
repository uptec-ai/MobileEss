# Harness Rules

## 기본 원칙

- 작업 시작은 반드시 `scripts/harness/start-task.ps1`로 한다.
- 앱 코드 수정 전 `scripts/harness/guard-before-edit.ps1`을 통과해야 한다.
- 판단 근거와 변경 내용은 `scripts/harness/write-log.ps1`로 계속 기록한다.
- 완료 전 품질 게이트 순서는 단위 테스트, 통합 테스트, 정적 분석, 빌드, E2E 검증이다.
- 리팩토링은 명시 요청이 있을 때만 한다.
- 워크스페이스 밖 파일은 수정, 저장, 삭제하지 않는다.

## 현재 프로젝트 맞춤 규칙

- Target Framework는 `.NET Framework 4.8`로 취급한다.
- 빌드는 Visual Studio/MSBuild 기반으로 수행한다.
- NuGet은 `packages.config`와 `packages/` 폴더 참조를 우선한다.
- DevExpress는 `v23.1.5`, SciChart는 `8.6.0.28199` 기준으로 검증한다.
- View/ViewModel/Model/Manager 기존 폴더 구조를 유지한다.
- WPF View의 `DataContext`가 `App.xaml.cs`에서 직접 주입되는 현재 패턴을 바꾸지 않는다.
- DB 변경은 `DbManager` 경유를 원칙으로 하며, SQL 문자열은 파라미터 바인딩을 우선한다.
- 장비 통신 주소, DB 연결 문자열, SciChart 라이선스는 코드에 하드코딩하지 않는다.

## 금지/주의

- Harness 없이 앱 코드 수정 금지.
- 사용자 변경을 되돌리는 명령 금지.
- DevExpress/SciChart 버전 업그레이드, .NET SDK 스타일 전환, DI 도입은 별도 리팩토링 작업으로 분리한다.
- 장비 제어 로직 변경 시 롤백 방법과 수동 검증 절차를 계획 문서에 적는다.
