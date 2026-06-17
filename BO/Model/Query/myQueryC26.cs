using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BO
{
    public class myQueryC26:baseQuery
    {       
        public myQueryC26()
        {
            this.Prefix = "c26";
        }
        
        

        public override List<QRow> GetRows()
        {
            if (this.global_d1 != null && this.global_d2 !=null)
            {
                AQ("a.c26Date BETWEEN @d1 AND @d2", "d1", this.global_d1, "AND", null, null, "d2",this.global_d2);

            }

            return this.InhaleRows();

        }
    }
}
