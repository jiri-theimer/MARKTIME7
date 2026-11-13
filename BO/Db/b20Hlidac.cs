

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

        /// <summary>
        /// Jen hlídač za celou databázi (b20Entity = "x01"): typ záznamů, které vrací b20TestSql (p41, p28, p56, p91, o23, p90, j02).
        /// Prázdné = SQL vrací jednu hodnotu za databázi (událost při změně hodnoty).
        /// </summary>
        public string b20ListEntity { get; set; }

        /// <summary>Hlídač za celou databázi: jedno pravidlo, žádné přiřazování k záznamům.</summary>
        public bool IsSystem => this.b20Entity == "x01";
        /// <summary>Hlídač za celou databázi, jehož SQL vrací seznam záznamů – hlásí se jen nově zachycené.</summary>
        public bool IsSystemList => IsSystem && this.b20TypeFlag == b20TypeFlagEnum.TestBySql && !string.IsNullOrWhiteSpace(this.b20ListEntity);

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
