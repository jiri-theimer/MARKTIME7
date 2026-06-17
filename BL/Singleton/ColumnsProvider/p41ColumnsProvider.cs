

namespace BL
{
    public class p41ColumnsProvider:ColumnsProviderBase
    {
        public p41ColumnsProvider()
        {
            this.EntityName = "p41Project";

            this.CurrentFieldGroup = "Root";
            oc = AF("p41Name","L5"); oc.NotShowRelInHeader = true; oc.DefaultColumnFlag = gdc1;oc.SqlExplicitGroupBy = "a.p41ID";
            this.AppendProjectColumns("p41");

            AppendTimestamp();
        }
    }
}
