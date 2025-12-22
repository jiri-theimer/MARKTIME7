using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BO
{
    public class myQueryJ95:baseQuery
    {
        public int p15id { get; set; }

        public myQueryJ95()
        {
            this.Prefix = "j95";
        }
        public override List<QRow> GetRows()
        {
            if (this.p15id > 0)
            {
                AQ("a.j95RecordEntity='p15' AND a.j95RecordPid=@p15id", "p15id", this.p15id);
            }

            return this.InhaleRows();

        }
    }
}
