using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMS_PJT_Hamburger.Models.Client.PCS
{
    public enum ModbusDataType
    {
        U16,
        S16,
        U32,
        S32,
    }
    public sealed class ModbusFieldSpec
    {
        public int Address { get; set; } // Register address
        public ModbusDataType DataType { get; set; }
        public string Name { get; set; }
        public double Scale { get; set; }
        public string Unit { get; set; }
        public ModbusFieldSpec()
        {
            Name = string.Empty;
            Unit = string.Empty;
        }
        public int Length
        {
            get
            {
                if (DataType == ModbusDataType.U32 || DataType == ModbusDataType.S32)
                    return 2;
                return 1;
            }
        }
    }
    public sealed class ModbusWriteSpec
    {
        public ushort Address { get; set; }
        public double Scale { get; set; }
    }
}
