using DevExpress.Mvvm;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMS_PJT_Hamburger.Models.Client.PCS
{

    public static class PcsSpecs
    {
        public static readonly IList<ModbusFieldSpec> GridData = new List<ModbusFieldSpec>
        {
            new ModbusFieldSpec { Address = 5, DataType = ModbusDataType.U32, Name = "Grid_Total_ImportedEnergy", Scale = 0.0001, Unit = "MWh", },     // Grid 수전 누적 전력량
            new ModbusFieldSpec { Address = 7, DataType = ModbusDataType.U32, Name = "Grid_Total_ExportedEnergy", Scale = 0.0001, Unit = "MWh", },     // Grid 송전 누적 전력량
            new ModbusFieldSpec { Address = 8 , DataType = ModbusDataType.U16, Name = "Grid_Daily_ImportedEnergy", Scale = 0.1, Unit = "kWh", },     // Grid 수전 누적 전력량 (일간)
            new ModbusFieldSpec { Address = 9, DataType = ModbusDataType.U16, Name = "Grid_Daily_ExportedEnergy", Scale = 0.1, Unit = "kWh", },     // Grid 송전 누적 전력량 (일간)
            new ModbusFieldSpec { Address = 24, DataType = ModbusDataType.U16, Name = "Grid_Power", Scale = 0.1, Unit = "V", },                // Grid 선간 전압 (A-B)
            new ModbusFieldSpec { Address = 30, DataType = ModbusDataType.U16, Name = "Grid_Volt_AB", Scale = 0.1, Unit = "V", },                // Grid 선간 전압 (A-B)
            new ModbusFieldSpec { Address = 31, DataType = ModbusDataType.U16, Name = "Grid_Volt_BC", Scale = 0.1, Unit = "V", },                // Grid 선간 전압 (B-C)
            new ModbusFieldSpec { Address = 32, DataType = ModbusDataType.U16, Name = "Grid_Volt_CA", Scale = 0.1, Unit = "V", },                // Grid 선간 전압 (C-A)
            new ModbusFieldSpec { Address = 39, DataType = ModbusDataType.S16, Name = "Grid_Curr_AB", Scale = 0.1, Unit = "A", },                // Grid 선간 전류 (A-B)
            new ModbusFieldSpec { Address = 40, DataType = ModbusDataType.S16, Name = "Grid_Curr_BC", Scale = 0.1, Unit = "A", },                // Grid 선간 전류 (B-C)
            new ModbusFieldSpec { Address = 41, DataType = ModbusDataType.S16, Name = "Grid_Curr_CA", Scale = 0.1, Unit = "A", },                // Grid 선간 전류 (C-A)
            new ModbusFieldSpec { Address = 82, DataType = ModbusDataType.U16, Name = "Grid_Freq", Scale = 0.01, Unit = "Hz", },               // Grid 주파수
            new ModbusFieldSpec { Address = 83, DataType = ModbusDataType.S16, Name = "Grid_PF", Scale = 0.01, Unit = "%", }               // Grid 역률
        };

        public static readonly IList<ModbusFieldSpec> InvData = new List<ModbusFieldSpec>
        {
            new ModbusFieldSpec { Address = 58, DataType = ModbusDataType.U16, Name = "Inv_Daily_ImportedEnergy", Scale = 0.1, Unit = "V", }, // Inv 수전 누적 전력량 (일간)
            new ModbusFieldSpec { Address = 59, DataType = ModbusDataType.U16, Name = "Inv_Daily_ExportedEnergy", Scale = 0.1, Unit = "V", }, // Inv 송전 누적 전력량 (일간)
            new ModbusFieldSpec { Address = 74, DataType = ModbusDataType.S32, Name = "Inv_ActivePower", Scale = 0.0001, Unit = "kW", },      // Inverter 유효전력
            new ModbusFieldSpec { Address = 81, DataType = ModbusDataType.U16, Name = "Inv_Volt_AB", Scale = 0.1, Unit = "V", },              // Inv 선간전압 (A-B)
            new ModbusFieldSpec { Address = 82, DataType = ModbusDataType.U16, Name = "Inv_Volt_BC", Scale = 0.1, Unit = "V", },              // Inv 선간전압 (B-C)
            new ModbusFieldSpec { Address = 83, DataType = ModbusDataType.U16, Name = "Inv_Volt_CA", Scale = 0.1, Unit = "V", },              // Inv 선간전압 (C-A)
            new ModbusFieldSpec { Address = 87, DataType = ModbusDataType.U16, Name = "Inv_Curr_AB", Scale = 0.1, Unit = "A", },              // Inv 선간전류 (A-B) 
            new ModbusFieldSpec { Address = 88, DataType = ModbusDataType.U16, Name = "Inv_Curr_BC", Scale = 0.1, Unit = "A", },              // Inv 선간전류 (B-C)
            new ModbusFieldSpec { Address = 89, DataType = ModbusDataType.U16, Name = "Inv_Curr_CA", Scale = 0.1, Unit = "A", },              // Inv 선간전류 (C-A)
        };

        public static readonly IList<ModbusFieldSpec> LoadData = new List<ModbusFieldSpec>
        {
            new ModbusFieldSpec { Address = 100, DataType = ModbusDataType.U32, Name = "Load_Total_Energy", Scale = 0.0001, Unit = "MWh", },           // Load 누적 전력량
            new ModbusFieldSpec { Address = 103, DataType = ModbusDataType.U16, Name = "Load_Daily_Energy", Scale = 0.1, Unit = "kWh", },           // Load 누적 전력량 (일간)
            new ModbusFieldSpec { Address = 119, DataType = ModbusDataType.U16, Name = "Load_Volt_AB", Scale = 0.1, Unit = "V", },                  // Load 선간전압 (A-B)
            new ModbusFieldSpec { Address = 120, DataType = ModbusDataType.U16, Name = "Load_Volt_BC", Scale = 0.1, Unit = "V", },                  // Load 선간전압 (B-C)
            new ModbusFieldSpec { Address = 121, DataType = ModbusDataType.U16, Name = "Load_Volt_CA", Scale = 0.1, Unit = "V", },                  // Load 선간전압 (C-A)
            new ModbusFieldSpec { Address = 125, DataType = ModbusDataType.S16, Name = "Load_Curr_AB", Scale = 0.1, Unit = "A", },                  // Load 선간전류 (A-B)
            new ModbusFieldSpec { Address = 126, DataType = ModbusDataType.S16, Name = "Load_Curr_BC", Scale = 0.1, Unit = "A", },                  // Load 선간전류 (B-C)
            new ModbusFieldSpec { Address = 127, DataType = ModbusDataType.S16, Name = "Load_Curr_CA", Scale = 0.1, Unit = "A", },                  // Load 선간전류 (C-A)
            new ModbusFieldSpec { Address = 128, DataType = ModbusDataType.U16, Name = "Load_Freq", Scale = 0.01, Unit = "Hz", },                   // Load 주파수
            new ModbusFieldSpec { Address = 129, DataType = ModbusDataType.S16, Name = "Load_PF", Scale = 0.01, Unit = "%", }                       // Load 역률
        };

        public static readonly IList<ModbusFieldSpec> EtcData = new List<ModbusFieldSpec>
        {
            new ModbusFieldSpec { Address = 172, DataType = ModbusDataType.S16, Name = "INV_Ambient_Temp01", Scale = 1, Unit = "℃", },             // Inverter 내부온도 (주위) 1
            new ModbusFieldSpec { Address = 173, DataType = ModbusDataType.S16, Name = "INV_Ambient_Temp02", Scale = 1, Unit = "℃", },             // Inverter 내부온도 (주위) 2
            new ModbusFieldSpec { Address = 174, DataType = ModbusDataType.S16, Name = "INV_Ambient_Temp03", Scale = 1, Unit = "℃", },             // Inverter 내부온도 (주위) 3
            new ModbusFieldSpec { Address = 175, DataType = ModbusDataType.S16, Name = "INV_Ambient_Temp04", Scale = 1, Unit = "℃", },             // Inverter 내부온도 (주위) 4
                                                                         
            new ModbusFieldSpec { Address = 176, DataType = ModbusDataType.S16, Name = "INV_Heatsink_Temp01", Scale = 1, Unit = "℃", },            // Inverter 내부온도 (방열판) 1
            new ModbusFieldSpec { Address = 177, DataType = ModbusDataType.S16, Name = "INV_Heatsink_Temp02", Scale = 1, Unit = "℃", },            // Inverter 내부온도 (방열판) 2
            new ModbusFieldSpec { Address = 178, DataType = ModbusDataType.S16, Name = "INV_Heatsink_Temp03", Scale = 1, Unit = "℃", },            // Inverter 내부온도 (방열판) 3
            new ModbusFieldSpec { Address = 179, DataType = ModbusDataType.S16, Name = "INV_Heatsink_Temp04", Scale = 1, Unit = "℃", },            // Inverter 내부온도 (방열판) 4
            new ModbusFieldSpec { Address = 180, DataType = ModbusDataType.S16, Name = "INV_Heatsink_Temp05", Scale = 1, Unit = "℃", },            // Inverter 내부온도 (방열판) 5
            new ModbusFieldSpec { Address = 181, DataType = ModbusDataType.S16, Name = "INV_Heatsink_Temp06", Scale = 1, Unit = "℃", },            // Inverter 내부온도 (방열판) 6
            new ModbusFieldSpec { Address = 182, DataType = ModbusDataType.S16, Name = "INV_Heatsink_Temp07", Scale = 1, Unit = "℃", },            // Inverter 내부온도 (방열판) 7
            new ModbusFieldSpec { Address = 183, DataType = ModbusDataType.S16, Name = "INV_Heatsink_Temp08", Scale = 1, Unit = "℃", },            // Inverter 내부온도 (방열판) 8
                                                                         
            new ModbusFieldSpec { Address = 184, DataType = ModbusDataType.S16, Name = "INV_IGBT_Temp01", Scale = 1, Unit = "℃", },                // Inverter 내부온도 (IGBT) 1
            new ModbusFieldSpec { Address = 185, DataType = ModbusDataType.S16, Name = "INV_IGBT_Temp02", Scale = 1, Unit = "℃", },                // Inverter 내부온도 (IGBT) 2
            new ModbusFieldSpec { Address = 186, DataType = ModbusDataType.S16, Name = "INV_IGBT_Temp03", Scale = 1, Unit = "℃", },                // Inverter 내부온도 (IGBT) 3
            new ModbusFieldSpec { Address = 187, DataType = ModbusDataType.S16, Name = "INV_IGBT_Temp04", Scale = 1, Unit = "℃", },                // Inverter 내부온도 (IGBT) 4
            new ModbusFieldSpec { Address = 188, DataType = ModbusDataType.S16, Name = "INV_IGBT_Temp05", Scale = 1, Unit = "℃", },                // Inverter 내부온도 (IGBT) 5
            new ModbusFieldSpec { Address = 189, DataType = ModbusDataType.S16, Name = "INV_IGBT_Temp06", Scale = 1, Unit = "℃", },                // Inverter 내부온도 (IGBT) 6
            new ModbusFieldSpec { Address = 190, DataType = ModbusDataType.S16, Name = "INV_IGBT_Temp07", Scale = 1, Unit = "℃", },                // Inverter 내부온도 (IGBT) 7
            new ModbusFieldSpec { Address = 191, DataType = ModbusDataType.S16, Name = "INV_IGBT_Temp08", Scale = 1, Unit = "℃", },                // Inverter 내부온도 (IGBT) 8
            new ModbusFieldSpec { Address = 192, DataType = ModbusDataType.S16, Name = "INV_IGBT_Temp09", Scale = 1, Unit = "℃", },                // Inverter 내부온도 (IGBT) 9
            new ModbusFieldSpec { Address = 193, DataType = ModbusDataType.S16, Name = "INV_IGBT_Temp10", Scale = 1, Unit = "℃", },                // Inverter 내부온도 (IGBT) 10
            new ModbusFieldSpec { Address = 194, DataType = ModbusDataType.S16, Name = "INV_IGBT_Temp11", Scale = 1, Unit = "℃", },                // Inverter 내부온도 (IGBT) 11
            new ModbusFieldSpec { Address = 195, DataType = ModbusDataType.S16, Name = "INV_IGBT_Temp12", Scale = 1, Unit = "℃", },                // Inverter 내부온도 (IGBT) 12

            new ModbusFieldSpec { Address = 197, DataType = ModbusDataType.U16, Name = "Grid_SC_Cnt", Scale = 1, Unit = "Cyc", },                   // Grid 서지 카운터
            new ModbusFieldSpec { Address = 199, DataType = ModbusDataType.U16, Name = "PV_SC_Cnt", Scale = 1, Unit = "Cyc", },                     // PV 서지 카운터
            new ModbusFieldSpec { Address = 196, DataType = ModbusDataType.U16, Name = "Battery_SC_Cnt", Scale = 1, Unit = "Cyc", },                // Battery 서지 카운터
            new ModbusFieldSpec { Address = 198, DataType = ModbusDataType.U16, Name = "Load_SC_Cnt", Scale = 1, Unit = "Cyc", },                   // Load 서지 카운터
            new ModbusFieldSpec { Address = 200, DataType = ModbusDataType.U16, Name = "Heartbeat", Scale = 1, Unit = "Cyc", },                     // Battery 충전 전력 (일간)
                                                                                                                                                     
        };

        public static readonly IList<ModbusFieldSpec> StatusData = new List<ModbusFieldSpec>
        {
            new ModbusFieldSpec { Address = 211, DataType = ModbusDataType.U32, Name = "Grid_Status", Scale = 1 },                                   // Grid 상태
            new ModbusFieldSpec { Address = 212, DataType = ModbusDataType.S32, Name = "PV_Status", Scale = 1 },                                     // PV 상태
            new ModbusFieldSpec { Address = 213, DataType = ModbusDataType.S32, Name = "INV_Status", Scale = 1 },                                    // INV 상태
            new ModbusFieldSpec { Address = 214, DataType = ModbusDataType.S32, Name = "Batt_Status", Scale = 1 },                                   // Battery 상태
            new ModbusFieldSpec { Address = 215, DataType = ModbusDataType.U16, Name = "Load_Status", Scale = 1 },                                   // Load 상태
            new ModbusFieldSpec { Address = 216, DataType = ModbusDataType.S16, Name = "Comm_Status", Scale = 1 },                                   // Communication 상태
                                                                                                                                                 
            new ModbusFieldSpec { Address = 217, DataType = ModbusDataType.U16, Name = "Grid_Fault", Scale = 1 },                                    // Grid 에러
            new ModbusFieldSpec { Address = 218, DataType = ModbusDataType.U16, Name = "PV_Fault", Scale = 1 },                                      // PV 에러
            new ModbusFieldSpec { Address = 219, DataType = ModbusDataType.U16, Name = "INV_Fault", Scale = 1 },                                     // INV 에러
            new ModbusFieldSpec { Address = 220, DataType = ModbusDataType.U16, Name = "Batt_Fault", Scale = 1 },                                    // Battery 에러
            new ModbusFieldSpec { Address = 221, DataType = ModbusDataType.U16, Name = "Load_Fault", Scale = 1 },                                    // Load 에러
            new ModbusFieldSpec { Address = 222, DataType = ModbusDataType.U16, Name = "Comm_Fault", Scale = 1 },                                    // Communication 에러
                                                                                                                                                 
            new ModbusFieldSpec { Address = 224, DataType = ModbusDataType.U16, Name = "Grid_CB", Scale = 1 },                                       // Grid 차단기
            new ModbusFieldSpec { Address = 226, DataType = ModbusDataType.U16, Name = "PV_CB", Scale = 1, },                                        // PV 차단기
            new ModbusFieldSpec { Address = 223, DataType = ModbusDataType.U16, Name = "Batt_CB", Scale = 1 },                                       // Batt 차단기
            new ModbusFieldSpec { Address = 225, DataType = ModbusDataType.U16, Name = "Load_CB", Scale = 1 },                                       // Load 차단기
                                                                                                                                                 
            new ModbusFieldSpec { Address = 228, DataType = ModbusDataType.U16, Name = "Grid_Fuse", Scale = 1 },                                     // Grid 퓨즈
            new ModbusFieldSpec { Address = 230, DataType = ModbusDataType.U16, Name = "PV_Fuse", Scale = 1 },                                       // PV 퓨즈
            new ModbusFieldSpec { Address = 227, DataType = ModbusDataType.U16, Name = "Batt_Fuse", Scale = 1 },                                     // Batt 퓨즈
            new ModbusFieldSpec { Address = 229, DataType = ModbusDataType.U16, Name = "Load_Fuse", Scale = 1 },                                     // Load 퓨즈
                                                                                                                                                 
            new ModbusFieldSpec { Address = 228, DataType = ModbusDataType.U16, Name = "Grid_SPD", Scale = 1 },                                      // Grid SPD
            new ModbusFieldSpec { Address = 230, DataType = ModbusDataType.U16, Name = "PV_SPD", Scale = 1 },                                        // PV SPD
            new ModbusFieldSpec { Address = 227, DataType = ModbusDataType.U16, Name = "Batt_SPD", Scale = 1 },                                      // Batt SPD
            new ModbusFieldSpec { Address = 229, DataType = ModbusDataType.U16, Name = "Load_SPD", Scale = 1 },                                      // Load SPD
                                                                                                                                                 
            new ModbusFieldSpec { Address = 228, DataType = ModbusDataType.U16, Name = "Grid_SC", Scale = 1 },                                       // Grid 서지 카운터 상태
            new ModbusFieldSpec { Address = 230, DataType = ModbusDataType.U16, Name = "PV_SC", Scale = 1 },                                         // PV 서지 카운터 상태
            new ModbusFieldSpec { Address = 227, DataType = ModbusDataType.U16, Name = "Batt_SC", Scale = 1 },                                       // Batt 서지 카운터 상태
            new ModbusFieldSpec { Address = 229, DataType = ModbusDataType.U16, Name = "Load_SC", Scale = 1 },                                       // Load 서지 카운터 상태
        };

        public static readonly Dictionary<string, ModbusWriteSpec> ControlWrite = new Dictionary<string, ModbusWriteSpec>
        {
            { "OperationMode", new ModbusWriteSpec { Address = 1000, Scale = 1 } },                 // 기동모드
            { "ChargeMode", new ModbusWriteSpec { Address = 1001, Scale = 1 } },                    // 충방전 모드
            { "RunStop", new ModbusWriteSpec { Address = 1002, Scale = 1 } },                       // Run/Stop
            { "ChargeDischargeStart", new ModbusWriteSpec { Address = 1003, Scale = 1 } },          // 충전/방전 기동
            { "EmergencyFault", new ModbusWriteSpec { Address = 1004, Scale = 1 } },                // 비상정지
            { "PmsHeartbeat", new ModbusWriteSpec { Address = 1005, Scale = 1 } },                  // PMS Heartbeat
            { "MaxChargePowerPercent", new ModbusWriteSpec { Address = 1006, Scale = 0.01 } },      // 최대 충전 전력(%)
            { "MaxDischargePowerPercent", new ModbusWriteSpec { Address = 1007, Scale = 0.01 } },   // 최대 방전 전력(%)
            { "MaxChargePower", new ModbusWriteSpec { Address = 1008, Scale = 1 } },                // 최대 충전 전력_H/L
            { "MaxDischargePower", new ModbusWriteSpec { Address = 1010, Scale = 1 } },             // 최대 방전 전력_H/L
            { "MaxChargeSOC", new ModbusWriteSpec { Address = 1012, Scale = 0.1 } },                // 최대 충전 SOC
            { "MinDischargeSOC", new ModbusWriteSpec { Address = 1013, Scale = 0.1 } },             // 최소 방전 SOC
            { "MaxChargeVoltage", new ModbusWriteSpec { Address = 1014, Scale = 0.1 } },            // 최대 충전 전압
            { "MaxDischargeVoltage", new ModbusWriteSpec { Address = 1015, Scale = 0.1 } },         // 최대 방전 전압
            { "MaxChargeCurrent", new ModbusWriteSpec { Address = 1016, Scale = 0.1 } },            // 최대 충전 전류
            { "MaxDischargeCurrent", new ModbusWriteSpec { Address = 1017, Scale = 0.1 } },         // 최대 방전 전류
            { "GridMaxImportPower", new ModbusWriteSpec { Address = 1018, Scale = 1 } },            // Grid 최대 수전 전력_H/L
            { "GridMaxExportPower", new ModbusWriteSpec { Address = 1020, Scale = 1 } },            // Grid 최대 송전 전력_H/L
            { "FaultReset", new ModbusWriteSpec { Address = 1024, Scale = 1 } }                     // Fault Reset
        };

        public static IEnumerable<ModbusFieldSpec> All => GridData.Concat(InvData).Concat(LoadData).Concat(EtcData).Concat(StatusData);

    }
    public class ConnectionSettings : ViewModelBase
    {
        public string Ip
        {
            get => GetProperty(() => Ip);
            set => SetProperty(() => Ip, value);
        }
        public int Port
        {
            get => GetProperty(() => Port);
            set => SetProperty(() => Port, value);
        }
        public int TimeOut
        {
            get => GetProperty(() => TimeOut);
            set => SetProperty(() => TimeOut, value);
        }
    }
    public class ConnectionState : ViewModelBase
    {
        public string Status
        {
            get => GetProperty(() => Status);
            set => SetProperty(() => Status, value);
        }
        public string Rtt
        {
            get => GetProperty(() => Rtt);
            set => SetProperty(() => Rtt, value);
        }

        public bool IsConnected => Status == "Connected"; // 편의 프로퍼티
    }

    public sealed class PCS_PanelData : INotifyPropertyChanged
    {
        public string Status { get; set; }
        public string AlarmCnt { get; set; }
        public string T_Import_Energy { get; set; }
        public string T_Export_Energy { get; set; }
        public string D_Import_Energy { get; set; }
        public string D_Export_Energy { get; set; }
        public event PropertyChangedEventHandler PropertyChanged;
    }

    public sealed class INV_PcsData : INotifyPropertyChanged
    {
        public string Power { get; set; } // 유효전력
        public string D_ImportEnergy { get; set; }
        public string D_ExportEnergy { get; set; }
        public string VoltAverage { get; set; }
        public string CurrAverage { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
    }


}
