# WPF Rules

## View와 ViewModel

- View는 `Views`, ViewModel은 `ViewModels` 아래에 둔다.
- 기존 화면 연결 방식인 `App.xaml.cs`의 명시적 View/ViewModel 생성과 DataContext 주입을 유지한다.
- 새 명령은 기존 패턴에 맞춰 `DelegateCommand` 또는 `ICommand`로 노출한다.
- 명령 이름은 기존 코드와 맞춰 `Cmd_` prefix를 사용한다.

## DevExpress

- 현재 프로젝트 기준 버전은 `v23.1.5`이다.
- `dx:ThemedWindow`, `dx:SimpleButton`, `dxg:GridControl`, `DXImage` 사용 패턴을 따른다.
- 테마 변경은 전역 리소스와 개별 View 영향을 함께 확인한다.

## SciChart

- 라이선스 키는 `EMS_SCICHART_LICENSE_KEY` 환경변수를 사용한다.
- 차트 리소스는 `App.xaml` 병합 리소스를 기준으로 한다.
- `XyDataSeries<DateTime, double>` 변경 시 데이터 Append 위치와 UI 바인딩 영향을 같이 검토한다.

## Threading

- UI 업데이트는 WPF Dispatcher 컨텍스트를 고려한다.
- 장비 폴링 루프는 CancellationToken과 Dispose 경로를 확인한다.
- 생성자 fire-and-forget 변경은 예외 처리와 종료 흐름을 계획에 남긴다.
