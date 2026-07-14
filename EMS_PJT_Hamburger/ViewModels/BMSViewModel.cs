using DevExpress.Mvvm;
using EMS_PJT_Hamburger.Models;
using EMS_PJT_Hamburger.Models.Client;
using EMS_PJT_Hamburger.Models.Client.BMS;
using EMS_PJT_Hamburger.Models.Managers;
using EMS_PJT_Hamburger.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;
using static EMS_PJT_Hamburger.Models.Managers.DbManager;

namespace EMS_PJT_Hamburger.ViewModels
{
    public class BMSViewModel : BmsDataModel, IDisposable
    {
        private static readonly TimeSpan BmsFrameTimeout = TimeSpan.FromSeconds(3);
        // 초기화 실패 시 자동 재시도 간격
        private static readonly TimeSpan BmsReconnectInterval = TimeSpan.FromSeconds(5);
        private bool _disposed;
        private bool _isPcanStarted;
        private DateTime _lastFrameReceivedUtc = DateTime.MinValue;
        private DateTime _lastStartAttemptUtc = DateTime.MinValue;
        private StatusManager.BMSStatus _lastPublishedBmsStatus = StatusManager.BMSStatus.None;
        private DispatcherTimer _bmsStatusTimer;
        private string _lastBmsReadyStateKey;
        private readonly HashSet<int> _activeBmsFaultCodes = new HashSet<int>();
        private readonly Dictionary<uint, DateTime> _lastBmsRawSavedUtcByCanId = new Dictionary<uint, DateTime>();

        public BMSViewModel()
        {
            _rx = new PcanRxService(Peak.Can.Basic.PcanChannel.Usb01, Peak.Can.Basic.Bitrate.Pcan500);
            _rx.FrameReceived += OnFrameReceived;
            _rx.ConnectionStateChanged += HandlePcanConnectionStateChanged;
            StatusMsg02.PropertyChanged += StatusMsg02_PropertyChanged;

            app.nlog.Info($"Started BMS");
            VariableInitialize();
            StartBmsReceiver();

            // 고정 Pack 17개 생성
            for (int i = 1; i <= PackCount; i++)
            {
                //PacksReady.Add(new PackCount
                //{
                //    IsReady = false,
                //});
                Packs.Add(new PackViewModel(i));
            }
            //StatusMsg01.Ready = 1;
            //StatusMsg01.MbmsState = 15;

            //ApplyRandomFaults(StatusMsg02);
            //SaveFaults(GetActiveFaults(StatusMsg02));

            //StatusMsg03.MbmsReady = 1;
            UpdatePacks(_random.Next(0, 131071));

            //StatusMsg04.CellMinPackNumber = 5;
            //StatusMsg04.MaxTemperaturePackNumber = 7;
            //StatusMsg04.MinTemperaturePackNumber = 15;

            //var info0 = BmsSpecs._specMap[0x150]; // canId 0x150에 대한 스펙을 가져온다.
            //var maxVolt = BmsSpecs._specMap[0x154].Fields[0].Convert; // canId 0x154에 대한 스펙을 가져온다.

            CommandInitialize();
        }

        private void StartBmsReceiver()
        {
            if (_disposed || _rx == null) return;

            // 재시도 간격 계산 기준이 되는 마지막 시도 시각 기록
            _lastStartAttemptUtc = DateTime.UtcNow;
            UpdateBmsConnectionStatus(StatusManager.BMSStatus.TryConnect);

            try
            {
                _isPcanStarted = _rx.Start();
                if (!_isPcanStarted)
                {
                    UpdateBmsConnectionStatus(StatusManager.BMSStatus.Disconnected);
                    app?.nlog?.Warn($"PCAN initialize failed. Will retry in {BmsReconnectInterval.TotalSeconds:0}s. Check channel, bitrate, driver, and platform.");
                }
            }
            catch (Exception ex)
            {
                _isPcanStarted = false;
                UpdateBmsConnectionStatus(StatusManager.BMSStatus.Error);
                app?.nlog?.Warn(ex, $"BMS receiver start failed. Will retry in {BmsReconnectInterval.TotalSeconds:0}s.");
            }
        }
        
