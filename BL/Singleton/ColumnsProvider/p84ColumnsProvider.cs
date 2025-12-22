

namespace BL.Singleton.ColumnsProvider
{
    public class p84ColumnsProvider: ColumnsProviderBase
    {
        public p84ColumnsProvider()
        {
            this.EntityName = "p84Upominka";
            this.CurrentFieldGroup = "Root";
            oc = AF("p84Code", "Číslo upomínky", null, "string"); oc.FixedWidth = 110;
            oc = AF("CisloFaktury", "Číslo faktury", "p91x.p91Code", "string"); oc.DefaultColumnFlag = gdc1; oc.NotShowRelInHeader = true; oc.FixedWidth = 110;
            oc = AF("p84Name", "Název"); oc.NotShowRelInHeader = true; oc.DefaultColumnFlag = gdc1;

            oc = AF("TypUpominky", "Typ upomínky", "p83x.p83Name");


            AF("p84Date", "Datum upomínky", null, "date").DefaultColumnFlag = gdc1;
            AF("p84Index", "Stupeň", "a.p84Index", "num0");
            AF("p84TextA", "Text upomínky").DefaultColumnFlag = gdc2;
            AF("p84TextB", "Technický text");

            AppendTimestamp();
        }
    }
}
