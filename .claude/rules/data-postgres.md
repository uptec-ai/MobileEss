---
paths:
  - "EMS_PJT_Hamburger/Models/Managers/DbManager.cs"
  - "EMS_PJT_Hamburger/ViewModels/HistoryViewModel.cs"
---
# 데이터 접근(PostgreSQL) 규칙

매칭 파일 편집 시에만 로드.

- **드라이버**: Npgsql 8.0.3. 연결 문자열은 env `EMS_DB_CONN` 또는 App.config. **시크릿을 소스/커밋에 넣지 말 것.**
- **스키마**: `DbManager.EnsureEssHistoryTables()`가 이력 테이블 보장. 스키마 변경은 이 초기화와 동기화. 참고: `docs/database.md`, Document의 `DB.xlsx`.
- **직렬화**: 페이로드 JSON은 `System.Text.Json`(`JsonSerializer`). Newtonsoft 아님.
- **실패 처리**: DB 미가용은 치명적 예외로 앱을 죽이지 말 것(기존 `App.InitManagers`는 warn 후 계속). 이 패턴 유지.
- **쿼리**: 파라미터 바인딩 사용(문자열 연결 SQL 금지).
