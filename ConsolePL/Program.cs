using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BE;
using BL;

namespace ConsolePL
{
    class Program
    {
        private static IBL MyBL = BL_Imp.GetBL();
        static void Main(string[] args)
        {
            DateTime entry = DateTime.Now.Date.AddDays(5);
            DateTime release = DateTime.Now.Date.AddDays(10);
            string fname = "Jonah";
            string lname = "Lawrence";
            string email = "jonah@google.com";
            District region = District.TelAviv;
            City city = City.BneiBrak;
            Dictionary<Amenity, PrefLevel> amenities = new Dictionary<Amenity, PrefLevel>
            {
                [Amenity.TV] = PrefLevel.Required,
                [Amenity.Pool] = PrefLevel.NotInterested,
                [Amenity.Kitchen] = PrefLevel.Required
            };

            GuestRequest guest = new GuestRequest(entry, release, fname, lname, email, region, city, amenities);

            MyBL.CreateGuestRequest(guest);

            List<GuestRequest> list = MyBL.GetGuestRequests();

            foreach (GuestRequest gr in list)
            {
                Console.WriteLine(gr);
            }

            List<string> citiesInNorth = Config.GetCities[District.North].ConvertAll(c => Config.CityNames[c]);

            foreach (string c in citiesInNorth)
            {
                Console.WriteLine(c);
            }

            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }
    }
}
