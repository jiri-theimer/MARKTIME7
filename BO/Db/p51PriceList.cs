

namespace BO
{
    public enum p51TypeFlagENUM
    {
        BillingRates = 1,
        CostRates = 2,
        OverheadRates = 3,
        RootBillingRates = 5
    }

    public class p51PriceList:BaseBO
    {
        public int x01ID { get; set; }
        public string p51Name { get; set; }
        public string p51Code { get; set; }
        public int j27ID { get; set; }
        public int p51ID_Master { get; set; }
        public double p51DefaultRateT { get; set; }
        public double p51DefaultRateU { get; set; }
        public p51TypeFlagENUM p51TypeFlag { get; set; } = p51TypeFlagENUM.BillingRates;
        public bool p51IsMasterPriceList { get; set; }
        public int p51Ordinary { get; set; }
        public bool p51IsCustomTailor { get; set; }
        public bool p51IsFPR { get; set; }
        public string p51InlineInfo { get; set; }

        public string j27Code { get; }
        
        public string p51Name_Master { get; }

        public string NameWithCurr
        {
            get
            {
                return this.p51Name + " (" + this.j27Code + ")";
            }
        }

        public string TypeAlias
        {
            get
            {
                switch (this.p51TypeFlag)
                {
                    case p51TypeFlagENUM.BillingRates:
                        {
                            return "Fakturační hodinové sazby";
                        }

                    case p51TypeFlagENUM.CostRates:
                        {
                            return "Nákladové hodinové sazby";
                        }                   

                    case p51TypeFlagENUM.OverheadRates:
                        {
                            return "Režijní hodinové sazby";
                        }

                    case p51TypeFlagENUM.RootBillingRates:
                        {
                            return "ROOT fakturační sazby";
                        }

                    default:
                        {
                            return "???";
                        }
                }
            }
        }
    }
}
