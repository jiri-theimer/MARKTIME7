

namespace BO
{
    public enum b20TypeFlagEnum
    {
        TestBySql=1,
        ZmenaAdresy=2,
        Insolvence=3,
        WipFaHodinyZakazPrekrocit=4,
        WipFaHonorarZakazPrekrocit=5,
        WipFaHodinyZdarma=6,
        WipFaHonorarZdarma=7
       
    }
    
    public class b20Hlidac:BaseBO
    {
        public int x01ID { get; set; }
        public int j61ID { get; set; }
        public b20TypeFlagEnum b20TypeFlag { get; set; }
        
        public string b20Name { get; set; }
        public string b20Entity { get; set; }
        public string b20EntityAlias { get; set; }
        public string b20Par1Name { get; set; }
        public string b20Par2Name { get; set; }
        public int b20Ordinary { get; set; }
        public string b20TestSql { get; set; }
        public string b20RunSql { get; set; }
        
        public int b20NextRunAfterMinutes { get; set; }
        public string b20TestTimeFrom { get; set; }
        public string b20TestTimeUntil { get; set; }
        public string b20NotifyReceivers { get; set; }
        public string b20NotifyMessage { get; set; }
        public bool b20IsNotifyRecordOwner { get; set; }
        public int j11ID_Notify { get; set; }
        public int x67ID_Notify1 { get; set; }
        public int x67ID_Notify2 { get; set; }

        public bool IsCyklus
        {
            get
            {
                switch (this.b20TypeFlag)
                {
                    case b20TypeFlagEnum.WipFaHodinyZdarma:
                    case b20TypeFlagEnum.WipFaHonorarZdarma:
                    case b20TypeFlagEnum.WipFaHodinyZakazPrekrocit:
                    case b20TypeFlagEnum.WipFaHonorarZakazPrekrocit:
                        return false;
                    default:
                        return true;
                }
            }
            
}
    }
}
