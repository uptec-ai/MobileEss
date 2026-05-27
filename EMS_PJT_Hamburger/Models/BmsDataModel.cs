using DevExpress.Mvvm;
using EMS_PJT_Hamburger.Models.Client;
using EMS_PJT_Hamburger.Models.Client.BMS;
using EMS_PJT_Hamburger.ViewModels;
using EMS_PJT_Hamburger.Views;
using Peak.Can.Basic;
using Peak.Can.Basic.BackwardCompatibility;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace EMS_PJT_Hamburger.Models
{
    public sealed class PackSnapshot
    {
        public DateTime LastUpdateUtc { get; set; }
        public Dictionary<string, object> Fields { get; set; }
    }
    public class BmsDataModel : ViewModelBase, INotifyPropertyChanged
    {
        public App app = Application.Current as App;
        
        public Random _random = new Random();
        // random으로 UI 동작 확인
        public void ApplyRandomFaults(BMS_Status_Message02 target)
        {
            var properties = typeof(BMS_Status_Message02)
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => p.PropertyType == typeof(bool) &&
                            p.GetCustomAttribute<FaultCodeAttribute>() != null);

            foreach (var prop in properties)
            {
                bool randomValue = _random.Next(0, 2) == 1;
                prop.SetValue(target, randomValue);
            }
        }

        #region # Xpack 관련 변수 정의

        public const int PackCount = 17;
        public DispatcherTimer _uiTimer; // time마다 snap shot 동기화
        // [ Alarm ]
        public AlarmService AlarmService { get; set; }
        public bool OccurredFault { get; set; } = false;
        public int Alarms
        {
            get
            {

                return StatusMsg02.GetType().GetProperties().Where(p => p.PropertyType == typeof(bool)).Count(p => (bool)p.GetValue(StatusMsg02));
            }
        }
        public bool AlarmWindowOpen { get; set; } = true;
        public AlarmDetailWindow _alarmWin { get; set; }
        public AlarmDetailWindowViewModel CreateAlarmDetailVm() => new AlarmDetailWindowViewModel(AlarmService, Alarms);
        // [ Pack ]
        //public ObservableCollection<PackCount> PacksReady { get; } = new ObservableCollection<PackCount>();
        public ConcurrentDictionary<int, PackSnapshot> _packCache = new ConcurrentDictionary<int, PackSnapshot>();
        public ObservableCollection<PackViewModel> Packs { get; } = new ObservableCollection<PackViewModel>(); // Pack Info 
        public IReadOnlyList<KeyValuePair<uint, Dictionary<string, object>>> Snapshot { get; private set; }
        public BMS_Status_Message01 StatusMsg01 { get; } = new BMS_Status_Message01();
        public BMS_Status_Message02 StatusMsg02 { get; } = new BMS_Status_Message02();
        public BMS_Status_Message03 StatusMsg03 { get; } = new BMS_Status_Message03();
        public BMS_Status_Message04 StatusMsg04 { get; } = new BMS_Status_Message04();

        #endregion

        #region # BMS Client 관련 변수 정의

        public PcanRxService _rx;
        public CancellationTokenSource _canCts;
        public bool RelayStatus { get => GetProperty(() => RelayStatus); set { SetProperty(() => RelayStatus, value); } }
        // 바인딩 프로퍼티
        public string ReadyStatus { get => GetProperty(() => ReadyStatus); set { SetProperty(() => ReadyStatus, value); } }

        public int Soc { get => GetProperty(() => Soc); set { SetProperty(() => Soc, value); } }

        public double TotalCurrent { get => GetProperty(() => TotalCurrent); set { SetProperty(() => TotalCurrent, value); } }

        public double TotalVoltage { get => GetProperty(() => TotalVoltage); set { SetProperty(() => TotalVoltage, value); } }

        public bool FaultModuleConnLoss { get => GetProperty(() => FaultModuleConnLoss); set { SetProperty(() => FaultModuleConnLoss, value); } }

        public bool FaultInit { get => GetProperty(() => FaultInit); set{ SetProperty(() => FaultInit, value); } }

        public bool FaultCellUV { get => GetProperty(() => FaultCellUV); set { SetProperty(() => FaultCellUV, value); } }

        public bool FaultCellOV { get => GetProperty(() => FaultCellOV); set { SetProperty(() => FaultCellOV, value); } }
        public ObservableCollection<string> Logs { get; } = new ObservableCollection<string>();

        #endregion

        #region # Async Command Button 정의

        //public AsyncCommand Cmd_RelayBtn { get; set; }
        public ICommand Cmd_AlarmsPopupBtn { get; set; }
        public ICommand Cmd_RelayBtn { get; set; }

        #endregion

        #region # Function 
        public void SendRelayCommand(bool relayOn)
        {
            TPCANMsg msg = new TPCANMsg
            {
                ID = 0x180,
                MSGTYPE = TPCANMessageType.PCAN_MESSAGE_STANDARD,
                LEN = 8
            };

            msg.DATA = new byte[8];

            // Byte0 Bit0 = 1 (Relay Control Command)
            msg.DATA[0] = 0x01;

            // Byte1 = Relay Value
            msg.DATA[1] = relayOn ? (byte)1 : (byte)0; // ON : OFF

            // 나머지는 0으로 둠

            TPCANStatus result = PCANBasic.Write(PCANBasic.PCAN_USBBUS1, ref msg);

            if (result != TPCANStatus.PCAN_ERROR_OK)
            {
                // 로그 남기기
                app.nlog.Warn($"[TX] {result}");
            }
        }

        public void StatusMessage(Dictionary<string, object> parsed, uint canId)
        {
            switch (canId)
            {
                case 0x150:
                    // BMS_Status_Message01
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        StatusMsg01.Ready = parsed["BMS_Ready_Status"].ToString();
                        StatusMsg01.SOC = (double)parsed["BMS_SOC"];
                        StatusMsg01.TotalCurrent = (double)parsed["BMS_Total_Current"];
                        StatusMsg01.TotalVoltage = (double)parsed["BMS_Total_Voltage"];
                        StatusMsg01.MbmsState = (byte)parsed["MBMS_State"];
                        StatusMsg01.DispSOC = (double)parsed["BMS_Disp_SOC"];
                        app.nlog.Debug($"[ID:{canId}] Ready:{StatusMsg01.Ready}  SOC:{StatusMsg01.SOC}  Curr:{StatusMsg01.TotalCurrent}  Volt:{StatusMsg01.TotalVoltage}  State:{StatusMsg01.MbmsState}  DispSOC:{StatusMsg01.DispSOC}");
                    });
                    break;

                case 0x151:
                    // BMS_Status_Message02
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        StatusMsg02.M_Connection = (bool)parsed["Module Connection Loss Fault"];        // 0
                        StatusMsg02.C_UnderVolt = (bool)parsed["Cell Under Voltage Fault"];             // 6
                        StatusMsg02.C_OverVolt = (bool)parsed["Cell Over Voltage Fault"];               // 7
                        StatusMsg02.P_UnderVolt = (bool)parsed["Pack Under Voltage Fault"];             // 8
                        StatusMsg02.P_OverVolt = (bool)parsed["Pack Over Voltage Fault"];               // 9
                        StatusMsg02.ChargeOverCurr = (bool)parsed["Charge Over Current Fault"];         // 10
                        StatusMsg02.DischargeOverCurr = (bool)parsed["Discharge Over Current Fault"];   // 11
                        StatusMsg02.HighTemp = (bool)parsed["High Temperature Fault"];                  // 12
                        StatusMsg02.LowTemp = (bool)parsed["Low Temperature Fault"];                    // 13
                        StatusMsg02.M_TempImbal = (bool)parsed["Module Temperature Imbalance Fault"];   // 14
                        StatusMsg02.C_VoltImbal = (bool)parsed["Cell Voltage Imbalance Fault"];         // 15
                        StatusMsg02.C_UnderSOC = (bool)parsed["Cell Under SOC Fault"];                  // 17
                        StatusMsg02.PackNum = (int)parsed["Fault Pack Number"];
                        StatusMsg02.MaxCellVolt = (double)parsed["Max Cell Voltage"];
                        StatusMsg02.MaxCellVoltNum = (int)parsed["Max Cell Voltage Pack Number"];
                        StatusMsg02.Version = (double)parsed["Software Version"];
                        app.nlog.Debug($"[ID:{canId}] M_Connection:{StatusMsg02.M_Connection}  C_UnderVolt:{StatusMsg02.C_UnderVolt}  C_OverVolt:{StatusMsg02.C_OverVolt}  P_UnderVolt:{StatusMsg02.P_UnderVolt}  P_OverVolt:{StatusMsg02.P_OverVolt}  " +
                                       $"ChargeOverCurr:{StatusMsg02.DischargeOverCurr}  DischargeOverCurr:{StatusMsg02.DischargeOverCurr}  HighTemp:{StatusMsg02.HighTemp}  LowTemp:{StatusMsg02.LowTemp}  M_TempImbal:{StatusMsg02.M_TempImbal}  " +
                                       $"C_VoltImbal:{StatusMsg02.C_VoltImbal}  C_UnderSOC:{StatusMsg02.C_UnderSOC}  PackNum:{StatusMsg02.PackNum}  MaxCell:{StatusMsg02.MaxCellVolt}  MaxCellNum:{StatusMsg02.MaxCellVoltNum}  Ver:{StatusMsg02.Version}");
                    });

                    //SaveFaults(GetActiveFaults(StatusMsg02)); // true 알람 -> db저장

                    break;

                case 0x152:
                    // BMS_Status_Message03
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        StatusMsg03.MbmsReady = parsed["MBMS_Ready"].ToString();
                        StatusMsg03.PackReady = (int)parsed["Pack_Ready"];
                        StatusMsg03.MaxPackCount = (int)parsed["Max_Pack_Count"];
                        StatusMsg03.CurrentPackCount = (int)parsed["Current_Pack_Count"];
                        UpdatePacks(StatusMsg03.PackReady);

                        app.nlog.Debug($"[ID:{canId}] MbmsReady:{StatusMsg03.MbmsReady}  PackReady:{StatusMsg03.PackReady}  MaxPackCnt:{StatusMsg03.MaxPackCount}  CurrPackCnt:{StatusMsg03.CurrentPackCount}");
                    });
                    break;

                case 0x153:
                    // BMS_Status_Message04
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        StatusMsg04.CellMinVoltage = (double)parsed["Cell_Min_Voltage"] / 10000;
                        StatusMsg04.CellMinPackNumber = (int)parsed["Cell_Min_PackNum"];
                        StatusMsg04.MaxTemperature = (int)parsed["Max_Temperature"];
                        StatusMsg04.MaxTemperaturePackNumber = (int)parsed["Max_Temperature_PackNum"];
                        StatusMsg04.MinTemperature = (int)parsed["Min_Temperature"];
                        StatusMsg04.MinTemperaturePackNumber = (int)parsed["Min_Temperature_PackNum"];
                        app.nlog.Debug($"[ID:{canId}] CellMinVoltage:{StatusMsg04.CellMinVoltage}  CellMinPackNumber:{StatusMsg04.CellMinPackNumber}  MaxTemperature:{StatusMsg04.MaxTemperature}  MaxTemperaturePackNumber:{StatusMsg04.MaxTemperaturePackNumber}  " +
                                       $"MinTemperature:{StatusMsg04.MinTemperature}  MinTemperaturePackNumber:{StatusMsg04.MinTemperaturePackNumber}");
                    });
                    break;

                default:
                    break;
            }
        }

        
        public int CanIdToPackNo(uint canId)
        {
            // 0x154 -> 1, 0x155 -> 2 ... 0x170 -> 17
            if (canId < 0x154 || canId > 0x164) return 0;
            return (int)(canId - 0x154) + 1;
        }
        public void UpdatePacks(int packReady)
        {
            for (int i = 0; i < Packs.Count; i++)
            {
                Packs[i].IsOnline = (packReady & (1 << i)) != 0 ? true : false;
            }
        }
        public void SaveFaults(IEnumerable<(int Code, string Name)> faults)
        {
            App app = Application.Current as App;
            foreach (var fault in faults)
            {
                app.DbManager.InsertBmsAlarmData(fault, 0);
            }
        }

        public List<(int Code, string Name)> GetActiveFaults(BMS_Status_Message02 model)
        {
            var result = model.GetType()
                .GetProperties()
                .Where(p => Attribute.IsDefined(p, typeof(FaultCodeAttribute)))
                .Where(p => (bool)p.GetValue(model))   // true만 필터
                .Select(p =>
                {
                    var attr = (FaultCodeAttribute)Attribute
                        .GetCustomAttribute(p, typeof(FaultCodeAttribute));

                    return (attr.AlarmCode, attr.AlarmName);
                })
                .ToList();

            return result;
        }
        #endregion


        public event PropertyChangedEventHandler PropertyChanged = delegate { };
        public void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        
    }

    public class CellVM
    {
        public double Voltage { get; set; }
        public double Temperature { get; set; }
        public Brush V_Background { get; set; }
        public Brush T_Background { get; set; }
    }
    public class PackCount : BindableBase
    {
        public bool IsReady
        {
            get => GetProperty(() => IsReady);
            set => SetProperty(() => IsReady, value);
        }
    }

}
