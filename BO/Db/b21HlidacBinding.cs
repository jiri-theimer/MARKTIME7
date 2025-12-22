

namespace BO
{
    
  
    public class b21HlidacBinding:BaseBO
    {
        public int b20ID { get; set; }
        public int j61ID { get; set; }        
        public string b21RecordEntity { get; set; }
        public int b21RecordPid { get; set; }
        public double b21Par1 { get; set; }
        public double b21Par2 { get; set; }
        public string b21NotifyMessage { get; set; }        
        public string b21NotifyReceivers { get; set; }

        public bool IsSetAsDeleted { get; set; }

        public string b20Par2Name { get; }
        public string b20Par1Name { get; }
        public string b20Name { get; }
        public string b20NotifyReceivers { get; }
        public BO.b20TypeFlagEnum b20TypeFlag { get; }
    }
}
