

namespace BL
{
    public class defColumnsProvider : ColumnsProviderBase
    {


        public defColumnsProvider()
        {



            this.EntityName = "j04UserRole";
            AA("j04Name", "Aplikační role", gdc1, null, "string", false, true);
            AppendTimestamp();

            this.EntityName = "j05MasterSlave";
            AA("MasterPerson", "Nadřízený", gdc1, "j02master.j02LastName+' '+j02master.j02FirstName+isnull(' '+j02master.j02TitleBeforeName,'')", "string", false, true);
            AA("SlavePerson", "Podřízený (jednotlivec)", gdc1, "j02slave.j02LastName+' '+j02slave.j02FirstName+isnull(' '+j02slave.j02TitleBeforeName,'')", "string");
            AA("SlaveTeam", "Podřízený tým", gdc1, "j11slave.j11Name", "string");
            AppendTimestamp();



            this.EntityName = "j07PersonPosition";
            AA("j07Name", "Pozice", gdc1, null, "string", false, true);

            AFNUM0("j07Ordinary", "#").DefaultColumnFlag = gdc2;

            AppendTimestamp();

            this.EntityName = "j18CostUnit";
            AA("j18Name", "Středisko", gdc1, null, "string", false, true);
            AFNUM0("j18Ordinary", "#").DefaultColumnFlag = gdc2;
            AA("j18Code", "Kód");
            AppendTimestamp();



            this.EntityName = "j19PaymentType";
            AA("j19Name", "Forma úhrady", gdc1, null, "string", false, true);
            AFNUM0("j19Ordinary", "#");

            this.EntityName = "c21FondCalendar";
            AA("c21Name", "Název fondu", gdc1, null, "string", false, true);
            AFNUM0("c21Ordinary", "#").DefaultColumnFlag = gdc2;
            AppendTimestamp();

            this.EntityName = "c24DayColor";
            AA("c24Name", "Název", gdc1, null, "string", false, true);
            AA("o21Color", "Barva", gdc1, "'<div style=\"background-color:'+a.c24Color+';\">...</div>'").FixedWidth = 30;
            AppendTimestamp();

            this.EntityName = "c26Holiday";
            AFDATE("c26Date", "Datum").DefaultColumnFlag = gdc1;
            AA("c26Name", "Název svátku", gdc1, null, "string", false, true);
            AA("c26CountryCode", "ISO kód státu");
            AppendTimestamp();

            this.EntityName = "p24ContactGroup";
            AA("p24Name", "Skupina kontaktů", gdc1, null, "string", false, true);
            AA("p24Email", "Skupinový e-mail");
            AppendTimestamp();

            this.EntityName = "j11Team";
            AA("j11Name", "Pracovní tým", gdc1, null, "string", false, true);
            AA("j11Email", "Skupinový e-mail");
            AppendTimestamp();

            this.EntityName = "j25ReportCategory";
            AA("j25Name", "Kategorie sestav", gdc1, null, "string", false, true);
            AFNUM0("j25Ordinary", "#").DefaultColumnFlag = gdc2;
            AppendTimestamp();

            this.EntityName = "j27Currency";
            oc = AA("j27Code", "Měna", gdc1, null, "string", false, true); oc.FixedWidth = 70;
            AA("j27Name", "Název měny");

            this.EntityName = "j61TextTemplate";
            AA("j61Name", "Šablona zprávy", gdc1, null, "string", false, true);
            AA("j61Entity", "Entita", gdc2, "dbo.get_entity_alias(a.j61Entity)");
            AA("j61MailSubject", "Předmět zprávy", gdc2);
            AA("j61MailTO", "TO");
            AA("j61MailCC", "CC");
            AA("j61MailBCC", "BCC");
            AppendTimestamp();

            this.EntityName = "p15Location";
            AA("p15Name", "Lokalita", gdc1, null, "string", false, true);
            AA("p15Street", "Ulice", gdc1, null, "string", false, true);
            AA("p15City", "Město", gdc1, null, "string", false, true);
            AppendTimestamp();

            //b20 = hlídač
            this.EntityName = "b20Hlidac";
            oc = AA("b20Name", "Hlídač", gdc1, null, "string", false, true);
            oc = AA("Entita", "Entita", gdc1, "dbo.get_entity_alias(a.b20Entity)"); oc.FixedWidth = 140;
            AFNUM0("b20Ordinary", "#").DefaultColumnFlag = gdc2;
            AppendTimestamp();

            //b01 = workflow šablona     
            this.EntityName = "b01WorkflowTemplate";
            oc = AA("b01Name", "Workflow šablona", gdc1, null, "string", false, true);
            oc = AA("b01Code", "Kód"); oc.FixedWidth = 70;
            AppendTimestamp();

            //b02 = workflow stav                        
            this.EntityName = "b02WorkflowStatus";
            AA("b02Name", "Stav", gdc1);
            oc = AA("b02Code", "Kód stavu", gdc1); oc.FixedWidth = 70;
            AFBOOL("b02IsDefaultStatus", "Výchozí stav").DefaultColumnFlag = gdc2;
            AFNUM0("b02Ordinary", "#").DefaultColumnFlag = gdc2;
            AppendTimestamp();

            this.EntityName = "j90LoginAccessLog";
            AA("j90Date", "Čas", gdc1, null, "datetime");
            AA("j90BrowserFamily", "Prohlížeč", gdc1);
            AA("j90Platform", "OS", gdc1);
            AA("j90BrowserDeviceType", "Device");
            AA("j90ScreenPixelsWidth", "Šířka (px)", gdc1);
            AA("j90ScreenPixelsHeight", "Výška (px)", gdc1);
            AA("j90UserHostAddress", "Host", gdc1);
            AA("j90LoginMessage", "Chyba", gdc1);
            oc = AFNUM0("j90CookieExpiresInHours", "Expirace přihlášení"); oc.DefaultColumnFlag = gdc1;
            AA("j90LoginName", "Login", gdc1);

            //j92 = ping log uživatelů
            this.EntityName = "j92PingLog";
            AA("j92Date", "Čas", gdc1, null, "datetime");
            AA("j92BrowserFamily", "Prohlížeč", gdc1);
            AA("j92BrowserOS", "OS", gdc1);
            AA("j92BrowserDeviceType", "Device", gdc1);
            AA("j92BrowserAvailWidth", "Šířka (px)", gdc1);
            AA("j92BrowserAvailHeight", "Výška (px)", gdc1);
            AA("j92RequestURL", "Url", gdc1);

            this.EntityName = "j95GeoWeatherLog";
            AA("j95Date", "Čas", gdc1, null, "datetime");
            AA("j95Description", "Slovně", gdc1);
            AA("j95Temp", "Teplota", gdc1, null, "num");

            AF("j95TempFeelsLike", "Pocitová tepl.", null, "num");
            AF("j95TempMin", "Min tepl.", null, "num");
            AF("j95TempMax", "Max tepl.", null, "num");
            AA("j95WindSpeed", "Vítr", gdc1, null, "num");
            AF("j95WindDeg", "Směr větru", null, "num0");
            AF("j95Pressure", "Tlak", null, "num0");
            AA("j95Humidity", "Vlhkost", gdc1, null, "num0");
            AF("j95Visibility", "Viditelnost", null, "num0");
            AA("j95Location", "Lokalita", gdc1);

            this.EntityName = "o15AutoComplete";
            AA("o15Value", "Hodnota", gdc1);
            AA("o15Flag", "Typ dat", gdc1, "case a.o15Flag when 1 then 'Titul před' when 2 then 'Titul za' when 3 then 'Pracovní funkce' when 328 then 'Stát' when 427 then 'URL adresa' end");
            oc = AFNUM0("o15Ordinary", "#"); oc.DefaultColumnFlag = gdc2;

            this.EntityName = "p07ProjectLevel";
            AA("p07Name", "Úroveň", gdc1, null, "string", false, true);
            oc = AA("LevelIndex", "Index úrovně", gdc1, "'L'+convert(varchar(10),a.p07Level)", "string", false, true);
            AA("p07NamePlural", "Množné číslo", gdc2);
            AA("p07NameInflection", "Koho čeho");
            AppendTimestamp();

            this.EntityName = "p42ProjectType";
            AA("p42Name", "Typ projektu", gdc1, null, "string", false, true);
            AA("p42Code", "Kód");
                        
            AFNUM0("p42Ordinary", "#");
            AppendTimestamp();

            this.EntityName = "p29ContactType";
            AA("p29Name", "Typ kontaktu", gdc1, null, "string", false, true);
            AFNUM0("p29Ordinary", "#");
            AF("Scope", "Rozsah kontaktu", "case a.p29ScopeFlag when 0 then 'Kdokoliv' when 1 then 'Pouze klient' when 2 then 'Pouze dodavatel' when 3 then 'Klient nebo dodavatel' when 4 then 'Pouze fyzické osoby' end").DefaultColumnFlag = BO.TheGridDefColFlag.GridOnly;
            AppendTimestamp();

            this.EntityName = "p57TaskType";
            AA("p57Name", "Název", gdc1, null, "string", false, true);
            AFNUM0("p57Ordinary", "#");
            AppendTimestamp();

            this.EntityName = "p55TodoList";
            AA("p55Name", "Todo-list", gdc1, null, "string", false, true);
            AA("p55UserInsert", "Založil", gdc1, null, "string", false, true);
            AA("p55DateInsert", "Založeno", gdc1, null, "datetime", false, true);
            string strRelSql = "LEFT OUTER JOIN (select p55ID,count(*) as Pocet,sum(case when GETDATE() between p56ValidFrom AND p56ValidUntil then 1 end) as PocetOtevrenych FROM p56Task GROUP BY p55ID) xxa ON a.p55ID=xxa.p55ID";
            oc = AF("PocetUkolu", "Počet úkolů", "xxa.Pocet", "num0", true); oc.RelSqlInCol = strRelSql;
            oc = AF("PocetOtevrenychUkolu", "Otevřené úkoly", "xxa.PocetOtevrenych", "num0", true); oc.RelSqlInCol = strRelSql;

            this.EntityName = "o21MilestoneType";
            AA("o21Name", "Název", gdc1, null, "string", false, true);
            AFNUM0("o21Ordinary", "#");
            AF("Typ", "Typ", "case a.o21TypeFlag when 1 then 'Událost' when 2 then 'Lhůta' when 3 then 'Milník' when 4 then 'Kapacita' end").DefaultColumnFlag = BO.TheGridDefColFlag.GridAndCombo;
            AA("o21Color", "Barva", gdc1, "'<div style=\"background-color:'+a.o21Color+';\">...</div>'").FixedWidth = 30;
            AppendTimestamp();



            this.EntityName = "p39WorkSheet_Recurrence_Plan";
            AA("p39Text", "Text aktivity", gdc1, null, "string");
            AA("p39Date", "Datum úkonu", gdc1, null, "date");
            AA("p39DateCreate", "Datum generování", gdc1, null, "date");
            AA("p31Date", "Datum vygenerovaného úkonu", gdc0, "p31.p31Date", "date");
            AA("p31DateInsert", "Čas vygenerování", gdc1, "p31.p31DateInsert", "datetime");
            AA("p31Text", "Text vygenerovaného úkonu", gdc0, "p31.p31Text", "string");
            AA("p39ErrorMessage_NewInstance", "Chyba generování", gdc1, null, "string");

            this.EntityName = "p40WorkSheet_Recurrence";
            AA("p40Name", "Název předpisu", gdc1, null, "string", false, true);
            AA("p40RecurrenceType", "Frekvence generování", gdc1, "case a.p40RecurrenceType when 1 then 'Denní' when 2 then 'Týdenní' when 3 then 'Měsíční' when 4 then 'Čtvrtletní' when 5 then 'Roční' end", "string", false, true); oc.SqlExplicitGroupBy = "a.p40RecurrenceType";
            oc = AA("p40Value", "Cena/Hodnota", gdc1, null, "num"); oc.SqlExplicitGroupBy = "a.p40Value";
            oc = AA("Mena", "Měna", gdc1, "j27x.j27Code"); oc.FixedWidth = 50; oc.SqlExplicitGroupBy = "a.j27ID";
            oc = AA("p40Text", "Maska textu úkonu", gdc1, null, "string", false, true); oc.SqlExplicitGroupBy = "a.p40Text";
            oc = AA("p40TextInternal", "Interní textu úkonu", gdc1, null, "string", false, true); oc.SqlExplicitGroupBy = "a.p40TextInternal";
            oc = AA("HodinyZdarma", "Hodiny zdarma", gdc2, "a.p40FreeHours", "num", false, true); oc.SqlExplicitGroupBy = "a.p40FreeHours";
            AA("HonorarZdarma", "Honorář zdarma", gdc2, "a.p40FreeFee", "num", false, true);
            AA("NejblizsiGenerovani", "Nejbližší generování", gdc1, "ceka.Kdy", "date"); oc.RelSqlInCol = "LEFT OUTER JOIN view_p40_nejblizsi_generovani ceka ON a.p40ID=ceka.p40ID";
            AA("p40FirstSupplyDate", "První rozhodné datum", gdc1, null, "date"); oc.SqlExplicitGroupBy = "a.p40FirstSupplyDate";
            oc = AA("p40LastSupplyDate", "Posl.rozhodné datum", gdc1, null, "date"); oc.SqlExplicitGroupBy = "a.p40LastSupplyDate";

            this.CurrentFieldGroup = "Skluz generování (SG)";
            AA("p39Count", "Počet SG", gdc1, "p39miss.p39Count", "num0");
            AA("p39DateCreate_Min", "První SG", gdc0, "p39miss.p39DateCreate_Min", "date");
            AA("p39DateCreate_Max", "Poslední SG", gdc0, "p39miss.p39DateCreate_Max", "date");
            AA("p39Text_Max", "Text posledního SG", gdc0, "p39miss.p39Text_Max");

            AppendTimestamp();

            //this.EntityName = "view_p40_nevygenerovano";
            //AA("Pocet", "Počet", gdc1, null, "num0", true);
            //AA("p39DateCreate_Min", "První", gdc1, null, "date");
            //oc=AA("p39DateCreate_Max", "Poslední", gdc1, null, "date"); oc.NotShowRelInHeader = true;
            //oc =AA("p39Text", "Text posledního");oc.NotShowRelInHeader = true;

            this.EntityName = "view_p40_cerpano";
            AA("Hodiny", "Svázané hodiny", gdc1, null, "num", true, true);
            AA("Honorar", "Svázaný honorář", gdc0, null, "num", true, true);


            this.EntityName = "p49FinancialPlan";
            AA("PlanOdmena", "Odměna", gdc1, "case when p34x.p34IncomeStatementFlag=2 then a.p49Amount end", "num", true);
            AA("PlanVydaj", "Výdaj", gdc1, "case when p34x.p34IncomeStatementFlag=1 then a.p49Amount end", "num", true);
            AA("PlanZisk", "O-V", gdc0, "case when p34x.p34IncomeStatementFlag=2 then a.p49Amount else -1*a.p49Amount end", "num", true);
            AA("p49Amount", "Částka", gdc0, null, "num", false);
            oc = AA("Mena", "Měna", gdc0, "j27x.j27Code"); oc.FixedWidth = 50; oc.SqlExplicitGroupBy = "a.j27ID";
            AA("p49Date", "Datum", gdc1, null, "date");
            oc = AA("Mesic", "Měsíc", gdc0, "convert(varchar(7),a.p49Date,102)"); oc.FixedWidth = 70; oc.SqlExplicitGroupBy = "convert(varchar(7),a.p49Date,102)";
            oc = AA("Rok", "Rok", gdc0, "convert(varchar(4),a.p49Date,102)"); oc.FixedWidth = 50; oc.SqlExplicitGroupBy = "convert(varchar(4),a.p49Date,102)";
            AA("p49Text", "Text", gdc1, null, "string", false, true);
            oc = AA("p49Code", "Kód", gdc0); oc.FixedWidth = 100;
            oc = AA("Strana", "P|V", gdc0, "case when p34x.p34IncomeStatementFlag=2 then 'P' else 'V' end"); oc.FixedWidth = 50; oc.SqlExplicitGroupBy = "p34x.p34IncomeStatementFlag";
            oc = AA("p49StatusFlag", "Stav", gdc0, "a.p49StatusFlag", "num0");
            AA("Sesit", "Sešit", gdc0, "p34x.p34Name");
            AA("p49PieceAmount", "Cena ks", gdc0, "a.p49PieceAmount", "num", true, true);
            AA("p49Pieces", "Počet ks", gdc0, "a.p49Pieces", "num", false, true);
            oc = AF("Dodavatel", "Dodavatel", "dodavatel.p28Name"); oc.RelSqlInCol = "LEFT OUTER JOIN p28Contact dodavatel ON a.p28ID_Supplier=dodavatel.p28ID";
            AppendTimestamp();

            this.EntityName = "p75InvoiceRecurrence";
            AA("p75Name", "Název předpisu", gdc1, null, "string", false, true);
            AA("p75InvoiceText", "Text faktury");
            oc = AA("p75RecurrenceType", "Frekvence generování", gdc1, "case a.p75RecurrenceType when 3 then 'Měsíční' when 4 then 'Čtvrtletní' when 5 then 'Roční' end", "string", false, true); oc.FixedWidth = 90;
            AA("p75BaseDateStart", "První rozhodné datum", gdc1, null, "date");
            AA("p75BaseDateEnd", "Poslední rozhodné datum", gdc1, null, "date");
            AppendTimestamp();

            this.EntityName = "p58TaskRecurrence";
            AA("p58Name", "Maska názvu úkolu", gdc1, null, "string", false, true);
            oc = AA("p58RecurrenceType", "Frekvence generování", gdc1, "case a.p58RecurrenceType when 1 then 'Denní' when 2 then 'Týdenní' when 3 then 'Měsíční' when 4 then 'Čtvrtletní' when 5 then 'Roční' when 6 then '2-týdenní' end", "string", false, true); oc.FixedWidth = 90;
            AA("p58BaseDateStart", "První rozhodné datum", gdc1, null, "date");
            AA("p58BaseDateEnd", "Poslední rozhodné datum", gdc1, null, "date");
            AA("p58IsPlanUntil", "Generovat termín", gdc1, null, "bool");
            AA("p58IsPlanFrom", "Generovat zahájení", gdc1, null, "bool");

            oc = AF("Role v úkolu", "Role", "a.p58RolesInline");

            AA("p58Plan_Hours", "Plán hodin", gdc0, null, "num");
            AA("p58Plan_Expenses", "Plán pen.výdajů", gdc0, null, "num");
            AA("p58Plan_Revenue", "Plán fakturace", gdc0, null, "num");
            AppendTimestamp();

            this.EntityName = "p60TaskTemplate";
            AA("p60Name", "Název", gdc1, null, "string", false, true);
            oc = AF("TypUkolu", "Typ úkolu", "p57x.p57Name"); oc.RelSqlInCol = "LEFT OUTER JOIN p57TaskType p57x ON a.p57ID=p57x.p57ID";
            oc = AF("Vlastnik", "Vlastník záznamu", "p60_j02x.j02Name"); oc.RelSqlInCol = "LEFT OUTER JOIN j02User p60_j02x ON a.j02ID_Owner=p60_j02x.j02ID"; oc.DefaultColumnFlag = gdc1;
            AFBOOL("p60IsPublic", "Pro všechny uživatele");
            AppendTimestamp();

            this.EntityName = "p51PriceList";
            AA("p51TypeFlag", "Typ ceníku", gdc2, "case a.p51TypeFlag when 1 then 'Fakturační sazby' when 2 then 'Nákladové sazby' when 3 then 'Režijní sazby' when 5 then 'Kořenový (ROOT) ceník' when 4 then 'Efektivní sazby' end", "string", false, true);
            AA("p51Name", "Pojmenovaný ceník", gdc1, null, "string", false, true);
            AA("p51DefaultRateT", "Výchozí hod.sazba", gdc1, null, "num");
            oc = AF("Mena", "Měna", "j27x.j27Code"); oc.RelSqlInCol = "LEFT OUTER JOIN j27Currency j27x ON a.j27ID=j27x.j27ID"; oc.DefaultColumnFlag = gdc1; oc.FixedWidth = 100;
            oc = AA("p51InlineInfo", "Výjimky z výchozí sazby", gdc1); oc.FixedWidth = 300;
            AFNUM0("p51Ordinary", "#");
            oc = AFBOOL("p51IsFPR", "Vzor pro efektivní sazby");
            oc = AFBOOL("p51IsCustomTailor", "Sazby na míru"); oc.DefaultColumnFlag = gdc2;
            AppendTimestamp();

            this.EntityName = "p44ProjectTemplate";
            AA("p44Name", "Projektová šablona", gdc1, null, "string", false, true);
            AA("Pattern", "Vzorový projekt", gdc1, "isnull(p28x.p28Name+' - ','')+p41x.p41Name");
            AFNUM0("p44Ordinary", "#");
            AppendTimestamp();

            this.EntityName = "p34ActivityGroup";
            oc = AA("p34Name", "Sešit", gdc1, null, "string", false, true); oc.SqlExplicitGroupBy = "a.p34ID";
            AA("p34Code", "Kód");
            AA("p34Ordinary", "#", gdc2);
            AA("p33ID", "Vstupí data", gdc2, "case a.p33ID when 1 then 'Čas' when 2 then 'Peníze bez DPH' when 3 then 'Kusovník' when 5 then 'Peníze+DPH' end");
            oc = AF("Editor", "Notepad konfigurace", "p34_x04.x04Name"); oc.RelSqlInCol = "LEFT OUTER JOIN x04NotepadConfig p34_x04 ON a.x04ID=p34_x04.x04ID";
            AppendTimestamp();

            this.EntityName = "p38ActivityTag";
            AA("p38Name", "Odvětví aktivity", gdc1, null, "string", false, true);
            AFNUM0("p38Ordinary", "#").DefaultColumnFlag = gdc2;
            AppendTimestamp();


            this.EntityName = "p32Activity";
            oc = AA("p32Name", "Aktivita", gdc1, null, "string", false, true); oc.SqlExplicitGroupBy = "a.p32ID";
            AA("AktivitaPlusSesit", "Aktivita+Sešit", gdc2, "a.p32Name+' ('+p34x.p34Name+')'", "string", false, true);
            AA("p32Code", "Kód");
            oc = AFBOOL("p32IsBillable", "FA"); oc.DefaultColumnFlag = gdc2;
            oc = AF("Oddil", "Fakturační oddíl", "p32_p95.p95Name"); oc.NotShowRelInHeader = true; oc.RelSqlInCol = "LEFT OUTER JOIN p95InvoiceRow p32_p95 ON a.p95ID=p32_p95.p95ID"; oc.SqlExplicitGroupBy = "a.p95ID";
            oc = AF("Odvetvi", "Odvětví aktivity", "p32_p38.p38Name"); oc.NotShowRelInHeader = true; oc.RelSqlInCol = "LEFT OUTER JOIN p38ActivityTag p32_p38 ON a.p38ID=p32_p38.p38ID"; oc.SqlExplicitGroupBy = "a.p38ID";
            oc = AFNUM0("p32Ordinary", "#"); oc.DefaultColumnFlag = gdc2;
            AA("p32Color", "Barva", gdc2, "'<div style=\"background-color:'+a.p32Color+';\">...</div>'").FixedWidth = 30;
            oc = AFBOOL("IsAbsence", "Nepřítomnost"); oc.SqlSyntax = "case when isnull(a.p32AbsenceFlag,0)>0 then 1 else 0 end";
            oc = AFBOOL("IsAbsenceBreak", "Přestávka v docházce"); oc.SqlSyntax = "case when isnull(a.p32AbsenceBreakFlag,0)>0 then 1 else 0 end";
            AFBOOL("p32IsTextRequired", "Povinný text úkonu");
            AA("p32Value_Default", "Výchozí hodnota úkonu", gdc0, null, "num");
            AA("p32Value_Minimum", "MIN", gdc0, null, "num");
            AA("p32Value_Maximum", "MAX", gdc0, null, "num");
            AA("p32DefaultWorksheetText", "Výchozí text úkonu");
            AA("p32HelpText", "Nápověda/Metodika");
            AA("p32Name_BillingLang1", "Název #1");
            AA("p32Name_BillingLang2", "Název #2");
            AA("p32DefaultWorksheetText_Lang1", "Výchozí popis #1");
            AA("p32DefaultWorksheetText_Lang2", "Výchozí popis #2");

            oc = AF("Predkontace", "Předkontace [par]", "p32AccountingIds.o59Value"); oc.NotShowRelInHeader = true; oc.RelSqlInCol = "LEFT OUTER JOIN (select o58.o58Name,o59.o59Value,o59.o59RecordPid from o59GlobalParamBinding o59 INNER JOIN o58GlobalParam o58 ON o59.o58ID=o58.o58ID AND o58.o58Key='p32AccountingIds') p32AccountingIds ON a.p32ID=p32AccountingIds.o59RecordPid";
            oc = AF("Cinnost", "Činnost [par]", "p32ActivityIds.o59Value"); oc.NotShowRelInHeader = true; oc.RelSqlInCol = "LEFT OUTER JOIN (select o58.o58Name,o59.o59Value,o59.o59RecordPid from o59GlobalParamBinding o59 INNER JOIN o58GlobalParam o58 ON o59.o58ID=o58.o58ID AND o58.o58Key='p32ActivityIds') p32ActivityIds ON a.p32ID=p32ActivityIds.o59RecordPid";

            //oc = AF("Odvetvi", "Odvětví aktivity", "p32_p38.p38Name"); oc.RelSqlInCol = "LEFT OUTER JOIN p38ActivityTag p32_p38 ON a.p38ID = p32_p38.p38ID";            
            AppendTimestamp();


            this.EntityName = "p61ActivityCluster";
            AA("p61Name", "Klast aktivit", gdc1, null, "string", false, true);
            AppendTimestamp();

            this.EntityName = "p54OvertimeLevel";
            AA("p54Name", "Hladina sazby", gdc1, null, "string", false, true);
            AFNUM0("p54Ordinary", "#").DefaultColumnFlag = gdc2;
            AppendTimestamp();

            this.EntityName = "p36LockPeriod";
            AFDATE("p36DateFrom", "Od").DefaultColumnFlag = gdc1;
            AFDATE("p36DateUntil", "Do").DefaultColumnFlag = gdc1;
            AFBOOL("p36IsAllSheets", "Všechny sešity").DefaultColumnFlag = gdc2;
            AFBOOL("p36IsAllPersons", "Všichni uživatelé").DefaultColumnFlag = gdc2;
            AppendTimestamp();

            this.EntityName = "p53VatRate";
            AA("p53Value", "Sazba DPH", gdc1, null, "num", false, true);
            oc = AF("Hladina", "Hladina sazby", "x15x.x15Name"); oc.RelSqlInCol = "LEFT OUTER JOIN x15VatRateType x15x ON a.x15ID=x15x.x15ID"; oc.DefaultColumnFlag = gdc1;
            oc = AF("Mena", "Měna", "j27x.j27Code"); oc.RelSqlInCol = "LEFT OUTER JOIN j27Currency j27x ON a.j27ID=j27x.j27ID"; oc.DefaultColumnFlag = gdc1; oc.FixedWidth = 100;
            AFDATE("p53ValidFrom", "Platí od").DefaultColumnFlag = gdc1;
            AFDATE("p53ValidUntil", "Platí do").DefaultColumnFlag = gdc1;
            oc = AA("p53CountryCode", "ISO kód státu"); oc.FixedWidth = 100; oc.DefaultColumnFlag = gdc1;
            AppendTimestamp();

            this.EntityName = "p83UpominkaType";
            AA("p83Name", "Typ upomínky", gdc1, null, "string", false, true);            
            AppendTimestamp();

            this.EntityName = "p92InvoiceType";
            AA("p92Name", "Typ faktury", BO.TheGridDefColFlag.GridAndCombo, null, "string", false, true);
            AA("p92ReportConstantPreText1", "Preambule hlavního textu faktury");
            AA("p92InvoiceDefaultText1", "Výchozí hlavní text faktury");
            AA("p92ReportConstantText", "Preambule technického textu");
            AA("p92InvoiceDefaultText2", "Výchozí technický text");
            AA("p92RepDocName", "Název na sestavě faktury");

            AFNUM0("p92Ordinary", "#").DefaultColumnFlag = gdc2;
            oc = AF("TypUpominek","Typ upomínek", "p83x.p83Name"); oc.RelSqlInCol = "LEFT OUTER JOIN p83UpominkaType p83x ON a.p83ID=p83x.p83ID";

            oc = AF("Predkontace", "Předkontace [par]", "p92AccountingIds.o59Value"); oc.NotShowRelInHeader = true; oc.RelSqlInCol = "LEFT OUTER JOIN (select o58.o58Name,o59.o59Value,o59.o59RecordPid from o59GlobalParamBinding o59 INNER JOIN o58GlobalParam o58 ON o59.o58ID=o58.o58ID AND o58.o58Key='p92AccountingIds') p92AccountingIds ON a.p92ID=p92AccountingIds.o59RecordPid";
            oc = AF("KlasifikaceDPH", "Klasifikace DPH [par]", "p92ClassificationVatIds.o59Value"); oc.NotShowRelInHeader = true; oc.RelSqlInCol = "LEFT OUTER JOIN (select o58.o58Name,o59.o59Value,o59.o59RecordPid from o59GlobalParamBinding o59 INNER JOIN o58GlobalParam o58 ON o59.o58ID=o58.o58ID AND o58.o58Key='p92ClassificationVatIds') p92ClassificationVatIds ON a.p92ID=p92ClassificationVatIds.o59RecordPid";
            oc = AF("RadaDokladuHelios", "Řada dokladů Helios DPH [par]", "p92RadaDokladuHelios.o59Value"); oc.NotShowRelInHeader = true; oc.RelSqlInCol = "LEFT OUTER JOIN (select o58.o58Name,o59.o59Value,o59.o59RecordPid from o59GlobalParamBinding o59 INNER JOIN o58GlobalParam o58 ON o59.o58ID=o58.o58ID AND o58.o58Key='p92RadaDokladuHelios') p92RadaDokladuHelios ON a.p92ID=p92RadaDokladuHelios.o59RecordPid";

            oc = AF("WorkflowSablona", "Workflow šablona", "b01x.b01Name"); oc.RelSqlInCol = "LEFT OUTER JOIN b01WorkflowTemplate b01x ON a.b01ID=b01x.b01ID";

            AppendTimestamp();

            this.EntityName = "p98Invoice_Round_Setting_Template";
            AA("p98Name", "Zaokrouhlovací pravidlo", gdc1, null, "string", false, true);
            AFBOOL("p98IsDefault", "Výchozí pravidlo").DefaultColumnFlag = gdc2;
            AppendTimestamp();


            this.EntityName = "p63Overhead";
            AA("p63Name", "Režijní přirážka", gdc1, null, "string", false, true);
            AppendTimestamp();

            this.EntityName = "p80InvoiceAmountStructure";
            AA("p80Name", "Název rozpisu", gdc1, null, "string", false, true);
            AFBOOL("p80IsTimeSeparate", "Čas 1:1").DefaultColumnFlag = gdc2;
            AFBOOL("p80IsExpenseSeparate", "Výdaje 1:1").DefaultColumnFlag = gdc2;
            AFBOOL("p80IsFeeSeparate", "Pevné odměny 1:1").DefaultColumnFlag = gdc2;
            AppendTimestamp();

            this.EntityName = "p93InvoiceHeader";
            AA("p93Name", "Vystavovatel faktury", gdc1, null, "string", false, true);
            AA("p93Company", "Firma", gdc2);
            AA("p93RegID", "IČ", gdc2);
            AA("p93VatID", "DIČ", gdc2);
            AA("p93City", "Město");
            AA("p93Street", "Ulice");
            AA("p93Zip", "PSČ");
            AppendTimestamp();

            this.EntityName = "p89ProformaType";
            AA("p89Name", "Typ zálohy", gdc1, null, "string", false, true);
            AppendTimestamp();

            this.EntityName = "p86BankAccount";
            AA("p86Name", "Bankovní účet", gdc2, null, "string", false, true);
            AA("p86Account", "Číslo účtu", gdc1);
            AA("p86Code", "Kód banky", gdc1);
            AA("p93Names", "Vazba na vystavovatele faktur", gdc2, "dbo.p86_get_p93names(a.p86ID)");
            AA("p86BankName", "Banka");
            AA("p86SWIFT", "SWIFT");
            AA("p86IBAN", "IBAN");
            AA("p86BankAddress", "Adresa banky");
            AppendTimestamp();

            this.EntityName = "p70BillingStatus";
            AA("p70Name", "Fakturační status", gdc1, null, "string", false, true);
            this.EntityName = "p72PreBillingStatus";
            AA("p72Name", "Fakturační status schvalovatele", gdc1, null, "string", false, true);

            this.EntityName = "p95InvoiceRow";
            oc = AA("p95Name", "Fakturační oddíl", gdc1, null, "string", false, true); oc.SqlExplicitGroupBy = "a.p95ID";
            AA("p95Code", "Kód");
            AFNUM0("p95Ordinary", "#").DefaultColumnFlag = gdc2;
            AA("p95Name_BillingLang1", "Název €1");
            AA("p95Name_BillingLang2", "Název €2");
            AA("p95Name_BillingLang3", "Název €3");
            AA("p95Name_BillingLang4", "Název €4");
            oc = AF("Predkontace", "Předkontace [par]", "p95AccountingIds.o59Value"); oc.NotShowRelInHeader = true; oc.RelSqlInCol = "LEFT OUTER JOIN (select o58.o58Name,o59.o59Value,o59.o59RecordPid from o59GlobalParamBinding o59 INNER JOIN o58GlobalParam o58 ON o59.o58ID=o58.o58ID AND o58.o58Key='p95AccountingIds') p95AccountingIds ON a.p95ID=p95AccountingIds.o59RecordPid";
            oc = AF("Cinnost", "Činnost [par]", "p95ActivityIds.o59Value"); oc.NotShowRelInHeader = true; oc.RelSqlInCol = "LEFT OUTER JOIN (select o58.o58Name,o59.o59Value,o59.o59RecordPid from o59GlobalParamBinding o59 INNER JOIN o58GlobalParam o58 ON o59.o58ID=o58.o58ID AND o58.o58Key='p95ActivityIds') p95ActivityIds ON a.p95ID=p95ActivityIds.o59RecordPid";

            AppendTimestamp();

            this.EntityName = "m62ExchangeRate";
            AFDATE("m62Date", "Datum kurzu").DefaultColumnFlag = gdc1;
            AA("m62Rate", "Kurz", gdc1, null, "num3");
            AA("Veta", "", gdc2, "CONVERT(varchar(10),a.m62Units)+' '+(select j27Code from j27Currency where j27ID=a.j27ID_Slave)+' = '+CONVERT(varchar(10),a.m62Rate)+' '+(select j27Code FROM j27Currency where j27ID=a.j27ID_Master)");
            AA("m62RateType", "Typ kurzu", gdc2, "case when a.m62RateType=1 then 'Fakturační kurz' else 'Fixní kurz' end");
            AppendTimestamp();

            this.EntityName = "p35Unit";
            AA("p35Name", "Kusovníková jednotka", gdc1, null, "string", false, true);
            AA("p35Code", "Kód", gdc1);
            AppendTimestamp();

            //j40 = poštovní účty
            this.EntityName = "j40MailAccount";
            AA("j40SmtpEmail", "Adresa odesílatele").DefaultColumnFlag = gdc1;
            AA("j40SmtpName", "Jméno odesílatele", gdc1);
            AA("Usage", "Typ účtu", gdc1, "case a.j40UsageFlag when 1 then 'SMTP privátní' when 2 then 'SMTP globální/sdílený' when 3 then 'IMAP privátní' when 4 then 'IMAP globální/sdílený' end");
            AA("j40SmtpHost", "SMTP server");

            AA("j40ImapHost", "IMAP server");
            AA("j40ImapLogin", "IMAP Login", gdc2);
            AA("j40Name", "Název/Popis");
            oc = AF("Vlastnik", "Vlastník účtu", "j40_j02x.j02Name"); oc.RelSqlInCol = "LEFT OUTER JOIN j02User j40_j02x ON a.j02ID=j40_j02x.j02ID"; oc.DefaultColumnFlag = gdc1;
            AppendTimestamp();

            //j40 = IMAP pravidla
            this.EntityName = "o42ImapRule";
            AA("o42Name", "Název pravidla", gdc1);
            AA("j40Name", "IMAP účet", gdc1, "j40x.j40Name");
            AA("WhatToDo", "Cíl pravidla", gdc2, "case a.o42WhatToDoFlag when 1 then 'Vytvořit dokument' when 2 then 'Připojit k projektu' when 3 then 'Připojit k dokumentu' when 4 then 'Připojit ke kontaktu' when 5 then 'Připojit k uživateli' end");
            AA("o42Description", "Poznámka");
            AppendTimestamp();

            this.EntityName = "r02CapacityVersion";
            AA("r02Name", "Verze plánu", gdc1, null, "string", false, true);
            AFNUM0("r02Ordinary", "#").DefaultColumnFlag = gdc2;
            AppendTimestamp();

            //o53 = štítky
            this.EntityName = "o53TagGroup";
            oc = AA("o53Name", "Skupina štítku", gdc1, null, "string", false, true); oc.SqlExplicitGroupBy = "a.o53ID";
            AFNUM0("o53Ordinary", "#").DefaultColumnFlag = gdc2;
            AppendTimestamp();

            //o51 = Položky štítku
            this.EntityName = "o51Tag";
            AA("o51Name", "Položka štítku", gdc1, null, "string", false, true);
            AFBOOL("o51IsColor", "Je barva?");
            AA("o51BackColor", "Barva pozadí", gdc2, "'<div style=\"background-color:'+a.o51BackColor+';\">...</div>'").FixedWidth = 110;
            AA("o51ForeColor", "Barva písma", gdc2, "'<div style=\"background-color:'+a.o51ForeColor+';\">...</div>'").FixedWidth = 110;
            AFNUM0("o51Ordinary", "#").DefaultColumnFlag = gdc2;
            AppendTimestamp();

            //o18 = typy agend
            this.EntityName = "o18DocType";
            AA("o18Name", "Název", gdc1, null, "string", false, true);
            AFNUM0("o18Ordinary", "#").DefaultColumnFlag = gdc2;
            oc = AF("Menu", "Menu dokumentů", "o18_o17.o17Name"); oc.RelSqlInCol = "LEFT OUTER JOIN o17DocMenu o18_o17 ON a.o17ID=o18_o17.o17ID";
            AFBOOL("o18IsAllowEncryption", "Zaheslovat Notepad?");
            AFBOOL("o18IsAllowTree", "Strom?");
            AFBOOL("o18IsColors", "Barva?");
            AppendTimestamp();

            //o17 = menu agend
            this.EntityName = "o17DocMenu";
            AA("o17Name", "Název", gdc1, null, "string", false, true);
            AFNUM0("o17Ordinary", "#").DefaultColumnFlag = gdc2;
            AppendTimestamp();

            //x40 = OUTBOX     
            this.EntityName = "x40MailQueue";
            AA("MessageTime", "Čas", gdc1, "case when a.x40DatetimeProcessed is not null then a.x40DatetimeProcessed else a.x40DateInsert end", "datetime", false, true);
            AA("x40SenderName", "Odesílatel", gdc1);
            AA("x40SenderAddress", "Odesílatel (adresa)");
            AA("x40Recipient", "Komu", gdc1);
            AA("x40CC", "Cc");
            AA("x40BCC", "Bcc");
            AA("x40Status", "Stav", gdc1, "case a.x40Status when 1 then 'Čeká na odeslání' when 2 then 'Chyba' when 3 then 'Odesláno' when 4 then 'Zastaveno' when 5 then 'Čeká na schválení' end");
            AA("x40Subject", "Předmět zprávy", gdc1);
            //AF("x40MailQueue", "x40Body", "Text zprávy", BO.TheGridDefColFlag.GridAndCombo, "convert(varchar(150),a.x40Body)+'...'");
            AA("x40Attachments", "Přílohy", gdc1);
            AA("x40ErrorMessage", "Chyba", gdc1);
            AA("Velikost", "Velikost", gdc1, "a.x40EmlFileSize", "filesize");

            //o24 = Reminder
            this.EntityName = "o24Reminder";
            AA("o24DatetimeProcessed", "Čas odeslání", gdc1, null, "datetime");
            oc = AA("o24RecordEntity", "Entita", gdc1); oc.FixedWidth = 100;
            AA("o24RecordPid", "ID záznamu entity", gdc1, null, "num0");
            oc = AA("o24Unit", "Jednotka", gdc1); oc.FixedWidth = 100;
            AA("o24Count", "Počet jednotek", gdc1, null, "num0");

            this.EntityName = "j06UserHistory";
            oc = AF("Pozice", "Pozice", "j06_j07.j07Name"); oc.RelSqlInCol = "LEFT OUTER JOIN j07PersonPosition j06_j07 ON a.j07ID=j06_j07.j07ID"; oc.DefaultColumnFlag = gdc1;
            oc = AF("Fond", "Časový fond", "j06_c21.c21Name"); oc.RelSqlInCol = "LEFT OUTER JOIN c21FondCalendar j06_c21 ON a.c21ID=j06_c21.c21ID"; oc.DefaultColumnFlag = gdc1;
            AA("j06ValidFrom", "Platnost od", gdc1, null, "date");
            AA("j06ValidUntil", "Platnost do", gdc1, null, "date");

            //konfigurace html editoru
            this.EntityName = "x04NotepadConfig";
            AA("x04Name", "Název", gdc1, null, "string", false, true);
            AppendTimestamp();

            this.EntityName = "x07Integration";
            AA("x07Flag", "Typ", gdc1, "case a.x07Flag when 1 then 'Ecomail' when 2 then 'SmartEmailing' when 3 then 'AbraFlexi' end", "string", false, true);
            AA("x07Name", "Název", gdc1, null, "string", false, true);
            AppendTimestamp();

            //x31 = tisková sestava      
            this.EntityName = "x31Report";
            AA("x31Name", "Tisková sestava", gdc1, null, "string", false, true);
            AA("RepFormat", "Formát", gdc0, "case a.x31FormatFlag when 1 then 'REPORT' when 2 then 'DOCX' when 3 then 'PLUGIN' when 4 then 'XLS' end");
            oc = AA("Kontext", "Kontext", gdc0, "dbo.get_entity_alias(a.x31Entity)"); oc.FixedWidth = 140;
            AA("x31Code", "Kód sestavy");
            oc = AF("Kategorie", "Kategorie", "j25x.j25Name"); oc.RelSqlInCol = "LEFT OUTER JOIN j25ReportCategory j25x ON a.j25ID=j25x.j25ID"; oc.DefaultColumnFlag = gdc1;
            AFBOOL("x31IsPeriodRequired", "Filtr čaového období");
            AFBOOL("x31IsAllowPfx", "Podpora Pfx");

            oc = AA("Soubor", "Soubor šablony", gdc2, "o27x.o27ArchiveFileName"); oc.RelSqlInCol = "LEFT OUTER JOIN o27Attachment o27x ON a.x31ID=o27x.o27RecordPid AND o27x.o27Entity='x31' AND o27x.o27RecordPid IS NOT NULL";


            AA("x31ExportFileNameMask", "Maska export souboru");
            AA("x31IsScheduling", "Pravidelné odesílání");
            AFNUM0("x31Ordinary", "#").DefaultColumnFlag = gdc2;
            AA("x31Description", "Poznámka");
            AppendTimestamp();

            //uživatelská pole
            this.EntityName = "x28EntityField";
            AA("x28Name", "Uživatelské pole", gdc1, null, "string", false, true);
            AA("Entita", "Entita", gdc1, "dbo.get_entity_alias(a.x28Entity)");
            AA("x28Field", "Fyzický název", gdc2);
            AFBOOL("x28IsRequired", "Povinné").DefaultColumnFlag = gdc1;
            AFNUM0("x28Ordinary", "#").DefaultColumnFlag = gdc1;
            AppendTimestamp();

            //skupina uživatelských polí
            this.EntityName = "x27EntityFieldGroup";
            AA("x27Name", "Skupina polí", gdc1, null, "string", false, true);
            AFNUM0("x27Ordinary", "#").DefaultColumnFlag = gdc2;

            //číselné řady
            this.EntityName = "x38CodeLogic";
            AA("x38Name", "Číselná řada", gdc1, null, "string", false, true);
            AA("x38ConstantBeforeValue", "Konstanta před", gdc2);
            AA("x38ConstantAfterValue", "Konstanta za", gdc2);
            AFNUM0("x38Scale", "Rozsah nul").DefaultColumnFlag = gdc2;
            AA("Maska", "Min-Max", 0, "case when a.x38MaskSyntax IS NULL then ISNULL(a.x38ConstantBeforeValue,'')+RIGHT('000000001',a.x38Scale)+' - '+ISNULL(a.x38ConstantBeforeValue,'')+RIGHT('99999999999',a.x38Scale) else a.x38MaskSyntax end");
            AppendTimestamp();

            //x52 = novinky
            this.EntityName = "x52Blog";
            AA("x52Name", "Název", gdc1, null, "string", false, true);
            AA("x52Date", "Datum", gdc1, "a.x52Date", "date");
            AA("x52Symbol", "Symbol");
            AA("x52Ordinary", "#", gdc1);

            //x51 = nápověda
            this.EntityName = "x51HelpCore";
            AA("x51Name", "Nápověda", gdc1, null, "string", false, true);
            AA("x51ViewUrl", "View Url", gdc2);
            AA("x51Ordinary", "#", gdc1);
            AA("x51TreeIndex", "Strom index", gdc1);
            oc = AA("Strom", "Strom", gdc1, "case when a.x51ParentID is not null then a.x51TreePath end"); oc.FixedWidth = 400;
            AA("x51NearUrls", "Near urls");
            AppendTimestamp();

            //x55 = dashboard widget
            this.EntityName = "x55Widget";
            AA("x55Name", "Widget", gdc1, null, "string", false, true);
            AA("x55Category", "Kategorie", gdc1, null, "string", false, true);
            AA("x55Code", "Kód", gdc1, null, "string", false, true);
            AA("x55DataTablesLimit", "Minimum záznamů pro [DataTables]", gdc2);
            AFNUM0("x55Ordinal", "#").DefaultColumnFlag = gdc2;
            AppendTimestamp();

            //x54 =  widget skupiny
            this.EntityName = "x54WidgetGroup";
            AA("x54Name", "Widget", gdc1, null, "string", false, true);
            AFNUM0("x54Ordinary", "#").DefaultColumnFlag = gdc2;
            AppendTimestamp();

            this.EntityName = "x67EntityRole";
            AA("x67Name", "Název role", gdc1, null, "string", false, true);
            AFNUM0("x67Ordinary", "#").DefaultColumnFlag = gdc2;
            AppendTimestamp();


            //x97 = překlad
            this.EntityName = "x97Translate";
            AA("x97Code", "Originál", gdc1, null, "string", false, true);
            AA("x97Lang1", "English", gdc1);
            AA("x97Lang2", "Deutsch", gdc1);
            AA("x97Lang4", "Slovenčina", gdc1);
            AA("x97OrigSource", "Zdroj");
            AppendTimestamp(false);

            //doručená pošta - INBOX
            this.EntityName = "o43Inbox";
            AA("o43DateReceived", "Datum", gdc1, "a.o43DateReceived", "datetime");
            AA("o43Subject", "Předmět", gdc1);

            oc = AA("Sender", "Odesílatel", gdc1, "case when a.o43SenderName is not null then a.o43SenderName else a.o43SenderAddress end"); oc.SqlExplicitGroupBy = "case when a.o43SenderName is not null then a.o43SenderName else a.o43SenderAddress end";
            oc = AA("o43SenderName", "Od (Jméno)"); oc.SqlExplicitGroupBy = "a.o43SenderName";
            oc = AA("o43SenderAddress", "Od (E-mail)"); oc.SqlExplicitGroupBy = "a.o43SenderAddress";

            AA("o43To", "Komu");
            AA("o43Cc", "Cc");
            AA("o43Bcc", "Bcc");

            AA("Velikost", "Velikost", gdc1, "a.o43Length", "filesize").DefaultColumnFlag = gdc2;

            AA("o43ImapFolder", "Imap složka", gdc1);
            AA("Inserted", "Naimportováno", gdc1, "a.o43DateInsert", "datetime");

            AA("o43Attachments", "Seznam příloh");
            AFNUM0("o43AttachmentsCount", "Počet příloh");
            AA("MaPrilohy", "Má přílohy?", gdc0, "convert(bit,case when a.AttachmentsCount>0 then 1 else 0 end)", "bool");
            oc = AFBOOL("o43IsSeen", "Přečteno"); oc.SqlExplicitGroupBy = "a.o43IsSeen";
            oc = AFBOOL("o43IsDraft", "Draft"); oc.SqlExplicitGroupBy = "a.o43IsDraft";
            oc = AFBOOL("o43IsFlagged", "Flagged"); oc.SqlExplicitGroupBy = "a.o43IsFlagged";
            AFBOOL("o43IsDeleted", "Deleted");

            AF("MaVazbu", "Má vazbu", "convert(bit,case when a.p41ID IS NOT NULL OR a.o23ID is not null OR a.p56ID IS NOT NULL OR a.b05ID IS NOT NULL OR a.o22ID IS NOT NULL then 1 else 0 end)", "bool");
            AF("DruhVazby", "Druh vazby", "case when a.p41ID IS NOT NULL then 'Projekt' when a.o23ID is not null then 'Dokument' when a.b05ID IS NOT NULL then 'Notepad' when a.p56ID IS NOT NULL then 'Úkol' when a.o22ID IS NOT NULL then 'Událost/Termín' end");



            this.EntityName = "view_28_kontaktni_osoby";
            AF("p28FirstName", "Jméno");
            AF("p28LastName", "Příjmení");
            AF("p28Name", "Název");
            oc = AF("p28Street1", "Ulice #1");
            AF("p28City1", "Město #1");
            oc = AF("p28PostCode1", "PSČ #1");
            oc = AF("p28Country1", "Stát #1");
            AF("Stitky", "Štítky");

            this.EntityName = "com_vykazano";
            oc = AF("PosledniUkon_Kdy", "Poslední úkon", "a.PosledniUkon_Kdy", "date"); oc.NotShowRelInHeader = true;
            oc = AF("PrvniUkon_Kdy", "První úkon", "a.PrvniUkon_Kdy", "date"); oc.NotShowRelInHeader = true;
            oc = AF("Hodiny", "Hodiny", "a.Hodiny", "num", true); oc.NotShowRelInHeader = true; oc.IsHours = true;
            oc = AF("Hodiny_Fa", "Hodiny Fa", "a.Hodiny_Fa", "num", true); oc.NotShowRelInHeader = true; oc.IsHours = true;
            oc = AF("Hodiny_NeFa", "Hodiny Nefa", "a.Hodiny_NeFa", "num", true); oc.NotShowRelInHeader = true; oc.IsHours = true;
            oc = AF("Hodiny_Vyloucene", "Hodiny vyloučené z vyúčt.", "a.Hodiny_Vyloucene", "num", true); oc.NotShowRelInHeader = true; oc.IsHours = true;
            oc = AF("BezDph", "Bez dph", "a.BezDph", "num", true); oc.IHRC = true; oc.NotShowRelInHeader = true;
            oc = AF("Honorar_Fakturacni", "Honorář", "a.Honorar_Fakturacni", "num", true); oc.IHRC = true;
            oc = AF("Honorar_Fakturacni_CZK", "Honorář CZK", "a.Honorar_Fakturacni_CZK", "num", true); oc.IHRC = true; oc.NotShowRelInHeader = true;
            oc = AF("Honorar_Fakturacni_EUR", "Honorář EUR", "a.Honorar_Fakturacni_EUR", "num", true); oc.IHRC = true; oc.NotShowRelInHeader = true;
            oc = AF("Honorar_Nakladovy", "Nákladový honorář", "a.Honorar_Nakladovy", "num", true); oc.IHRC = true; oc.NotShowRelInHeader = true;
            oc = AF("Vydaje", "Výdaje", "a.Vydaje", "num", true); oc.IHRC = true; oc.NotShowRelInHeader = true;
            oc = AF("Vydaje_CZK", "Výdaje CZK", "a.Vydaje_CZK", "num", true); oc.IHRC = true; oc.NotShowRelInHeader = true;
            oc = AF("Vydaje_EUR", "Výdaje EUR", "a.Vydaje_EUR", "num", true); oc.IHRC = true; oc.NotShowRelInHeader = true;
            oc = AF("Pausaly", "Pevné odměny", "a.Pausaly", "num", true); oc.IHRC = true; oc.NotShowRelInHeader = true;
            oc = AF("Pausaly_CZK", "Pevné odměny CZK", "a.Pausaly_CZK", "num", true); oc.IHRC = true; oc.NotShowRelInHeader = true;
            oc = AF("Pausaly_EUR", "Pevné odměny EUR", "a.Pausaly_EUR", "num", true); oc.IHRC = true; oc.NotShowRelInHeader = true;


            //this.EntityName = "com_rozpracovano";
            //AF("PosledniUkon_Kdy", "Naposledy kdy", "a.PosledniUkon_Kdy", "date");
            //oc = AF("Hodiny", "Rozpracované hodiny", "a.Hodiny", "num",true); oc.NotShowRelInHeader = true;
            //oc = AF("Hodiny_Fa", "Rozpracované Fa hodiny", "a.Hodiny_Fa", "num", true); oc.NotShowRelInHeader = true;
            //oc = AF("BezDph", "Rozpracováno bez dph", "a.BezDph", "num", true); oc.IHRC = true; oc.NotShowRelInHeader = true;
            //oc = AF("Honorar_Fakturacni", "Honorář", "a.Honorar_Fakturacni", "num", true); oc.IHRC = true;
            //oc = AF("Honorar_Fakturacni_CZK", "Honorář CZK", "a.Honorar_Fakturacni_CZK", "num", true); oc.IHRC = true;
            //oc = AF("Honorar_Fakturacni_EUR", "Honorář EUR", "a.Honorar_Fakturacni_EUR", "num", true); oc.IHRC = true;
            //oc = AF("Honorar_Nakladovy", "Nákladový honorář", "a.Honorar_Nakladovy", "num", true); oc.IHRC = true;
            //oc = AF("Vydaje", "Rozpracované výdaje", "a.Vydaje", "num", true); oc.IHRC = true; oc.NotShowRelInHeader = true;
            //oc = AF("Vydaje_CZK", "Výdaje CZK", "a.Vydaje_CZK", "num", true); oc.IHRC = true;
            //oc = AF("Vydaje_EUR", "Výdaje EUR", "a.Vydaje_EUR", "num", true); oc.IHRC = true;
            //oc = AF("Pausaly", "Rozpracované pevné odměny", "a.Pausaly", "num", true); oc.IHRC = true; oc.NotShowRelInHeader = true;
            //oc = AF("Pausaly_CZK", "Pevné odměny CZK", "a.Pausaly_CZK", "num", true); oc.IHRC = true;
            //oc = AF("Pausaly_EUR", "Pevné odměny EUR", "a.Pausaly_EUR", "num", true); oc.IHRC = true;


            this.EntityName = "com_nevyuctovano";
            this.CurrentFieldGroup = "Nevyúčtováno";
            oc = AF("PosledniUkon_Kdy", "Poslední nevyúčt.úkon", "a.PosledniUkon_Kdy", "date"); oc.NotShowRelInHeader = true;
            oc = AF("PrvniUkon_Kdy", "První nevyúčt.úkon", "a.PrvniUkon_Kdy", "date"); oc.NotShowRelInHeader = true;
            oc = AF("Hodiny", "Nevyúčtované hodiny", "a.Hodiny", "num", true); oc.NotShowRelInHeader = true; oc.IsHours = true;
            oc = AF("Hodiny_Fa", "Nevyúčtované Fa hodiny", "a.Hodiny_Fa", "num", true); oc.NotShowRelInHeader = true; oc.IsHours = true;
            oc = AF("BezDph", "Nevyúčtováno bez dph", "a.BezDph", "num", true); oc.IHRC = true; oc.NotShowRelInHeader = true;
            oc = AF("Honorar", "Nevyúčtovaný honorář", "a.Honorar", "num", true); oc.IHRC = true; oc.NotShowRelInHeader = true;
            oc = AF("Honorar_CZK", "Nevyúčt.honorář CZK", "a.Honorar_CZK", "num", true); oc.IHRC = true; oc.NotShowRelInHeader = true;
            oc = AF("Honorar_EUR", "Nevyúčt.honorář EUR", "a.Honorar_EUR", "num", true); oc.IHRC = true; oc.NotShowRelInHeader = true;
            oc = AF("Vydaje", "Nevyúčtované výdaje", "a.Vydaje", "num", true); oc.IHRC = true; oc.NotShowRelInHeader = true;
            oc = AF("Vydaje_CZK", "Nevyúčt.výdaje CZK", "a.Vydaje_CZK", "num", true); oc.IHRC = true; oc.NotShowRelInHeader = true;
            oc = AF("Vydaje_EUR", "Nevyúčt.výdaje EUR", "a.Vydaje_EUR", "num", true); oc.IHRC = true; oc.NotShowRelInHeader = true;
            oc = AF("Pausaly", "Nevyúčt.odměny", "a.Pausaly", "num", true); oc.IHRC = true; oc.NotShowRelInHeader = true;
            oc = AF("Pausaly_CZK", "Nevyúčt.odměny CZK", "a.Pausaly_CZK", "num", true); oc.IHRC = true; oc.NotShowRelInHeader = true;
            oc = AF("Pausaly_EUR", "Nevyúčt.odměny EUR", "a.Pausaly_EUR", "num", true); oc.IHRC = true; oc.NotShowRelInHeader = true;
            this.CurrentFieldGroup = "Rozpracováno";
            oc = AF("PosledniUkon_Kdy_Rozpr", "Poslední rozpr.úkon", "a.PosledniUkon_Kdy_Rozpr", "date"); oc.NotShowRelInHeader = true;
            oc = AF("PrvniUkon_Kdy_Rozpr", "První rozpr.úkon", "a.PrvniUkon_Kdy_Rozpr", "date"); oc.NotShowRelInHeader = true;
            oc = AF("Hodiny_Rozpr", "Rozpracované hodiny", "a.Hodiny_Rozpr", "num", true); oc.NotShowRelInHeader = true; oc.IsHours = true;
            oc = AF("Hodiny_Fa_Rozpr", "Rozpr.Fa hodiny", "a.Hodiny_Fa_Rozpr", "num", true); oc.NotShowRelInHeader = true; oc.IsHours = true;
            oc = AF("Hodiny_NeFa_Rozpr", "Rozpr.Nefa hodiny", "a.Hodiny_NeFa_Rozpr", "num", true); oc.NotShowRelInHeader = true; oc.IsHours = true;
            oc = AF("BezDph_Rozpr", "Rozpr. bez dph", "a.BezDph_Rozpr", "num", true); oc.IHRC = true; oc.NotShowRelInHeader = true;
            oc = AF("Honorar_Fakturacni_Rozpr", "Rozpr.Honorář", "a.Honorar_Fakturacni_Rozpr", "num", true); oc.IHRC = true; oc.NotShowRelInHeader = true;
            oc = AF("Honorar_Fakturacni_CZK_Rozpr", "Rozpr.Honorář CZK", "a.Honorar_Fakturacni_CZK_Rozpr", "num", true); oc.IHRC = true; oc.NotShowRelInHeader = true;
            oc = AF("Honorar_Fakturacni_EUR_Rozpr", "Rozpr.Honorář EUR", "a.Honorar_Fakturacni_EUR_Rozpr", "num", true); oc.IHRC = true; oc.NotShowRelInHeader = true;
            oc = AF("Honorar_Nakladovy_Rozpr", "Rozpr.Nákladový honorář", "a.Honorar_Nakladovy_Rozpr", "num", true); oc.IHRC = true; oc.NotShowRelInHeader = true;
            oc = AF("Vydaje_Rozpr", "Rozpracované výdaje", "a.Vydaje_Rozpr", "num", true); oc.IHRC = true; oc.NotShowRelInHeader = true;
            oc = AF("Vydaje_CZK_Rozpr", "Rozpr.Výdaje CZK", "a.Vydaje_CZK_Rozpr", "num", true); oc.IHRC = true; oc.NotShowRelInHeader = true;
            oc = AF("Vydaje_EUR_Rozpr", "Rozpr.Výdaje EUR", "a.Vydaje_EUR_Rozpr", "num", true); oc.IHRC = true; oc.NotShowRelInHeader = true;
            oc = AF("Pausaly_Rozpr", "Rozpr.Pevné odměny", "a.Pausaly_Rozpr", "num", true); oc.IHRC = true; oc.NotShowRelInHeader = true;
            oc = AF("Pausaly_CZK_Rozpr", "Rozpr.Pevné odměny CZK", "a.Pausaly_CZK_Rozpr", "num", true); oc.IHRC = true; oc.NotShowRelInHeader = true;
            oc = AF("Pausaly_EUR_Rozpr", "Rozpr.Pevné odměny EUR", "a.Pausaly_EUR_Rozpr", "num", true); oc.IHRC = true; oc.NotShowRelInHeader = true;
            this.CurrentFieldGroup = "Schváleno";
            oc = AF("Hodiny_Schval", "Schvál.hodiny", "a.Hodiny_Schval", "num", true); oc.NotShowRelInHeader = true;oc.IsHours = true;
            oc = AF("Hodiny_Schval_FAK", "Schvál.hodiny FAK", "a.Hodiny_Schval_Fak", "num", true); oc.NotShowRelInHeader = true; oc.IsHours = true;
            oc = AF("Hodiny_Schval_PAU", "Schvál.hodiny PAU", "a.Hodiny_Schval_Pau", "num", true); oc.NotShowRelInHeader = true; oc.IsHours = true;
            oc = AF("Hodiny_Schval_ODP", "Schvál.hodiny ODP", "a.Hodiny_Schval_Odp", "num", true); oc.NotShowRelInHeader = true; oc.IsHours = true;
            oc = AF("Honorar_Schval_CZK", "Schvál.honorář CZK", "a.Honorar_Schval_CZK", "num", true); oc.NotShowRelInHeader = true;
            oc = AF("Honorar_Schval_EUR", "Schvál.honorář EUR", "a.Honorar_Schval_EUR", "num", true); oc.NotShowRelInHeader = true;
            oc = AF("Vydaje_Schval", "Schvál.výdaje", "a.Vydaje_Schval", "num", true); oc.IHRC = true; oc.NotShowRelInHeader = true;
            oc = AF("Vydaje_Schval_CZK", "Schvál.výdaje CZK", "a.Vydaje_Schval_CZK", "num", true); oc.IHRC = true; oc.NotShowRelInHeader = true;
            oc = AF("Vydaje_Schval_EUR", "Schvál.výdaje EUR", "a.Vydaje_Schval_EUR", "num", true); oc.IHRC = true; oc.NotShowRelInHeader = true;
            oc = AF("Pausaly_Schval", "Schvál.odměny", "a.Pausaly_Schval", "num", true); oc.IHRC = true; oc.NotShowRelInHeader = true;
            oc = AF("Pausaly_Schval_CZK", "Schvál.odměny CZK", "a.Pausaly_Schval_CZK", "num", true); oc.IHRC = true; oc.NotShowRelInHeader = true;
            oc = AF("Pausaly_Schval_EUR", "Schvál.odměny EUR", "a.Pausaly_Schval_EUR", "num", true); oc.IHRC = true; oc.NotShowRelInHeader = true;
            oc = AF("BezDph_Schval", "Schváleno bez dph", "a.BezDph_Schval", "num", true); oc.IHRC = true; oc.NotShowRelInHeader = true;

            this.EntityName = "com_vyuctovano";
            this.CurrentFieldGroup = null;
            oc = AF("PosledniFaktura_Kdy", "Poslední faktura kdy", "a.PosledniFaktura_Kdy", "date"); oc.NotShowRelInHeader = true;
            AF("PosledniUkon_Kdy", "Poslední úkon kdy", "a.PosledniUkon_Kdy", "date");
            AF("PrvniUkon_Kdy", "První úkon kdy", "a.PrvniUkon_Kdy", "date");
            oc = AF("Hodiny", "Vyúčtované hodiny", "a.Hodiny", "num", true); oc.NotShowRelInHeader = true; oc.IsHours = true;
            oc=AF("Hodiny_Odpis", "Hodiny ODP", "a.Hodiny_Odpis", "num", true); oc.IsHours = true;
            oc=AF("Hodiny_Pausal", "Hodiny PAU", "a.Hodiny_Pausal", "num", true); oc.IsHours = true;
            oc = AF("BezDph", "Vyúčtováno bez dph", "a.BezDph", "num", true); oc.IHRC = true; oc.NotShowRelInHeader = true;
            oc = AF("Honorar", "Vyúčtovaný honorář", "a.Honorar", "num", true); oc.IHRC = true; oc.NotShowRelInHeader = true;
            oc = AF("Honorar_Fakturacni_CZK", "Honorář CZK", "a.Honorar_Fakturacni_CZK", "num", true); oc.IHRC = true;
            oc = AF("Honorar_Fakturacni_EUR", "Honorář EUR", "a.Honorar_Fakturacni_EUR", "num", true); oc.IHRC = true;
            oc = AF("Vydaje", "Vyúčtované výdaje", "a.Vydaje", "num", true); oc.IHRC = true; oc.NotShowRelInHeader = true;
            oc = AF("Vydaje_CZK", "Výdaje CZK", "a.Vydaje_CZK", "num", true); oc.IHRC = true;
            oc = AF("Vydaje_EUR", "Výdaje EUR", "a.Vydaje_EUR", "num", true); oc.IHRC = true;
            oc = AF("Pausaly", "Vyúčtované pevné odměny", "a.Pausaly", "num"); oc.IHRC = true; oc.NotShowRelInHeader = true;
            oc = AF("Pausaly_CZK", "Pevné odměny CZK", "a.Pausaly_CZK", "num", true); oc.IHRC = true;
            oc = AF("Pausaly_EUR", "Pevné odměny EUR", "a.Pausaly_EUR", "num", true); oc.IHRC = true;

            oc = AF("Vyfakturovano_PAU_Obrat", "PAU obrat podíl", "a.Vyfakturovano_PAU_Obrat", "num",true); oc.IHRC = true;
            oc = AF("Prumerna_Sazba_Celkem", "Prům.sazba vč.PAU", "case when ISNULL(a.Hodiny_Vykazane,0)<>0 then (ISNULL(a.Vyfakturovano_PAU_Obrat,0)+ISNULL(a.Honorar,0))/ISNULL(a.Hodiny_Vykazane,0) end", "num", false); oc.IHRC = true;oc.Tooltip = "(Vyúčtovaný honorář + Podíl na PAU obratu) / Celkově vykázané hodiny";
            oc = AF("Prumerna_Sazba_Bez_Pau", "Prům.sazba bez PAU", "case when ISNULL(a.Hodiny_Vykazane,0)<>0 then ISNULL(a.Honorar,0)/ISNULL(a.Hodiny_Vykazane,0) end", "num", false); oc.IHRC = true;oc.Tooltip = "Vyúčtovaný honorář / Celkově vykázané hodiny";
            oc = AF("Prumerna_Sazba_Vyuctovana", "Prům.sazba FAK", "case when ISNULL(a.Hodiny,0)<>0 then ISNULL(a.Honorar,0)/ISNULL(a.Hodiny,0) end", "num", false); oc.IHRC = true; oc.Tooltip = "Vyúčtovaný honorář / Vyúčtované hodiny sazbou";
            oc = AF("Prumerna_Sazba_Pau", "Prům.sazba PAU", "case when ISNULL(a.Hodiny_Pausal,0)<>0 then ISNULL(a.Vyfakturovano_PAU_Obrat,0)/ISNULL(a.Hodiny_Pausal,0) end", "num", false); oc.IHRC = true; oc.Tooltip = "Podíl na PAU obratu / Hodiny vyúčtované PAU";



        }




        private BO.TheGridColumn AA(string strField, string strHeader, BO.TheGridDefColFlag dcf = BO.TheGridDefColFlag._none, string strSqlSyntax = null, string strFieldType = "string", bool bolIsShowTotals = false, bool bolNotShowRelInHeader = false)
        {
            oc = AF(strField, strHeader, strSqlSyntax, strFieldType, bolIsShowTotals);
            oc.DefaultColumnFlag = dcf;
            oc.NotShowRelInHeader = bolNotShowRelInHeader;

            return oc;
        }
    }
}
