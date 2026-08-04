# MobileEss (EMS_PJT_Hamburger) 인수인계 문서

- 최종 갱신: 2026-08-04
- 대상: 이동형 ESS(에너지저장장치) HMI — WPF 데스크톱 애플리케이션
- 저장소: https://github.com/uptec-ai/MobileEss (main 브랜치)

---

## 1. 시스템 개요

PCS(전력변환장치)·BMS(배터리관리시스템)·GPS를 감시/제어하는 차량 탑재형 ESS HMI.

| 구분 | 내용 |
|---|---|
| 플랫폼 | Windows, .NET Framework 4.8, WPF (MVVM) |
| UI | DevExpress WPF 23.1.5 (지도: DevExpress.Xpf.Map 오프라인 타일) |
| 차트 | SciChart 8.6 |
| PCS 통신 | Modbus TCP (NModbus4) — 기본 `127.0.0.1:502` (루프백, 실장비 검증 대기) |
| BMS 통신 | CAN (Peak PCANBasic.NET, PCAN-USB) |
| GPS | Serial NMEA (BU-353N, COM 포트) |
| DB | PostgreSQL (Npgsql 8.0.3) |
| 로깅 | NLog 5.4 (`NLog.config`) |

## 2. 저장소 구조 · 빌드 · 실행

```
EMS_PJT_Hamburger.sln
├─ EMS_PJT_Hamburger/            앱 본체 (Views / ViewModels / Models / Converters …)
│  ├─ Models/Client/{PCS,BMS,GPS}/   장치 통신 계층
│  ├─ Models/Managers/               DbManager 등 공용 매니저
│  └─ Maps/tiles/{z}/{x}/{y}.png     오프라인 지도 타일 (git 미추적, ~52MB — 별도 복사 필요)
├─ EMS_PJT_DeploymentInstaller/  배포 설치 프로젝트
├─ docs/                         문서 (본 문서, harness 워크플로우 등)
├─ scripts/                      harness 태스크 스크립트, setup-worktrees.ps1, Resolve-ProjectRoot.ps1
└─ plans/completed/              작업 이력(태스크 플랜)
```

- 빌드: Visual Studio 2022 / `MSBuild EMS_PJT_Hamburger.sln /p:Configuration=Debug /p:Platform="Any CPU"`
  (구식 csproj + packages.config — `dotnet build` 아님. 최초 1회 `nuget restore` 필요)
- 실행: `EMS_PJT_Hamburger\bin\Debug\EMS_PJT_Hamburger.exe`
- 새 PC 셋업: clone → `nuget restore` → `Maps\tiles` 복사 → `.\scripts\setup-worktrees.ps1`
  (병렬 개발용 worktree 4개 + NTFS 정션 자동 구성 — 상세는 `README.md`)

## 3. 설정

### App.config (`appSettings`)

| 키 | 기본값 | 설명 |
|---|---|---|
| `PcsHost` | `127.0.0.1` | PCS Modbus TCP 호스트. **실장비 연결 시 장비 IP로 변경** |
| `PcsPort` | `502` | Modbus TCP 포트 |
| `PcsTimeoutMs` | `10000` | PCS 통신 타임아웃(ms) |
| `GpsPort` | `COM19` | GPS 시리얼 포트. 빈값이면 GPS 미사용 |
| `GpsBaud` | `4800` | GPS 보레이트 (BU-353N 실측 8N1) |

### 환경변수 (시크릿 — 소스/커밋 금지)

| 변수 | 용도 |
|---|---|
| `EMS_SCICHART_LICENSE_KEY` | SciChart 라이선스 키 |
| `EMS_DB_CONN` | PostgreSQL 연결 문자열 (미설정 시 App.config 경로 사용) |

## 4. 외부 인터페이스

| 인터페이스 | 프로토콜 | 구현 위치 |
|---|---|---|
| PCS | Modbus TCP, 레지스터 스펙 `PcsSpecs.cs` | `Models/Client/PCS/` (`PcsModel`, `PcsViewModel`) |
| BMS | CAN — 상태 0x150~0x153, 팩 0x154~, 릴레이 명령 0x180 | `Models/Client/BMS/` (`PcanRxService`, `BmsSpecs`, `CanFieldSpec`) |
| GPS | NMEA 파싱 → 오프라인 지도 표시 | `Models/Client/GPS/` (`GpsService`, `NmeaParser`) |
| DB | PostgreSQL | `Models/Managers/DbManager.cs` |

