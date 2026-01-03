

namespace BL
{
    public class TheQueryFieldProvider
    {
        //private readonly BL.TheEntitiesProvider _ep;
        private string _Prefix;
        private List<BO.TheQueryField> _lis;
        private string _lastEntity;

        public TheQueryFieldProvider(string strPrefix)
        {
            _Prefix = strPrefix;
            //_ep = ep;
            _lis = new List<BO.TheQueryField>();
            SetupPallete();


        }
        public List<BO.TheQueryField> getPallete()
        {
            return _lis;
        }
        private void SetupPallete()
        {
            BO.TheQueryField of;
            switch (_Prefix)
            {
                case "j02":
                    AF("j02User", "MimoArchiv", "a.j02ValidUntil>GETDATE()", "Pouze otevřené záznamy", null, null, "bool1x");
                    AF("j02User", "VArchivu", "a.j02ValidUntil<GETDATE()", "Pouze záznamy v archivu", null, null, "bool1x");
                    
                    of = AF("j02User", "Pozice", "a.j07ID", "Pozice", "j07PersonPosition", null, "multi");
                    of = AF("j02User", "Stredisko", "a.j18ID", "Středisko uživatele", "j18CostUnit", null, "multi");
                    of = AF("j02User", "Fond", "a.c21ID", "Pracovní fond", "c21FondCalendar", null, "multi");
                    of = AF("j02User", "Role", "a.j04ID", "Aplikační role", "j04UserRole", null, "multi");

                    
                        AF("j02User", "OmezeniZpetneVykazovat", "ISNULL(a.j02TimesheetEntryDaysBackLimit,0)>0", "Omezení zpětně vykazovat hodiny/kusovník", null, null, "bool1x");
                    AF("j02User", "VirtualniUzivatel", "a.j02VirtualParentID IS NOT NULL", "Virtuální uživatel", null, null, "bool1x");

                    of = AF("j02User", "ExistRozpracovane", "a.j02ID", "Existují rozpracované úkony", null, null, "bool");
                    of.SqlWrapper = "select xa.j02ID FROM p31Worksheet xa INNER JOIN p41Project xb ON xa.p41ID=xb.p41ID WHERE xa.p71ID IS NULL AND xa.p91ID IS NULL AND xa.p31Date BETWEEN @p31date1 AND @p31date2";
                    of = AF("j02User", "ExistNevyuctovane", "a.j02ID", "Existují nevyúčtované úkony", null, null, "bool");
                    of.SqlWrapper = "select xa.j02ID FROM p31Worksheet xa INNER JOIN p41Project xb ON xa.p41ID=xb.p41ID WHERE xa.p91ID IS NULL AND xa.p31Date BETWEEN @p31date1 AND @p31date2";
                    of = AF("j02User", "ExistCekajici", "a.j02ID", "Existují schválené a dosud nevyúčtované úkony", null, null, "bool");
                    of.SqlWrapper = "select xa.j02ID FROM p31Worksheet xa INNER JOIN p41Project xb ON xa.p41ID=xb.p41ID WHERE xa.p71ID=1 AND xa.p91ID IS NULL AND xa.p31Date BETWEEN @p31date1 AND @p31date2";
                    of = AF("j02User", "ExistVyuctovane", "a.j02ID", "Existují vyúčtované úkony", null, null, "bool");
                    of.SqlWrapper = "select j02ID FROM p31Worksheet WHERE p91ID IS NOT NULL AND p31Date BETWEEN @p31date1 AND @p31date2";

                    AF("j02User", "MusiZmenitHeslo", "a.j02IsMustChangePassword", "Musí si změnit heslo", null, null, "bool");
                    AF("j02User", "AutomatickyZablokovan", "a.j02IsLoginAutoLocked", "Byl automaticky zablokován", null, null, "bool");

                    AF("j02User", "j02Email", "a.j02Email", "E-mail");
                    AF("j02User", "j02Mobile", "a.j02Mobile", "Mobil");
                    AF("j02User", "j02Code", "a.j02Code", "Kód");
                    AF("j02User", "j02JobTitle", "a.j02JobTitle", "Funkce na vizitce");

                    break;
                case "o23":
                    AF("o23Doc", "MimoArchiv", "a.o23ValidUntil>GETDATE()", "Pouze otevřené záznamy", null, null, "bool1x");
                    AF("o23Doc", "VArchivu", "a.o23ValidUntil<GETDATE()", "Pouze záznamy v archivu", null, null, "bool1x");

                    AF("o23Doc", "TypDokumentu", "a.o18ID", "Typ dokumentu", "o18DocType", null, "multi");

                    of = AF("o23Doc", "RoleDokumentu", "xa.x67ID", "Role v dokumentu", "x67EntityRole", "x67entity|string|o23", "multi");
                    of.IsOfferPersonsAdd = true;
                    of.SourceSql = "select * from x67EntityRole where x67Entity='o23'";
                    of.SqlWrapper = "EXISTS(select xb.o23ID FROM x69EntityRole_Assign xa INNER JOIN o23Doc xb ON xa.x69RecordPid=xb.o23ID WHERE #filter# AND xb.o23ID=a.o23ID)";

                    AF("o23Doc", "MaReminder", "EXISTS (SELECT 1 FROM o24Reminder WHERE o24RecordPid=a.o23ID AND o24RecordEntity='o23')", "Vyplněno upozornění", null, null, "bool1x");
                    AF("o23Doc", "VyplnenaBarva", "a.o23RowColorFlag IS NOT NULL", "Nahozená barva záznamu", null, null, "bool1x");

                    break;
                case "p40":                 
                    AF("p40WorkSheet_Recurrence", "ExistujeSkluzGenerovani", "p39miss.p39DateCreate_Min IS NOT NULL", "Skluz generování", null, null, "bool1x");
                    
                    AF("p40WorkSheet_Recurrence", "ExistujeSkluzGenerovaniMimoRobot", "p39miss.p39DateCreate_Min<DATEADD(DAY,-31,GETDATE())", "Skluz generování mimo dosah robota", null, null, "bool1x");

                    AF("p40WorkSheet_Recurrence", "ExistujeChybaVGenerovani", "a.p40ID IN (select p40ID FROM p39WorkSheet_Recurrence_Plan WHERE p39ErrorMessage_NewInstance IS NOT NULL)", "Chyba v generování úkonu", null, null, "bool1x");

                    AF("p40WorkSheet_Recurrence", "ExpiracePredpisu", "a.p40LastSupplyDate<GETDATE()", "Expirovalo období předpisu odměny", null, null, "bool1x");
                   
                    AF("p40WorkSheet_Recurrence", "ProjektVArchivu", "p41x.p41ValidUntil<GETDATE()", "Projekt v archivu", null, null, "bool1x");
                    

                    break;
                case "p58":                   
                    AF("p58TaskRecurrence", "ExistujeSkluzGenerovani", "p39miss.p39DateCreate_Min IS NOT NULL", "Skluz generování", null, null, "bool1x");

                    AF("p58TaskRecurrence", "ExistujeSkluzGenerovaniMimoRobot", "p39miss.p39DateCreate_Min<DATEADD(DAY,-31,GETDATE())", "Skluz generování mimo dosah robota", null, null, "bool1x");
                    AF("p58TaskRecurrence", "ExpiracePredpisu", "a.p58BaseDateEnd<GETDATE()", "Expirovalo období opakování", null, null, "bool1x");

                    AF("p58TaskRecurrence", "ProjektVArchivu", "p41x.p41ValidUntil<GETDATE()", "Projekt v archivu", null, null, "bool1x");

                    break;
                case "p91":
                    AF("p91Invoice", "NeuhrazenePoSplatnosti", "a.p91IsDraft=0 AND DATEADD(day,1,a.p91DateMaturity)<GETDATE() AND a.p91Amount_Debt>1", "Neuhrazené po splatnosti", null, null, "bool1x");
                    AF("p91Invoice", "Neuhrazene", "a.p91IsDraft=0 AND a.p91Amount_Debt>1", "Neuhrazené", null, null, "bool1x");
                    AF("p91Invoice", "VeSplatnosti", "a.p91IsDraft=0 AND DATEADD(day,1,a.p91DateMaturity)>GETDATE()", "Ve splatnosti", null, null, "bool1x");
                    AF("p91Invoice", "Uhrazene", "a.p91IsDraft=0 AND a.p91Amount_Debt<1", "Uhrazené", null, null, "bool1x");
                    AF("p91Invoice", "CastecneUhrazene", "a.p91IsDraft=0 AND a.p91Amount_Debt>1 AND a.p91Amount_Debt+1<p91Amount_TotalDue", "Částečně uhrazené", null, null, "bool1x");

                    AF("p91Invoice", "JeDobropis", "EXISTS(select 1 FROM p92InvoiceType WHERE p92TypeFlag=2 AND p92ID=a.p92ID)", "Opravný doklad (dobropis)", null, null, "bool1x");

                    AF("p91Invoice", "MaPrvniUpominku", "EXISTS (select 1 FROM p84Upominka WHERE p91ID=a.p91ID AND p84Index=1)", "První upomínka", null, null, "bool1x");
                    AF("p91Invoice", "MaDruhouUpominku", "EXISTS (select 1 FROM p84Upominka WHERE p91ID=a.p91ID AND p84Index=2)", "Druhá upomínka", null, null, "bool1x");
                    AF("p91Invoice", "MaTretiUpominku", "EXISTS (select 1 FROM p84Upominka WHERE p91ID=a.p91ID AND p84Index=3)", "Třetí upomínka", null, null, "bool1x");

                    AF("p91Invoice", "ZamekA", "a.p91LockFlag & 2 = 2", "Zámek A", null, null, "bool1x");
                    AF("p91Invoice", "ZamekB", "a.p91LockFlag & 4 = 4", "Zámek B", null, null, "bool1x");
                    AF("p91Invoice", "ZamekC", "a.p91LockFlag & 8 = 8", "Zámek C", null, null, "bool1x");
                    AF("p91Invoice", "ZamekABC", "a.p91LockFlag & 2 = 2 AND a.p91LockFlag & 4 = 4 AND a.p91LockFlag & 8 = 8", "Zámek A+B+C", null, null, "bool1x");
                    AF("p91Invoice", "BezZamku", "a.p91LockFlag=0", "Bez zámku", null, null, "bool1x");

                    AF("p91Invoice", "ObsahujiUkonyVarchivu", "EXISTS (select 1 FROM p31Worksheet_Del WHERE p91ID=a.p91ID)", "Obsahují úkony v archivu", null, null, "bool1x");

                    AF("p91Invoice", "ObsahujiUkony6", "EXISTS (select 1 FROM p31Worksheet WHERE p91ID=a.p91ID AND p70ID=6)", "Obsahuje [Zahrnout do paušálu]", null, null, "bool1x");
                    AF("p91Invoice", "ObsahujiUkony3", "EXISTS (select 1 FROM p31Worksheet WHERE p91ID=a.p91ID AND p70ID=3)", "Obsahuje [Skrytý odpis]", null, null, "bool1x");
                    AF("p91Invoice", "ObsahujiUkony2", "EXISTS (select 1 FROM p31Worksheet WHERE p91ID=a.p91ID AND p70ID=2)", "Obsahuje [Viditelný odpis]", null, null, "bool1x");

                    AF("p91Invoice", "JeDraft", "a.p91IsDraft", "DRAFT faktury", null, null, "bool");

                    AF("p91Invoice", "MimoArchiv", "a.p91ValidUntil>GETDATE()", "Pouze otevřené záznamy", null, null, "bool1x");
                    AF("p91Invoice", "VArchivu", "a.p91ValidUntil<GETDATE()", "Pouze záznamy v archivu", null, null, "bool1x");

                    of = AF("p91Invoice", "TypFaktury", "a.p92ID", "Typ faktury", "p92InvoiceType", null, "multi");
                    of = AF("p91Invoice", "VystavovatelFaktury", "p92x.p93ID", "Vystavovatel faktury", "p93InvoiceHeader", null, "multi");
                    AF("p91Invoice", "VyplnenaBarva", "a.p91RowColorFlag IS NOT NULL", "Nahozená barva záznamu", null, null, "bool1x");

                    AF("p91Invoice", "VyuctovaneVydaje", "EXISTS (select 1 FROM p31Worksheet xa INNER JOIN p32Activity xb ON xa.p32ID=xb.p32ID INNER JOIN p34ActivityGroup xc ON xb.p34ID=xc.p34ID WHERE xa.p91ID=a.p91ID AND xc.p33ID IN (2,5) AND xc.p34IncomeStatementFlag=1)", "Obsahují peněžní výdaje", null, null, "bool1x");
                    AF("p91Invoice", "VyuctovaneOdmeny", "EXISTS (select 1 FROM p31Worksheet xa INNER JOIN p32Activity xb ON xa.p32ID=xb.p32ID INNER JOIN p34ActivityGroup xc ON xb.p34ID=xc.p34ID WHERE xa.p91ID=a.p91ID AND xc.p33ID IN (2,5) AND xc.p34IncomeStatementFlag=2)", "Obsahují pevné odměny", null, null, "bool1x");
                    AF("p91Invoice", "VyuctovaneOdmeny", "EXISTS (select 1 FROM p31Worksheet xa INNER JOIN p32Activity xb ON xa.p32ID=xb.p32ID INNER JOIN p34ActivityGroup xc ON xb.p34ID=xc.p34ID WHERE xa.p91ID=a.p91ID AND xc.p33ID=3)", "Obsahují kusovník", null, null, "bool1x");

                    break;
                case "o43":
                    AF("o43Inbox", "MimoArchiv", "a.o43ValidUntil>GETDATE()", "Pouze otevřené záznamy", null, null, "bool1x");
                    AF("o43Inbox", "VArchivu", "a.o43ValidUntil<GETDATE()", "Pouze záznamy v archivu", null, null, "bool1x");

                    break;
                case "p28":
                    AF("p28Contact", "MimoArchiv", "a.p28ValidUntil>GETDATE()", "Pouze otevřené záznamy", null, null, "bool1x");
                    AF("p28Contact", "VArchivu", "a.p28ValidUntil<GETDATE()", "Pouze záznamy v archivu", null, null, "bool1x");

                    AF("p28Contact", "TypKontaktu", "a.p29ID", "Typ kontaktu", "p29ContactType", null, "multi");
                    AF("p28Contact", "PravnickaOsoba", "a.p28IsCompany", "Právnická osoba", null, null, "bool");
                    AF("p28Contact", "IsoCode", "a.p28CountryCode", "ISO kód státu");

                    of=AF("p28Contact", "RoleKontaktu", "xa.x67ID", "Role v kontaktu", "x67EntityRole", "x67entity|string|p28", "multi");
                    of.IsOfferPersonsAdd = true;
                    of.SourceSql = "select * from x67EntityRole where x67Entity='p28'";
                    of.SqlWrapper = "EXISTS(select xb.p28ID FROM x69EntityRole_Assign xa INNER JOIN p28Contact xb ON xa.x69RecordPid=xb.p28ID WHERE #filter# AND xb.p28ID=a.p28ID)";

                    of = AF("p28Contact", "StitkyKontaktu", "xa.o51ID", "Štítky", "o51Tag", null, "multi");
                    of.SourceSql = "select xxa.* from o51Tag xxa INNER JOIN o53TagGroup xxb ON xxa.o53ID=xxb.o53ID where xxb.o53Entities LIKE '%p28%'";
                    of.SqlWrapper = "EXISTS(select xa.o52RecordPid FROM o52TagBinding xa INNER JOIN o51Tag xb ON xa.o51ID=xb.o51ID INNER JOIN o53TagGroup xc ON xb.o53ID=xc.o53ID WHERE #filter# AND xa.o52RecordPid=a.p28ID AND xc.o53Entities LIKE '%p28%')";



                    of = AF("p28Contact", "ExistOtevreneProjekty", "a.p28ID", "Existují otevřené projekty", null, null, "bool");
                    of.SqlWrapper = "select p28ID_Client FROM p41Project WHERE p28ID_Client IS NOT NULL AND GETDATE() BETWEEN p41ValidFrom AND p41ValidUntil";

                    of = AF("p28Contact", "ExistujeVyuctovani6", "a.p28ID", "Vyúčtování v posledních 6ti měsících", null, null, "bool");
                    of.SqlWrapper = "select p28ID FROM p91Invoice WHERE DATEADD(MONTH,6,p91Date)>GETDATE()";

                    of = AF("p28Contact", "ExistRozpracovane", "a.p28ID", "Existují rozpracované úkony", null, null, "bool");
                    of.SqlWrapper = "select xb.p28ID_Client FROM p31Worksheet xa INNER JOIN p41Project xb ON xa.p41ID=xb.p41ID WHERE xb.p28ID_Client IS NOT NULL AND xa.p71ID IS NULL AND xa.p91ID IS NULL AND xa.p31Date BETWEEN @p31date1 AND @p31date2";
                    of = AF("p28Contact", "ExistNevyuctovane", "a.p28ID", "Existují nevyúčtované úkony", null, null, "bool");
                    of.SqlWrapper = "select xb.p28ID_Client FROM p31Worksheet xa INNER JOIN p41Project xb ON xa.p41ID=xb.p41ID WHERE xb.p28ID_Client IS NOT NULL AND xa.p91ID IS NULL AND xa.p31Date BETWEEN @p31date1 AND @p31date2";
                    of = AF("p28Contact", "ExistCekajici", "a.p28ID", "Existují schválené a dosud nevyúčtované úkony", null, null, "bool");
                    of.SqlWrapper = "select xb.p28ID_Client FROM p31Worksheet xa INNER JOIN p41Project xb ON xa.p41ID=xb.p41ID WHERE xb.p28ID_Client IS NOT NULL AND xa.p71ID=1 AND xa.p91ID IS NULL AND xa.p31Date BETWEEN @p31date1 AND @p31date2";
                    of = AF("p28Contact", "ExistVyuctovane", "a.p28ID", "Existují vyúčtované úkony", null, null, "bool");
                    of.SqlWrapper = "select xb.p28ID_Client FROM p31Worksheet xa INNER JOIN p41Project xb ON xa.p41ID=xb.p41ID WHERE xb.p28ID_Client IS NOT NULL AND xa.p91ID IS NOT NULL AND xa.p31Date BETWEEN @p31date1 AND @p31date2";

                    AF("p28Contact", "CenikNaMiru", "a.p51ID_Billing IN (select p51ID FROM p51PriceList WHERE p51IsCustomTailor=1)", "Fakturační sazby na míru", null, null, "bool1x");
                    AF("p28Contact", "CenikRoot", "a.p51ID_Billing IS NULL", "Nepřiřazený ceník (ROOT fakturační sazby)", null, null, "bool1x");
                    AF("p28Contact", "CenikPrirazeny", "a.p51ID_Billing IN (select p51ID FROM p51PriceList WHERE p51TypeFlag=1)", "Přiřazený ceník fakturačních sazeb", null, null, "bool1x");

                    AF("p28Contact", "MajiFakturacniPoznamky", "EXISTS (SELECT 1 FROM b05Workflow_History WHERE b05RecordPid=a.p28ID AND b05RecordEntity='p28' AND b05Tab1Flag IN (4,6))", "Vyplněna fakturační poznámka", null, null, "bool1x");
                    AF("p28Contact", "MaKontaktniOsoby", "EXISTS (SELECT p30ID FROM p30ContactPerson WHERE p28ID=a.p28ID)", "Vyplněné kontaktní osoby", null, null, "bool1x");
                    AF("p28Contact", "MaKontaktniMedia", "EXISTS (SELECT 1 FROM o32Contact_Medium WHERE p28ID=a.p28ID)", "Vyplněná kontaktní média", null, null, "bool1x");
                    AF("p28Contact", "MaReminder", "EXISTS (SELECT 1 FROM o24Reminder WHERE o24RecordPid=a.p28ID AND o24RecordEntity='p28')", "Vyplněno upozornění", null, null, "bool1x");
                    AF("p28Contact", "MaHlidace", "EXISTS (SELECT 1 FROM b21HlidacBinding WHERE b21RecordPid=a.p28ID AND b21RecordEntity='p28')", "Vyplněn hlídač", null, null, "bool1x");

                    AF("p28Contact", "VyplnenaBarva", "a.p28RowColorFlag IS NOT NULL", "Nahozená barva záznamu", null, null, "bool1x");

                    break;
                case "p41":
                case "le1":
                case "le2":
                case "le3":
                case "le4":
                case "le5":
                    AF("p41Project", "TypProjektu", "a.p42ID", "Typ projektu", "p42ProjectType", null, "multi");
                    AF("p41Project", "StrediskoProjektu", "a.j18ID", "Středisko projektu", "j18CostUnit", null, "multi");
                    AF("p41Project", "TypFaktury", "a.p92ID", "Výchozí typ faktury", "p92InvoiceType", null, "multi");
                    AF("p41Project", "KlastrAktivit", "a.p61ID", "Klastr aktivit", "p61ActivityCluster", null, "multi");
                    AF("p41Project", "PojmenovanaLokalita", "a.p15ID", "Pojmenovaná lokalita", "p15Location", null, "multi");

                    of = AF("p41Project", "RoleProjektu", "xa.x67ID", "Role v projektu", "x67EntityRole", "x67entity|string|p41", "multi");
                    of.IsOfferPersonsAdd = true;
                    of.SourceSql = "select * from x67EntityRole where x67Entity='p41'";
                    of.SqlWrapper = "EXISTS(select xb.p41ID FROM x69EntityRole_Assign xa INNER JOIN p41Project xb ON xa.x69RecordPid=xb.p41ID WHERE #filter# AND xb.p41ID=a.p41ID)";


                    of = AF("p41Project", "StitkyProjektu", "xa.o51ID", "Štítky", "o51Tag",null, "multi");                    
                    of.SourceSql = "select * from o51Tag xxa INNER JOIN o53TagGroup xxb ON xxa.o53ID=xxb.o53ID where xxb.o53Entities LIKE ='%p41%'";
                    of.SqlWrapper = "EXISTS(select xa.o52RecordPid FROM o52TagBinding xa INNER JOIN o51Tag xb ON xa.o51ID=xb.o51ID WHERE #filter# AND xa.o52RecordPid=a.p41ID)";


                    AF("p41Project", "MajiOpakovaneOdmeny", "EXISTS (SELECT 1 FROM p40WorkSheet_Recurrence WHERE p41ID=a.p41ID)", "Projekty s opakovanou odměnou", null, null, "bool1x");
                    AF("p41Project", "MajiOtevreneUkoly", "EXISTS (SELECT 1 FROM p56Task WHERE p41ID=a.p41ID AND GETDATE() BETWEEN p56ValidFrom AND p56ValidUntil)", "Projekty s otevřenými úkoly", null, null, "bool1x");
                    AF("p41Project", "MajiOpakovaneUkoly", "EXISTS (SELECT 1 FROM p58TaskRecurrence WHERE p41ID=a.p41ID)", "Projekty s opakovanými úkoly", null, null, "bool1x");
                    AF("p41Project", "MajiPoznamky", "EXISTS (SELECT 1 FROM b05Workflow_History WHERE b05RecordPid=a.p41ID AND b05RecordEntity='p41' AND b05IsCommentOnly=1)", "Projekty s poznámkou", null, null, "bool1x");
                    AF("p41Project", "MajiFakturacniPoznamky", "EXISTS (SELECT 1 FROM b05Workflow_History WHERE b05RecordPid=a.p41ID AND b05RecordEntity='p41' AND b05Tab1Flag IN (4,6))", "Vyplněna fakturační poznámka", null, null, "bool1x");

                    of = AF("p41Project", "InterniProjekty", "ISNULL(a.p41BillingFlag,0) IN (6,99)", "Interní projekty", null, null, "bool1x");

                    of = AF("p41Project", "ExistRozpracovane", "a.p41ID", "Existují rozpracované úkony", null, null, "bool");
                    of.SqlWrapper = "select p41ID FROM p31Worksheet WHERE p71ID IS NULL AND p91ID IS NULL AND p31Date BETWEEN @p31date1 AND @p31date2";
                    of = AF("p41Project", "ExistNevyuctovane", "a.p41ID", "Existují nevyúčtované úkony", null, null, "bool");
                    of.SqlWrapper = "select p41ID FROM p31Worksheet WHERE p91ID IS NULL AND p31Date BETWEEN @p31date1 AND @p31date2";
                    of = AF("p41Project", "ExistCekajici", "a.p41ID", "Existují schválené a dosud nevyúčtované úkony", null, null, "bool");
                    of.SqlWrapper = "select p41ID FROM p31Worksheet WHERE p71ID=1 AND p91ID IS NULL AND p31Date BETWEEN @p31date1 AND @p31date2";
                    of = AF("p41Project", "ExistVyuctovane", "a.p41ID", "Existují vyúčtované úkony", null, null, "bool");
                    of.SqlWrapper = "select p41ID FROM p31Worksheet WHERE p91ID IS NOT NULL AND p31Date BETWEEN @p31date1 AND @p31date2";

                    AF("p41Project", "CenikNaMiru", "a.p51ID_Billing IN (select p51ID FROM p51PriceList WHERE p51IsCustomTailor=1)", "Fakturační sazby na míru", null, null, "bool1x");
                    AF("p41Project", "CenikRoot", "a.p51ID_Billing IS NULL", "Nepřiřazený ceník (klientský nebo root)", null, null, "bool1x");
                    AF("p41Project", "CenikPrirazeny", "a.p51ID_Billing IN (select p51ID FROM p51PriceList WHERE p51TypeFlag=1)", "Přiřazený ceník fakturačních sazeb", null, null, "bool1x");

                    AF("p41Project", "ExistujeRozpocet", "a.p41Plan_Hours>0 OR a.p41Plan_Expenses>0 OR a.p41Plan_Revenue>0", "Vyplněný rozpočet projektu", null, null, "bool1x");

                    of = AF("p41Project", "ZakazVykazovat", "a.p41WorksheetOperFlag=1", "V projektu zákaz vykazovat", null, null, "bool1x");


                    AF("p41Project", "MaHlidace", "EXISTS (SELECT 1 FROM b21HlidacBinding WHERE b21RecordPid=a.p41ID AND b21RecordEntity='p41')", "Vyplněn hlídač", null, null, "bool1x");
                    AF("p41Project", "MaReminder", "EXISTS (SELECT 1 FROM o24Reminder WHERE o24RecordPid=a.p41ID AND o24RecordEntity='p41')", "Vyplněno upozornění", null, null, "bool1x");
                    AF("p41Project", "VyplnenaBarva", "a.p41RowColorFlag IS NOT NULL", "Nahozená barva záznamu", null, null, "bool1x");

                    of = AF("p41Project", "MimoArchiv", "a.p41ValidUntil>GETDATE()", "Pouze otevřené záznamy", null, null, "bool1x");
                    of = AF("p41Project", "VArchivu", "a.p41ValidUntil<GETDATE()", "Pouze záznamy v archivu", null, null, "bool1x");
                    break;
                case "p31":
                    of = AF("p31Worksheet", "FStatus", "a.p70ID", "Fakturační status", "p70BillingStatus", null, "multi");
                    of = AF("p31Worksheet", "ApproveStatus", "a.p72ID_AfterApprove", "Fakturační status od schvalovatele", "p72PreBillingStatus", null, "multi");
                    of = AF("p31Worksheet", "Sesit", "p32x.p34ID", "Sešit", "p34ActivityGroup", null, "multi");
                    of = AF("p31Worksheet", "Aktivita", "a.p32ID", "Aktivita", "p32Activity", null, "multi");
                    of = AF("p31Worksheet", "FakturacniOddil", "p32x.p95ID", "Fakturační oddíl", "p95InvoiceRow", null, "multi");
                    of = AF("p31Worksheet", "TypProjektu", "p41x.p42ID", "Typ projektu", "p42ProjectType", null, "multi");
                    of = AF("p31Worksheet", "Uživatel", "a.j02ID", "Uživatel", "j02User", null, "multi");
                    of = AF("p31Worksheet", "StrediskoProjektu", "p41x.j18ID", "Středisko projektu", "j18CostUnit", null, "multi");
                    //of = AF("p31Worksheet", "StrediskoUzivatele", "j02x.j18ID", "Středisko uživatele", "j18CostUnit", null, "multi");
                    of = AF("p31Worksheet", "KlientProjektu", "p41x.p28ID_Client", "Klient projektu", "p28Contact", "canbe_client|bool|1", "multi");
                    of = AF("p31Worksheet", "Dodavatel", "a.p28ID_Supplier", "Dodavatel výdaje", "p28Contact", "canbe_supplier|bool|1", "multi");
                    of = AF("p31Worksheet", "MenaUkonu", "a.j27ID_Billing_Orig", "Měna úkonu", "j27Currency", null, "multi");

                    of = AF("p31Worksheet", "PouzeHodiny", "p34x.p33ID=1", "Pouze hodiny", null, null, "bool1x");
                    of = AF("p31Worksheet", "PouzeVydaje", "p34x.p33ID IN (2,5) AND p34x.p34IncomeStatementFlag=1", "Pouze výdaje", null, null, "bool1x");
                    of = AF("p31Worksheet", "PouzeOdmeny", "p34x.p33ID IN (2,5) AND p34x.p34IncomeStatementFlag=2", "Pouze peněžní odměny", null, null, "bool1x");

                    of = AF("p31Worksheet", "Rozpracovane", "a.p71ID IS NULL AND a.p91ID IS NULL", "Rozpracované úkony", null, null, "bool1x");
                    of = AF("p31Worksheet", "Schvalene", "a.p71ID=1 AND a.p91ID IS NULL", "Schváleno, čeká na vyúčtování", null, null, "bool1x");
                    of = AF("p31Worksheet", "Vyuctovane", "a.p91ID IS NOT NULL", "Vyúčtováno", null, null, "bool1x");
                    of = AF("p31Worksheet", "Nevyuctovane", "a.p91ID IS NULL", "Nevyúčtováno", null, null, "bool1x");
                    of = AF("p31Worksheet", "VyplnenaKorekce", "a.p72ID_AfterTrimming IS NOT NULL", "Vyplněná korekce úkonu", null, null, "bool1x");

                    AF("p31Worksheet", "UhrazeneFaktury", "EXISTS (SELECT 1 FROM p91Invoice WHERE p91ID=a.p91ID AND p91Amount_Debt<1)", "Úkony ve 100% uhrazeném vyúčtování", null, null, "bool1x");

                    
                        AF("p31Worksheet", "BudouciNepritomnost", "ISNULL(p32x.p32AbsenceFlag,0)>0", "Aktivita budoucí nepřítomnosti", null, null, "bool1x");
                    
                    AF("p31Worksheet", "SDokumentem", "a.o23ID IS NOT NULL", "Vazba s dokumentem", null, null, "bool1x");
                    AF("p31Worksheet", "SDodavatelem", "a.p28ID_Supplier IS NOT NULL", "Vazba s dodavatelem výdaje", null, null, "bool1x");
                    AF("p31Worksheet", "SKontaktniOsobou", "a.p28ID_ContactPerson IS NOT NULL", "Vazba s kontaktní osobou", null, null, "bool1x");
                    AF("p31Worksheet", "SKodem", "a.p31Code IS NOT NULL", "Vyplněn kód dokladu", null, null, "bool1x");
                    AF("p31Worksheet", "PripojenyNotepad", "EXISTS (SELECT 1 FROM b05Workflow_History WHERE b05RecordPid=a.p31ID AND b05RecordEntity='p31')", "Připojena poznámka (Notepad)", null, null, "bool1x");
                    AF("p31Worksheet", "VygenerovanoRobotem", "a.p40ID_Source IS NOT NULL","Vygenerováno robotem (opakovaná odměna)", null, null, "bool1x");
                    AF("p31Worksheet", "VazbaSUkolem", "a.p56ID IS NOT NULL", "Vazba s úkolem", null, null, "bool1x");
                    AF("p31Worksheet", "VyplnenyInterniText", "a.p31TextInternal IS NOT NULL", "Vyplněný interní text", null, null, "bool1x");
                    AF("p31Worksheet", "MaReminder", "EXISTS (SELECT 1 FROM o24Reminder WHERE o24RecordPid=a.p31ID AND o24RecordEntity='p31')", "Vyplněno upozornění", null, null, "bool1x");
                    AF("p31Worksheet", "VyplnenaBarva", "a.p31RowColorFlag IS NOT NULL", "Nahozená barva záznamu", null, null, "bool1x");

                    of = AF("p31Worksheet", "NazevProjektu", "p41x.p41Name", "Název projektu", "p41Task");
                    of = AF("p31Worksheet", "KlientVyuctovaniNazev", "p91x.p91Client", "Klient vyúčtování", "p91Invoice");

                    break;

                case "o22":
                    AF("o22Milestone", "MimoArchiv", "a.o22ValidUntil>GETDATE()", "Pouze otevřené záznamy", null, null, "bool1x");
                    AF("o22Milestone", "VArchivu", "a.o22ValidUntil<GETDATE()", "Pouze záznamy v archivu", null, null, "bool1x");

                    AF("o22Milestone", "TypTerminu", "a.o21ID", "Typ termínu/události", "o21MilestoneType", null, "multi");

                    of = AF("o22Milestone", "RoleUdalosti", "xa.x67ID", "Role v termínu/události", "x67EntityRole", "x67entity|string|o22", "multi");
                    of.IsOfferPersonsAdd = true;
                    of.SourceSql = "select * from x67EntityRole where x67Entity='o22'";
                    of.SqlWrapper = "EXISTS(select xb.o22ID FROM x69EntityRole_Assign xa INNER JOIN o22Milestone xb ON xa.x69RecordPid=xb.o22ID WHERE #filter# AND xb.o22ID=a.o22ID)";

                    AF("o22Milestone", "MaReminder", "EXISTS (SELECT 1 FROM o24Reminder WHERE o24RecordPid=a.o22ID AND o24RecordEntity='o22')", "Vyplněno upozornění", null, null, "bool1x");

                    break;
                case "p56":
                    AF("p56Task", "MimoArchiv", "a.p56ValidUntil>GETDATE()", "Pouze otevřené záznamy", null, null, "bool1x");
                    AF("p56Task", "VArchivu", "a.p56ValidUntil<GETDATE()", "Pouze záznamy v archivu", null, null, "bool1x");

                    AF("p56Task", "TypUkolu", "a.p57ID", "Typ úkolu", "p57TaskType", null, "multi");

                    of = AF("p56Task", "RoleUkolu", "xa.x67ID", "Role v úkolu", "x67EntityRole", "x67entity|string|p56", "multi");
                    of.IsOfferPersonsAdd = true;
                    of.SourceSql = "select * from x67EntityRole where x67Entity='p56'";
                    of.SqlWrapper = "EXISTS(select xb.p56ID FROM x69EntityRole_Assign xa INNER JOIN p56Task xb ON xa.x69RecordPid=xb.p56ID WHERE #filter# AND xb.p56ID=a.p56ID)";

                    AF("p56Task", "VygenerovanoRobotem", "EXISTS(select 1 FROM p59TaskRecurrence_Plan WHERE p56ID_NewInstance=a.p56ID)", "Vygenerováno robotem (opakovaný úkol)", null, null, "bool1x");
                    AF("p56Task", "VTodolist", "a.p55ID IS NOT NULL", "V rámci Todo-list", null, null, "bool1x");

                    AF("p56Task", "MaReminder", "EXISTS (SELECT 1 FROM o24Reminder WHERE o24RecordPid=a.p56ID AND o24RecordEntity='p56')", "Vyplněno upozornění", null, null, "bool1x");
                    break;
                case "p84":
                    AF("p84Upominka", "p84Index", "a.p84Index", "Stupeň upomínky", null, null, "number");
                    AF("p84Upominka", "TypUpominky", "a.p83ID", "Typ upomínky", "p83UpominkaType", null, "multi");
                    AF("p84Upominka", "TypFakturyUpominky", "p91x.p92ID", "Typ faktury", "p92InvoiceType", null, "multi");

                   


                    break;
                default:
                    break;
            }




        }


        private BO.TheQueryField AF(string strEntity, string strField, string strSqlSyntax, string strHeader, string strSourceEntity = null, string strMyQueryInline = null, string strFieldType = "string")
        {
            if (strEntity != _lastEntity)
            {
                //zatím nic
            }

            _lis.Add(new BO.TheQueryField() { Field = strField, FieldSqlSyntax = strSqlSyntax, Entity = strEntity, Header = strHeader, FieldType = strFieldType, SourceEntity = strSourceEntity, MyQueryInline = strMyQueryInline, TranslateLang1 = strHeader, TranslateLang2 = strHeader, TranslateLang3 = strHeader });
            _lastEntity = strEntity;
            return _lis[_lis.Count - 1];
        }
    }
}
