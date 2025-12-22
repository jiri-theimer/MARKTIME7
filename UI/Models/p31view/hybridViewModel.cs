namespace UI.Models.p31view
{
    public class hybridViewModel: BaseViewModel
    {
        public int j02ID { get; set; }
        public BO.j02User RecJ02 { get; set; }
        public bool ShowHHMM { get; set; }
        public bool ShowP31Recs { get; set; }
        public bool ShowP31RecsNoTime { get; set; }
        public bool ShowP56Recs { get; set; }
        public bool ShowO22Recs { get; set; }


        public DateTime d0 { get; set; }
        public DateTime d1 { get; set; }
        public DateTime d2 { get; set; }
        public int m0 { get; set; }
        public int y0 { get; set; }

        public string InitFrameSrc { get; set; }
        public string MonthFrameSrc { get; set; }

        public string StatTotalsByPrefix { get; set; }

    }
}
