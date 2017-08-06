using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BotLibrary
{
    public class GeoLocation
    {
        public string Latidute { get; private set; }
        public string Longitude { get; private set; }

        public GeoLocation(string latitude, string longitude)
        {
            Latidute = latitude;
            Longitude = longitude;
        }
    }
}