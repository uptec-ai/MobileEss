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
        private bool _disposed;

        public BMSViewModel()
        {
            _rx = new PcanRxService(Peak.Can.Basic.PcanChannel.Usb01, Peak.Can.Basic.Bitrate.Pcan500);
            _rx.FrameReceived += OnFrameReceived;
            StatusMsg02.PropertyChanged += StatusMsg02_PropertyChanged;

            app.nlog.Info($"Started BMS");
            //if (!_rx.Start())
            //{
            //    MessageBox.Show("PCAN Initialize 실패. 채널/비트레이트/드라이버/플랫폼(x64) 확인하세요.");
            //}

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

            VariableInitialize();
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

            // Worker 스레드 → UI 스레드로
            var parsed = CanMessageParser.Parse(spec, data);

            if(canId >= 0x150 && canId < 0x154)
            {
                StatusMessage(parsed, canId);
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
        
        private void VariableInitialize()
        {
            // snapshot timer update
            _uiTimer = new DispatcherTimer();
            _uiTimer.Interval = TimeSpan.FromMilliseconds(500);
            _uiTimer.Tick += Snapshot_Tick;

            StatusMsg01.Ready = "OFF";
            StatusMsg01.DispSOC = 20d;
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
            OnPropertyChanged(nameof(Alarms));
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

                StatusMsg02.PropertyChanged -= StatusMsg02_PropertyChanged;

                if (_rx != null)
                {
                    _rx.FrameReceived -= OnFrameReceived;
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
