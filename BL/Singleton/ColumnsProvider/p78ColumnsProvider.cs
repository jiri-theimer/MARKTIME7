
namespace BL.Singleton.ColumnsProvider
{
    public class p78ColumnsProvider: ColumnsProviderBase
    {
        public p78ColumnsProvider()
        {
            this.EntityName = "p78UpominkaSdruzena";
            this.CurrentFieldGroup = "Root";
            oc = AF("p78Code", "Číslo upomínky", null, "string"); oc.FixedWidth = 110;
            
            oc = AF("p78Name", "Název"); oc.NotShowRelInHeader = true; oc.DefaultColumnFlag = gdc1;

            AF("p78Date", "Datum upomínky", null, "date").DefaultColumnFlag = gdc1;
            
            AF("p78TextA", "Text upomínky").DefaultColumnFlag = gdc2;
            AF("p78TextB", "Technický text");

            AppendTimestamp();
        }
    }
}
