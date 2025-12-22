namespace UI.Models.p31view
{
    public class minicalendarViewModel
    {
        public string mode { get; set; }    //onlycalendar/hybrid
        public int j02ID { get; set; }
        public bool ShowWeekend { get; set; } = true;
        public bool ShowHHMM { get; set; }

        public DateTime d0 { get; set; } = DateTime.Today;
        
   
    }
}
