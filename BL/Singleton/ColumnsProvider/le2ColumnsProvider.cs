using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BL
{
    public class le2ColumnsProvider:ColumnsProviderBase
    {
        public le2ColumnsProvider()
        {
            this.EntityName = "le2";

            this.CurrentFieldGroup = "Root";
            //oc = AF("p41Name", "L4"); oc.NotShowRelInHeader = true; oc.DefaultColumnFlag = gdc1;
            this.AppendProjectColumns("le2");

            AppendTimestamp();
        }
    }
}
