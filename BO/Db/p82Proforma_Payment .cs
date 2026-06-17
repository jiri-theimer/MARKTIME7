using System;

namespace BO
{
    public class p82Proforma_Payment:BaseBO
    {
        public int p90ID { get; set; }
        public DateTime? p82Date { get; set; }
        public double p82Amount { get; set; }
        public double p82Amount_WithoutVat { get; set; }
        public double p82Amount_Vat { get; set; }
        public string p82Code { get; set; }        
        public string p82Text { get; set; }
        public DateTime? p82DateIssue { get; set; }

        public string DateWithAmount
        {
            get
            {
                return BO.Code.Bas.ObjectDate2String(p82Date) + ": " + BO.Code.Bas.Number2String(this.p82Amount);
            }
        }
    }
}
