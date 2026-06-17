
using Microsoft.AspNetCore.Mvc;
using UI.Models;
using UI.Models.Guru;

using System.Text.Json;
using DocumentFormat.OpenXml.Drawing;
using System.Data;


namespace UI.Controllers.guru
{
    public class MigrationController : Controller
    {
        private BL.Factory _f;
        private string _CountryCode { get; set; }

        public MigrationController(BL.Factory f)
        {
            _f = f;
        }
        public IActionResult Index()
        {
            var v = new MigrationViewModel() { DestPassword = DateTime.Now.ToString().Substring(0, 1) + "." + BO.Code.Bas.GetGuid().Substring(0, 5) + "X" };



            return ViewOnlyForVerified(v);
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

                if (v.DestConnectString == v.SourceConnectString)
                {
                    v.Message = "DestConnectString = SourceConnectString!";
                    return View(v);
                }
                if (!v.DestConnectString.Contains("a7"))
                {
                    v.Message = "Název cílové databáze musí začínem na 'a7'!";
                    return View(v);
                }

                
                _f.InhaleUserByLogin($"admin@{v.x01LoginDomain}");  //v databázi musí existovat takový admin login
                Migration_TryClearDataBeforeMigration(v);
                Migration_CreateLicense(v);

                var dtX29 = _f.FBL.GetDataTable("select * from x29Entity", v.SourceConnectString);
                var lisX90 = _f.FBL.GetListX90().Where(p => p.x90IsUse == true);
                //var lisX90 = getDistributionJson<BO.x90Migration>("x90Migration").Where(p => p.x90IsUse).OrderBy(p => p.x90Ordinary);

                foreach (var c in lisX90)
                {
                    string strTab1 = c.x90Table1;
                    BO.Code.File.LogInfo("Migration, x90Table1: " + strTab1);
                    string strTab2 = c.x90Table2;
                    if (strTab1 == "p31Worksheet" || strTab1 == "p31WorkSheet_FreeField" || strTab1 == "p39WorkSheet_Recurrence_Plan" || strTab1 == "p81InvoiceAmount" || strTab1 == "c11StatPeriod" || strTab1 == "c22FondCalendar_Date")
                    {
                        continue;
                    }
                    string strW = null;
                    if (c.x90SqlCondition1 != null)
                    {
                        strW = c.x90SqlCondition1;
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

                    bool bolIdentity = true; string s = ""; bool bolX01ID = false;
                    if (strFieldsMissing1 != null && strFieldsMissing1.Contains("x01ID"))
                    {
                        bolX01ID = true;
                    }
                    if (strTab1 == "c11StatPeriod" || strTab1.Contains("_FreeField"))
                    {
                        bolIdentity = false;
                    }
                    if (bolIdentity)
                    {
                        s = $"SET IDENTITY_INSERT {strTab2} ON;";
                    }

                    s += $" INSERT INTO {strTab2}";
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
                        case "c22FondCalendar_Date":
                            s += ",c22CountryCode";
                            break;
                        case "j02Person":
                            s += ",j04ID,j02Login";
                            break;
                        case "x28EntityField":
                            s += ",x28Entity";
                            break;
                        case "p86BankAccount":
                            s += ",p86Account,p86Code";
                            break;
                        case "p92InvoiceType":
                            s += ",p92TypeFlag";
                            break;
                        case "x40MailQueue":
                            s += ",x40EmlFolder,x40RecordEntity,x40DatetimeProcessed,x40Status";
                            break;
                        case "p28Contact":
                            s += ",p28ShortName,p28BillingLangIndex";
                            break;
                        case "x38CodeLogic":
                            s += ",x38Entity";
                            break;
                        case "b01WorkflowTemplate":
                            s += ",b01Entity";
                            break;

                    }
                    s += ")";

                    string strSourceSQL = $"select * from {strTab1}";
                    if (strW != null)
                    {
                        strSourceSQL += " WHERE " + strW;
                    }
                    var dt = _f.FBL.GetDataTable(strSourceSQL, v.SourceConnectString);
                    foreach (System.Data.DataRow dbr in dt.Rows)
                    {
                        string strSQL = s + " VALUES(";
                        var fields = BO.Code.Bas.ConvertString2List(strMapping1);
                        for (int i = 0; i < fields.Count(); i++)
                        {
                            if (i == 0)
                            {
                                strSQL += v.GVN(dbr[fields[i]], fields[i]);

                            }
                            else
                            {
                                strSQL += "," + v.GVN(dbr[fields[i]], fields[i]);
                            }

                        }
                        if (bolX01ID)
                        {
                            strSQL += "," + v.DestX01ID.ToString();
                        }
                        switch (strTab1)
                        {
                            case "x67EntityRole":
                                strSQL += $",'{v.GetPrefix(Convert.ToInt32(dbr["x29ID"]), dtX29)}'";
                                break;
                            case "c22FondCalendar_Date":
                                strSQL += $",'{_CountryCode}'";
                                break;
                            case "j02Person":
                                strSQL += $",{v.GetUserJ04ID(_f, Convert.ToInt32(dbr["j02ID"]))},{v.GetUserLogin(_f, Convert.ToInt32(dbr["j02ID"]))}";
                                break;
                            case "x28EntityField":
                                strSQL += $",'{v.GetPrefix(Convert.ToInt32(dbr["x29ID"]), dtX29)}'";
                                break;
                            case "p86BankAccount":
                                strSQL += $",{v.SVN(dbr["p86BankAccount"])},{v.SVN(dbr["p86BankCode"])}";
                                break;
                            case "p92InvoiceType":
                                strSQL += $",{dbr["p92InvoiceType"]}";
                                break;

                            case "x40MailQueue":

                                strSQL += $",{v.SVN(dbr["o40ID"])},'{v.GetPrefix(Convert.ToInt32(dbr["x29ID"]), dtX29)}',{v.GVN(dbr["x40WhenProceeded"], "x40WhenProceeded")},3";  //j40ID se načítá z o40ID přes textové pole emlfolder no
                                break;
                            case "p28Contact":
                                strSQL += $",{v.SVN(dbr["p28CompanyShortName"])},{v.GVN(dbr["p87ID"], "p87ID")}";
                                break;
                            case "x38CodeLogic":
                                strSQL += $",'{v.GetPrefix(Convert.ToInt32(dbr["x29ID"]), dtX29)}'";
                                break;
                            case "b01WorkflowTemplate":
                                strSQL += $",'{v.GetPrefix(Convert.ToInt32(dbr["x29ID"]), dtX29)}'";
                                break;


                        }

                        strSQL += ")";
                        if (bolIdentity)
                        {
                            strSQL += $"; SET IDENTITY_INSERT {strTab2} OFF";
                        }

                        if (!_f.FBL.RunSql(strSQL, null, v.DestConnectString))
                        {
                            v.Errors.Add(strSQL);
                        }
                    }

                }

                Migration_HandleUserRoles(v, dtX29);
                Migration_HandleTypKontaktu(v);
                Migration_HandleDocs(v, dtX29);
                Migration_UpdateLicense(v);
                //Migration_Widgets(v);
                Migration_Reports(v);
                //Migration_HandleWorkflowHistory(v, dtX29);
                Migration_Finish(v);
                v.Message = "Hotovo";
            }



            return View(v);
        }

