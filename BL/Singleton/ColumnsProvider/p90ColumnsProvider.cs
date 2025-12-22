

namespace BL
{
    public class p90ColumnsProvider:ColumnsProviderBase
    {
        public p90ColumnsProvider()
        {
            this.EntityName = "p90Proforma";


            this.CurrentFieldGroup = "Root";
            oc = AF("p90Code", "Číslo zálohy", null, "string"); oc.DefaultColumnFlag = gdc1; oc.NotShowRelInHeader = true; oc.FixedWidth = 110;
            oc = AF("p90CodeNumeric", "Variabilní symbol", null, "string"); oc.NotShowRelInHeader = true; oc.FixedWidth = 110;

            AF("p90Text1", "Text zálohy").DefaultColumnFlag = gdc2;
            AF("p90Text2", "Technický text");
           
            oc=AFBOOL("p90IsDraft", "Draft");oc.SqlExplicitGroupBy = "a.p90IsDraft";

            oc = AF("Client", "Klient", "p90client.p28Name"); oc.RelSqlInCol = "LEFT OUTER JOIN p28Contact p90client ON a.p28ID=p90client.p28ID";oc.NotShowRelInHeader = true;oc.SqlExplicitGroupBy = "a.p28ID";


            oc = AFDATE("p90Date", "Vystaveno"); oc.DefaultColumnFlag = gdc2;  oc.SqlExplicitGroupBy = "a.p90Date";oc.SqlExplicitGroupBy = "a.p90Date";
            oc = AFDATE("p90DateMaturity", "Splatnost"); oc.DefaultColumnFlag = gdc2; oc.SqlExplicitGroupBy = "a.p90DateMaturity";

            AF("p90Amount", "Částka", null, "num", true).DefaultColumnFlag = gdc1;
            AF("p90Amount_Billed", "Uhrazeno", null, "num", true);
            AF("p90Amount_Debt", "Dluh", null, "num", true);
            AF("p90Amount_WithoutVat", "Bez DPH", null, "num", true);
            AF("p90Amount_Vat", "Částka DPH", null, "num", true);
            oc=AFNUM0("p90VatRate", "Sazba DPH");oc.SqlExplicitGroupBy = "a.p90VatRate";

            oc = AF("j27Code", "Měna", "p90_j27x.j27Code"); oc.RelSqlInCol = "LEFT OUTER JOIN j27Currency p90_j27x ON a.j27ID=p90_j27x.j27ID"; oc.FixedWidth = 60;oc.SqlExplicitGroupBy = "a.j27ID";

            oc = AF("TagsHtml", "Štítky", "p90_o54x.o54InlineHtml"); oc.RelSqlInCol = "LEFT OUTER JOIN o54TagBindingInline p90_o54x ON a.p90ID=p90_o54x.o54RecordPid AND p90_o54x.o54RecordEntity='p90'";

            this.CurrentFieldGroup = "Párování úhrad zálohy";
            AF("p91codes", "Spárované daňové faktury", "dbo.p90_get_bind_p91codes(a.p90ID)");
            AF("p82codes", "Spárované DPP", "dbo.p90_get_bind_p82codes(a.p90ID)");

            string s = "LEFT OUTER JOIN (select sum(p99Amount_WithoutVat) as JizSparovano,p82.p90ID FROM p99Invoice_Proforma a INNER JOIN p82Proforma_Payment p82 ON a.p82ID=p82.p82ID GROUP BY p82.p90ID) sparovano ON a.p90ID=sparovano.p90ID";
            s += " LEFT OUTER JOIN (select SUM(p82Amount_WithoutVat) as JizUhrazeno,p90ID,MAX(p82Date) as NaposledyUhrazeno FROM p82Proforma_Payment GROUP BY p90ID) uhrazeno ON a.p90ID=uhrazeno.p90ID";
            oc=AF("JizSparovano", "Spárované úhrady", "sparovano.JizSparovano", "num", true);oc.RelSqlInCol = s;
            oc = AF("ChybiSparovat", "Zbývá spárovat", "isnull(uhrazeno.JizUhrazeno,0)-isnull(sparovano.JizSparovano,0)", "num", true); oc.RelSqlInCol = s;
            oc = AF("NaposledyUhrazeno", "Kdy uhrazeno", "uhrazeno.NaposledyUhrazeno", "date");

            

            AppendTimestamp();
        }
    }
}
