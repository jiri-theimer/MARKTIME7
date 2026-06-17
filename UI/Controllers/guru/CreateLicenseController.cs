using BL;
using BO;
using Microsoft.AspNetCore.Mvc;
using UI.Models.Guru;

namespace UI.Controllers.guru
{
    public class CreateLicenseController : Controller
    {
        private BL.Factory _f;
        private System.Text.StringBuilder _sbLogSql { get; set; }
        private bool _bolTotalCloud { get; set; }

        public CreateLicenseController(BL.Factory f)
        {
            _f = f;
            if (_f.App.HostingMode == BL.Singleton.HostingModeEnum.TotalCloud)
            {
                _bolTotalCloud = true;
            }
        }

        public IActionResult Index(string guru,bool allowprefill)
        {                        
            var v = new CreateLicenseViewModel {guru=guru, AllowPrefill=allowprefill, Rec = new BO.x01License(), IsZakladatKlientaPlusProjekty = true, p28RegID = "47123737",Project1="DPPO + Poradenství",Project2=$"Audit {DateTime.Now.Year}" };
            v.UserPassword = new BL.Code.PasswordSupport().GetRandomPassword();
            if (string.IsNullOrEmpty(v.guru))
            {
                v.Message = "Chybí heslo.";
            }
            return View(v);
        }

        [HttpPost]
        public ActionResult Index(CreateLicenseViewModel v, string oper)
        {
            _f.CurrentUser = new BO.RunningUser() { j02Login = _f.App.GuruLogin };
            if (string.IsNullOrEmpty(v.guru))
            {
                v.Message = "Chybí heslo.";
            }
            if (oper == "postback")
            {
                return View(v);
            }
            if (oper == "Handle_Import_Tabulky")
            {
                Handle_Import_Tabulky(v);
                return View(v);
            }

            if (ModelState.IsValid)
            {
                if (string.IsNullOrEmpty(v.DestDbName))
                {
                    v.Message = "Chybí název cílové databáze!";
                    return View(v);
                }
                if (string.IsNullOrEmpty(v.Rec.x01Name) || string.IsNullOrEmpty(v.Rec.x01AppName))
                {
                    v.Message = "Chybí zadat název licence nebo název aplikace!";
                    return View(v);
                }
                if (string.IsNullOrEmpty(v.UserName) || string.IsNullOrEmpty(v.UserPassword))
                {
                    v.Message = "Chybí zadat Login a heslo prvního uživatele!";
                    return View(v);
                }
                if (string.IsNullOrEmpty(v.FirstName) || string.IsNullOrEmpty(v.LastName) || string.IsNullOrEmpty(v.Email))
                {
                    v.Message = "Chybí zadat Jméno, Příjmení nebo E-mail prvního uživatele!";
                    return View(v);
                }
                //var test = _f.j02UserBL.TestUniqueLoginOverLicenses(v.UserName, 0);
                //if (test.Flag == BO.ResultEnum.Failed)
                //{
                //    v.Message = test.Message;
                //    return View(v);
                //}

                if (_f.App.HostingMode == BL.Singleton.HostingModeEnum.SharedApp)
                {
                    _f.InhaleUserByLogin("admin@marktime.cz");
                }


                var dt = _f.FBL.GetDataTable($"select TOP 1 * from [{v.DestDbName}].dbo.x01License ORDER BY x01ID DESC");

                if (dt.Rows.Count > 0 && _f.App.HostingMode != BL.Singleton.HostingModeEnum.TotalCloud)
                {
                    v.Message = $"V databázi již je založená licence ID={dt.Rows[0]["x01ID"]} [{dt.Rows[0]["x01Name"]}]!";
                    return View(v);
                }
                v.DestX01ID = 1;
                if (dt.Rows.Count > 0)
                {
                    v.DestX01ID = (int)dt.Rows[0]["x01ID"] + 1;
                }


                Handle_Import_Tabulky(v);
                
                Handle_After_Create(v);
                Handle_Reports(v);

                v.Message += "<br>Operace dokončena.";

                return View(v);




            }


            v.Message += "<br>Záznam nebyl uložen.";

            return View(v);
        }
        
