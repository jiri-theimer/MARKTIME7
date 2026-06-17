using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BL
{
    public class le5ColumnsProvider:ColumnsProviderBase
    {
        public le5ColumnsProvider()
        {
            this.EntityName = "le5";

            this.CurrentFieldGroup = "Root";
            
            
            this.AppendProjectColumns("le5");

            AppendTimestamp();
        }
    }
}
