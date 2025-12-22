using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BO
{
    public class x21DatePeriod:BaseBO
    {        
        public int x01ID { get; set; }
        public int j02ID { get; set; }
        public string x21Name { get; set; }
        
        public DateTime x21ValidFrom { get; set; }
        public DateTime x21ValidUntil { get; set; }



        public bool IsTempDeleted { get; set; }
        public string TempGuid { get; set; }
        public string CssTempDisplay
        {
            get
            {
                if (this.IsTempDeleted)
                {
                    return "display:none;";
                }
                else
                {
                    return "display:table-row;";
                }
            }
        }
    }
}
