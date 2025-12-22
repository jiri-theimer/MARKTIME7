namespace UI.Models.Record
{
    public class p56Record:BaseRecordViewModel
    {
        public int j02id_create_taskfor { get; set; }
        public string b05RecordEntity { get; set; }   //pro uložení vazby s b05Workflow_History
        public int b05RecordPid { get; set; }     //pro uložení vazby s b05Workflow_History
        public BO.p56Task Rec { get; set; }
        
        public string ComboP57 { get; set; }
        
        public string ComboOwner { get; set; }
        public string ComboP55 { get; set; }
        public string ComboP15 { get; set; }
        public BO.p57TaskType RecP57 { get; set; }

        public FreeFieldsViewModel ff1 { get; set; }

        public RoleAssignViewModel roles { get; set; }
        public ReminderViewModel reminder { get; set; }
        public Notepad.EditorViewModel Notepad { get; set; }

        public ProjectComboViewModel ProjectCombo { get; set; }
        public bool CanEditRecordCode { get; set; }
        public bool IsShowP57Combo { get; set; }
        public int o43id_source { get; set; }
        public string UploadGuid { get; set; }
    }
}
