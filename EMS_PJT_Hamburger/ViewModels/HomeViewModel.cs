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

        private bool _gpsIsValid;
        public bool GpsIsValid
        {
            get => _gpsIsValid;
            set { _gpsIsValid = value; OnPropertyChanged(nameof(GpsIsValid)); }
        }

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
        }

        // ─── GPS 연결/수신 ───────────────────────────────────────────────
        // App.config의 GpsPort가 설정된 경우에만 연결한다(BU-353N 기본 115200 8N1).
        private void StartGps()
        {
            try
            {
                var port = ConfigurationManager.AppSettings["GpsPort"];
                if (string.IsNullOrWhiteSpace(port))
                    return; // 미설정 시 GPS 미사용('--' 유지)

                if (!int.TryParse(ConfigurationManager.AppSettings["GpsBaud"], out var baud) || baud <= 0)
                    baud = 115200;

                _gpsParser = new NmeaParser();
                _gpsService = new GpsService();
                _gpsService.SentenceReceived += OnGpsSentenceReceived;
                _gpsService.ErrorOccurred += OnGpsError;
                _gpsService.Connect(port.Trim(), baud);
            }
            catch (Exception)
            {
                // 연결 실패 시 조용히 '--' 유지 (장치 미연결/포트 사용중 등)
            }
        }

        // 백그라운드 스레드 수신 -> UI 스레드에서 파싱/표시
        private void OnGpsSentenceReceived(string sentence)
        {
            var dispatcher = Application.Current?.Dispatcher;
            if (dispatcher == null)
            {
                _gpsParser.Parse(sentence);
                return;
            }

            dispatcher.InvokeAsync(() =>
            {
                _gpsParser.Parse(sentence);
                UpdateGpsDisplay(_gpsParser.CurrentData);
            });
        }

        private void OnGpsError(Exception ex)
        {
            // 포트 오류 시 무시(연결 유지/해제는 서비스가 관리). 필요 시 로깅 추가 가능.
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
            }
        }

        private void ResetGpsDisplay()
        {
            GpsIsValid = false;
            GpsFixStatus = "No Fix";
            GpsSatelliteCount = "0";
            GpsLatitude = "--";
            GpsLongitude = "--";
        }
        public void StartLoop()
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
            ChargeOnGrid = (isActive && SelectedLoadTarget == LoadStatus.OnGrid) ? Brushes.Orange : Brushes.Gray;
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

            if (_gpsService != null)
            {
                _gpsService.SentenceReceived -= OnGpsSentenceReceived;
                _gpsService.ErrorOccurred -= OnGpsError;
                _gpsService.Dispose();
                _gpsService = null;
            }
        }
    }
}
