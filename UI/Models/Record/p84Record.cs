namespace UI.Models.Record
{
    public class p84Record: BaseRecordViewModel
    {
        public BO.p84Upominka Rec { get; set; }
        public BO.p91Invoice RecP91 { get; set; }
        public string ComboOwner { get; set; }

        public ReminderViewModel reminder { get; set; }
    }
}
