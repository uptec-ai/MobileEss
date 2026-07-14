# EMS_PJT_Hamburger (MobileEss) — 프로젝트 지침

이동형 ESS(에너지저장장치) HMI. WPF 데스크톱 앱으로 PCS(전력변환)·BMS(배터리)·PV·EV충전기·부하를 감시/제어하고, GPS 위치를 오프라인 지도에 표시한다. 상세 규칙·도메인은 `docs/`와 `.claude/docs/`에 있다.

<!-- 기존 하네스(AGENTS.md + scripts/harness + docs/)와 공존. 태스크 워크플로우는 AGENTS.md를 따른다. -->
@../AGENTS.md

## 기술 스택 (출처 = 매니페스트: EMS_PJT_Hamburger.csproj / packages.config)
- .NET Framework 4.8, WPF, MVVM (DevExpress `ViewModelBase`/`DelegateCommand` + PropertyChanged.Fody 4.1)
- UI: DevExpress WPF **23.1.5** (지도 = `DevExpress.Xpf.Map`). 차트: SciChart 8.6
- 통신: PCS = Modbus(NModbus4 2.1) · BMS = CAN(Peak PCANBasic.NET) · GPS = Serial NMEA
- DB: PostgreSQL(Npgsql 8.0.3) · 로그: NLog 5.4 · JSON: System.Text.Json 10.0.4
- Shell: PowerShell · 빌드: **MSBuild** (구식 csproj + packages.config; `dotnet build` 아님)

## 프로젝트 루트 — 포터블, 절대경로 금지
이 저장소는 어느 폴더로든 clone될 수 있다. 루트는 앵커(`EMS_PJT_Hamburger.sln`)에서 해석한다:
`$root = & ".\scripts\Resolve-ProjectRoot.ps1"` (앵커까지 상위로 탐색). 빌드/실행은 `$root` 기준으로 작성.

## 빌드 / 실행
- 기존 하네스 게이트: `.\scripts\harness\run-build.ps1`, `.\scripts\harness\run-quality-gates.ps1`
- 직접 빌드: `MSBuild "$root\EMS_PJT_Hamburger.sln" /p:Configuration=Debug /p:Platform=AnyCPU`
- 실행: `"$root\EMS_PJT_Hamburger\bin\Debug\EMS_PJT_Hamburger.exe"`
- 시크릿: SciChart = `EMS_SCICHART_LICENSE_KEY`(env), DB = `EMS_DB_CONN` 또는 App.config. 소스에 넣지 말 것.

## 레이아웃
- `EMS_PJT_Hamburger/` — 앱 본체
  - `Views/*.xaml(.cs)` — 화면(Home/PCS/BMS/History/DashBoard/System …)
  - `ViewModels/*.cs` — 뷰모델
  - `Models/Client/{PCS,BMS,GPS}/` — 장치 통신(Modbus/CAN/Serial)
  - `Models/Managers/` — Db/Status/Convert/Animation/Alarm 등 공용
  - `Converters/`, `Assets/`, `Fonts/`, `Maps/tiles/` — 리소스(지도 타일은 빌드 복사 안 함)
- `EMS_PJT_DeploymentInstaller/` — 배포 설치 프로젝트
- `docs/` — 규칙/워크플로우(rules.md, wpf-rules.md, workflow.md, database.md …)
- `scripts/harness/` — 기존 태스크 하네스 · `scripts/Resolve-ProjectRoot.ps1` — 포터블 루트 리졸버

## 컨벤션 (검증 가능한 규칙)
- MVVM 구조 유지: `Views`/`ViewModels`/`Models`/`Models/Managers`/`Models/Client`. 요청 없이 리팩터링·되돌리기 금지.
- 한글 주석/문자열 포함 `.cs`는 **UTF-8 BOM**으로 저장(BOM 없으면 csc가 CP949로 오독 → 주석/문자열 mojibake).
- 앱 종료 시 폴링/스레드 자원은 `App.OnExit → 각 VM.Dispose`에서 해제(CancellationTokenSource 취소 · 시리얼/포트 정리).
- DevExpress/SciChart 라이선스·DB 시크릿을 커밋하지 말 것.
- 빌드 산출물(`bin`/`obj`/`*.g.cs`/`*_wpftmp.csproj`) 편집 금지.

## 유닛(View)별 문서 — 토큰 최소화
화면 1개당 `.claude/rules/views/<name>.md` 1개를 `paths:`로 스코프 → 해당 화면 파일 편집 시에만 로드.
새 화면: `.claude/templates/view-doc-template.md` 복사 → `paths:` 지정 → 채운다. 현재 문서화: Home/PCS/BMS/History.

## 경계
- 워크스페이스(= 저장소 루트) 밖 파일은 확인 없이 수정 금지.
- `.claude/**` 문서 언어: **한국어**.

## 도메인 quick-facts (상세는 `.claude/docs/README.md`, `docs/`)
- 운전모드: 충전 / 방전 / 대기(`HomeStatus`). 부하 대상: OnGrid / OffGrid / Vehicle(`LoadStatus`).
- PCS: Modbus TCP(App.config `PcsHost`/`PcsPort`). 충전·방전·정지 + 릴레이 토글.
- BMS: CAN(PCAN). Ready·Relay·전압/전류·SoC.
- GPS: BU-353N, Serial NMEA(App.config `GpsPort`=COM19@4800). 위치 → 오프라인 DevExpress 지도 + 시/도/군.
- 지도 타일: `Maps/tiles/{z}/{x}/{y}.png`(오프라인, 빌드 복사 안 함, 런타임 직접 참조). 시/도/군: `Maps/skorea_muni.json`.

## 에이전트 팀 (조직 → 실행)
전문 에이전트 페르소나는 `.claude/agents/*.md`에 정의(GPS·PCS·BMS·이력/차트·WPF·빌드검증). 실행/조율은 **`init-multi-task`의 `multi-task` 워크플로우가 유일한 오케스트레이터**다(에이전트를 `agentType`으로 병렬 호출). 여러 서브시스템에 걸친 큰 작업은 이 워크플로우로 팬아웃한다.

`multi-task`는 `.claude/skills/multi-task/`(SKILL.md + workflow.js)로 구현됨 — **병렬 다기능 작업 전용 opt-in**(단일 소규모 작업엔 쓰지 않는다). 현재 실재 worktree는 메인(`feature/gps`) 1개뿐이라 별도 "항상 worktree 라우팅" 규칙은 두지 않는다. 병렬 개발을 시작하면 `git worktree add ../0706_gps-<feature> -b feature/<feature>` 후 `workflow.js`의 `WORKTREE_MAP`/`FEATURE_AGENT`와 이 문서에 **절대경로**로 등록한다.

**변경 이력:**
| 날짜 | 변경 내용 | 대상 | 사유 |
|------|----------|------|------|
| 2026-07-13 | harness 컨텍스트 초기 구성 | `.claude/CLAUDE.md`·rules·docs·templates, `scripts/Resolve-ProjectRoot.ps1` | 하네스 체인 1단계 |
| 2026-07-13 | 에이전트 페르소나 6종 생성 | `.claude/agents/*.md` | 하네스 체인 2단계(harness-team) |
| 2026-07-13 | multi-task 실행 인프라 생성(worktree 미생성) | `.claude/skills/multi-task/*` | 하네스 체인 3단계(init-multi-task) |
