namespace UI.Models.Notepad
{
    public class EditorViewModel
    {
        public string HtmlContent { get; set; }

        public BO.x04NotepadConfig ExternalConfig { get; set; }
        public int SelectedX04ID { get; set; }
        public string Prefix { get; set; }
        public string TempGuid { get; set; }
        public string PlaceHolder { get; set; }
    }
}
