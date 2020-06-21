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
        // Calendar of reserved dates - This method of storing reserved dates was approved by the professor
        public List<DateRange> Calendar { get; set; }
        // Owner
        public Host Owner { get; set; }
        // Unit name
        public string UnitName { get; set; }
        // Unit location
        public District UnitDistrict { get; set; }
        public City UnitCity { get; set; }

        public HostingUnit()
        {
            // get next available serial number
            HostingUnitKey = Config.stHostingUnitKey++;
            Calendar = new List<DateRange>();
        }

        public HostingUnit(Host owner, string name, District district, City city)
        {
            // get next available serial number
            HostingUnitKey = Config.stHostingUnitKey++;
            Calendar = new List<DateRange>();
            Owner = owner;
            UnitName = name;
            UnitDistrict = district;
            UnitCity = city;
        }

        // list all reserved date ranges
        public override string ToString()
        {
            StringBuilder output = new StringBuilder();
            // add serial number to output
            output.AppendLine($"#{HostingUnitKey} : \"{UnitName}\" in {UnitCity}, {UnitDistrict}");
            output.Append($"Owner ID: {Owner.HostKey}");
            return output.ToString();
        }
    }
}
