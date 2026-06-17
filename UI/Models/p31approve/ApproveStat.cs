namespace UI.Models.p31approve
{
    public class ApproveStat
    {
        public BO.ResultEnum Flag { get; set; }
        public string Message { get; set; }
        public string hodiny9 { get; set; } //rozpracované hodiny
        public string hodiny0 { get; set; }
        public string hodiny4 { get; set; }
        public string hodiny6 { get; set; }
        public string hodiny7 { get; set; }
        public string hodiny3 { get; set; }
        public string hodiny2 { get; set; }
        public string bezdph4czk { get; set; }
        public string bezdph4eur { get; set; }

        public string hodiny_minus { get; set; }
        public string penize_czk_minus { get; set; }
        public string penize_eur_minus { get; set; }
        public string penize_czk0 { get; set; }
        public string penize_eur0 { get; set; }

        public string honorar_hodiny9 { get; set; }

    }
}
