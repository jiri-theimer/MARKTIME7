namespace UI.Models.p31oper
{
    public class p68ViewModel:BaseViewModel
    {
        public List<StopWatchRow> lisRows { get; set; }
        public BO.TimeApi.Record TimeApiRecord { get; set; }
        public int Jump2Pid { get; set; }

        public string StartStopWatchUtc { get; set; }

        public bool IsUseTimeApi { get; set; }
        
    }

    public class StopWatchRow:BO.p68StopWatch
    {        
        public string Project { get; set; }
        public string Activity { get; set; }
        public string duration_hhmm { get; set;}
        public int p61ID { get; set; }

        


    }
}
