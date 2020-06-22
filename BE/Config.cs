using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace BE
{
    public class Config
    {
        // serial keys
        public static long INITIAL_ORDER_KEY = 1;
        public static long INITIAL_HOST_KEY = 1;
        public static long INITIAL_HOSTING_UNIT_KEY = 1;
        public static long INITIAL_GUEST_REQUEST_KEY = 1;

        // Static counters for serial keys
        private static long _nextOrderKey;
        private static long _nextHostKey;
        private static long _nextHostingUnitKey;
        private static long _nextGuestRequestKey;

        // set to make sure no duplicate keys given
        public static long NextOrderKey
        {
            get => _nextOrderKey = (_nextOrderKey < INITIAL_ORDER_KEY) ? INITIAL_ORDER_KEY : _nextOrderKey;
            set => _nextOrderKey = value;
        }
        public static long NextHostKey
        {
            get => _nextHostKey = (_nextHostKey < INITIAL_HOST_KEY) ? INITIAL_HOST_KEY : _nextHostKey;
            set => _nextHostKey = value;
        }
        public static long NextHostingUnitKey
        {
            get => _nextHostingUnitKey = (_nextHostingUnitKey < INITIAL_HOSTING_UNIT_KEY) ? INITIAL_HOSTING_UNIT_KEY : _nextHostingUnitKey;
            set => _nextHostingUnitKey = value;
        }
        public static long NextGuestRequestKey
        {
            get => _nextGuestRequestKey = (_nextGuestRequestKey < INITIAL_GUEST_REQUEST_KEY) ? INITIAL_GUEST_REQUEST_KEY : _nextGuestRequestKey;
            set => _nextGuestRequestKey = value;
        }

        // Banking
        public static float TRANSACTION_FEE_NIS = 10;

        // Request
        public static int REQUEST_MAX_MONTHS_AHEAD = 11; // Maximum number of months a request can be booked ahead

        // Expiration
        public static int ORDER_OPEN_LIMIT_DAYS = 30;

        // Locations
        public static Dictionary<District, List<City>> GetCities = new Dictionary<District, List<City>>
        {
            [District.Jerusalem] = new List<City> { City.Jerusalem, City.BeitShemesh },
            [District.TelAviv] = new List<City> { City.TelAviv, City.Holon, City.BatYam, City.BneiBrak },
            [District.Haifa] = new List<City> { City.Haifa, City.Hadera },
            [District.South] = new List<City> { City.Ashdod, City.Ashkelon, City.Arad },
            [District.Central] = new List<City> { City.RishonLetziyon, City.PetahTikva, City.Netanya, City.Rehovot },
            [District.North] = new List<City> { City.Tzfat, City.SeaOfGalilee, City.Acre }
        };
    }
}