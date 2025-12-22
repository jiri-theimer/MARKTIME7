using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BL
{
    public class le3ColumnsProvider: ColumnsProviderBase
    {
        public le3ColumnsProvider()
        {
            this.EntityName = "le3";

            this.CurrentFieldGroup = "Root";            
            this.AppendProjectColumns("le3");

            AppendTimestamp();
        }
    }
}
