
namespace BL.Singleton
{
    public class TheEntitiesProvider
    {
        private readonly Singleton.RunningApp _app;
        private readonly Singleton.TheTranslator _tt;
        private List<BO.TheEntity> _lis;
        public TheEntitiesProvider(Singleton.RunningApp runningapp, Singleton.TheTranslator tt)
        {
            _app = runningapp;
            _tt = tt;

            SetupPallete();



        }

        public BO.TheEntity ByPrefix(string strPrefix)
        {
            return _lis.Where(p => p.Prefix == strPrefix).First();
        }
        public BO.TheEntity ByTable(string strTableName)
        {
            return _lis.Where(p => p.TableName == strTableName).First();
        }



        private void SetupPallete()
        {
            _lis = new List<BO.TheEntity>();
            if (_app.HostingMode == HostingModeEnum.TotalCloud)
            {
                AE("j02User", "Uživatelé", "Uživatel", "j02User a INNER JOIN j04UserRole j04x ON a.j04ID=j04x.j04ID INNER JOIN x67EntityRole x67x ON j04x.x67ID=x67x.x67ID", "a.j02Name");
            }
            else
            {
                AE("j02User", "Uživatelé", "Uživatel", "j02User a INNER JOIN j04UserRole j04x ON a.j04ID=j04x.j04ID", "a.j02Name");
            }
            

            AE("j04UserRole", "Aplikační role", "Aplikační role", "j04UserRole a INNER JOIN x67EntityRole x67x ON a.x67ID=x67x.x67ID", "a.j04Name");
            if (_app.HostingMode == HostingModeEnum.TotalCloud)
            {
                AE("j05MasterSlave", "Nadřízený/Podřízený", "Nadřízení/Podřízení", "j05MasterSlave a INNER JOIN j02User j02master ON a.j02ID_Master=j02master.j02ID INNER JOIN j04UserRole j04x ON j02master.j04ID=j04x.j04ID INNER JOIN x67EntityRole x67x ON j04x.x67ID=x67x.x67ID LEFT OUTER JOIN j02User j02slave ON a.j02ID_Slave=j02slave.j02ID LEFT OUTER JOIN j11Team j11slave ON a.j11ID_Slave=j11slave.j11ID", "a.j05ID DESC");
                
            }
            else
            {
                AE("j05MasterSlave", "Nadřízený/Podřízený", "Nadřízení/Podřízení", "j05MasterSlave a INNER JOIN j02User j02master ON a.j02ID_Master=j02master.j02ID LEFT OUTER JOIN j02User j02slave ON a.j02ID_Slave=j02slave.j02ID LEFT OUTER JOIN j11Team j11slave ON a.j11ID_Slave=j11slave.j11ID", "a.j05ID DESC");
                
            }
                
            AE("j11Team", "Pracovní týmy", "Pracovní tým", "j11Team a ", "a.j11Name");
            AE("j07PersonPosition", "Pozice", "Pozice", "j07PersonPosition a ", "a.j07Ordinary");
            AE("j18CostUnit", "Střediska", "Středisko", "j18CostUnit a ", "a.j18Ordinary,a.j18Name");
            AE("j19PaymentType", "Forma úhrady", "Forma úhrady", "j19PaymentType a ", "a.j19Ordinary");


            AE("j27Currency", "Měny", "Měna", "j27Currency a ", "a.j27Ordinary,a.j27Code");
            
            AE_TINY("j90LoginAccessLog", "LOGIN Log", "LOGIN Log");
            ByPrefix("j90").IsWithoutValidity = true;

            if (_app.HostingMode == HostingModeEnum.TotalCloud)
            {
                
                AE("j92PingLog", "PING Log","PING Log","j92PingLog a INNER JOIN j02User j02x ON a.j02ID=j02x.j02ID INNER JOIN j04UserRole j04x ON j02x.j04ID=j04x.j04ID INNER JOIN x67EntityRole x67x ON j04x.x67ID=x67x.x67ID","a.j92ID DESC");
                ByPrefix("j92").IsWithoutValidity = true;
            }
            else
            {
                
                AE_TINY("j92PingLog", "PING Log", "PING Log");
                ByPrefix("j92").IsWithoutValidity = true;
            }
            
            AE_TINY("j95GeoWeatherLog", "Historie počasí", "Historie počasí");
            ByPrefix("j95").IsWithoutValidity = true;

            AE("x40MailQueue", "OUTBOX", "OUTBOX", "x40MailQueue a", "a.x40ID DESC", "a.x40ID DESC");
            ByPrefix("x40").IsWithoutValidity = true;

            AE("o24Reminder", "Reminder fronta", "Reminder fronta", "o24Reminder a", "a.o24ID DESC", "a.o24ID DESC");
            ByPrefix("o24").IsWithoutValidity = true;

            AE("j06UserHistory", "Historie uživatelů", "Historie uživatele", "j06UserHistory a INNER JOIN j02User j02x ON a.j02ID=j02x.j02ID INNER JOIN j04UserRole j04x ON j02x.j04ID=j04x.j04ID INNER JOIN x67EntityRole x67x ON j04x.x67ID=x67x.x67ID", "a.j06ID DESC", "a.j06ID DESC");
            ByPrefix("j06").IsWithoutValidity = true;



            AE("b20Hlidac", "Hlídači", "Hlídač", "b20Hlidac a", "a.b20Ordinary");
            AE("b01WorkflowTemplate", "Workflow šablony", "Workflow šablona", "b01WorkflowTemplate a", "a.b01Name");
            AE("b02WorkflowStatus", "Workflow stavy", "Workflow stav", "b02WorkflowStatus a", "a.b01ID,a.b02Order,a.b02Name", "a.b01ID,a.b02Order,a.b02Name");
            if (_app.HostingMode == HostingModeEnum.TotalCloud)
            {
                AE("b05Workflow_History", "Poznámky/Historie", "Poznámky/Historie", "b05Workflow_History a INNER JOIN j02User j02x ON a.j02ID_Sys=j02x.j02ID INNER JOIN j04UserRole j04x ON j02x.j04ID=j04x.j04ID INNER JOIN x67EntityRole x67x ON j04x.x67ID=x67x.x67ID AND a.b05IsManualStep=1 LEFT OUTER JOIN b06WorkflowStep b06x ON a.b06ID=b06x.b06ID LEFT OUTER JOIN b02WorkflowStatus b02to ON a.b02ID_To=b02to.b02ID LEFT OUTER JOIN b02WorkflowStatus b02from ON a.b02ID_From=b02from.b02ID", "a.b05ID DESC");
                AE("p55TodoList", "Todo-list", "Todo-list", "p55TodoList a INNER JOIN j02User j02x ON a.j02ID_Owner=j02x.j02ID INNER JOIN j04UserRole j04x ON j02x.j04ID=j04x.j04ID INNER JOIN x67EntityRole x67x ON j04x.x67ID=x67x.x67ID", "a.p55ID DESC");
            }
            else
            {
                AE("b05Workflow_History", "Poznámky/Historie", "Poznámky/Historie", "b05Workflow_History a INNER JOIN j02User j02x ON a.j02ID_Sys=j02x.j02ID AND a.b05IsManualStep=1 LEFT OUTER JOIN b06WorkflowStep b06x ON a.b06ID=b06x.b06ID LEFT OUTER JOIN b02WorkflowStatus b02to ON a.b02ID_To=b02to.b02ID LEFT OUTER JOIN b02WorkflowStatus b02from ON a.b02ID_From=b02from.b02ID", "a.b05ID DESC");                
                AE("p55TodoList", "Todo-list", "Todo-list", "p55TodoList a", "a.p55ID DESC");
            }
            
            ByPrefix("b05").IsWithoutValidity = true;
            //AE("b06WorkflowStep", "Workflow kroky", "Workflow krok", "b06WorkflowStep a", "a.b06Order", "a.b06Order");
            //AE("b65WorkflowMessage", "Šablony notifikačních zpráv", "Workflow notifikační zpráva", "b65WorkflowMessage a", "a.b65Name");

            AE("o15AutoComplete", "AutoComplete položky", "AutoComplete položka", "o15AutoComplete a", "a.o15Flag");


            AE("p34ActivityGroup", "Sešity aktivit", "Sešit", "p34ActivityGroup a", "a.p34Ordinary,a.p34Name");
            AE("p32Activity", "Aktivity", "Aktivita", "p32Activity a INNER JOIN p34ActivityGroup p34x ON a.p34ID=p34x.p34ID", "a.p32Ordinary,a.p32Name");
            AE("p35Unit", "Kusovníkové jednotky", "Jednotka kusovníku", "p35Unit a", "a.p35Name");
            AE("p54OvertimeLevel", "Hladiny sazeb", "Hladina sazby", "p54OvertimeLevel a", "a.p34Ordinary");
            AE("p36LockPeriod", "Uzamčená období", "Uzamčené období", "p36LockPeriod a", "a.p36DateFrom");
            AE("p38ActivityTag", "Odvětví aktivit", "Odvětví aktivity", "p38ActivityTag a", "a.p38Ordinary");
            AE("p53VatRate", "DPH sazby", "DPH sazba", "p53VatRate a", "a.p53ID DESC");
            AE("p61ActivityCluster", "Klastry aktivit", "Klastr aktivit", "p61ActivityCluster a", "a.p61Name");
            AE("p63Overhead", "Režijní přirážka k fakturaci", "Režijní přirážka", "p63Overhead a", "a.p63Name");
            AE("p56Task", "Úkoly", "Úkol", "p56Task a INNER JOIN p57TaskType p57x ON a.p57ID=p57x.p57ID LEFT OUTER JOIN b02WorkflowStatus b02x ON a.b02ID=b02x.b02ID LEFT OUTER JOIN p41Project p41x ON a.p41ID=p41x.p41ID", "a.p56ID DESC");
            AE("p57TaskType", "Typy úkolů", "Typ úkolu", "p57TaskType a", "a.p57Ordinary");
            
            
            AE("p58TaskRecurrence", "Opakované úkoly", "Opakovaný úkol", "p58TaskRecurrence a INNER JOIN p57TaskType p57x ON a.p57ID=p57x.p57ID", "a.p58ID DESC");
            AE("p60TaskTemplate", "Šablony úkolů", "Šablona úkolu", "p60TaskTemplate a INNER JOIN p57TaskType p57x ON a.p57ID=p57x.p57ID", "a.p60ID DESC");
            AE("o22Milestone", "Termíny/Lhůty", "Termín/Událost", "o22Milestone a INNER JOIN o21MilestoneType o21x ON a.o21ID=o21x.o21ID", "a.o21ID DESC");
            AE("o21MilestoneType", "Typy termínů", "Typ termínu", "o21MilestoneType a", "a.o21Ordinary");

            AE("p75InvoiceRecurrence", "Opakovaná vyúčtování", "Opakované vyúčtování", "p75InvoiceRecurrence a","a.p75ID DESC");

            AE("p95InvoiceRow", "Fakturační oddíly", "Fakturační oddíl", "p95InvoiceRow a", "a.p95Ordinary");
            AE("p70BillingStatus", "Fakturační status", "Fakturační status", "p70BillingStatus a", "a.p70ID");
            AE("p72PreBillingStatus", "Fakturační status schvalovatelem", "Fakturační status schvalovatelem", "p72PreBillingStatus a", "a.p72ID");

            AE("p39WorkSheet_Recurrence_Plan", "Plán generování úkonů", "Plán generování úkonů", "p39WorkSheet_Recurrence_Plan a LEFT OUTER JOIN p31Worksheet p31 ON a.p31ID_NewInstance=p31.p31ID", "a.p39Date");
            ByPrefix("p39").IsWithoutValidity = true;
            AE("p40WorkSheet_Recurrence", "Opakované odměny/paušály", "Opakovaná odměna", "p40WorkSheet_Recurrence a INNER JOIN p34ActivityGroup p34x ON a.p34ID=p34x.p34ID INNER JOIN p41Project p41x ON a.p41ID=p41x.p41ID LEFT OUTER JOIN j27Currency j27x ON a.j27ID=j27x.j27ID LEFT OUTER JOIN dbo.view_p40_nevygenerovano p39miss ON a.p40ID=p39miss.p40ID", "a.p40Name");

            AE("view_p40_cerpano", "Vyčerpáno z paušálu","Vyčerpáno z paušálu","dbo.view_p40_cerpano a","a.p40ID");
            //AE("view_p40_nevygenerovano", "Chybí vygenerovat", "Chybí vygenerovat", "dbo.view_p40_nevygenerovano a", "a.p40ID");
            AE("view_p41_fakturacni_poznamka", "Fakturační poznámka", "Fakturační poznámka", "dbo.view_p41_fakturacni_poznamka a", "a.p41ID");
            AE("view_p28_fakturacni_poznamka", "Fakturační poznámka", "Fakturační poznámka", "dbo.view_p28_fakturacni_poznamka a", "a.p28ID");

            AE("p49FinancialPlan", "Finanční plány", "Finanční plán", "p49FinancialPlan a INNER JOIN p34ActivityGroup p34x ON a.p34ID=p34x.p34ID INNER JOIN j27Currency j27x ON a.j27ID=j27x.j27ID","a.p49ID DESC");
            for(int i = 1; i <= 3; i++)
            {
                AE($"fp{i}", "FP statistiky", "FP statistiky", $"dbo.fp{i}(@x01id,@d1,@d2) a", $"a.fp{i}ID");
                ByPrefix($"fp{i}").IsWithoutValidity = true;
                AE($"fp{i}_p31", "Realizováno", "Realizováno", "????", $"a.fp{i}ID");
            }
            AE("fp2_r01", "Kapacitní plán", "Kapacitní plán", "????", "a.fp2ID");



            if (_app.HostingMode == HostingModeEnum.TotalCloud)
            {
                AE("p11Attendance", "Záznamy docházky", "Docházka", "p11Attendance a INNER JOIN j02User j02x ON a.j02ID=j02x.j02ID INNER JOIN j04UserRole j04x ON j02x.j04ID=j04x.j04ID INNER JOIN x67EntityRole x67x ON j04x.x67ID=x67x.x67ID", "a.p11ID");
            }
            else
            {
                AE("p11Attendance", "Záznamy docházky", "Docházka", "p11Attendance a INNER JOIN j02User j02x ON a.j02ID=j02x.j02ID", "a.p11ID");
            }
            ByPrefix("p11").IsWithoutValidity = true;
            AE("com_dochazka", "Vykázáno", "Vykázáno", "?????", "a.j02ID");

            AE("p41Project", "Projekty", "Projekt", "p41Project a INNER JOIN p42ProjectType p42x ON a.p42ID=p42x.p42ID INNER JOIN p07ProjectLevel p07x ON a.p07ID=p07x.p07ID LEFT OUTER JOIN b02WorkflowStatus b02x ON a.b02ID=b02x.b02ID", "a.p41ID DESC");
            AE("le1", "L1", "L1", "p41Project a INNER JOIN p42ProjectType p42x ON a.p42ID=p42x.p42ID INNER JOIN p07ProjectLevel p07x ON a.p07ID=p07x.p07ID AND p07x.p07Level=1 LEFT OUTER JOIN b02WorkflowStatus b02x ON a.b02ID=b02x.b02ID", "a.p41ID DESC", "a.p41ID DESC");
            AE("le2", "L2", "L2", "p41Project a INNER JOIN p42ProjectType p42x ON a.p42ID=p42x.p42ID INNER JOIN p07ProjectLevel p07x ON a.p07ID=p07x.p07ID AND p07x.p07Level=2 LEFT OUTER JOIN b02WorkflowStatus b02x ON a.b02ID=b02x.b02ID", "a.p41ID DESC", "a.p41ID DESC");
            AE("le3", "L3", "L3", "p41Project a INNER JOIN p42ProjectType p42x ON a.p42ID=p42x.p42ID INNER JOIN p07ProjectLevel p07x ON a.p07ID=p07x.p07ID AND p07x.p07Level=3 LEFT OUTER JOIN b02WorkflowStatus b02x ON a.b02ID=b02x.b02ID", "a.p41ID DESC", "a.p41ID DESC");
            AE("le4", "L4", "L4", "p41Project a INNER JOIN p42ProjectType p42x ON a.p42ID=p42x.p42ID INNER JOIN p07ProjectLevel p07x ON a.p07ID=p07x.p07ID AND p07x.p07Level=4 LEFT OUTER JOIN b02WorkflowStatus b02x ON a.b02ID=b02x.b02ID", "a.p41ID DESC", "a.p41ID DESC");
            AE("le5", "L5", "L5", "p41Project a INNER JOIN p42ProjectType p42x ON a.p42ID=p42x.p42ID INNER JOIN p07ProjectLevel p07x ON a.p07ID=p07x.p07ID AND p07x.p07Level=5 LEFT OUTER JOIN b02WorkflowStatus b02x ON a.b02ID=b02x.b02ID", "a.p41ID DESC", "a.p41ID DESC");

            AE("c21FondCalendar", "Pracovní fondy", "Pracovní fond", "c21FondCalendar a", "a.c21Ordinary");
            AE("c26Holiday", "Dny svátků", "Svátek", "c26Holiday a", "a.c26Date");
            AE("c24DayColor", "Barvy dnů", "Barva dne", "c24DayColor a", "a.c24Name");

            AE("p31Worksheet", "Úkony", "Úkon", "p31Worksheet a INNER JOIN p41Project p41x ON a.p41ID=p41x.p41ID INNER JOIN p32Activity p32x ON a.p32ID=p32x.p32ID INNER JOIN p34ActivityGroup p34x ON p32x.p34ID=p34x.p34ID LEFT OUTER JOIN p91Invoice p91x ON a.p91ID=p91x.p91ID", "a.p31ID DESC");
            AE("o23Doc", "Dokumenty", "Dokument", "o23Doc a INNER JOIN o18DocType o18x ON a.o18ID=o18x.o18ID LEFT OUTER JOIN b02WorkflowStatus b02x ON a.b02ID=b02x.b02ID", "a.o23ID DESC");

            AE("p28Contact", "Kontakty", "Kontakt", "p28Contact a INNER JOIN p29ContactType p29x ON a.p29ID=p29x.p29ID", "a.p28Name");
            AE("p29ContactType", "Typy kontaktu", "Typ kontaktu", "p29ContactType a", "a.p29Ordinary,a.p29Name");
            AE("p24ContactGroup", "Skupiny kontaktů", "Skupina kontaktů", "p24ContactGroup a ", "a.p24Name");

            AE("p07ProjectLevel", "Úrovně projektů", "Úroveň projektu", "p07ProjectLevel a", "a.p07Level DESC", "a.p07Level DESC");
            AE("p42ProjectType", "Typy projektů", "Typ projektu", "p42ProjectType a", "a.p42Ordinary", "a.p42Ordinary");
            AE("p51PriceList", "Ceníky sazeb", "Ceník sazeb", "p51PriceList a", "a.p51Ordinary");

            AE("p44ProjectTemplate", "Šablony projektů", "Projektová šablona", "p44ProjectTemplate a LEFT OUTER JOIN p41Project p41x ON a.p41ID_Pattern=p41x.p41ID LEFT OUTER JOIN p28Contact p28x ON p41x.p28ID_Client=p28x.p28ID", "a.p44Ordinary");

            AE("x67EntityRole", "Role", "Role", "x67EntityRole a", "a.x67Ordinary,a.x67Name");
            AE("x04NotepadConfig", "Notepad konfigurace", "Notepad konfigurace", "x04NotepadConfig a", "a.x04Name");
            AE("x07Integration", "Integrace", "Integrace", "x07Integration a", "a.x07Ordinary");

            AE("j61TextTemplate", "Šablony poštovních zpráv", "Šablona poštovní zprávy", "j61TextTemplate a ", "a.j61Ordinary");
            AE("p15Location", "Pojmenované lokality", "Pojmenovaná lokalita", "p15Location a ", "a.p15Name");

            AE("m62ExchangeRate", "Měnové kurzy", "Měnový kurz", "m62ExchangeRate a", "a.m62ID DESC");


            AE("o53TagGroup", "Štítky", "Štítek", "o53TagGroup a", "a.o53Ordinary");
            AE_TINY("o54TagBindingInline", "Štítky", "Štítky");
            AE("o51Tag", "Položky štítků", "Položka štítku", "o51Tag a INNER JOIN o53TagGroup o53x ON a.o53ID=o53x.o53ID", "a.o51ID DESC");
            AE("j40MailAccount", "Poštovní účty", "Poštovní účet", "j40MailAccount a", "a.j40ID DESC");

            AE("o42ImapRule", "IMAP pravidla", "IMAP pravidlo", "o42ImapRule a INNER JOIN j40MailAccount j40x ON a.j40ID=j40x.j40ID", "a.o42ID DESC");


            AE("o18DocType", "Typy dokumentů", "Typ dokumentu", "o18DocType a", "a.o18Ordinary");
            AE("o17DocMenu", "Menu dokumentů", "Menu dokumentů", "o17DocMenu a", "a.o17Ordinary");

            AE_TINY("x28EntityField", "Uživatelská pole", "Uživatelské pole");
            AE_TINY("x27EntityFieldGroup", "Skupiny uživatelských polí", "Skupina uživatelských polí");

            AE("x31Report", "Report šablony", "Pevná tisková sestava","x31Report a","a.x31Ordinary,a.x31Name");
            AE_TINY("j25ReportCategory", "Kategorie sestav", "Kategorie sestavy");
            AE_TINY("x38CodeLogic", "Číselné řady", "Číselná řada");

            AE_TINY("x51HelpCore", "Uživatelská nápověda", "Uživatelská nápověda");
            AE_TINY("x52Blog", "Novinky | Blog", "Novinky | Blog");
            AE_TINY("x55Widget", "Widgety", "Widget");
            AE_TINY("x54WidgetGroup", "Skupiny widgetů", "Skupina widgetů");
            AE_TINY("x97Translate", "Aplikační překlad", "Aplikační překlad");
            ByPrefix("x97").IsWithoutValidity = true;

            AE_TINY("r02CapacityVersion", "Verze kapacitních plánů", "Typ kapacitního plánu");

            AE("p91Invoice", "Vyúčtování", "Vyúčtování", "p91Invoice a INNER JOIN p92InvoiceType p92x ON a.p92ID=p92x.p92ID LEFT OUTER JOIN b02WorkflowStatus b02x ON a.b02ID=b02x.b02ID", "a.p91ID DESC");
            AE("p90Proforma", "Zálohy", "Záloha", "p90Proforma a INNER JOIN p89ProformaType p89x ON a.p89ID=p89x.p89ID", "a.p90ID DESC");
            AE("p92InvoiceType", "Typy faktur", "Typ faktury", "p92InvoiceType a", "a.p92Ordinary");
            AE("p93InvoiceHeader", "Vystavovatelé faktur", "Vystavovatel faktury", "p93InvoiceHeader a", "a.p93Name");
            AE("p86BankAccount", "Bankovní účty", "Bankovní účet", "p86BankAccount a", "a.p86Name");
            AE("p98Invoice_Round_Setting_Template", "Zaokrouhlovací pravidla", "Zaokrouhlovací pravidlo", "p98Invoice_Round_Setting_Template a", "a.p98Name");
            AE("p80InvoiceAmountStructure", "Struktury rozpisu částky faktury", "Struktura rozpisu částky faktury", "p80InvoiceAmountStructure a", "a.p80Name");
            AE("p82Proforma_Payment", "Úhrady záloh", "Úhrada zálohy", "p82Proforma_Payment a", "a.p82ID DESC");
            AE("p89ProformaType", "Typy záloh", "Typ zálohy", "p89ProformaType a", "a.p89Name");
            AE("p83UpominkaType", "Typy upomínek", "Typ upomínky", "p83UpominkaType a", "a.p83Ordinary,a.p83Name");

            AE("o43Inbox", "Doručená pošta", "Doručená pošta", "o43Inbox a", "a.o43ID DESC");

            AE("p84Upominka", "Upomínky", "Upomínka", "p84Upominka a INNER JOIN p91Invoice p91x ON a.p91ID=p91x.p91ID INNER JOIN p83UpominkaType p83x ON a.p83ID=p83x.p83ID", "a.p84ID DESC");

            AE("p28_vykazano", "Vykázáno", "Vykázáno", "?????", "a.p28ID");
            AE("p28_rozpracovano", "Rozpracováno", "Rozpracováno", "?????", "a.p28ID");
            AE("p28_nevyuctovano", "Nevyúčtováno", "Nevyúčtováno", "?????", "a.p28ID");
            AE("p28_vyuctovano", "Vyúčtováno", "Vyúčtováno", "?????", "a.p28ID");

            AE("j02_vykazano", "Vykázáno", "Vykázáno", "?????", "a.j02ID");
            AE("j02_rozpracovano", "Rozpracováno", "Rozpracováno", "?????", "a.j02ID");
            AE("j02_nevyuctovano", "Nevyúčtováno", "Nevyúčtováno", "?????", "a.j02ID");
            AE("j02_vyuctovano", "Vyúčtováno", "Vyúčtováno", "?????", "a.j02ID");

            AE("com_vykazano", "Vykázáno", "Vykázáno", "?????", "a.p56ID");
            //AE("com_rozpracovano", "Rozpracováno", "Rozpracováno", "?????", "a.p56ID");
            AE("com_nevyuctovano", "Nevyúčtováno", "Nevyúčtováno", "?????", "a.p56ID");
            AE("com_vyuctovano", "Vyúčtováno", "Vyúčtováno", "?????", "a.p56ID");

            
            AE("p91_vyuctovano", "Vyúčtováno", "Vyúčtováno", "?????", "a.p91ID");
            AE("p91_vykazano", "Vykázáno", "Vykázáno", "?????", "a.p91ID");

            //if (this._app.HostingMode != BL.Singleton.HostingModeEnum.TotalCloud)
            //{
                
            //}
            AE("view_28_kontaktni_osoby", "Kontaktní osoby", "Kontaktní osoby", "?????", "a.p28ID");

        }

