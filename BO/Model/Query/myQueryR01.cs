using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BO
{
    public class myQueryR01:baseQuery
    {
        public int p28id { get; set; }
        public int p41id { get; set; }
        public List<int> p41ids { get; set; }
        public int j02id { get; set; }
        public int r02id { get; set; }
        public List<int> j02ids { get; set; }

        public myQueryR01()
        {
            this.Prefix = "r01";
        }

        public override List<QRow> GetRows()
        {

            if (this.p41id > 0)
            {
                AQ("a.p41ID = @p41id", "p41id", this.p41id);
            }
            if (this.p41ids !=null)
            {
                AQ($"a.p41ID IN ({string.Join(",",this.p41ids)})",null,null);
            }
            if (this.j02id > 0)
            {
                AQ("a.j02ID = @j02id", "j02id", this.j02id);
            }
            if (this.j02ids !=null && this.j02ids.Count() > 0)
            {
                AQ($"a.j02ID IN ({string.Join(",", this.j02ids)})", null, null);
            }
            if (this.r02id > 0)
            {
                AQ("a.r02ID = @r02id", "r02id", this.r02id);
            }
            if (this.p28id > 0)
            {
                AQ("a.p41ID IN (select p41ID FROM p41Project where p28ID_Client=@p28id)", "p28id", this.p28id);
            }


            if (this.IsActivePeriodQuery())
            {
                switch (this.period_field)
                {
                    case "r01DateInsert":
                        AQ("a.r01DateInsert BETWEEN @d1 AND @d2", "d1", this.global_d1_query, "AND", null, null, "d2", this.global_d2_235959);
                        break;
                    case "r05Date":
                        AQ("r05.r05Date BETWEEN @d1 AND @d2", "d1", this.global_d1_query, "AND", null, null, "d2", this.global_d2);
                        break;
                    default:
                        AQ("a.r01ID IN (SELECT r01ID FROM r05CapacityUnit WHERE r05Date BETWEEN @d1 AND @d2)", "d1", this.global_d1_query, "AND", null, null, "d2", this.global_d2);                        
                        break;
                }
            }


            return this.InhaleRows();

        }
    }
}
