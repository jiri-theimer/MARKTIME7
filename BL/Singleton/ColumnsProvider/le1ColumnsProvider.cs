using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BL
{
    public class le1ColumnsProvider: ColumnsProviderBase
    {
        public le1ColumnsProvider()
        {
            this.EntityName = "le1";

            this.CurrentFieldGroup = "Root";
            //oc = AF("p41Name", "L1"); oc.NotShowRelInHeader = true; oc.DefaultColumnFlag = gdc1;

            this.AppendProjectColumns("le1");

            AppendTimestamp();
        }
    }
}