        private void AE(string strTabName, string strPlural, string strSingular, string strSqlFromGrid, string strSqlOrderByCombo, string strSqlOrderBy = null)
        {
            if (strSqlOrderBy == null) strSqlOrderBy = "a." + strTabName.Substring(0, 3) + "ID DESC";
            BO.TheEntity c = new BO.TheEntity() { TableName = strTabName, AliasPlural = strPlural, AliasSingular = strSingular, SqlFromGrid = strSqlFromGrid, SqlOrderByCombo = strSqlOrderByCombo, SqlOrderBy = strSqlOrderBy };
            c.TranslateLang1 = _tt.DoTranslate(strPlural, 1, "TheEntitiesProvider:AE");
            c.TranslateLang2 = _tt.DoTranslate(strPlural, 2, "TheEntitiesProvider:AE");

            _lis.Add(c);

        }
        private void AE_TINY(string strTabName, string strPlural, string strSingular)
        {

            _lis.Add(new BO.TheEntity() { TableName = strTabName, AliasPlural = strPlural, AliasSingular = strSingular, SqlFromGrid = strTabName + " a", SqlOrderByCombo = "a." + strTabName.Substring(0, 3) + "Name", SqlOrderBy = "a." + strTabName.Substring(0, 3) + "ID DESC", TranslateLang1 = _tt.DoTranslate(strPlural, 1), TranslateLang2 = _tt.DoTranslate(strPlural, 2) });
        }
        private BO.EntityRelation getREL(string strTabName, string strRelName, string strSingular, string strSqlFrom, string strDependOnRel = null)
        {
            return new BO.EntityRelation() { TableName = strTabName, RelName = strRelName, AliasSingular = strSingular, SqlFrom = strSqlFrom, RelNameDependOn = strDependOnRel, Translate1 = _tt.DoTranslate(strSingular, 1), Translate2 = _tt.DoTranslate(strSingular, 2, "TheEntitiesProvider:getREL") };



        }