        private void OnFrameReceived(uint canId, byte[] data)
        {
            if (_disposed) return;

            var hexValueFormat = string.Format("{0:X}", canId);
            var payload = BitConverter.ToString(data ?? Array.Empty<byte>());
            // 수신 프레임은 양이 많아 DEBUG 레벨로만 기록한다.
            app.nlog.Debug($"[RX] ID:{hexValueFormat} DLC:{data?.Length ?? 0} DATA:{payload}");
            // TryGetValue : 있으면 가져와서 쓰고, 없으면 무시
            if (!BmsSpecs._specMap.TryGetValue(canId, out var spec)) // 해시 탐색 1번, key확인 + value추출
                return;

            _lastFrameReceivedUtc = DateTime.UtcNow;
            UpdateBmsConnectionStatus(StatusManager.BMSStatus.Connected);

            // Worker 스레드 → UI 스레드로
            var parsed = CanMessageParser.Parse(spec, data);

            if(canId >= 0x150 && canId < 0x154)
            {
                StatusMessage(parsed, canId);
                SaveBmsHistory(canId, spec.Name, parsed, data);
            }
            else
            {
                if (!_uiTimer.IsEnabled) _uiTimer.Start();
                int packNo = CanIdToPackNo(canId); // packNo : 1 ~ 17
                if (packNo >= 1 && packNo <= PackCount)
                {
                    _packCache[packNo] = new PackSnapshot
                    {
                        LastUpdateUtc = DateTime.UtcNow,
                        Fields = parsed
                    };
                }
            }
        }

        private void SaveBmsHistory(uint canId, string messageName, Dictionary<string, object> parsed, byte[] rawFrame)
        {
            if (parsed == null) return;

            if (canId == 0x151)
                SaveBmsFaultChanges();

            if (app?.DbManager == null) return;

            if (canId == 0x150 || canId == 0x152 || canId == 0x153)
                SaveBmsRawData(canId, messageName, parsed, rawFrame);

            if (canId == 0x150 || canId == 0x152)
                SaveBmsReadyState();
        }

        private void SaveBmsRawData(uint canId, string messageName, Dictionary<string, object> parsed, byte[] rawFrame)
        {
            var nowUtc = DateTime.UtcNow;
            if (_lastBmsRawSavedUtcByCanId.TryGetValue(canId, out var lastSaved) &&
                nowUtc - lastSaved < TimeSpan.FromSeconds(1))
                return;

            app.DbManager.InsertCompressedRawData(
                "BMS",
                messageName,
                unchecked((int)canId),
                parsed,
                rawFrame,
                DateTime.Now);

            _lastBmsRawSavedUtcByCanId[canId] = nowUtc;
        }

        private void SaveBmsReadyState()
        {
            var bmsReady = string.Equals(StatusMsg01.Ready, "Ready", StringComparison.OrdinalIgnoreCase);
            var mbmsReady = string.Equals(StatusMsg03.MbmsReady, "Ready", StringComparison.OrdinalIgnoreCase);
            var readyKey = $"{StatusMsg01.Ready}|{StatusMsg03.MbmsReady}|{StatusMsg01.MbmsState}|{StatusMsg03.PackReady}";
            if (readyKey == _lastBmsReadyStateKey) return;

            var now = DateTime.Now;
            app.DbManager.InsertSystemReadyState("BMS", "BmsReady", bmsReady, StatusMsg01.Ready, StatusMsg01.MbmsState, now);
            app.DbManager.InsertSystemReadyState("BMS", "MbmsReady", mbmsReady, StatusMsg03.MbmsReady, StatusMsg03.PackReady, now);

            _lastBmsReadyStateKey = readyKey;
        }

