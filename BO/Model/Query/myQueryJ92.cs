using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BO
{
    public class myQueryJ92: baseQuery
    {
        public int j02id { get; set; }
        
        public myQueryJ92()
        {
            this.Prefix = "j92";
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
