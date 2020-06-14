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

        // UI
        public static int CONTROL_WIDTH = 160;

        // get string from district
        public static Dictionary<District, string> DistrictNames = new Dictionary<District, string>
        {
            [District.Jerusalem] = "Jerusalem",
            [District.TelAviv] = "Tel Aviv",
            [District.Haifa] = "Haifa",
            [District.South] = "South",
            [District.Central] = "Central",
            [District.North] = "North"
        };

        // get string from city
        public static Dictionary<City, string> CityNames = new Dictionary<City, string>
        {
            [City.Jerusalem] = "Jerusalem",
            [City.BeitShemesh] = "Beit Shemesh",
            [City.TelAviv] = "Tel Aviv",
            [City.Holon] = "Holon",
            [City.BatYam] = "Bat Yam",
            [City.BneiBrak] = "Bnei Brak",
            [City.Haifa] = "Haifa",
            [City.Hadera] = "Hadera",
            [City.Ashdod] = "Ashdod",
            [City.Ashkelon] = "Ashkelon",
            [City.Arad] = "Arad",
            [City.RishonLetziyon] = "Rishon Letziyon",
            [City.PetahTikva] = "Petah Tikva",
            [City.Netanya] = "Netanya",
            [City.Rehovot] = "Rehovot",
            [City.Tzfat] = "Tzfat",
            [City.SeaOfGalilee] = "Sea Of Galilee",
            [City.Acre] = "Acre"
        };

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