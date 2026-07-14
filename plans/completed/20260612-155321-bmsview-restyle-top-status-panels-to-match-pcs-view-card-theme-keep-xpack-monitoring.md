# BMSView restyle top status panels to match PCS View card theme keep Xpack monitoring

- Task ID: 20260612-155321-bmsview-restyle-top-status-panels-to-match-pcs-view-card-theme-keep-xpack-monitoring
- Status: active
- Created: 2026-06-12 15:53:21

## Goal

BMSView 상단 패널(Row1: StatusMsg01, Row2: StatusMsg03/04)을 PCS View의
카드 테마/구조(PanelBorderStyle 다크 라운드 카드 + 작은 라벨 + 큰 흰색 값)로 변경한다.
Xpack monitoring(ItemsControl, Packs 4x5)과 Control AutoHideGroup은 고정(미변경).

## Scope

- 포함:
  - UserControl.Resources에 PCS 테마 카드 스타일 추가
    (PanelBorderStyle, CardLabelStyle, CardValueStyle).
  - Row1/Row2의 MainBorderStyle 단일 박스 -> 6열 UniformGrid 개별 카드로 교체.
  - 모든 데이터 바인딩(StatusMsg01/03/04, Packs ready-flag indicator) 유지.
- 제외:
  - Xpack monitoring ItemsControl(라인 324 영역) 변경 금지.
  - Control(AutoHideGroup) 패널 변경 금지.
  - ViewModel/Model/바인딩 경로 변경 없음.

## Impacted Files

- EMS_PJT_Hamburger/Views/BMSView.xaml

## Test Strategy

- Build: 별도 OutputPath 컴파일 검증.
- E2E(수동): 상단 카드가 PCS 카드 테마로 표시, 값/Pack indicator 정상 바인딩, Xpack 영역 동일.

## Rollback

- BMSView.xaml을 변경 이전 상태로 되돌린다.

## Notes

- SOH 카드는 원본과 동일하게 DispSOC를 바인딩(원본 동작 보존, 별도 수정 아님).
