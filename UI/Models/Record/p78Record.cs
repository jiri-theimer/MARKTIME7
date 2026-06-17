namespace UI.Models.Record
{
    public class p78Record: BaseRecordViewModel
    {
        public BO.p78UpominkaSdruzena Rec { get; set; }
        
        public string ComboOwner { get; set; }
        public string ComboP28Name { get; set; }
        public string ComboJ27Code { get; set; }
        public ReminderViewModel reminder { get; set; }


        public List<int> SelectedP84IDs { get; set; }
        public IEnumerable<BO.p84Upominka> lisAllP84 { get; set; }
    }
}
