# Organize handover documentation

- Task ID: 20260804-171356-organize-handover-documentation
- Status: active
- Created: 2026-08-04 17:13:56

## Goal

인수인계 문서(`docs/handover.md`)를 신규 작성해 흩어진 정보(README, AGENTS, docs/harness,
plans/completed, DB 스키마, 설정)를 한 문서로 정리한다. (KongBoard 남은 목표 #7 "인수 문서 정비")

## Scope

- 포함: `docs/handover.md` 신규 작성(시스템 개요·빌드/배포·설정·인터페이스·DB·기능 맵·
  알람 체계·개발 워크플로우·잔여 검증 체크리스트·문서 맵), `README.md`에 링크 1줄 추가.
- 제외: 앱 코드 수정 없음. 기존 문서 이동/삭제 없음.

## Impacted Files

- `docs/handover.md` (신규)
- `README.md` (링크 추가)

## Test Strategy

- Unit/Integration: 해당 없음(문서 전용).
- Static analysis / Build / E2E: pre-commit 게이트로 기존 빌드 무결성만 재확인.
- 문서 내용은 본 세션에서 코드로 검증한 사실만 기재(추정 금지).

## Rollback

- 커밋 전: 신규 파일 삭제 + `git checkout -- README.md`
- 커밋 후: 해당 커밋 `git revert`.

## Notes

- Harness guard must pass before app code edits. (본 태스크는 문서 전용)