        private void Migration_CreateLicense(MigrationViewModel v)
        {
            var dt = _f.FBL.GetDataTable($"select * from x01License WHERE x01ID={v.DestX01ID}", v.DestConnectString);
            if (dt.Rows.Count == 0)
            {
                _f.FBL.RunSql($"INSERT INTO x01License(x01ID,x01Name,x01AppName,x01LoginDomain,x01CountryCode,x01DateInsert,x01UserInsert,x01ValidFrom,x01ValidUntil,x01DateUpdate,x01UserUpdate,x01IsAllowPasswordRecovery) VALUES({v.DestX01ID},'hovado','hovado','hovado','CZ',GETDATE(),'guru',GETDATE(),convert(datetime,'01.01.3000',104),GETDATE(),'guru',1)", null, v.DestConnectString);
            }
            dt = _f.FBL.GetDataTable("select * from x35GlobalParam", v.SourceConnectString);
            string strAppName = null; string strCountryCode = "CZ"; string strGUID = BO.Code.Bas.GetGuid(); int intRoundMinutes = 5; int intMaturityDays = 10;
            foreach (System.Data.DataRow dbRow in dt.Rows)
            {
                switch (dbRow["x35Key"])
                {
                    case "AppName":
                        strAppName = dbRow["x35Value"].ToString();
                        break;

                    case "COUNTRY_CODE":
                        strCountryCode = dbRow["x35Value"].ToString();
                        _CountryCode = strCountryCode;
                        break;
                    case "Round2Minutes":
                        intRoundMinutes = Convert.ToInt32(dbRow["x35Value"]);
                        break;
                    case "DefMaturityDays":
                        intMaturityDays = Convert.ToInt32(dbRow["x35Value"]);
                        break;



                }
            }


            _f.FBL.RunSql($"UPDATE x01License set x01IsCapacityFaNefa=0,x01IsAllowDuplicity_RegID=0,x01IsAllowDuplicity_VatID=0, x01InvoiceMaturityDays={intMaturityDays}, x01ImportCnb_j27Codes='EUR', x01LimitUsers=99, x01Round2Minutes={intRoundMinutes},x01ApiKey=ISNULL(x01ApiKey,NEWID()), x01Guid=ISNULL(x01Guid,NEWID()), x01Name='{strAppName}',x01AppName='{strAppName}',x01CountryCode='{strCountryCode}',x01LoginDomain='{v.x01LoginDomain}' WHERE x01ID={v.DestX01ID}", null, v.DestConnectString);


        }

        //private void Migration_Widgets(MigrationViewModel v)
        //{
        //    var strPath = _f.App.RootUploadFolder + "\\_distribution\\x55Widget.json";            
        //    var lisX55 = BO.Code.basJson.DeserializeList_From_File<BO.x55Widget>(strPath);

        //    _f.FBL.RunSql("DELETE FROM x56WidgetBinding", null, v.DestConnectString);
        //    _f.FBL.RunSql("DELETE FROM x59WidgetToUser", null, v.DestConnectString);
        //    _f.FBL.RunSql("DELETE FROM x57WidgetToGroup", null, v.DestConnectString);
        //    _f.FBL.RunSql("DELETE FROM x54WidgetGroup", null, v.DestConnectString);
        //    _f.FBL.RunSql("DELETE FROM x55Widget", null, v.DestConnectString);
        //    //foreach (var c in lisX55)
        //    //{
        //    //    string s = "SET IDENTITY_INSERT x55Widget ON;";
        //    //    s+= "INSERT INTO x55Widget(x55ID,x55Name,x55Code,x55Ordinal,x55Content,x55TableSql,x55TableColHeaders,x55TableColTypes,x55ChartSql,x55ChartHeaders,x55ChartType,x55UserInsert,x55UserUpdate,x55DateUpdate,x01ID,x55ChartColors,x55Category,x55TableColTotals)";
        //    //    s += " VALUES(";
        //    //    s += $"{c.x55ID},{v.SVN(c.x55Name)},{v.SVN(c.x55Code)},{c.x55Ordinal},{v.SVN(c.x55Content)},{v.SVN(c.x55TableSql)},{v.SVN(c.x55TableColHeaders)},{v.SVN(c.x55TableColTypes)},{v.SVN(c.x55ChartSql)},{v.SVN(c.x55ChartHeaders)},{v.SVN(c.x55ChartType)},'guru','guru',GETDATE(),{v.DestX01ID},{v.SVN(c.x55ChartColors)},{v.SVN(c.x55Category)},{v.SVN(c.x55TableColTotals)}";
        //    //    s += ")";
        //    //    s += "SET IDENTITY_INSERT x55Widget OFF;";
        //    //    _f.FBL.RunSql(s, null, v.DestConnectString);

        //    //}

        //    Import_One_Table("x55Widget", v);
        //    Import_One_Table("x54WidgetGroup", v);
        //    Import_One_Table("x57WidgetToGroup", v);
        //    _f.FBL.RunSql("INSERT INTO x59WidgetToUser(x54ID,j04ID) select 3,j04ID FROM j04UserRole", null, v.DestConnectString);
        //    _f.FBL.RunSql("INSERT INTO x59WidgetToUser(x54ID,j04ID) select 5,j04ID FROM j04UserRole WHERE j04Name like 'admin%' or j04Name like 'super%'", null, v.DestConnectString);
        //    _f.FBL.RunSql("INSERT INTO x59WidgetToUser(x54ID,j04ID) select 4,j04ID FROM j04UserRole WHERE j04Name like 'admin%' or j04Name like 'super%'", null, v.DestConnectString);

        //    //strPath = _f.App.RootUploadFolder + "\\_distribution\\x54WidgetGroup.json";
        //    //var lisX54 = BO.Code.basJson.DeserializeList_From_File<BO.x54WidgetGroup>(strPath);
        //    //foreach (var c in lisX54)
        //    //{
        //    //    string s = "SET IDENTITY_INSERT x54WidgetGroup ON;";
        //    //    s += "INSERT INTO x54WidgetGroup(x54ID,x54Name,x54Ordinary,x54IsParamP28ID,x54IsParamToday,x54IsAllowAutoRefresh,x54IsAllowSkins,x01ID)";
        //    //    s += " VALUES(";
        //    //    s += $"{c.pid},{v.SVN(c.x55Name)},{v.SVN(c.x55Code)},{c.x55Ordinal},{v.SVN(c.x55Content)},{v.SVN(c.x55TableSql)},{v.SVN(c.x55TableColHeaders)},{v.SVN(c.x55TableColTypes)},{v.SVN(c.x55ChartSql)},{v.SVN(c.x55ChartHeaders)},{v.SVN(c.x55ChartType)},'guru','guru',GETDATE(),{v.DestX01ID},{v.SVN(c.x55ChartColors)},{v.SVN(c.x55Category)},{v.SVN(c.x55TableColTotals)}";
        //    //    s += ")";
        //    //    _f.FBL.RunSql(s, null, v.DestConnectString);

        //    //}

