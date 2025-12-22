namespace UI.Models.Record
{
    public class b05Record: BaseViewModel
    {
        public int b05ID { get; set; }
        public BO.b05Workflow_History RecB05 { get; set; }
        public Notepad.EditorViewModel Notepad { get; set; }
        public IEnumerable<BO.o27Attachment> lisO27_CanDelete { get; set; }
        public string RecPrefix { get; set; }
        public string RecAlias { get; set; }
        public int RecPid { get; set; }
        public ProjectComboViewModel ProjectCombo { get; set; }
        public ReminderViewModel reminder { get; set; }
        public DateTime? b05Date { get; set; }
        public string b05Name { get; set; }
        public bool IsPortalAccess { get; set; }
        public bool IsTab1 { get; set; }
        public bool IsBillingMemo { get; set; }
        public int o43ID_Source { get; set; }
        public string UploadGuid { get; set; }


        
    }
}
