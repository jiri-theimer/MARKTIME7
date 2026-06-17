using System;

namespace BO
{
    public class p94Invoice_Payment:BaseBO
    {
        public int p91ID { get; set; }
        public DateTime p94Date { get; set; }
        public double p94Amount { get; set; }
        public string p94Code { get; set; }
        public string p94Description { get; set; }
    }
}
