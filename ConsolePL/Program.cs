using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using BE;
using BL;

namespace ConsolePL
{
    class Program
    {
        private static IBL Bl = FactoryBL.Build();

        private static string PascalCaseToText(object value)
        {
            string enumString = value.ToString();
            return Regex.Replace(enumString, "(.[A-Z])", " $1");
        }

        static void Main(string[] args)
        {
            Order order = new Order(1, 2);

            Bl.CreateOrder(order);

            order.Status = OrderStatus.ClosedByCustomerResponse;

            Bl.UpdateOrder(order);

            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();


            DateTime entry = DateTime.Today.AddDays(5);
            DateTime release = DateTime.Today.AddDays(10);
            string fname = "Jonah";
            string lname = "Lawrence";
            string email = "jonah@google.com";
            District region = District.TelAviv;
            City city = City.BneiBrak;
            BE.TypeOfPlace type = BE.TypeOfPlace.Apartment;
            Dictionary<Amenity, PrefLevel> amenities = new Dictionary<Amenity, PrefLevel>
            {
                [Amenity.TV] = PrefLevel.Required,
                [Amenity.Pool] = PrefLevel.NotInterested,
                [Amenity.Kitchen] = PrefLevel.Required
            };

            GuestRequest guest = new GuestRequest(entry, release, fname, lname, email, region, city, type, 6, 8, amenities);

            Bl.CreateGuestRequest(guest);

            List<GuestRequest> list = Bl.GetGuestRequests();

            foreach (GuestRequest gr in list)
            {
                Console.WriteLine(gr);
            }

            List<string> citiesInNorth = Config.GetCities[District.North].ConvertAll(c => PascalCaseToText(c));

            foreach (string c in citiesInNorth)
            {
                Console.WriteLine(c);
            }

            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }
    }
}
