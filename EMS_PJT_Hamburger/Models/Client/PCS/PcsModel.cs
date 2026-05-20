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
        private string _lastPcsStatusSnapshotKey;
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
            PcsTime = "--",
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
        public ObservableCollection<DataItem> _invtems { get; set; }
        public ObservableCollection<DataItem> InvItems
        {
            get => _invtems;
            set
            {
                _invtems = value;
                OnPropertyChanged(nameof(InvItems));
            }
        }
        public ObservableCollection<DataItem> _battItems { get; set; }
        public ObservableCollection<DataItem> BattItems
        {
            get => _battItems;
            set
            {
                _battItems = value;
                OnPropertyChanged(nameof(BattItems));
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
//        private DateTime _chartBaseDate = DateTime.Today;
//        private const int DailyEnergySummaryStartOffsetDays = -3;
//        private const int DailyEnergySummaryEndOffsetDays = 1;
//        private readonly Dictionary<DateTime, double> _dailyFinalImportedEnergyByDate = new Dictionary<DateTime, double>();
//        private readonly Dictionary<DateTime, double> _dailyFinalExportedEnergyByDate = new Dictionary<DateTime, double>();
//        private double _todayImportedEnergy = 0d;
//        private double _todayExportedEnergy = 0d;

//        public XyDataSeries<DateTime, double> DailyImportedEnergySummarySeries
//        {
//            get => GetProperty(() => DailyImportedEnergySummarySeries);
//            set => SetProperty(() => DailyImportedEnergySummarySeries, value);
//        }
//        public XyDataSeries<DateTime, double> DailyExportedEnergySummarySeries
//        {
//            get => GetProperty(() => DailyExportedEnergySummarySeries);
//            set => SetProperty(() => DailyExportedEnergySummarySeries, value);
//        }
//        public string DailyImportedYesterdayLabel
//        {
//            get => GetProperty(() => DailyImportedYesterdayLabel);
//            set => SetProperty(() => DailyImportedYesterdayLabel, value);
//        }
//        public DateTime DailyImportedYesterdayX
//        {
//            get => GetProperty(() => DailyImportedYesterdayX);
//            set => SetProperty(() => DailyImportedYesterdayX, value);
//        }
//        public double DailyImportedYesterdayY
//        {
//            get => GetProperty(() => DailyImportedYesterdayY);
//            set => SetProperty(() => DailyImportedYesterdayY, value);
//        }
//        public string DailyImportedTodayLabel
//        {
//            get => GetProperty(() => DailyImportedTodayLabel);
//            set => SetProperty(() => DailyImportedTodayLabel, value);
//        }
//        public DateTime DailyImportedTodayX
//        {
//            get => GetProperty(() => DailyImportedTodayX);
//            set => SetProperty(() => DailyImportedTodayX, value);
//        }
//        public double DailyImportedTodayY
//        {
//            get => GetProperty(() => DailyImportedTodayY);
//            set => SetProperty(() => DailyImportedTodayY, value);
//        }
//        public string DailyExportedYesterdayLabel
//        {
//            get => GetProperty(() => DailyExportedYesterdayLabel);
//            set => SetProperty(() => DailyExportedYesterdayLabel, value);
//        }
//        public DateTime DailyExportedYesterdayX
//        {
//            get => GetProperty(() => DailyExportedYesterdayX);
//            set => SetProperty(() => DailyExportedYesterdayX, value);
//        }
//        public double DailyExportedYesterdayY
//        {
//            get => GetProperty(() => DailyExportedYesterdayY);
//            set => SetProperty(() => DailyExportedYesterdayY, value);
//        }
//        public string DailyExportedTodayLabel
//        {
//            get => GetProperty(() => DailyExportedTodayLabel);
//            set => SetProperty(() => DailyExportedTodayLabel, value);
//        }
//        public DateTime DailyExportedTodayX
//        {
//            get => GetProperty(() => DailyExportedTodayX);
//            set => SetProperty(() => DailyExportedTodayX, value);
//        }
//        public double DailyExportedTodayY
//        {
//            get => GetProperty(() => DailyExportedTodayY);
//            set => SetProperty(() => DailyExportedTodayY, value);
//        }
//        public async Task InitializeDailyImportedEnergySummaryAsync()
//        {
//            _chartBaseDate = DateTime.Today;
//            await LoadDailyFinalGridEnergyAsync(_chartBaseDate.AddDays(DailyEnergySummaryStartOffsetDays), _chartBaseDate);
//            _todayImportedEnergy = 0d;
//            _todayExportedEnergy = 0d;

//            DailyImportedEnergySummarySeries = new XyDataSeries<DateTime, double>
//            {
//                SeriesName = "Daily Imported Energy Summary"
//            };
//            DailyExportedEnergySummarySeries = new XyDataSeries<DateTime, double>
//            {
//                SeriesName = "Daily Exported Energy Summary"
//            };
//            RebuildDailyImportedEnergySummary();
//            RebuildDailyExportedEnergySummary();
//        }

//        private async Task LoadDailyFinalGridEnergyAsync(DateTime fromDate, DateTime toDate)
//        {
//            await Task.Run(() =>
//            {
//                var app = Application.Current as App;
//                if (app?.DbManager == null) return;

//                DataSet ds = app.DbManager.GetDataSetByQuery(@"
//select distinct on (collected_at::date)
//       collected_at::date as collected_day,
//       total_imported,
//       total_exported
//from public.tb_pcs_grid
//where collected_at >= @from_dt
//  and collected_at < @to_dt
//order by collected_at::date, collected_at desc;",
//                cmd =>
//                {
//                    cmd.Parameters.AddWithValue("@from_dt", fromDate.Date);
//                    cmd.Parameters.AddWithValue("@to_dt", toDate.Date);
//                });

//                _dailyFinalImportedEnergyByDate.Clear();
//                _dailyFinalExportedEnergyByDate.Clear();

//                for (var date = fromDate.Date; date < toDate.Date; date = date.AddDays(1))
//                {
//                    _dailyFinalImportedEnergyByDate[date] = 0d;
//                    _dailyFinalExportedEnergyByDate[date] = 0d;
//                }

//                if (ds.Tables.Count == 0 || ds.Tables[0].Rows.Count == 0) return;

//                foreach (DataRow row in ds.Tables[0].Rows)
//                {
//                    var date = Convert.ToDateTime(row["collected_day"]).Date;
//                    _dailyFinalImportedEnergyByDate[date] = Convert.ToDouble(row["total_imported"]);
//                    _dailyFinalExportedEnergyByDate[date] = Convert.ToDouble(row["total_exported"]);
//                }
//            });
//        }
        //public void UpdateDailyImportedEnergySummary(double todayDailyImportedEnergy)
        //{
        //    EnsureDailyEnergySummaryDate();
        //    _todayImportedEnergy = todayDailyImportedEnergy;
        //    RebuildDailyImportedEnergySummary();
        //}

        //public void UpdateDailyExportedEnergySummary(double todayDailyExportedEnergy)
        //{
        //    EnsureDailyEnergySummaryDate();
        //    _todayExportedEnergy = todayDailyExportedEnergy;
        //    RebuildDailyExportedEnergySummary();
        //}

        //private void EnsureDailyEnergySummaryDate()
        //{
        //    var today = DateTime.Today;
        //    if (today == _chartBaseDate) return;

        //    var previousBaseDate = _chartBaseDate.Date;
        //    _chartBaseDate = today;
        //    _dailyFinalImportedEnergyByDate[previousBaseDate] = _todayImportedEnergy;
        //    _dailyFinalExportedEnergyByDate[previousBaseDate] = _todayExportedEnergy;
        //    _todayImportedEnergy = 0d;
        //    _todayExportedEnergy = 0d;
        //    TrimDailyEnergySummaryCache();
        //}

        //private void RebuildDailyImportedEnergySummary()
        //{
        //    DailyImportedEnergySummarySeries.Clear();

        //    for (int offset = DailyEnergySummaryStartOffsetDays; offset <= DailyEnergySummaryEndOffsetDays; offset++)
        //    {
        //        var date = _chartBaseDate.AddDays(offset);
        //        DailyImportedEnergySummarySeries.Append(date, GetDailyImportedEnergyValue(date, offset));
        //    }

        //    var yesterday = _chartBaseDate.AddDays(-1);
        //    var yesterdayValue = GetDailyFinalImportedEnergy(yesterday);
        //    DailyImportedYesterdayLabel = yesterdayValue.ToString("0.0");
        //    DailyImportedYesterdayX = _chartBaseDate.AddDays(-1);
        //    DailyImportedYesterdayY = yesterdayValue;
        //    DailyImportedTodayLabel = _todayImportedEnergy.ToString("0.0");
        //    DailyImportedTodayX = _chartBaseDate;
        //    DailyImportedTodayY = _todayImportedEnergy;
        //}

        //private void RebuildDailyExportedEnergySummary()
        //{
        //    DailyExportedEnergySummarySeries.Clear();

        //    for (int offset = DailyEnergySummaryStartOffsetDays; offset <= DailyEnergySummaryEndOffsetDays; offset++)
        //    {
        //        var date = _chartBaseDate.AddDays(offset);
        //        DailyExportedEnergySummarySeries.Append(date, GetDailyExportedEnergyValue(date, offset));
        //    }

        //    var yesterday = _chartBaseDate.AddDays(-1);
        //    var yesterdayValue = GetDailyFinalExportedEnergy(yesterday);
        //    DailyExportedYesterdayLabel = yesterdayValue.ToString("0.0");
        //    DailyExportedYesterdayX = _chartBaseDate.AddDays(-1);
        //    DailyExportedYesterdayY = yesterdayValue;
        //    DailyExportedTodayLabel = _todayExportedEnergy.ToString("0.0");
        //    DailyExportedTodayX = _chartBaseDate;
        //    DailyExportedTodayY = _todayExportedEnergy;
        //}

        //private double GetDailyImportedEnergyValue(DateTime date, int offset)
        //{
        //    if (offset == 0) return _todayImportedEnergy;
        //    if (offset > 0) return 0d;

        //    return GetDailyFinalImportedEnergy(date);
        //}

        //private double GetDailyExportedEnergyValue(DateTime date, int offset)
        //{
        //    if (offset == 0) return _todayExportedEnergy;
        //    if (offset > 0) return 0d;

        //    return GetDailyFinalExportedEnergy(date);
        //}

        //private double GetDailyFinalImportedEnergy(DateTime date)
        //{
        //    double value;
        //    return _dailyFinalImportedEnergyByDate.TryGetValue(date.Date, out value) ? value : 0d;
        //}

        //private double GetDailyFinalExportedEnergy(DateTime date)
        //{
        //    double value;
        //    return _dailyFinalExportedEnergyByDate.TryGetValue(date.Date, out value) ? value : 0d;
        //}

        //private void TrimDailyEnergySummaryCache()
        //{
        //    var fromDate = _chartBaseDate.AddDays(DailyEnergySummaryStartOffsetDays).Date;
        //    var toDate = _chartBaseDate.Date;

        //    foreach (var date in _dailyFinalImportedEnergyByDate.Keys.ToList())
        //    {
        //        if (date < fromDate || date >= toDate)
        //            _dailyFinalImportedEnergyByDate.Remove(date);
        //    }

        //    foreach (var date in _dailyFinalExportedEnergyByDate.Keys.ToList())
        //    {
        //        if (date < fromDate || date >= toDate)
        //            _dailyFinalExportedEnergyByDate.Remove(date);
        //    }
        //}


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

        private static int ToBitValue(ushort bits, int bit)
        {
            return (bits & (1 << bit)) != 0 ? 1 : 0;
        }

        private static ushort ReadU16(Dictionary<string, object> parsed, string key)
        {
            if (parsed == null || !parsed.TryGetValue(key, out var raw) || raw == null)
                return 0;

            try { return Convert.ToUInt16(raw); }
            catch { return 0; }
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
                throw new ArgumentOutOfRangeException(
                    nameof(input),
                    $"[{ctrl}] write value is out of U16 range."
                );

            await _client.WriteMultipleRegistersAsync(
                spec.Address,
                new ushort[]
                {
            (ushort)raw
                }
            );
        }
        //protected async Task WriteControlU16Async(string ctrl, double input)
        //{
        //    var spec = PcsSpecs.ControlWrite[ctrl];

        //    var raw = (int)Math.Round(input / spec.Scale);
        //    if (raw < ushort.MinValue || raw > ushort.MaxValue)
        //        throw new ArgumentOutOfRangeException(nameof(input), $"[{ctrl}] write value is out of U16 range.");

        //    await _client.WriteSingleRegisterAsync(spec.Address, (ushort)raw);
        //}

        protected async Task WriteControlRawU16Async(string ctrl, ushort value)
        {
            var spec = PcsSpecs.ControlWrite[ctrl];

            // FC10: Write Multiple Registers
            await _client.WriteMultipleRegistersAsync(
                spec.Address,
                new ushort[] { value }
            );
        }
        //protected async Task WriteControlRawU16Async(string ctrl, ushort value)
        //{
        //    var spec = PcsSpecs.ControlWrite[ctrl];

        //    await _client.WriteSingleRegisterAsync(spec.Address, value);
        //}

        protected async Task WriteControlU32Async(string ctrl, double input)
        {
            var spec = PcsSpecs.ControlWrite[ctrl];

            var raw = input / spec.Scale;
            if (raw < uint.MinValue || raw > uint.MaxValue)
                throw new ArgumentOutOfRangeException(nameof(input), $"[{ctrl}] write value is out of U32 range.");

            var value = (uint)Math.Round(raw);
            // low word
            //var values = new[]
            //{
            //    (ushort)(value & 0xFFFF),
            //    (ushort)(value >> 16)
            //};
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


            if (TryGetDouble(parsed, "GridTotalImportActivePower", out var tImport))
            {
                PanelData.T_Import_Energy = tImport.ToString("0.0");
            }
            if (TryGetDouble(parsed, "GridTotalExportedActivePower", out var tExport))
            {
                PanelData.T_Export_Energy = tExport.ToString("0.0");
            }
            UpdatePcsDeviceTime(parsed);
        }

        private void UpdatePcsDeviceTime(Dictionary<string, object> parsed)
        {
            if (!TryGetDouble(parsed, "PcsTimeYear", out var yearRaw)) return;
            if (!TryGetDouble(parsed, "PcsTimeMonthDay", out var monthDayRaw)) return;
            if (!TryGetDouble(parsed, "PcsTimeHourMinute", out var hourMinuteRaw)) return;
            if (!TryGetDouble(parsed, "PcsTimeSecondMs", out var secondMsRaw)) return;

            var year = Convert.ToInt32(yearRaw);
            var monthDay = Convert.ToInt32(monthDayRaw);
            var hourMinute = Convert.ToInt32(hourMinuteRaw);
            var secondMs = Convert.ToInt32(secondMsRaw);

            var month = monthDay / 100;
            var day = monthDay % 100;
            var hour = hourMinute / 100;
            var minute = hourMinute % 100;
            var second = secondMs / 100;

            if (year < 1 || year > 9999) return;
            if (month < 1 || month > 12) return;
            if (day < 1 || day > DateTime.DaysInMonth(year, month)) return;
            if (hour < 0 || hour > 23) return;
            if (minute < 0 || minute > 59) return;
            if (second < 0 || second > 59) return;

            PanelData.PcsTime = new DateTime(year, month, day, hour, minute, second).ToString("20yy-MM-dd HH:mm:ss");
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

                GridItems[2].Value = (fuse == 0) ? "normal" : $"fault";
                if(fuse != 0) SystemMsg = faultPhase;
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
            // Grid 유효전력
            if (TryGetDouble(parsed, "GridActivePower", out var power))
            {
                GridItems[5].Value = power.ToString("0");
            }

            if (TryGetDouble(parsed, "GridVoltageAN", out var vAn)) GridItems[6].Value = vAn.ToString("0.0");
            if (TryGetDouble(parsed, "GridVoltageBN", out var vBn)) GridItems[7].Value = vBn.ToString("0.0");
            if (TryGetDouble(parsed, "GridVoltageCN", out var vCn)) GridItems[8].Value = vCn.ToString("0.0");

            if (TryGetDouble(parsed, "GridCurrentAN", out var cAn)) GridItems[9].Value = cAn.ToString("0.0");
            if (TryGetDouble(parsed, "GridCurrentBN", out var cBn)) GridItems[10].Value = cBn.ToString("0.0");
            if (TryGetDouble(parsed, "GridCurrentCN", out var cCn)) GridItems[11].Value = cCn.ToString("0.0");

            if (TryGetDouble(parsed, "GridVoltageAB", out var vAb)) GridItems[12].Value = vAb.ToString("0.0");
            if (TryGetDouble(parsed, "GridVoltageBC", out var vBc)) GridItems[13].Value = vBc.ToString("0.0");
            if (TryGetDouble(parsed, "GridVoltageCA", out var vCa)) GridItems[14].Value = vCa.ToString("0.0");

            if (TryGetDouble(parsed, "GridFrequency", out var freq)) GridItems[15].Value = freq.ToString("0.00");
            if (TryGetDouble(parsed, "GridPowerFactor", out var pf)) GridItems[16].Value = pf.ToString("0.00");
            if (TryGetDouble(parsed, "GridSurgeCounter", out var sc)) GridItems[17].Value = sc.ToString("0");
        }
        void ChangeInverterData(Dictionary<string, object> parsed)
        {
            //if (TryGetDouble(parsed, "Load_Status", out var ready)) LoadItems[1].Value = ready >= 500 ? "on" : "off";
            if (TryGetDouble(parsed, "InvFault", out var fault))
            {
                var readyBits = Convert.ToUInt16(fault);

                if ((readyBits & (1 << 0)) != 0) { InvItems[1].Value = "fault"; SystemMsg = "over inverter fault"; }// line freq
                else LoadItems[1].Value = "normal";
            }

            if (TryGetDouble(parsed, "InvTotalImportActivePower", out var tImport)) InvItems[3].Value = tImport.ToString("0.0");
            if (TryGetDouble(parsed, "InvTotalExportedActivePower", out var tExport)) InvItems[4].Value = tExport.ToString("0.0");
            if (TryGetDouble(parsed, "InvActivePower", out var p)) InvItems[5].Value = p.ToString("0");


            if (TryGetDouble(parsed, "InvVoltageAN", out var vAn)) InvItems[6].Value = vAn.ToString("0.0");
            if (TryGetDouble(parsed, "InvVoltageBN", out var vBn)) InvItems[7].Value = vBn.ToString("0.0");
            if (TryGetDouble(parsed, "InvVoltageCN", out var vCn)) InvItems[8].Value = vCn.ToString("0.0");

            if (TryGetDouble(parsed, "InvCurrentAN", out var cAn)) InvItems[9].Value = cAn.ToString("0.0");
            if (TryGetDouble(parsed, "InvCurrentBN", out var cBn)) InvItems[10].Value = cBn.ToString("0.0");
            if (TryGetDouble(parsed, "InvCurrentCN", out var cCn)) InvItems[11].Value = cCn.ToString("0.0");

            if (TryGetDouble(parsed, "InvVoltageAB", out var vAb)) InvItems[12].Value = vAb.ToString("0");
            if (TryGetDouble(parsed, "InvVoltageBC", out var vBc)) InvItems[13].Value = vBc.ToString("0");
            if (TryGetDouble(parsed, "InvVoltageCA", out var vCa)) InvItems[14].Value = vCa.ToString("0");

            if (TryGetDouble(parsed, "InvFrequency", out var freq)) InvItems[15].Value = freq.ToString("0.0");
            if (TryGetDouble(parsed, "InvPowerFactor", out var pf)) InvItems[16].Value = pf.ToString("0.00");
        }
        void ChangeLoadData(Dictionary<string, object> parsed)
        {
            //if (TryGetDouble(parsed, "Load_Status", out var ready)) LoadItems[1].Value = ready >= 500 ? "on" : "off";
            if (TryGetDouble(parsed, "LoadFault", out var fault))
            {
                var readyBits = Convert.ToUInt16(fault);

                if ((readyBits & (1 << 0)) != 0) { LoadItems[1].Value = "fault"; SystemMsg = "over load fault"; }// line freq
                else LoadItems[1].Value = "normal";
            }

            if (TryGetDouble(parsed, "LoadTotalExportedActivePower", out var tExport)) LoadItems[3].Value = tExport.ToString("0.0");
            if (TryGetDouble(parsed, "LoadActivePower", out var p)) LoadItems[4].Value = p.ToString("0");
            if (TryGetDouble(parsed, "LoadActivePowerRN", out var pRn)) LoadItems[5].Value = pRn.ToString("0");
            if (TryGetDouble(parsed, "LoadActivePowerSN", out var pSn)) LoadItems[6].Value = pSn.ToString("0");
            if (TryGetDouble(parsed, "LoadActivePowerTN", out var pTn)) LoadItems[7].Value = pTn.ToString("0");

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
        void ChangeBatteryData(Dictionary<string, object> parsed)
        {
            if (TryGetDouble(parsed, "BatteryTotalChargePower", out var tcPower)) BattItems[0].Value = tcPower.ToString("0.0");
            if (TryGetDouble(parsed, "BatteryTotalDischargePower", out var tdPower)) BattItems[1].Value = tdPower.ToString("0.0");
            if (TryGetDouble(parsed, "BatteryPower", out var power)) BattItems[2].Value = power.ToString("0.0");
            if (TryGetDouble(parsed, "BatteryVoltage", out var volt)) BattItems[3].Value = volt.ToString("0");
            if (TryGetDouble(parsed, "BatteryCurrent", out var curr)) BattItems[4].Value = curr.ToString("0.0");
        }

        void ChangeEtcData(Dictionary<string, object> parsed)
        {
            if (TryGetDouble(parsed, "InvAmbientTemperature", out var aTemp01)) EtcItems[1].Value = aTemp01.ToString("0.0");

            if (TryGetDouble(parsed, "InvHeatsinkTemperature01", out var hTemp01)) EtcItems[2].Value = hTemp01.ToString("0.0");
            if (TryGetDouble(parsed, "InvHeatsinkTemperature02", out var hTemp02)) EtcItems[3].Value = hTemp02.ToString("0.0");
            if (TryGetDouble(parsed, "InvHeatsinkTemperature03", out var hTemp03)) EtcItems[4].Value = hTemp03.ToString("0.0");
            if (TryGetDouble(parsed, "InvHeatsinkTemperature04", out var hTemp04)) EtcItems[5].Value = hTemp04.ToString("0.0");
            if (TryGetDouble(parsed, "InvHeatsinkTemperature05", out var hTemp05)) EtcItems[6].Value = hTemp05.ToString("0.0");
            if (TryGetDouble(parsed, "InvHeatsinkTemperature06", out var hTemp06)) EtcItems[7].Value = hTemp06.ToString("0.0");
            if (TryGetDouble(parsed, "InvHeatsinkTemperature07", out var hTemp07)) EtcItems[8].Value = hTemp07.ToString("0.0");
            if (TryGetDouble(parsed, "InvHeatsinkTemperature08", out var hTemp08)) EtcItems[9].Value = hTemp08.ToString("0.0");

            if (TryGetDouble(parsed, "InvLeakageCurrent", out var leak)) EtcItems[10].Value = leak.ToString("0");
            if (TryGetDouble(parsed, "InvHeartBeat", out var hb)) EtcItems[11].Value = hb.ToString("0");
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
                    ChangeGridData(snapshot);
                    ChangeInverterData(snapshot);
                    ChangeLoadData(snapshot);
                    ChangeEtcData(snapshot);
                    ChangeBatteryData(snapshot);
                    //if (TryGetDouble(snapshot, "GridTotalImportActivePower", out var totalImported))
                    //    UpdateDailyImportedEnergySummary(totalImported);
                    //if (TryGetDouble(snapshot, "GridTotalExportedActivePower", out var totalExported))
                    //    UpdateDailyExportedEnergySummary(totalExported);
                }
                finally
                {
                    Interlocked.Exchange(ref _uiUpdatePending, 0);
                }
            }));
            //SavePcsData(_latestParsed);

            SavePcsStatusIfChanged(_latestParsed);
            SavePcsGridDailyTotals(_latestParsed);
        }

        #endregion

        #region [ Database Function ]

        private void SavePcsStatusIfChanged(Dictionary<string, object> parsed)
        {
            var app = Application.Current as App;
            if (app?.DbManager == null || parsed == null) return;

            if (!parsed.ContainsKey("ReadyStatus")) return;

            var ready = ReadU16(parsed, "ReadyStatus");
            var gridFault = ReadU16(parsed, "GridFault");
            var invFault = ReadU16(parsed, "InvFault");
            var loadFault = ReadU16(parsed, "LoadFault");
            var commFault = ReadU16(parsed, "CommunicationFault");
            var gridFuse = ReadU16(parsed, "GridFuseStatus");
            var battFuse = ReadU16(parsed, "BatteryFuseStatus");
            var bypassFuse = ReadU16(parsed, "BypassFuseStatus");

            var statusGrid = ToBitValue(ready, 3);
            var statusInv = ToBitValue(ready, 2);
            var statusBatt = ToBitValue(ready, 1);
            var statusComm = ToBitValue(ready, 5);
            var statusBypass = ToBitValue(ready, 6);

            var snapshotKey = string.Join("|",
                statusGrid,
                statusInv,
                statusBatt,
                statusComm,
                statusBypass,
                gridFault,
                invFault,
                loadFault,
                commFault,
                gridFuse,
                battFuse,
                bypassFuse);

            if (snapshotKey == _lastPcsStatusSnapshotKey)
                return;

            app.DbManager.InsertPcsStatusSnapshot(
                statusGrid,
                statusInv,
                statusBatt,
                statusComm,
                statusBypass,
                gridFault,
                invFault,
                loadFault,
                commFault,
                gridFuse,
                battFuse,
                bypassFuse,
                DateTime.Now);

            _lastPcsStatusSnapshotKey = snapshotKey;
        }

        private void SavePcsGridDailyTotals(Dictionary<string, object> parsed)
        {
            var app = Application.Current as App;
            if (app?.DbManager == null || parsed == null) return;

            if (!TryGetDouble(parsed, "GridTotalImportActivePower", out var totalImported)) return;
            if (!TryGetDouble(parsed, "GridTotalExportedActivePower", out var totalExported)) return;

            app.DbManager.UpsertPcsGridDailyTotals(totalImported, totalExported, DateTime.Now);
        }

        public void SavePcsData(Dictionary<string, object> parsed)
        {
            App app = Application.Current as App;
            
            app.DbManager.InsertPcsData(parsed);
            
        }

        #endregion
    }
}
