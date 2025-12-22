namespace UI.Models.Imap
{
    public class InboxRecordViewModel:BaseRecordViewModel
    {
        public BO.o43Inbox Rec { get; set; }
        public BO.o43Inbox RecPreview { get; set; }
        
        public string ComboP28 { get; set; }
        public string ComboJ02 { get; set; }
        public string ComboP91 { get; set; }
        public string ComboO23 { get; set; }
        public string ComboP56 { get; set; }
        public string CurrentBody { get; set; } = "html";
        public string UpdatedSubject { get; set; }


        public ProjectComboViewModel ProjectCombo { get; set; }
    }
}
