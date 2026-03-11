using DevExpress.Mvvm;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static EMS_PJT_Hamburger.Models.Managers.DbManager;

namespace EMS_PJT_Hamburger.Models.Client.BMS
{
    public static class BmsSpecs
    {
        public static readonly CanMessageSpec BMS_Status_Message01 = new CanMessageSpec
        {
            Name = "BMS_Status_Message01",
            CanId = 0x150,
            IdType = CanIdType.STD,
            Dlc = 8,
            PollMs = 200,
            Fields = new List<CanFieldSpec>
            {
                new CanFieldSpec { Name="BMS_Ready_Status", BitOffset=0,  BitLength=1,  Convert = v => (v == 1UL) ? "Ready" : "Not Ready" },
                new CanFieldSpec { Name="BMS_SOC",          BitOffset=8,  BitLength=8,  Convert = v => (double)v },
                new CanFieldSpec { Name="BMS_Total_Current",BitOffset=16, BitLength=16, Convert = v => Math.Round(((int)v) * 0.1 - 500, 1) },
                new CanFieldSpec { Name="BMS_Total_Voltage",BitOffset=32, BitLength=16, Convert = v => Math.Round(((int)v) * 0.1, 1) },
                new CanFieldSpec { Name="MBMS_State",       BitOffset=48, BitLength=8,  Convert = v => (byte)v },
                new CanFieldSpec { Name="BMS_Disp_SOC",     BitOffset=56, BitLength=8,  Convert = v => (double)v },
            }
        };
        public static readonly CanMessageSpec BMS_Status_Message02 = new CanMessageSpec
        {
            Name = "BMS_Status_Message02",
            CanId = 0x151,
            IdType = CanIdType.STD,
            Dlc = 8,
            PollMs = 200,
            Fields = new List<CanFieldSpec>
            {
                // Flag
                Flag("Module Connection Loss Fault",        0),
                Flag("Cell Under Voltage Fault",            6),
                Flag("Cell Over Voltage Fault",             7),
                Flag("Pack Under Voltage Fault",            8),
                Flag("Pack Over Voltage Fault",             9),
                Flag("Charge Over Current Fault",           10),
                Flag("Discharge Over Current Fault",        11),
                Flag("High Temperature Fault",              12),
                Flag("Low Temperature Fault",              13),
                Flag("Module Temperature Imbalance Fault",  14),
                Flag("Cell Voltage Imbalance Fault",        15),
                Flag("Cell Under SOC Fault",                17),
                // double
                new CanFieldSpec { Name="Fault Pack Number",BitOffset=24, BitLength=8, Convert = v => (int)v },
                new CanFieldSpec { Name="Max Cell Voltage",BitOffset=32, BitLength=16, Convert = v => Math.Round(((int)v) * 0.1, 1) },
                new CanFieldSpec { Name="Max Cell Voltage Pack Number",BitOffset=48, BitLength=8, Convert = v => (int)v },
                new CanFieldSpec { Name="Software Version",BitOffset=56, BitLength=8, Convert = v => Math.Round(((int)v) * 0.1, 1) },
            }
        };
        public static readonly CanMessageSpec BMS_Status_Message03 = new CanMessageSpec
        {
            Name = "BMS_Status_Message03",
            CanId = 0x152,
            IdType = CanIdType.STD,
            Dlc = 8,
            PollMs = 200,
            Fields = new List<CanFieldSpec>
            {
                new CanFieldSpec { Name="MBMS_Ready", BitOffset=0,  BitLength=1,  Convert = v => (v == 1UL) ? "Ready" : "Not Ready" },
                new CanFieldSpec { Name="Pack_Ready",          BitOffset=8,  BitLength=24,  Convert = v => (int)v },
                new CanFieldSpec { Name="Max_Pack_Count",BitOffset=32, BitLength=8, Convert = v => (int)v },
                new CanFieldSpec { Name="Current_Pack_Count",BitOffset=40, BitLength=8, Convert = v => (int)v },
            }
        };
        public static readonly CanMessageSpec BMS_Status_Message04 = new CanMessageSpec
        {
            Name = "BMS_Status_Message04",
            CanId = 0x153,
            IdType = CanIdType.STD,
            Dlc = 8,
            PollMs = 200,
            Fields = new List<CanFieldSpec>
            {
                new CanFieldSpec { Name="Cell_Min_Voltage", BitOffset=0,  BitLength=16,  Convert = v => (double)v },
                new CanFieldSpec { Name="Cell_Min_PackNum", BitOffset=16,  BitLength=8,  Convert = v => (int)v },
                new CanFieldSpec { Name="Max_Temperature",BitOffset=24, BitLength=8, Convert = v => (int)v - 30 },
                new CanFieldSpec { Name="Max_Temperature_PackNum",BitOffset=32, BitLength=8, Convert = v => (int)v },
                new CanFieldSpec { Name="Min_Temperature", BitOffset=40, BitLength=8,  Convert = v => (int)v - 30 },
                new CanFieldSpec { Name="Min_Temperature_PackNum",     BitOffset=48, BitLength=8,  Convert = v => (int)v },
            }
        };
        public static CanMessageSpec CreateBmsPackInfo(int packNo)
        {
            return new CanMessageSpec
            {
                Name = $"Bms_Pack_Info{packNo:00}",
                CanId = 0x154 + ((uint)packNo - 1),
                IdType = CanIdType.STD,
                Dlc = 8,
                PollMs = 500,
                Fields = new List<CanFieldSpec>
                {
                    new CanFieldSpec
                    {
                        Name = $"MaxCellVoltage",
                        BitOffset = 0,
                        BitLength = 16,
                        Convert = v => (int)v
                    },
                    new CanFieldSpec
                    {
                        Name = $"MinCellVoltage",
                        BitOffset = 16,
                        BitLength = 16,
                        Convert = v => (int)v
                    },
                    new CanFieldSpec
                    {
                        Name = $"MaxTemperature",
                        BitOffset = 32,
                        BitLength = 8,
                        Convert = v => (int)v - 30
                    },
                    new CanFieldSpec
                    {
                        Name = $"MinTemperature",
                        BitOffset = 40,
                        BitLength = 8,
                        Convert = v => (int)v - 30
                    }
                }
            };
        }
        public static List<CanMessageSpec> BMS_Status_Messages = new List<CanMessageSpec>
        {
            BMS_Status_Message01,
            BMS_Status_Message02,
            BMS_Status_Message03,
            BMS_Status_Message04,
        };

