# 패치 노트 (추가/삭제 + 이유)

이 문서는 주요 추가/삭제/변경 사항과 적용 이유를 정리한 문서입니다.

## 1) `App.config` ■

- 추가: `appSettings -> PcsHost`, `PcsPort`, `PcsTimeoutMs`
  - 이유: ViewModel 생성자의 고정 네트워크 값을 제거하고 배포 환경 변경을 안전하게 만들기 위해서입니다.

## 2) `App.xaml.cs` ■

- 삭제: 하드코딩된 `SciChartSurface.SetRuntimeLicenseKey("...")`
  - 이유: 소스 이력/바이너리에서 비밀값이 노출될 위험이 있기 때문입니다.
- 추가: `ConfigureSciChartLicense()`
  - 이유: `EMS_SCICHART_LICENSE_KEY` 환경변수를 우선 사용하고, 없으면 config를 사용하도록 하여 운영 유연성과 보안을 확보하기 위해서입니다.
- 추가: `OnExit`에서 `DisposeViewDataContext(...)` 호출
  - 이유: 앱 종료 시 ViewModel 리소스를 명시적으로 정리하기 위해서입니다.
- 추가: `OnExit`에서 `StatusManager?.Dispose()`
  - 이유: 타이머/이벤트 참조가 프로세스 종료까지 남는 것을 방지하기 위해서입니다.
- 추가: `LogManager.Shutdown()`
  - 이유: 종료 시 NLog 파일 핸들을 안전하게 flush/해제하기 위해서입니다.

## 3) `Models/Managers/DbManager.cs` ■

- 삭제: 하드코딩 DB 연결 문자열(`HOST/PORT/USERNAME/PASSWORD`)
  - 이유: 자격증명 노출 위험 및 환경 종속성을 제거하기 위해서입니다.
- 추가: `EMS_DB_CONN` 환경변수 또는 `ConnectionStrings["EMS_DB"]` 로딩
  - 이유: 환경별 안전한 설정 주입 전략을 적용하기 위해서입니다.
- 추가: `CreateConnection()`
  - 이유: 연결 생성/검증을 중앙화하고 설정 누락 시 즉시 원인을 확인할 수 있게 하기 위해서입니다.
- 추가: `ExecuteNonQuery(sql, bindParameters)`, `GetDataSetByQuery(sql, bindParameters)` 오버로드
  - 이유: 파라미터 SQL을 쉽게 사용해 SQL 인젝션과 형변환 오류를 줄이기 위해서입니다.
- 변경: 문자열 보간 INSERT/SELECT -> 파라미터 명령
  - 이유: SQL 안전성과 데이터 무결성 향상을 위해서입니다.
- 추가: `TruncateWhitelist`
  - 이유: 동적 테이블 작업을 제한해 실수/오남용으로 인한 파괴적 동작 위험을 낮추기 위해서입니다.
- 추가: `ToInt(...)`, `ToDouble(...)`
  - 이유: 문자열 기반 수신값 파싱을 안전하게 처리하기 위해서입니다.

## 4) `Models/Managers/StatusManager.cs` ■

- 추가: `IDisposable` 구현
  - 이유: 장시간 동작하는 WPF 앱에서 타이머/이벤트를 명시적으로 정리하기 위해서입니다.
- 변경: 인라인 타이머 람다 -> 명명된 핸들러 `DtTimer_Tick`
  - 이유: `-=` 구독 해제를 명확히 수행하기 위해서입니다.
- 추가: `Init()`에서 재구독 전에 `PropertyChanged -= ...`
  - 이유: 초기화 재호출 시 중복 핸들러가 쌓이는 것을 방지하기 위해서입니다.
- 추가: `Dispose()`에서 타이머 정지 및 이벤트 해제
  - 이유: 이벤트/타이머 참조로 인한 메모리 유지(누수성 유지)를 막기 위해서입니다.

## 5) `Models/Client/BMS/PcanRxService.cs` ■

