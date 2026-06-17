namespace UI.Models.Record
{
    public class p58Record:BaseRecordViewModel
    {
        public string b05RecordEntity { get; set; }   //pro uložení vazby s b05Workflow_History
        public int b05RecordPid { get; set; }     //pro uložení vazby s b05Workflow_History
        public BO.p58TaskRecurrence Rec { get; set; }
        public bool IsShowMore { get; set; }
        public string ComboP57 { get; set; }
        
        public string ComboOwner { get; set; }
        public BO.p57TaskType RecP57 { get; set; }
        public BO.p41Project RecP41 { get; set; }
       
        public RoleAssignViewModel roles { get; set; }
        public Notepad.EditorViewModel Notepad { get; set; }


        public ProjectComboViewModel ProjectCombo { get; set; }

        public ReminderViewModel reminder { get; set; }

    }
}
