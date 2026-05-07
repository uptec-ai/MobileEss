using DevExpress.Mvvm;
using EMS_PJT_Hamburger.Models.Managers;
using SciChart.Charting.Model.DataSeries;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace EMS_PJT_Hamburger.Models.Client.PCS
{
    public class DataItem : INotifyPropertyChanged
    {
        public string Header { get; set; }
        public string Name { get; set; }
        public string Value { get; set; }
        public string Factor { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
    }
    public class PcsFaultItem
    {
        public DateTime OccurredAt { get; set; }
        public string Category { get; set; }
        public int Bit { get; set; }
        public string Message { get; set; }
        public ushort RawValue { get; set; }
    }
    public class PcsModel : ViewModelBase, INotifyPropertyChanged
    {
        private static readonly string[] GridFaultMessages =
        {
            "Line Frequency Fault",
            "G Frequency Fault",
            "Line Voltage Fault",
            "Line Check Fault",
            "Island Fault",
            "OV Grid Fault",
            "UV Grid Fault",
            "OF Grid Fault",
            "UF Grid Fault",
            "OC Grid Fault",
        };

        private static readonly string[] InvFaultMessages =
        {
            "Connection Error",
            "DC OV Fault",
            "OC INV Fault",
            "OC Converter Fault",
            "Sensor Fault",
            "IGBT Error",
            "INV GND Fault",
            "Zero Sequence Fault",
            "DCL OC Fault",
            "DC UV Fault",
            "DC Charge Fault",
            "OV INV Fault",
            "UV INV Fault",
            "Unbalanced AC Fault",
        };

        private static readonly string[] LoadFaultMessages =
        {
            "Over Load Fault",
        };

        private static readonly string[] BatteryFaultMessages =
        {
            "Battery OC Fault",
            "BMS Fault",
            "Battery OV Fault",
            "Battery UV Fault",
            "BMS OC1 Fault",
            "BMS OC2 Fault",
            "Low SOC Fault",
            "BMS Warning",
            "BMS Fault",
        };

        private static readonly string[] SystemFaultMessages =
        {
            "PV Fault",
            "Battery Fault",
            "INV Fault",
            "Grid Fault",
            "Load Fault",
            "Communication Fault",
        };

        private static readonly string[] CommunicationFaultMessages =
        {
            "PMS Communication Fault",
            "KPD Communication Fault",
            "RTU Communication Fault",
            "PMS Command Error",
        };

        private readonly HashSet<string> _activePcsFaultKeys = new HashSet<string>();
        public ModbusService _client = new ModbusService();
        public ConnectionSettings Conn_Settings { get; set; }
        public ConnectionState Conn_State { get; set; }
        public ObservableCollection<RegisterItem> KeepAliveRegisters { get; } = new ObservableCollection<RegisterItem>();
        public ObservableCollection<PcsFaultItem> PcsFaultMessages { get; } = new ObservableCollection<PcsFaultItem>();
        protected const ushort PollStartAddress = 0;
        protected const ushort PollRegisterCount = 355;
        protected static readonly TimeSpan PollInterval = TimeSpan.FromMilliseconds(500);

        // UI Data Init
        public PCS_PanelData PanelData { get; set; } = new PCS_PanelData()
        {
            BattReady = false,
            InvReady = false,
            GridReady = false,
            CommReady = false,
            BypassReady = false,
            AlarmCnt = "0",
            T_Import_Energy = "0.0",
            T_Export_Energy = "0.0",
        };
        public INV_PcsData InvData { get; set; } = new INV_PcsData()
        {
            Power = "0.0",
            VoltAverage = "0.0",
            CurrAverage = "0.0",
        };
        public ObservableCollection<DataItem> _gridItems { get; set; }
        public ObservableCollection<DataItem> GridItems
        {
            get => _gridItems;
            set
            {
                _gridItems = value;
                OnPropertyChanged(nameof(GridItems));
            }
        }

        public ObservableCollection<DataItem> _loadItems { get; set; }
        public ObservableCollection<DataItem> LoadItems
        {
            get => _loadItems;
            set
            {
                _loadItems = value;
                OnPropertyChanged(nameof(LoadItems));
            }
        }

        public ObservableCollection<DataItem> _etcItems { get; set; }
        public ObservableCollection<DataItem> EtcItems
        {
            get => _etcItems;
            set
            {
                _etcItems = value;
                OnPropertyChanged(nameof(EtcItems));
            }
        }

        // 상태
        protected IDispatcherService Dispatcher => GetService<IDispatcherService>();
        public bool IsConnected { get => GetProperty(() => IsConnected); set => SetProperty(() => IsConnected, value); } // Server connected
        public bool IsReceive { get => GetProperty(() => IsReceive); set => SetProperty(() => IsReceive, value); } // RX(수신)
        public bool IsTransmit { get => GetProperty(() => IsTransmit); set => SetProperty(() => IsTransmit, value); } // TX(송신)
        public bool IsWrite { get => GetProperty(() => IsWrite); set => SetProperty(() => IsWrite, value); } // Send Write 동작?
        public bool IsRelay { get => GetProperty(() => IsRelay); set => SetProperty(() => IsRelay, value); } // BMS Relay 동작?
        public string SystemMsg { get; set; } = string.Empty; // [Log] system msg

        // Chart
        private DateTime _chartBaseDate = DateTime.Today;
        private double _yesterdayFinalImportedEnergy = 0d;
        private double _todayImportedEnergy = 0d;

        public XyDataSeries<DateTime, double> DailyImportedEnergySummarySeries
        {
            get => GetProperty(() => DailyImportedEnergySummarySeries);
            set => SetProperty(() => DailyImportedEnergySummarySeries, value);
        }
        public async Task InitializeDailyImportedEnergySummaryAsync()
        {
            _chartBaseDate = DateTime.Today;
            _yesterdayFinalImportedEnergy = await LoadYesterdayFinalImportedEnergyAsync();
            _todayImportedEnergy = 0d;

            DailyImportedEnergySummarySeries = new XyDataSeries<DateTime, double>
            {
                SeriesName = "Daily Imported Energy Summary"
            };

            RebuildDailyImportedEnergySummary();
        }

        private async Task<double> LoadYesterdayFinalImportedEnergyAsync()
        {
            return await Task.Run(() =>
            {
                var app = Application.Current as App;
                if (app?.DbManager == null) return 0d;

                DataSet ds = app.DbManager.GetDataSetByQuery(@"
select g.grid_daily_imported_energy
from tb_pcs_status s
join tb_pcs_grid g on g.snapshot_id = s.snapshot_id
where s.collected_at >= @from_dt
  and s.collected_at < @to_dt
order by s.collected_at desc
limit 1;",
                cmd =>
                {
                    cmd.Parameters.AddWithValue("@from_dt", DateTime.Today.AddDays(-1));
                    cmd.Parameters.AddWithValue("@to_dt", DateTime.Today);
                });

                if (ds.Tables.Count == 0 || ds.Tables[0].Rows.Count == 0)
                    return 0d;

                return Convert.ToDouble(ds.Tables[0].Rows[0][0]);
            });
        }
        public void UpdateDailyImportedEnergySummary(double todayDailyImportedEnergy)
        {
            var today = DateTime.Today;

            if (today != _chartBaseDate)
            {
                _chartBaseDate = today;
                _yesterdayFinalImportedEnergy = _todayImportedEnergy;
                _todayImportedEnergy = 0d;
            }

            _todayImportedEnergy = todayDailyImportedEnergy;
            RebuildDailyImportedEnergySummary();
        }

        private void RebuildDailyImportedEnergySummary()
        {
            DailyImportedEnergySummarySeries.Clear();

            DailyImportedEnergySummarySeries.Append(_chartBaseDate.AddDays(-1), _yesterdayFinalImportedEnergy);
            DailyImportedEnergySummarySeries.Append(_chartBaseDate, _todayImportedEnergy);
            DailyImportedEnergySummarySeries.Append(_chartBaseDate.AddDays(1), 0d);
        }


        #region [ Using Function ]

        public event PropertyChangedEventHandler PropertyChanged = delegate { };
        public void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        private static bool TryGetString(Dictionary<string, object> parsed, string key, out string value)
        {
            value = null;
            if (parsed == null || string.IsNullOrWhiteSpace(key)) return false;
            if (!parsed.TryGetValue(key, out var raw) || raw == null) return false;

            value = raw.ToString();
            return true;
        }

        private static bool TryGetDouble(Dictionary<string, object> parsed, string key, out double value)
        {
            value = 0;
            if (parsed == null || string.IsNullOrWhiteSpace(key)) return false;
            if (!parsed.TryGetValue(key, out var raw) || raw == null) return false;

            try
            {
                value = Convert.ToDouble(raw);
                return true;
            }
            catch
            {
                return false;
            }
        }

        private void UpdatePcsFaultMessages(string category, ushort bits, string[] messages)
        {
            for (var bit = 0; bit < messages.Length; bit++)
            {
                var key = $"{category}:{bit}";
                var isFault = (bits & (1 << bit)) != 0;

                if (!isFault)
                {
                    _activePcsFaultKeys.Remove(key);
                    continue;
                }

                if (!_activePcsFaultKeys.Add(key))
                    continue;

                AddPcsFaultMessage(new PcsFaultItem
                {
                    OccurredAt = DateTime.Now,
                    Category = category,
                    Bit = bit,
                    Message = messages[bit],
                    RawValue = bits,
                });
            }
        }

        private void AddPcsFaultMessage(PcsFaultItem fault)
        {
            void Add()
            {
                PcsFaultMessages.Insert(0, fault);
                if (PcsFaultMessages.Count > 500)
                    PcsFaultMessages.RemoveAt(PcsFaultMessages.Count - 1);
            }

            var ui = Application.Current?.Dispatcher;
            if (ui == null || ui.CheckAccess())
            {
                Add();
                return;
            }

            ui.BeginInvoke((Action)Add);
        }

        #endregion

        #region [ Conncet Function ]

        public async Task UpdateAsync()
        {
            var data = await _client
                .ReadHoldingRegistersAsync(1, PollStartAddress, PollRegisterCount, CancellationToken.None)
                .ConfigureAwait(false);

            ApplyParsedData(data);
        }

        public async Task StartPolling()
        {
            try
            {
                await _client.StartAsync();
            }
            catch (OperationCanceledException)
            {
                // 정상 취소
            }
            catch (Exception ex)
            {
                SystemMsg = $"failed:{ex.Message}";
            }
        }

        public void StopPolling()
        {
            try { _client?.StopAsync().GetAwaiter().GetResult(); }
            catch { }
        }
        public async void OnConnectionChanged(bool connected)
        {
            try
            {
                var ui = Application.Current?.Dispatcher;
                Action updateUi = () =>
                {
                    var app = Application.Current as App;
                    IsConnected = connected;
                    IsReceive = connected;

                    // ConnectionState.IsConnected(== "Connected")와 문자열을 맞춰 false 오판정을 방지합니다.
                    Conn_State.Status = connected ? "Connected" : "Disconnected";

                    if (app?.StatusManager != null)
                    {
                        app.StatusManager.CurrentPCS_Status = connected
                            ? StatusManager.PCSStatus.Connected
                            : StatusManager.PCSStatus.Disconnected;
                    }

                    Conn_State.Rtt = connected ? Conn_State.Rtt : "0";
                    SystemMsg = connected ? "connected to server." : "disconnected from server";
                };

                if (ui?.CheckAccess() == true)
                {
                    updateUi();
                }
                else if (Dispatcher != null)
                {
                    await Dispatcher.BeginInvoke(updateUi);
                }
                else if (ui != null)
                {
                    ui.BeginInvoke(updateUi);
                }
                else
                {
                    updateUi();
                }
            }
            catch (Exception ex)
            {
                SystemMsg = $"[E] connection state update failed: {ex.Message}";
            }
        }
        public virtual void DisposeModelResources()
        {
            StopPolling();
            _client?.Dispose();
        }

        #endregion

        #region [ Received Data Function ]
        public void OnInputRegistersReceived(object sender, InputRegistersEventArgs e)
        {
            if (e == null || e.Values == null) return;

            if (e.StartAddress != PollStartAddress || e.Values.Length < PollRegisterCount)
            {
                SystemMsg = $"invalid frame: start={e.StartAddress}, len={e.Values.Length}";
                return;
            }

            ApplyParsedData(e.Values);
        }

        private void ApplyParsedData(ushort[] registers)
        {
            if (registers == null || registers.Length < PollRegisterCount)
            {
                SystemMsg = $"insufficient registers: {registers?.Length ?? 0}/{PollRegisterCount}";
                return;
            }

            var parsed = ModbusParser.ParseRegisters(registers, PcsSpecs.All, PollStartAddress);
            ChangeInformation(parsed);
        }

        protected async Task WriteControlU16Async(string ctrl, double input)
        {
            var spec = PcsSpecs.ControlWrite[ctrl];

            var raw = (int)Math.Round(input / spec.Scale);
            if (raw < ushort.MinValue || raw > ushort.MaxValue)
                throw new ArgumentOutOfRangeException(nameof(input), $"[{ctrl}] write value is out of U16 range.");

            await _client.WriteSingleRegisterAsync(spec.Address, (ushort)raw);
        }

        protected async Task WriteControlRawU16Async(string ctrl, ushort value)
        {
            var spec = PcsSpecs.ControlWrite[ctrl];

            await _client.WriteSingleRegisterAsync(spec.Address, value);
        }

        protected async Task WriteControlU32Async(string ctrl, double input)
        {
            var spec = PcsSpecs.ControlWrite[ctrl];

            var raw = input / spec.Scale;
            if (raw < uint.MinValue || raw > uint.MaxValue)
                throw new ArgumentOutOfRangeException(nameof(input), $"[{ctrl}] write value is out of U32 range.");

            var value = (uint)Math.Round(raw);
            var values = new[]
            {
                (ushort)(value >> 16),
                (ushort)(value & 0xFFFF)
            };

            await _client.WriteMultipleRegistersAsync(spec.Address, values);
        }

        #endregion

        #region [ UI Function ]
        private static int CountSetBits(ushort value)
        {
            int count = 0;

            while (value != 0)
            {
                count += value & 1;
                value >>= 1;
            }

            return count;
        }
        void ChangePanelData(Dictionary<string, object> parsed)
        {
            if (TryGetDouble(parsed, "ReadyStatus", out var ready))
            {
                var readyBits = Convert.ToUInt16(ready);
                PanelData.BattReady = (readyBits & (1 << 1)) != 0;
                PanelData.InvReady = (readyBits & (1 << 2)) != 0;
                PanelData.GridReady = (readyBits & (1 << 3)) != 0;
                PanelData.CommReady = (readyBits & (1 << 5)) != 0;
                PanelData.BypassReady = (readyBits & (1 << 6)) != 0;
            }

            int alarmCnt = 0;
            if (TryGetDouble(parsed, "GridFault", out var gridFault))
            {
                var bits = Convert.ToUInt16(gridFault);
                alarmCnt += CountSetBits(bits);
                UpdatePcsFaultMessages("Grid", bits, GridFaultMessages);
            }
            if (TryGetDouble(parsed, "InvFault", out var invFault))
            {
                var bits = Convert.ToUInt16(invFault);
                alarmCnt += CountSetBits(bits);
                UpdatePcsFaultMessages("Inverter", bits, InvFaultMessages);
            }
            if (TryGetDouble(parsed, "LoadFault", out var loadFault))
            {
                var bits = Convert.ToUInt16(loadFault);
                alarmCnt += CountSetBits(bits);
                UpdatePcsFaultMessages("Load", bits, LoadFaultMessages);
            }
            if (TryGetDouble(parsed, "BatteryFault", out var batteryFault))
            {
                var bits = Convert.ToUInt16(batteryFault);
                alarmCnt += CountSetBits(bits);
                UpdatePcsFaultMessages("Battery", bits, BatteryFaultMessages);
            }
            if (TryGetDouble(parsed, "SystemFault", out var systemFault))
            {
                var bits = Convert.ToUInt16(systemFault);
                alarmCnt += CountSetBits(bits);
                UpdatePcsFaultMessages("System", bits, SystemFaultMessages);
            }
            if (TryGetDouble(parsed, "CommunicationFault", out var communicationFault))
            {
                var bits = Convert.ToUInt16(communicationFault);
                alarmCnt += CountSetBits(bits);
                UpdatePcsFaultMessages("Communication", bits, CommunicationFaultMessages);
            }
            PanelData.AlarmCnt = alarmCnt.ToString();

        }
        void ChangeGridData(Dictionary<string, object> parsed)
        {
            if (TryGetDouble(parsed, "ReadyStatus", out var ready))
            {
                var readyBits = Convert.ToUInt16(ready);
                GridItems[1].Value = (readyBits & (1 << 3)) != 0 ? "on" : "off";
            }
            if (TryGetDouble(parsed, "GridFuseStatus", out var fuse))
            {
                var readyBits = Convert.ToUInt16(fuse);
                var faultPhase = "grid fuse fault : ";

                if ((readyBits & (1 << 0)) != 0) faultPhase += "R";
                if ((readyBits & (1 << 1)) != 0) faultPhase += "S";
                if ((readyBits & (1 << 2)) != 0) faultPhase += "T";

                SystemMsg = faultPhase;
                GridItems[2].Value = (fuse == 0) ? "normal" : $"fault";
            }
            
            if (TryGetDouble(parsed, "GridFault", out var fault))
            {
                var readyBits = Convert.ToUInt16(fault);
                int alarmCnt = 0;

                if ((readyBits & (1 << 0)) != 0) { alarmCnt++; SystemMsg = "line frequency fault"; }// line freq
                if ((readyBits & (1 << 1)) != 0) { alarmCnt++; SystemMsg = "G.frequency fault"; }// G.freq
                if ((readyBits & (1 << 2)) != 0) { alarmCnt++; SystemMsg = "line voltage fault"; } // Line Volt
                if ((readyBits & (1 << 3)) != 0) { alarmCnt++; SystemMsg = "line check fault"; }// Line Check
                if ((readyBits & (1 << 4)) != 0) { alarmCnt++; SystemMsg = "island fault"; } // Island
                if ((readyBits & (1 << 5)) != 0) { alarmCnt++; SystemMsg = "ov grid fault"; }// OV grid
                if ((readyBits & (1 << 6)) != 0) { alarmCnt++; SystemMsg = "uv grid fault"; }// UV grid
                if ((readyBits & (1 << 7)) != 0) { alarmCnt++; SystemMsg = "of grid fault"; }// OF grid
                if ((readyBits & (1 << 8)) != 0) { alarmCnt++; SystemMsg = "uf grid fault"; } // UF grid
                if ((readyBits & (1 << 9)) != 0) { alarmCnt++; SystemMsg = "oc grid fault"; }// OC grid

                GridItems[3].Value = alarmCnt.ToString();
            }
            // Grid 수전 유효전력량
            if (TryGetDouble(parsed, "GridTotalImportActivePower", out var tImport))
            {
                PanelData.T_Import_Energy = tImport.ToString("0.0");
                GridItems[5].Value = tImport.ToString("0.0");
            }
            // Grid 송전 유효전력량
            if (TryGetDouble(parsed, "GridTotalExportedActivePower", out var tExport))
            {
                PanelData.T_Export_Energy = tExport.ToString("0.0");
                GridItems[6].Value = tExport.ToString("0.0");
            }
            if (TryGetDouble(parsed, "GridVoltageAN", out var vAn)) GridItems[7].Value = vAn.ToString("0.0");
            if (TryGetDouble(parsed, "GridVoltageBN", out var vBn)) GridItems[8].Value = vBn.ToString("0.0");
            if (TryGetDouble(parsed, "GridVoltageCN", out var vCn)) GridItems[9].Value = vCn.ToString("0.0");

            if (TryGetDouble(parsed, "GridCurrentAN", out var cAn)) GridItems[10].Value = cAn.ToString("0.0");
            if (TryGetDouble(parsed, "GridCurrentBN", out var cBn)) GridItems[11].Value = cBn.ToString("0.0");
            if (TryGetDouble(parsed, "GridCurrentCN", out var cCn)) GridItems[12].Value = cCn.ToString("0.0");

            if (TryGetDouble(parsed, "GridVoltageAB", out var vAb)) GridItems[13].Value = vAb.ToString("0.0");
            if (TryGetDouble(parsed, "GridVoltageBC", out var vBc)) GridItems[14].Value = vBc.ToString("0.0");
            if (TryGetDouble(parsed, "GridVoltageCA", out var vCa)) GridItems[15].Value = vCa.ToString("0.0");

            if (TryGetDouble(parsed, "GridFrequency", out var freq)) GridItems[16].Value = freq.ToString("0.00");
            if (TryGetDouble(parsed, "GridPowerFactor", out var pf)) GridItems[17].Value = pf.ToString("0.00");
            if (TryGetDouble(parsed, "GridSurgeCounter", out var sc)) GridItems[18].Value = sc.ToString();
        }
        void ChangeInverterData(Dictionary<string, object> parsed)
        {
            

        }
        void ChangeLoadData(Dictionary<string, object> parsed)
        {
            //if (TryGetDouble(parsed, "Load_Status", out var ready)) LoadItems[1].Value = ready >= 500 ? "on" : "off";
            if (TryGetDouble(parsed, "LoadFault", out var fault))
            {
                var readyBits = Convert.ToUInt16(fault);

                if ((readyBits & (1 << 0)) != 0) { GridItems[1].Value = "fault"; SystemMsg = "over load fault"; }// line freq
                else GridItems[1].Value = "normal";
            }

            if (TryGetDouble(parsed, "LoadTotalExportedActivePower", out var tExport)) LoadItems[3].Value = tExport.ToString("0.0");
            if (TryGetDouble(parsed, "LoadActivePower", out var p)) LoadItems[4].Value = p.ToString("0.0");
            if (TryGetDouble(parsed, "LoadActivePowerRN", out var pRn)) LoadItems[5].Value = pRn.ToString("0.0");
            if (TryGetDouble(parsed, "LoadActivePowerSN", out var pSn)) LoadItems[6].Value = pSn.ToString("0.0");
            if (TryGetDouble(parsed, "LoadActivePowerTN", out var pTn)) LoadItems[7].Value = pTn.ToString("0.0");

            if (TryGetDouble(parsed, "LoadVoltageAN", out var vAn)) LoadItems[8].Value = vAn.ToString("0.0");
            if (TryGetDouble(parsed, "LoadVoltageBN", out var vBn)) LoadItems[9].Value = vBn.ToString("0.0");
            if (TryGetDouble(parsed, "LoadVoltageCN", out var vCn)) LoadItems[10].Value = vCn.ToString("0.0");

            if (TryGetDouble(parsed, "LoadCurrentAN", out var cAn)) LoadItems[11].Value = cAn.ToString("0.0");
            if (TryGetDouble(parsed, "LoadCurrentBN", out var cBn)) LoadItems[12].Value = cBn.ToString("0.0");
            if (TryGetDouble(parsed, "LoadCurrentCN", out var cCn)) LoadItems[13].Value = cCn.ToString("0.0");

            if (TryGetDouble(parsed, "LoadVoltageAB", out var vAb)) LoadItems[14].Value = vAb.ToString("0.0");
            if (TryGetDouble(parsed, "LoadVoltageBC", out var vBc)) LoadItems[15].Value = vBc.ToString("0.0");
            if (TryGetDouble(parsed, "LoadVoltageCA", out var vCa)) LoadItems[16].Value = vCa.ToString("0.0");

            if (TryGetDouble(parsed, "LoadFrequency", out var freq)) LoadItems[17].Value = freq.ToString("0.00");
            if (TryGetDouble(parsed, "LoadPowerFactor", out var pf)) LoadItems[18].Value = pf.ToString("0.00");
        }
        void ChangeEtcData(Dictionary<string, object> parsed)
        {
            if (TryGetDouble(parsed, "InvAmbientTemperature", out var aTemp01)) EtcItems[1].Value = aTemp01.ToString();

            if (TryGetDouble(parsed, "InvHeatsinkTemperature01", out var hTemp01)) EtcItems[2].Value = hTemp01.ToString();
            if (TryGetDouble(parsed, "InvHeatsinkTemperature02", out var hTemp02)) EtcItems[3].Value = hTemp02.ToString();
            if (TryGetDouble(parsed, "InvHeatsinkTemperature03", out var hTemp03)) EtcItems[4].Value = hTemp03.ToString();
            if (TryGetDouble(parsed, "InvHeatsinkTemperature04", out var hTemp04)) EtcItems[5].Value = hTemp04.ToString();
            if (TryGetDouble(parsed, "InvHeatsinkTemperature05", out var hTemp05)) EtcItems[6].Value = hTemp05.ToString();
            if (TryGetDouble(parsed, "InvHeatsinkTemperature06", out var hTemp06)) EtcItems[7].Value = hTemp06.ToString();
            if (TryGetDouble(parsed, "InvHeatsinkTemperature07", out var hTemp07)) EtcItems[8].Value = hTemp07.ToString();
            if (TryGetDouble(parsed, "InvHeatsinkTemperature08", out var hTemp08)) EtcItems[9].Value = hTemp08.ToString();

            if (TryGetDouble(parsed, "InvLeakageCurrent", out var leak)) EtcItems[10].Value = leak.ToString();
            if (TryGetDouble(parsed, "InvHeartBeat", out var hb)) EtcItems[11].Value = hb.ToString();
        }

        private int _uiUpdatePending;
        private Dictionary<string, object> _latestParsed;

        void ChangeInformation(Dictionary<string, object> parsed)
        {
            _latestParsed = parsed;

            if (Interlocked.Exchange(ref _uiUpdatePending, 1) == 1)
                return;

            Application.Current.Dispatcher.BeginInvoke(new Action(() =>
            {

                try
                {
                    var snapshot = _latestParsed;
                    ChangePanelData(snapshot);
                    ChangeInverterData(snapshot);
                    ChangeGridData(snapshot);
                    ChangeLoadData(snapshot);
                    ChangeEtcData(snapshot);
                }
                finally
                {
                    Interlocked.Exchange(ref _uiUpdatePending, 0);
                }
            }));
            App app = Application.Current as App;
            //SavePcsData(_latestParsed);

            UpdateDailyImportedEnergySummary(app.DbManager.ReadDouble(_latestParsed, "Grid_Daily_ImportedEnergy"));
        }

        #endregion

        #region [ Database Function ]

        public void SavePcsData(Dictionary<string, object> parsed)
        {
            App app = Application.Current as App;
            
            app.DbManager.InsertPcsData(parsed);
            
        }

        #endregion
    }
}