        //}
        private void Migration_Reports(MigrationViewModel v)
        {
            var strPath = _f.App.RootUploadFolder + "\\_distribution\\x31Report.json";
            string s = BO.Code.File.GetFileContent(strPath);
            var lisX31 = JsonSerializer.Deserialize<List<BO.x31Report>>(s);

            _f.FBL.RunSql("UPDATE p92InvoiceType SET x31ID_Invoice=NULL,x31ID_Attachment=NULL,x31ID_Letter=NULL", null, v.DestConnectString);
            _f.FBL.RunSql("UPDATE p89ProformaType SET x31ID=NULL,x31ID_Payment=NULL", null, v.DestConnectString);


            _f.FBL.RunSql("DELETE FROM o27Attachment WHERE o27Entity='x31' AND o27RecordPid IN (select x31ID FROM x31Report WHERE x31UserInsert LIKE 'guru')", null, v.DestConnectString);
            _f.FBL.RunSql("DELETE FROM x31Report WHERE x31UserInsert LIKE 'guru'", null, v.DestConnectString);

            string strDestDir = _f.App.RootUploadFolder + "\\" + v.x01LoginDomain + "\\x31";
            if (!System.IO.Directory.Exists(strDestDir))
            {
                System.IO.Directory.CreateDirectory(strDestDir);
            }

            foreach (var rec in lisX31)
            {
                _f.FBL.RunSql($"INSERT INTO x31Report(x31Entity,x31FormatFlag,x31Code,x31Name,x31Ordinary,x31IsPeriodRequired,x31UserInsert,x31UserUpdate,x31DateUpdate,x01ID) VALUES({v.SVN(rec.x31Entity)},{(int)rec.x31FormatFlag},'{rec.x31Code}','{rec.x31Name}',{rec.x31Ordinary},{BO.Code.Bas.GB(rec.x31IsPeriodRequired)},'guru','guru',GETDATE(),{v.DestX01ID})", null, v.DestConnectString);
                if (rec.j25Code != null && rec.x31Code != null)
                {
                    int intJ25ID = v.GetJ25ID(_f, rec.j25Code);
                    if (intJ25ID > 0)
                    {
                        _f.FBL.RunSql($"UPDATE x31Report SET j25ID={intJ25ID} WHERE x31Code='{rec.x31Code}'", null, v.DestConnectString);
                    }

                }



                if (System.IO.File.Exists(_f.App.RootUploadFolder + "\\_distribution\\trdx\\" + rec.ReportFileName))
                {
                    System.IO.File.Copy(_f.App.RootUploadFolder + "\\_distribution\\trdx\\" + rec.ReportFileName, strDestDir + "\\" + rec.ReportFileName, true);
                    s = "INSERT INTO o27Attachment(o27Entity,o27RecordPid,o27OriginalFileName,o27FileExtension,o27ArchiveFileName,o27ArchiveFolder,o27ContentType,o27UserInsert,o27UserUpdate,o27DateUpdate,o27Guid,x01ID)";
                    s += $" VALUES('x31',{v.GetX31ID(_f, rec.x31Code)},'{rec.ReportFileName}','.trdx','{rec.ReportFileName}','x31','application/trdx','guru','guru',GETDATE(),NEWID(),{v.DestX01ID})";
                    _f.FBL.RunSql(s, null, v.DestConnectString);
                }

            }

            _f.FBL.RunSql("UPDATE x31Report set x31IsAllowPfx=1 WHERE x31Entity IN ('p91','p90') AND (x31Code like 'p91_faktura_vycho%' OR x31Code like '%dobropis%' OR x31Code like '%p90_zaloha%')", null, v.DestConnectString);
            _f.FBL.RunSql("UPDATE x31Report set x31IsAllowPfx=1 WHERE x31Entity IN ('p82')", null, v.DestConnectString);
            _f.FBL.RunSql("UPDATE j25ReportCategory SET j25ValidUntil=convert(datetime,'01.01.3000',104)", null, v.DestConnectString);
            _f.FBL.RunSql("UPDATE j25ReportCategory SET j25ValidUntil=GETDATE() WHERE j25ID NOT IN (select j25ID FROM x31Report WHERE j25ID IS NOT NULL)", null, v.DestConnectString);


        }

        private void Migration_Finish(MigrationViewModel v)
        {
            if (_f.FBL.GetDataTable("select * from p07ProjectLevel", v.DestConnectString).Rows.Count == 0)
            {
                for (int i = 1; i <= 4; i++)
                {
                    _f.FBL.RunSql($"INSERT INTO p07ProjectLevel(p07Level,p07Name,p07NamePlural,p07NameInflection,x01ID,p07DateUpdate,p07UserUpdate,p07UserInsert,p07ValidUntil) VALUES({i},'level{i}','level{i}','level{i}',{v.DestX01ID},GETDATE(),'guru','guru',convert(datetime,'01.01.2000',104))", null, v.DestConnectString);
                }

                _f.FBL.RunSql($"INSERT INTO p07ProjectLevel(p07Level,p07Name,p07NamePlural,p07NameInflection,x01ID,p07DateUpdate,p07UserUpdate,p07UserInsert) VALUES(5,'Projekt','Projekty','projektu',{v.DestX01ID},GETDATE(),'guru','guru')", null, v.DestConnectString);

            }

            _f.FBL.RunSql($"UPDATE p93InvoiceHeader set p93CountryCode='{_CountryCode}' WHERE p93CountryCode IS NULL", null, v.DestConnectString);

            _f.FBL.RunSql("UPDATE p42ProjectType set p07ID=(select p07ID FROM p07ProjectLevel WHERE p07Level=5)", null, v.DestConnectString);
            _f.FBL.RunSql("UPDATE o21MilestoneType set o21TypeFlag=1,o21IsP41Compulsory=1 WHERE o21TypeFlag IS NULL", null, v.DestConnectString);
            _f.FBL.RunSql("UPDATE j02User set j02GridCssBitStream=50, j02HesBitStream=133122,j02Name=replace(ISNULL(j02LastName,'')+' '+isnull(j02FirstName,'')+' '+isnull(j02TitleBeforeName,''),'  ',' ')", null, v.DestConnectString);

            string s = null;
            var dt = _f.FBL.GetDataTable("SELECT * FROM o40SmtpAccount", v.SourceConnectString);
            foreach (System.Data.DataRow d in dt.Rows)
            {
                s = "SET IDENTITY_INSERT j40MailAccount ON;";
                s += " INSERT INTO j40MailAccount(j40ID,j40SmtpName,j40UsageFlag,j40SmtpHost,j40SmtpPort,j40SmtpEmail,j40DateInsert,j40UserInsert,j40DateUpdate,j40UserUpdate,j40ValidFrom,j40ValidUntil";
                s += ",j40SmtpLogin,j40SmtpPassword,j40SmtpUseDefaultCredentials,j40SmtpEnableSsl,j40SslModeFlag,j02ID,x01ID";
                s += ") VALUES(";
                s += $"{d["o40ID"]},{v.SVN(d["o40Name"])},2,{v.SVN(d["o40Server"])},{v.GVN(d["o40Port"], "o40Port")},{v.SVN(d["o40EmailAddress"])},{v.GVN(d["o40DateInsert"], "o40DateInsert")},{v.GVN(d["o40UserInsert"], "o40UserInsert")},{v.GVN(d["o40DateUpdate"], "o40DateUpdate")},{v.GVN(d["o40UserUpdate"], "o40UserUpdate")},{v.GVN(d["o40ValidFrom"], "o40ValidFrom")},{v.GVN(d["o40ValidUntil"], "o40ValidUntil")}";

                string strUseDefCred = "0";
                if (v.GVN(d["o40IsVerify"], "o40IsVerify") == "0")
                {
                    strUseDefCred = "1";
                }
                string strIsSSL = "0";
                string strSslModeFlag = v.GVN(d["o40SslModeFlag"], "o40SslModeFlag");
                if (strSslModeFlag != "0")
                {
                    strIsSSL = "1";
                }

                s += $",{v.SVN(d["o40Login"])},{v.SVN(d["o40Password"])},{strUseDefCred},{strIsSSL},{strSslModeFlag},{v.GVN(d["j02ID_Owner"], "j02ID_Owner")},{v.DestX01ID}";
                s += ");";
                s += "SET IDENTITY_INSERT j40MailAccount OFF;";
                _f.FBL.RunSql(s, null, v.DestConnectString);
            }
            _f.FBL.RunSql($"UPDATE x40MailQueue set j40ID=convert(int,x40EmlFolder),x40EmlFolder=null WHERE x40EmlFolder IS NOT NULL", null, v.DestConnectString);

            dt = _f.FBL.GetDataTable("SELECT a.*,b.p28ID FROM o38Address a INNER JOIN o37Contact_Address b ON a.o38ID=b.o38ID WHERE b.o36ID=1", v.SourceConnectString);
            foreach (System.Data.DataRow d in dt.Rows)
            {
                _f.FBL.RunSql("UPDATE p28Contact SET p28Street1=@street,p28City1=@city,p28PostCode1=@psc,p28Country1=@country WHERE p28ID=@p28id", new { street = d["o38Street"], city = d["o38City"], p28id = d["p28ID"], psc = d["o38ZIP"], country = d["o38Country"] }, v.DestConnectString);
            }
            dt = _f.FBL.GetDataTable("SELECT a.*,b.p28ID FROM o38Address a INNER JOIN o37Contact_Address b ON a.o38ID=b.o38ID WHERE b.o36ID=2", v.SourceConnectString);
            foreach (System.Data.DataRow d in dt.Rows)
            {
                _f.FBL.RunSql("UPDATE p28Contact SET p28Street2=@street,p28City2=@city,p28PostCode2=@psc,p28Country2=@country WHERE p28ID=@p28id", new { street = d["o38Street"], city = d["o38City"], p28id = d["p28ID"], psc = d["o38ZIP"], country = d["o38Country"] }, v.DestConnectString);
            }

            _f.FBL.RunSql("UPDATE p28Contact set p28Guid=NEWID(),p28BitStream=12314", null, v.DestConnectString);
            _f.FBL.RunSql("UPDATE p41Project set p41BitStream=2458 WHERE p41Name not like '%Interní%'", null, v.DestConnectString);
            _f.FBL.RunSql("UPDATE p41Project set p41BillingFlag=1 WHERE p41BillingFlag is null", null, v.DestConnectString);
            _f.FBL.RunSql("UPDATE p28Contact set p28BillingFlag=1 WHERE p28BillingFlag is null", null, v.DestConnectString);

            _f.FBL.RunSql("update p91Invoice SET p91CodeNumeric = dbo.get_only_numerics(p91Code) WHERE p91IsDraft=0", null, v.DestConnectString);

            s = "update a set p91Supplier=p93.p93Company,p91Supplier_RegID=p93.p93RegID,p91Supplier_VatID=p93.p93VatID,p91Supplier_ICDPH_SK=p93.p93ICDPH_SK";
            s += ",p91Supplier_Street=p93.p93Street,p91Supplier_City=p93.p93City,p91Supplier_ZIP=p93.p93Zip,p91Supplier_Country=p93.p93Country,p91Supplier_Registration=p93.p93Registration";
            s += " FROM p91Invoice a INNER JOIN p92InvoiceType p92 ON a.p92ID=p92.p92ID INNER JOIN p93InvoiceHeader p93 ON p92.p93ID=p93.p93ID";
            _f.FBL.RunSql(s, null, v.DestConnectString);


            _f.FBL.RunSql("UPDATE p31Worksheet set p31ExcludeBillingFlag=1,p31ValidUntil=convert(datetime,'01.01.3000',104) WHERE GETDATE() NOT BETWEEN p31ValidFrom AND p31ValidUntil", null, v.DestConnectString);
            //_f.FBL.RunSql("INSERT INTO x36UserParam(j02ID,x36Key,x36Value,x36UserInsert,x36UserUpdate) SELECT j02ID,'grid-period-p31-value',30,'guru','guru' FROM j02User WHERE GETDATE() BETWEEN j02ValidFrom AND j02ValidUntil", null, v.DestConnectString);

            _f.FBL.RunSql("UPDATE j02User set j02HomePageUrl='/p31calendar/Index' WHERE j04ID<>1", null, v.DestConnectString);
            _f.FBL.RunSql("exec dbo.x71_recovery_all", null, v.DestConnectString);

            if (_f.FBL.GetDataTable("select * from o58GlobalParam", v.DestConnectString).Rows.Count == 0)
            {
                dt = Code.basGuru.Load_DataTable_From_File(_f, "o58GlobalParam");
                foreach (System.Data.DataRow d in dt.Rows)
                {
                    _f.FBL.RunSql("INSERT INTO o58GlobalParam(o58ID,o58Entity,o58Key,o58Name,o58Ordinary,o58DateInsert,o58UserInsert,o58ValidFrom,o58ValidUntil,o58DateUpdate,o58UserUpdate) VALUES(@o58ID,@o58Entity,@o58Key,@o58Name,0,GETDATE(),'guru',GETDATE(),convert(datetime,'01.01.3000',104),GETDATE(),'guru')", new { o58ID = d["o58ID"], o58Entity = d["o58Entity"], o58Key = d["o58Key"], o58Name = d["o58Name"] }, v.DestConnectString);

                }
            }




        }

