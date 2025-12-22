namespace UI.Models.Widgets
{
    public class MiniAbsenceViewModel:BaseViewModel
    {
        public DateTime d0 { get; set; }
        
        public int j02ID { get; set; }
        public string Prichod { get; set; }
        public string Odchod { get; set; }
        public IEnumerable<BO.p31Worksheet> lisP31 { get; set; }
        public IEnumerable<BO.p11Attendance> lisP11 { get; set; }
        public IEnumerable<BO.p32Activity> lisP32FUT { get; set; }
    }
}
