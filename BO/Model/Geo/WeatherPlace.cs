using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BO.Geo
{
    public class WeatherPlace
    {
        public double LongitudeInput { get; set; }
        public double LatitudeInput { get; set; }

        public float Temp { get; set; }
        public float TempFeelsLike { get; set; }
        public float TempMin { get; set; }
        public float TempMax { get; set; }
        public int Humidity { get; set; }
        public int Pressure { get; set; }
        public float WindSpeed { get; set; }
        public int WindDeg { get; set; }
        public float WindGust { get; set; } //síla poryvu větru
        public int Visibility { get; set; } //viditelnost v metrech
        public string Description { get; set; }
        public string Icon { get; set; }
        public string Country { get; set; }
        public string Location { get; set; }
        public int Sunset { get; set; }
        public int Sunrise { get; set; }

        public string ErrorMessage { get; set; }
        public bool IsError
        {
            get
            {
                if (this.ErrorMessage != null)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        public string LocalAddress
        {
            get
            {
                return this.Location + "/" + this.Country;
            }
        }

        public string IconUrl
        {
            get
            {
                return $"http://openweathermap.org/img/wn/{this.Icon}@2x.png";
            }
        }
    }
}