        private void Migration_UpdateLicense(MigrationViewModel v)
        {
            var dt = _f.FBL.GetDataTable("select * from x35GlobalParam", v.SourceConnectString);
            int intJ27ID = 2; int intX15ID = 3;
            foreach (System.Data.DataRow dbRow in dt.Rows)
            {
                switch (dbRow["x35Key"])
                {

                    case "j27ID_Domestic":
                        intJ27ID = Convert.ToInt32(dbRow["x35Value"]);
                        break;
                    case "x15ID":
                        intX15ID = Convert.ToInt32(dbRow["x35Value"]);
                        break;

                }
            }

            _f.FBL.RunSql($"INSERT INTO x04NotepadConfig(x01ID,x04Name,x04PlaceHolder,x04ImageMaxSize,x04FileMaxSize,x04FileAllowedTypes,x04ImageAllowedTypes,x04DateUpdate,x04UserInsert,x04UserUpdate) VALUES({v.DestX01ID},'Notepad editor','Notepad nabízí úžasné funkce!  Vložte Word/Excel obsah, nahrávejte přílohy, obrázky, videa, odkazy na stránky, tabulky...',3145728,8388608,'pdf,docx,doc,xlsx,xls,txt,csv','png,jpg,jpeg,gif,bmp',GETDATE(),'guru','guru')", null, v.DestConnectString);


            _f.FBL.RunSql($"UPDATE x01License set x15ID={intX15ID}, j27ID={intJ27ID}, x04ID_Default=(select top 1 x04ID FROM x04NotepadConfig) WHERE x01ID={v.DestX01ID}", null, v.DestConnectString);

            dt = _f.FBL.GetDataTable("select * from p87BillingLanguage", v.SourceConnectString);
            foreach (System.Data.DataRow d in dt.Rows)
            {
                _f.FBL.RunSql($"UPDATE x01License SET x01BillingLang{d["p87LangIndex"]}='{d["p87Name"]}',x01BillingFlag{d["p87LangIndex"]}='{d["p87Icon"]}' WHERE x01ID={v.DestX01ID}", null, v.DestConnectString);

            }



            //if (!System.IO.Directory.Exists(_f.App.RootUploadFolder + "\\" + v.x01LoginDomain + "\\TEMP"))
            //{
            //    System.IO.Directory.CreateDirectory(_f.App.RootUploadFolder + "\\" + v.x01LoginDomain + "\\TEMP");
            //}
            //if (!System.IO.Directory.Exists(_f.App.RootUploadFolder + "\\" + v.x01LoginDomain + "\\DELETED"))
            //{
            //    System.IO.Directory.CreateDirectory(_f.App.RootUploadFolder + "\\" + v.x01LoginDomain + "\\DELETED");
            //}            

            
            _f.db.ChangeConnectString(v.DestConnectString);
            _f.InhaleUserByLogin($"admin@{v.x01LoginDomain}"); //aktuální uživatel je nutný            
            var recX01 = _f.x01LicenseBL.Load(v.DestX01ID);
            _f.x01LicenseBL.Save(recX01);

        }

        //private void Migration_HandleWorkflowHistory(MigrationViewModel v, System.Data.DataTable dtX29)
        //{
        //    _f.FBL.RunSql("DELETE FROM b05Workflow_History", null, v.DestConnectString);

