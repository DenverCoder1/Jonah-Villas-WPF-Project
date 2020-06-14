using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BE
{
    public class HostingUnit
    {
        // static counter for serial keys
        private static long stHostingUnitKey = Config.INITIAL_HOSTING_UNIT_KEY;
        // serial key
        public long HostingUnitKey { get; private set; }
        // calendar
        public List<DateRange> Calendar { get; set; }
        // Owner
        public Host Owner { get; set; }
        // Name
        public string UnitName { get; set; }

        public HostingUnit()
        {
            // get next available serial number
            HostingUnitKey = ++stHostingUnitKey;
            Calendar = new List<DateRange>();
        }

        public HostingUnit(Host owner, string name)
        {
            // get next available serial number
            HostingUnitKey = ++stHostingUnitKey;
            Calendar = new List<DateRange>();
            Owner = owner;
            UnitName = name;
        }

        public HostingUnit Clone()
        {
            HostingUnit Clone = new HostingUnit
            {
                HostingUnitKey = this.HostingUnitKey,
                Calendar = new List<DateRange>(),
                Owner = this.Owner,
                UnitName = this.UnitName
            };
            foreach (DateRange d in Calendar)
            {
                Clone.Calendar.Add(d);
            }
            return Clone;
        }

        // list all reserved date ranges
        public override string ToString()
        {
            StringBuilder output = new StringBuilder();
            // add serial number to output
            output.AppendLine("Serial Number: " + HostingUnitKey);

            // list reserved sequences
            for (int i = 0; i < Calendar.Count; ++i)
            {
                output.AppendLine(Calendar[i].ToString());
            }

            return output.ToString();
        }
    }
}