- 삭제: `Dispose()`의 `NotImplementedException`
  - 이유: 종료 경로에서 예외로 앱 안정성이 깨질 수 있기 때문입니다.
- 추가: dispose 가드(`_disposed`)와 `ThrowIfDisposed()`
  - 이유: 해제된 객체 재사용을 방지하기 위해서입니다.
- 추가: 제한 시간 있는 스레드 조인(`Join(1000)`) 및 정리
  - 이유: 종료 시 무기한 블로킹을 피하기 위해서입니다.
- 추가: dispose 시 `FrameReceived = null`
  - 이유: 구독자 참조를 해제해 누수 위험을 줄이기 위해서입니다.

## 6) `ViewModels/BMSViewModel.cs` ■

- 추가: `IDisposable` 및 `_disposed` 가드
  - 이유: 타이머/이벤트/CAN 서비스 정리를 명시적으로 수행하기 위해서입니다.
- 변경: 고빈도 RX 로그 `Info -> Debug`
  - 이유: 로그량과 운영 데이터 노출을 줄이기 위해서입니다.
- 추가: 명시적 구독 해제(`StatusMsg02.PropertyChanged`, `_rx.FrameReceived`, timer tick)
  - 이유: 고아 핸들러 및 메모리 누수 가능성을 낮추기 위해서입니다.
- 추가: `Dispose`에서 알람 창/서비스('AlarmService', '_alarmWin') 정리
  - 이유: 일시 UI/서비스 리소스를 종료 시점에 확실히 해제하기 위해서입니다.

## 7) `ViewModels/HomeViewModel.cs` ■

- 추가: `IDisposable` 지원
  - 이유: ViewModel 생명주기에 맞춘 취소/정리를 보장하기 위해서입니다.
- 변경: `StopLoop()`에서 `_loopCts`를 `Cancel + Dispose + null`
  - 이유: 토큰 소스 리소스 누수를 방지하기 위해서입니다.
- 추가: 루프 내 `OperationCanceledException` 처리
  - 이유: 취소를 오류가 아닌 정상 흐름으로 처리하기 위해서입니다.

## 8) `Models/Client/PCS/PcsModel.cs` ■

- 추가: 폴링/업데이트 흐름에 `_cts` null/취소 가드
  - 이유: 연결/해제 전환 시 경합 상태에서 null 접근을 막기 위해서입니다.
- 변경: `ChangeInfomation(...)` 호출을 `ChangeInformation(...)`으로 정리
  - 이유: 메서드명 불일치로 인한 경로 오류를 바로잡고 실행 일관성을 확보하기 위해서입니다.
- 변경: `StopPolling()`에서 토큰 소스 dispose
  - 이유: 취소 관련 리소스를 정상 해제하기 위해서입니다.
- 추가: `DisposeModelResources()`
  - 이유: 파생 ViewModel이 공통 종료 경로를 재사용할 수 있게 하기 위해서입니다.

## 9) `ViewModels/PcsViewModel.cs` ■

- 삭제: 생성자 고정 연결 설정값
  - 이유: 환경 종속성과 설정 드리프트 위험을 줄이기 위해서입니다.
- 추가: config 기반 endpoint 로딩(`PcsHost/Port/Timeout`)
  - 이유: 배포 환경에서 안전하게 설정을 바꿀 수 있게 하기 위해서입니다.
- 추가: 중복 연결 가드 + 중복 이벤트 구독 방지
  - 이유: 재연결 시 폴링 루프/핸들러 중복 생성 문제를 막기 위해서입니다.
- 추가: `Dispose()`에서 구독 해제 + 모델 리소스 정리
  - 이유: 네트워크/폴링 리소스를 결정적으로 정리하기 위해서입니다.

## 10) `Models/Client/PCS/ModbusService.cs` ■

- 추가: `IDisposable`, `_disposed`, `ThrowIfDisposed()`
  - 이유: 객체 생명주기를 명확히 하고 잘못된 상태 접근을 방지하기 위해서입니다.