        private void SaveBmsFaultChanges()
        {
            var activeFaults = GetActiveFaults(StatusMsg02);
            var activeCodes = new HashSet<int>(activeFaults.Select(x => x.Code));

            foreach (var fault in activeFaults)
            {
                if (!_activeBmsFaultCodes.Add(fault.Code))
                    continue;

                AlarmFileLogger.WriteFault(
                    "BMS",
                    "BMS",
                    fault.Code,
                    fault.Name,
                    fault.Name,
                    string.Empty,
                    DateTime.Now);

                app?.DbManager?.InsertBmsAlarmData((fault.Code, fault.Name), 0);
            }

            _activeBmsFaultCodes.RemoveWhere(code => !activeCodes.Contains(code));
        }
        
        private void VariableInitialize()
        {
            // snapshot timer update
            _uiTimer = new DispatcherTimer();
            _uiTimer.Interval = TimeSpan.FromMilliseconds(500);
            _uiTimer.Tick += Snapshot_Tick;

            StatusMsg01.Ready = "Not Ready";
            StatusMsg01.DispSOC = 0d;

            StatusMsg03.MbmsReady = "Not Ready";

            _bmsStatusTimer = new DispatcherTimer();
            _bmsStatusTimer.Interval = TimeSpan.FromSeconds(1);
            _bmsStatusTimer.Tick += BmsStatusTimer_Tick;
            _bmsStatusTimer.Start();
        }

        private void HandlePcanConnectionStateChanged(bool connected)
        {
            if (_disposed) return;

            UpdateBmsConnectionStatus(connected
                ? StatusManager.BMSStatus.TryConnect
                : StatusManager.BMSStatus.Disconnected);
        }

        private void BmsStatusTimer_Tick(object sender, EventArgs e)
        {
            if (_disposed) return;

            // 초기화 실패(미시작) 상태면 일정 간격으로 PCAN 초기화를 자동 재시도한다.
            // 모든 상태 전이는 UpdateBmsConnectionStatus를 거쳐 StatusManager에 연동된다.
            if (!_isPcanStarted)
            {
                if (DateTime.UtcNow - _lastStartAttemptUtc >= BmsReconnectInterval)
                {
                    app?.nlog?.Info("Retrying BMS PCAN initialization...");
                    StartBmsReceiver();
                }
                return;
            }

            // 시작 후에는 프레임 타임아웃으로 연결 끊김을 감지한다.
            if (_lastFrameReceivedUtc == DateTime.MinValue ||
                DateTime.UtcNow - _lastFrameReceivedUtc > BmsFrameTimeout)
            {
                UpdateBmsConnectionStatus(StatusManager.BMSStatus.Disconnected);
            }
        }

        private void UpdateBmsConnectionStatus(StatusManager.BMSStatus status)
        {
            if (_lastPublishedBmsStatus == status) return;

            Action update = () =>
            {
                if (_lastPublishedBmsStatus == status) return;

                var currentApp = Application.Current as App;
                if (currentApp?.StatusManager == null) return;

                currentApp.StatusManager.CurrentBMS_Status = status;
                _lastPublishedBmsStatus = status;

                // 상단 헤더(System Connect 상태 패널) 표시값 반영
                IsBmsConnected = status == StatusManager.BMSStatus.Connected;
                BmsConnectionStatus = BmsStatusText(status);
            };

            var dispatcher = Application.Current?.Dispatcher;
            if (dispatcher?.CheckAccess() == true)
                update();
            else if (dispatcher != null)
                dispatcher.BeginInvoke(update);
        }

        private static string BmsStatusText(StatusManager.BMSStatus status)
        {
            switch (status)
            {
                case StatusManager.BMSStatus.Connected: return "Connected";
                case StatusManager.BMSStatus.TryConnect: return "Connecting";
                case StatusManager.BMSStatus.Disconnected: return "Disconnected";
                case StatusManager.BMSStatus.Error: return "Error";
                default: return "N/A";
            }
        }

