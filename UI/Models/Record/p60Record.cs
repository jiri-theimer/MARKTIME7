namespace UI.Models.Record
{
    public class p60Record: BaseRecordViewModel
    {
        public BO.p60TaskTemplate Rec { get; set; }

        public string ComboP57 { get; set; }
        public string ComboOwner { get; set; }

        public string ComboP55 { get; set; }
        public string ComboP15 { get; set; }
        public BO.p41Project RecP41 { get; set; }
        public BO.p57TaskType RecP57 { get; set; }
        public Notepad.EditorViewModel Notepad { get; set; }
        public RoleAssignViewModel roles { get; set; }

        public ProjectComboViewModel ProjectCombo { get; set; }
    }
}
