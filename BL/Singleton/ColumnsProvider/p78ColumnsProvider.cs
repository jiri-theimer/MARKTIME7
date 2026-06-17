
namespace BL.Singleton.ColumnsProvider
{
    public class p78ColumnsProvider: ColumnsProviderBase
    {
        public p78ColumnsProvider()
        {
            this.EntityName = "p78UpominkaSdruzena";
            this.CurrentFieldGroup = "Root";
            oc = AF("p78Code", "Kód upomínky", null, "string"); oc.FixedWidth = 110;
            oc = AF("p78VariableSymbol", "Variabilní symbol", null, "string"); oc.FixedWidth = 110;

            oc = AF("p78Name", "Název"); oc.NotShowRelInHeader = true; oc.DefaultColumnFlag = gdc1;

            AF("p78Date", "Datum upomínky", null, "date").DefaultColumnFlag = gdc1;
            oc = AF("PocetUpominek", "Počet upomínek", "p78soucty.Pocet", "num0"); oc.RelSqlInCol = "LEFT OUTER JOIN view_p78_soucty p78soucty On a.p78ID = p78soucty.p78ID";


            this.CurrentFieldGroup = "Dluh";
            oc = AF("p78Amount_Debt", "Dluh", null, "num", true); oc.DefaultColumnFlag = gdc1;
            AF("DluhKratKurz", "Dluh x Kurz", "a.p78Amount_Debt_KratKurz", "num", true);
            oc = AF("j27Code", "Měna", "p78_j27x.j27Code"); oc.RelSqlInCol = "LEFT OUTER JOIN j27Currency p78_j27x ON a.j27ID=p78_j27x.j27ID"; oc.DefaultColumnFlag = gdc1; oc.FixedWidth = 60; oc.SqlExplicitGroupBy = "a.j27ID";
            
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

            this.CurrentFieldGroup = "Elektronicky odesláno";
            oc = AF("p78VomKdyOdeslano", "Čas odeslání", "p78vom.Kdy_Odeslano", "datetime"); oc.RelSqlInCol = "LEFT OUTER JOIN view_p78_sendbyemail p78vom On a.p78ID = p78vom.p78ID";
            oc = AF("p78VomStav", "Stav odeslání", "p78vom.AktualniStav"); oc.RelSqlInCol = "LEFT OUTER JOIN view_p78_sendbyemail p78vom On a.p78ID = p78vom.p78ID"; oc.SqlExplicitGroupBy = "p78vom.AktualniStav";
            oc = AF("p78VomKomu", "Komu odesláno", "p78vom.Komu"); oc.RelSqlInCol = "LEFT OUTER JOIN view_p78_sendbyemail p78vom On a.p78ID = p78vom.p78ID";



            AppendTimestamp();
        }
    }
}
