

namespace BO.Integrace
{
    public class IsdocImport
    {
        public string doklad_id { get; set; }
        public string dodavatel_ico { get; set; }
        public string dodavatel_dic { get; set; }
        public string dodavatel_nazev { get; set; }
        public string dodavatel_ulice { get; set; }
        public string dodavatel_mesto { get; set; }
        public string dodavatel_psc { get; set; }
        public float castka_bezdph { get; set; }
        public float castka_dph { get; set; }
        public float sazba_dph { get; set; }
        public float castka_vcdphh { get; set; }
        public DateTime datum_vystaveni { get; set; }
        public DateTime datum_duzp { get; set; }
        public DateTime datum_splatnost { get; set; }

        public string doklad_text { get; set; }
        public string mena { get; set; }
        public string xml { get; set; }
    }
}
