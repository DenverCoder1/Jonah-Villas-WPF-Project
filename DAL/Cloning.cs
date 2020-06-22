using BE;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL
{
    public static class Cloning
    {
        public static BankBranch Clone(this BankBranch original)
        {
            BankBranch clone = new BankBranch
            {
                BankCode = original.BankCode,
                BankName = original.BankName,
                BranchCode = original.BranchCode,
                BranchAddress = original.BranchAddress,
                BranchCity = original.BranchCity
            };
            return clone;
        }

        public static GuestRequest Clone(this GuestRequest original)
        {
            GuestRequest clone = new GuestRequest
            {
                // request details
                GuestRequestKey = original.GuestRequestKey,
                EntryDate = original.EntryDate,
                ReleaseDate = original.ReleaseDate,
                Status = original.Status,
                NumAdults = original.NumAdults,
                NumChildren = original.NumChildren,
                InquiryDate = original.InquiryDate,

                // Guest info
                FirstName = original.FirstName,
                LastName = original.LastName,
                Email = original.Email,

                // preferences
                PrefDistrict = original.PrefDistrict,
                PrefCity = original.PrefCity,
                PrefType = original.PrefType,
                PrefAmenities = new SerializableDictionary<Amenity, PrefLevel>()
            };
            foreach (KeyValuePair<Amenity, PrefLevel> item in original.PrefAmenities)
            {
                clone.PrefAmenities[item.Key] = item.Value;
            }
            return clone;
        }

        public static Host Clone(this Host original)
        {
            Host clone = new Host
            {
                HostKey = original.HostKey,
                FirstName = original.FirstName,
                LastName = original.LastName,
                Email = original.Email,
                PhoneNumber = original.PhoneNumber,
                BankDetails = original.BankDetails,
                BankClearance = original.BankClearance,
                AmountCharged = original.AmountCharged
            };
            return clone;
        }

        public static HostingUnit Clone(this HostingUnit original)
        {
            HostingUnit clone = new HostingUnit
            {
                HostingUnitKey = original.HostingUnitKey,
                Calendar = new List<DateRange>(),
                Owner = original.Owner,
                UnitName = original.UnitName,
                UnitCity = original.UnitCity,
                UnitDistrict = original.UnitDistrict,
                TotalCommissionsNIS = original.TotalCommissionsNIS
            };
            foreach (DateRange d in original.Calendar)
            {
                clone.Calendar.Add(d);
            }
            return clone;
        }

        public static Order Clone(this Order original)
        {
            Order clone = new Order
            {
                HostingUnitKey = original.HostingUnitKey,
                GuestRequestKey = original.GuestRequestKey,
                OrderKey = original.OrderKey,
                Status = original.Status,
                CreationDate = original.CreationDate,
                EmailDeliveryDate = original.EmailDeliveryDate
            };
            return clone;
        }
    }
}