        private void Handle_After_Create(CreateLicenseViewModel v)
        {
            _f.db.ChangeDbNameInConnectString(v.DestDbName);
            _f.App.RefreshP07List();
            var dt = _f.FBL.GetDataTable($"select TOP 1 j02Login from [{v.DestDbName}].dbo.j02User");
            string strLogin = dt.Rows[0]["j02Login"].ToString();
            _f.InhaleUserByLogin(strLogin); //aktuální uživatel je nutný
            var recX01 = _f.x01LicenseBL.Load(_f.CurrentUser.x01ID);
            recX01.x01RobotLogin = strLogin;
            _f.x01LicenseBL.Save(recX01);   //založit/aktualizovat souvislosti s licencí
            if (_f.App.HostingMode != BL.Singleton.HostingModeEnum.SharedApp)
            {
                _f.App.lisX01 = _f.x01LicenseBL.GetList(new BO.myQuery("x01"));
            }
            
            _f.db.RunSql("update p51PriceList set p51ValidFrom=@d", new { d = new DateTime(DateTime.Now.Year, 1, 1) });            
            foreach (var c in _f.p51PriceListBL.GetList(new BO.myQueryP51()))
            {                
                _f.db.RunSql("exec dbo.p51_aftersave @p51id,@j02id", new { p51id = c.pid, j02id = _f.CurrentUser.pid });    //aktualizace ceníků
            }
            _f.db.RunSql("update p50OfficePriceList set p50ValidFrom=@d", new { d = new DateTime(DateTime.Now.Year, 1, 1) });

            for (int i = -1; i < 4; i++)
            {
                _f.FBL.RunSql($"exec [{v.DestDbName}].dbo.c11_yearrecovery {DateTime.Now.Year - i}");
            }

            dt = _f.FBL.GetDataTable($"select TOP 1 * from {v.DestDbName}.dbo.c21FondCalendar");
            foreach (System.Data.DataRow dbRow in dt.Rows)
            {
                _f.FBL.RunSql($"exec {v.DestDbName}.dbo.c21_aftersave {dbRow["c21ID"]},3");
            }

            
            var recP93 = _f.p93InvoiceHeaderBL.GetList(new BO.myQuery("p93")).First();
            recP93.p93Name = v.p93Company;
            recP93.p93Company = v.p93Company;
            recP93.p93RegID = v.p93RegID;
            recP93.p93VatID = v.p93VatID;
            recP93.p93Street = v.p93Street;
            recP93.p93City = v.p93City;
            recP93.p93Zip = v.p93Zip;
            if (v.Rec.x01CountryCode == "CZ")
            {
                recP93.p93Country = "Česká republika";
            }
            if (v.Rec.x01CountryCode == "SK")
            {
                recP93.p93Country = "Slovenská republika";
            }
            _f.p93InvoiceHeaderBL.Save(recP93, null);
            
            var recJ02 = _f.j02UserBL.GetList(new BO.myQueryJ02()).First();
            recJ02.j02MyMenuLinks = _f.j02UserBL.GetDefaultMenuLinks(); //výchozí menu uživatele
            recJ02.j02HomePageUrl = null;
            _f.j02UserBL.Save(recJ02, null);
            
            var recP28 = new BO.p28Contact() {p29ID=15,j02ID_Owner=recJ02.pid, p28CountryCode = v.Rec.x01CountryCode,p28IsCompany=true, p28CompanyName = v.p93Company, p28RegID = v.p93RegID, p28VatID = v.p93VatID, p28Street1 = v.p93Street, p28City1 = v.p93City, p28PostCode1 = v.p93Zip, p28ICDPH_SK = v.p93ICDPH_SK };
            recP28.p28Country1 = recP93.p93Country;
            int intP28ID = _f.p28ContactBL.Save(recP28, null, null, null, null, null);
            if (intP28ID > 0)
            {
                var recP41 = new BO.p41Project() { j02ID_Owner = recJ02.pid, p28ID_Client = intP28ID, p41Name = "Interní", p42ID = 5 };
                var lisX69 = new List<BO.x69EntityRole_Assign>()
                {
                    new BO.x69EntityRole_Assign() { x67ID = 13, j02ID = recJ02.pid },   //manažer projektu
                    new BO.x69EntityRole_Assign() { x67ID = 14, x69IsAllUsers = true }, //člen projektu
                    new BO.x69EntityRole_Assign() { x67ID = 107, x69IsAllUsers = true }  //vidí všechny hodiny projektu
                };              
                int intP41ID = _f.p41ProjectBL.Save(recP41, null, lisX69, null);
                if (intP41ID > 0)
                {
                    _f.FBL.RunSql($"UPDATE p32Activity SET p41ID_Absence={intP41ID} WHERE p41ID_Absence IS NOT NULL");
                }
            }
            if (v.IsZakladatKlientaPlusProjekty)
            {
                recP28 = new BO.p28Contact() { j02ID_Owner = recJ02.pid,p29ID = 9, p28CountryCode = v.Rec.x01CountryCode,p28IsCompany=true, p28CompanyName = v.p28CompanyName, p28RegID = v.p28RegID, p28VatID = v.p28VatID, p28Street1 = v.p28Street1, p28City1 = v.p28City1, p28PostCode1 = v.p28PostCode1, p28ICDPH_SK = v.p28ICDPH_SK };
                intP28ID = _f.p28ContactBL.Save(recP28, null, null, null, null, null);
                if (intP28ID > 0 && !string.IsNullOrEmpty(v.Project1))
                {
                    var recP41 = new BO.p41Project() { j02ID_Owner = recJ02.pid,p28ID_Client = intP28ID, p41Name = v.Project1,p42ID=1 };
                    var lisX69 = new List<BO.x69EntityRole_Assign>()
                    {
                        new BO.x69EntityRole_Assign() { x67ID = 13, j02ID = recJ02.pid },   //manažer projektu
                        new BO.x69EntityRole_Assign() { x67ID = 14, x69IsAllUsers = true }  //člen projektu
                    };                   
                    int intP41ID = _f.p41ProjectBL.Save(recP41, null, lisX69,null);
                }
                if (intP28ID > 0 && !string.IsNullOrEmpty(v.Project2))
                {
                    var recP41 = new BO.p41Project() { j02ID_Owner = recJ02.pid, p28ID_Client = intP28ID, p41Name = v.Project2, p42ID = 1 };
                    var lisX69 = new List<BO.x69EntityRole_Assign>()
                    {
                        new BO.x69EntityRole_Assign() { x67ID = 13, j02ID = recJ02.pid },   //manažer projektu
                        new BO.x69EntityRole_Assign() { x67ID = 14, x69IsAllUsers = true }  //člen projektu
                    };                    
                    int intP41ID = _f.p41ProjectBL.Save(recP41, null, lisX69, null);
                }
            }

        }

