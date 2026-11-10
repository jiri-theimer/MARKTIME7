namespace UI.Models
{
    /// <summary>Průvodce „Vykázat z Outlooku“ (OutlookP31Controller): události přetažené z Outlooku → návrhy časových úkonů.</summary>
    public class OutlookP31ViewModel : BaseViewModel
    {
        public List<OutlookP31Row> Rows { get; set; } = new List<OutlookP31Row>();
        public bool ShowSkipped { get; set; }

        // hromadné nastavení vybraným řádkům (jen pro combo, neukládá se)
        public int BulkP41ID { get; set; }
        public string BulkComboP41 { get; set; }
        public int BulkP32ID { get; set; }
        public string BulkComboP32 { get; set; }

        public int CountNew => Rows.Count(p => p.State == OutlookP31State.New || p.State == OutlookP31State.Error);
        public int CountSelected => Rows.Count(p => p.IsSelected && (p.State == OutlookP31State.New || p.State == OutlookP31State.Error || p.State == OutlookP31State.Skipped));
    }

    public enum OutlookP31State
    {
        New = 0,        // návrh k uložení
        Skipped = 1,    // celodenní, zrušená, odmítnutá, volno, export z MARKTIME
        Duplicate = 2,  // už zapsáno (p31ExternalCode)
        Saved = 3,      // uloženo v tomto průvodci
        Error = 4       // ukládání selhalo, řádek zůstává k opravě
    }

    public class OutlookP31Row
    {
        public string Key { get; set; }             // p31ExternalCode
        public bool IsSelected { get; set; }
        public OutlookP31State State { get; set; }
        public string Message { get; set; }         // důvod přeskočení / chyba uložení
        public string Warning { get; set; }         // překryv s již zapsanými úkony apod.
        public int p31ID { get; set; }              // existující / uložený úkon

        public DateTime Date { get; set; }
        public string TimeFrom { get; set; }
        public string TimeUntil { get; set; }
        public string Hours { get; set; }           // HH:mm

        public string Subject { get; set; }
        public string Attendees { get; set; }
        public string FileName { get; set; }
        public string p31Text { get; set; }
        public string p31TextInternal { get; set; }

        public int p41ID { get; set; }
        public string ComboP41 { get; set; }
        public int p32ID { get; set; }
        public string ComboP32 { get; set; }

        public int Confidence { get; set; }         // 0 nenalezeno, 1 nízká, 2 střední, 3 vysoká
        public string Reason { get; set; }          // proč tento projekt
        public string Client { get; set; }
    }
}
