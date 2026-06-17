using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BO
{
    public class myQueryP15:baseQuery
    {
       
        public myQueryP15()
        {
            this.Prefix = "p15";
        }
        public override List<QRow> GetRows()
        {
            

            return this.InhaleRows();

        }
    }
}
