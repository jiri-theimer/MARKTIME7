namespace UI.Models
{
    public class UctenkaViewModel: BaseRecordViewModel
    {
        public BO.o23Doc Rec { get; set; }
        public string UploadGuid { get; set; }
        
        public int o18ID { get; set; }
        public BO.o18DocType RecO18 { get; set; }
        public Notepad.EditorViewModel Notepad { get; set; }
        public string SelectedComboOwner { get; set; }
        public string SelectedComboJ27 { get; set; }
        public ProjectComboViewModel ProjectCombo { get; set; }

        public IEnumerable<BO.p34ActivityGroup> lisP34 { get; set; }
    }
}
