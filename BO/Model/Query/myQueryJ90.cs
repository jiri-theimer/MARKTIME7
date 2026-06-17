using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BO
{
    public class myQueryJ90:baseQuery
    {
        public int j02id { get; set; }

        public myQueryJ90()
        {
            this.Prefix = "j90";
        }
        public override List<QRow> GetRows()
        {
            if (this.j02id > 0)
            {
                AQ("a.j02ID=@j02id", "j02id", this.j02id);
            }

            return this.InhaleRows();

        }
    }
}
