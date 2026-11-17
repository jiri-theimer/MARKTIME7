namespace UI.Models.r11
{
    public class r11ViewModel : BaseViewModel
    {
        public int p41id { get; set; }              //kontext jednoho projektu, 0 = všichni lidé se stropem
        public BO.p41Project RecP41 { get; set; }
        public bool CanEditContext { get; set; }    //smí přihlášený upravovat kapacitní plán projektu v kontextu
        public string Zoom { get; set; }            //day|week|month
        public int Months { get; set; }             //počet zobrazených měsíců

        //nový strop: výběr osoby a projektu
        public int NewJ02ID { get; set; }
        public string NewJ02Name { get; set; }
        public int NewP41ID { get; set; }
        public string NewP41Name { get; set; }
    }

    public class r11Block
    {
        public int id { get; set; }
        public string d1 { get; set; }
        public string d2 { get; set; }
        public double hours { get; set; }
        public string text { get; set; }
    }

    public class r11Row
    {
        public int r11id { get; set; }
        public int p41id { get; set; }
        public string project { get; set; }
        public string code { get; set; }
        public string client { get; set; }
        public string text { get; set; }
        public double fa { get; set; }
        public double nefa { get; set; }
        public double total { get; set; }
        public double planned { get; set; }         //součet bloků
        public double actfa { get; set; }           //vykázáno Fa
        public double actnefa { get; set; }         //vykázáno Nefa
        public int edit { get; set; }               //1: může upravit, 2: rozhodne server při uložení
        public bool ctx { get; set; }               //řádek projektu, ze kterého je stránka otevřená
        public string pf { get; set; }              //plánované období projektu
        public string pu { get; set; }
        public bool spread { get; set; }            //bez bloků: strop je rozložený do období projektu
        public List<r11Block> blocks { get; set; }

        [Newtonsoft.Json.JsonIgnore]                    //aplikace serializuje JSON přes Newtonsoft (Startup: AddNewtonsoftJson)
        [System.Text.Json.Serialization.JsonIgnore]
        public double[] dayplan { get; set; }       //naplánované hodiny řádku po dnech; jen pro export, do prohlížeče se neposílá
    }

    public class r11Person
    {
        public int j02id { get; set; }
        public string name { get; set; }
        public double[] cap { get; set; }           //pracovní fond po dnech zobrazeného období
        public double[] plan { get; set; }          //naplánované hodiny po dnech (všechny projekty osoby)
        public int hidden { get; set; }             //počet projektů, které přihlášený nevidí (v plan jsou započtené)
        public List<r11Row> rows { get; set; }
    }
}
