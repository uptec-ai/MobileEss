export const meta = {
  name: 'ess-multi-task',
  description: '작업 규모에 따라 순차/병렬/팀토론을 자동 선택해 실행하고 통합 검토까지 수행',
  phases: [
    { title: '규모 분석' }, { title: '팀 토론' },
    { title: '합의' },      { title: '실행' }, { title: '검토' },
  ],
}

// ── 프로젝트 특화 상수 (init-multi-task가 채움) ──────────────────
// WORKTREE_MAP은 실재하는 worktree의 절대경로만 등록한다(`git worktree list` 기준).
// 메인 저장소(C:/Project/2. ESS/EMS_PJT v1.4/EMS_PJT_Hamburger, main)는 공유 파일
// 전용이라 등록하지 않는다. worktree를 추가/제거하면 여기와 FEATURE_AGENT,
// .claude/CLAUDE.md의 Worktree routing 표를 함께 갱신한다.
const WORKTREE_MAP = {
  gps:     'C:/Project/2. ESS/EMS_PJT v1.4/EMS_PJT_Hamburger-gps',
  pcs:     'C:/Project/2. ESS/EMS_PJT v1.4/EMS_PJT_Hamburger-pcs',
  bms:     'C:/Project/2. ESS/EMS_PJT v1.4/EMS_PJT_Hamburger-bms',
  history: 'C:/Project/2. ESS/EMS_PJT v1.4/EMS_PJT_Hamburger-history',
}

// 기능 → 도메인 담당 에이전트(.claude/agents/*.md). 없으면 WORKER_AGENT_TYPE로 폴백.
const FEATURE_AGENT = {
  gps:     'gps-map-engineer',
  pcs:     'pcs-modbus-engineer',
  bms:     'bms-can-engineer',
  history: 'history-data-engineer',
}

const BUILD_CMD = '& "C:\\Program Files\\Microsoft Visual Studio\\2022\\Community\\MSBuild\\Current\\Bin\\MSBuild.exe" EMS_PJT_Hamburger.sln /p:Configuration=Debug "/p:Platform=Any CPU" (작업 디렉토리에서 실행; dotnet build 아님)'

// complex 티어 설계 토론 패널(4관점). agentType은 harness-team 산출 커스텀 에이전트.
const TEAM = [
  { role: 'GPS/지도',    agentType: 'gps-map-engineer',    lens: 'GPS 수신·오프라인 지도·역지오코딩' },
  { role: '장치 통신',    agentType: 'pcs-modbus-engineer', lens: 'PCS(Modbus)/BMS(CAN) 통신·제어 안전' },
  { role: 'UI/아키텍처',  agentType: 'wpf-mvvm-engineer',   lens: 'WPF/MVVM 구조·바인딩·네비게이션' },
  { role: '검증/품질',    agentType: 'build-verify-qa',     lens: '빌드·기동·경계면 정합성·회귀' },
]
const WORKER_AGENT_TYPE   = null              // 기능별은 FEATURE_AGENT로 라우팅
const REVIEWER_AGENT_TYPE = 'build-verify-qa' // 통합 검토 담당

// 기능 → 담당 에이전트 해석(폴백 포함)
const agentTypeFor = (task) => FEATURE_AGENT[task.feature] || WORKER_AGENT_TYPE

const PLAN_SCHEMA = {
  type: 'object',
  properties: {
    size:   { type: 'string', enum: ['small','medium','large','complex'] },
    reason: { type: 'string' },
    tasks: { type: 'array', items: { type: 'object', properties: {
      name:         { type: 'string' },
      feature:      { type: 'string', enum: Object.keys(WORKTREE_MAP) },
      allowedFiles: { type: 'array', items: { type: 'string' } },
      blockedFiles: { type: 'array', items: { type: 'string' } },
      goal:         { type: 'string' },
    }}}
  }
}
const RESULT_SCHEMA = {
  type: 'object',
  properties: {
    name:          { type: 'string' },
    modifiedFiles: { type: 'array', items: { type: 'string' } },
    buildSuccess:  { type: 'boolean' },
    notes:         { type: 'string' },
  }
}
const REVIEW_SCHEMA = {
  type: 'object',
  properties: {
    findings: { type: 'array', items: { type: 'object', properties: {
      severity: { type: 'string', enum: ['CRITICAL', 'WARNING', 'INFO'] },
      item:     { type: 'string' },
      file:     { type: 'string' },
      line:     { type: 'integer' },
      detail:   { type: 'string' },
    }}},
    summary: { type: 'string' },
  }
}

phase('규모 분석')
const plan = await agent(
  `작업 규모와 위험도를 분석해줘.\n작업: ${JSON.stringify(args)}\n\n` +
  `기능(feature) 목록 — 각 작업을 이 중 하나에 배분해야 한다 (다른 값 금지):\n` +
  `${Object.keys(WORKTREE_MAP).map(f => `- ${f}`).join('\n')}\n\n` +
  `규모 기준:\n` +
  `- small:   단일 파일·공유 파일 미포함\n` +
  `- medium:  복수 파일·독립 worktree 분리 가능\n` +
  `- large:   공유 파일 포함·아키텍처 변경·회귀 위험\n` +
  `- complex: 설계 결정 필요·상충 요구사항·시스템 전반 영향`,
  { label: '규모 분석', schema: PLAN_SCHEMA }
)

