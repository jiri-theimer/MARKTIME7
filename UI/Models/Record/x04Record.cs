namespace UI.Models.Record
{
    public class x04Record: BaseRecordViewModel
    {
        public BO.x04NotepadConfig Rec { get; set; }

        
        public Notepad.EditorViewModel Notepad { get; set; }
    }
}
