﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BE
{
    public class GuestRequest
    {
        // request details
        public long GuestRequestKey { get; set; }
        public DateTime EntryDate { get; set; }
        public DateTime ReleaseDate { get; set; }
        public GuestStatus Status { get; set; }
        public int NumAdults { get; set; }
        public int NumChildren { get; set; }
        public DateTime InquiryDate { get; set; } // Registration date

        // Guest info
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }

        // preferences
        public District PrefDistrict { get; set; } // Area
        public City PrefCity { get; set; } // Sub-area
        public TypeOfPlace PrefType { get; set; }
        public SerializableDictionary<Amenity, PrefLevel> PrefAmenities { get; set; }

        public GuestRequest()
        {
            Status = GuestStatus.Open;
            PrefAmenities = new SerializableDictionary<Amenity, PrefLevel>();
            InquiryDate = DateTime.Now;
        }
        
        public GuestRequest(
            DateTime entry,
            DateTime release, 
            string fname, 
            string lname, 
            string email,
            District region,
            City city,
            TypeOfPlace type,
            int numAdults,
            int numChildren,
            SerializableDictionary<Amenity, PrefLevel> amenities)
        {
            GuestRequestKey = Config.NextGuestRequestKey++;
            Status = GuestStatus.Open;
            EntryDate = entry;
            ReleaseDate = release;
            InquiryDate = DateTime.Now;
            FirstName = fname;
            LastName = lname;
            Email = email;
            PrefDistrict = region;
            PrefCity = city;
            PrefType = type;
            NumAdults = numAdults;
            NumChildren = numChildren;
            PrefAmenities = amenities;
        }

        public override string ToString()
        {
            StringBuilder output = new StringBuilder();
            output.Append($"#{GuestRequestKey} : {LastName}, {FirstName} | ");
            output.Append($"Inquiry date: {InquiryDate:dd.MM.yyyy} | ");
            output.Append($"Status: {Status}\n");
            output.Append($"{NumAdults} Adults {NumChildren} Children | ");
            output.Append($"{PrefType} | {PrefCity}, {PrefDistrict} | ");
            output.Append($"{EntryDate:dd.MM.yyyy}-{ReleaseDate:dd.MM.yyyy}\n");
            IEnumerable<Amenity> matches = from KeyValuePair<Amenity, PrefLevel> item in PrefAmenities
                                           where item.Value == PrefLevel.Required
                                           select item.Key;
            string amenities = String.Join(", ", matches.ToList());
            output.Append($"Amenities: {(amenities != "" ? amenities : "None")}");
            return output.ToString();
        }
    }
}
