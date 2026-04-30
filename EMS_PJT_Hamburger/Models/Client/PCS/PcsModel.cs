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
    public class PcsModel : ViewModelBase, INotifyPropertyChanged
    {
        public ModbusService _client = new ModbusService();
        public ConnectionSettings Conn_Settings { get; set; }
        public ConnectionState Conn_State { get; set; }
        public ObservableCollection<RegisterItem> KeepAliveRegisters { get; } = new ObservableCollection<RegisterItem>();
        protected const ushort PollStartAddress = 4;
        protected const ushort PollRegisterCount = 355;
        protected static readonly TimeSpan PollInterval = TimeSpan.FromMilliseconds(500);

        // UI Data Init
        public PCS_PanelData PanelData { get; set; } = new PCS_PanelData()
        {
            Status = "정지",
            AlarmCnt = "0",
            T_Import_Energy = "0.0",
            T_Export_Energy = "0.0",
            D_Import_Energy = "0.0",
            D_Export_Energy = "0.0",
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

        public ObservableCollection<DataItem> _invItems { get; set; }
        public ObservableCollection<DataItem> InvItems
        {
            get => _invItems;
            set
            {
                _invItems = value;
                OnPropertyChanged(nameof(InvItems));
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
                    PanelData.Status = connected ? "동작" : "정지";
                    GridItems[1].Value = connected ? "on" : "off";
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

        void ChangePanelData(Dictionary<string, object> parsed)
        {
            if (TryGetDouble(parsed, "Grid_Status", out var ready)) { PanelData.Status = ready != 0 ? "동작" : "정지"; }
            if (TryGetDouble(parsed, "Grid_CB", out var cnt)) PanelData.AlarmCnt = cnt.ToString(); ;
            if (TryGetDouble(parsed, "Grid_Total_ImportedEnergy", out var tImport)) PanelData.T_Import_Energy = tImport.ToString("0.0");
            if (TryGetDouble(parsed, "Grid_Total_ExportedEnergy", out var tExport)) PanelData.T_Export_Energy = tExport.ToString("0.0");
            if (TryGetDouble(parsed, "Grid_Daily_ImportedEnergy", out var dImport)) PanelData.D_Import_Energy = dImport.ToString("0.0");
            if (TryGetDouble(parsed, "Grid_Daily_ExportedEnergy", out var dExport)) PanelData.D_Export_Energy = dExport.ToString("0.0");
        }
        void ChangeInverterData(Dictionary<string, object> parsed)
        {
            double volt = 0;
            double curr = 0;
            if (TryGetDouble(parsed, "Inv_Daily_ImportedEnergy", out var dImport)) { InvData.D_ImportEnergy = dImport.ToString("0.0"); }
            if (TryGetDouble(parsed, "Inv_Daily_ExportedEnergy", out var dExport)) { InvData.D_ExportEnergy = dExport.ToString("0.0"); }
            if (TryGetDouble(parsed, "Inv_ActivePower", out var power)) { InvData.Power = power.ToString("0.0"); }
            if (TryGetDouble(parsed, "Inv_Volt_AB", out var vab)) volt += vab;
            if (TryGetDouble(parsed, "Inv_Volt_BC", out var vbc)) volt += vbc;
            if (TryGetDouble(parsed, "Inv_Volt_CA", out var vca)) volt += vca;
            if (TryGetDouble(parsed, "Inv_Curr_AB", out var cab)) curr += cab;
            if (TryGetDouble(parsed, "Inv_Curr_BC", out var cbc)) curr += cbc;
            if (TryGetDouble(parsed, "Inv_Curr_BC", out var cca)) curr += cca;

            InvData.VoltAverage = (volt / 3).ToString("0.0");
            InvData.CurrAverage = (curr / 3).ToString("0.0");

        }
        void ChangeGridData(Dictionary<string, object> parsed)
        {
            if (TryGetDouble(parsed, "Grid_Status", out var ready)) { GridItems[1].Value = IsConnected ? "on" : "off"; }
            if (TryGetDouble(parsed, "Grid_CB", out var cb)) GridItems[2].Value = cb >= 500 ? "on" : "off";
            if (TryGetDouble(parsed, "Grid_Fuse", out var fuse)) GridItems[3].Value = fuse >= 500 ? "on" : "off";
            if (TryGetDouble(parsed, "Grid_SPD", out var spd)) GridItems[4].Value = spd >= 500 ? "on" : "off";
            if (TryGetDouble(parsed, "Grid_SC", out var sc)) GridItems[5].Value = sc >= 500 ? "on" : "off";
            if (TryGetDouble(parsed, "Grid_Fault", out var fault)) GridItems[6].Value = fault.ToString();

            if (TryGetDouble(parsed, "Grid_Total_ImportedEnergy", out var tImport)) GridItems[8].Value = tImport.ToString("0.0");
            if (TryGetDouble(parsed, "Grid_Total_ExportedEnergy", out var tExport)) GridItems[9].Value = tExport.ToString("0.0");
            if (TryGetDouble(parsed, "Grid_Volt_AB", out var vAb)) GridItems[10].Value = vAb.ToString("0.0");
            if (TryGetDouble(parsed, "Grid_Volt_BC", out var vBc)) GridItems[11].Value = vBc.ToString("0.0");
            if (TryGetDouble(parsed, "Grid_Volt_CA", out var vCa)) GridItems[12].Value = vCa.ToString("0.0");
            if (TryGetDouble(parsed, "Grid_Curr_AB", out var cAb)) GridItems[13].Value = cAb.ToString("0.0");
            if (TryGetDouble(parsed, "Grid_Curr_BC", out var cBc)) GridItems[14].Value = cBc.ToString("0.0");
            if (TryGetDouble(parsed, "Grid_Curr_CA", out var cCa)) GridItems[15].Value = cCa.ToString("0.0");
            if (TryGetDouble(parsed, "Grid_Freq", out var freq)) GridItems[16].Value = freq.ToString("0.00");
            if (TryGetDouble(parsed, "Grid_PF", out var pf)) GridItems[17].Value = pf.ToString("0.00");
            if (TryGetDouble(parsed, "Grid_SC_Cnt", out var scCnt)) GridItems[18].Value = scCnt.ToString("0");
        }
        void ChangeLoadData(Dictionary<string, object> parsed)
        {
            if (TryGetDouble(parsed, "Load_Status", out var ready)) LoadItems[1].Value = ready >= 500 ? "on" : "off";
            if (TryGetDouble(parsed, "Load_CB", out var cb)) LoadItems[2].Value = cb >= 500 ? "on" : "off";
            if (TryGetDouble(parsed, "Load_Fuse", out var fuse)) LoadItems[3].Value = fuse >= 500 ? "on" : "off";
            if (TryGetDouble(parsed, "Load_SPD", out var spd)) LoadItems[4].Value = spd >= 500 ? "on" : "off";
            if (TryGetDouble(parsed, "Load_SC", out var sc)) LoadItems[5].Value = sc >= 500 ? "on" : "off";
            if (TryGetDouble(parsed, "Load_Fault", out var fault)) LoadItems[6].Value = fault.ToString();

            if (TryGetDouble(parsed, "Load_Total_Energy", out var tImport)) LoadItems[8].Value = tImport.ToString("0.0");
            if (TryGetDouble(parsed, "Load_Daily_Energy", out var tExport)) LoadItems[9].Value = tExport.ToString("0.0");
            if (TryGetDouble(parsed, "Load_Volt_AB", out var vAb)) LoadItems[10].Value = vAb.ToString("0.0");
            if (TryGetDouble(parsed, "Load_Volt_BC", out var vBc)) LoadItems[11].Value = vBc.ToString("0.0");
            if (TryGetDouble(parsed, "Load_Volt_CA", out var vCa)) LoadItems[12].Value = vCa.ToString("0.0");
            if (TryGetDouble(parsed, "Load_Curr_AB", out var cAb)) LoadItems[13].Value = cAb.ToString("0.0");
            if (TryGetDouble(parsed, "Load_Curr_BC", out var cBc)) LoadItems[14].Value = cBc.ToString("0.0");
            if (TryGetDouble(parsed, "Load_Curr_CA", out var cCa)) LoadItems[15].Value = cCa.ToString("0.0");
            if (TryGetDouble(parsed, "Load_Freq", out var freq)) LoadItems[16].Value = freq.ToString("0.00");
            if (TryGetDouble(parsed, "Load_PF", out var pf)) LoadItems[17].Value = pf.ToString("0.00");
            if (TryGetDouble(parsed, "Load_SC_Cnt", out var scCnt)) LoadItems[18].Value = scCnt.ToString("0");
        }
        void ChangeEtcData(Dictionary<string, object> parsed)
        {
            if (TryGetDouble(parsed, "INV_Ambient_Temp01", out var aTemp01)) InvItems[1].Value = aTemp01.ToString();
            if (TryGetDouble(parsed, "INV_Ambient_Temp02", out var aTemp02)) InvItems[2].Value = aTemp02.ToString();
            if (TryGetDouble(parsed, "INV_Ambient_Temp03", out var aTemp03)) InvItems[3].Value = aTemp03.ToString();
            if (TryGetDouble(parsed, "INV_Ambient_Temp04", out var aTemp04)) InvItems[4].Value = aTemp04.ToString();

            if (TryGetDouble(parsed, "INV_Heatsink_Temp01", out var hTemp01)) InvItems[5].Value = hTemp01.ToString();
            if (TryGetDouble(parsed, "INV_Heatsink_Temp02", out var hTemp02)) InvItems[6].Value = hTemp02.ToString();
            if (TryGetDouble(parsed, "INV_Heatsink_Temp03", out var hTemp03)) InvItems[7].Value = hTemp03.ToString();
            if (TryGetDouble(parsed, "INV_Heatsink_Temp04", out var hTemp04)) InvItems[8].Value = hTemp04.ToString();
            if (TryGetDouble(parsed, "INV_Heatsink_Temp05", out var hTemp05)) InvItems[9].Value = hTemp05.ToString();
            if (TryGetDouble(parsed, "INV_Heatsink_Temp06", out var hTemp06)) InvItems[10].Value = hTemp06.ToString();
            if (TryGetDouble(parsed, "INV_Heatsink_Temp07", out var hTemp07)) InvItems[11].Value = hTemp07.ToString();
            if (TryGetDouble(parsed, "INV_Heatsink_Temp08", out var hTemp08)) InvItems[12].Value = hTemp08.ToString();

            if (TryGetDouble(parsed, "INV_IGBT_Temp01", out var iTemp01)) InvItems[13].Value = iTemp01.ToString();
            if (TryGetDouble(parsed, "INV_IGBT_Temp02", out var iTemp02)) InvItems[14].Value = iTemp02.ToString();
            if (TryGetDouble(parsed, "INV_IGBT_Temp03", out var iTemp03)) InvItems[15].Value = iTemp03.ToString();
            if (TryGetDouble(parsed, "INV_IGBT_Temp04", out var iTemp04)) InvItems[16].Value = iTemp04.ToString();
            if (TryGetDouble(parsed, "INV_IGBT_Temp05", out var iTemp05)) InvItems[17].Value = iTemp05.ToString();
            if (TryGetDouble(parsed, "INV_IGBT_Temp06", out var iTemp06)) InvItems[18].Value = iTemp06.ToString();
            if (TryGetDouble(parsed, "INV_IGBT_Temp07", out var iTemp07)) InvItems[19].Value = iTemp07.ToString();
            if (TryGetDouble(parsed, "INV_IGBT_Temp08", out var iTemp08)) InvItems[20].Value = iTemp08.ToString();
            if (TryGetDouble(parsed, "INV_IGBT_Temp09", out var iTemp09)) InvItems[21].Value = iTemp09.ToString();
            if (TryGetDouble(parsed, "INV_IGBT_Temp10", out var iTemp10)) InvItems[22].Value = iTemp10.ToString();
            if (TryGetDouble(parsed, "INV_IGBT_Temp11", out var iTemp11)) InvItems[23].Value = iTemp11.ToString();
            if (TryGetDouble(parsed, "INV_IGBT_Temp12", out var iTemp12)) InvItems[24].Value = iTemp12.ToString();
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
