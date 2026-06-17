namespace BO
{
    public class GpcRecord          //ČSOB dokumentace struktury GPC: https://www.csob.cz/portal/documents/10710/1927786/format-gpc.pdf
    {
        public string TypZaznamu { get; set; }      //1-3
        public string MujUcet { get; set; }         //4-19
        public string CisloUctuProtistrany { get; set; }       //20-35
        public string IdentifikatorTransakce { get; set; }    //36-48
        public string CastkaTransakce { get; set; }    //49-60, číslo, pro CZK v haléřích!
        public string KodUctovani { get; set; }     //61, 1:odchozí platba, 2:příchozí platba,4:storno, 5:storno
        public string VariabilniSymbol { get; set; }    //62-71, číslo
        public string Oddelovac { get; set; }           //vždy 00
        public string KodBankyProtistrany { get; set; }
        public string KonstantniSymbol { get; set; }    //72-81, číslo
        public string SpecifickySymbol { get; set; }    //82-91, číslo
        
        public string Popis { get; set; }    //98-117
        public string DatumValuty { get; set; }
        public string KodMeny { get; set; }    //119-122, CZK:00203, EUR:00978, GBP:00826, CHF: 00756, PLN: 00985, RUB: 00643, SEK: 00752, USD: 00840
        //public string DatumZauctovani { get; set; }    //123-128
        
        public int p91ID { get; set; }
        public double CurrentDebt { get; set; }

        public string TheProtistranaUcet
        {
            get
            {
                if (string.IsNullOrEmpty(this.CisloUctuProtistrany) || string.IsNullOrEmpty(this.KodBankyProtistrany))
                {
                    return null;
                }
                return this.CisloUctuProtistrany.TrimStart('0') + "/" + this.KodBankyProtistrany.TrimStart('0');
            }
        }
        
        public string TheVariabilniSymbol
        {
            get
            {
                return this.VariabilniSymbol.TrimStart('0');
            }
        }
        public double TheCastka { get
            {
                return Convert.ToDouble(this.CastkaTransakce) / 100;
            }
        }
        public string TheMena
        {
            get
            {
                switch (this.KodMeny)
                {
                    case "00203": return "CZK";
                    case "00978": return "EUR";
                    case "00840": return "USD";
                    case "00756": return "CHF";
                    case "00826": return "GBP";
                    case "00985": return "PLN";
                    case "00752": return "SEK";
                    default:
                        return "???";
                }
            }
        }
        public string ThePopis
        {
            get
            {
                return this.Popis.Trim();
            }
        }
        public DateTime? TheDatum
        {
            get
            {
                if (string.IsNullOrEmpty(this.DatumValuty) || this.DatumValuty.Length<6)
                {
                    return null;
                }
                int d = Convert.ToInt32(this.DatumValuty.Substring(0, 2));
                int m = Convert.ToInt32(this.DatumValuty.Substring(2, 2));
                int y = 2000+Convert.ToInt32(this.DatumValuty.Substring(4, 2));

                return new DateTime(y, m, d);
            }
        }
    }
}
