using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMS_PJT_Hamburger.Models.Client.BMS
{
    public enum CanIdType { STD, XTD }
    public sealed class CanFieldSpec
    {
        // CAN pay load(8byte) 기준 비트 위치
        public int BitOffset { get; set; } // 0 ~ 63
        public int BitLength { get; set; }
        public string Name { get; set; }
        public Func<ulong, object> Convert { get; set; } // null 가능
        public CanFieldSpec()
        {
            Name = string.Empty;
        }
    }
    public sealed class CanMessageSpec
    {
        public string Name { get; set; }
        public uint CanId { get; set; }
        public CanIdType IdType { get; set; }
        public int Dlc { get; set; }
        public int PollMs { get; set; }
        public IList<CanFieldSpec> Fields { get; set; }

        public CanMessageSpec()
        {
            Name = string.Empty;
            Dlc = 8;
            PollMs = 200;
            Fields = new List<CanFieldSpec>();
        }
    }
    [AttributeUsage(AttributeTargets.Property)]
    public class FaultCodeAttribute : Attribute
    {
        public int AlarmCode { get; private set; }
        public string AlarmName { get; private set; }
        public FaultCodeAttribute(int code, string name)
        {
            AlarmCode = code;
            AlarmName = name;
        }
    }
}
