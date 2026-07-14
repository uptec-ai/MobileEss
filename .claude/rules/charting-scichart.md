---
paths:
  - "EMS_PJT_Hamburger/Views/HistoryView.xaml*"
  - "EMS_PJT_Hamburger/Views/DashBoardView.xaml*"
  - "EMS_PJT_Hamburger/ViewModels/HistoryViewModel.cs"
  - "EMS_PJT_Hamburger/ViewModels/DashBoardViewModel.cs"
---
# 차트(SciChart) 규칙

매칭 파일 편집 시에만 로드.

- **라이브러리**: SciChart 8.6. 런타임 라이선스는 `App.ConfigureSciChartLicense()`가 env `EMS_SCICHART_LICENSE_KEY`에서 주입. 소스에 키 금지.
- **바인딩 소스**: 외부 참조 `SciChart.Examples.ExternalDependencies`는 사용자 로컬 SDK 경로에 의존(csproj HintPath). 경로 하드코딩 주의.
- **대용량 데이터**: 이력 차트는 다량 포인트 → `System.Text.Json`으로 직렬화된 이력을 사용. UI 스레드 블로킹 피하고 필요 시 다운샘플.
- **리소스 해제**: 차트/렌더 표면과 관련 폴링은 뷰 종료 시 정리.