        //    var dt = _f.FBL.GetDataTable("select a.*,c.j02ID,b.b07Value from b05Workflow_History a INNER JOIN b07Comment b ON a.b07ID=b.b07ID INNER JOIN j03User c ON a.j03ID_Sys=c.j03ID", v.SourceConnectString);
        //    foreach (System.Data.DataRow d in dt.Rows)
        //    {
        //        string s = "INSERT INTO b05Workflow_History(b05RecordEntity,b05RecordPid,j02ID_Sys,b02ID_From,b02ID_To,b05IsCommentOnly,b05IsManualStep,b05Notepad,b05DateInsert,b05UserInsert,b05DateUpdate,b05UserUpdate)";
        //        s += " VALUES(";
        //        s += $"{v.SVN(v.GetPrefix(d["x29ID"], dtX29))},{v.GVN(d["b05RecordPID"], "b05RecordPID")},{v.GVN(d["j02ID"], "j02ID")},{v.GVN(d["b02ID_From"], "b02ID_From")},{v.GVN(d["b02ID_To"], "b02ID_To")},{v.GVN(d["b05IsCommentOnly"], "b05IsCommentOnly")},1,{v.SVN(d["b07Value"])},{v.GVN(d["b05DateInsert"], "b05DateInsert")},{v.SVN(d["b05UserInsert"])},{v.GVN(d["b05DateInsert"], "b05DateInsert")},{v.SVN(d["b05UserInsert"])}";
        //        s += ");";

        //        _f.FBL.RunSql(s, null, v.DestConnectString);
        //    }

