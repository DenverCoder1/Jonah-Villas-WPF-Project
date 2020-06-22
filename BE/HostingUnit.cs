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

        public float TotalCommissionsNIS { get; set; }

        public HostingUnit()
        {
            Calendar = new List<DateRange>();
        }

        public HostingUnit(Host owner, string name, District district, City city)
        {
            // get next available serial number
            HostingUnitKey = Config.NextHostingUnitKey++;
            Calendar = new List<DateRange>();
            Owner = owner;
            UnitName = name;
            UnitDistrict = district;
            UnitCity = city;
            TotalCommissionsNIS = 0;
        }

        public string FullDetails()
        {
            StringBuilder output = new StringBuilder();
            output.AppendLine($"Hosting unit ID: {HostingUnitKey}");
            output.AppendLine($"Hosting unit name: {UnitName}");
            output.AppendLine($"Hosting unit location: {UnitCity}, {UnitDistrict}");
            output.AppendLine($"Hosting unit total commissions: {TotalCommissionsNIS} NIS");
            output.AppendLine($"Owner ID: {Owner.HostKey}\n");
            if (Calendar.Count == 0)
                output.AppendLine("No dates have been reserved.");
            foreach (DateRange dr in Calendar)
            {
                output.AppendLine(dr.ToString());
            }
            return output.ToString();
        }

        // list all reserved date ranges
        public override string ToString()
        {
            StringBuilder output = new StringBuilder();
            // add serial number to output
            output.AppendLine($"#{HostingUnitKey} : \"{UnitName}\" in {UnitCity}, {UnitDistrict}");
            output.Append($"Unit commissions: {TotalCommissionsNIS} NIS | Owner ID: {Owner.HostKey}");
            return output.ToString();
        }
    }
}
