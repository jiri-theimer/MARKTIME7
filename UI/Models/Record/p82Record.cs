

namespace UI.Models.Record
{
    public class p82Record: BaseRecordViewModel
    {
        public BO.p82Proforma_Payment Rec { get; set; }
        public int p90ID { get; set; }
        public BO.p90Proforma RecP90 { get; set; }
        public bool CanEditRecordCode { get; set; }
    }
}
