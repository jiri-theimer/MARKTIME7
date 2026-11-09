
namespace BO.Integrace
{
    /// <summary>Výsledek načtení ISDOC dokladu (BL.Code.IsdocImportSupport) pro import výdaje do úkonu.</summary>
    public class IsdocImport
    {
        public string doklad_id { get; set; }
        public int doklad_typ { get; set; }         //ISDOC DocumentType: 1 faktura, 2 opravný daňový doklad (dobropis), 3 vrubopis, 4 zálohová faktura, 5 daňový doklad k záloze, 6 opravný DD k záloze, 7 zjednodušený DD
        public string dodavatel_ico { get; set; }
        public string dodavatel_dic { get; set; }
        public string dodavatel_nazev { get; set; }
        public string dodavatel_ulice { get; set; }
        public string dodavatel_mesto { get; set; }
        public string dodavatel_psc { get; set; }
        public double castka_bezdph { get; set; }   //double: float měl jen ~7 platných číslic
        public double castka_dph { get; set; }
        public double sazba_dph { get; set; }       //při více sazbách sazba s největším základem (viz sazby, upozorneni)
        public double castka_vcdphh { get; set; }
        public List<double> sazby { get; set; } = new List<double>();
        public bool platce_dph { get; set; }        //ISDOC VATApplicable
        public DateTime? datum_vystaveni { get; set; }
        public DateTime? datum_duzp { get; set; }   //TaxPointDate je nepovinné (neplátce) – použít datum_plneni
        public DateTime? datum_splatnost { get; set; }
        public DateTime? datum_plneni => datum_duzp ?? datum_vystaveni;
        public string variabilni_symbol { get; set; }
        public string doklad_text { get; set; }
        public string mena { get; set; }
        public List<string> upozorneni { get; set; } = new List<string>();     //co má uživatel po importu zkontrolovat
    }
}
