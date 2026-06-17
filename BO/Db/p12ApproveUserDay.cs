

namespace BO
{
    public enum p12StatusFlagENUM
    {
        None=0,
        ApprovedAndLocked=1,
        ApprovedNoLocked=2,
        NotApproved=3

    }
    public class p12ApproveUserDay:BaseBO
    {
        public int j02ID { get; set; }
        public int j02ID_ApprovedBy { get; set; }
        public DateTime p12Date { get; set; }
        public string p12Memo { get; set; }
        public p12StatusFlagENUM p12StatusFlag { get; set; }

        public string StatusAlias
        {
            get
            {
                switch (this.p12StatusFlag)
                {
                    case p12StatusFlagENUM.ApprovedAndLocked:
                        return "🔒";
                    case p12StatusFlagENUM.ApprovedNoLocked:
                        return "✓";
                    case p12StatusFlagENUM.NotApproved:
                        return "!!";
                    default:
                        return "?";
                }
            }
        }
        
        public string StatusCssClass
        {
            get
            {
                switch (this.p12StatusFlag)
                {
                    case p12StatusFlagENUM.ApprovedAndLocked:
                        return "dot dot1";
                    case p12StatusFlagENUM.ApprovedNoLocked:
                        return "dot dot2";
                    case p12StatusFlagENUM.NotApproved:
                        return "dot dot3";
                    case p12StatusFlagENUM.None:
                        return "dot dot0";
                    default:
                        return null;
                }
            }
        }
        public string Person { get; }
    }
}