        private void Handle_Reports(CreateLicenseViewModel v)
        {            
            var lisX31 = _f.x31ReportBL.GetList(new BO.myQueryX31());            
            string strDestDir=$"{_f.App.RootUploadFolder}\\{v.Rec.x01LoginDomain}\\X31";
            
            foreach (var rec in lisX31)
            {
                if (System.IO.File.Exists(_f.App.RootUploadFolder + "\\_distribution\\trdx\\" + rec.ReportFileName))
                {
                    System.IO.File.Copy(_f.App.RootUploadFolder + "\\_distribution\\trdx\\" + rec.ReportFileName, strDestDir + "\\" + rec.ReportFileName, true);
                    
                }
            }
        }

        private void Handle_Import_Tabulky(CreateLicenseViewModel v)
        {
            if (!_bolTotalCloud)
            {
                Import_One_Table("j27Currency", v);
                Import_One_Table("x15VatRateType", v);
                Import_One_Table("x24DataType", v);
                Import_One_Table("j19PaymentType", v);
                Import_One_Table("p70BillingStatus", v);
                Import_One_Table("p71ApproveStatus", v);
                Import_One_Table("p72PreBillingStatus", v);
                Import_One_Table("o33MediumType", v);
                Import_One_Table("o58GlobalParam", v);
                Import_One_Table("p33ActivityInputType", v);
            }
            
            Import_One_Table("x01License", v);

            Import_One_Table("c24DayColor", v);
            Import_One_Table("c21FondCalendar", v);
            Import_One_Table("b20Hlidac", v);

            Import_One_Table("x67EntityRole", v);
            Import_One_Table("x68EntityRole_Permission", v);
            Import_One_Table("j04UserRole", v);
            Import_One_Table("j07PersonPosition", v);

            Import_One_Table("b01WorkflowTemplate", v);
            Import_One_Table("b02WorkflowStatus", v);
            Import_One_Table("b06WorkflowStep", v);
            Import_One_Table("b08WorkflowReceiverToStep", v);

            Import_One_Table("r02CapacityVersion", v);
            Import_One_Table("x04NotepadConfig", v);
            Import_One_Table("o15AutoComplete", v);

            Import_One_Table("p53VatRate", v);

            Import_One_Table("x38CodeLogic", v);
            Import_One_Table("o21MilestoneType", v);
            Import_One_Table("p57TaskType", v);
            Import_One_Table("p07ProjectLevel", v);
            Import_One_Table("p42ProjectType", v);
            Import_One_Table("p80InvoiceAmountStructure", v);

            
            Import_One_Table("p35Unit", v);
            Import_One_Table("p95InvoiceRow", v);

            Import_One_Table("p38ActivityTag", v);
            Import_One_Table("p34ActivityGroup", v);
            Import_One_Table("p32Activity", v);
            Import_One_Table("o28ProjectRole_Workload", v);
            Import_One_Table("p43ProjectType_Workload", v);
            Import_One_Table("p61ActivityCluster", v);
            Import_One_Table("p62ActivityCluster_Item", v);


            Import_One_Table("j25ReportCategory", v);
            Import_One_Table("x31Report", v);
            Import_One_Table("o27Attachment", v);

            Import_One_Table("p29ContactType", v);



            Import_One_Table("p86BankAccount", v);
            Import_One_Table("p93InvoiceHeader", v);
            Import_One_Table("p88InvoiceHeader_BankAccount", v);

            Import_One_Table("p92InvoiceType", v);
            Import_One_Table("p89ProformaType", v);

            Import_One_Table("x55Widget", v);
            Import_One_Table("x54WidgetGroup", v);
            Import_One_Table("x57WidgetToGroup", v);
            Import_One_Table("x59WidgetToUser", v);


            Import_One_Table("o53TagGroup", v);
            Import_One_Table("o51Tag", v);

            Import_One_Table("c26Holiday", v);

            Import_One_Table("j02User", v);

            Import_One_Table("j40MailAccount", v);

            

            Import_One_Table("p51PriceList", v);
            Import_One_Table("p52PriceList_Item", v);

            //Import_One_Table("p07ProjectLevel", v);

            Import_One_Table("o17DocMenu", v);
            Import_One_Table("o18DocType", v);
            Import_One_Table("o16DocType_FieldSetting", v);            
            Import_One_Table("o20DocTypeEntity", v);


            BO.Code.File.WriteText2File($"{_f.App.RootUploadFolder}\\{v.DestDbName}-{v.Rec.x01LoginDomain}-{BO.Code.Bas.GetGuid()}.sql", _sbLogSql.ToString());
        }