        //}
        private void Migration_HandleDocs(MigrationViewModel v, System.Data.DataTable dtX29)
        {
            _f.FBL.RunSql("DELETE FROM o19DocTypeEntity_Binding", null, v.DestConnectString);
            _f.FBL.RunSql("DELETE FROM o16DocType_FieldSetting", null, v.DestConnectString);
            _f.FBL.RunSql("DELETE FROM o20DocTypeEntity", null, v.DestConnectString);
            _f.FBL.RunSql("DELETE FROM o23Doc", null, v.DestConnectString);
            _f.FBL.RunSql("DELETE FROM o18DocType", null, v.DestConnectString);
            _f.FBL.RunSql("DELETE FROM o17DocMenu", null, v.DestConnectString);            
            _f.FBL.RunSql("DELETE FROM o27Attachment", null, v.DestConnectString);

            _f.FBL.RunSql($"SET IDENTITY_INSERT o17DocMenu ON; INSERT INTO o17DocMenu(o17ID,o17Name,o17DateUpdate,o17UserInsert,o17UserUpdate,x01ID,o17ValidFrom,o17ValidUntil) VALUES(1,'DOC',GETDATE(),'guru','guru',{v.DestX01ID},convert(datetime,'01.01.2020',104),convert(datetime,'01.01.3000',104)); SET IDENTITY_INSERT o17DocMenu OFF", null, v.DestConnectString);

            var dt = _f.FBL.GetDataTable("SELECT a.* FROM x18EntityCategory a", v.SourceConnectString);
            foreach (System.Data.DataRow dbRow in dt.Rows)
            {
                
                _f.FBL.RunSql($"SET IDENTITY_INSERT o18DocType ON; INSERT INTO o18DocType(o18ID,o17ID,o18Name,o18DateUpdate,o18UserInsert,o18UserUpdate,x01ID,o18ValidFrom,o18ValidUntil,o18NotepadTab,o18EntryNameFlag,o18EntryCodeFlag) VALUES({dbRow["x18ID"]},1,'{dbRow["x18Name"]}',GETDATE(),'guru','guru',{v.DestX01ID},{v.GVN(dbRow["x18ValidFrom"],"x18ValidFrom")},{v.GVN(dbRow["x18ValidUntil"], "x18ValidUntil")},3,1,2); SET IDENTITY_INSERT o18DocType OFF", null, v.DestConnectString);
            }
            dt = _f.FBL.GetDataTable("SELECT a.*,b.x18ID FROM o23Doc a INNER JOIN x18EntityCategory b ON a.x23ID=b.x23ID", v.SourceConnectString);
            string s = null;
            foreach (System.Data.DataRow d in dt.Rows)
            {
                s = "SET IDENTITY_INSERT o23Doc ON;";
                s += " INSERT INTO o23Doc(o23ID,o18ID,b02ID,j02ID_Owner,o23Guid,o23Name,o23Code,o23BackColor,o23ForeColor,o23UserInsert,o23DateUpdate,o23UserUpdate";
                s += ",o23FreeNumber01,o23FreeNumber02,o23FreeNumber03,o23FreeNumber04,o23FreeNumber05";
                s += ",o23FreeDate01,o23FreeDate02,o23FreeDate03,o23FreeDate04,o23FreeDate05";
                s += ",o23FreeBoolean01,o23FreeBoolean02,o23FreeBoolean03,o23FreeBoolean04,o23FreeBoolean05";
                s += ",o23FreeText01,o23FreeText02,o23FreeText03,o23FreeText04,o23FreeText05,o23FreeText06,o23FreeText07,o23FreeText08,o23FreeText09,o23FreeText10";
                s += ",p28ID_First,p41ID_First,j02ID_First)";
                s += $" VALUES({d["o23ID"]},{d["x18ID"]},{v.GVN(d["b02ID"], "b02ID")},{v.GVN(d["j02ID_Owner"], "j02ID_Owner")},NEWID(),'{d["o23Name"]}',{v.GVN(d["o23Code"], "o23Code")},{v.GVN(d["o23BackColor"], "o23BackColor")},{v.GVN(d["o23ForeColor"], "o23ForeColor")},{v.SVN(d["o23UserInsert"])},{v.GVN(d["o23DateUpdate"],"o23DateUpdate")},{v.SVN(d["o23UserUpdate"])}";
                s += $",{v.GVN(d["o23FreeNumber01"], "o23FreeNumber01")},{v.GVN(d["o23FreeNumber02"], "o23FreeNumber02")},{v.GVN(d["o23FreeNumber03"], "o23FreeNumber03")},{v.GVN(d["o23FreeNumber04"], "o23FreeNumber04")},{v.GVN(d["o23FreeNumber05"], "o23FreeNumber05")}";
                s += $",{v.GVN(d["o23FreeDate01"], "o23FreeDate01")},{v.GVN(d["o23FreeDate02"], "o23FreeDate02")},{v.GVN(d["o23FreeDate03"], "o23FreeDate03")},{v.GVN(d["o23FreeDate04"], "o23FreeDate04")},{v.GVN(d["o23FreeDate05"], "o23FreeDate05")}";
                s += $",{v.GVN(d["o23FreeBoolean01"], "o23FreeBoolean01")},{v.GVN(d["o23FreeBoolean02"], "o23FreeBoolean02")},{v.GVN(d["o23FreeBoolean03"], "o23FreeBoolean03")},{v.GVN(d["o23FreeBoolean04"], "o23FreeBoolean04")},{v.GVN(d["o23FreeBoolean05"], "o23FreeBoolean05")}";
                s += $",{v.GVN(d["o23FreeText01"], "o23FreeText01")},{v.GVN(d["o23FreeText02"], "o23FreeText02")},{v.GVN(d["o23FreeText03"], "o23FreeText03")},{v.GVN(d["o23FreeText04"], "o23FreeText04")},{v.GVN(d["o23FreeText05"], "o23FreeText05")}";
                s += $",{v.GVN(d["o23FreeText06"], "o23FreeText06")},{v.GVN(d["o23FreeText07"], "o23FreeText07")},{v.GVN(d["o23FreeText08"], "o23FreeText08")},{v.GVN(d["o23FreeText09"], "o23FreeText09")},{v.GVN(d["o23FreeText10"], "o23FreeText10")}";
                s += $",{v.GVN(d["p28ID_First"], "p28ID_First")},{v.GVN(d["p41ID_First"], "p41ID_First")},{v.GVN(d["j02ID_First"], "j02ID_First")}";
                s += ");";
                s += "SET IDENTITY_INSERT o23Doc OFF";
                _f.FBL.RunSql(s, null, v.DestConnectString);
            }

            dt = _f.FBL.GetDataTable("SELECT * FROM x16EntityCategory_FieldSetting", v.SourceConnectString);
            foreach (System.Data.DataRow d in dt.Rows)
            {
                s = "SET IDENTITY_INSERT o16DocType_FieldSetting ON;";
                s += "INSERT INTO o16DocType_FieldSetting(o16ID,o18ID,o16IsEntryRequired,o16Name,o16Field,o16Ordinary,o16DataSource,o16IsFixedDataSource,o16IsGridField,o16TextboxWidth,o16TextboxHeight,o16NameGrid,o16IsReportField,o16FieldGroup,o16Format)";
                s += " VALUES(";
                s += $"{d["x16ID"]},{d["x18ID"]},{v.GVN(d["x16IsEntryRequired"], "x16IsEntryRequired")},{v.SVN(d["x16Name"])},{v.SVN(d["x16Field"])},{v.GVN(d["x16Ordinary"], "x16Ordinary")},{v.SVN(d["x16DataSource"])},{v.GVN(d["x16IsFixedDataSource"], "x16IsFixedDataSource")},{v.GVN(d["x16IsGridField"], "x16IsGridField")},{v.GVN(d["x16TextboxWidth"], "x16TextboxWidth")},{v.GVN(d["x16TextboxHeight"], "x16TextboxHeight")},{v.SVN(d["x16NameGrid"])},{v.GVN(d["x16IsReportField"], "x16IsReportField")},{v.SVN(d["x16FieldGroup"])},{v.SVN(d["x16Format"])}";
                s += ")";
                s = "SET IDENTITY_INSERT o16DocType_FieldSetting OFF;";
            }

            dt = _f.FBL.GetDataTable("SELECT * FROM x20EntiyToCategory", v.SourceConnectString);
            foreach (System.Data.DataRow d in dt.Rows)
            {
                s = "SET IDENTITY_INSERT o20DocTypeEntity ON;";
                s += " INSERT INTO o20DocTypeEntity(o20ID,o18ID,o20Entity,o20Name,o20IsEntryRequired,o20IsMultiSelect,o20EntryModeFlag,o20Ordinary,o20IsClosed,o20EntityPageFlag,o20RecTypeEntity,o20RecTypePid)";
                s += " VALUES(";
                s += $"{d["x20ID"]},{d["x18ID"]},{v.SVN(v.GetPrefix(d["x29ID"], dtX29))},{v.GVN(d["x20Name"], "x20Name")},{v.GVN(d["x20IsEntryRequired"], "x20IsEntryRequired")},{v.GVN(d["x20IsMultiSelect"], "x20IsMultiSelect")},1,{v.GVN(d["x20Ordinary"], "x20Ordinary")},{v.GVN(d["x20IsClosed"], "x20IsClosed")},{v.GVN(d["x20EntityPageFlag"], "x20EntityPageFlag")},{v.SVN(v.GetPrefix(d["x29ID_EntityType"], dtX29))},{v.GVN(d["x20EntityTypePID"], "x20EntityTypePID")}";
                s += ");";
                s += "SET IDENTITY_INSERT o20DocTypeEntity OFF";

                _f.FBL.RunSql(s, null, v.DestConnectString);
            }
            _f.FBL.RunSql("UPDATE o20DocTypeEntity set o20Entity='le5' WHERE o20Entity='p41'", null, v.DestConnectString);

            dt = _f.FBL.GetDataTable("SELECT * FROM x19EntityCategory_Binding", v.SourceConnectString);
            foreach (System.Data.DataRow d in dt.Rows)
            {
                s = "INSERT INTO o19DocTypeEntity_Binding(o20ID,o23ID,o19RecordPid,o19UserInsert,o19DateInsert,o19UserUpdate,o19DateUpdate)";
                s += " VALUES(";
                s += $"{d["x20ID"]},{d["o23ID"]},{v.GVN(d["x19RecordPID"], "x19RecordPID")},{v.GVN(d["x19UserInsert"], "x19UserInsert")},{v.GVN(d["x19DateInsert"], "x19DateInsert")},{v.GVN(d["x19UserUpdate"], "x19UserUpdate")},{v.GVN(d["x19DateUpdate"], "x19DateUpdate")}";
                s += ");";

                _f.FBL.RunSql(s, null, v.DestConnectString);
            }
            //dt = _f.FBL.GetDataTable("SELECT a.*,b.x29ID,b.b07RecordPID FROM o27Attachment a LEFT OUTER JOIN b07Comment b ON a.b07ID=b.b07ID WHERE a.x31ID IS NULL and b07Value='upload'", v.SourceConnectString);
            //foreach (System.Data.DataRow d in dt.Rows)
            //{
            //    s = "INSERT INTO o27Attachment(o27Entity,o27RecordPid,o27OriginalFileName,o27Name,o27FileExtension,o27ArchiveFileName,o27ArchiveFolder,o27FileSize,o27ContentType,o27Guid,o27DateInsert,o27UserInsert,o27DateUpdate,o27UserUpdate,x01ID,o27CallerFlag)";
            //    s += " VALUES(";
            //    s += $"{v.SVN(v.GetPrefix(d["x29ID"], dtX29))},{v.GVN(d["b07RecordPID"], "b07RecordPID")},{v.GVN(d["o27OriginalFileName"], "o27OriginalFileName")},{v.GVN(d["o27Name"], "o27Name")},{v.GVN(d["o27FileExtension"], "o27FileExtension")},{v.GVN(d["o27ArchiveFileName"], "o27ArchiveFileName")},{v.GVN(d["o27ArchiveFolder"], "o27ArchiveFolder")},{v.GVN(d["o27FileSize"], "o27FileSize")},{v.GVN(d["o27ContentType"], "o27ContentType")},NEWID(),{v.GVN(d["o27DateInsert"], "o27DateInsert")},{v.GVN(d["o27UserInsert"], "o27UserInsert")},{v.GVN(d["o27DateUpdate"], "o27DateUpdate")},{v.GVN(d["o27UserUpdate"], "o27UserUpdate")},{v.DestX01ID},0";
            //    s += ");";

            //    _f.FBL.RunSql(s, null, v.DestConnectString);
            //}

        }
        private void Migration_HandleTypKontaktu(MigrationViewModel v)
        {
            
            v.ZalozitTypKontaktu(_f, "Klient", 1,1,3,3,3);
            v.ZalozitTypKontaktu(_f, "Dodavatel", 2,2,0,3,3);
            v.ZalozitTypKontaktu(_f, "Potenciální klient", 0, 0, 0, 3,3);
            v.ZalozitTypKontaktu(_f, "Kontaktní osoba", 0,3,0,3,0);
            v.ZalozitTypKontaktu(_f, "Interní", 0,99,3,0,0);
            _f.FBL.RunSql("UPDATE p28Contact set p29ID=(select p29ID FROM p29ContactType where p29Name='Klient' AND p29UserInsert='guru') WHERE p29ID IS NULL", null, v.DestConnectString);

            var dt = _f.FBL.GetDataTable("SELECT * FROM p29ContactType WHERE p29Name like 'Kontaktní osoba'", v.DestConnectString);
            int intP29ID = (int)dt.Rows[0]["p29ID"];
            dt = _f.FBL.GetDataTable("select a.*,b.p28ID from j02Person a INNER JOIN p30Contact_Person b ON a.j02ID=b.j02ID WHERE b.p28ID IS NOT NULL ORDER BY a.j02ID", v.SourceConnectString);
            int intLastJ02ID = 0; int intP28ID_Person = 0;
            foreach (System.Data.DataRow d in dt.Rows)
            {
                int intJ02ID = (int)d["j02ID"];
                int intP28ID = (int)d["p28ID"];

                if (intJ02ID != intLastJ02ID)
                {
                    string strGUID = Guid.NewGuid().ToString();
                    string s = "INSERT INTO p28Contact(p28Guid,p29ID,p28FirstName,p28LastName,p28TitleBeforeName,p28TitleAfterName,p28IsCompany,p28CountryCode,p28DateInsert,p28UserInsert,p28DateUpdate,p28UserUpdate,p28ValidFrom,p28ValidUntil,p28Salutation,p28ExternalCode,p28Code)";
                    s += $" VALUES('{strGUID}',{intP29ID},{v.SVN(d["j02FirstName"])},{v.SVN(d["j02LastName"])},{v.SVN(d["j02TitleBeforeName"])},{v.SVN(d["j02TitleAfterName"])},0,'{_CountryCode}',{v.GVN(d["j02DateInsert"], "j02DateInsert")},{v.GVN(d["j02UserInsert"], "j02UserInsert")},{v.GVN(d["j02DateUpdate"], "j02DateUpdate")},{v.GVN(d["j02UserUpdate"], "j02UserUpdate")},{v.GVN(d["j02ValidFrom"], "j02ValidFrom")},{v.GVN(d["j02ValidUntil"], "j02ValidUntil")},{v.SVN(d["j02Email"])},{v.SVN(d["j02Mobile"])},'j02-{intJ02ID}')";
                    _f.FBL.RunSql(s, null, v.DestConnectString);
                    try
                    {
                        intP28ID_Person = (int)_f.FBL.GetDataTable($"SELECT p28ID FROM p28Contact WHERE p28Guid='{strGUID}'", v.DestConnectString).Rows[0]["p28ID"];
                    }
                    catch
                    {
                        intP28ID_Person = 0;
                    }

                }

                _f.FBL.RunSql($"INSERT INTO p30ContactPerson(p28ID,p28ID_Person,p30Name) VALUES({intP28ID},{intP28ID_Person},{v.SVN(d["j02JobTitle"])})", null, v.DestConnectString);
                intLastJ02ID = intJ02ID;
            }
            _f.FBL.RunSql("INSERT INTO o32Contact_Medium(p28ID,o33ID,o32Value) SELECT p28ID,2,p28Salutation FROM p28Contact WHERE p28Salutation is not null", null, v.DestConnectString);
            _f.FBL.RunSql("INSERT INTO o32Contact_Medium(p28ID,o33ID,o32Value) SELECT p28ID,1,p28ExternalCode FROM p28Contact WHERE p28ExternalCode is not null", null, v.DestConnectString);
            _f.FBL.RunSql("UPDATE p28Contact SET p28Name = replace(ISNULL(p28LastName,'')+' '+isnull(p28FirstName,'')+' '+isnull(p28TitleBeforeName,''),'  ',' '),p28Salutation=null WHERE p28IsCompany=0", null, v.DestConnectString);
        }

