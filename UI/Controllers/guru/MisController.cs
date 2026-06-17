using BO;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using UI.Models.Guru;

using System.Text.Json;

namespace UI.Controllers.guru
{
    public class MisController : Controller
    {
        private BL.Factory _f;
        private System.Text.StringBuilder _sb;


        public MisController(BL.Factory f)
        {
            _f = f;

        }

        public IActionResult Index()
        {

            var v = new MigrationViewModel() { CountryCode = "CZ", DestPassword = DateTime.Now.ToString().Substring(0, 1) + "." + BO.Code.Bas.GetGuid().Substring(0, 5) + "X" };
            v.SourceDbName = "zdrojovadb";
            v.DestDbName = "a7marktime";

            return ViewOnlyForVerified(v);

        }

        private IActionResult ViewOnlyForVerified(object v)
        {
            return View(v); //pro testování

        }

        [HttpPost]
        public ActionResult Index(MigrationViewModel v, string oper)
        {
            _f.CurrentUser = new BO.RunningUser() { j02Login = _f.App.GuruLogin };

            if (oper == "postback")
            {
                return View(v);
            }

            if (ModelState.IsValid)
            {
                v.Errors = new List<string>();
                v.lisKeys = new List<CreateLicenseKey>();



                _sb = new System.Text.StringBuilder();
                string s = null;

                Handle_Pre1(v);

                var lisX90 = _f.FBL.GetListX90().Where(p => p.x90IsUse == true);
                //var lisX90 = getDistributionJson<BO.x90Migration>("x90Migration").Where(p => p.x90IsUse).OrderBy(p => p.x90Ordinary).ThenBy(p=>p.x90ID);

                foreach (var c in lisX90)
                {
                    string strTab1 = c.x90Table1;

                    if (strTab1 == "p35Unit")
                    {
                        Handle_CreateX01(v);    //p35Unit je první tabulka s vazbou na X01ID
                    }

                    sb("");
                    sb($"---{strTab1}---");
                    sb("");

                    string strTab2 = c.x90Table2;

                    string strW = null;
                    if (c.x90SqlCondition1 != null)
                    {
                        strW = c.x90SqlCondition1;
                        strW = strW.Replace("#sourcedb#", $"{v.SourceDbName}.dbo.");

                    }
                    string strMapping1 = null;
                    if (c.x90Mapping1 != null)
                    {
                        strMapping1 = c.x90Mapping1;
                    }
                    string strFieldsMissing1 = null;
                    if (c.x90Fields1Missing != null)
                    {
                        strFieldsMissing1 = c.x90Fields1Missing;
                    }

                    bool bolIdentity = true; bool bolX01ID = false;
                    s = null;
                    if (strFieldsMissing1 != null && strFieldsMissing1.Contains("x01ID"))
                    {
                        bolX01ID = true;
                    }
                    if (strTab1 == "c11StatPeriod" || strTab1.Contains("_FreeField") || strTab1 == "p39WorkSheet_Recurrence_Plan")
                    {
                        bolIdentity = false;
                    }
                    if (bolIdentity)
                    {
                        sb($"SET IDENTITY_INSERT {v.DestDbName}.dbo.{strTab2} ON;");
                        sb("");
                    }

                    s += $"INSERT INTO {v.DestDbName}.dbo.{strTab2}";
                    s += "(" + strMapping1;
                    if (bolX01ID)
                    {
                        s += ",x01ID";
                    }

                    switch (strTab1)
                    {
                        case "x67EntityRole":
                            s += ",x67Entity";
                            break;
                        case "j02Person":
                            s += ",j04ID,j02Login";
                            break;
                        case "x28EntityField":
                            s += ",x28Entity";
                            break;

                        case "p28Contact":
                            s += ",p28ShortName";
                            break;
                        case "x38CodeLogic":
                            s += ",x38Entity";
                            break;
                        case "b01WorkflowTemplate":
                            s += ",b01Entity";
                            break;
                        case "c22FondCalendar_Date":
                            s += ",c22CountryCode";
                            break;


                    }

                    s += ")";

                    sb(s);

                    string strSourceSQL = $"SELECT {strMapping1}";
                    if (bolX01ID)
                    {
                        strSourceSQL += $",{v.DestX01ID}";
                    }

                    switch (strTab1)
                    {
                        case "x67EntityRole":
                            strSourceSQL += $",{v.SourceDbName}.dbo.get_prefix(x29ID)";
                            break;

                        case "x28EntityField":
                            strSourceSQL += $",{v.SourceDbName}.dbo.get_prefix(x29ID)";
                            break;
                        case "j02Person":
                            strSourceSQL += $",(select min(j04ID) FROM {v.DestDbName}.dbo.j04UserRole),NULL";
                            break;

                        case "p28Contact":
                            strSourceSQL += ",p28CompanyShortName";
                            break;
                        case "x38CodeLogic":
                            strSourceSQL += $",{v.SourceDbName}.dbo.get_prefix(x29ID)";
                            break;
                        case "b01WorkflowTemplate":
                            strSourceSQL += $",{v.SourceDbName}.dbo.get_prefix(x29ID)";
                            break;
                        case "c22FondCalendar_Date":
                            strSourceSQL += $",'{v.CountryCode}'";
                            break;


                    }

                    sb(strSourceSQL);

                    strSourceSQL = $"FROM {v.SourceDbName}.dbo.{strTab1}";
                    sb(strSourceSQL);

                    if (strW != null)
                    {
                        strSourceSQL = "WHERE " + strW;
                        sb(strSourceSQL);
                    }

                    if (bolIdentity)
                    {
                        sb("");
                        sb($"SET IDENTITY_INSERT {v.DestDbName}.dbo.{strTab2} OFF;");
                        sb("");
                    }




                    sb("");
                    sb("GO");

                    //var dt = _f.FBL.GetDataTable(strSourceSQL, v.SourceConnectString);


                }

                Handle_Update_Zbytek1(v);
                Handle_Update_Zbytek_Role(v);
                Handle_Reports(v);
                Handle_Update_Zbytek_KontaktniOsoby(v);
                handle_Finish(v);

                v.Vysledek = _sb.ToString();


                v.Message = "Hotovo";
            }



            return View(v);
        }

