# Move BMS guard toggle to HomeView match touch keyboard switch style and arrange toggles

- Task ID: 20260615-132717-move-bms-guard-toggle-to-homeview-match-touch-keyboard-switch-style-and-arrange-toggles
- Status: active
- Created: 2026-06-15 13:27:17

## Goal

BMS Guard 토글을 PCS View에서 HomeView로 이동. Touch Keyboard 스위치와 동일 스타일
(TouchKeyboardToggleStyle iOS 스위치)로 통일하고, 두 토글(Touch KB, BMS Guard)을
상단 토글 보더에 라벨+스위치 세로 그룹으로 나란히 이쁘게 배치.

## Scope

- 포함:
  - PCSView.xaml: BmsGuardToggleStyle 리소스 + BMS Guard ToggleButton 제거.
  - HomeView.xaml: Touch Keyboard 보더(220x46) 내용을 2개 토글 그룹으로 재구성.
    BMS Guard는 PcsVm.IsBmsGuardEnabled(Source=Application.Current) TwoWay 바인딩.
- 제외:
  - PcsViewModel 로직/IsBmsGuardEnabled 동작 변경 없음(바인딩 위치만 이동).
  - 가드 판정 로직 변경 없음.

## Impacted Files

- EMS_PJT_Hamburger/Views/PCSView.xaml
- EMS_PJT_Hamburger/Views/HomeView.xaml

## Test Strategy

- Build: 별도 OutputPath 컴파일 검증.
- E2E(수동): HomeView 상단에 Touch KB/BMS Guard 스위치 2개 표시, BMS Guard 토글이
  PcsVm.IsBmsGuardEnabled를 ON/OFF, PCS View엔 토글 없음.

## Rollback

- 두 파일을 변경 이전 상태로 되돌린다.

## Notes

- 두 스위치 모두 TouchKeyboardToggleStyle 재사용(시각 통일).
- 220 보더 내 세로 그룹 2개(스위치 65 x2 + 간격) = ~146 < 220, 오버플로 없음.
