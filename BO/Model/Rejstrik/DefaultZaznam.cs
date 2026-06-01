

namespace BO.Rejstrik
{
    public class DefaultZaznam
    {
        public string name { get; set; }
        public string street { get; set; }
        public string zipcode { get; set; }
        public string city { get; set; }
        public string city_extended { get; set; }
        public string city_part { get; set; }
        public string ico { get; set; }
        
        public string dic { get; set; }
        public string country { get; set; }
        
        public string errormessage { get; set; }

        public string pravniForma { get; set; }
        public string financniUrad { get; set; }
        public DateTime datumVzniku { get; set; }
        public DateTime datumAktualizace { get; set; }
        public string sidlo_kodStatu { get; set; }

        public string fullcity
        {
            get
            {
                if (!string.IsNullOrEmpty(this.city_extended))
                {
                    return this.city_extended;
                }
                else
                {
                    return this.city;
                }

            }
        }
        public string fulladdress
        {
            get
            {
                string s = this.street;

                if (string.IsNullOrEmpty(s))
                {
                    s = this.fullcity;
                }
                else
                {
                    s += ", " + this.fullcity;
                }
                if (!string.IsNullOrEmpty(this.zipcode))
                {
                    s += ", " + this.zipcode;
                }
                if (!string.IsNullOrEmpty(this.country))
                {
                    s += ", " + this.country;
                }

                return s;
            }
        }

    }
}
