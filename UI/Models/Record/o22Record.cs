namespace UI.Models.Record
{
    public class o22Record:BaseRecordViewModel
    {
        public int j02id_create_taskfor { get; set; }
        public string b05RecordEntity { get; set; }   //pro uložení vazby s b05Workflow_History
        public int b05RecordPid { get; set; }     //pro uložení vazby s b05Workflow_History
        public BO.o22Milestone Rec { get; set; }
        public bool IsShowMore { get; set; }
        public string ComboO21 { get; set; }
        
        public string ComboOwner { get; set; }
        
        public BO.o21MilestoneType RecO21 { get; set; }

        public RoleAssignViewModel roles { get; set; }
        public Notepad.EditorViewModel Notepad { get; set; }

        public ReminderViewModel reminder { get; set; }

        public int o43id_source { get; set; }
        public string UploadGuid { get; set; }
        public ProjectComboViewModel ProjectCombo { get; set; }
    }
}
