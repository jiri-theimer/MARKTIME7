using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BO
{
    public class myQueryP39: baseQuery
    {
        public int p40id { get; set; }
       

        public myQueryP39()
        {
            this.Prefix = "p39";
        }

        public override List<QRow> GetRows()
        {
            if (this.IsActivePeriodQuery())
            {
                switch (this.period_field)
                {
                    case "p39DateCreate":
                        AQ("a.p39ID IN (select p39ID FROM p39WorkSheet_Recurrence_Plan WHERE p39DateCreate BETWEEN @d1 AND @d2)", "d1", this.global_d1_query, "AND", null, null, "d2", this.global_d2_235959);
                        break;
                    default:
                        AQ("a.p39ID IN (select p39ID FROM p39WorkSheet_Recurrence_Plan WHERE p39Date BETWEEN @d1 AND @d2)", "d1", this.global_d1_query, "AND", null, null, "d2", this.global_d2_query);
                        break;
                }
            }


            if (this.p40id > 0)
            {
                AQ("a.p40ID=@p40id", "p40id", this.p40id);
            }
          
            return this.InhaleRows();

        }
    }
}
