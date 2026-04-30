using DevExpress.Mvvm;
using EMS_PJT_Hamburger.Models.Client.PCS;
using System;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace EMS_PJT_Hamburger.ViewModels
{
    public class PcsViewModel : PcsModel, IDisposable
    {
        private bool _disposed;
        private DateTime _lastKeepAliveReceivedUtc = DateTime.MinValue;

        public DelegateCommand Cmd_StartCharge { get; private set; }
        public DelegateCommand Cmd_StartDischarge { get; private set; }
        public DelegateCommand Cmd_StopPcs { get; private set; }
        public DelegateCommand Cmd_FaultReset { get; private set; }
        public DelegateCommand Cmd_EmergencyStop { get; private set; }

        public bool IsControlBusy { get => GetProperty(() => IsControlBusy); set => SetProperty(() => IsControlBusy, value); }
        public string ControlOperationMode { get => GetProperty(() => ControlOperationMode); set => SetProperty(() => ControlOperationMode, value); }
        public string ControlChargeMode { get => GetProperty(() => ControlChargeMode); set => SetProperty(() => ControlChargeMode, value); }
        public string ControlMaxChargePowerPercent { get => GetProperty(() => ControlMaxChargePowerPercent); set => SetProperty(() => ControlMaxChargePowerPercent, value); }
        public string ControlMaxDischargePowerPercent { get => GetProperty(() => ControlMaxDischargePowerPercent); set => SetProperty(() => ControlMaxDischargePowerPercent, value); }
        public string ControlMaxChargePowerW { get => GetProperty(() => ControlMaxChargePowerW); set => SetProperty(() => ControlMaxChargePowerW, value); }
        public string ControlMaxDischargePowerW { get => GetProperty(() => ControlMaxDischargePowerW); set => SetProperty(() => ControlMaxDischargePowerW, value); }
        public string ControlMaxChargeSoc { get => GetProperty(() => ControlMaxChargeSoc); set => SetProperty(() => ControlMaxChargeSoc, value); }
        public string ControlMinDischargeSoc { get => GetProperty(() => ControlMinDischargeSoc); set => SetProperty(() => ControlMinDischargeSoc, value); }
        public string ControlMaxChargeVoltage { get => GetProperty(() => ControlMaxChargeVoltage); set => SetProperty(() => ControlMaxChargeVoltage, value); }
        public string ControlMaxDischargeVoltage { get => GetProperty(() => ControlMaxDischargeVoltage); set => SetProperty(() => ControlMaxDischargeVoltage, value); }
        public string ControlMaxChargeCurrent { get => GetProperty(() => ControlMaxChargeCurrent); set => SetProperty(() => ControlMaxChargeCurrent, value); }
        public string ControlMaxDischargeCurrent { get => GetProperty(() => ControlMaxDischargeCurrent); set => SetProperty(() => ControlMaxDischargeCurrent, value); }
        public string ControlGridMaxImportPowerW { get => GetProperty(() => ControlGridMaxImportPowerW); set => SetProperty(() => ControlGridMaxImportPowerW, value); }
        public string ControlGridMaxExportPowerW { get => GetProperty(() => ControlGridMaxExportPowerW); set => SetProperty(() => ControlGridMaxExportPowerW, value); }

        public PcsViewModel()
        {
            InitControlDefaults();
            InitControlCommands();

            int.TryParse(ConfigurationManager.AppSettings["PcsPort"], out var port);
            int.TryParse(ConfigurationManager.AppSettings["PcsTimeoutMs"], out var timeoutMs);

            Conn_Settings = new ConnectionSettings()
            {
                Ip = ConfigurationManager.AppSettings["PcsHost"] ?? "172.30.1.47",
                Port = (port <= 0) ? 7000 : port,
                TimeOut = (timeoutMs <= 0) ? 10000 : timeoutMs
            };

            Conn_State = new ConnectionState()
            {
                Status = "Wait..",
                Rtt = "0",
            };

            GridItems = new ObservableCollection<DataItem>
            {
                new DataItem { Header="■ 상태"},
                new DataItem { Name="운전 상태", Value=$"off"},
                new DataItem { Name="차단기 상태", Value=$"on"},
                new DataItem { Name="퓨즈 상태", Value=$"on"},
                new DataItem { Name="SPD 상태", Value=$"on"},
                new DataItem { Name="Sourge Count 상태", Value=$"on"},
                new DataItem { Name="Fault 상태", Value=$"0"},
                new DataItem { Header="■ 정보"},
                new DataItem { Name="수전 누적 전력량", Value=$"0.0", Factor="MWh"},
                new DataItem { Name="송전 누적 전력량", Value=$"0.0", Factor="MWh"},
                new DataItem { Name="선간 전압 (AB)", Value=$"0.0", Factor="V"},
                new DataItem { Name="선간 전압 (BC)", Value=$"0.0", Factor="V"},
                new DataItem { Name="선간 전압 (CA)", Value=$"0.0", Factor="V"},
                new DataItem { Name="선간 전류 (AB)", Value=$"0.0", Factor="A"},
                new DataItem { Name="선간 전류 (BC)", Value=$"0.0", Factor="A"},
                new DataItem { Name="선간 전류 (CA)", Value=$"0.0", Factor="A"},
                new DataItem { Name="주파수", Value=$"0.00", Factor="Hz"},
                new DataItem { Name="역률", Value=$"0.00", Factor="%"},
                new DataItem { Name="Sourge Count", Value=$"0", Factor="Cyc"},
            };

            LoadItems = new ObservableCollection<DataItem>
            {
                new DataItem { Header="■ 상태"},
                new DataItem { Name="운전 상태", Value=$"off"},
                new DataItem { Name="차단기 상태", Value=$"on"},
                new DataItem { Name="퓨즈 상태", Value=$"on"},
                new DataItem { Name="SPD 상태", Value=$"on"},
                new DataItem { Name="Sourge Count 상태", Value=$"on"},
                new DataItem { Name="Fault 상태", Value=$"0"},
                new DataItem { Header="■ 정보"},
                new DataItem { Name="누적 전력량", Value=$"0.0", Factor="MWh"},
                new DataItem { Name="누적 전력량", Value=$"0.0", Factor="kWh"},
                new DataItem { Name="선간 전압 (AB)", Value=$"0.0", Factor="V"},
                new DataItem { Name="선간 전압 (BC)", Value=$"0.0", Factor="V"},
                new DataItem { Name="선간 전압 (CA)", Value=$"0.0", Factor="V"},
                new DataItem { Name="선간 전류 (AB)", Value=$"0.0", Factor="A"},
                new DataItem { Name="선간 전류 (BC)", Value=$"0.0", Factor="A"},
                new DataItem { Name="선간 전류 (CA)", Value=$"0.0", Factor="A"},
                new DataItem { Name="주파수", Value=$"0.00", Factor="Hz"},
                new DataItem { Name="역률", Value=$"0.00", Factor="%"},
                new DataItem { Name="Sourge Count", Value=$"0", Factor="Cyc"},
                //new DataItem { Name="수전 누적 전력량(일간)", Value="124.4", Factor="kWh"},
                //new DataItem { Name="송전 누적 전력량(일간)", Value="5.4", Factor="kWh"},
                //new DataItem { Name="선간전압 BC", Value="23.0", Factor="V"},
                //new DataItem { Name="선간전압 CA", Value="31.0", Factor="V"},
                //new DataItem { Name="선간전류 BC", Value="23.1", Factor="A"},
                //new DataItem { Name="선간전류 CA", Value="31.1", Factor="A"},
            };

            InvItems = new ObservableCollection<DataItem>
            {
                new DataItem { Header="■ 정보"},
                new DataItem { Name="내부온도1 (주위)", Value=$"0.0", Factor="℃"},
                new DataItem { Name="내부온도2 (주위)", Value=$"0.0", Factor="℃"},
                new DataItem { Name="내부온도3 (주위)", Value=$"0.0", Factor="℃"},
                new DataItem { Name="내부온도4 (주위)", Value=$"0.0", Factor="℃"},
                new DataItem { Name="내부온도1 (방열판)", Value=$"0.0", Factor="℃"},
                new DataItem { Name="내부온도2 (방열판)", Value=$"0.0" , Factor="℃"},
                new DataItem { Name="내부온도3 (방열판)", Value=$"0.0" , Factor="℃"},
                new DataItem { Name="내부온도4 (방열판)", Value=$"0.0" , Factor="℃"},
                new DataItem { Name="내부온도5 (방열판)", Value=$"0.0" , Factor="℃"},
                new DataItem { Name="내부온도6 (방열판)", Value=$"0.0" , Factor="℃"},
                new DataItem { Name="내부온도7 (방열판)", Value=$"0.0" , Factor="℃"},
                new DataItem { Name="내부온도8 (방열판)", Value=$"0.0" , Factor="℃"},
                new DataItem { Name="내부온도1 (IGBT)", Value=$"0.0", Factor="℃"},
                new DataItem { Name="내부온도2 (IGBT)", Value=$"0.0", Factor="℃"},
                new DataItem { Name="내부온도3 (IGBT)", Value=$"0.0", Factor="℃"},
                new DataItem { Name="내부온도4 (IGBT)", Value=$"0.0", Factor="℃"},
                new DataItem { Name="내부온도5 (IGBT)", Value=$"0.0", Factor="℃"},
                new DataItem { Name="내부온도6 (IGBT)", Value=$"0.0", Factor="℃"},
                new DataItem { Name="내부온도7 (IGBT)", Value=$"0.0", Factor="℃"},
                new DataItem { Name="내부온도8 (IGBT)", Value=$"0.0", Factor="℃"},
                new DataItem { Name="내부온도9 (IGBT)", Value=$"0.0", Factor="℃"},
                new DataItem { Name="내부온도10 (IGBT)", Value=$"0.0", Factor="℃"},
                new DataItem { Name="내부온도11 (IGBT)", Value=$"0.0", Factor="℃"},
                new DataItem { Name="내부온도12 (IGBT)", Value=$"0.0", Factor="℃"},
            };

            _ = ConnectAsync();
        }

        private void InitControlDefaults()
        {
            ControlOperationMode = "7";
            ControlChargeMode = "1";
            ControlMaxChargePowerPercent = "100";
            ControlMaxDischargePowerPercent = "100";
            ControlMaxChargePowerW = "0";
            ControlMaxDischargePowerW = "0";
            ControlMaxChargeSoc = "90";
            ControlMinDischargeSoc = "10";
            ControlMaxChargeVoltage = "0";
            ControlMaxDischargeVoltage = "0";
            ControlMaxChargeCurrent = "0";
            ControlMaxDischargeCurrent = "0";
            ControlGridMaxImportPowerW = "0";
            ControlGridMaxExportPowerW = "0";
        }

        private void InitControlCommands()
        {
            Cmd_StartCharge = new DelegateCommand(async () => await ExecuteControlSequenceAsync("충전 시작", StartChargeSequenceAsync));
            Cmd_StartDischarge = new DelegateCommand(async () => await ExecuteControlSequenceAsync("방전 시작", StartDischargeSequenceAsync));
            Cmd_StopPcs = new DelegateCommand(async () => await ExecuteControlSequenceAsync("정지", StopSequenceAsync));
            Cmd_FaultReset = new DelegateCommand(async () => await ExecuteControlSequenceAsync("Fault Reset", FaultResetSequenceAsync));
            Cmd_EmergencyStop = new DelegateCommand(async () => await ExecuteControlSequenceAsync("비상정지", EmergencyStopSequenceAsync));
        }

        private async Task ExecuteControlSequenceAsync(string name, Func<Task> sequence)
        {
            if (IsControlBusy) return;

            try
            {
                IsControlBusy = true;
                IsTransmit = true;
                SystemMsg = $"[PCS] {name} sequence start.";

                await sequence();
                var statusMessage = await ReadControlStatusMessageAsync();

                SystemMsg = $"[PCS] {name} sequence complete. {statusMessage}";
            }
            catch (Exception ex)
            {
                SystemMsg = $"[E] {name} failed: {ex.Message}";
            }
            finally
            {
                IsTransmit = false;
                IsControlBusy = false;
            }
        }

        private async Task StartChargeSequenceAsync()
        {
            await WriteModeAsync(); // 1000 -> 7, 1001 -> 1
            await WriteChargeParametersAsync(); // 현재 control setting write
            await WriteControlRawU16Async("ChargeDischargeStart", 0x004A); // 1003 -> 74
            await WriteControlRawU16Async("RunStop", 1); // 1002 -> 1
        }

        private async Task StartDischargeSequenceAsync()
        {
            await WriteModeAsync(); // 1000 -> 7, 1001 -> 1
            await WriteDischargeParametersAsync(); // 현재 control setting write
            await WriteControlRawU16Async("ChargeDischargeStart", 0x0044); // 1003 -> 68
            await WriteControlRawU16Async("RunStop", 1); // 1002 -> 1
        }

        private async Task StopSequenceAsync()
        {
            await WriteControlRawU16Async("ChargeDischargeStart", 0); // 1003 -> 0
            await WriteControlRawU16Async("RunStop", 0); // 1002 -> 0
        }

        private async Task FaultResetSequenceAsync()
        {
            await StopSequenceAsync();
            await WriteControlRawU16Async("FaultReset", 1); // 1024 -> 1
            await Task.Delay(200);
            await WriteControlRawU16Async("FaultReset", 0); // 1024 -> 0
        }

        private async Task EmergencyStopSequenceAsync()
        {
            await WriteControlRawU16Async("EmergencyFault", 1); // 1004 -> 1
            await StopSequenceAsync();
        }

        private async Task<string> ReadControlStatusMessageAsync()
        {
            var values = await _client.ReadHoldingRegistersAsync(1, 148, 2, CancellationToken.None);
            if (values == null || values.Length < 2) return "status read failed.";

            return $"Status(148)={values[0]}, Fault(149)={values[1]}";
        }

        private async Task WriteModeAsync()
        {
            await WriteControlRawU16Async("OperationMode", ParseControlU16(ControlOperationMode, "기동모드")); // 1000 -> 7 (원격, 매뉴얼, 단독 운전)
            await WriteControlRawU16Async("ChargeMode", ParseControlU16(ControlChargeMode, "충방전 모드")); // 1001 -> 1 (CP)
        }

        private async Task WriteChargeParametersAsync()
        {
            await WriteControlU16Async("MaxChargePowerPercent", ParseControlDouble(ControlMaxChargePowerPercent, "최대 충전 전력(%)"));
            await WriteControlU32Async("MaxChargePower", ParseControlDouble(ControlMaxChargePowerW, "최대 충전 전력(W)"));
            await WriteControlU16Async("MaxChargeSOC", ParseControlDouble(ControlMaxChargeSoc, "최대 충전 SOC(%)"));
            await WriteControlU16Async("MaxChargeVoltage", ParseControlDouble(ControlMaxChargeVoltage, "최대 충전 전압(V)"));
            await WriteControlU16Async("MaxChargeCurrent", ParseControlDouble(ControlMaxChargeCurrent, "최대 충전 전류(A)"));
            await WriteControlU32Async("GridMaxImportPower", ParseControlDouble(ControlGridMaxImportPowerW, "Grid 최대 수전 전력(W)"));
        }

        private async Task WriteDischargeParametersAsync()
        {
            await WriteControlU16Async("MaxDischargePowerPercent", ParseControlDouble(ControlMaxDischargePowerPercent, "최대 방전 전력(%)"));
            await WriteControlU32Async("MaxDischargePower", ParseControlDouble(ControlMaxDischargePowerW, "최대 방전 전력(W)"));
            await WriteControlU16Async("MinDischargeSOC", ParseControlDouble(ControlMinDischargeSoc, "최소 방전 SOC(%)"));
            await WriteControlU16Async("MaxDischargeVoltage", ParseControlDouble(ControlMaxDischargeVoltage, "최대 방전 전압(V)"));
            await WriteControlU16Async("MaxDischargeCurrent", ParseControlDouble(ControlMaxDischargeCurrent, "최대 방전 전류(A)"));
            await WriteControlU32Async("GridMaxExportPower", ParseControlDouble(ControlGridMaxExportPowerW, "Grid 최대 송전 전력(W)"));
        }

        private static ushort ParseControlU16(string text, string fieldName)
        {
            if (string.IsNullOrWhiteSpace(text))
                throw new ArgumentException($"{fieldName} 값을 입력하세요.");

            var valueText = text.Trim();
            if (valueText.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
            {
                return Convert.ToUInt16(valueText.Substring(2), 16);
            }

            if (ushort.TryParse(valueText, NumberStyles.Integer, CultureInfo.CurrentCulture, out var value))
                return value;

            throw new ArgumentException($"{fieldName} 값이 올바르지 않습니다.");
        }

        private static double ParseControlDouble(string text, string fieldName)
        {
            if (string.IsNullOrWhiteSpace(text))
                throw new ArgumentException($"{fieldName} 값을 입력하세요.");

            if (double.TryParse(text.Trim(), NumberStyles.Float, CultureInfo.CurrentCulture, out var value))
                return value;

            if (double.TryParse(text.Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out value))
                return value;

            throw new ArgumentException($"{fieldName} 값이 올바르지 않습니다.");
        }

        public async Task ConnectAsync()
        {
            if (_disposed) return;

            try
            {
                _client.ConnectionStateChanged -= OnConnectionChanged;
                _client.KeepAliveHoldingReceived -= OnKeepAliveHoldingReceived;
                _client.InputRegistersReceived -= OnInputRegistersReceived;

                _client.ConnectionStateChanged += OnConnectionChanged; // client 연결상태 이벤트
                _client.KeepAliveHoldingReceived += OnKeepAliveHoldingReceived; // keep-alive 수신 이벤트
                _client.InputRegistersReceived += OnInputRegistersReceived; // 데이터 수신 이벤트

                _client.Configure(
                    Conn_Settings.Ip,
                    Conn_Settings.Port,
                    slaveId: 1,
                    timeoutMs: Conn_Settings.TimeOut,
                    pollStartAddress: PollStartAddress,
                    pollCount: PollRegisterCount,
                    pollInterval: PollInterval,
                    keepAliveStartAddress: 256,
                    keepAliveCount: 1,
                    keepAliveInterval: TimeSpan.FromSeconds(1),
                    minBackoff: TimeSpan.FromSeconds(1),
                    maxBackoff: TimeSpan.FromSeconds(15));

                await InitializeDailyImportedEnergySummaryAsync();

                await _client.StartAsync();
                Conn_State.Status = "Connecting..";

            }
            catch (OperationCanceledException)
            {
                // 정상 취소
            }
            catch (Exception ex)
            {
                Conn_State.Status = "Disconnected";
                SystemMsg = $"[E] connect failed: {ex.Message}";
            }
        }

        public void OnKeepAliveHoldingReceived(object sender, HoldingRegistersEventArgs e)
        {
            if (_disposed) return;
            if (e?.Values == null || e.Values.Length == 0) return;

            var now = DateTime.UtcNow;
            var previous = _lastKeepAliveReceivedUtc;
            _lastKeepAliveReceivedUtc = now;

            var intervalMs = (previous == DateTime.MinValue) ? 0 : (now - previous).TotalMilliseconds;

            void update()
            {
                IsReceive = true;
                Conn_State.Rtt = (intervalMs > 0) ? $"{intervalMs:0} ms" : "-";

                KeepAliveRegisters.Clear();
                for (int i = 0; i < e.Values.Length; i++)
                {
                    KeepAliveRegisters.Add(new RegisterItem
                    {
                        Address = e.StartAddress + i,
                        Value = e.Values[i].ToString()
                    });
                }

                SystemMsg = $"[KA] keep-alive ok ({e.Values[0]})";
            }

            if (Dispatcher != null) Dispatcher.BeginInvoke(update);
            else Application.Current?.Dispatcher?.BeginInvoke((Action)update);
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;

            try
            {
                _client.ConnectionStateChanged -= OnConnectionChanged;
                _client.KeepAliveHoldingReceived -= OnKeepAliveHoldingReceived;
                _client.InputRegistersReceived -= OnInputRegistersReceived;
            }
            catch
            {
                // 종료 경로에서는 무시
            }

            DisposeModelResources();
        }
    }
}