        private void Handle_Pre1(MigrationViewModel v)
        {
            string s = BO.Code.File.GetFileContent($"{_f.App.RootUploadFolder}\\_distribution\\sys\\migration_pre1.sql");
            s = s.Replace("zdrojovadb", v.SourceDbName);
            sbgo(s);
            sbgo($"USE {v.DestDbName}");
        }
        private void Handle_Docs(MigrationViewModel v)
        {
            var s = $"SET IDENTITY_INSERT o17DocMenu ON; INSERT INTO o17DocMenu(o17ID,o17Name,o17DateUpdate,o17UserInsert,o17UserUpdate,x01ID,o17ValidFrom,o17ValidUntil) VALUES(1,'DOC',GETDATE(),'guru','guru',{v.DestX01ID},convert(datetime,'01.01.2020',104),convert(datetime,'01.01.3000',104)); SET IDENTITY_INSERT o17DocMenu OFF";
            sbgo(s);

            sbgo($"SET IDENTITY_INSERT o18DocType ON;");
            s = $"INSERT INTO o18DocType(o18ID,o17ID,o18Name,o18DateUpdate,o18UserInsert,o18UserUpdate,x01ID,o18ValidFrom,o18ValidUntil,o18NotepadTab,o18EntryNameFlag,o18EntryCodeFlag)";


            sbgo($"SET IDENTITY_INSERT o18DocType OFF;");

        }
        private void Handle_Reports(MigrationViewModel v)
        {
            string s = BO.Code.File.GetFileContent($"{_f.App.RootUploadFolder}\\_distribution\\sys\\migration_reports.sql");
            
            sbgo(s);
            
        }
        private void Handle_CreateX01(MigrationViewModel v)
        {
            string s = null;


            s = $"INSERT INTO {v.DestDbName}.dbo.x01License(x01ID,x01Name,x01AppName,x01LoginDomain,x01CountryCode,x01DateInsert,x01UserInsert,x01ValidFrom,x01ValidUntil,x01DateUpdate,x01UserUpdate,x01IsAllowPasswordRecovery) VALUES({v.DestX01ID},'hovado','hovado','{v.x01LoginDomain}','{v.CountryCode}',GETDATE(),'guru',GETDATE(),convert(datetime,'01.01.3000',104),GETDATE(),'guru',1)";

            sbgo(s);

            s = $"INSERT INTO {v.DestDbName}.dbo.x04NotepadConfig(x01ID,x04Name,x04PlaceHolder,x04ImageMaxSize,x04FileMaxSize,x04FileAllowedTypes,x04ImageAllowedTypes,x04DateUpdate,x04UserInsert,x04UserUpdate) VALUES({v.DestX01ID},'Notepad editor','Notepad nabízí úžasné funkce!  Vložte Word/Excel obsah, nahrávejte přílohy, obrázky, videa, odkazy na stránky, tabulky...',3145728,8388608,'pdf,docx,doc,xlsx,xls,txt,csv,msg,eml,zfo','png,jpg,jpeg,gif,bmp',GETDATE(),'guru','guru')";
            sbgo(s);

            s = $"UPDATE {v.DestDbName}.dbo.x01License SET x01Guid=NEWID(),x01ApiKey=LEFT(NEWID(),8), x01IsCapacityFaNefa=0,x01IsAllowDuplicity_RegID=0,x01IsAllowDuplicity_VatID=0,x01ImportCnb_j27Codes='EUR', x01LimitUsers=99,x01RobotLogin='admin@{v.x01LoginDomain}'";
            sb(s);
            sb($",x01Name=(select x35Value FROM {v.SourceDbName}.dbo.x35GlobalParam WHERE x35Key='AppName')");
            sb($",x01AppName=(select x35Value FROM {v.SourceDbName}.dbo.x35GlobalParam WHERE x35Key='AppName')");
            sb($",x01CountryCode=(select x35Value FROM {v.SourceDbName}.dbo.x35GlobalParam WHERE x35Key LIKE 'COUNTRY_CODE')");
            sb($",x01Round2Minutes=(select x35Value FROM {v.SourceDbName}.dbo.x35GlobalParam WHERE x35Key LIKE 'Round2Minutes')");
            sb($",x01InvoiceMaturityDays=(select x35Value FROM {v.SourceDbName}.dbo.x35GlobalParam WHERE x35Key LIKE 'DefMaturityDays')");
            sb($",j27ID=(select x35Value FROM {v.SourceDbName}.dbo.x35GlobalParam WHERE x35Key LIKE 'j27ID_Domestic')");
            sb($",x15ID=(select x35Value FROM {v.SourceDbName}.dbo.x35GlobalParam WHERE x35Key LIKE 'x15ID')");
            sb($",x04ID_Default=(select top 1 x04ID FROM {v.DestDbName}.dbo.x04NotepadConfig)");
            sb($",x01BillingLang1=(select p87Name FROM {v.SourceDbName}.dbo.p87BillingLanguage WHERE p87LangIndex=1)");
            sb($",x01BillingLang2=(select p87Name FROM {v.SourceDbName}.dbo.p87BillingLanguage WHERE p87LangIndex=2)");
            sb($",x01BillingLang3=(select p87Name FROM {v.SourceDbName}.dbo.p87BillingLanguage WHERE p87LangIndex=3)");
            sb($",x01BillingLang4=(select p87Name FROM {v.SourceDbName}.dbo.p87BillingLanguage WHERE p87LangIndex=4)");
            sb($",x01BillingFlag1=(select p87Icon FROM {v.SourceDbName}.dbo.p87BillingLanguage WHERE p87LangIndex=1)");
            sb($",x01BillingFlag2=(select p87Icon FROM {v.SourceDbName}.dbo.p87BillingLanguage WHERE p87LangIndex=2)");
            sb($",x01BillingFlag3=(select p87Icon FROM {v.SourceDbName}.dbo.p87BillingLanguage WHERE p87LangIndex=3)");
            sb($",x01BillingFlag4=(select p87Icon FROM {v.SourceDbName}.dbo.p87BillingLanguage WHERE p87LangIndex=4)");

            sb("");

            if (string.IsNullOrEmpty(v.x01LoginDomain))
            {
                sb($"UPDATE {v.DestDbName}.dbo.x01License SET x01RobotLogin='admin',x01LoginDomain=null");
            }

            sb("GO");
            sb("");
        }

