using UI.Models.p31oper;

namespace UI.Models.p91oper
{
    public class p91CloneViewModel:BaseViewModel
    {

        public int pid { get; set; }
        public BO.p91Invoice recOrigP91 { get; set; }
        public IEnumerable<BO.p31Worksheet> lisOrigP31 { get; set; }

        public List<p91Clonep31DestRecord> lisDestP31 { get; set; }

        public BO.p91Invoice recDestP91 { get; set; }
    }

    public class p91Clonep31DestRecord
    {
        public DateTime p31Date { get; set; }
        public int p31ID { get; set; }
        
        public string p31Text { get; set; }

        public BO.p31Worksheet recOrig { get; set; }
    }
}
