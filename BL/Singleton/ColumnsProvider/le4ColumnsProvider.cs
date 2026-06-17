using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BL
{
    public class le4ColumnsProvider: ColumnsProviderBase
    {
        public le4ColumnsProvider()
        {
            this.EntityName = "le4";

            this.CurrentFieldGroup = "Root";
            //oc = AF("p41Name", "L4"); oc.NotShowRelInHeader = true; oc.DefaultColumnFlag = gdc1;
            this.AppendProjectColumns("le4");

            AppendTimestamp();
        }
    }
}
