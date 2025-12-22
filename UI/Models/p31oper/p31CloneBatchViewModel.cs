namespace UI.Models.p31oper
{
    public class p31CloneBatchViewModel:BaseViewModel
    {
        public string pids { get; set; }
        public IEnumerable<BO.p31Worksheet> lisSourceP31 { get; set; }
        public List<DestRecord> lisDest { get; set; }
        public BO.p41Project RecP41 { get; set; }
        public ProjectComboViewModel ProjectCombo { get; set; }
    }

    public class DestRecord
    {
        public DateTime p31Date { get; set; }
        public int p31ID { get; set; }
        public string DestHours { get; set; }
        public string DestText { get; set; }

        public string Project { get; set; }
        
        public int p41ID { get; set; }
        public string Person { get; set; }
        public string p32Name { get; set; }
        public int p31ID_Saved { get; set; }
    }

}
