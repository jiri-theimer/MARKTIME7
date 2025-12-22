namespace UI.Models.Record
{
    public class p75Record:BaseRecordViewModel
    {
        
        public BO.p75InvoiceRecurrence Rec { get; set; }
        
        
        public string ComboP28 { get; set; }

        public string ComboOwner { get; set; }
        public BO.p41Project RecP41 { get; set; }
        public BO.p28Contact RecP28 { get; set; }


        public ProjectComboViewModel ProjectCombo { get; set; }


    }
}
