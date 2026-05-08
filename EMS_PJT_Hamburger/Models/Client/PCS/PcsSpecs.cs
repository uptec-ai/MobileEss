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
            new ModbusFieldSpec { Address = 5, DataType = ModbusDataType.U32, Name = "GridTotalImportActivePower", Scale = 0.1, Unit = "kWh", },   // Grid 수전 유효전력량
            new ModbusFieldSpec { Address = 7, DataType = ModbusDataType.U32, Name = "GridTotalExportedActivePower", Scale = 0.1, Unit = "kWh", }, // Grid 송전 유효전력량
            new ModbusFieldSpec { Address = 21, DataType = ModbusDataType.S32, Name = "GridActivePower", Scale = 1, Unit = "W", },                 // Grid 유효전력

            new ModbusFieldSpec { Address = 29, DataType = ModbusDataType.U16, Name = "GridVoltageAN", Scale = 0.1, Unit = "V", },                 // Grid 상전압 (AN)
            new ModbusFieldSpec { Address = 30, DataType = ModbusDataType.U16, Name = "GridVoltageBN", Scale = 0.1, Unit = "V", },                 // Grid 상전압 (BN)
            new ModbusFieldSpec { Address = 31, DataType = ModbusDataType.U16, Name = "GridVoltageCN", Scale = 0.1, Unit = "V", },                 // Grid 상전압 (CN)
                                                                                                                                                   
            new ModbusFieldSpec { Address = 32, DataType = ModbusDataType.S16, Name = "GridCurrentAN", Scale = 0.1, Unit = "A", },                 // Grid 상전류 (AN)
            new ModbusFieldSpec { Address = 33, DataType = ModbusDataType.S16, Name = "GridCurrentBN", Scale = 0.1, Unit = "A", },                 // Grid 상전류 (BN)
            new ModbusFieldSpec { Address = 34, DataType = ModbusDataType.S16, Name = "GridCurrentCN", Scale = 0.1, Unit = "A", },                 // Grid 상전류 (CN)
                                                                                                                                                   
            new ModbusFieldSpec { Address = 35, DataType = ModbusDataType.U16, Name = "GridVoltageAB", Scale = 0.1, Unit = "V", },                 // Grid 선간 전압 (A-B)
            new ModbusFieldSpec { Address = 36, DataType = ModbusDataType.U16, Name = "GridVoltageBC", Scale = 0.1, Unit = "V", },                 // Grid 선간 전압 (B-C)
            new ModbusFieldSpec { Address = 37, DataType = ModbusDataType.U16, Name = "GridVoltageCA", Scale = 0.1, Unit = "V", },                 // Grid 선간 전압 (C-A)

            new ModbusFieldSpec { Address = 41, DataType = ModbusDataType.U16, Name = "GridFrequency", Scale = 0.01, Unit = "Hz", },               // Grid 주파수
            new ModbusFieldSpec { Address = 42, DataType = ModbusDataType.S16, Name = "GridPowerFactor", Scale = 0.01, Unit = "%", }               // Grid 역률
        };

        public static readonly IList<ModbusFieldSpec> InvData = new List<ModbusFieldSpec>
        {
            new ModbusFieldSpec { Address = 46, DataType = ModbusDataType.U32, Name = "InvTotalImportActivePower", Scale = 0.1, Unit = "kWh", },   // Inv 수전 유효전력량
            new ModbusFieldSpec { Address = 48, DataType = ModbusDataType.U32, Name = "InvTotalExportedActivePower", Scale = 0.1, Unit = "kWh", }, // Inv 송전 유효전력량
            new ModbusFieldSpec { Address = 62, DataType = ModbusDataType.S32, Name = "InvActivePower", Scale = 1, Unit = "W", },                   // Inv 유효전력

            new ModbusFieldSpec { Address = 70, DataType = ModbusDataType.U16, Name = "InvVoltageAN", Scale = 0.1, Unit = "V", },                // Inv 상전압 (AN)
            new ModbusFieldSpec { Address = 71, DataType = ModbusDataType.U16, Name = "InvVoltageBN", Scale = 0.1, Unit = "V", },                // Inv 상전압 (BN)
            new ModbusFieldSpec { Address = 72, DataType = ModbusDataType.U16, Name = "InvVoltageCN", Scale = 0.1, Unit = "V", },                // Inv 상전압 (CN)

            new ModbusFieldSpec { Address = 73, DataType = ModbusDataType.S16, Name = "InvCurrentAN", Scale = 0.1, Unit = "A", },                // Inv 상전류 (AN)
            new ModbusFieldSpec { Address = 74, DataType = ModbusDataType.S16, Name = "InvCurrentBN", Scale = 0.1, Unit = "A", },                // Inv 상전류 (BN)
            new ModbusFieldSpec { Address = 75, DataType = ModbusDataType.S16, Name = "InvCurrentCN", Scale = 0.1, Unit = "A", },                // Inv 상전류 (CN)

            new ModbusFieldSpec { Address = 76, DataType = ModbusDataType.U16, Name = "InvVoltageAB", Scale = 0.1, Unit = "V", },                // Inv 선간 전압 (A-B)
            new ModbusFieldSpec { Address = 77, DataType = ModbusDataType.U16, Name = "InvVoltageBC", Scale = 0.1, Unit = "V", },                // Inv 선간 전압 (B-C)
            new ModbusFieldSpec { Address = 78, DataType = ModbusDataType.U16, Name = "InvVoltageCA", Scale = 0.1, Unit = "V", },                // Inv 선간 전압 (C-A)

            new ModbusFieldSpec { Address = 82, DataType = ModbusDataType.U16, Name = "InvFrequency", Scale = 0.01, Unit = "Hz", },               // Inv 주파수
            new ModbusFieldSpec { Address = 83, DataType = ModbusDataType.S16, Name = "InvPowerFactor", Scale = 0.01, Unit = "%", }               // Inv 역률
        };

        public static readonly IList<ModbusFieldSpec> LoadData = new List<ModbusFieldSpec>
        {
            new ModbusFieldSpec { Address = 87, DataType = ModbusDataType.U32, Name = "LoadTotalExportedActivePower", Scale = 0.1, Unit = "kWh", }, // Load 송전 유효전력량
            new ModbusFieldSpec { Address = 95, DataType = ModbusDataType.S32, Name = "LoadActivePower", Scale = 1, Unit = "W", },                  // Load 유효전력
            new ModbusFieldSpec { Address = 97, DataType = ModbusDataType.S32, Name = "LoadActivePowerRN", Scale = 1, Unit = "W", },                // Load RN 유효전력
            new ModbusFieldSpec { Address = 99, DataType = ModbusDataType.S32, Name = "LoadActivePowerSN", Scale = 1, Unit = "W", },                // Load SN 유효전력
            new ModbusFieldSpec { Address = 101, DataType = ModbusDataType.S32, Name = "LoadActivePowerTN", Scale = 1, Unit = "W", },               // Load TN 유효전력

            new ModbusFieldSpec { Address = 103, DataType = ModbusDataType.U16, Name = "LoadVoltageAN", Scale = 0.1, Unit = "V", },                 // Load 상전압 (AN)
            new ModbusFieldSpec { Address = 104, DataType = ModbusDataType.U16, Name = "LoadVoltageBN", Scale = 0.1, Unit = "V", },                 // Load 상전압 (BN)
            new ModbusFieldSpec { Address = 105, DataType = ModbusDataType.U16, Name = "LoadVoltageCN", Scale = 0.1, Unit = "V", },                 // Load 상전압 (CN)
                                            
            new ModbusFieldSpec { Address = 106, DataType = ModbusDataType.S16, Name = "LoadCurrentAN", Scale = 0.1, Unit = "A", },                 // Load 상전류 (AN)
            new ModbusFieldSpec { Address = 107, DataType = ModbusDataType.S16, Name = "LoadCurrentBN", Scale = 0.1, Unit = "A", },                 // Load 상전류 (BN)
            new ModbusFieldSpec { Address = 108, DataType = ModbusDataType.S16, Name = "LoadCurrentCN", Scale = 0.1, Unit = "A", },                 // Load 상전류 (CN)
                                            
            new ModbusFieldSpec { Address = 109, DataType = ModbusDataType.U16, Name = "LoadVoltageAB", Scale = 0.1, Unit = "V", },                 // Load 선간 전압 (A-B)
            new ModbusFieldSpec { Address = 110, DataType = ModbusDataType.U16, Name = "LoadVoltageBC", Scale = 0.1, Unit = "V", },                 // Load 선간 전압 (B-C)
            new ModbusFieldSpec { Address = 111, DataType = ModbusDataType.U16, Name = "LoadVoltageCA", Scale = 0.1, Unit = "V", },                 // Load 선간 전압 (C-A)
                                            
            new ModbusFieldSpec { Address = 115, DataType = ModbusDataType.U16, Name = "LoadFrequency", Scale = 0.01, Unit = "Hz", },               // Load 주파수
            new ModbusFieldSpec { Address = 116, DataType = ModbusDataType.S16, Name = "LoadPowerFactor", Scale = 0.01, Unit = "%", },              // Load 역률

            new ModbusFieldSpec { Address = 117, DataType = ModbusDataType.S16, Name = "LoadPowerFactorRN", Scale = 0.01, Unit = "%", },            // Load RN 역률
            new ModbusFieldSpec { Address = 118, DataType = ModbusDataType.S16, Name = "LoadPowerFactorSN", Scale = 0.01, Unit = "%", },            // Load SN 역률
            new ModbusFieldSpec { Address = 119, DataType = ModbusDataType.S16, Name = "LoadPowerFactorTN", Scale = 0.01, Unit = "%", },            // Load TN 역률
        };

        public static readonly IList<ModbusFieldSpec> BatteryData = new List<ModbusFieldSpec>
        {
            new ModbusFieldSpec { Address = 120, DataType = ModbusDataType.U32, Name = "BatteryTotalChargePower", Scale = 0.1, Unit = "kWh", }, // Battery 총 충전량
            new ModbusFieldSpec { Address = 122, DataType = ModbusDataType.U32, Name = "BatteryTotalDischargePower", Scale = 0.1, Unit = "kWh", },   // Battery 총 방전량
            new ModbusFieldSpec { Address = 124, DataType = ModbusDataType.S32, Name = "BatteryPower", Scale = 1, Unit = "W", },   // Battery 전력
            new ModbusFieldSpec { Address = 126, DataType = ModbusDataType.U16, Name = "BatteryVoltage", Scale = 0.1, Unit = "V", },   // Battery 전압
            new ModbusFieldSpec { Address = 127, DataType = ModbusDataType.S16, Name = "BatteryCurrent", Scale = 0.1, Unit = "A", },   // Battery 전류
        };

        public static readonly IList<ModbusFieldSpec> EtcData = new List<ModbusFieldSpec>
        {
            new ModbusFieldSpec { Address = 132, DataType = ModbusDataType.S16, Name = "InvAmbientTemperature", Scale = 0.1, Unit = "℃", },       // Inverter 내부온도 (주위)
                                                                         
            new ModbusFieldSpec { Address = 133, DataType = ModbusDataType.S16, Name = "InvHeatsinkTemperature01", Scale = 0.1, Unit = "℃", },    // Inverter 내부온도1 (방열판)
            new ModbusFieldSpec { Address = 134, DataType = ModbusDataType.S16, Name = "InvHeatsinkTemperature02", Scale = 0.1, Unit = "℃", },    // Inverter 내부온도2 (방열판)
            new ModbusFieldSpec { Address = 135, DataType = ModbusDataType.S16, Name = "InvHeatsinkTemperature03", Scale = 0.1, Unit = "℃", },    // Inverter 내부온도3 (방열판)
            new ModbusFieldSpec { Address = 136, DataType = ModbusDataType.S16, Name = "InvHeatsinkTemperature04", Scale = 0.1, Unit = "℃", },    // Inverter 내부온도4 (방열판)
            new ModbusFieldSpec { Address = 137, DataType = ModbusDataType.S16, Name = "InvHeatsinkTemperature05", Scale = 0.1, Unit = "℃", },    // Inverter 내부온도5 (방열판)
            new ModbusFieldSpec { Address = 138, DataType = ModbusDataType.S16, Name = "InvHeatsinkTemperature06", Scale = 0.1, Unit = "℃", },    // Inverter 내부온도6 (방열판)
            new ModbusFieldSpec { Address = 139, DataType = ModbusDataType.S16, Name = "InvHeatsinkTemperature07", Scale = 0.1, Unit = "℃", },    // Inverter 내부온도7 (방열판)
            new ModbusFieldSpec { Address = 140, DataType = ModbusDataType.S16, Name = "InvHeatsinkTemperature08", Scale = 0.1, Unit = "℃", },    // Inverter 내부온도8 (방열판)
                                                                         
            new ModbusFieldSpec { Address = 142, DataType = ModbusDataType.S16, Name = "BatteryInsulationResistance", Scale = 0.1, Unit = "kΩ", }, // Battery 절연 저항
            new ModbusFieldSpec { Address = 144, DataType = ModbusDataType.S16, Name = "BatteryInsulationVoltage", Scale = 0.1, Unit = "V", },     // Battery 절연 전압

            new ModbusFieldSpec { Address = 145, DataType = ModbusDataType.U16, Name = "InvLeakageCurrent", Scale = 1, Unit = "mA", },             // Inverter 누설 전류
            new ModbusFieldSpec { Address = 146, DataType = ModbusDataType.U16, Name = "GridSurgeCounter", Scale = 1, Unit = "Cyc", },             // Grid 서지 카운터 횟수
            new ModbusFieldSpec { Address = 147, DataType = ModbusDataType.U16, Name = "InvHeartBeat", Scale = 1, Unit = "Cyc", },                 // Inverter 심박수
            
                                                                                                                                                     
        };

        public static readonly IList<ModbusFieldSpec> StatusData = new List<ModbusFieldSpec>
        {
            new ModbusFieldSpec { Address = 148, DataType = ModbusDataType.U16, Name = "ReadyStatus", Scale = 1, Unit = "", },                     // 준비 상태
            new ModbusFieldSpec { Address = 149, DataType = ModbusDataType.U16, Name = "FaultStatus", Scale = 1, Unit = "", },                     // 폴트 Status

            new ModbusFieldSpec { Address = 150, DataType = ModbusDataType.U16, Name = "GridFault", Scale = 1, Unit = "", },                       // Grid Fault
            new ModbusFieldSpec { Address = 151, DataType = ModbusDataType.U16, Name = "InvFault", Scale = 1, Unit = "", },                        // Inverter Fault
            new ModbusFieldSpec { Address = 152, DataType = ModbusDataType.U16, Name = "LoadFault", Scale = 1, Unit = "", },                       // Load Fault
            new ModbusFieldSpec { Address = 153, DataType = ModbusDataType.U16, Name = "BatteryFault", Scale = 1, Unit = "", },                    // Battery Fault
            new ModbusFieldSpec { Address = 154, DataType = ModbusDataType.U16, Name = "SystemFault", Scale = 1, Unit = "", },                     // System Fault
            new ModbusFieldSpec { Address = 155, DataType = ModbusDataType.U16, Name = "CommunicationFault", Scale = 1, Unit = "", },              // Communication Fault

            new ModbusFieldSpec { Address = 156, DataType = ModbusDataType.U16, Name = "BatteryFuseStatus", Scale = 1, Unit = "", },               // Battery Fuse Status
            new ModbusFieldSpec { Address = 157, DataType = ModbusDataType.U16, Name = "GridFuseStatus", Scale = 1, Unit = "", },                  // Grid Fuse Status
            new ModbusFieldSpec { Address = 158, DataType = ModbusDataType.U16, Name = "BypassFuseStatus", Scale = 1, Unit = "", },                // Bypass Fuse Status
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

        public static IEnumerable<ModbusFieldSpec> All => GridData.Concat(InvData).Concat(LoadData).Concat(BatteryData).Concat(EtcData).Concat(StatusData);

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
        public bool BattReady { get; set; }
        public bool InvReady { get; set; }
        public bool GridReady { get; set; }
        public bool CommReady { get; set; }
        public bool BypassReady { get; set; }
        public string AlarmCnt { get; set; }
        public string T_Import_Energy { get; set; }
        public string T_Export_Energy { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
    }

    public sealed class INV_PcsData : INotifyPropertyChanged
    {
        public string Power { get; set; } // 유효전력
        public string T_ImportEnergy { get; set; }
        public string T_ExportEnergy { get; set; }
        public string VoltAverage { get; set; }
        public string CurrAverage { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
    }


}