        private void Handle_Update_Zbytek1(MigrationViewModel v)
        {
            sb("");
            sb($"---Zbytek #1---");
            sb("");


            string s = $"UPDATE a set p28BillingLangIndex=c.p87LangIndex FROM {v.DestDbName}.dbo.p28Contact a INNER JOIN {v.SourceDbName}.dbo.p28Contact b ON a.p28ID=b.p28ID INNER JOIN {v.SourceDbName}.dbo.p87BillingLanguage c ON b.p87ID=c.p87ID";
            sbgo(s);
            s = $"UPDATE a set p41BillingLangIndex=c.p87LangIndex FROM {v.DestDbName}.dbo.p41Project a INNER JOIN {v.SourceDbName}.dbo.p41Project b ON a.p41ID=b.p41ID INNER JOIN {v.SourceDbName}.dbo.p87BillingLanguage c ON b.p87ID=c.p87ID";
            sbgo(s);

            s = $"UPDATE {v.DestDbName}.dbo.c22FondCalendar_Date SET c22CountryCode=(select x35Value FROM {v.SourceDbName}.dbo.x35GlobalParam WHERE x35Key LIKE 'COUNTRY_CODE')";
            sbgo(s);

            s = $"UPDATE a set p92TypeFlag=b.p92InvoiceType,p92QrCodeFlag=1,p92FilesTab=0,p92RolesTab=1 FROM {v.DestDbName}.dbo.p92InvoiceType a INNER JOIN {v.SourceDbName}.dbo.p92InvoiceType b ON a.p92ID=b.p92ID";
            sbgo(s);

            s = $"update {v.DestDbName}.dbo.p92InvoiceType set x31ID_Invoice=(select top 1 x31ID from x31Report where x31Entity='p91' and x31Code like 'p91_faktura_vychozi') where p92TypeFlag=1";
            sbgo(s);

            s = $"update {v.DestDbName}.dbo.p92InvoiceType set x31ID_Attachment=(select top 1 x31ID from x31Report where x31Entity='p91' and x31Code like 'p91_faktura_priloha_vychozi')";
            sbgo(s);

            s = $"update p92InvoiceType set x31ID_Invoice=(select top 1 x31ID from x31Report where x31Entity='p91' and x31Code like 'p91_faktura_dobropis') where p92TypeFlag=2";
            sbgo(s);

            s = $"UPDATE a set p86Account=b.p86BankAccount,p86Code=b.p86BankCode FROM {v.DestDbName}.dbo.p86BankAccount a INNER JOIN {v.SourceDbName}.dbo.p86BankAccount b ON a.p86ID=b.p86ID";
            sbgo(s);

            s = $"UPDATE a set j04ID=b.j04ID,j02Login=b.j03Login FROM {v.DestDbName}.dbo.j02User a INNER JOIN {v.SourceDbName}.dbo.j03User b ON a.j02ID=b.j02ID";
            sbgo(s);

            s = $"UPDATE {v.DestDbName}.dbo.p28Contact set p29ID=15 WHERE p29ID IS NULL AND p28ID IN (select p28ID_Client FROM {v.DestDbName}.dbo.p41Project where p42ID=2 OR p41Name like '%interní%')";


        }

