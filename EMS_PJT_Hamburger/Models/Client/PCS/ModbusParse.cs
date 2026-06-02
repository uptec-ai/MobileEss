using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMS_PJT_Hamburger.Models.Client.PCS
{
    public static class ModbusParser
    {
        public static Dictionary<string, object> ParseRegisters(
            ushort[] registers,
            IEnumerable<ModbusFieldSpec> specs,
            int startAddress = 0,
            ModbusWordOrder read32WordOrder = ModbusWordOrder.HighLow)
        {
            if (registers == null) throw new ArgumentNullException(nameof(registers));
            if (specs == null) throw new ArgumentNullException(nameof(specs));

            var result = new Dictionary<string, object>();

            foreach (var spec in specs)
            {
                if (spec == null || string.IsNullOrWhiteSpace(spec.Name))
                    continue;

                // spec.Address는 장치 절대 주소, registers는 startAddress 기준 상대 배열입니다.
                var index = spec.Address - startAddress;
                var length = spec.Length;
                if (index < 0 || index + length > registers.Length)
                    continue;

                long raw = 0;

                switch (spec.DataType)
                {
                    case ModbusDataType.U16: // ushort
                        raw = registers[index];
                        break;

                    case ModbusDataType.S16: // short
                        raw = (short)registers[index];
                        break;

                    case ModbusDataType.U32: // uint
                        raw = ReadU32(registers, index, read32WordOrder);
                        break;

                    case ModbusDataType.S32: // int
                        raw = unchecked((int)ReadU32(registers, index, read32WordOrder));
                        break;
                }

                object value = raw * spec.Scale;

                result[spec.Name] = value;
            }

            return result;
        }

        public static uint ReadU32(ushort[] registers, int index, ModbusWordOrder wordOrder)
        {
            if (registers == null) throw new ArgumentNullException(nameof(registers));
            if (index < 0 || index + 1 >= registers.Length)
                throw new ArgumentOutOfRangeException(nameof(index));

            var high = registers[index];
            var low = registers[index + 1];

            if (wordOrder == ModbusWordOrder.LowHigh)
            {
                low = registers[index];
                high = registers[index + 1];
            }

            return ((uint)high << 16) | low;
        }
    }
    public class RegisterItem
    {
        public int Address { get; set; }
        public string Value { get; set; }
    }
}
