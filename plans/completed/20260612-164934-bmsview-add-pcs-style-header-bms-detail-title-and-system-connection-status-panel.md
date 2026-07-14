# BMSView add PCS-style header BMS DETAIL title and system connection status panel

- Task ID: 20260612-164934-bmsview-add-pcs-style-header-bms-detail-title-and-system-connection-status-panel
- Status: active
- Created: 2026-06-12 16:49:34

## Goal

BMS View 상단에 PCS View(PCS DETAIL, line 252)와 같은 헤더를 추가한다.
- 좌측: "BMS DETAIL" 이름 + 부제 "Battery Management System".
- 우측: Last Update 시간 + System Connect 상태 패널(연결시 녹색/미연결시 적색 인디케이터).

## Scope

- 포함:
  - BmsDataModel: 헤더 바인딩용 표시 속성 추가
    (BmsConnectionStatus, IsBmsConnected, BmsTime) - hidden PropertyChanged(OnPropertyChanged)로 알림.
    StatusMessage(0x150) 수신 시 BmsTime 갱신.
  - BMSViewModel: UpdateBmsConnectionStatus에서 연결상태 문자열/플래그 반영 + BmsStatusText 헬퍼.
  - BMSView.xaml: 최상단 Row(0 -> 64)에 헤더 Grid 추가(타이틀 + 연결상태 패널).
- 제외:
  - PCS 전용 액션 버튼(Charge/Discharge 등) 추가하지 않음.
  - 기존 카드/Xpack/Control 패널 변경 없음.

## Impacted Files

- EMS_PJT_Hamburger/Models/BmsDataModel.cs
- EMS_PJT_Hamburger/ViewModels/BMSViewModel.cs
- EMS_PJT_Hamburger/Views/BMSView.xaml

## Test Strategy

- Build: 별도 OutputPath 컴파일 검증.
- E2E(수동): 헤더 타이틀 표시, 연결시 녹색+"Connected", 미연결시 적색, BmsTime 갱신.

## Rollback

- 위 3개 파일을 변경 이전 상태로 되돌린다.

## Notes

- BmsDataModel은 PropertyChanged를 재선언(hidden)하므로 SetProperty(base) 대신
  OnPropertyChanged(hidden)로 알려야 OneWay 표시 바인딩이 갱신됨.
