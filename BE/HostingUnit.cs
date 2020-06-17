using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BE
{
    public class HostingUnit
    {
        // Serial key
        public long HostingUnitKey { get; set; }
        // Calendar
        public List<DateRange> Calendar { get; set; }
        // Owner
        public Host Owner { get; set; }
        // Unit Name
        public string UnitName { get; set; }

        public HostingUnit()
        {
            // get next available serial number
            HostingUnitKey = ++Config.stHostingUnitKey;
            Calendar = new List<DateRange>();
        }

        public HostingUnit(Host owner, string name)
        {
            // get next available serial number
            HostingUnitKey = ++Config.stHostingUnitKey;
            Calendar = new List<DateRange>();
            Owner = owner;
            UnitName = name;
        }

        // list all reserved date ranges
        public override string ToString()
        {
            StringBuilder output = new StringBuilder();
            // add serial number to output
            output.Append($"#{HostingUnitKey} : Name: {UnitName}, ");
            output.Append($"Owner ID: {Owner.HostKey}");
            return output.ToString();
        }
    }
}
