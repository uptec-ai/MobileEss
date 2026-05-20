# Database Rules

## 연결

- DB는 PostgreSQL/Npgsql 기반이다.
- 연결 문자열 우선순위:
  - `EMS_DB_CONN`
  - `App.config`의 `connectionStrings:EMS_DB`
- 연결 문자열과 비밀번호를 소스에 직접 저장하지 않는다.

## 접근 계층

- DB 접근은 `Models/Managers/DbManager.cs`를 경유한다.
- SQL 값은 파라미터 바인딩을 사용한다.
- 테이블명처럼 파라미터화할 수 없는 값은 화이트리스트를 사용한다.

## 검증

- DB 통합 테스트가 필요한 작업은 `EMS_RUN_DB_INTEGRATION=1`과 `EMS_DB_CONN`을 설정한다.
- DB 스키마 변경은 롤백 SQL 또는 백업 절차를 계획 문서에 적는다.
- 앱 시작 시 `DbManager` 초기화 여부와 Null 호출 가능성을 확인한다.
