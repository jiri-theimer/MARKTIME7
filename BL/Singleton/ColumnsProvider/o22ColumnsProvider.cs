

namespace BL
{
    public class o22ColumnsProvider: ColumnsProviderBase
    {
        public o22ColumnsProvider()
        {
            this.EntityName = "o22Milestone";

            this.CurrentFieldGroup = "Root";
            
            oc=AF("o22Name", "Název"); oc.NotShowRelInHeader = true; oc.DefaultColumnFlag = gdc1;

            oc=AF("Typ", "Typ", "o21x.o21Name+' ('+case o21x.o21TypeFlag when 1 then 'Událost' when 2 then 'Lhůta' when 3 then 'Milník' when 4 then 'Kapacita' end+')'");oc.DefaultColumnFlag = BO.TheGridDefColFlag.GridAndCombo;oc.SqlExplicitGroupBy = "o21x.o21TypeFlag";
            oc = AF("o22PlanFrom", "Začátek",null,"datetime"); oc.DefaultColumnFlag = gdc1;
            
            oc = AF("o22PlanUntil", "Konec",null,"datetime"); oc.DefaultColumnFlag = gdc1;
            oc = AF("KonecRok", "Konec rok", "convert(varchar(4),a.o22PlanUntil,126)"); oc.SqlExplicitGroupBy = "convert(varchar(4),a.o22PlanUntil,126)"; oc.FixedWidth = 80;
            oc = AF("KonecMesic", "Konec měsíc", "convert(varchar(7),a.o22PlanUntil,126)"); oc.SqlExplicitGroupBy = "convert(varchar(7),a.o22PlanUntil,126)"; oc.FixedWidth = 80;

            oc = AF("CasOd", "Čas od", "case when DATEPART(HOUR,a.o22PlanFrom)>0 or DATEPART(MINUTE,a.o22PlanFrom)>0 then a.o22PlanFrom end", "time");
            oc = AF("CasDo", "Čas do", "case when DATEPART(HOUR,a.o22PlanFrom)>0 or DATEPART(MINUTE,a.o22PlanFrom)>0 then a.o22PlanUntil end", "time");

            AF("o22Location", "Lokalita (text)");

            AF("Jednotky", "Jednotky", "case a.o22DurationUnit when 'd' then 'Dny' when 'e' then 'Prac.dny' when 'w' then 'Týdny' when 'm' then 'Měsíce' when 'y' then 'Roky' end");
            AFNUM0("o22DurationCount", "Počet jednotek");

            AF("PocetDni", "Počet dní", "DATEDIFF(DAY,a.o22PlanFrom,a.o22PlanUntil)", "num0");
            AF("VuciDnes", "K Dnes", "DATEDIFF(DAY,a.o22PlanUntil,GETDATE())", "num0");

            oc = AF("o21Color", "Barva", "'<div style=\"background-color:'+o21x.o21Color+';\">...</div>'");oc.FixedWidth = 30;
            oc = AF("TagsHtml", "Štítky", "o22_o54x.o54InlineHtml"); oc.RelSqlInCol = "LEFT OUTER JOIN o54TagBindingInline o22_o54x ON a.o22ID=o22_o54x.o54RecordPid AND o22_o54x.o54RecordEntity='o22'";
            oc = AF("TagsText", "Štítky (text)", "o22_o54x.o54InlineText"); oc.RelSqlInCol = "LEFT OUTER JOIN o54TagBindingInline o22_o54x ON a.o22ID=o22_o54x.o54RecordPid AND o22_o54x.o54RecordEntity='o22'";

            AppendTimestamp();
        }
    }
}
