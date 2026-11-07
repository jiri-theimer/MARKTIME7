namespace UI.Models.p31oper
{
    public class p68ViewModel:BaseViewModel
    {
        public List<StopWatchRow> lisRows { get; set; }
        /// <summary>Karta, ke které se má po překreslení posunout (nový nebo změněný časovač).</summary>
        public int Jump2Pid { get; set; }
        /// <summary>Součet naměřeného času všech časovačů včetně běžícího (sekundy).</summary>
        public int TotalSeconds { get; set; }
    }

    public class StopWatchRow:BO.p68StopWatch
    {
        public string Project { get; set; }
        public string Activity { get; set; }
        public string duration_hhmm { get; set;}
        public int p61ID { get; set; }
        /// <summary>Naměřený čas včetně právě běžícího úseku (sekundy, v okamžiku načtení).</summary>
        public int ElapsedSeconds { get; set; }
    }
}
