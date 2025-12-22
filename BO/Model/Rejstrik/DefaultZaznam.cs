

namespace BO.Rejstrik
{
    public class DefaultZaznam
    {
        public string name { get; set; }
        public string street { get; set; }
        public string zipcode { get; set; }
        public string city { get; set; }
        public string ico { get; set; }
        public string dic { get; set; }
        public string country { get; set; }
        public string fulladdress { get; set; }
        public string errormessage { get; set; }

        public string pravniforma { get; set; }
        public string financniurad { get; set; }
        public DateTime datum_vzniku { get; set; }
        public DateTime datum_aktualizace { get; set; }
        public string sidlo_kodstatu { get; set; }
    }
}
