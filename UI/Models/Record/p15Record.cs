namespace UI.Models.Record
{
    public class p15Record:BaseRecordViewModel
    {
        public BO.p15Location Rec { get; set; }
        public string ComboOwner { get; set; }

        public Notepad.EditorViewModel Notepad { get; set; }
        public IEnumerable<BO.o15AutoComplete> lisAutocomplete { get; set; }

        public string Longitude { get; set; }
        public string Latitude { get; set; }
    }
}
