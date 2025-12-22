namespace UI.Models.Record
{
    public class x07Record: BaseRecordViewModel
    {
        public BO.x07Integration Rec { get; set; }

        public bool IsShowToken { get; set; }
        public bool IsShowLoginPassword { get; set; }
    }
}