## 5. 데이터베이스 (주요 테이블)

스키마 원본: `..\DB_EMS.sql` + `DbManager`의 `create table if not exists` (앱이 자체 보정).

| 테이블 | 내용 |
|---|---|
| `tb_ems_alarm` | **통합 알람** (BMS/PCS 공용: source, category, code, severity, ack/reset 컬럼, 인덱스 2종) |
| `tb_bms_alarm` | BMS 알람 (레거시 — 통합 알람과 이중 기록) |
| `tb_pcs_grid` | PCS 계통 누적 전력량 **하루 1행** (당일 행을 upsert — 일일 누적 전력량 계산의 기준) |
| `tb_ems_raw_data` | 원시 수집 데이터(압축 JSON payload + `pcs_total_export/import_kwh`, `bms_soc` 발췌 컬럼) |
| `tb_ems_system_state` | PCS/BMS Ready 상태 이력 |
| `tb_ems_control_log` | 제어 명령 감사 로그 (Start/Complete/Canceled/Failed) |
| `tb_bms` | BMS 스냅샷(전압/전류/SOC) |

## 6. 주요 기능 · 코드 맵

| 화면/기능 | ViewModel | 비고 |
|---|---|---|
| Home | `HomeViewModel` | 운전모드, PCS 충/방/정지, BMS 릴레이, BMS Guard 토글, GPS 지도 |
| PCS | `PcsViewModel` | Grid/Inverter/Battery/Load/Control 섹션, 전력 트렌드 차트(Δ), 제어 시퀀스 |
| BMS | `BMSViewModel` | 팩 17개 모니터, Ready/Relay, 알람 |
| History | `HistoryViewModel` | 기간 조회(분/시 버킷), 알람 이력, 전력량 Δ 차트 |
| 알람 상세 | `AlarmDetailWindowViewModel` | 전체 / 최근 100 / 현재 발생 조회 + 내보내기(txt) |

### 안전 · 제어 흐름 (실계통 검증 전 필수 숙지)

- **제어 확인**: 모든 PCS/BMS 제어 버튼은 `ControlConfirmationService.Confirm()` 확인 대화상자를
  거친다. 우회 금지.
- **BMS Guard**: 홈 화면 토글(`PcsVm.IsBmsGuardEnabled`). PCS 충/방전 명령 전 BMS
  ready/fault/SOC 상태를 반영하고, 폴링 중 SOC 기반 정지 정책을 강제한다
  (`ApplyBmsGuardPolicyAsync`, plans/completed의 20260528 태스크 참조). **실계통 검증 대기.**
- 모든 제어 명령은 `tb_ems_control_log`에 감사 기록된다.

### 알람 체계 (2026-08-04 완성)

1. **발생**: BMS CAN fault(0x151) → `BmsDataModel.SaveFaults` → `tb_bms_alarm` + `tb_ems_alarm` 기록.
   PCS fault는 레지스터 비트 → 실시간 fault 목록 + `tb_ems_alarm`.
2. **폴링**: `AlarmService`(BMS 알람 창이 열린 동안 5초 주기) — `tb_ems_alarm`에서 마지막
   `alarm_id` 이후 신규 행을 감지해 창 목록에 실시간 반영. 첫 폴링은 기준점만 설정(과거 알람 미발행).
3. **조회/내보내기**: `AlarmDetailWindow` (BMS/PCS 소스별).

### 일일 누적 전력량 (2026-08-04 복원)

- PCS 화면 Load 섹션에 "수전/송전 누적 전력량(일간)" 표시.
- 계산: 일간 = 현재 총 누적(kWh, Grid 레지스터) − 전일 마감 누적(`tb_pcs_grid`의 전일 행).
  기준값은 하루 1회 로드, 날짜가 바뀌면 자동 재로드. 전일 데이터가 없는 최초 가동일은
  현재 총량 기준 0부터 집계.

## 7. 개발 워크플로우