        private void Migration_HandleUserRoles(MigrationViewModel v, System.Data.DataTable dtX29)
        {
            _f.FBL.RunSql("UPDATE x67EntityRole set x67Entity='j04' WHERE x67Entity='j03'", null, v.DestConnectString);
            _f.FBL.RunSql("DELETE FROM x68EntityRole_Permission", null, v.DestConnectString);
            _f.FBL.RunSql("DELETE FROM x69EntityRole_Assign", null, v.DestConnectString);

            _f.FBL.RunSql("DELETE FROM x67EntityRole WHERE x67Entity IN ('p28','p91','p90','o22','o17','o23')", null, v.DestConnectString);


            v.ZalozitRoli(_f, "p28", "Vlastník záznamu kontaktu", "1100000000000000000000000000000000000000");
            v.ZalozitRoli(_f, "p28", "Čtenář záznamu kontaktu", "1000000000000000000000000000000000000000");
            v.ZalozitRoli(_f, "p91", "Vlastník záznamu vyúčtování", "1100000000000000000000000000000000000000");
            v.ZalozitRoli(_f, "p91", "Čtenář záznamu vyúčtování", "1000000000000000000000000000000000000000");
            v.ZalozitRoli(_f, "p90", "Čtenář záznamu zálohy", "1000000000000000000000000000000000000000");
            v.ZalozitRoli(_f, "p90", "Vlastník záznamu zálohy", "1100000000000000000000000000000000000000");
            v.ZalozitRoli(_f, "o22", "Příjemce termínu", "1000000000000000000000000000000000000000");
            v.ZalozitRoli(_f, "o17", "Přístup", "0000000000000000000000000000000000000000");
            v.ZalozitRoli(_f, "o23", "Vlastník záznamu", "1100000000000000000000000000000000000000");
            v.ZalozitRoli(_f, "o23", "Čtenář záznamu", "1000000000000000000000000000000000000000");


            _f.FBL.RunSql("UPDATE x67EntityRole set x67RoleValue='1011010000000000000000000000000000000000' where x67Entity='p41' AND x67Name like 'Manažer projektu'", null, v.DestConnectString);
            _f.FBL.RunSql("UPDATE x67EntityRole set x67RoleValue='0010000000000000000000000000000000000000' where x67Entity='p41' AND x67Name not like 'Manažer projektu'", null, v.DestConnectString);
            _f.FBL.RunSql("UPDATE x67EntityRole set x67RoleValue='1011000000000000000000000000000000000000' where x67Entity='p56'", null, v.DestConnectString);

            //v.ZalozitRoli(_f, "p41", "Manažer projektu", "1011010000000000000000000000000000000000");
            //v.ZalozitRoli(_f, "p41", "Člen projektu", "0010000000000000000000000000000000000000");
            //v.ZalozitRoli(_f, "p56", "Řešitel úkolu", "1011000000000000000000000000000000000000");


            //_f.FBL.RunSql("INSERT INTO o28ProjectRole_Workload(p34ID,x67ID,o28EntryFlag,o28PermFlag) SELECT p34ID,(select x67ID FROM x67EntityRole WHERE x67Name='Manažer projektu'),1,3 FROM p34ActivityGroup", null, v.DestConnectString);
            //_f.FBL.RunSql("INSERT INTO o28ProjectRole_Workload(p34ID,x67ID,o28EntryFlag,o28PermFlag) SELECT p34ID,(select x67ID FROM x67EntityRole WHERE x67Name='Člen projektu'),1,0 FROM p34ActivityGroup", null, v.DestConnectString);

            var dt = _f.FBL.GetDataTable("select a.*,b.x53Value from x68EntityRole_Permission a INNER JOIN x53Permission b ON a.x53ID=b.x53ID INNER JOIN x67EntityRole c ON a.x67ID=c.x67ID WHERE c.x29ID IN (141,356,103)", v.SourceConnectString);
            foreach (System.Data.DataRow d in dt.Rows)
            {
                _f.FBL.RunSql($"INSERT INTO x68EntityRole_Permission(x67ID,x68PermValue) VALUES({d["x67ID"]},{d["x53Value"]})", null, v.DestConnectString);
            }


            dt = _f.FBL.GetDataTable("select * from j02User", v.DestConnectString);
            foreach (System.Data.DataRow d in dt.Rows)
            {
                string strHash = new BL.Code.PasswordSupport().GetPasswordHash(v.DestPassword, d["j02Login"].ToString(), Convert.ToInt32(d["j02ID"]));
                _f.FBL.RunSql("UPDATE j02User set j02PasswordHash=@s,j02MyMenuLinks='dashboard|p31calendar|p31dayline|approving|p31totals|absence|p41|p28|p91|p56|x31|j02|p31grid|admin' WHERE j02ID=@j02id", new { s = strHash, j02id = d["j02ID"] }, v.DestConnectString);
            }

            dt = _f.FBL.GetDataTable("select a.*,b.x29ID from x69EntityRole_Assign a INNER JOIN x67EntityRole b ON a.x67ID=b.x67ID WHERE b.x29ID IN (141,356)", v.SourceConnectString);
            foreach (System.Data.DataRow d in dt.Rows)
            {
                string s = "INSERT INTO x69EntityRole_Assign(x67ID,j02ID,j11ID,x69IsAllUsers,x69RecordPid,x69RecordEntity)";
                int intX67ID = (int)d["x67ID"];
                string strAllUsers = "0";

                string strJ11ID = v.GVN(d["j11ID"], "j11ID");
                if (strJ11ID == "1")
                {
                    strJ11ID = "NULL";
                    strAllUsers = "1";
                }

                s += $" VALUES({intX67ID},{v.GVN(d["j02ID"], "j02ID")},{strJ11ID},{strAllUsers},{v.GVN(d["x69RecordPid"], "x69RecordPid")},{v.SVN(v.GetPrefix(d["x29ID"], dtX29))})";
                _f.FBL.RunSql(s, null, v.DestConnectString);

            }

            dt = _f.FBL.GetDataTable("select * from j04UserRole", v.SourceConnectString);
            foreach (System.Data.DataRow d in dt.Rows)
            {
                _f.FBL.RunSql($"UPDATE j04UserRole set j04IsModule_p31={v.BVN(d["j04IsMenu_Worksheet"])},j04IsModule_p41={v.BVN(d["j04IsMenu_Project"])},j04IsModule_p28={v.BVN(d["j04IsMenu_Contact"])},j04IsModule_p91={v.BVN(d["j04IsMenu_Invoice"])},j04IsModule_p90={v.BVN(d["j04IsMenu_Proforma"])},j04IsModule_o23={v.BVN(d["j04IsMenu_Notepad"])},j04IsModule_p56={v.BVN(d["j04IsMenu_Task"])},j04IsModule_x31={v.BVN(d["j04IsMenu_Report"])},j04IsModule_j02={v.BVN(d["j04IsMenu_People"])},j04IsModule_p11=1,j04IsModule_Widgets=1 WHERE j04ID={d["j04ID"]}", null, v.DestConnectString);
            }



        }

