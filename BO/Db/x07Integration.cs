namespace BO
{
    public enum x07FlagEnum
    {
        Ecomail=1,
        SmartEmailing=2,    //jen historická hodnota: napojení nikdy nevzniklo, v nabídce typů už není
        AbraFlexi=3

    }
    public class x07Integration:BaseBO
    {
        public x07FlagEnum x07Flag { get; set; }
        public int x01ID { get; set; }
        public string x07Name { get; set; }
        public string x07Login { get; set; }
        public string x07Password { get; set; }
        public int x07Ordinary { get; set; }
        public string x07Token { get; set; }
        public string x07Url { get; set; }          //adresa služby (AbraFlexi: https://firma.flexibee.eu)
        public string x07Config { get; set; }       //nastavení podle typu integrace jako JSON (BO.x07Config)
        public DateTime? x07LastRun { get; set; }   //poslední běh
        public string x07LastError { get; set; }    //chyba posledního běhu (null = bez chyby)

        public string TypeName
        {
            get { return this.x07Flag.ToString(); }
        }
        //entita (prefix), nad jejímiž vybranými záznamy integrace pracuje
        public string RecordPrefix
        {
            get
            {
                switch (this.x07Flag)
                {
                    case x07FlagEnum.Ecomail: return "p28";
                    case x07FlagEnum.AbraFlexi: return "o23";
                    default: return null;
                }
            }
        }
    }

    //nastavení integrace uložené v x07Config
    public class x07Config
    {
        public string Company { get; set; }                 //AbraFlexi: identifikátor firmy v adrese (/c/{firma}/)
        public string HomeCurrency { get; set; } = "CZK";   //AbraFlexi: měna účetnictví
        public bool SendFiles { get; set; } = true;         //AbraFlexi: posílat i přílohy dokumentu
        public List<x07ConfigDocMap> DocMaps { get; set; } = new List<x07ConfigDocMap>();   //AbraFlexi: typ dokumentu -> druh dokladu
        public string LastListID { get; set; }              //Ecomail: naposledy použitý seznam kontaktů
    }
    public class x07ConfigDocMap
    {
        public int o18ID { get; set; }
        public string Evidence { get; set; }    //"pokladni-pohyb" | "faktura-prijata" | prázdné = neodesílat
        public string TypDokl { get; set; }     //kód typu dokladu v AbraFlexi
        public string Pokladna { get; set; }    //kód pokladny (jen pokladni-pohyb)
    }

    //vazba záznamu MARKTIME na záznam v cizím systému
    public class x08IntegrationBinding
    {
        public int x08ID { get; set; }
        public int x07ID { get; set; }
        public string x08RecordEntity { get; set; }
        public int x08RecordPid { get; set; }
        public string x08ExternalPid { get; set; }
        public DateTime? x08DateSync { get; set; }
    }

    //protokol běhů integrace
    public class x09IntegrationLog
    {
        public int x09ID { get; set; }
        public int x07ID { get; set; }
        public DateTime x09Date { get; set; }
        public string x09User { get; set; }
        public string x09RecordEntity { get; set; }
        public int x09CountAll { get; set; }
        public int x09CountOk { get; set; }
        public string x09Message { get; set; }
    }
}
