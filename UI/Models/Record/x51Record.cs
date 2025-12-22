namespace UI.Models.Record
{
    public class x51Record: BaseRecordViewModel
    {
        public BO.x51HelpCore Rec { get; set; }
        public Notepad.EditorViewModel Notepad { get; set; }
        public string ComboParent { get; set; }
    }
}
