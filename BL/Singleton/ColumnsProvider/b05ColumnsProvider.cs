

namespace BL.Singleton.ColumnsProvider
{
    public class b05ColumnsProvider:ColumnsProviderBase
    {
        public b05ColumnsProvider()
        {
            this.EntityName = "b05Workflow_History";
            AF("b05DateInsert", "Čas", null, "datetime").DefaultColumnFlag = gdc1;
            AF("b05Name", "Název", "case when a.b05Name is not null then a.b05Name else b06x.b06Name end");
            oc = AF("Text200", "Text", "a.b05NotepadText200", "string"); oc.FixedWidth = 340;
            oc = AF("NazevPlusText", "Název+Text", "isnull('<mark>'+a.b05Name+'</mark>','')+isnull(a.b05NotepadText200,'')", "string"); oc.FixedWidth = 340;
            AF("Posun", "Krok/Změna stavu", "case when a.b02ID_To is not null then isnull(b06x.b06Name+': ','')+isnull(b02from.b02Name,'')+' -> '+b02to.b02Name end");
            AF("Krok", "Workflow krok", "b06x.b06Name");
            AFDATE("b05Date", "Datum");
            AF("b05IsCommentOnly", "Pouze komentář", null, "bool");
            AF("b05IsManualStep", "Ruční krok", null, "bool");

            oc=AF("DruhVazby", "Druh vazby", "dbo.get_entity_alias(a.b05RecordEntity)");oc.SqlExplicitGroupBy = "dbo.get_entity_alias(a.b05RecordEntity)";
            oc = AF("Projekt", "Vazba: Projekt (plus klient)", "isnull(p28x.p28Name+' - ','')+b05_p41x.p41Name"); oc.RelSqlInCol = "LEFT OUTER JOIN p41Project b05_p41x ON a.b05RecordPid=b05_p41x.p41ID AND a.b05RecordEntity='p41' LEFT OUTER JOIN p28Contact p28x ON b05_p41x.p28ID_Client=p28x.p28ID";
            oc = AF("p41Name", "Vazba: Název projektu", "b05_p41.p41Name"); oc.RelSqlInCol = "LEFT OUTER JOIN p41Project b05_p41 ON a.b05RecordPid=b05_p41.p41ID AND a.b05RecordEntity='p41'";

            oc = AF("p28Name", "Vazba: Kontakt", "b05_p28.p28Name"); oc.RelSqlInCol = "LEFT OUTER JOIN p28Contact b05_p28 ON a.b05RecordPid=b05_p28.p28ID AND a.b05RecordEntity='p28'";

            oc = AF("Ukol", "Vazba: Úkol (plus projekt)", "isnull(p41x.p41Name+' - ','')+b05_p56x.p56Name"); oc.RelSqlInCol = "LEFT OUTER JOIN p56Task b05_p56x ON a.b05RecordPid=b05_p56x.p56ID AND a.b05RecordEntity='p56' LEFT OUTER JOIN p41Project p41x ON b05_p56x.p41ID=p41x.p41ID";
            oc = AF("p56Name", "Vazba: Název úkolu", "b05_p56.p56Name"); oc.RelSqlInCol = "LEFT OUTER JOIN p56Task b05_p56 ON a.b05RecordPid=b05_p56.p56ID AND a.b05RecordEntity='p56'";
            
            oc = AF("Vyuctovani", "Vazba: Vyúčtování", "b05_p91.p91Code+isnull(': '+b05_p91.p91Client,'')"); oc.RelSqlInCol = "LEFT OUTER JOIN p91Invoice b05_p91 ON a.b05RecordPid=b05_p91.p91ID AND a.b05RecordEntity='p91'";
            oc = AF("Zaloha", "Vazba: Záloha", "b05_p90.p90Code"); oc.RelSqlInCol = "LEFT OUTER JOIN p90Proforma b05_p90 ON a.b05RecordPid=b05_p90.p90ID AND a.b05RecordEntity='p90'";

            oc = AF("Uzivatel", "Vazba: Uživatel", "b05_j02.j02Name"); oc.RelSqlInCol = "LEFT OUTER JOIN j02User b05_j02 ON a.b05RecordPid=b05_j02.j02ID AND a.b05RecordEntity='j02'";

            oc = AF("Dokument", "Vazba: Dokument", "isnull(b05_o23.o23Name+' ('+o18.o18Name+')',o18.o18Name)"); oc.RelSqlInCol = "LEFT OUTER JOIN o23Doc b05_o23 ON a.b05RecordPid=b05_o23.o23ID AND a.b05RecordEntity='o23' LEFT OUTER JOIN o18DocType o18 ON b05_o23.o18ID=o18.18ID";

            

            AppendTimestamp();
        }
    }
}