        private void Import_One_Table(string strTable, CreateLicenseViewModel v)
        {
            string strSQL = $"select * from [{v.DestDbName}].dbo.{strTable}";

            var dt = _f.FBL.GetDataTable(strSQL);
            if (dt.Rows.Count > 0)
            {
                return; //tabulka již obsahuje data - byla dřive naimportovaná
            }
            wr_sql($"------[{v.DestDbName}].dbo.{strTable}");
            try
            {
                dt = Code.basGuru.Load_DataTable_From_File(_f, strTable);
            }
            catch (Exception ex)
            {
                wr_sql($"------[{v.DestDbName}].dbo.{strTable}: {ex.Message}");
                return;
            }

            if (dt.Rows.Count == 0) return;
            bool bolIdentity = true;
            if (strTable == "o58GlobalParam")
            {
                bolIdentity = false;

            }
            if (strTable == "x01License")
            {
                
                bolIdentity = false;
                dt.Rows[0]["x01ApiKey"] = BO.Code.Bas.GetGuid();
                dt.Rows[0]["x01LoginDomain"] = v.Rec.x01LoginDomain;
                dt.Rows[0]["x01Name"] = v.Rec.x01Name;
                dt.Rows[0]["x01AppName"] = v.Rec.x01AppName;
                dt.Rows[0]["x01AppHost"] = v.Rec.x01AppHost;
                dt.Rows[0]["x01CountryCode"] = v.Rec.x01CountryCode;
                dt.Rows[0]["x01LangIndex"] = v.Rec.x01LangIndex;
                dt.Rows[0]["x01ContactEmail"] = v.Rec.x01ContactEmail;
                dt.Rows[0]["x01ContactName"] = v.Rec.x01ContactName;
                dt.Rows[0]["x01Round2Minutes"] = v.Rec.x01Round2Minutes;
                dt.Rows[0]["x01LimitUsers"] = v.Rec.x01LimitUsers;
                dt.Rows[0]["j27ID"] = v.Rec.j27ID;
                dt.Rows[0]["x04ID_Default"] = 2;
                dt.Rows[0]["x01IsAllowPasswordRecovery"] = true;
            }
            if (strTable == "j02User")
            {
                dt.Rows[0]["j02FirstName"] = v.FirstName;
                dt.Rows[0]["j02LastName"] = v.LastName;
                dt.Rows[0]["j02Email"] = v.Email;
                dt.Rows[0]["j02Login"] = v.UserName;
                dt.Rows[0]["j02LangIndex"] = v.UserLangIndex;
                string strHash = new BL.Code.PasswordSupport().GetPasswordHash(v.UserPassword, v.UserName, Convert.ToInt32(dt.Rows[0]["j02ID"].ToString()));
                dt.Rows[0]["j02PasswordHash"] = strHash;
            }

            var cols = Code.basGuru.GetColumns(dt);
            foreach (System.Data.DataRow dbRow in dt.Rows)
            {
                var sb = new System.Text.StringBuilder();
                bool bolGO = true;
                if (strTable == "c26Holiday" && dbRow["c26CountryCode"] != System.DBNull.Value && dbRow["c26CountryCode"].ToString() != v.Rec.x01CountryCode)
                {
                    bolGO = false;
                }
                if (bolGO)
                {
                    if (bolIdentity) sb.AppendLine($"SET IDENTITY_INSERT [{v.DestDbName}].dbo.{strTable} ON;");

                    sb.Append($" INSERT INTO [{v.DestDbName}].dbo.{strTable} (");
                    sb.Append(string.Join(",", cols.Select(p => p.Key)));
                    sb.Append(")");
                    sb.Append(" VALUES(");
                    for (int i = 0; i < cols.Count(); i++)
                    {
                        if (i > 0) sb.Append(",");
                        string strField = cols[i].Key;
                        sb.Append(Code.basGuru.GVN(dbRow[strField], cols[i].Key, v.DestX01ID));
                    }
                    sb.Append(");");

                    if (bolIdentity) sb.AppendLine($"; SET IDENTITY_INSERT [{v.DestDbName}].dbo.{strTable} OFF");

                    _f.FBL.RunSql(sb.ToString());

                    wr_sql(sb.ToString());
                }

            }

        }

        private void wr_sql(string s)
        {
            if (_sbLogSql == null) _sbLogSql = new System.Text.StringBuilder(1000);

            _sbLogSql.AppendLine(s);
        }
    }
}
