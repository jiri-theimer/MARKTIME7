namespace UI.Models
{
    public class HelpdeskViewModel:BaseViewModel
    {
        public int rec_pid { get; set; }
        public int p41id { get; set; }
        public int p28id { get; set; }
        public string p56Name { get; set; }
        public string p56Notepad { get; set; }
        public int p57ID { get; set; }
        public int x04ID { get; set; }
        public string ComboP57 { get; set; }
        public string SelectedComboProject { get; set; }
        public BO.p57TaskType RecP57 { get; set; }

        public FreeFieldsViewModel ff1 { get; set; }
        public Notepad.EditorViewModel Notepad { get; set; }


        public bool IsShowP57Combo { get; set; }
    }
}
