# .claude/docs — 온디맨드 도메인 레퍼런스 (인덱스)

이 폴더는 **자동 로드되지 않는다**. 필요할 때 경로로 열어 읽는다. 무거운 원본 스펙은 `docs/`와 `Document/`(상위 `2. ESS\Document`)에 있고, 여기서는 그것을 가리키는 인덱스 + 간결한 도메인 모델만 둔다.

## 기존 규칙/스펙 문서 (원본 = source of truth)
- `docs/rules.md`, `docs/wpf-rules.md` — 코딩/ WPF 규칙
- `docs/workflow.md` — 태스크 하네스 워크플로우(AGENTS.md가 강제)
- `docs/database.md` — DB 스키마/사용
- `docs/analysis.md`, `docs/qc.md` — 분석/품질
- `docs/PCSView_Button_Sequence*.txt` — PCS 버튼 시퀀스(제어 순서)
- `Document/DB.xlsx`, `Document/CAN_DB_XPack_ESV_*.xlsx` — DB/CAN 정의
- `Document/*.png/pptx/pdf` — 단선도, 순서도, 제품 브로슈어, 회의자료

## 도메인 모델 (요약)
- **ESS(이동형)**: 컨테이너형 에너지저장. PCS + BMS + PV + (옵션)EV충전기 + 부하.
- **운전모드 `HomeStatus`**: 충전(Charging) / 방전(Discharging) / 대기(Waiting).
- **부하 대상 `LoadStatus`**: OnGrid / OffGrid(독립운전) / Vehicle(차량) / Waiting.
- **PCS**: 계통/부하와 배터리 사이 전력변환. Modbus TCP로 감시·제어(충/방/정지, 릴레이).
- **BMS**: 배터리 보호/상태(SoC, 전압, 전류, 릴레이, 알람). CAN 통신.
- **GPS**: 이동형이라 현재 위치를 표시. BU-353N NMEA → 오프라인 지도 + 시/도/군.

## 규칙(rules) 맵 — path-scoped, 파일 열 때만 로드
- `.claude/rules/gps-map.md` — GPS + 오프라인 지도
- `.claude/rules/pcs-modbus.md` — PCS/Modbus
- `.claude/rules/bms-can.md` — BMS/CAN
- `.claude/rules/charting-scichart.md` — 차트(SciChart)
- `.claude/rules/data-postgres.md` — DB(PostgreSQL)
- `.claude/rules/views/{home,pcs,bms,history}.md` — 화면별 문서
