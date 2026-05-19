namespace UI.Models.Record
{
    public class p78Record: BaseRecordViewModel
    {
        public BO.p78UpominkaSdruzena Rec { get; set; }
        
        public string ComboOwner { get; set; }

        public ReminderViewModel reminder { get; set; }
    }
}
