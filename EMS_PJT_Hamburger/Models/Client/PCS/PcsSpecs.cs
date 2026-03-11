using DevExpress.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMS_PJT_Hamburger.Models.Client.PCS
{

    public static class PcsSpecs
    {
        public static readonly IList<ModbusFieldSpec> GridData = new List<ModbusFieldSpec>
        {
            new ModbusFieldSpec { Address = 4, DataType = ModbusDataType.U32, Name = "Grid_Total_ImportedEnergy", Scale = 0.1, Unit = "kWh", },    // Grid 수전 누적 전력량
            new ModbusFieldSpec { Address = 6, DataType = ModbusDataType.U32, Name = "Grid_Total_ExportedEnergy", Scale = 0.1, Unit = "kWh", },    // Grid 송전 누적 전력량
            new ModbusFieldSpec { Address = 8, DataType = ModbusDataType.U16, Name = "Grid_Daily_ImportedEnergy", Scale = 0.1, Unit = "kWh", },    // Grid 수전 누적 전력량 (일간)
            new ModbusFieldSpec { Address = 9, DataType = ModbusDataType.U16, Name = "Grid_Daily_ExportedEnergy", Scale = 0.1, Unit = "kWh", },    // Grid 송전 누적 전력량 (일간)

            new ModbusFieldSpec { Address = 30, DataType = ModbusDataType.U16, Name = "Grid_Voltage_AB", Scale = 0.1, Unit = "V", },                // Grid 선간 전압 (A-B)
            new ModbusFieldSpec { Address = 31, DataType = ModbusDataType.U16, Name = "Grid_Voltage_BC", Scale = 0.1, Unit = "V", },                // Grid 선간 전압 (B-C)
            new ModbusFieldSpec { Address = 32, DataType = ModbusDataType.U16, Name = "Grid_Voltage_CA", Scale = 0.1, Unit = "V", },                // Grid 선간 전압 (C-A)

            new ModbusFieldSpec { Address = 39, DataType = ModbusDataType.S16, Name = "Grid_Current_AB", Scale = 0.1, Unit = "A", },                // Grid 선간 전류 (A-B)
            new ModbusFieldSpec { Address = 40, DataType = ModbusDataType.S16, Name = "Grid_Current_BC", Scale = 0.1, Unit = "A", },                // Grid 선간 전류 (B-C)
            new ModbusFieldSpec { Address = 41, DataType = ModbusDataType.S16, Name = "Grid_Current_CA", Scale = 0.1, Unit = "A", },                // Grid 선간 전류 (C-A)

            new ModbusFieldSpec { Address = 42, DataType = ModbusDataType.U16, Name = "Grid_Frequency", Scale = 0.01, Unit = "Hz", },                // Grid 주파수
            new ModbusFieldSpec { Address = 43, DataType = ModbusDataType.S16, Name = "Grid_PowerFactor", Scale = 0.01, Unit = "%", }                // Grid 역률
        };

        public static readonly IList<ModbusFieldSpec> LoadData = new List<ModbusFieldSpec>
        {
            new ModbusFieldSpec { Address = 100, DataType = ModbusDataType.U32, Name = "Load_Total_Energy", Scale = 0.1, Unit = "kWh", },            // Load 누적 전력량

            new ModbusFieldSpec { Address = 103, DataType = ModbusDataType.U16, Name = "Load_Daily_Energy", Scale = 0.1, Unit = "kWh", },            // Load 누적 전력량 (일간)
            
            new ModbusFieldSpec { Address = 119, DataType = ModbusDataType.U16, Name = "Load_Voltage_AB", Scale = 0.1, Unit = "V", },                // Load 선간전압 (A-B)

            new ModbusFieldSpec { Address = 120, DataType = ModbusDataType.U16, Name = "Load_Voltage_BC", Scale = 0.1, Unit = "V", },                // Load 선간전압 (B-C)

            new ModbusFieldSpec { Address = 121, DataType = ModbusDataType.U16, Name = "Load_Voltage_CA", Scale = 0.1, Unit = "V", },                // Load 선간전압 (C-A)

            new ModbusFieldSpec { Address = 125, DataType = ModbusDataType.S16, Name = "Load_Current_AB", Scale = 0.1, Unit = "A", },                // Load 선간전류 (A-B)

            new ModbusFieldSpec { Address = 126, DataType = ModbusDataType.S16, Name = "Load_Current_BC", Scale = 0.1, Unit = "A", },                // Load 선간전류 (B-C)

            new ModbusFieldSpec { Address = 127, DataType = ModbusDataType.S16, Name = "Load_Current_CA", Scale = 0.1, Unit = "A", },                // Load 선간전류 (C-A)
                                                                                                                                                
            new ModbusFieldSpec { Address = 128, DataType = ModbusDataType.U16, Name = "Load_Frequency", Scale = 0.01, Unit = "Hz", },               // Load 주파수
                                                                                                                                                
            new ModbusFieldSpec { Address = 129, DataType = ModbusDataType.S16, Name = "Load_PowerFactor", Scale = 0.01, Unit = "%", }               // Load 역률
        };

        public static readonly IList<ModbusFieldSpec> EtcData = new List<ModbusFieldSpec>
        {
            new ModbusFieldSpec { Address = 172, DataType = ModbusDataType.S16, Name = "INV_Ambient_Temperature01", Scale = 1, Unit = "℃", },      // Inverter 내부온도 (주위) 1
            new ModbusFieldSpec { Address = 173, DataType = ModbusDataType.S16, Name = "INV_Ambient_Temperature02", Scale = 1, Unit = "℃", },      // Inverter 내부온도 (주위) 2
            new ModbusFieldSpec { Address = 174, DataType = ModbusDataType.S16, Name = "INV_Ambient_Temperature03", Scale = 1, Unit = "℃", },      // Inverter 내부온도 (주위) 3
            new ModbusFieldSpec { Address = 175, DataType = ModbusDataType.S16, Name = "INV_Ambient_Temperature04", Scale = 1, Unit = "℃", },      // Inverter 내부온도 (주위) 4
                                                                         
            new ModbusFieldSpec { Address = 176, DataType = ModbusDataType.S16, Name = "INV_Heatsink_Temperature01", Scale = 1, Unit = "℃", },     // Inverter 내부온도 (방열판) 1
            new ModbusFieldSpec { Address = 177, DataType = ModbusDataType.S16, Name = "INV_Heatsink_Temperature02", Scale = 1, Unit = "℃", },     // Inverter 내부온도 (방열판) 2
            new ModbusFieldSpec { Address = 178, DataType = ModbusDataType.S16, Name = "INV_Heatsink_Temperature03", Scale = 1, Unit = "℃", },     // Inverter 내부온도 (방열판) 3
            new ModbusFieldSpec { Address = 179, DataType = ModbusDataType.S16, Name = "INV_Heatsink_Temperature04", Scale = 1, Unit = "℃", },     // Inverter 내부온도 (방열판) 4
            new ModbusFieldSpec { Address = 180, DataType = ModbusDataType.S16, Name = "INV_Heatsink_Temperature05", Scale = 1, Unit = "℃", },     // Inverter 내부온도 (방열판) 5
            new ModbusFieldSpec { Address = 181, DataType = ModbusDataType.S16, Name = "INV_Heatsink_Temperature06", Scale = 1, Unit = "℃", },     // Inverter 내부온도 (방열판) 6
            new ModbusFieldSpec { Address = 182, DataType = ModbusDataType.S16, Name = "INV_Heatsink_Temperature07", Scale = 1, Unit = "℃", },     // Inverter 내부온도 (방열판) 7
            new ModbusFieldSpec { Address = 183, DataType = ModbusDataType.S16, Name = "INV_Heatsink_Temperature08", Scale = 1, Unit = "℃", },     // Inverter 내부온도 (방열판) 8
                                                                         
            new ModbusFieldSpec { Address = 184, DataType = ModbusDataType.S16, Name = "INV_IGBT_Temperature01", Scale = 1, Unit = "℃", },         // Inverter 내부온도 (IGBT) 1
            new ModbusFieldSpec { Address = 185, DataType = ModbusDataType.S16, Name = "INV_IGBT_Temperature02", Scale = 1, Unit = "℃", },         // Inverter 내부온도 (IGBT) 2
            new ModbusFieldSpec { Address = 186, DataType = ModbusDataType.S16, Name = "INV_IGBT_Temperature03", Scale = 1, Unit = "℃", },         // Inverter 내부온도 (IGBT) 3
            new ModbusFieldSpec { Address = 187, DataType = ModbusDataType.S16, Name = "INV_IGBT_Temperature04", Scale = 1, Unit = "℃", },         // Inverter 내부온도 (IGBT) 4
            new ModbusFieldSpec { Address = 188, DataType = ModbusDataType.S16, Name = "INV_IGBT_Temperature05", Scale = 1, Unit = "℃", },         // Inverter 내부온도 (IGBT) 5
            new ModbusFieldSpec { Address = 189, DataType = ModbusDataType.S16, Name = "INV_IGBT_Temperature06", Scale = 1, Unit = "℃", },         // Inverter 내부온도 (IGBT) 6
            new ModbusFieldSpec { Address = 190, DataType = ModbusDataType.S16, Name = "INV_IGBT_Temperature07", Scale = 1, Unit = "℃", },         // Inverter 내부온도 (IGBT) 7
            new ModbusFieldSpec { Address = 191, DataType = ModbusDataType.S16, Name = "INV_IGBT_Temperature08", Scale = 1, Unit = "℃", },         // Inverter 내부온도 (IGBT) 8
            new ModbusFieldSpec { Address = 192, DataType = ModbusDataType.S16, Name = "INV_IGBT_Temperature09", Scale = 1, Unit = "℃", },         // Inverter 내부온도 (IGBT) 9
            new ModbusFieldSpec { Address = 193, DataType = ModbusDataType.S16, Name = "INV_IGBT_Temperature10", Scale = 1, Unit = "℃", },         // Inverter 내부온도 (IGBT) 10
            new ModbusFieldSpec { Address = 194, DataType = ModbusDataType.S16, Name = "INV_IGBT_Temperature11", Scale = 1, Unit = "℃", },         // Inverter 내부온도 (IGBT) 11
            new ModbusFieldSpec { Address = 195, DataType = ModbusDataType.S16, Name = "INV_IGBT_Temperature12", Scale = 1, Unit = "℃", },         // Inverter 내부온도 (IGBT) 12

            new ModbusFieldSpec { Address = 196, DataType = ModbusDataType.U16, Name = "Battery_SurgeCount", Scale = 1, Unit = "Cyc", },            // Battery 충전 전력 (일간)
            new ModbusFieldSpec { Address = 197, DataType = ModbusDataType.U16, Name = "Grid_SurgeCount", Scale = 1, Unit = "Cyc", },               // Battery 충전 전력 (일간)
            new ModbusFieldSpec { Address = 198, DataType = ModbusDataType.U16, Name = "Load_SurgeCount", Scale = 1, Unit = "Cyc", },               // Battery 충전 전력 (일간)
            new ModbusFieldSpec { Address = 199, DataType = ModbusDataType.U16, Name = "PV_SurgeCount", Scale = 1, Unit = "Cyc", },                 // Battery 충전 전력 (일간)
            new ModbusFieldSpec { Address = 200, DataType = ModbusDataType.U16, Name = "Heartbeat", Scale = 1, Unit = "Cyc", },                     // Battery 충전 전력 (일간)
                                                                                                                                                     
        };

        public static readonly IList<ModbusFieldSpec> StatusData = new List<ModbusFieldSpec>
        {
            new ModbusFieldSpec { Address = 211, DataType = ModbusDataType.U32, Name = "Status_Grid", Scale = 1 },                  // Grid Status
            new ModbusFieldSpec { Address = 212, DataType = ModbusDataType.S32, Name = "Status_PV", Scale = 1, Unit = "kWh", },                      // PV Status
            new ModbusFieldSpec { Address = 213, DataType = ModbusDataType.S32, Name = "Status_INV", Scale = 1, Unit = "V", },                       // INV Status
            new ModbusFieldSpec { Address = 214, DataType = ModbusDataType.S32, Name = "Status_Battery", Scale = 1, Unit = "V", },                   // Battery Status
            new ModbusFieldSpec { Address = 215, DataType = ModbusDataType.U16, Name = "Status_Load", Scale = 0.1, Unit = "V", },                    // Load Status
            new ModbusFieldSpec { Address = 216, DataType = ModbusDataType.S16, Name = "Status_Communication", Scale = 0.1, Unit = "A", },           // Communication Status
            new ModbusFieldSpec { Address = 217, DataType = ModbusDataType.U16, Name = "Fault_Grid", Scale = 0.1, Unit = "%", },                     // Grid Fault
            new ModbusFieldSpec { Address = 218, DataType = ModbusDataType.U16, Name = "Fault_PV", Scale = 0.1, Unit = "%", },                       // PV Fault
            new ModbusFieldSpec { Address = 219, DataType = ModbusDataType.U16, Name = "Fault_INV", Scale = 0.1, Unit = "%", },                      // INV Fault
            new ModbusFieldSpec { Address = 220, DataType = ModbusDataType.U16, Name = "Fault_Battery", Scale = 0.1, Unit = "%", },                  // Battery Fault
            new ModbusFieldSpec { Address = 221, DataType = ModbusDataType.U16, Name = "Fault_Load", Scale = 0.1, Unit = "%", },                     // Load Fault
            new ModbusFieldSpec { Address = 222, DataType = ModbusDataType.U16, Name = "Fault_Communication", Scale = 0.1, Unit = "%", },            // Communication Fault
        };

        public static readonly Dictionary<string, ModbusWriteSpec> ControlWrite = new Dictionary<string, ModbusWriteSpec>
        {
            { "Operation", new ModbusWriteSpec { Address = 245, Scale = 1 } },                  // Run/Stop
            { "PowerDirection", new ModbusWriteSpec { Address = 246, Scale = 1 } },             // 충전/방전
            { "MaxChargeVoltage", new ModbusWriteSpec { Address = 247, Scale = 0.1 } },         // 최대 충전 전압
            { "MaxDischargeVoltage", new ModbusWriteSpec { Address = 248, Scale = 0.1 } },      // 최대 방전 전압
            { "BAT_MaxChargeCurrent", new ModbusWriteSpec { Address = 249, Scale = 0.1 } },     // Battery 최대 충전 전류
            { "BAT_MaxDischargeCurrent", new ModbusWriteSpec { Address = 250, Scale = 0.1 } },  // Battery 최대 방전 전류
            { "BAT_MaxChargePower", new ModbusWriteSpec { Address = 251, Scale = 0.1 } },       // Battery 최대 충전 전력
            { "BAT_MaxDischargePower", new ModbusWriteSpec { Address = 252, Scale = 0.1 } },    // Battery 최대 방전 전력
            { "MaxChargeSOC", new ModbusWriteSpec { Address = 253, Scale = 1 } },               // 최대 충전 SOC
            { "MaxDischargeSOC", new ModbusWriteSpec { Address = 254, Scale = 1 } },            // 최대 방전 SOC
            { "ChargeMode", new ModbusWriteSpec { Address = 255, Scale = 1 } },                 // 충방전 모드
            { "Heartbeat", new ModbusWriteSpec { Address = 256, Scale = 1 } },                  // Heartbeat
            { "EmergencyFault", new ModbusWriteSpec { Address = 257, Scale = 1 } },             // 비상정지
            { "FaultReset", new ModbusWriteSpec { Address = 258, Scale = 1 } }                  // Fault Reset
        };

        public static IEnumerable<ModbusFieldSpec> All => GridData.Concat(LoadData).Concat(EtcData).Concat(StatusData);

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
    
}
