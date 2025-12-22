

namespace BL
{
    public class p56ColumnsProvider: ColumnsProviderBase
    {
        public p56ColumnsProvider()
        {
            this.EntityName = "p56Task";

            this.CurrentFieldGroup = "Root";
            oc = AF("p56Name", "Název úkolu", null, "string"); oc.NotShowRelInHeader = true; oc.DefaultColumnFlag = gdc1;oc.SqlExplicitGroupBy = "a.p56Name";
            oc = AF("AktualniStav", "Aktuální stav", "b02x.b02Name");oc.DefaultColumnFlag = gdc1; oc.SqlExplicitGroupBy = "a.b02ID";
            
            oc = AF("p56Code", "Kód"); oc.FixedWidth = 100;

            oc =AF("TypUkolu", "Typ úkolu", "p57x.p57Name");oc.SqlExplicitGroupBy = "a.p57ID"; oc.NotShowRelInHeader = true;

            oc =AFDATE("p56PlanUntil", "Termín dokončení");oc.DefaultColumnFlag = gdc1;oc.SqlExplicitGroupBy = "a.p56PlanUntil"; oc.NotShowRelInHeader = true;
            oc = AF("TerminYear", "Rok dokončení", "convert(varchar(4),a.p56PlanUntil,126)"); oc.SqlExplicitGroupBy = "convert(varchar(4),a.p56PlanUntil,126)";oc.FixedWidth = 80; oc.NotShowRelInHeader = true;
            oc = AF("TerminnMesic", "Měsíc dokončení", "convert(varchar(7),a.p56PlanUntil,126)"); oc.SqlExplicitGroupBy = "convert(varchar(7),a.p56PlanUntil,126)";oc.FixedWidth = 80; oc.NotShowRelInHeader = true;
            oc =AFDATE("p56PlanFrom", "Plánované zahájení");oc.DefaultColumnFlag = gdc1;oc.SqlExplicitGroupBy = "a.p56PlanFrom";
            AFNUM0("p56Ordinary", "#");

            oc = AF("ToDoList", "Todo-list", "p56_p55x.p55Name"); oc.RelSqlInCol = "LEFT OUTER JOIN p55TodoList p56_p55x ON a.p55ID=p56_p55x.p55ID";oc.SqlExplicitGroupBy = "a.p55ID";oc.NotShowRelInHeader = true;
            oc = AF("Recurrence", "Opakovaný úkol", "p58.p58Name"); oc.RelSqlInCol = "LEFT OUTER JOIN p59TaskRecurrence_Plan p59 ON a.p56ID=p59.p56ID_NewInstance LEFT OUTER JOIN p58TaskRecurrence p58 ON p59.p58ID=p58.p58ID"; oc.NotShowRelInHeader = true;

            //oc = AF("BarvaUkolu", "Barva", "'<div style=\"background-color:'+b02x.b02Color+';\">...</div>'"); oc.FixedWidth = 40;

            oc = AF("TagsHtml", "Štítky", "p56_o54x.o54InlineHtml"); oc.RelSqlInCol = "LEFT OUTER JOIN o54TagBindingInline p56_o54x ON a.p56ID=p56_o54x.o54RecordPid AND p56_o54x.o54RecordEntity='p56'";
            //oc = AF("TagsText", "Štítky (text)", "p56_o54x.o54InlineText"); oc.RelSqlInCol = "LEFT OUTER JOIN o54TagBindingInline p56_o54x ON a.p56ID=p56_o54x.o54RecordPid AND p56_o54x.o54RecordEntity='p56'";

            //AF("OutlookStav", "outlook stav", "case a.p56StatusFlag when 1 then 'Nezahájeno' when 2 then 'Probíhá' when 3 then 'Dokončeno' when 4 then 'Čeká na někoho jiného' when 5 then 'Odloženo' end");
            oc = AF("VlastnikUkolu", "Vlastník záznamu", "p56_j02owner.j02Name"); oc.RelSqlInCol = "LEFT OUTER JOIN j02User p56_j02owner ON a.j02ID_Owner=p56_j02owner.j02ID";

            this.CurrentFieldGroup = "Rozpočet úkolu";
            oc=AF("p56Plan_Hours", "Plán hodin", "case when p56Plan_Hours>0 then a.p56Plan_Hours end", "num",true); oc.DefaultColumnFlag = gdc1;
            AF("p56Plan_Expenses", "Plán výdajů", null, "num",true);
            AF("p56Plan_Revenue", "Plán odměn", "a.p56Plan_Revenue", "num", true);
            oc = AF("p56Plan_Internal_Fee", "Plán ceny hodin", "a.p56Plan_Internal_Fee", "num", true);

            //AF("DnuDoTerminu", "Dnů do termínu", "datediff(day,getdate(),a.p56PlanUntil)", "num0");
            //AF("HodinDoTerminu", "Hodin do termínu", "datediff(hour,getdate(),a.p56PlanUntil)", "num0");
            //AF("DnuPoTerminu", "Dnů po termínu", "datediff(day,a.p56PlanUntil,getdate())", "num0");
            //AF("HodinPoTerminu", "Hodin po termínu", "datediff(hour,a.p56PlanUntil,getdate())", "num0");




            AppendTimestamp();



            

        }

    }
}