        private List<T> getDistributionJson<T>(string strTabName)
        {
            return BO.Code.basJson.DeserializeList_From_File<T>($"{_f.App.RootUploadFolder}\\_distribution\\{strTabName}.json").ToList();
        }

        private void Migration_TryClearDataBeforeMigration(MigrationViewModel v)
        {
            
            
            _f.FBL.RunSql("DELETE FROM x43MailQueue_Recipient", null, v.DestConnectString);
            _f.FBL.RunSql("DELETE FROM x40MailQueue", null, v.DestConnectString);
            _f.FBL.RunSql("DELETE FROM j40MailAccount", null, v.DestConnectString);
            _f.FBL.RunSql("DELETE FROM x36UserParam", null, v.DestConnectString);
            _f.FBL.RunSql("DELETE FROM x56WidgetBinding", null, v.DestConnectString);
            _f.FBL.RunSql("DELETE FROM j90LoginAccessLog", null, v.DestConnectString);
            _f.FBL.RunSql("DELETE FROM j92PingLog", null, v.DestConnectString);
            _f.FBL.RunSql("DELETE FROM j75TheGridState", null, v.DestConnectString);
            _f.FBL.RunSql("DELETE FROM j72TheGridTemplate", null, v.DestConnectString);
            _f.FBL.RunSql("DELETE FROM p30ContactPerson", null, v.DestConnectString);

            _f.FBL.RunSql($"UPDATE x01License set j27ID=null where x01ID={v.DestX01ID}", null, v.DestConnectString);

            //var dtTables = _f.FBL.GetDataTable("select * from x90Migration ORDER BY x90Ordinary DESC"); //tady jsou všechny tabulky - ty x90IsUse=0

            var lisX90 = getDistributionJson<BO.x90Migration>("x90Migration").OrderByDescending(p => p.x90Ordinary);    //tabulky vč. x90IsUse=0
            foreach (var rec in lisX90)
            {
                
                var dt = _f.FBL.GetDataTable($"select TOP 1 * from {rec.x90Table2}", v.DestConnectString);
                if (dt.Rows.Count > 0)
                {
                    string strSQL = $"DELETE FROM {rec.x90Table2}";
                    if (!_f.FBL.RunSql(strSQL, null, v.DestConnectString))
                    {
                        v.Errors.Add(strSQL);
                    }
                }
            }



            _f.FBL.RunSql("DELETE FROM j40MailAccount", null, v.DestConnectString);
        }

        private IActionResult ViewOnlyForVerified(object v)
        {
            return View(v);
            //if (HttpContext.User.Identity.Name == _f.App.GuruLogin)
            //{
            //    //ověřený guru uživatel - nemusí být v j02User
            //    return View(v);
            //}

            //v = new StopPageViewModel() { Message = "Nejsi GURU!" };
            //return View("_StopPage", v);
        }


        private void Import_One_Table(string strTable, MigrationViewModel v)
        {
            System.Data.DataTable dt = Code.basGuru.Load_DataTable_From_File(_f, strTable);
                       
            if (dt.Rows.Count == 0) return;
            bool bolIdentity = true;

            var cols = Code.basGuru.GetColumns(dt);
            foreach (System.Data.DataRow dbRow in dt.Rows)
            {
                var sb = new System.Text.StringBuilder();
                bool bolGO = true;
                
                if (bolGO)
                {
                    if (bolIdentity) sb.AppendLine($"SET IDENTITY_INSERT {strTable} ON;");

                    sb.Append($" INSERT INTO {strTable} (");
                    sb.Append(string.Join(",", cols.Select(p => p.Key)));
                    sb.Append(")");
                    sb.Append(" VALUES(");
                    for (int i = 0; i < cols.Count(); i++)
                    {
                        if (i > 0) sb.Append(",");
                        string strField = cols[i].Key;
                        string strValue = Code.basGuru.GVN(dbRow[strField], strField, v.DestX01ID);
                        if (strField == strTable.Substring(0, 3) + "ID")
                        {
                            strValue = (GetMaxPidValue(strTable, v) + 1).ToString();
                        }

                        if (dbRow[strField] != System.DBNull.Value && v.lisKeys.Where(p => p.Field == strField && p.OrigValue == Convert.ToInt32(dbRow[strField])).Count() > 0)
                        {
                            strValue = v.lisKeys.Where(p => p.Field == strField && p.OrigValue == Convert.ToInt32(dbRow[strField])).First().NewValue.ToString();    //cizí klíč
                        }

                        if (strTable == "b06WorkflowStep" && strField == "b02ID_Target" && dbRow["b02ID_Target"] != System.DBNull.Value)
                        {
                            strValue = v.lisKeys.Where(p => p.Field == "b02ID" && p.OrigValue == Convert.ToInt32(dbRow["b02ID_Target"])).First().NewValue.ToString();    //cizí klíč
                        }
                        if (strTable == "b06WorkflowStep" && strField == "x67ID_Direct" && dbRow["x67ID_Direct"] != System.DBNull.Value && Convert.ToInt32(dbRow["x67ID_Direct"]) != 0)
                        {
                            strValue = v.lisKeys.Where(p => p.Field == "x67ID" && p.OrigValue == Convert.ToInt32(dbRow["x67ID_Direct"])).First().NewValue.ToString();    //cizí klíč
                        }
                        if (strField == "x01ID")
                        {
                            strValue = v.DestX01ID.ToString();
                        }


                        sb.Append(strValue);
                    }
                    sb.Append(");");

                    if (bolIdentity) sb.AppendLine($"; SET IDENTITY_INSERT {strTable} OFF");

                    if (_f.FBL.RunSql(sb.ToString(),null,v.DestConnectString))
                    {

                        var cc = new CreateLicenseKey { Table = strTable, Field = strTable.Substring(0, 3) + "ID" };
                        cc.NewValue = GetMaxPidValue(strTable, v);
                        cc.OrigValue = Convert.ToInt32(dbRow[strTable.Substring(0, 3) + "ID"]);
                        v.lisKeys.Add(cc);

                       
                    }

                    
                }

            }

        }

        private int GetMaxPidValue(string strTable, MigrationViewModel v)
        {
            var dt = _f.FBL.GetDataTable($"select MAX({strTable.Substring(0, 3)}ID) as pid from {strTable}");
            if (dt.Rows.Count > 0)
            {
                if (dt.Rows[0]["pid"] == System.DBNull.Value)
                {
                    return 0;
                }
                return (int)dt.Rows[0]["pid"];
            }
            else
            {
                return 0;
            }

        }

    }
}
