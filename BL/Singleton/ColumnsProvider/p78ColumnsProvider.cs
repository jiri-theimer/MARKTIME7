
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


            this.CurrentFieldGroup = "Dlužník";
            oc = AF("p78Client", "Název dlužníka"); oc.DefaultColumnFlag = gdc1; oc.SqlExplicitGroupBy = "a.p78Client";
            oc = AF("p78Client_RegID", "IČO"); oc.FixedWidth = 100;
            oc = AF("p78Client_VatID", "DIČ"); oc.FixedWidth = 100;
            oc = AF("p78Client_ICDPH_SK", "IČ DPH (SK)"); oc.FixedWidth = 100;
            AF("p78ClientAddress1_Street", "Ulice");
            AF("p78ClientAddress1_City", "Město");
            oc = AF("p78ClientAddress1_ZIP", "PSČ"); oc.FixedWidth = 70;
            AF("p78ClientAddress1_Country", "Stát");
            AF("p78ClientAddress1_Before", "Doplnění adresy");


            this.CurrentFieldGroup = "Dluh";
            AF("p78Amount_Debt", "Dluh", null, "num", true);
            AF("DluhKratKurz", "Dluh x Kurz", "case When a.j27ID=a.j27ID_Domestic Then p91Amount_Debt Else p91Amount_Debt*p91ExchangeRate End", "num", true);

            AppendTimestamp();
        }
    }
}