        public static readonly List<CanMessageSpec> BMS_Pack_Info = Enumerable.Range(1, 17).Select(CreateBmsPackInfo).ToList(); // create pack1 ~ 17 

        public static readonly Dictionary<uint, CanMessageSpec> _specMap = BMS_Status_Messages.Concat(BMS_Pack_Info).Where(s => s != null).ToDictionary(s => s.CanId);
        private static CanFieldSpec Flag(string name, int bitOffset, int bitLength = 1)
        {
            return new CanFieldSpec
            {
                Name = name,
                BitOffset = bitOffset,
                BitLength = bitLength,
                Convert = bitLength == 1
                    ? new Func<ulong, object>(v => (v & 1UL) == 1UL)
                    : new Func<ulong, object>(v => (int)v)
            };
        }
    }

    public static class CanMessageParser
    {
        public static Dictionary<string, object> Parse(CanMessageSpec spec, byte[] data)
        {
            if (data.Length < spec.Dlc)
                throw new ArgumentException("DLC mismatch");

            ulong payload = ToUInt64LittleEndian(data);

            var result = new Dictionary<string, object>();

            foreach (var field in spec.Fields)
            {
                ulong raw = ExtractBits(payload, field.BitOffset, field.BitLength);
                object value = field.Convert != null ? field.Convert(raw) : raw;
                result[field.Name] = value;
            }

            return result;
        }

        private static ulong ToUInt64LittleEndian(byte[] data)
        {
            ulong value = 0;
            for (int i = 0; i < data.Length; i++)
                value |= ((ulong)data[i]) << (8 * i);
            return value;
        }

