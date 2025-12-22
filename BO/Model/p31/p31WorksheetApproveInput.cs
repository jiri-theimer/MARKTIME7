

namespace BO
{
    public class p31WorksheetApproveInput
    {
        public string Guid { get; set; }
        public int p31ID { get; set; }
        public BO.p33IdENUM p33ID { get; set; }
        public BO.p71IdENUM p71id { get; set; }
        public BO.p72IdENUM p72id { get; set; }
        public double Value_Approved_Billing { get; set; }
        public double Value_Approved_Internal { get; set; }
        public double Rate_Billing_Approved { get; set; }
        public double Rate_Internal_Approved { get; set; }
        public string p31Text { get; set; }
        public string p31TextInternal { get; set; }
        public DateTime p31Date { get; set; }
        public int p31ApprovingLevel { get; set; }
        public double p31Value_FixPrice { get; set; }
        public double VatRate_Approved { get; set; }
        public int p32ID { get; set; }
        
        public int p32ManualFeeFlag { get; set; }
        public double ManualFee_Approved { get; set; }

    }
}
