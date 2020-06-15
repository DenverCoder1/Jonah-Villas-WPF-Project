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
        public static long INITIAL_HOSTING_UNIT_KEY = 1;
        public static long INITIAL_GUEST_REQUEST_KEY = 1;

        // Static counters for serial keys
        public static long stOrderKey = INITIAL_ORDER_KEY;
        public static long stHostingUnitKey = INITIAL_HOSTING_UNIT_KEY;
        public static long stGuestRequestKey = INITIAL_GUEST_REQUEST_KEY;

        // UI
        public static int CONTROL_WIDTH = 160;

        // locations mapping
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