- 변경: `ReadInputRegistersAsync`에서 불필요한 `Task.Run` 제거
  - 이유: 불필요한 스레드 오프로딩을 줄이고 취소 흐름을 단순화하기 위해서입니다.
- 추가: 연결/해제 시 `ConnectionStateChanged?.Invoke(true/false)`
  - 이유: 연결 상태 알림 일관성을 유지하기 위해서입니다.
- 추가: dispose 시 이벤트 필드 null 처리
  - 이유: 구독자 참조를 해제해 객체 그래프 잔존 가능성을 낮추기 위해서입니다.

## 11) `Models/Client/ModbusTcpConnectionService.cs`

- 변경: dispose 경로에서 `StopAsync` 완료까지 대기
  - 이유: 백그라운드 루프/소켓 정리가 완료된 뒤 객체가 종료되도록 보장하기 위해서입니다.
- 추가: dispose 시 이벤트 null 처리
  - 이유: 종료 후 남아있는 이벤트 참조를 제거하기 위해서입니다.

## 12) `Models/BmsDataModel.cs`

- 변경: 고빈도 상태 로그 `Info -> Debug`
  - 이유: 데이터 노출 및 로그 I/O 부담을 줄이기 위해서입니다.
- 변경: TX 실패 로그 `Info -> Warn`
  - 이유: 장애성 이벤트의 심각도를 더 정확히 표현하기 위해서입니다.
- 수정: `CanIdToPackNo` 범위 조건 (`&&` -> `||`)
  - 이유: 범위 밖 판정 버그를 수정하기 위해서입니다.

## 13) `NLog.config`

- 추가: 롤링 아카이브 옵션(`archiveFileName`, `archiveAboveSize`, `maxArchiveFiles`)
  - 이유: 로그 파일 무한 증가를 방지하기 위해서입니다.
- 추가: `keepFileOpen=false`, `concurrentWrites=true`
  - 이유: 파일 잠금 이슈를 줄이고 런타임 안정성을 높이기 위해서입니다.
- 유지: 운영 규칙 최소 레벨(Info/Warn/Error/Fatal)
  - 이유: 운영 가시성은 유지하면서 과도한 로그 누적은 억제하기 위해서입니다.

## 14) `EasyModbus 제거 + NModbus RunLoop 전환` ■

- 삭제: `Models/Client/ModbusTcpConnectionService.cs`
  - 이유: EasyModbus 기반 통신 경로를 완전히 제거해 라이브러리 혼용으로 인한 유지보수 복잡도를 줄이기 위해서입니다.
- 삭제: `EMS_PJT_Hamburger.csproj`의 `EasyModbus` 참조, `packages.config`의 `EasyModbusTCP` 패키지
  - 이유: 실제 미사용 패키지를 정리해 빌드/배포 산출물과 의존성 관리 리스크를 줄이기 위해서입니다.
- 변경: `Models/Client/PCS/ModbusService.cs`
  - 추가: `Configure -> StartAsync -> RunLoopAsync -> StopAsync` 흐름
    - 이유: 상시 연결, 자동 재접속(백오프), 주기 폴링을 서비스 단에서 일관되게 관리하기 위해서입니다.
  - 추가: `InputRegistersReceived` 이벤트
    - 이유: 폴링 수신 데이터를 ViewModel로 푸시해 UI 갱신 경로를 단순화하기 위해서입니다.
  - 추가: 레지스터 길이 검증(`expected == actual`)
    - 이유: 응답 프레임 길이 이상을 조기에 감지해 데이터 신뢰성을 높이기 위해서입니다.
  - 추가: `StopAsync()` + `Dispose()` 정리 강화
    - 이유: 종료 시 백그라운드 루프/소켓/이벤트를 명시적으로 해제해 누수 위험을 낮추기 위해서입니다.
