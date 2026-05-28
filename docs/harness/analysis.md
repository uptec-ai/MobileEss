# Harness Analysis

분석 대상: `C:\Project\2. ESS\EMS_PJT v1.4\EMS_PJT_Hamburger`  
기준일: 2026-05-20

## 요약

현재 솔루션은 Visual Studio 2019 형식의 WPF/.NET Framework 4.8 솔루션이다. 주 애플리케이션은 `EMS_PJT_Hamburger`이고, 보조 프로젝트로 배포 설치용 WPF 프로젝트 `EMS_PJT_DeploymentInstaller`가 있다. 솔루션은 SDK 스타일이 아니며 `packages.config`, 명시 DLL 참조, Fody import, DevExpress/SciChart 상용 라이브러리에 의존한다.

## 프로젝트 의존성

- `EMS_PJT_Hamburger.sln`
  - `EMS_PJT_Hamburger/EMS_PJT_Hamburger.csproj`
  - `EMS_PJT_DeploymentInstaller/EMS_PJT_DeploymentInstaller.csproj`
- Target Framework: `.NET Framework 4.8`
- UI: WPF
- DevExpress: `v23.1.5` 계열 참조
- SciChart: `8.6.0.28199`
- 통신/장비: `NModbus4`, `Peak.PCANBasic.NET`
- DB: `Npgsql 8.0.3`
- Logging: `NLog 5.4.0`
- MVVM/INPC: `DevExpress.Mvvm`, `PropertyChanged.Fody`

## WPF/MVVM 구조

- `App.xaml.cs`가 앱 전역 객체를 직접 생성하고 보관한다.
- View는 `Views/*View.xaml`에 있고 ViewModel은 `ViewModels/*ViewModel.cs`에 있다.
- 일부 모델 클래스가 `ViewModelBase`, `BindableBase`, `INotifyPropertyChanged`를 직접 사용한다.
- DI 컨테이너는 없으며 `Application.Current`와 App 싱글턴 성격의 전역 상태 접근이 많다.
- ViewModel 명령은 DevExpress `DelegateCommand`와 `ICommand`로 구성된다.
- 장비 연결과 폴링은 ViewModel/Model에서 직접 시작된다.

## DevExpress 사용 패턴

- MainWindow는 `dx:ThemedWindow`를 사용한다.
- 버튼은 `dx:SimpleButton`, 이미지는 `dx:DXImage`, Grid는 `dxg:GridControl/TableView`를 사용한다.
- Splash는 `SplashScreenManager`, `DXSplashScreenViewModel` 기반이다.
- MVVM은 `DevExpress.Mvvm.ViewModelBase`, `BindableBase`, `DelegateCommand` 중심이다.

## SciChart 사용 패턴

- `App.xaml`에서 SciChart 예제 테마 리소스를 병합한다.
- 런타임 라이선스 키는 `EMS_SCICHART_LICENSE_KEY` 환경변수에서 읽는다.
- `DashBoardView.xaml`에 `SciChartSurface`, `FastMountainRenderableSeries`, `DateTimeAxis`, `NumericAxis`가 있다.
- `PcsModel`에서 `XyDataSeries<DateTime, double>`를 생성하고 `Append`로 데이터를 추가한다.

## 데이터 접근 구조

- `Models/Managers/DbManager.cs`가 DB 접근을 집중 관리한다.
- 연결 문자열 우선순위는 환경변수 `EMS_DB_CONN`, `App.config`의 `connectionStrings:EMS_DB`이다.
- PostgreSQL/Npgsql을 사용한다.
- SQL 값은 파라미터 바인딩을 우선하고, 테이블명은 화이트리스트를 사용한다.

## 테스트 구조

- 현재 솔루션 안에 테스트 프로젝트가 없다.
- Harness는 테스트 프로젝트가 없을 때 품질 게이트를 경고 통과로 처리하되, 테스트 프로젝트가 생기면 자동 실행하도록 설계한다.

## 빌드 방식

- SDK 스타일이 아닌 legacy `.csproj`이다.
- `packages.config`와 `packages/` 폴더의 DLL HintPath에 의존한다.
- `MSBuild.exe` 기반 빌드가 적합하다.
- 솔루션 구성은 `Debug|Any CPU`, `Release|Any CPU`이다.
- Harness 빌드는 기본적으로 `Release|Any CPU`로 실행한다.

## 기존 코드 스타일

- block-scoped namespace를 사용한다.
- `#region`을 사용한다.
- private field는 `_camelCase`, public property/method는 `PascalCase`가 많다.
- 명령 속성은 `Cmd_` prefix가 쓰인다.
- XAML과 코드에 한국어 주석이 많다.

## 네이밍 규칙

- View: `*View.xaml`, `*Window.xaml`
- ViewModel: `*ViewModel.cs`
- Manager: `*Manager.cs`
- 모델/스펙: `*Model.cs`, `*Specs.cs`, `*FieldSpec.cs`
- 장비 클라이언트 구조: `Models/Client/BMS`, `Models/Client/PCS`
- 명령: `Cmd_*`

## 공통 레이어 구조

- `Models/Managers`: 앱 상태, 변환, DB, credential, animation 등 공통 성격
- `Models/Client/BMS`: CAN/PCAN, BMS 데이터 해석
- `Models/Client/PCS`: Modbus, PCS 데이터 해석/제어
- `ViewModels`: 화면별 상태와 명령
- `Views`: XAML UI
- `Assets`, `Fonts`: WPF Resource

## 기술 부채

- 테스트 프로젝트가 없다.
- DB, 통신, UI 스레드, 장비 제어 코드가 ViewModel/Model에 강하게 결합되어 있다.
- 일부 예외가 삼켜져 품질 게이트에서 런타임 실패를 잡기 어렵다.
- 인코딩 깨짐 문자열이 일부 보인다.
- DevExpress와 SciChart 버전이 프로젝트 파일에 직접 묶여 있어 라이선스/참조 경로가 빌드 환경에 민감하다.

## Harness 반영 결정

- 품질 게이트는 `start-task.ps1`로 만든 `current-task.json`, 계획 파일, 로그 파일이 없으면 실행하지 않는다.
- 앱 코드 수정 전 `guard-before-edit.ps1` 통과를 필수로 한다.
- 테스트 프로젝트가 없는 현재 상태는 경고 통과로 처리하되, 테스트 프로젝트가 추가되면 자동 실행한다.
- 빌드는 `MSBuild.exe` 탐색 후 `EMS_PJT_Hamburger.sln /p:Configuration=Release /p:Platform="Any CPU"`로 수행한다.
- SciChart 라이선스는 E2E/런타임 검증에서 환경변수 존재를 확인하되, 로컬 빌드 자체를 막지는 않는다.
- DB 통합 검증은 `EMS_RUN_DB_INTEGRATION=1`일 때만 `EMS_DB_CONN`을 요구한다.
