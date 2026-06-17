using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BO
{
    public class myQueryP92:baseQuery
    {
        public int invoicetype { get; set; }
        public myQueryP92()
        {
            this.Prefix = "p92";
        }

        public override List<QRow> GetRows()
        {
            if (this.invoicetype > 0)
            {
                AQ("a.p92TypeFlag=@invoicetype", "invoicetype", this.invoicetype);
            }


            return this.InhaleRows();

        }
    }
}