        private void Snapshot_Tick(object sender, EventArgs e)
        {
            var now = DateTime.UtcNow;
            var onlineThreshold = TimeSpan.FromMilliseconds(500);
            // Packs에 값 업데이트
            foreach (var packVm in Packs)
            {
                if (_packCache.TryGetValue(packVm.PackNo, out var snap))
                {
                    packVm.IsOnline = (now - snap.LastUpdateUtc) <= onlineThreshold;

                    if (snap.Fields.TryGetValue("MaxCellVoltage", out var maxV))
                        packVm.MaxCellVoltage = Convert.ToDouble(maxV) / 10000;

                    if (snap.Fields.TryGetValue("MinCellVoltage", out var minV))
                        packVm.MinCellVoltage = Convert.ToDouble(minV) / 10000;

                    if (snap.Fields.TryGetValue("MaxTemperature", out var maxT))
                        packVm.MaxTemperature = Convert.ToDouble(maxT);

                    if (snap.Fields.TryGetValue("MinTemperature", out var minT))
                        packVm.MinTemperature = Convert.ToDouble(minT);

                    app.nlog.Debug($"[PackNo:{packVm.PackNo}] MaxCellVoltage:{maxV}  MinCellVoltage:{minV}  " +
                                   $"MaxTemperature:{maxT}  MinTemperature:{minT}");

                }
                else { packVm.IsOnline = false; }
            }
        }
        private void Relay()
        {
            var relayOn = RelayStatus;
            var action = relayOn ? "Relay ON" : "Relay OFF";

            if (!ControlConfirmationService.Confirm("BMS", action))
            {
                RelayStatus = !relayOn;
                return;
            }

            if (RelayStatus) // Relay ON
            {
                SendRelayCommand(true);
            }
            else // Relay OFF
            {
                SendRelayCommand(false);

            }
        }

        #region # Command Button Function
        private void CommandInitialize()
        {
            Cmd_AlarmsPopupBtn = new DelegateCommand(OpenAlarmsWindow);
            Cmd_RelayBtn = new DelegateCommand(Relay);
            //ConnectCommand = new AsyncCommand(ConnectAsync, () => !_service.IsConnected);
        }

        private void OpenAlarmsWindow()
        {
            if (_alarmWin != null) return;
            AlarmService = new AlarmService();
            _alarmWin = new AlarmDetailWindow
            {
                DataContext = CreateAlarmDetailVm(),
                Owner = Application.Current.MainWindow,
                WindowStartupLocation = WindowStartupLocation.CenterOwner
            };

            _alarmWin.Closed += (_, __) =>
            {
                AlarmService?.Stop();
                _alarmWin = null;
                AlarmWindowOpen = true;
            };

            _alarmWin.Show();
            AlarmWindowOpen = false;
            AlarmService.Start();
        }

        #endregion
       
        private void StatusMsg02_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            RaisePropertyChanged(nameof(Alarms));
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;

            try
            {
                if (_uiTimer != null)
                {
                    _uiTimer.Stop();
                    _uiTimer.Tick -= Snapshot_Tick;
                    _uiTimer = null;
                }

                if (_bmsStatusTimer != null)
                {
                    _bmsStatusTimer.Stop();
                    _bmsStatusTimer.Tick -= BmsStatusTimer_Tick;
                    _bmsStatusTimer = null;
                }

                UpdateBmsConnectionStatus(StatusManager.BMSStatus.Disconnected);

                StatusMsg02.PropertyChanged -= StatusMsg02_PropertyChanged;

                if (_rx != null)
                {
                    _rx.FrameReceived -= OnFrameReceived;
                    _rx.ConnectionStateChanged -= HandlePcanConnectionStateChanged;
                    _rx.Dispose();
                    _rx = null;
                }

                AlarmService?.Stop();
                AlarmService = null;

                if (_alarmWin != null)
                {
                    _alarmWin.Close();
                    _alarmWin = null;
                }
            }
            catch (Exception ex)
            {
                app?.nlog?.Warn(ex, "BMSViewModel dispose failed.");
            }
        }

    }
}
