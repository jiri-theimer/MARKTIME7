using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BO
{
    public class j95GeoWeatherLog
    {
        public int j95ID { get; set; }
        public DateTime j95Date { get; set; }
        public int p15ID { get; set; }
        public float j95Longitude { get; set; }
        public float j95Latitude { get; set; }
        public float j95Temp { get; set; }
        public float j95TempFeelsLike { get; set; }
        public float j95TempMin { get; set; }
        public float j95TempMax { get; set; }
        public int j95Pressure { get; set; }
        public int j95Humidity { get; set; }
        public float j95WindSpeed { get; set; }
        public int j95WindDeg { get; set; }
        public float j95WindGust { get; set; }
        public string j95Description { get; set; }
        public string j95Country { get; set; }
        public string j95Icon { get; set; }
        public string j95ErrorMessage { get; set; }
        public string j95Location { get; set; }
        public int j95Visibility { get; set; }

        public string LocalAddress
        {
            get
            {
                return this.j95Location + "/" + this.j95Country;
            }
        }
        public string IconUrl
        {
            get
            {
                return $"http://openweathermap.org/img/wn/{this.j95Icon}.png";
            }
        }
    }
}
