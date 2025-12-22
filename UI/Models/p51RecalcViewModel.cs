namespace UI.Models
{
    public class p51RecalcViewModel:BaseViewModel
    {
        public int p51id { get; set; }
        public bool isafter_p51record { get; set; }
        public int pocetRozpracovanychUkonu { get; set; }
        public BO.p51PriceList Rec { get; set; }
        public string flag { get; set; }
        public DateTime? d1 { get; set; }
        public DateTime? d2 { get; set; }

        public string CostRatesRecalcFlag { get; set; } //drzet_cenik/bez_ohledu_na_cenik
    }
}
