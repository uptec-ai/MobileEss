# MobileEss

이동형 ESS(에너지저장장치) HMI — WPF 데스크톱 앱. PCS(전력변환)·BMS(배터리)·GPS를 감시/제어한다.
빌드·컨벤션 등 상세 지침은 `.claude/CLAUDE.md`, 도메인 문서는 `docs/`와 `.claude/docs/` 참조.

## scripts/setup-worktrees.ps1 — 클론 후 worktree 환경 재구성

git worktree는 PC별 로컬 구성이라 **클론에 포함되지 않는다**. 새 PC에서 클론을 받은 뒤
이 스크립트를 한 번 실행하면 병렬 개발 환경(도메인 worktree 4개)이 자동으로 재구성된다.
멱등(idempotent)이라 여러 번 실행해도 안전하다 — 이미 있는 항목은 건너뛴다.

### 하는 일

1. **worktree 4개 생성** — 클론 폴더 옆에 `{저장소명}-{기능}` 형태로 생성한다.

   | 기능 | Worktree 폴더 | Branch |
   |------|--------------|--------|
   | gps | `..\{저장소명}-gps` | feature/gps |
   | pcs | `..\{저장소명}-pcs` | feature/pcs |
   | bms | `..\{저장소명}-bms` | feature/bms |
   | history | `..\{저장소명}-history` | feature/history |

   로컬 브랜치가 있으면 그대로, 없으면 `origin/feature/*`에서, 그것도 없으면 main에서 생성한다.

2. **NTFS 정션 생성** — 각 worktree의 `packages\`(NuGet)와 `EMS_PJT_Hamburger\Maps\tiles\`(오프라인 지도 타일)를
   메인 클론 폴더로 연결한다(둘 다 git 미추적이라 정션 없이는 빌드 실패/지도 빈 화면).
   대상 폴더가 아직 없으면 경고만 하고 넘어간다 — 아래 사전 준비 후 재실행하면 된다.

3. **절대경로 자동 패치** — `.claude/skills/multi-task/workflow.js`의 `WORKTREE_MAP`과
   `.claude/CLAUDE.md`의 Worktree routing 표에 기록된 worktree 절대경로를 **이 PC의 클론 위치 기준으로** 고쳐 쓴다.
   (multi-task 워크플로우와 worktree 라우팅이 절대경로로 폴더를 찾기 때문에 PC마다 갱신이 필요하다.)
   파일이 바뀌면 커밋해 두고, 다른 PC로 옮기면 pull 후 스크립트를 다시 실행한다.

### 사용법

```powershell
git clone https://github.com/uptec-ai/MobileEss.git
cd MobileEss

# 사전 준비 (PC당 1회)
nuget restore EMS_PJT_Hamburger.sln    # 또는 Visual Studio에서 빌드 1회 → packages\ 복원
# 기존 PC에서 EMS_PJT_Hamburger\Maps\tiles 폴더 복사해 오기 (~52MB, git 미추적)

.\scripts\setup-worktrees.ps1
```

실행이 끝나면 `git worktree list`로 5개(메인 + 4개)가 보이면 정상이다.
이후 각 기능 영역 수정은 해당 worktree에서, 공유 파일은 메인 클론에서 작업한다
(라우팅 규칙: `.claude/CLAUDE.md`의 "Worktree routing" 섹션).
