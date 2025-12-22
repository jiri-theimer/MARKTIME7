namespace UI.Models.Record
{
    public class j06Record:BaseRecordViewModel
    {
        public BO.j06UserHistory Rec { get; set; }
        
        public string ComboJ07 { get; set; }
        public string ComboC21 { get; set; }
        public string ComboJ02 { get; set; }
        public DateTime? d1 { get; set; }
        public DateTime? d2 { get; set; }

        public string Volba { get; set; }
    }
}
