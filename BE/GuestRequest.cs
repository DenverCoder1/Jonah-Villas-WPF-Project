﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BE
{
    public class GuestRequest
    {
        // static counter for serial keys
        public static long stGuestRequestKey = Config.INITIAL_GUEST_REQUEST_KEY;

        // request details
        public long GuestRequestKey { get; set; }
        public DateTime EntryDate { get; set; }
        public DateTime ReleaseDate { get; set; }
        public GuestStatus Status { get; set; }
        public int NumAdults { get; set; }
        public int NumChildren { get; set; }

        // Guest info
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }

        // preferences
        public District PrefDistrict { get; set; }
        public City PrefCity { get; set; }
        public Type PrefType { get; set; }
        public Dictionary<Amenity, PrefLevel> PrefAmenities { get; set; }

        public GuestRequest()
        {
            GuestRequestKey = ++stGuestRequestKey;
            Status = GuestStatus.Open;
            PrefAmenities = new Dictionary<Amenity, PrefLevel>();
        }
        
        public GuestRequest(
            DateTime entry,
            DateTime release, 
            string fname, 
            string lname, 
            string email,
            District region,
            City city,
            Dictionary<Amenity, PrefLevel> amenities)
        {
            EntryDate = entry;
            ReleaseDate = release;
            Status = GuestStatus.Open;
            FirstName = fname;
            LastName = lname;
            Email = email;
            PrefDistrict = region;
            PrefCity = city;
            PrefAmenities = amenities;
        }

        public GuestRequest Clone()
        {
            GuestRequest Clone = new GuestRequest
            {
                // request details
                GuestRequestKey = this.GuestRequestKey,
                EntryDate = this.EntryDate,
                ReleaseDate = this.ReleaseDate,
                Status = this.Status,
                NumAdults = this.NumAdults,
                NumChildren = this.NumChildren,

                // Guest info
                FirstName = this.FirstName,
                LastName = this.LastName,
                Email = this.Email,

                // preferences
                PrefDistrict = this.PrefDistrict,
                PrefCity = this.PrefCity,
                PrefType = this.PrefType,
                PrefAmenities = this.PrefAmenities
            };
            return Clone;
        }

        public override string ToString()
        {
            StringBuilder output = new StringBuilder();

            output.AppendLine("Entry Date: " + EntryDate.ToString("dd.MM.yyyy"));
            output.AppendLine("Release Date: " + ReleaseDate.ToString("dd.MM.yyyy"));
            output.AppendLine("Is Approved: " + Status.ToString());

            return output.ToString();
        }
    }
}
