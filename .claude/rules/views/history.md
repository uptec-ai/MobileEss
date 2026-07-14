---
paths:
  - "EMS_PJT_Hamburger/Views/HistoryView.xaml"
  - "EMS_PJT_Hamburger/Views/HistoryView.xaml.cs"
  - "EMS_PJT_Hamburger/ViewModels/HistoryViewModel.cs"
---
# View: History

## 목적
이력 데이터 조회/시각화. 기간별 ESS 운전 이력을 PostgreSQL에서 읽어 SciChart로 표시.

## 상태
구현 완료(세부는 코드 확인).

## 담당 ViewModel / 소유
`HistoryViewModel`(= `App.HistoryVm`).

## 데이터 & 외부 I/O
- DB: PostgreSQL(Npgsql) via `Models/Managers/DbManager`. 테이블은 `EnsureEssHistoryTables()`.
- 직렬화: `System.Text.Json`(`JsonSerializer`).
- 차트: SciChart 8.6.

## UI 표면
기간 선택(DateNavigator; Licenses.licx에 등록됨) + 차트 시리즈.

## 주의사항 / 규칙
- 데이터 접근 규칙 `.claude/rules/data-postgres.md`, 차트 규칙 `.claude/rules/charting-scichart.md` 참조.
- 대용량 이력은 UI 스레드 블로킹 주의(비동기 조회 + 필요 시 다운샘플).

## 관련 문서
`.claude/rules/data-postgres.md`, `.claude/rules/charting-scichart.md`, `docs/database.md`
