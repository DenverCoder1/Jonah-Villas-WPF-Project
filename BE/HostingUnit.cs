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
        // Owner - storing hosts separately and linking unit to host with key
        public long OwnerHostID { get; set; }
        // Unit name
        public string UnitName { get; set; }
        // Unit location
        public District UnitDistrict { get; set; }
        public City UnitCity { get; set; }
        public TypeOfPlace UnitType { get; set; }
        public List<Amenity> Amenities { get; set; }
        public float TotalCommissionsNIS { get; set; }

        public HostingUnit()
        {
            Calendar = new List<DateRange>();
        }

        public HostingUnit(Host owner, string name, District district, City city, TypeOfPlace type, List<Amenity> amenities)
        {
            // get next available serial number
            HostingUnitKey = Config.NextHostingUnitKey++;
            Calendar = new List<DateRange>();
            OwnerHostID = owner.HostKey;
            UnitName = name;
            UnitDistrict = district;
            UnitCity = city;
            UnitType = type;
            Amenities = amenities;
            TotalCommissionsNIS = 0;
        }

        // list all reserved date ranges
        public override string ToString()
        {
            StringBuilder output = new StringBuilder();
            // add serial number to output
            output.AppendLine($"#{HostingUnitKey} : \"{UnitName}\" in {UnitCity}, {UnitDistrict}");
            output.AppendLine($"Unit commissions: {TotalCommissionsNIS} NIS | Owner ID: {OwnerHostID}");
            output.Append($"Unit type: {UnitType}");
            return output.ToString();
        }
    }
}
