# Harness Workflow

## 작업 시작

```powershell
.\scripts\harness\start-task.ps1 -Title "작업 제목"
```

생성물:

- `current-task.json`
- `plans/active/<task-id>.md`
- `logs/harness/<task-id>.log`

## 계획 작성

계획 문서에 다음 항목을 채운다.

- 목표
- 범위
- 영향 파일
- 테스트 전략
- 롤백 방법

## 코드 수정 전 가드

```powershell
.\scripts\harness\guard-before-edit.ps1
```

다음이 없으면 실패한다.

- `current-task.json`
- 활성 계획 파일
- 활성 로그 파일

## 구현 로그

```powershell
.\scripts\harness\write-log.ps1 -Message "변경 내용과 판단 근거"
```

## 품질 게이트

```powershell
.\scripts\harness\run-quality-gates.ps1
```

순서:

1. 단위 테스트
2. 통합 테스트
3. 린트/정적 분석
4. 빌드
5. E2E 검증

## 완료

```powershell
.\scripts\harness\suggest-commit-message.ps1
.\scripts\harness\complete-task.ps1
```

완료 시 계획 문서는 `plans/completed`로 이동하고 `current-task.json`은 제거된다.

Git hooks are allowed to run quality gates after completion. In that case they use the latest completed plan/log as read-only task context. This does not relax edit safety: app code edits still require a live `current-task.json` and `guard-before-edit.ps1`.
