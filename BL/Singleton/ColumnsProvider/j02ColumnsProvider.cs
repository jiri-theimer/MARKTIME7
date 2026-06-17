
namespace BL
{
    public class j02ColumnsProvider:ColumnsProviderBase
    {
        public j02ColumnsProvider()
        {
            this.EntityName = "j02User";

            oc=AF("fullname_desc", "Příjmení+Jméno", "a.j02Name", "string",false);oc.DefaultColumnFlag=gdc1;oc.NotShowRelInHeader = true;oc.SqlExplicitGroupBy = "a.j02ID";

            oc=AF("fullname_asc", "Jméno+Příjmení", "isnull(a.j02TitleBeforeName+' ','')+a.j02FirstName+' '+a.j02LastName+isnull(' '+a.j02TitleAfterName,'')", "string");oc.NotShowRelInHeader = true;oc.SqlExplicitGroupBy = "a.j02ID";

            
            //oc = AF("Pozice", "Pozice", "j07.j07Name"); oc.RelSqlInCol = "LEFT OUTER JOIN j07PersonPosition j07 ON a.j07ID=j07.j07ID"; oc.NotShowRelInHeader = true; oc.DefaultColumnFlag = gdc1;
            //oc = AF("AppRole", "Aplikační role", "j04.j04Name"); oc.RelSqlInCol = "INNER JOIN j04UserRole j04 ON a.j04ID=j04.j04ID"; oc.NotShowRelInHeader = true; oc.DefaultColumnFlag = gdc2;

            AF("j02Login", "Login", "case when a.j02VirtualParentID is null then a.j02Login end").DefaultColumnFlag = gdc2;
            oc = AF("inicialy", "Iniciály", "UPPER(LEFT(a.j02FirstName,1))+UPPER(LEFT(a.j02LastName,1))", "string"); oc.NotShowRelInHeader = true; oc.SqlExplicitGroupBy = "a.j02ID";oc.FixedWidth = 50;
            oc = AF("inicialy2", "Iniciály²", "UPPER(LEFT(a.j02FirstName,1))+LOWER(SUBSTRING(a.j02FirstName,2,1))+UPPER(LEFT(a.j02LastName,1))+LOWER(SUBSTRING(a.j02LastName,2,1))", "string"); oc.NotShowRelInHeader = true; oc.SqlExplicitGroupBy = "a.j02ID";oc.FixedWidth = 50;

            AFBOOL("j02IsLoginManualLocked", "Uzavřený účet");
            AFBOOL("j02IsLoginAutoLocked", "Automaticky zablokovaný účet");
            AFDATE("j02AutoLockedWhen", "Čas zablokování účtu");
            AF("j02Email", "E-mail").DefaultColumnFlag = gdc2;
            AF("j02FirstName", "Křestní jméno");
            AF("j02LastName", "Příjmení");
            AF("j02TitleBeforeName", "Titul před");
            AF("j02TitleAfterName", "Titul za");
           
            

            oc = AF("AplikacniRoleUzivatele", "Aplikační role", "case when a.j02VirtualParentID is null then j02_j04y.j04Name end");oc.DefaultColumnFlag = gdc2; oc.RelSqlInCol = "LEFT OUTER JOIN j04UserRole j02_j04y ON a.j04ID=j02_j04y.j04ID"; oc.NotShowRelInHeader = true;oc.SqlExplicitGroupBy = "a.j04ID";

            oc = AF("PoziceUzivatele", "Pozice", "j02_j07y.j07Name"); oc.DefaultColumnFlag = gdc2; oc.RelSqlInCol = "LEFT OUTER JOIN j07PersonPosition j02_j07y ON a.j07ID=j02_j07y.j07ID"; oc.NotShowRelInHeader = true;oc.SqlExplicitGroupBy = "a.j07ID";


            oc = AF("PracFond", "Pracovní fond", "c21.c21Name"); oc.RelSqlInCol = "LEFT OUTER JOIN c21FondCalendar c21 ON a.c21ID=c21.c21ID"; oc.NotShowRelInHeader = true; oc.SqlExplicitGroupBy = "a.c21ID";
            oc = AF("Stredisko", "Středisko", "j18.j18Name"); oc.RelSqlInCol = "LEFT OUTER JOIN j18CostUnit j18 ON a.j18ID=j18.j18ID"; oc.NotShowRelInHeader = true; oc.SqlExplicitGroupBy = "a.j18ID";

            AFBOOL("j02IsMustChangePassword", "Musí změnit heslo");
            
            AF("LangIndex", "Jazyk", "case isnull(a.j02LangIndex,0) when 0 then 'Česky' when 1 then 'English' when 4 then 'Slovenčina' end");

            oc=AF("j02Code", "Osobní kód");oc.FixedWidth = 80;
            oc = AF("j02Plan_Internal_Rate", "Simulační nákl.sazba", null, "num");

            AF("j02CountryCode", "ISO kód státu");

            AF("j02Ping_TimeStamp", "PING naposledy", null, "datetime");
            //AFDATE("j02Ping_TimeStamp", "PING naposledy");

            oc = AF("TagsHtml", "Štítky", "j02_o54x.o54InlineHtml"); oc.RelSqlInCol = "LEFT OUTER JOIN o54TagBindingInline j02_o54x ON a.j02ID=j02_o54x.o54RecordPid AND j02_o54x.o54RecordEntity='j02'";
            //oc = AF("TagsText", "Štítky (text)", "j02_o54x.o54InlineText"); oc.RelSqlInCol = "LEFT OUTER JOIN o54TagBindingInline j02_o54x ON a.j02ID=j02_o54x.o54RecordPid AND j02_o54x.o54RecordEntity='j02'";


            AF("j02EmailSignature", "E-mail podpis");
            AF("j02DefaultHoursFormat", "Formát hodin", "case when a.j02DefaultHoursFormat='T' then 'HH:MM' else 'NUM' end");
            AF("j02TimesheetEntryDaysBackLimit", "Omezení zpětně vykazovat", "-1*a.j02TimesheetEntryDaysBackLimit","num0");
            




            //AF("VazbaKlient", "Vazba na klienta", "dbo.j02_clients_inline(a.j02ID)");

            AppendTimestamp();





        }
    }
}
