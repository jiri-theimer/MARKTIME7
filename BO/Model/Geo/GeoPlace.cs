using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;

namespace BO.Geo
{
    public class GeoPlace
    {
        public float Longitude { get; set; }
        public float Latitude { get; set; }
        public string AddressInput { get; set; }
        public string FormattedAddress { get; set; }
        public string Locality { get; set; }
        public string PostalCode { get; set; }
        public string CountryRegion { get; set; }
        public string ErrorMessage { get; set; }
        public bool IsError {
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
    }
}
