using DevExpress.Mvvm;
using EMS_PJT_Hamburger.Behaviors;
using EMS_PJT_Hamburger.Models;
using EMS_PJT_Hamburger.Models.Client.GPS;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace EMS_PJT_Hamburger.ViewModels
{
    
    public class HomeViewModel : HomeModel, IDisposable
    {
        private bool _disposed;

        // ─── GPS (BU-353N) ────────────────────────────────────────────────
        private GpsService _gpsService;
        private NmeaParser _gpsParser;
        private readonly object _gpsSyncRoot = new object();
        private CancellationTokenSource _gpsReconnectCts;
        private Task _gpsReconnectTask;
        private string _gpsPort;
        private int _gpsBaud;
        private DateTime _lastGpsSentenceUtc = DateTime.MinValue;

        private const int GpsReconnectDelayMs = 3000;
        // 연결 실패/포트 부재가 반복되면 재시도 간격을 3s→6s→10s(상한)로 늘려
        // GPS 미장착 PC에서의 상시 예외·폴링 비용을 줄인다. 성공 시 3s로 리셋.
        private const int GpsReconnectDelayMaxMs = 10000;
        private int _gpsReconnectCurrentDelayMs = GpsReconnectDelayMs;
        private const int GpsReceiveTimeoutSeconds = 10;

        // 좌표 → 시/도/군(한글) 오프라인 조회 (KOSTAT GeoJSON point-in-polygon). 미로드/미매칭 시 "--" 유지.
        private readonly IReverseGeocoder _geocoder = new KoreaRegionLookup();
        private double _lastGeocodedLat, _lastGeocodedLng;
        private volatile bool _hasGeocodeAnchor;
        private volatile bool _geocodeInFlight;
        // 이 거리(m) 이상 이동했을 때만 역지오코딩 재조회(불필요한 조회 억제)
        private const double GeocodeMoveThresholdM = 60.0;

        // 표시 속성: HomeModel이 PropertyChanged를 재선언(hidden)하므로 OnPropertyChanged로 알림.
        private string _gpsLatitude = "--";
        public string GpsLatitude
        {
            get => _gpsLatitude;
            set { _gpsLatitude = value; OnPropertyChanged(nameof(GpsLatitude)); }
        }

        private string _gpsLongitude = "--";
        public string GpsLongitude
        {
            get => _gpsLongitude;
            set { _gpsLongitude = value; OnPropertyChanged(nameof(GpsLongitude)); }
        }

        private string _gpsFixStatus = "No Fix";
        public string GpsFixStatus
        {
            get => _gpsFixStatus;
            set { _gpsFixStatus = value; OnPropertyChanged(nameof(GpsFixStatus)); }
        }

        private string _gpsSatelliteCount = "0";
        public string GpsSatelliteCount
        {
            get => _gpsSatelliteCount;
            set { _gpsSatelliteCount = value; OnPropertyChanged(nameof(GpsSatelliteCount)); }
        }

        // GPS 패널/지도 중앙 마커에 표시할 시/도/군(한글). 유효 fix + 조회 성공 시 갱신, 실패 시 "--".
        private string _gpsRegion = "--";
        public string GpsRegion
        {
            get => _gpsRegion;
            set { _gpsRegion = value; OnPropertyChanged(nameof(GpsRegion)); GpsRegionChanged?.Invoke(value); }
        }

        private bool _gpsIsValid;
        public bool GpsIsValid
        {
            get => _gpsIsValid;
            set { _gpsIsValid = value; OnPropertyChanged(nameof(GpsIsValid)); }
        }

        // ─── 지도(DevExpress MapControl) follow ───────────────────────────
        // GPS 미수신 시 초기 중심(익산 근방). 유효 fix가 오면 해당 좌표로 follow.
        // (설치 사이트 위치에 맞게 아래 상수를 바꾸면 첫 fix 전 표시 위치가 바뀐다.)
        public const double SeoulLat = 37.566611, SeoulLng = 126.978211;
        public const double DongtanLat = 37.207580, DongtanLng = 127.097743;
        public const double GongjuLat = 36.467618, GongjuLng = 127.130815;
        public const double DefaultCenterLat = DongtanLat;
        public const double DefaultCenterLng = DongtanLng;

        // 지도 중심(위/경도). XAML MapControl.CenterPoint 가 이 값에 바인딩되어 follow 한다.
        // HomeModel 이 PropertyChanged 를 재선언(hidden)하므로 OnPropertyChanged 로 알림.
        private double _mapCenterLatitude = DefaultCenterLat;
        public double MapCenterLatitude
        {
            get => _mapCenterLatitude;
            set { _mapCenterLatitude = value; OnPropertyChanged(nameof(MapCenterLatitude)); }
        }

        private double _mapCenterLongitude = DefaultCenterLng;
        public double MapCenterLongitude
        {
            get => _mapCenterLongitude;
            set { _mapCenterLongitude = value; OnPropertyChanged(nameof(MapCenterLongitude)); }
        }

        /// <summary>유효 GPS fix 갱신 시 (위도, 경도)를 UI 스레드에서 통지(지도 마커 위치 갱신용).</summary>
        public event Action<double, double> GpsPositionChanged;

        /// <summary>시/도/군(GpsRegion) 변경 시 통지(지도 중앙 마커 라벨 갱신용). UI 스레드에서 발생.</summary>
        public event Action<string> GpsRegionChanged;

        public DelegateCommand<LoadStatus> Cmd_SelectLoadTarget { get; private set; }
        public bool IsTouchKeyboardEnabled
        {
            get => GetProperty(() => IsTouchKeyboardEnabled);
            set
            {
                if (SetProperty(() => IsTouchKeyboardEnabled, value))
                {
                    TouchKeyboardService.SetEnabled(value);
                }
            }
        }
        public HomeViewModel()
        {
            TouchKeyboardService.SetEnabled(false);
            Cmd_SelectLoadTarget = new DelegateCommand<LoadStatus>(SelectLoadTarget);
            StartLoop();
            ResetGpsDisplay();
            StartGps();

            // 기본 중심(첫 fix 전)의 시/도/군을 미리 조회 → 마커/패널에 즉시 표시.
            UpdateRegionLabel(MapCenterLatitude, MapCenterLongitude);
        }

        // ─── GPS 연결/수신 ───────────────────────────────────────────────
        // App.config의 GpsPort가 설정된 경우에만 연결한다(BU-353N 확인 설정: 4800 8N1).
        private void StartGps()
        {
            var port = ConfigurationManager.AppSettings["GpsPort"];
            if (string.IsNullOrWhiteSpace(port))
                return;

            _gpsPort = port.Trim();
            if (!int.TryParse(ConfigurationManager.AppSettings["GpsBaud"], out _gpsBaud) || _gpsBaud <= 0)
                _gpsBaud = 4800;

            _gpsReconnectCts = new CancellationTokenSource();
            _gpsReconnectTask = MonitorGpsConnectionAsync(_gpsReconnectCts.Token);
        }

        private async Task MonitorGpsConnectionAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested && !_disposed)
            {
                try
                {
                    if (!IsGpsConnectionHealthy())
                    {
                        ResetGpsConnection();

                        // 설정된 포트(GpsPort)가 시스템에 존재할 때만 Open을 시도한다.
                        // 포트 자체가 없는 PC(GPS 미장착)에서는 예외를 만들지 않고
                        // 백오프 간격으로 포트 출현만 재확인한다.
                        if (!IsGpsPortPresent())
                        {
                            ResetGpsDisplay("GPS Port Not Found");
                            IncreaseGpsReconnectDelay();
                        }
                        else
                        {
                            ResetGpsDisplay("GPS Reconnecting");
                            await ConnectGpsAsync(cancellationToken);
                            _gpsReconnectCurrentDelayMs = GpsReconnectDelayMs;
                        }
                    }
                    else
                    {
                        _gpsReconnectCurrentDelayMs = GpsReconnectDelayMs;
                    }

                    await Task.Delay(_gpsReconnectCurrentDelayMs, cancellationToken);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch
                {
                    ResetGpsConnection();
                    ResetGpsDisplay("GPS Disconnected");
                    IncreaseGpsReconnectDelay();

                    try
                    {
                        await Task.Delay(_gpsReconnectCurrentDelayMs, cancellationToken);
                    }
                    catch (OperationCanceledException)
                    {
                        break;
                    }
                }
            }
        }

        // GetPortNames()는 레지스트리 조회라 장치 I/O 없이 저렴하다.
        // 조회 자체가 실패하면 기존 동작(Open 시도)으로 폴백한다.
        private bool IsGpsPortPresent()
        {
            try
            {
                return System.IO.Ports.SerialPort.GetPortNames()
                    .Any(p => string.Equals(p, _gpsPort, StringComparison.OrdinalIgnoreCase));
            }
            catch
            {
                return true;
            }
        }

        private void IncreaseGpsReconnectDelay()
        {
            _gpsReconnectCurrentDelayMs = Math.Min(_gpsReconnectCurrentDelayMs * 2, GpsReconnectDelayMaxMs);
        }

        private async Task ConnectGpsAsync(CancellationToken cancellationToken)
        {
            await Task.Run(() =>
            {
                cancellationToken.ThrowIfCancellationRequested();

                var service = new GpsService();
                service.SentenceReceived += OnGpsSentenceReceived;
                service.ErrorOccurred += OnGpsError;

                try
                {
                    service.Connect(_gpsPort, _gpsBaud);

                    lock (_gpsSyncRoot)
                    {
                        _gpsParser = new NmeaParser();
                        _gpsService = service;
                        _lastGpsSentenceUtc = DateTime.UtcNow;
                    }
                }
                catch
                {
                    service.SentenceReceived -= OnGpsSentenceReceived;
                    service.ErrorOccurred -= OnGpsError;
                    service.Dispose();
                    throw;
                }
            }, cancellationToken);
        }

        private bool IsGpsConnectionHealthy()
        {
            lock (_gpsSyncRoot)
            {
                if (_gpsService?.IsConnected != true)
                    return false;

                if (_lastGpsSentenceUtc == DateTime.MinValue)
                    return false;

                return DateTime.UtcNow - _lastGpsSentenceUtc < TimeSpan.FromSeconds(GpsReceiveTimeoutSeconds);
            }
        }

        private void ResetGpsConnection()
        {
            GpsService service;

            lock (_gpsSyncRoot)
            {
                service = _gpsService;
                _gpsService = null;
                _gpsParser = null;
                _lastGpsSentenceUtc = DateTime.MinValue;
            }

            if (service == null)
                return;

            service.SentenceReceived -= OnGpsSentenceReceived;
            service.ErrorOccurred -= OnGpsError;
            service.Dispose();
        }
        // 백그라운드 스레드 수신 -> UI 스레드에서 파싱/표시
        private void OnGpsSentenceReceived(string sentence)
        {
            NmeaParser parser;
            lock (_gpsSyncRoot)
            {
                _lastGpsSentenceUtc = DateTime.UtcNow;
                parser = _gpsParser;
            }

            if (parser == null)
                return;

            var dispatcher = Application.Current?.Dispatcher;
            if (dispatcher == null)
            {
                parser.Parse(sentence);
                UpdateGpsDisplay(parser.CurrentData);
                return;
            }

            dispatcher.InvokeAsync(() =>
            {
                parser.Parse(sentence);
                if (ReferenceEquals(parser, _gpsParser))
                    UpdateGpsDisplay(parser.CurrentData);
            });
        }
        private void OnGpsError(Exception ex)
        {
            lock (_gpsSyncRoot)
            {
                _lastGpsSentenceUtc = DateTime.MinValue;
            }
        }
        private void UpdateGpsDisplay(GpsData data)
        {
            GpsIsValid = data.IsValid;
            GpsFixStatus = data.FixType ?? "No Fix";
            GpsSatelliteCount = data.SatelliteCount.ToString();

            if (data.IsValid)
            {
                GpsLatitude = $"{Math.Abs(data.Latitude):F6}° {(data.Latitude >= 0 ? "N" : "S")}";
                GpsLongitude = $"{Math.Abs(data.Longitude):F6}° {(data.Longitude >= 0 ? "E" : "W")}";

                // 지도 follow: 중심 이동(XAML CenterPoint 바인딩) + 마커 갱신(GpsPositionChanged).
                // UpdateGpsDisplay 는 UI 디스패처에서 호출되므로 UI 요소 접근 안전.
                MapCenterLatitude = data.Latitude;
                MapCenterLongitude = data.Longitude;
                GpsPositionChanged?.Invoke(data.Latitude, data.Longitude);

                // 시/도/군 이름 갱신(오프라인 역지오코딩, 백그라운드). 충분히 이동했을 때만 재조회.
                UpdateRegionLabel(data.Latitude, data.Longitude);
            }
        }

        // ─── 시/도/군 라벨(GpsRegion) 갱신 ────────────────────────────────
        private void UpdateRegionLabel(double lat, double lng)
        {
            // 이미 조회에 성공한 지점에서 충분히 이동하지 않았으면 스킵
            if (_hasGeocodeAnchor &&
                Haversine(lat, lng, _lastGeocodedLat, _lastGeocodedLng) < GeocodeMoveThresholdM)
                return;

            // 조회 진행 중이면 중복 실행 방지(다음 fix 때 다시 시도)
            if (_geocodeInFlight) return;
            _geocodeInFlight = true;

            _ = GeocodeAsync(lat, lng);
        }

        private async Task GeocodeAsync(double lat, double lng)
        {
            try
            {
                // GeoJSON 로드가 진행 중이면 완료될 때까지 대기 후 결과 반환(첫 조회가 '--' 로 빠지지 않도록).
                string name = await _geocoder
                    .GetRegionNameAsync(lat, lng, CancellationToken.None)
                    .ConfigureAwait(false);

                if (!string.IsNullOrEmpty(name))
                {
                    // 성공했을 때만 앵커 확정 → 정지 상태에서 실패하면 다음 fix 때 재시도.
                    _lastGeocodedLat = lat;
                    _lastGeocodedLng = lng;
                    _hasGeocodeAnchor = true;

                    string label = name;
                    var dispatcher = Application.Current?.Dispatcher;
                    if (dispatcher != null && !dispatcher.CheckAccess())
                        await dispatcher.InvokeAsync(() => GpsRegion = label);
                    else
                        GpsRegion = label;
                }
                // name 이 비면 GpsRegion 유지("--"), 앵커도 잡지 않음 → 다음 fix 때 재시도.
            }
            catch { /* 역지오코딩 실패는 무시(직전 값 유지, 다음 fix 때 재시도) */
    }
            finally
            {
                _geocodeInFlight = false;
            }
        }

        /// <summary>두 좌표 간 거리(m) — Haversine</summary>
        private static double Haversine(double lat1, double lng1, double lat2, double lng2)
        {
            const double R = 6371000.0;
            double dLat = ToRad(lat2 - lat1);
            double dLng = ToRad(lng2 - lng1);
            double a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2)
                     + Math.Cos(ToRad(lat1)) * Math.Cos(ToRad(lat2))
                     * Math.Sin(dLng / 2) * Math.Sin(dLng / 2);
            return R * 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        }

        private static double ToRad(double deg) => deg * Math.PI / 180.0;

        private void ResetGpsDisplay(string fixStatus = "No Fix")
        {
            var dispatcher = Application.Current?.Dispatcher;
            if (dispatcher != null && !dispatcher.CheckAccess())
            {
                dispatcher.InvokeAsync(() => ResetGpsDisplay(fixStatus));
                return;
            }

            GpsIsValid = false;
            GpsFixStatus = fixStatus;
            GpsSatelliteCount = "0";
            GpsLatitude = "--";
            GpsLongitude = "--";
        }        public void StartLoop()
        {
            if (!_isLoopRunning) _isLoopRunning = true;
            if (_loopCts == null) _loopCts = new CancellationTokenSource();
            
            _ = SyncSystemModeAsync(_loopCts.Token); // fire-and-forget
        }
        public void StopLoop()
        {
            if (!_isLoopRunning) return;
            _loopCts?.Cancel();     // 진행 작업에 '중단 요청' 신호 - 정리
            _loopCts?.Dispose();    // 내부 리소스 해제 - dispose
            _loopCts = null;        // null 
            _isLoopRunning = false;
        }

        private async Task SyncSystemModeAsync(CancellationToken ct)
        {
            try
            {
                while (!ct.IsCancellationRequested)
                {
                    var app = Application.Current as App;
                    var pcsVm = app?.PcsVm;

                    if (pcsVm?.IsChargeModeActive == true)
                    {
                        ApplyChargingUi();
                    }
                    else if (pcsVm?.IsDischargeModeActive == true)
                    {
                        ApplyDischargingUi();
                    }
                    else
                    {
                        ApplyWaitingUi();
                    }

                    await Task.Delay(200, ct);
                }
            }
            catch (OperationCanceledException)
            {
                // 정상 취소
            }
            finally
            {
                _isLoopRunning = false;
            }
        }

        private void ApplyChargingUi()
        {
            ChargingStatus = HomeStatus.Charging;
            LoadTarget = LoadStatus.Waiting;
            CouplingStatus = false;

            PcsBorderBrush = Brushes.Lime;
            BmsBorderBrush = Brushes.Lime;
            PChargeBorderBrush = Brushes.Lime;
            BChargeBorderBrush = Brushes.Lime;
            OperationModeBrush = Brushes.Lime;

            UpdateLoadTargetUi(false);
            DischargeBorderBrush = Brushes.Gray;
            PDischargeBorderBrush = Brushes.Gray;
            BDischargeBorderBrush = Brushes.Gray;
        }

        private void ApplyDischargingUi()
        {
            ChargingStatus = HomeStatus.Discharging;

            PcsBorderBrush = Brushes.Orange;
            BmsBorderBrush = Brushes.Orange;
            DischargeBorderBrush = Brushes.Orange;
            PChargeBorderBrush = Brushes.Gray;
            BChargeBorderBrush = Brushes.Gray;
            OperationModeBrush = Brushes.Orange;

            CouplingStatus = true;
            UpdateLoadTargetUi(true);

            PDischargeBorderBrush = Brushes.Orange;
            BDischargeBorderBrush = Brushes.Orange;
        }

        private void ApplyWaitingUi()
        {
            ChargingStatus = HomeStatus.Waiting;
            LoadTarget = LoadStatus.Waiting;
            CouplingStatus = false;

            PcsBorderBrush = Brushes.Gray;
            BmsBorderBrush = Brushes.Gray;
            DischargeBorderBrush = Brushes.Gray;
            PChargeBorderBrush = Brushes.Gray;
            BChargeBorderBrush = Brushes.Gray;
            OperationModeBrush = Brushes.Gray;

            UpdateLoadTargetUi(false);
            PDischargeBorderBrush = Brushes.Gray;
            BDischargeBorderBrush = Brushes.Gray;
        }

        private void SelectLoadTarget(LoadStatus target)
        {
            if (target == LoadStatus.Waiting) return;

            SelectedLoadTarget = target;
            RaisePropertyChanged(nameof(SelectedLoadTarget));
            UpdateLoadTargetUi(ChargingStatus == HomeStatus.Discharging);
        }

        private void UpdateLoadTargetUi(bool isActive)
        {
            LoadTarget = isActive ? SelectedLoadTarget : LoadStatus.Waiting;
            ChargeOffGrid = (isActive && SelectedLoadTarget == LoadStatus.OffGrid) ? Brushes.Orange : Brushes.Gray;
            ChargeVihicle = (isActive && SelectedLoadTarget == LoadStatus.Vehicle) ? Brushes.Orange : Brushes.Gray;
        }

        public async Task WaitChangeAsync(CancellationToken ct)
        {
            try
            {
                while (!ct.IsCancellationRequested)
                {
                    switch (emsMode)
                    {
                        case 0: // charge mode
                            ApplyChargingUi();
                            emsMode = 2;
                            await Task.Delay(5000, ct);
                            break;
                        case 1: // discharge mode
                            ApplyDischargingUi();
                            emsMode = 0;
                            await Task.Delay(5000, ct);
                            break;
                        case 2: // none
                            ApplyWaitingUi();
                            emsMode = 1;
                            await Task.Delay(2000, ct);
                            break;
                    }
                }
            }
            catch (OperationCanceledException)
            {
                // 정상 취소
            }
            finally
            {
                _isLoopRunning = false;
            }
        }
        //public async Task ConnectDataAsync(CancellationToken ct)
        //{
        //    while (!ct.IsCancellationRequested)
        //    {
        //        App app = Application.Current as App;

        //        ConnectPCS = app.PcsVm.IsConnected ? "Enable" : "Disable";
        //        ConnectBMS = app.BmsVm.StatusMsg01.Ready == "Open" ? "Enable" : "Disable";

        //        await Task.Delay(200, ct);
        //    }
        //}

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            StopLoop();

            _gpsReconnectCts?.Cancel();
            _gpsReconnectCts?.Dispose();
            _gpsReconnectCts = null;
            _gpsReconnectTask = null;

            ResetGpsConnection();
        }
    }
}

