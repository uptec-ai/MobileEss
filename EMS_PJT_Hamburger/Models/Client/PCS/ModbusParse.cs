using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMS_PJT_Hamburger.Models.Client.PCS
{
    public static class ModbusParser
    {
        public static Dictionary<string, object> ParseRegisters(ushort[] registers, IEnumerable<ModbusFieldSpec> specs)
        {
            var result = new Dictionary<string, object>();

            foreach (var spec in specs)
            {
                long raw = 0;

                switch (spec.DataType)
                {
                    case ModbusDataType.U16: // ushort
                        raw = registers[spec.Address];
                        break;

                    case ModbusDataType.S16: // short
                        raw = (short)registers[spec.Address];
                        break;

                    case ModbusDataType.U32: // uint
                        raw = ((uint)registers[spec.Address] << 16)
                            | registers[spec.Address + 1];
                        break;

                    case ModbusDataType.S32: // int
                        raw = (registers[spec.Address] << 16)
                            | registers[spec.Address + 1];
                        break;
                }

                object value = raw * spec.Scale;

                result[spec.Name] = value;
            }

            return result;
        }
    }
    public class RegisterItem
    {
        public int Address { get; set; }
        public string Value { get; set; }
    }
}