- **태스크 하네스**(`AGENTS.md`, `docs/harness/workflow.md`): `start-task.ps1` → 플랜 작성 →
  `guard-before-edit.ps1` → 구현 → `write-log.ps1` → `run-quality-gates.ps1` →
  `suggest-commit-message.ps1` → `complete-task.ps1`. 앱 코드 수정은 guard 통과가 전제.
- **Git 훅**(`.githooks/`): commit-msg(conventional commits, 제목 72자),
  pre-commit/pre-push(품질 게이트 = 빌드 포함). 주의: `logs/harness/<task-id>.log`는
  머신 로컬(미추적)이라 다른 PC에서 pull 시 최근 완료 태스크의 로그를 한 줄짜리로 재생성해야
  게이트가 통과한다.
- **Worktree 라우팅**: 기능별 worktree 4개(gps/pcs/bms/history)가 저장소 옆에 존재.
  매칭 파일은 반드시 해당 worktree에서 수정(`.claude/CLAUDE.md`의 Worktree routing 표가
  소스오브트루스). 공유 파일(App.*, Managers 등)만 메인에서 수정.
- **테스트**: 현재 테스트 프로젝트 없음(하네스가 경고 후 통과). 구축 보류 상태 — 신규 구축 시
  하네스가 `*Test*.csproj` 패턴을 자동 인식한다(`Get-TestProjects`).

## 8. 인수 시점 잔여 항목 (KongBoard 기준, 2026-08-04)

| # | 항목 | 상태 | 비고 |
|---|---|---|---|
| 1 | 실장비 연결 검증 | **대기(하드웨어 필요)** | 코드 측 준비 완료 — `PcsHost`만 장비 IP로 변경 |
| 2 | BMS Guard 실계통 검증 | **대기(하드웨어 필요)** | 정책 구현 완료, 실계통 확인만 남음 |
| 3 | Alarm Service (알람 DB 폴링) | ✅ 완료 (2026-08-04) | §6 알람 체계 |
| 4 | 일일 누적 전력량 조회 | ✅ 완료 (2026-08-04) | §6 일일 누적 전력량 |
| 5 | 실차 주행 검증 | **대기(차량 필요)** | GPS COM19@4800 실측 설정 완료 |
| 6 | 테스트 프로젝트 구축 | 보류 | §7 테스트 |
| 7 | 인수 문서 정비 | ✅ 본 문서 | |

### 실장비 검증 체크리스트 (권장 절차)

1. `App.config`의 `PcsHost`를 실장비 IP로 변경 후 연결 상태·타임아웃/재연결 동작 확인.
2. PCS 각 섹션(Grid/Inverter/Battery/Load) 값 및 상단 패널 총 누적 전력량 표시 확인.
3. **일일 누적 전력량**: `tb_pcs_grid`에 전일 행이 있는 상태에서 일간값이 0부터 증가하는지,
   자정 경과 시 기준값이 갱신되는지 확인.
4. **알람 폴링**: BMS 알람 창을 연 상태에서
   `insert into tb_ems_alarm(source, alarm_code, alarm_name) values('BMS', 99, '수동 테스트');`
   실행 → 5초 내 목록 반영 확인.
5. **BMS Guard**: Guard ON 상태에서 BMS fault/저SOC 조건을 만들어 충/방전 명령 차단·정지 정책
   동작 확인 (제어 감사 로그 `tb_ems_control_log` 동시 확인).
6. 실차: GPS 수신(시/도/군 표시), 주행 중 지도 추적, 진동/전원 환경에서 CAN·Modbus 안정성.

## 9. 문서 맵

| 문서 | 내용 |
|---|---|
| `README.md` | 저장소 개요, 새 PC 셋업(setup-worktrees) |
| `AGENTS.md` | 태스크 하네스 규칙(필수 워크플로우) |
| `PATCH_NOTES.md` | 과거 패치 이력 |
| `docs/harness/*.md` | 워크플로우/규칙/DB/분석 문서 |
| `docs/PCSView_Button_Sequence.txt` | PCS 버튼 제어 시퀀스 (한양정공 원본 포함) |
| `plans/completed/*.md` | 전체 작업 이력(태스크 단위 목표/범위/전략) |
| `.claude/CLAUDE.md`, `.claude/rules/`, `.claude/docs/` | AI 협업용 프로젝트 규칙·도메인 문서 |
| `..\DB_EMS.sql` | DB 스키마 원본 |
