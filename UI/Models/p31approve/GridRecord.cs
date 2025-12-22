

namespace UI.Models.p31approve
{
    public class GridRecord
    {
        public string p31guid { get; set; }
        public int pid { get; set; }
        public string Datum { get; set; }
        public string Jmeno { get; set; }
        public string hodiny { get; set; }
        public string hodinypausal { get; set; }
        public string hodinyinterni { get; set; }
        public double bezdph { get; set; }
        public double dphsazba { get; set; }
        public double sazba { get; set; }
        public string Popis { get; set; }
        public string PopisInterni { get; set; }
        public string Projekt { get; set; }
        public string p28name { get; set; }
        public string p41name { get; set; }
        public string p56name { get; set; }
        public string tabquery { get; set; }
        //public string pl { get; set; }
        public string Aktivita { get; set; }
        public string Sesit { get; set; }
        public bool fakturovatelne { get; set; }
        public string vykazano { get; set; }
        public string rozdil_vykazano_schvaleno_hodnota { get; set; }
        public string rozdil_vykazano_schvaleno_sazba { get; set; }
        public string rozdil_vykazano_schvaleno_bezdph { get; set; }
        public string emotion { get; set; }
        public string vykazano_sazba { get; set; }
        public string j27code { get; set; }
        public string honorar_orig { get; set; }
        public string honorar_schvaleno { get; set; }
        public int p33id { get; set; }
        public int p71id { get; set; }
        public int p72id { get; set; }
        public int uroven { get; set; }

        public string errormessage { get; set; }

        public string timestamp_insert { get; set; }
        public string timestamp_update { get; set; }


        public string cssclass { get; set; }
        public int b05id_last { get; set; }

        public int p40id_fixprice { get; set; }
        public string p40name { get; set; }
    }
}