        private static ulong ExtractBits(ulong payload, int bitOffset, int bitLength)
        {
            ulong mask = (1UL << bitLength) - 1;
            return (payload >> bitOffset) & mask;
        }
    }
    public sealed class BMS_Status_Message01 : BindableBase
    {
        public string Ready // 0 ~ 7, 0:close / 1:open
        {
            get => GetProperty(() => Ready);
            set => SetProperty(() => Ready, value);
        }
        public double SOC // 8 ~ 15, 0 ~ 100 %
        {
            get => GetProperty(() => SOC);
            set => SetProperty(() => SOC, value);
        }
        public double TotalCurrent // 16 ~ 31, -50 ~ 950 A
        {
            get => GetProperty(() => TotalCurrent);
            set => SetProperty(() => TotalCurrent, value);
        }
        public double TotalVoltage // 32 ~ 47, 0 ~ 500 V
        {
            get => GetProperty(() => TotalVoltage);
            set => SetProperty(() => TotalVoltage, value);
        }
        public byte MbmsState // 48 ~ 55, 0 ~ 0xFF
        {
            get => GetProperty(() => MbmsState);
            set => SetProperty(() => MbmsState, value);
        }
        public double DispSOC // 56 ~ 63, 0 ~ 100 %
        {
            get => GetProperty(() => DispSOC);
            set => SetProperty(() => DispSOC, value);
        }
    }
    public sealed class BMS_Status_Message02 : BindableBase
    {
        [FaultCode(0, "Module Connection Loss Fault")]
        public bool M_Connection // 0
        {
            get => GetProperty(() => M_Connection);
            set => SetProperty(() => M_Connection, value);
        }
        [FaultCode(6, "Cell Under Voltage Fault")]
        public bool C_UnderVolt // 6
        {
            get => GetProperty(() => C_UnderVolt);
            set => SetProperty(() => C_UnderVolt, value);
        }
        [FaultCode(7, "Cell Over Voltage Fault")]
        public bool C_OverVolt // 7
        {
            get => GetProperty(() => C_OverVolt);
            set => SetProperty(() => C_OverVolt, value);
        }
        [FaultCode(8, "Pack Under Voltage Fault")]
        public bool P_UnderVolt // 8
        {
            get => GetProperty(() => P_UnderVolt);
            set => SetProperty(() => P_UnderVolt, value);
        }
        [FaultCode(9, "Pack Over Voltage Fault")]
        public bool P_OverVolt // 9
        {
            get => GetProperty(() => P_OverVolt);
            set => SetProperty(() => P_OverVolt, value);
        }
        [FaultCode(10, "Charge Over Current Fault")]
        public bool ChargeOverCurr // 10
        {
            get => GetProperty(() => ChargeOverCurr);
            set => SetProperty(() => ChargeOverCurr, value);
        }
        [FaultCode(11, "Discharge Over Current Fault")]
        public bool DischargeOverCurr // 11
        {
            get => GetProperty(() => DischargeOverCurr);
            set => SetProperty(() => DischargeOverCurr, value);
        }
        [FaultCode(12, "High Temperature Fault")]
        public bool HighTemp // 12
        {
            get => GetProperty(() => HighTemp);
            set => SetProperty(() => HighTemp, value);
        }
        [FaultCode(13, "Low Temperature Fault")]
        public bool LowTemp // 13
        {
            get => GetProperty(() => LowTemp);
            set => SetProperty(() => LowTemp, value);
        }
        [FaultCode(14, "Module Temperature Imbalance Fault")]
        public bool M_TempImbal // 14
        {
            get => GetProperty(() => M_TempImbal);
            set => SetProperty(() => M_TempImbal, value);
        }
        [FaultCode(15, "Cell Voltage Imbalance Fault")]
        public bool C_VoltImbal // 15
        {
            get => GetProperty(() => C_VoltImbal);
            set => SetProperty(() => C_VoltImbal, value);
        }
        [FaultCode(17, "Cell Under SOC Fault")]
        public bool C_UnderSOC // 17
        {
            get => GetProperty(() => C_UnderSOC);
            set => SetProperty(() => C_UnderSOC, value);
        }
        public int PackNum // 24 ~ 31, 0 ~ 17
        {
            get => GetProperty(() => PackNum);
            set => SetProperty(() => PackNum, value);
        }
        public double MaxCellVolt // 32 ~ 47, 0 ~ 5000 mV
        {
            get => GetProperty(() => MaxCellVolt);
            set => SetProperty(() => MaxCellVolt, value);
        }
        public int MaxCellVoltNum // 48 ~ 55, 0 ~ 17
        {
            get => GetProperty(() => MaxCellVoltNum);
            set => SetProperty(() => MaxCellVoltNum, value);
        }
        public double Version // 56 ~ 63
        {
            get => GetProperty(() => Version);
            set => SetProperty(() => Version, value);
        }

