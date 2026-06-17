using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BO
{
    public class p15Location:BaseBO
    {
        public int x01ID { get; set; }
        public int j02ID_Owner { get; set; }
        public string p15Name { get; set; }
        public string p15Street { get; set; }
        public string p15City { get; set; }
        public string p15PostCode { get; set; }
        public string p15Country { get; set; }
        public string p15Notepad { get; set; }
        public double p15Longitude { get; set; }
        public double p15Latitude { get; set; }
        public int x04ID { get; set; }
        public string Owner { get; }

        public string x { get
            {
                return this.p15Longitude.ToString().Replace(",", ".");
            }
        }
        public string y
        {
            get
            {
                return this.p15Latitude.ToString().Replace(",", ".");
            }
        }
        public string getMapUrlFrame()
        {
            if (this.p15Longitude == 0 || this.p15Latitude == 0) return null;
            return $"https://frame.mapy.cz/zakladni?source=coor&id={this.x}%2C{this.y}&ds=1&x={this.x}&y={this.y}&z=17";
        }
        public string getMapUrlClassic()
        {
            if (this.p15Longitude == 0 || this.p15Latitude == 0) return null;
            return $"https://mapy.cz/zakladni?source=coor&id={this.x}%2C{this.y}&ds=1&x={this.x}&y={this.y}&z=17";
        }
    }
}
