

namespace BL.Singleton.ColumnsProvider
{
    public class p84ColumnsProvider: ColumnsProviderBase
    {
        public p84ColumnsProvider()
        {
            this.EntityName = "p84Upominka";
            this.CurrentFieldGroup = "Root";
            oc = AF("p84Code", "Číslo upomínky", null, "string"); oc.FixedWidth = 110;
            AF("p84AmountDebt", "Dlužná částka", "a.p84AmountDebt", "num");
            oc = AF("CisloFaktury", "Číslo faktury", "p91x.p91Code", "string"); oc.DefaultColumnFlag = gdc1; oc.NotShowRelInHeader = true; oc.FixedWidth = 110;
            oc = AF("p84Name", "Název"); oc.NotShowRelInHeader = true; oc.DefaultColumnFlag = gdc1;

            oc = AF("TypUpominky", "Typ upomínky", "p83x.p83Name");


            AF("p84Date", "Datum upomínky", null, "date").DefaultColumnFlag = gdc1;
            AF("p84Index", "Stupeň", "a.p84Index", "num0");
            AF("p84TextA", "Text upomínky").DefaultColumnFlag = gdc2;
            AF("p84TextB", "Technický text");


            this.CurrentFieldGroup = "Elektronicky odesláno";
            oc = AF("p84VomKdyOdeslano", "Čas odeslání", "p84vom.Kdy_Odeslano", "datetime"); oc.RelSqlInCol = "LEFT OUTER JOIN view_p84_sendbyemail p84vom On a.p84ID = p84vom.p84ID";
            oc = AF("p84VomStav", "Stav odeslání", "p84vom.AktualniStav"); oc.RelSqlInCol = "LEFT OUTER JOIN view_p84_sendbyemail p84vom On a.p84ID = p84vom.p84ID"; oc.SqlExplicitGroupBy = "p84vom.AktualniStav";
            oc = AF("p84VomKomu", "Komu odesláno", "p84vom.Komu"); oc.RelSqlInCol = "LEFT OUTER JOIN view_p84_sendbyemail p84vom On a.p84ID = p84vom.p84ID";



            AppendTimestamp();
        }
    }
}