        public List<BO.EntityRelation> getApplicableRelations(string strPrimaryPrefix, BL.Factory f)
        {

            var lis = new List<BO.EntityRelation>();
            BO.TheEntity ce = ByPrefix(strPrimaryPrefix);

            switch (strPrimaryPrefix)
            {
                case "j02":
                    lis.Add(getREL("com_vykazano", "j02_vykazano", "Vykázáno", "LEFT OUTER JOIN [%j02_vykazano.sql%] j02_vykazano ON a.j02ID=j02_vykazano.j02ID"));
                    //lis.Add(getREL("com_rozpracovano", "j02_wip", "Rozpracováno", "LEFT OUTER JOIN ([%j02_rozpracovano.sql%]) j02_wip ON a.j02ID=j02_wip.j02ID"));
                    lis.Add(getREL("com_nevyuctovano", "j02_nevyuctovano", "Nevyúčtováno", "LEFT OUTER JOIN [%j02_nevyuctovano.sql%] j02_nevyuctovano ON a.j02ID=j02_nevyuctovano.j02ID"));
                    lis.Add(getREL("com_vyuctovano", "j02_invoice", "Vyúčtováno", "LEFT OUTER JOIN [%j02_vyuctovano.sql%] j02_invoice ON a.j02ID=j02_invoice.j02ID"));


                    break;

                case "p11":
                    lis.Add(getREL("com_dochazka", "p11_p31", "Hodiny v docházce", "LEFT OUTER JOIN [%p31_dochazka.sql%] p11_p31 ON a.j02ID=p11_p31.j02ID AND a.p11Date=p11_p31.p31Date"));
                    lis.Add(getREL("j02User", "p11_j02", "Uživatel", "INNER JOIN j02User p11_j02 ON a.j02ID=p11_j02.j02ID INNER JOIN j04UserRole j02_j04 ON p11_j02.j04ID=j02_j04.j04ID"));
                    
                    break;
                case "j90":
                    lis.Add(getREL("j02User", "j90_j02", "Uživatel", "INNER JOIN j02User j90_j02 ON a.j02ID=j90_j02.j02ID INNER JOIN j04UserRole j02_j04 ON j90_j02.j04ID=j02_j04.j04ID"));
                    break;
                case "j92":
                    lis.Add(getREL("j02User", "j92_j02", "Uživatel", "INNER JOIN j02User j92_j02 ON a.j02ID=j92_j02.j02ID INNER JOIN j04UserRole j02_j04 ON j92_j02.j04ID=j02_j04.j04ID"));
                    break;
                case "p32":
                    lis.Add(getREL("p34ActivityGroup", "p32_p34", "Sešit", "INNER JOIN p34ActivityGroup p32_p34 ON a.p34ID=p32_p34.p34ID"));
                    lis.Add(getREL("p95InvoiceRow", "p32_p95", "Fakturační oddíl", "LEFT OUTER JOIN p95InvoiceRow p32_p95 ON a.p95ID=p32_p95.p95ID"));
                    lis.Add(getREL("p38ActivityTag", "p32_p38", "Odvětví", "LEFT OUTER JOIN p38ActivityTag p32_p38 ON a.p38ID=p32_p38.p38ID"));
                    lis.Add(getREL("p35Unit", "p32_p35", "Jednotka kusovníku", "LEFT OUTER JOIN p35Unit p32_p35 ON a.p35ID=p32_p35.p35ID"));

                    break;
                case "p28":
                    lis.Add(getREL("p92InvoiceType", "p28_p92", "Typ faktury", "LEFT OUTER JOIN p92InvoiceType p28_p92 ON a.p92ID=p28_p92.p92ID"));
                    //lis.Add(getREL("p29ContactType", "p28_p29", "Typ kontaktu", "LEFT OUTER JOIN p29ContactType p28_p29 ON a.p29ID=p28_p29.p29ID"));
                    lis.Add(getREL("com_vykazano", "p28_vykazano", "Vykázáno", "LEFT OUTER JOIN [%p28_vykazano.sql%] p28_vykazano ON a.p28ID=p28_vykazano.p28ID"));
                    //lis.Add(getREL("com_rozpracovano", "p28_wip", "Rozpracováno", "LEFT OUTER JOIN ([%p28_rozpracovano.sql%]) p28_wip ON a.p28ID=p28_wip.p28ID"));                    
                    lis.Add(getREL("com_nevyuctovano", "p28_nevyuctovano", "Nevyúčtováno", "LEFT OUTER JOIN [%p28_nevyuctovano.sql%] p28_nevyuctovano ON a.p28ID=p28_nevyuctovano.p28ID"));
                    lis.Add(getREL("com_vyuctovano", "p28_invoice", "Vyúčtováno", "LEFT OUTER JOIN [%p28_vyuctovano.sql%] p28_invoice ON a.p28ID=p28_invoice.p28ID"));

                    lis.Add(getREL("view_28_kontaktni_osoby", "p28_ko", "Kontaktní osoba", "LEFT OUTER JOIN dbo.view_28_kontaktni_osoby p28_ko ON a.p28ID=p28_ko.p28ID"));
                    break;
                case "p31":
                    lis.Add(getREL("j02User", "p31_j02", "Uživatel", "LEFT OUTER JOIN j02User p31_j02 ON a.j02ID=p31_j02.j02ID"));

                    lis.Add(getREL("p32Activity", "p31_p32", "Aktivita", "INNER JOIN p32Activity p31_p32 ON a.p32ID=p31_p32.p32ID"));
                    lis.Add(getREL("p34ActivityGroup", "p32_p34", "Sešit", "INNER JOIN p32Activity aktivita1 ON a.p32ID=aktivita1.p32ID INNER JOIN p34ActivityGroup p32_p34 ON aktivita1.p34ID=p32_p34.p34ID"));
                    lis.Add(getREL("p41Project", "p31_p41", "Projekt", "LEFT OUTER JOIN p41Project p31_p41 ON a.p41ID=p31_p41.p41ID"));
                    lis.Add(getREL("p28Contact", "p31_p41_client", "Klient", "LEFT OUTER JOIN p28Contact p31_p41_client ON p31_p41.p28ID_Client=p31_p41_client.p28ID", "p31_p41"));
                    lis.Add(getREL("p91Invoice", "p31_p91", "Vyúčtování", "LEFT OUTER JOIN p91Invoice p31_p91 ON a.p91ID=p31_p91.p91ID"));
                    lis.Add(getREL("p56Task", "p31_p56", "Úkol", "LEFT OUTER JOIN p56Task p31_p56 ON a.p56ID=p31_p56.p56ID"));
                    break;
                
                case "le1":                
                    break;
                case "le2":
                    lis.Add(getREL("com_vykazano", "le2_vykazano", "Vykázáno", "LEFT OUTER JOIN [%le2_vykazano.sql%] le2_vykazano ON a.p41ID=le2_vykazano.p41ID"));
                    lis.Add(getREL("com_nevyuctovano", "le2_nevyuctovano", "Nevyúčtováno", "LEFT OUTER JOIN [%le2_nevyuctovano.sql%] le2_nevyuctovano ON a.p41ID=le2_nevyuctovano.p41ID"));
                    lis.Add(getREL("com_vyuctovano", "le2_vyuctovano", "Vyúčtováno", "LEFT OUTER JOIN [%le2_vyuctovano.sql%] le2_vyuctovano ON a.p41ID=le2_vyuctovano.p41ID"));
                    
                    break;
                case "le3":
                    lis.Add(getREL("com_vykazano", "le3_vykazano", "Vykázáno", "LEFT OUTER JOIN [%le3_vykazano.sql%] le3_vykazano ON a.p41ID=le3_vykazano.p41ID"));                    
                    lis.Add(getREL("com_nevyuctovano", "le3_nevyuctovano", "Nevyúčtováno", "LEFT OUTER JOIN [%le3_nevyuctovano.sql%] le3_nevyuctovano ON a.p41ID=le3_nevyuctovano.p41ID"));
                    lis.Add(getREL("com_vyuctovano", "le3_vyuctovano", "Vyúčtováno", "LEFT OUTER JOIN [%le3_vyuctovano.sql%] le3_vyuctovano ON a.p41ID=le3_vyuctovano.p41ID"));

                    break;
                case "le4":
                    lis.Add(getREL("p28Contact", "le4_p28client", "Klient", "LEFT OUTER JOIN p28Contact le4_p28client ON a.p28ID_Client=le4_p28client.p28ID"));

                    lis.Add(getREL("com_vykazano", "le4_vykazano", "Vykázáno", "LEFT OUTER JOIN [%le4_vykazano.sql%] le4_vykazano ON a.p41ID=le4_vykazano.p41ID"));
                    //lis.Add(getREL("com_rozpracovano", "le4_rozpracovano", "Rozpracováno", "LEFT OUTER JOIN ([%le4_rozpracovano.sql%]) le4_rozpracovano ON a.p41ID=le4_rozpracovano.p41ID"));
                    lis.Add(getREL("com_nevyuctovano", "le4_nevyuctovano", "Nevyúčtováno", "LEFT OUTER JOIN [%le4_nevyuctovano.sql%] le4_nevyuctovano ON a.p41ID=le4_nevyuctovano.p41ID"));
                    lis.Add(getREL("com_vyuctovano", "le4_vyuctovano", "Vyúčtováno", "LEFT OUTER JOIN [%le4_vyuctovano.sql%] le4_vyuctovano ON a.p41ID=le4_vyuctovano.p41ID"));

                    break;
                case "le5":
                    lis.Add(getREL("p28Contact", "p41_p28client", "Klient", "LEFT OUTER JOIN p28Contact p41_p28client ON a.p28ID_Client=p41_p28client.p28ID"));

                    lis.Add(getREL("p15Location", "p41_p15", "Lokalita", "LEFT OUTER JOIN p15Location p41_p15 ON a.p15ID=p41_p15.p15ID"));

                    lis.Add(getREL("com_vykazano", "le5_vykazano", "Vykázáno", "LEFT OUTER JOIN [%le5_vykazano.sql%] le5_vykazano ON a.p41ID=le5_vykazano.p41ID"));
                    //lis.Add(getREL("com_rozpracovano", "le5_rozpracovano", "Rozpracováno", "LEFT OUTER JOIN ([%le5_rozpracovano.sql%]) le5_rozpracovano ON a.p41ID=le5_rozpracovano.p41ID"));
                    lis.Add(getREL("com_nevyuctovano", "le5_nevyuctovano", "Nevyúčtováno", "LEFT OUTER JOIN [%le5_nevyuctovano.sql%] le5_nevyuctovano ON a.p41ID=le5_nevyuctovano.p41ID"));
                    lis.Add(getREL("com_vyuctovano", "le5_vyuctovano", "Vyúčtováno", "LEFT OUTER JOIN [%le5_vyuctovano.sql%] le5_vyuctovano ON a.p41ID=le5_vyuctovano.p41ID"));

                    break;
                case "p41":
                    lis.Add(getREL("p28Contact", "p41_p28client", "Klient", "LEFT OUTER JOIN p28Contact p41_p28client ON a.p28ID_Client=p41_p28client.p28ID"));

                    lis.Add(getREL("p15Location", "p41_p15", "Lokalita", "LEFT OUTER JOIN p15Location p41_p15 ON a.p15ID=p41_p15.p15ID"));

                    lis.Add(getREL("com_vykazano", "p41_vykazano", "Vykázáno", "LEFT OUTER JOIN [%p41_vykazano.sql%] p41_vykazano ON a.p41ID=p41_vykazano.p41ID"));                    
                    lis.Add(getREL("com_nevyuctovano", "p41_nevyuctovano", "Nevyúčtováno", "LEFT OUTER JOIN [%p41_nevyuctovano.sql%] p41_nevyuctovano ON a.p41ID=p41_nevyuctovano.p41ID"));
                    lis.Add(getREL("com_vyuctovano", "p41_vyuctovano", "Vyúčtováno", "LEFT OUTER JOIN [%p41_vyuctovano.sql%] p41_vyuctovano ON a.p41ID=p41_vyuctovano.p41ID"));

                    
                    break;
                case "p56":
                    lis.Add(getREL("p41Project", "p56_p41", "Projekt", "LEFT OUTER JOIN p41Project p56_p41 ON a.p41ID=p56_p41.p41ID"));

                    
                    lis.Add(getREL("p91Invoice", "p56_wrk_p91", "Vyúčtování", "LEFT OUTER JOIN b05Workflow_History b05_p91 ON a.p56ID=b05_p91.p56ID LEFT OUTER JOIN p91Invoice p56_wrk_p91 ON b05_p91.b05RecordPid=p56_wrk_p91.p91ID AND b05_p91.b05RecordEntity='p91'"));
                    lis.Add(getREL("p28Contact", "p56_wrk_p28", "Kontakt", "LEFT OUTER JOIN b05Workflow_History b05_p28 ON a.p56ID=b05_p28.p56ID LEFT OUTER JOIN p28Contact p56_wrk_p28 ON b05_p28.b05RecordPid=p56_wrk_p28.p28ID AND b05_p28.b05RecordEntity='p28'"));
                    lis.Add(getREL("p15Location", "p56_p15", "Lokalita", "LEFT OUTER JOIN p15Location p56_p15 ON a.p15ID=p56_p15.p15ID"));

                    lis.Add(getREL("com_vykazano", "p56_vykazano", "Vykázáno", "LEFT OUTER JOIN [%p56_vykazano.sql%] p56_vykazano ON a.p56ID=p56_vykazano.p56ID"));
                    //lis.Add(getREL("com_rozpracovano", "p56_rozpracovano", "Rozpracováno", "LEFT OUTER JOIN ([%p56_rozpracovano.sql%]) p56_rozpracovano ON a.p56ID=p56_rozpracovano.p56ID"));
                    lis.Add(getREL("com_nevyuctovano", "p56_nevyuctovano", "Nevyúčtováno", "LEFT OUTER JOIN [%p56_nevyuctovano.sql%] p56_nevyuctovano ON a.p56ID=p56_nevyuctovano.p56ID"));
                    lis.Add(getREL("com_vyuctovano", "p56_vyuctovano", "Vyúčtováno", "LEFT OUTER JOIN [%p56_vyuctovano.sql%] p56_vyuctovano ON a.p56ID=p56_vyuctovano.p56ID"));

                    lis.Add(getREL("com_fp", "p56_fp", "Finanční plán", "LEFT OUTER JOIN [%p56_fp.sql%] p56_fp ON a.p56ID=p56_fp.p56ID"));
                    break;
                case "p91":
                    lis.Add(getREL("p91_vyuctovano", "p91_vyuctovano", "Vyúčtováno", "LEFT OUTER JOIN [%p91_vyuctovano.sql%] p91_vyuctovano ON a.p91ID=p91_vyuctovano.p91ID"));
                    lis.Add(getREL("p91_vykazano", "p91_vykazano", "Vykázáno", "LEFT OUTER JOIN [%p91_vykazano.sql%] p91_vykazano ON a.p91ID=p91_vykazano.p91ID"));
                    lis.Add(getREL("p41Project", "p91_p41", "První projekt", "LEFT OUTER JOIN p41Project p91_p41 ON a.p41ID_First=p91_p41.p41ID"));
                    lis.Add(getREL("p84Upominka", "p91_p84", "Poslední upomínka", "LEFT OUTER JOIN p84Upominka p91_p84 ON a.p84ID_Last=p91_p84.p84ID"));
                    break;
                case "p58":
                    lis.Add(getREL("p41Project", "p58_p41", "Projekt", "LEFT OUTER JOIN p41Project p58_p41 ON a.p41ID=p58_p41.p41ID"));
                    lis.Add(getREL("p28Contact", "p58_p41_p28", "Klient projektu", "LEFT OUTER JOIN p28Contact p58_p41_p28 ON p58_p41.p28ID_Client=p58_p41_p28.p28ID", "p58_p41"));
                    break;
                case "p75":
                    lis.Add(getREL("p41Project", "p75_p41", "Projekt", "LEFT OUTER JOIN p41Project p75_p41 ON a.p41ID=p75_p41.p41ID"));
                    lis.Add(getREL("p28Contact", "p75_p28", "Klient", "LEFT OUTER JOIN p28Contact p75_p28 ON a.p28ID=p75_p28.p28ID"));
                    break;
                case "o22":
                    lis.Add(getREL("p41Project", "o22_p41", "Projekt", "LEFT OUTER JOIN p41Project o22_p41 ON a.p41ID=o22_p41.p41ID"));
                    lis.Add(getREL("o23Doc", "o22_o23", "Dokument", "LEFT OUTER JOIN o23Doc o22_o23 ON a.o23ID=o22_o23.o23ID"));
                    lis.Add(getREL("p28Contact", "o22_p28", "Kontakt", "LEFT OUTER JOIN p28Contact o22_p28 ON a.p28ID=o22_p28.p28ID"));
                    break;
                case "p90":
                    lis.Add(getREL("p28Contact", "p90_p28", "Klient", "LEFT OUTER JOIN p28Contact p90_p28 ON a.p28ID=p90_p28.p28ID"));
                    break;
                case "p15":
                    lis.Add(getREL("p41Project", "p15_p41", "Projekt", "LEFT OUTER JOIN p41Project p15_p41 ON a.p41ID=p15_p41.p41ID"));
                    //lis.Add(getREL("j95GeoWeatherLog", "p15_j95", "Historie počasí", "LEFT OUTER JOIN j95GeoWeatherLog p15_j95 ON a.p15ID=p15_j95.j95RecordPid AND p15_j95.j95RecordEntity='p15'"));
                    break;
                case "p40":
                    lis.Add(getREL("p32Activity", "p40_p32", "Aktivita", "LEFT OUTER JOIN p32Activity p40_p32 ON a.p32ID=p40_p32.p32ID"));
                    lis.Add(getREL("p41Project", "p40_p41", "Projekt", "LEFT OUTER JOIN p41Project p40_p41 ON a.p41ID=p40_p41.p41ID"));
                    lis.Add(getREL("p28Contact", "p40_p41_p28", "Klient projektu", "LEFT OUTER JOIN p28Contact p40_p41_p28 ON p40_p41.p28ID_Client=p40_p41_p28.p28ID", "p40_p41"));
                    //lis.Add(getREL("view_p40_nevygenerovano", "p40_nevygenerovano", "Chybí vygenerovat", "LEFT OUTER JOIN dbo.view_p40_nevygenerovano p40_nevygenerovano ON a.p40ID=p40_nevygenerovano.p40ID"));

                    lis.Add(getREL("view_p40_cerpano", "p40_cerpano", "Čerpáno z paušálu", "LEFT OUTER JOIN dbo.view_p40_cerpano p40_cerpano ON a.p40ID=p40_cerpano.p40ID"));
                    
                    lis.Add(getREL("j02User", "p40_j02", "Uživatel", "LEFT OUTER JOIN j02User p40_j02 ON a.j02ID=p40_j02.j02ID"));
                    break;

                case "p49":
                    lis.Add(getREL("p32Activity", "p49_p32", "Aktivita", "LEFT OUTER JOIN p32Activity p49_p32 ON a.p32ID=p49_p32.p32ID"));
                    lis.Add(getREL("p41Project", "p49_p41", "Projekt", "LEFT OUTER JOIN p41Project p49_p41 ON a.p41ID=p49_p41.p41ID"));
                    lis.Add(getREL("p28Contact", "p49_p41_p28", "Klient projektu", "LEFT OUTER JOIN p28Contact p49_p41_p28 ON p49_p41.p28ID_Client=p49_p41_p28.p28ID", "p49_p41"));                    
                    lis.Add(getREL("j02User", "p49_j02", "Uživatel", "LEFT OUTER JOIN j02User p49_j02 ON a.j02ID=p49_j02.j02ID"));
                    lis.Add(getREL("p56Task", "p49_p56", "Úkol", "LEFT OUTER JOIN p56Task p49_p56 ON a.p56ID=p49_p56.p56ID"));
                    break;
                                        
                case "p36":
                    lis.Add(getREL("j02User", "p36_j02", "Uživatel", "LEFT OUTER JOIN j02User p36_j02 ON a.j02ID=p36_j02.j02ID"));
                    lis.Add(getREL("j11Team", "p36_j11", "Tým", "LEFT OUTER JOIN j11Team p36_j11 ON a.j11ID=p36_j11.j11ID"));
                    break;

                case "o23":
                    //lis.Add(getREL("b02WorkflowStatus", "o23_b02", "Workflow stav", "LEFT OUTER JOIN b02WorkflowStatus o23_b02 ON a.b02ID=o23_b02.b02ID"));
                    lis.Add(getREL("p28Contact", "o23_p28", "Vazba s kontaktem", "LEFT OUTER JOIN p28Contact o23_p28 ON a.p28ID_First=o23_p28.p28ID"));
                    lis.Add(getREL("p41Project", "o23_p41", "Vazba s projektem", "LEFT OUTER JOIN p41Project o23_p41 ON a.p41ID_First=o23_p41.p41ID"));
                    lis.Add(getREL("j02User", "o23_j02", "Vazba s uživatelem", "LEFT OUTER JOIN j02User o23_j02 ON a.j02ID_First=o23_j02.j02ID"));
                    //lis.Add(getREL("p91Invoice", "o23_p91", "Vazba s vyúčtováním", "LEFT OUTER JOIN p91Invoice o23_p91 ON a.p91ID_First=o23_p91.p91ID"));
                    break;

                case "o51":
                    lis.Add(getREL("o53TagGroup", "o51_o53", "Štítek", "LEFT OUTER JOIN o53TagGroup o51_o53 ON a.o53ID=o51_o53.o53ID"));
                    break;
                case "p92":
                    lis.Add(getREL("p93InvoiceHeader", "p92_p93", "Vystavovatel", "LEFT OUTER JOIN p93InvoiceHeader p92_p93 ON a.p93ID=p92_p93.p93ID"));
                    lis.Add(getREL("x38CodeLogic", "p92_x38", "Číselná řada", "LEFT OUTER JOIN x38CodeLogic p92_x38 ON a.x38ID=p92_x38.x38ID"));
                    lis.Add(getREL("j27Currency", "p92_j27", "Výchozí měna", "LEFT OUTER JOIN j27Currency p92_j27 ON a.j27ID=p92_j27.j27ID"));
                    
                    break;

                case "o24":
                    lis.Add(getREL("x40MailQueue", "o24_x40", "Odeslaná pošta", "LEFT OUTER JOIN x40MailQueue o24_x40 ON a.o24ID=o24_x40.o24ID"));
                    break;
                case "p84":
                    lis.Add(getREL("p91Invoice", "p84_p91", "Vyúčtování", "LEFT OUTER JOIN p91Invoice p84_p91 ON a.p91ID=p84_p91.p91ID"));
                    break;
                case "j06":
                    lis.Add(getREL("j02User", "j06_j02", "Uživatel", "LEFT OUTER JOIN j02User j06_j02 ON a.j02ID=j06_j02.j02ID"));
                    break;
                case "fp1":
                    lis.Add(getREL("fp1_p31", "fp1_p31", "Realizováno", "LEFT OUTER JOIN dbo.fp1_p31(@x01id,@d1,@d2) fp1_p31 ON a.fp1ID=fp1_p31.p56ID"));
                    lis.Add(getREL("p56Task", "fp1_p56", "Úkol", "LEFT OUTER JOIN p56Task fp1_p56 ON a.fp1ID=fp1_p56.p56ID"));
                    lis.Add(getREL("p41Project", "fp1_p41", "Projekt", "LEFT OUTER JOIN p41Project fp1_p41 ON a.p41ID=fp1_p41.p41ID"));
                    break;
                case "fp2":
                    lis.Add(getREL("fp2_p31", "fp2_p31", "Realizováno", "LEFT OUTER JOIN dbo.fp2_p31(@x01id,@d1,@d2) fp2_p31 ON a.fp2ID=fp2_p31.p41ID"));
                    lis.Add(getREL("fp2_r01", "fp2_r01", "Kapacitní plán", "LEFT OUTER JOIN dbo.fp2_r01(@x01id,@d1,@d2) fp2_r01 ON a.fp2ID=fp2_r01.p41ID"));
                    lis.Add(getREL("p41Project", "fp2_p41", "Projekt", "LEFT OUTER JOIN p41Project fp2_p41 ON a.fp2ID=fp2_p41.p41ID"));

                    break;
                case "fp3":
                    lis.Add(getREL("fp3_p31", "fp3_p31", "Realizováno", "LEFT OUTER JOIN dbo.fp3_p31(@x01id,@d1,@d2) fp3_p31 ON a.fp3ID=fp3_p31.p28ID"));
                    lis.Add(getREL("p28Contact", "fp3_p28", "Kontakt", "LEFT OUTER JOIN p28Contact fp3_p28 ON a.fp3ID=fp3_p28.p28ID"));

                    break;
                default:
                    break;
            }

            if (f.p07LevelsCount > 1)
            {
                switch (strPrimaryPrefix)
                {
                    case "le2":
                    case "le3":
                    case "le4":
                    case "le5":
                        int intLevelIndex = Convert.ToInt32(strPrimaryPrefix.Substring(2, 1));
                        for (int i = 1; i < intLevelIndex; i++)
                        {
                            if (f.getP07Level(i, true) != null)
                            {
                                lis.Add(getREL($"le{i}", $"p41_le{i}", f.getP07Level(i, true), $"LEFT OUTER JOIN p41Project p41_le{i} ON a.p41ID_P07Level{i}=p41_le{i}.p41ID"));
                            }
                        }
                        break;
                    case "p31":
                        for (int i = 1; i <= 4; i++)
                        {
                            if (f.getP07Level(i, true) != null)
                            {
                                lis.Add(getREL($"le{i}", $"p31_p41_le{i}", f.getP07Level(i, true), $"LEFT OUTER JOIN p41Project p31_p41_le{i} ON p41x.p41ID_P07Level{i}=p31_p41_le{i}.p41ID"));
                            }
                        }
                        break;
                    case "p41":
                        for (int i = 1; i < 5; i++)
                        {
                            if (f.getP07Level(i, true) != null)
                            {
                                lis.Add(getREL($"le{i}", $"p41_le{i}", f.getP07Level(i, true), $"LEFT OUTER JOIN p41Project p41_le{i} ON a.p41ID_P07Level{i}=p41_le{i}.p41ID"));
                            }
                        }
                        break;
                }
            }

            return lis;
        }

        private string getOwnerSql(string prefix)
        {
            return string.Format("LEFT OUTER JOIN j02User {0}_owner ON a.j02ID_Owner = {0}_owner.j02ID", prefix);
        }

    }
}
