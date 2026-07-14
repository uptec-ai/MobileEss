---
name: multi-task
description: 작업 규모를 분석해 순차·병렬·팀토론 중 최적 전략을 자동 선택해 실행한다. 복수의 기능을 동시에 개발하거나 복잡한 설계 결정이 필요할 때 사용한다. 단일 파일 소규모 작업에는 과하므로 쓰지 않는다.
---

## 도메인 특화 규칙

- 기능-Worktree 매핑 (현재는 단일 GPS 개발 브랜치 — 실재 worktree는 1개):

  | 기능 | Worktree 경로(절대) | Branch |
  |------|--------------------|--------|
  | gps | `C:/Project/2. ESS/0706_gps` | feature/gps |

  > 병렬 개발이 필요해지면 `git worktree add ../0706_gps-<feature> -b feature/<feature>` 로
  > worktree를 추가하고, 위 표 + `workflow.js`의 `WORKTREE_MAP`/`FEATURE_AGENT`에 절대경로로 등록한다.

- 공유 파일: `App.xaml.cs`, `MainWindow.xaml(.cs)` → 메인(gps) worktree에서만 수정
- 통합 브랜치: `integrate/{작업명}-{YYYYMMDD}`
- 빌드 명령: `MSBuild "<worktree>\EMS_PJT_Hamburger.sln" /p:Configuration=Debug /p:Platform=AnyCPU` (VS2022 MSBuild.exe 전체 경로 사용; `dotnet build` 아님)
- Agent Team(.claude/agents/): gps-map-engineer, pcs-modbus-engineer, bms-can-engineer, history-data-engineer, wpf-mvvm-engineer, build-verify-qa

## 규모 분류 기준 (자동 선택)

| 규모    | 조건                                      | 자동 선택 전략                              |
|---------|-------------------------------------------|--------------------------------------------|
| small   | 단일 파일 · 공유 파일 미포함               | 순차 실행                                  |
| medium  | 복수 파일 · 독립 worktree 분리 가능        | 병렬 실행 + main 인라인 검토               |
| large   | 공유 파일 포함 · 아키텍처 변경 · 회귀 위험 | 병렬 실행 + build-verify-qa 통합 검토      |
| complex | 설계 결정 필요 · 상충 요구사항             | 팀 토론 → 합의 → 병렬 실행 + 검토          |

## 절차
1. 규모 분석 — 작업 목록을 분류하고 기능에 배분한다.
2. 사용자 확인 — 분류 결과와 실행 전략을 보여주고 승인을 받는다.
3. 규모별 실행 (workflow.js)
4. 통합 및 검토
5. 에러 핸들링 — 개별 실패는 `buildSuccess:false`로 계속 진행, CRITICAL 발견 시 `mergeBlocked` 확인.
6. 최종 보고

## 원칙
- 사용자 확인 없이 실행을 시작하지 않는다.
- CRITICAL 검토 항목은 main merge 전에 반드시 해소한다(`mergeBlocked`).
- complex 작업은 합의 없이 구현을 시작하지 않는다.
- **단일 기능 소규모 작업엔 이 스킬을 쓰지 않는다** — 직접 처리가 낫다(과잉 오케스트레이션 방지).

## 산출물
필요 시: `multi-task-result-{YYYYMMDD}.md`