        private void handle_Finish(MigrationViewModel v)
        {
            sb("");
            sb($"---Závěr | Finish---");
            sb("");
            for (int i = 1; i <= 4; i++)
            {
                sb($"INSERT INTO {v.DestDbName}.dbo.p07ProjectLevel(p07Level,p07Name,p07NamePlural,p07NameInflection,x01ID,p07DateUpdate,p07UserUpdate,p07UserInsert,p07ValidUntil) VALUES({i},'level{i}','level{i}','level{i}',{v.DestX01ID},GETDATE(),'guru','guru',convert(datetime,'01.01.2000',104))");
            }
            sbgo($"INSERT INTO {v.DestDbName}.dbo.p07ProjectLevel(p07Level,p07Name,p07NamePlural,p07NameInflection,x01ID,p07DateUpdate,p07UserUpdate,p07UserInsert) VALUES(5,'Projekt','Projekty','projektu',{v.DestX01ID},GETDATE(),'guru','guru')");

            sbgo($"UPDATE {v.DestDbName}.dbo.p93InvoiceHeader set p93CountryCode=(select x35Value FROM {v.SourceDbName}.dbo.x35GlobalParam WHERE x35Key LIKE 'COUNTRY_CODE') WHERE p93CountryCode IS NULL");
            sbgo($"UPDATE {v.DestDbName}.dbo.p93InvoiceHeader set p93LogoFile=null");

            sbgo($"UPDATE {v.DestDbName}.dbo.p42ProjectType set x01ID={v.DestX01ID}");

            sbgo($"update {v.DestDbName}.dbo.p42ProjectType set p42RolesTab=3,p42BillingTab=3,p42FilesTab=0,p42ClientTab=3,p42ContactsTab=1");

            sbgo($"update {v.DestDbName}.dbo.p34ActivityGroup set p34TextInternalFlag=1");

            sbgo($"update {v.DestDbName}.dbo.j61TextTemplate set x04ID=1,j61IsPublic=1");

            sbgo($"UPDATE {v.DestDbName}.dbo.o21MilestoneType set o21TypeFlag=1,o21IsP41Compulsory=1 WHERE o21TypeFlag IS NULL");
            sbgo($"UPDATE {v.DestDbName}.dbo.j02User set j02GridCssBitStream=50, j02HesBitStream=133122,j02Name=replace(ISNULL(j02LastName,'')+' '+isnull(j02FirstName,'')+' '+isnull(j02TitleBeforeName,''),'  ',' ')");

            sb("UPDATE c SET p28Street1=a.o38Street,p28City1=a.o38City,p28PostCode1=LEFT(a.o38ZIP,10),p28Country1=a.o38Country");
            sb($"FROM {v.SourceDbName}.dbo.o38Address a INNER JOIN {v.SourceDbName}.dbo.o37Contact_Address b ON a.o38ID=b.o38ID INNER JOIN {v.DestDbName}.dbo.p28Contact c ON b.p28ID=c.p28ID");
            sb("WHERE b.o36ID=1");
            sb("GO");
            sb("UPDATE c SET p28Street2=a.o38Street,p28City2=a.o38City,p28PostCode2=LEFT(a.o38ZIP,10),p28Country2=a.o38Country");
            sb($"FROM {v.SourceDbName}.dbo.o38Address a INNER JOIN {v.SourceDbName}.dbo.o37Contact_Address b ON a.o38ID=b.o38ID INNER JOIN {v.DestDbName}.dbo.p28Contact c ON b.p28ID=c.p28ID");
            sb("WHERE b.o36ID=2");
            sb("GO");

            sb($"UPDATE {v.DestDbName}.dbo.p28Contact set p28Guid=NEWID(),p28BitStream=12314");
            sb($"UPDATE {v.DestDbName}.dbo.p41Project set p07ID=(select p07ID from {v.DestDbName}.dbo.p07ProjectLevel where p07Level=5)");
            sb($"UPDATE {v.DestDbName}.dbo.p41Project set p41BitStream=2458 WHERE p41Name not like '%Interní%'");
            sb($"UPDATE {v.DestDbName}.dbo.p41Project set p41BillingFlag=1 WHERE p41BillingFlag is null");
            sb($"UPDATE {v.DestDbName}.dbo.p28Contact set p28BillingFlag=1 WHERE p28BillingFlag is null");

            sb($"update {v.DestDbName}.dbo.p56Task set x04ID=(select top 1 x01ID FROM {v.DestDbName}.dbo.x04NotepadConfig) where x04ID IS NULL");

            sbgo($"update {v.DestDbName}.dbo.p91Invoice SET p91CodeNumeric = dbo.get_only_numerics(p91Code) WHERE p91IsDraft=0");

            sb("update a set p91Supplier=p93.p93Company,p91Supplier_RegID=p93.p93RegID,p91Supplier_VatID=p93.p93VatID,p91Supplier_ICDPH_SK=p93.p93ICDPH_SK");
            sb(",p91Supplier_Street=p93.p93Street,p91Supplier_City=p93.p93City,p91Supplier_ZIP=p93.p93Zip,p91Supplier_Country=p93.p93Country,p91Supplier_Registration=p93.p93Registration");
            sb($"FROM {v.DestDbName}.dbo.p91Invoice a INNER JOIN {v.DestDbName}.dbo.p92InvoiceType p92 ON a.p92ID=p92.p92ID INNER JOIN {v.DestDbName}.dbo.p93InvoiceHeader p93 ON p92.p93ID=p93.p93ID");
            sb("GO");

            sbgo($"UPDATE {v.DestDbName}.dbo.p31Worksheet set p31ExcludeBillingFlag=1,p31ValidUntil=convert(datetime,'01.01.3000',104) WHERE GETDATE() NOT BETWEEN p31ValidFrom AND p31ValidUntil");

            sbgo($"UPDATE {v.DestDbName}.dbo.j02User set j02HomePageUrl='/p31calendar/Index' WHERE j04ID<>1");
            sbgo("exec dbo.x71_recovery_all");



            var dt = Code.basGuru.Load_DataTable_From_File(_f, "o58GlobalParam");
            foreach (System.Data.DataRow d in dt.Rows)
            {

                var s = $"INSERT INTO {v.DestDbName}.dbo.o58GlobalParam(o58ID,o58Entity,o58Key,o58Name,o58Ordinary,o58DateInsert,o58UserInsert,o58ValidFrom,o58ValidUntil,o58DateUpdate,o58UserUpdate) VALUES({d["o58ID"]},'{d["o58Entity"]}','{d["o58Key"]}','{d["o58Name"]}',0,GETDATE(),'guru',GETDATE(),convert(datetime,'01.01.3000',104),GETDATE(),'guru')";
                sbgo(s);
            }


        }
        private void Handle_Update_Zbytek_KontaktniOsoby(MigrationViewModel v)
        {
            sb("----Kontaktní osoby----");
            sb($"INSERT INTO {v.DestDbName}.dbo.p28Contact(p29ID,p28FirstName,p28LastName,p28TitleBeforeName,p28TitleAfterName,p28CountryCode,p28DateInsert,p28UserInsert,p28DateUpdate,p28UserUpdate,p28ValidFrom,p28ValidUntil,p28ExternalCode,p28Code,p28Name,p28JobTitle,p28IsCompany)");
            sb("select 13,j02FirstName,j02LastName,j02TitleBeforeName,j02TitleAfterName,'CZ',j02DateInsert,j02UserInsert,j02DateUpdate,j02UserUpdate,j02ValidFrom,j02ValidUntil,j02ID,'j02-'+convert(varchar(10),j02ID),j02LastName+' '+j02FirstName,j02JobTitle,0");
            sb($" from {v.SourceDbName}.dbo.j02Person where j02ID IN (select j02ID FROM {v.SourceDbName}.dbo.p30Contact_Person)");
            sb("");
            sb($"update {v.DestDbName}.dbo.p31Worksheet set p28ID_ContactPerson=null WHERE p28ID_ContactPerson is not null");
            sb("");
            sb("update a set p28ID_ContactPerson=d.p28ID");
            sb($"from p31Worksheet a INNER JOIN {v.SourceDbName}.dbo.p31Worksheet b ON a.p31ID=b.p31ID");
            sb($"INNER JOIN {v.SourceDbName}.dbo.p30Contact_Person c ON b.j02ID_ContactPerson=c.j02ID INNER JOIN p28Contact d ON c.j02ID=d.p28ExternalCode");
            sb("WHERE b.j02ID_ContactPerson is not null and d.p28ExternalCode is not null");

        }
        private void Handle_Update_Zbytek_Role(MigrationViewModel v)
        {
            sb("");
            sb($"---Zbytek #2---");
            sb("");
            string s = $"UPDATE {v.DestDbName}.dbo.x67EntityRole set x67Entity='j04' WHERE x67Entity='j03'";
            sbgo(s);
            s = $"DELETE FROM {v.DestDbName}.dbo.x68EntityRole_Permission";
            sbgo(s);
            s = $"DELETE FROM {v.DestDbName}.dbo.x69EntityRole_Assign";
            sbgo(s);
            s = $"DELETE FROM {v.DestDbName}.dbo.x67EntityRole WHERE x67Entity IN ('p28','p91','p90','o22','o17','o23')";
            sbgo(s);

            sb(v.GetSql_ZalozitRoli(v.DestDbName, "p28", "Vlastník záznamu kontaktu", "1100000000000000000000000000000000000000"));
            sb(v.GetSql_ZalozitRoli(v.DestDbName, "p28", "Čtenář záznamu kontaktu", "1000000000000000000000000000000000000000"));
            sb(v.GetSql_ZalozitRoli(v.DestDbName, "p91", "Vlastník záznamu vyúčtování", "1100000000000000000000000000000000000000"));
            sb(v.GetSql_ZalozitRoli(v.DestDbName, "p91", "Čtenář záznamu vyúčtování", "1000000000000000000000000000000000000000"));
            sb(v.GetSql_ZalozitRoli(v.DestDbName, "p90", "Čtenář záznamu zálohy", "1000000000000000000000000000000000000000"));
            sb(v.GetSql_ZalozitRoli(v.DestDbName, "p90", "Vlastník záznamu zálohy", "1100000000000000000000000000000000000000"));
            sb(v.GetSql_ZalozitRoli(v.DestDbName, "o22", "Příjemce termínu", "1000000000000000000000000000000000000000"));
            sb(v.GetSql_ZalozitRoli(v.DestDbName, "o17", "Přístup", "0000000000000000000000000000000000000000"));
            sb(v.GetSql_ZalozitRoli(v.DestDbName, "o23", "Vlastník záznamu", "1100000000000000000000000000000000000000"));
            sb(v.GetSql_ZalozitRoli(v.DestDbName, "o23", "Čtenář záznamu", "1000000000000000000000000000000000000000"));
            
            sbgo($"UPDATE {v.DestDbName}.dbo.x67EntityRole set x67RoleValue='10110110000000000000000000000000000000000000000000' where x67Entity='p41' AND x67Name like 'Manažer projektu'");
            sbgo($"UPDATE {v.DestDbName}.dbo.x67EntityRole set x67RoleValue='00100010000000000000000000000000000000000000000000' where x67Entity='p41' AND x67Name not like '%Manažer projektu%'");
            sbgo($"UPDATE {v.DestDbName}.dbo.x67EntityRole set x67RoleValue='1011000000000000000000000000000000000000' where x67Entity='p56'");

            s = $"INSERT INTO x68EntityRole_Permission(x67ID,x68PermValue) select a.x67ID,x53Value FROM {v.SourceDbName}.dbo.x68EntityRole_Permission a INNER JOIN {v.SourceDbName}.dbo.x53Permission b ON a.x53ID=b.x53ID INNER JOIN {v.SourceDbName}.dbo.x67EntityRole c ON a.x67ID=c.x67ID WHERE c.x29ID IN (141,356,103)";
            sbgo(s);

            s = $"UPDATE {v.DestDbName}.dbo.j02User set j02MyMenuLinks='dashboard|p31calendar|p31dayline|approving|p31totals|absence|p41|p28|p91|p56|x31|j02|p31grid|admin'";
            sbgo(s);

            sb($"INSERT INTO {v.DestDbName}.dbo.x69EntityRole_Assign(x67ID,j02ID,j11ID,x69IsAllUsers,x69RecordPid,x69RecordEntity)");
            sb($"SELECT a.x67ID,a.j02ID,a.j11ID,0,x69RecordPid,{v.SourceDbName}.dbo.get_prefix(x29ID)");
            sb($"FROM {v.SourceDbName}.dbo.x69EntityRole_Assign a INNER JOIN {v.SourceDbName}.dbo.x67EntityRole b ON a.x67ID=b.x67ID");
            sb($"WHERE b.x29ID IN (141,356) AND ISNULL(a.j11ID,0) NOT IN (select j11ID FROM {v.SourceDbName}.dbo.j11Team WHERE j11IsAllPersons=1)");
            sb("");
            sb("GO");
            sb("");

            sb($"INSERT INTO {v.DestDbName}.dbo.x69EntityRole_Assign(x67ID,j02ID,j11ID,x69IsAllUsers,x69RecordPid,x69RecordEntity)");
            sb($"SELECT a.x67ID,a.j02ID,NULL,1,x69RecordPid,{v.SourceDbName}.dbo.get_prefix(x29ID)");
            sb($"FROM {v.SourceDbName}.dbo.x69EntityRole_Assign a INNER JOIN {v.SourceDbName}.dbo.x67EntityRole b ON a.x67ID=b.x67ID");
            sb($"WHERE b.x29ID IN (141,356) AND ISNULL(a.j11ID,0) IN (select j11ID FROM {v.SourceDbName}.dbo.j11Team WHERE j11IsAllPersons=1)");
            sb("");
            sb("GO");
            sb("");

            sb($"UPDATE a set j04IsModule_p31=b.j04IsMenu_Worksheet,j04IsModule_p41=b.j04IsMenu_Project,j04IsModule_p28=b.j04IsMenu_Project,j04IsModule_p91=b.j04IsMenu_Invoice,j04IsModule_p90=b.j04IsMenu_Proforma,j04IsModule_o23=b.j04IsMenu_Notepad,j04IsModule_p56=b.j04IsMenu_Task,j04IsModule_x31=b.j04IsMenu_Report,j04IsModule_j02=b.j04IsMenu_People,j04IsModule_p11=1,j04IsModule_Widgets=1");
            sb($"FROM {v.DestDbName}.dbo.j04UserRole a INNER JOIN {v.SourceDbName}.dbo.j04UserRole b ON a.j04ID=b.j04ID");
            sb("");
            sb("GO");


        }


        

