# PCS add BMS guard toggle to enable disable ApplyBmsGuardPolicyAsync for standalone testing

- Task ID: 20260615-131736-pcs-add-bms-guard-toggle-to-enable-disable-applybmsguardpolicyasync-for-standalone-testing
- Status: active
- Created: 2026-06-15 13:17:36

## Goal

PCS View에 BMS Guard 토글 버튼을 추가해 ApplyBmsGuardPolicyAsync를 런타임에 ON/OFF.
PCS 단독 테스트 시 코드 주석 없이 가드를 끌 수 있게 한다. 기본값 ON(안전).

## Scope

- 포함:
  - PcsViewModel: IsBmsGuardEnabled(bool) 추가, 생성자에서 기본 true.
  - ApplyBmsGuardPolicyAsync 진입부에 `if (!IsBmsGuardEnabled) return;` 게이트(전 모드 무력화).
  - 주석 처리된 가드 호출 3곳 해제(291 Monitor, 470 BeforeCharge, 482 BeforeDischarge).
  - PCSView.xaml: 헤더 액션버튼 옆에 ON/OFF 토글(ON=녹색, OFF=적색).
- 제외:
  - 가드 판정 로직 자체 변경 없음.
  - 토글 상태 영구저장(설정 persist)은 범위 밖.

## Impacted Files

- EMS_PJT_Hamburger/ViewModels/PcsViewModel.cs
- EMS_PJT_Hamburger/Views/PCSView.xaml

## Test Strategy

- Build: 별도 OutputPath 컴파일 검증.
- E2E(수동): 토글 ON 상태에서 BMS 미준비 시 충/방전 차단/자동정지, OFF면 가드 무시하고 명령 진행.

## Rollback

- 위 2개 파일을 변경 이전 상태로 되돌린다(가드 호출 재주석 포함).

## Notes

- 게이트는 ApplyBmsGuardPolicyAsync 단일 진입부에 둬서 Monitor/BeforeCharge/BeforeDischarge 모두 한 번에 제어.
- 기본 ON이라 현재(전부 주석=off) 대비 기본 동작이 바뀜: BMS 미준비 시 충/방전이 차단됨(토글 OFF로 우회).
