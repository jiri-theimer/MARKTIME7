using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BO
{
    public class myQueryJ40: baseQuery
    {
        public int j02id { get; set; }
        public bool? is4sendmail { get; set; }

        public bool? isimap { get; set; }

        public myQueryJ40()
        {
            this.Prefix = "j40";
        }

        public override List<QRow> GetRows()
        {

            if (this.j02id > 0)
            {
                AQ("a.j02ID=@j02id", "j02id", this.j02id);
            }

            if (this.is4sendmail==true)
            {
                AQ("(a.j40UsageFlag=2 OR (a.j02ID=@j02id AND a.j40UsageFlag=1)) AND GETDATE() between a.j40ValidFrom AND a.j40ValidUntil", "j02id",this.CurrentUser.pid);
            }

            if (this.isimap != null)
            {
                if (this.isimap == true)
                {
                    AQ("(a.j40UsageFlag=4 OR (a.j02ID=@j02id AND a.j40UsageFlag=3)) AND GETDATE() between a.j40ValidFrom AND a.j40ValidUntil", "j02id", this.CurrentUser.pid);
                }
                else
                {
                    AQ("a.j40UsageFlag IN (1,2)", null, null);
                }
            }

            return this.InhaleRows();

        }
    }
}
