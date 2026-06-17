

namespace BO
{
    public class p54OvertimeLevel:BaseBO
    {
        public string p54Name { get; set; }
        public int p54Ordinary { get; set; }
        public double p54BillingRate { get; set; }  //násobek fakturační sazby
        public double p54InternalRate { get; set; }  //násobek nákladové sazby
        public int x01ID { get; set; }
    }
}