- 변경: `Models/Client/PCS/PcsModel.cs`
  - 변경: `_cts` 기반 자체 루프 제거, 서비스 이벤트(`OnInputRegistersReceived`) 기반 파싱으로 전환
    - 이유: 중복 루프를 제거해 연결/재접속 책임을 `ModbusService`에 집중하기 위해서입니다.
  - 추가: 수신 프레임 주소/길이 검증
    - 이유: 잘못된 프레임의 UI 반영을 방지하기 위해서입니다.
- 변경: `ViewModels/PcsViewModel.cs`
  - 변경: 연결 로직을 `Configure + StartAsync` 패턴으로 전환
    - 이유: `ModbusService`의 표준 생명주기 흐름을 직접 사용해 코드 경로를 단순화하기 위해서입니다.
  - 추가: `InputRegistersReceived` 이벤트 구독/해제
    - 이유: 데이터 수신과 해제 시점을 명확히 해 이벤트 누수를 방지하기 위해서입니다.
- 변경: `App.xaml.cs`
  - 삭제: EasyModbus 관련 using, `_pcsClient` 필드/초기화/종료 코드
    - 이유: 제거된 EasyModbus 경로를 앱 수명주기에서 완전히 분리하기 위해서입니다.

## 15) `PCS keep-alive 동작 추가` ■

- 변경: `ModbusService.cs`
  - 추가: `ReadHoldingRegistersAsync(...)`
    - 이유: keep-alive 검증을 입력 레지스터 폴링과 분리해 통신 생존 확인을 명확히 하기 위해서입니다.
  - 추가: `Configure(...)`에 keep-alive 설정값(`keepAliveStartAddress`, `keepAliveCount`, `keepAliveInterval`)
    - 이유: 설비별 keep-alive 주소/주기를 코드 수정 없이 조정할 수 있게 하기 위해서입니다.
  - 추가: `RunLoopAsync(...)`에서 주기적 keep-alive 읽기 + `KeepAliveHoldingReceived` 이벤트 발생
    - 이유: 실제 keep-alive 프레임을 기반으로 연결 생존성을 지속 감시하기 위해서입니다.
- 변경: `PcsViewModel.cs`
  - 변경: `OnKeepAliveHoldingReceived(...)`에서 keep-alive 수신 시점 기준으로 `IsReceive`, `Conn_State.Rtt`, `KeepAliveRegisters` 갱신
    - 이유: keep-alive 결과가 UI 상태/진단 정보에 즉시 반영되도록 하기 위해서입니다.
  - 변경: `ConnectAsync()`에서 keep-alive 이벤트 구독 및 keep-alive 설정값 전달
    - 이유: 통신 시작 시 keep-alive 기능이 항상 활성화되도록 보장하기 위해서입니다.
- 변경: `PcsModel.cs`
  - 변경: 연결 해제 시 `Conn_State.Rtt` 초기화
    - 이유: 단절 상태에서 마지막 keep-alive RTT가 남아 오해를 주는 것을 방지하기 위해서입니다.

## 16) `연결 상태 콜백 안정화` ■

- 변경: `PcsModel.cs`의 `OnConnectionChanged(...)`
  - 변경: UI 업데이트 블록에 예외 보호(`try/catch`) 및 `app?.StatusManager` null 가드 추가
    - 이유: 디스패처 콜백 내부 예외로 상태 전이 흐름이 흔들리는 문제를 방지하기 위해서입니다.
  - 변경: 상태 문자열 `"Connected."/"Disconnected."` -> `"Connected"/"Disconnected"` 통일
    - 이유: `ConnectionState.IsConnected => Status == "Connected"` 조건과 불일치로 발생하는 오판정을 제거하기 위해서입니다.
- 변경: `PcsViewModel.cs`
  - 변경: 연결 실패 시 상태 문자열을 `"Disconnected"`로 통일
    - 이유: 상태 판정 문자열 규칙을 일관되게 유지하기 위해서입니다.