const VALID_SIZES = ['small', 'medium', 'large', 'complex']
const resolveSize = (p) => {
  if (VALID_SIZES.includes(p.size)) return p.size
  const leaked = String(p.reason || '').match(/size"[^>]*>\s*(small|medium|large|complex)/i)
  if (leaked) return leaked[1].toLowerCase()
  const anyMention = String(p.reason || '').match(/\b(small|medium|large|complex)\b/i)
  if (anyMention) return anyMention[1].toLowerCase()
  throw new Error(`규모 분류 실패 — plan.size가 유효하지 않다: ${JSON.stringify(p)}`)
}
const size = resolveSize(plan)
log(`규모: [${size}] — ${plan.reason}`)

let results = [], review = null

// ── 헬퍼: 실행 프롬프트 ──────────────────────────────────────────
const mkTaskPrompt = (task, consensus = null) => {
  const dir = WORKTREE_MAP[task.feature]
  return `[${task.name}] ${task.goal}\n` +
    `작업 디렉토리(절대경로 — 반드시 이 안에서만 파일을 읽고 쓰고 커밋하라): ${dir}\n` +
    `이 폴더는 해당 feature 브랜치로 체크아웃된 worktree다. 다른 worktree/원본은 건드리지 마라.\n` +
    (consensus ? `합의된 구현 전략:\n${consensus}\n` : '') +
    `수정 허용: ${task.allowedFiles.join(', ')}\n수정 금지: ${task.blockedFiles.join(', ')}\n` +
    `빌드(위 작업 디렉토리에서): ${BUILD_CMD}\n` +
    `절차: 1. ${consensus ? '합의 전략 숙지 → ' : ''}작업 디렉토리 이동 → 2. 코드 분석 → 3. 구현 → 4. 빌드 → 5. 커밋`
}

// ── 헬퍼: 팬아웃/팬인 (기능별 도메인 에이전트로 라우팅) ───────────
const fanOut = (tasks, consensus = null) =>
  parallel(tasks.map(task => () =>
    agent(mkTaskPrompt(task, consensus), {
      label: task.name, phase: '실행', schema: RESULT_SCHEMA,
      ...(agentTypeFor(task) ? { agentType: agentTypeFor(task) } : {}),
    })
  ))

// ── 헬퍼: 심층 검증 (생성-검증) ──────────────────────────────────
const deepVerify = (res) =>
  agent(
    `[통합 검토] 다음 결과를 심층 검토해줘.\n결과: ${JSON.stringify(res.filter(Boolean))}\n` +
    `발견 문제는 findings 배열에 severity(CRITICAL/WARNING/INFO)·item·file·line·detail로, ` +
    `summary에 한 줄 총평. 문제 없으면 findings는 빈 배열 + summary에 승인 취지.`,
    {
      label: 'integration-reviewer', phase: '검토', schema: REVIEW_SCHEMA,
      ...(REVIEWER_AGENT_TYPE ? { agentType: REVIEWER_AGENT_TYPE } : {}),
    }
  )

// ── 전략 디스패치 ────────────────────────────────────────────────
const STRATEGY = {
  small: async () => {
    phase('실행'); log('소규모 → 순차 실행')
    for (const task of plan.tasks)
      results.push(await agent(mkTaskPrompt(task), {
        label: task.name, phase: '실행', schema: RESULT_SCHEMA,
        ...(agentTypeFor(task) ? { agentType: agentTypeFor(task) } : {}),
      }))
  },
  medium: async () => {
    phase('실행'); log('중규모 → 병렬 실행')
    results = await fanOut(plan.tasks)
    phase('검토'); log('중규모 → main 인라인 검토')
    review = await agent(
      `작업 결과를 간략히 검토해줘 (빌드·충돌 위주).\n결과: ${JSON.stringify(results.filter(Boolean))}`,
      { label: '검토(main)', phase: '검토' }
    )
  },
  large: async () => {
    phase('실행'); log('대규모 → 병렬 실행')
    results = await fanOut(plan.tasks)
    phase('검토'); log('대규모 → 통합 검토')
    review = await deepVerify(results)
  },
  complex: async () => {
    phase('팀 토론'); log('복잡한 작업 → Agent Team 토론')
    const opinions = await parallel(TEAM.map(m => () =>
      agent(
        `너는 ${m.role}야. ${m.lens} 관점에서 아래 작업을 분석하고 구현 방법을 제안해줘.\n` +
        `작업: ${JSON.stringify(plan.tasks)}\n포함: 위험·권장방법·다른관점과의충돌`,
        { label: m.role, phase: '팀 토론', ...(m.agentType ? { agentType: m.agentType } : {}) }
      )
    ))
    phase('합의'); log('토론 완료 → 합의 도출')
    const consensus = await agent(
      `${TEAM.length}개 관점을 종합해 최적 구현 전략을 합의해줘.\n` +
      `${opinions.filter(Boolean).map((o,i) => `[${TEAM[i].role}]\n${o}`).join('\n\n')}\n\n` +
      `합의: 1.구현방향 2.각관점제약 3.worktree배분(최종) 4.구현순서`,
      { label: '합의', phase: '합의' }
    )
    log('합의 완료 → 병렬 구현')
    phase('실행'); results = await fanOut(plan.tasks, consensus)
    phase('검토'); log('복잡한 작업 → 심층 통합 검토')
    review = await deepVerify(results)
  },
}

await STRATEGY[size]()

const succeeded = results.filter(Boolean).filter(r => r.buildSuccess !== false)
const failed    = results.filter(Boolean).filter(r => r.buildSuccess === false)
const criticalFindings = (review && Array.isArray(review.findings))
  ? review.findings.filter(f => f.severity === 'CRITICAL')
  : []

return {
  size,
  succeeded: succeeded.map(r => `✅ ${r.name}`),
  failed:    failed.map(r => `❌ ${r.name}`),
  review,
  mergeBlocked: criticalFindings.length > 0,
  criticalFindings,
}
