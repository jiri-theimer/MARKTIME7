using UI.Views.Shared.Components.myPeriod;

namespace UI.Models
{
    public class VysledovkaViewModel : BaseViewModel
    {
        public string prefix { get; set; }
        public int pid { get; set; }

        public string VysledovkaHeader { get; set; }
        public BO.Model.TimesheetCostRateEnum ScenarNakladovaSazba { get; set; }
        public int ScenarVysledovkaVydaje { get; set; }
        public int ScenarVysledovkaHodiny { get; set; }
        public myPeriodViewModel periodinput { get; set; }

        
        public double NakladovaSazbaCislo { get; set; }
        public double NakladovaSazbaProcento { get; set; }


        public string Vystup_Overview1 { get; set; }
        public string Vystup_Overview2 { get; set; }
        public string Vystup_Osoba { get; set; }
        public string Vystup_Aktivita { get; set; }
        public string Vystup_Osoba_Aktivita { get; set; }
        public string Vystup_Aktivita_Osoba { get; set; }
        public string Vystup_Vysledovka1 { get; set; }
        public string Vystup_Fee { get; set; }
        public string Vystup_Expense { get; set; }
        public string Vystup_Analyza { get; set; }

        public bool IsVystup_Osoba { get; set; }
        public bool IsVystup_Aktivita { get; set; }
        public bool IsVystup_Osoba_Aktivita { get; set; }
        public bool IsVystup_Aktivita_Osoba { get; set; }
        public bool IsVystup_Analyza { get; set; }
        public bool IsVystup_Fee { get; set; }
        public bool IsVystup_Expense { get; set; }
        public bool IsVystup_Vysledovka1 { get; set; }

    }
}