        //public OccurredAlarm[] GetAlarmArray()
        //{
        //    return typeof(BMS_Status_Message02)
        //     .GetProperties().Where(p => p.PropertyType == typeof(bool))
        //     .Select(p => new OccurredAlarm
        //     {
        //         Name = p.Name,
        //         Value = (bool)p.GetValue(this)
        //     }).ToArray();
        //}
    }
    public sealed class BMS_Status_Message03 : BindableBase
    {
        public string MbmsReady // 0 ~ 7, 0:not ready / 1:ready
        {
            get => GetProperty(() => MbmsReady);
            set => SetProperty(() => MbmsReady, value);
        }
        public int PackReady // 8 ~ 23, 0x00000 ~ 0x1FFFF
        {
            get => GetProperty(() => PackReady);
            set => SetProperty(() => PackReady, value);
        }
        public int MaxPackCount // 24 ~ 31, 0 ~ 17
        {
            get => GetProperty(() => MaxPackCount);
            set => SetProperty(() => MaxPackCount, value);
        }
        public int CurrentPackCount // 32 ~ 39, 0 ~ 17
        {
            get => GetProperty(() => CurrentPackCount);
            set => SetProperty(() => CurrentPackCount, value);
        }
    }
    public sealed class BMS_Status_Message04 : BindableBase
    {
        public double CellMinVoltage // 0 ~ 15, 0 ~ 50.00
        {
            get => GetProperty(() => CellMinVoltage);
            set => SetProperty(() => CellMinVoltage, value);
        }
        public int CellMinPackNumber // 16 ~ 23, 0 ~ 17
        {
            get => GetProperty(() => CellMinPackNumber);
            set => SetProperty(() => CellMinPackNumber, value);
        }
        public double MaxTemperature // 24 ~ 31, -30 ~ 80
        {
            get => GetProperty(() => MaxTemperature);
            set => SetProperty(() => MaxTemperature, value);
        }
        public int MaxTemperaturePackNumber // 32 ~ 39, 0 ~ 17
        {
            get => GetProperty(() => MaxTemperaturePackNumber);
            set => SetProperty(() => MaxTemperaturePackNumber, value);
        }
        public double MinTemperature // 40 ~ 47, -30 ~ 80
        {
            get => GetProperty(() => MinTemperature);
            set => SetProperty(() => MinTemperature, value);
        }
        public int MinTemperaturePackNumber // 48 ~ 55, 0 ~ 17
        {
            get => GetProperty(() => MinTemperaturePackNumber);
            set => SetProperty(() => MinTemperaturePackNumber, value);
        }
    }
    public sealed class PackViewModel : BindableBase
    {
        public int PackNo { get; }
        public double MaxCellVoltage
        {
            get => GetProperty(() => MaxCellVoltage);
            set => SetProperty(() => MaxCellVoltage, value);
        }
        public double MinCellVoltage
        {
            get => GetProperty(() => MinCellVoltage);
            set => SetProperty(() => MinCellVoltage, value);
        }
        public double MaxTemperature
        {
            get => GetProperty(() => MaxTemperature);
            set => SetProperty(() => MaxTemperature, value);
        }

        public double MinTemperature
        {
            get => GetProperty(() => MinTemperature);
            set => SetProperty(() => MinTemperature, value);
        }
        public bool IsOnline
        {
            get => GetProperty(() => IsOnline);
            set => SetProperty(() => IsOnline, value);
        }

        public event PropertyChangedEventHandler PropertyChanged = delegate { };
        public void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        public PackViewModel(int packNo)
        {
            PackNo = packNo;
        }

    }
}