        private void sb(string s)
        {
            _sb.AppendLine(s);

        }

        private void sbgo(string s)
        {
            _sb.AppendLine(s);
            _sb.AppendLine("");
            _sb.AppendLine("GO");
            _sb.AppendLine("");
        }

        private List<T> getDistributionJson<T>(string strTabName)
        {
            return BO.Code.basJson.DeserializeList_From_File<T>($"{_f.App.RootUploadFolder}\\_distribution\\{strTabName}.json").ToList();
        }


        private void Handle_One_Table(string strTable, CreateLicenseViewModel v, string oper)
        {
            string strDest = null;
            if (!string.IsNullOrEmpty(v.DestDbName))
            {
                strDest = $"{v.DestDbName}.dbo.";
            }
            if (oper == "delete")
            {
                _sb.AppendLine($"DELETE FROM {strDest}{strTable};");
                return;
            }
            string strSQL = $"SELECT * FROM {strTable}";

            DataTable dt = _f.FBL.GetDataTable(strSQL);

            if (dt.Rows.Count == 0) return;
            bool bolIdentity = true;


            _sb.AppendLine($"----tabulka {strTable}--------");

            var cols = Code.basGuru.GetColumns(dt);

            if (bolIdentity)
            {
                _sb.AppendLine("");
                _sb.AppendLine($"SET IDENTITY_INSERT {strDest}{strTable} ON;");
            }

            foreach (DataRow dbRow in dt.Rows)
            {
                _sb.AppendLine("");
                _sb.AppendLine($"INSERT INTO {strDest}{strTable} (");
                _sb.AppendLine(string.Join(",", cols.Select(p => p.Key)));
                _sb.AppendLine(")");
                _sb.AppendLine("VALUES (");

                for (int i = 0; i < cols.Count(); i++)
                {
                    if (i > 0) _sb.Append(",");
                    string strField = cols[i].Key;
                    string strValue = Code.basGuru.GVN(dbRow[strField], strField, v.DestX01ID);

                    if (strField == "x01ID")
                    {
                        strValue = v.DestX01ID.ToString();
                    }
                    if (strField == "b01ID")
                    {
                        strValue = dbRow[strField].ToString();
                    }
                    if (strField == strTable.Substring(0, 3) + "UserInsert" || strField == strTable.Substring(0, 3) + "UserUpdate")
                    {
                        strValue = "'dump'";
                    }

                    _sb.Append(strValue);
                }
                _sb.AppendLine(");");



            }

            if (bolIdentity)
            {
                _sb.AppendLine("");
                _sb.AppendLine($"SET IDENTITY_INSERT {strDest}{strTable} OFF");
            }

        }

    }
}
