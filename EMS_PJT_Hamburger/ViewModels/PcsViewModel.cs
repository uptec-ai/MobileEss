using DevExpress.Mvvm;
using EMS_PJT_Hamburger.Models.Client.PCS;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using static EMS_PJT_Hamburger.Models.Client.PCS.ModbusService;

namespace EMS_PJT_Hamburger.ViewModels
{
    public class PcsViewModel : PcsModel
    {
        public class DataItem
        {
            public string Header { get; set; }
            public string Name { get; set; }
            public string Value { get; set; }
            public string Factor { get; set; }

        }
        
        #region Grid Data Set
        public ObservableCollection<DataItem> GridItems { get; set; } = new ObservableCollection<DataItem>
        {
            new DataItem { Header="◆ 상태"},
            new DataItem { Name="운전 상태", Value="off"},
            new DataItem { Name="차단기 상태", Value="on"},
            new DataItem { Name="퓨즈 상태", Value="on"},
            new DataItem { Name="SPD 상태", Value="on"},
            new DataItem { Name="Sourge Count 상태", Value="on"},
            new DataItem { Name="Fault 상태", Value="0"},
            new DataItem { Header="◆ 정보"},
            new DataItem { Name="수전 누적 전력량", Value="12.4", Factor="MWh"},
            new DataItem { Name="송전 누적 전력량", Value="8.6", Factor="MWh"},
            //new DataItem { Name="수전 누적 전력량(일간)", Value="124.4", Factor="kWh"},
            //new DataItem { Name="송전 누적 전력량(일간)", Value="5.4", Factor="kWh"},
            new DataItem { Name="선간 전압", Value="12.0", Factor="V"},
            //new DataItem { Name="선간전압 BC", Value="23.0", Factor="V"},
            //new DataItem { Name="선간전압 CA", Value="31.0", Factor="V"},
            new DataItem { Name="선간 전류", Value="12.1", Factor="A"},
            //new DataItem { Name="선간전류 BC", Value="23.1", Factor="A"},
            //new DataItem { Name="선간전류 CA", Value="31.1", Factor="A"},
            new DataItem { Name="주파수", Value="60.00", Factor="Hz"},
            new DataItem { Name="역률", Value="1.00", Factor="%"},
            new DataItem { Name="Sourge Count", Value="30", Factor="Cyc"},
        };
        #endregion

        #region Load Data Set
        public ObservableCollection<DataItem> LoadItems { get; set; } = new ObservableCollection<DataItem>
        {
            new DataItem { Header="◆ 상태"},
            new DataItem { Name="   운전 상태", Value="off"},
            new DataItem { Name="   차단기 상태", Value="on"},
            new DataItem { Name="   퓨즈 상태", Value="on"},
            new DataItem { Name="   SPD 상태", Value="on"},
            new DataItem { Name="   Sourge Count 상태", Value="on"},
            new DataItem { Name="   Fault 상태", Value="0"},
            new DataItem { Header="◆ 정보"},
            new DataItem { Name="   누적 전력량", Value="12.4", Factor="MWh"},
            new DataItem { Name="   누적 전력량", Value="8.6", Factor="MWh"},
            //new DataItem { Name="수전 누적 전력량(일간)", Value="124.4", Factor="kWh"},
            //new DataItem { Name="송전 누적 전력량(일간)", Value="5.4", Factor="kWh"},
            new DataItem { Name="   선간 전압", Value="12.0", Factor="V"},
            //new DataItem { Name="선간전압 BC", Value="23.0", Factor="V"},
            //new DataItem { Name="선간전압 CA", Value="31.0", Factor="V"},
            new DataItem { Name="   선간 전류", Value="12.1", Factor="A"},
            //new DataItem { Name="선간전류 BC", Value="23.1", Factor="A"},
            //new DataItem { Name="선간전류 CA", Value="31.1", Factor="A"},
            new DataItem { Name="   주파수", Value="60.00", Factor="Hz"},
            new DataItem { Name="   역률", Value="1.00", Factor="%"},
            new DataItem { Name="   Sourge Count", Value="30", Factor="Cyc"},
        };
        #endregion

        #region Inverter Data Set

        public ObservableCollection<DataItem> InvItems { get; set; } = new ObservableCollection<DataItem>
        {
            new DataItem { Header="◆ 상태"},
            new DataItem { Name="   PCS 동작 모드", Value="Off"},        // Off / Charge / Discharge
            new DataItem { Name="   운전 상태", Value="Stop"},           // Run / Stop / Fault / Ready
            new DataItem { Name="   냉각팬 상태", Value="Stop"},         // Run / Stop / Fault / Ready
            new DataItem { Name="   릴레이 상태", Value="Stop"},         // Run / Stop / Fault / Ready
            new DataItem { Name="   접촉기 상태", Value="Stop"},         // Run / Stop / Fault / Ready
            new DataItem { Header="◆ 정보"},
            new DataItem { Name="   출력 전력 (kW)", Value="60.00", Factor="kW"},
            new DataItem { Name="   출력 전류 (A)", Value="+125", Factor="A"},
            new DataItem { Name="   DC 입력 전압 (V)", Value="12.45", Factor="V"},
            new DataItem { Name="   DC 입력 전류 (A)", Value="8.96", Factor="A"},
            new DataItem { Name="   DC Link 전압", Value="55.0", Factor="V"},
            new DataItem { Name="   내부 온도", Value="55.0", Factor="℃"},
            new DataItem { Name="   변환 효율", Value="60", Factor="%"},
            new DataItem { Name="   출력 역률", Value="Stop"},
            new DataItem { Name="   AC 변환 전력", Value="60.00", Factor="kW"},
        };

        #endregion

        public PcsViewModel()
        {
            Conn_Settings = new ConnectionSettings()
            {
                Ip = "172.30.1.47",
                Port = 7000,
                TimeOut = 10000
            };

            Conn_State = new ConnectionState()
            {
                Status = "Wait..",
                Rtt = "0",
            };

            _ = ConnectAsync();
        }
        public async Task ConnectAsync()
        {
            _cts = new CancellationTokenSource();

            await _client.ConnectAsync(Conn_Settings.Ip, Conn_Settings.Port, 10000, _cts.Token);
            _client.ConnectionStateChanged += OnConnectionChanged; // 연결상태 확인
            _client.KeepAliveHoldingReceived += OnKeepAliveHoldingReceived; // UI 업데이트

            _ = StartPolling();
        }

        public void OnKeepAliveHoldingReceived(object sender, HoldingRegistersEventArgs e)
        {
            // 컬렉션 갱신은 UI 스레드에서
            void update()
            {
                KeepAliveRegisters.Clear();
                //for (int i = 0; i < e.Values.Length; i++)
                //    KeepAliveRegisters.Add(new RegisterItem { Address = e.StartAddress + i, Value = e.Values[i].ToString("X2") });

                //Conn_State.Status = $"KeepAlive Read {e.Values.Length} @ {e.StartAddress} (UTC {e.Timestamp:HH:mm:ss})";
            }

            if (Dispatcher != null) Dispatcher.BeginInvoke(update);
            else Application.Current?.Dispatcher?.BeginInvoke((Action)update);
        }
    }
}
