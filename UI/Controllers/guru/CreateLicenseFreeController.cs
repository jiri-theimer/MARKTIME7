using BL;
using ceTe.DynamicPDF.PageElements.Charting.Values;
using DocumentFormat.OpenXml.Drawing;
using Microsoft.AspNetCore.Mvc;
using Rebex.Mime.Headers;
using Serilog;
using System.Data;
using System.Net.Mail;
using System.Security.Principal;
using System.Text;
using UI.Models.Guru;


namespace UI.Controllers.guru
{
    public class CreateLicenseFreeController : Controller
    {
        private BL.Factory _f;
        private System.Text.StringBuilder _sbLogSql { get; set; }
        private string _captcha_secret= "6LcPbGwUAAAAAGaIm2I6D9ZRDZDgFse2pdnaKFnj";


        public CreateLicenseFreeController(BL.Factory f)
        {
            _f = f;
            
        }

        public IActionResult Confirm(string username)
        {
            var v = new LicenseFreeConfirmViewModel() { UserName = username };
            if (string.IsNullOrEmpty(v.UserName))
            {
                return View(v);
            }
            var dt = _f.FBL.GetDataTable($"select j02ID as pid from j02User where j02Login like '{username}'");
            v.RecJ02 = _f.j02UserBL.Load(Convert.ToInt32(Convert.ToInt32(dt.Rows[0]["pid"])));
            var recJ04 = _f.j04UserRoleBL.Load(v.RecJ02.j04ID);
            v.RecX01 = _f.x01LicenseBL.Load(recJ04.x01ID);

            
            return View(v);
        }
        private BO.j02User LoadRecJ02(CreateLicenseViewModel v)
        {
            
            var dt = _f.FBL.GetDataTable($"select j02ID as pid from j02User where j02Login like '{v.UserName}'");
            var intJ02ID = Convert.ToInt32(Convert.ToInt32(dt.Rows[0]["pid"]));
            return _f.j02UserBL.Load(intJ02ID);
        }
        public IActionResult Index(string countrycode,bool allowprefill)
        {            
            var v = new CreateLicenseViewModel {AllowPrefill=allowprefill, Rec = new BO.x01License(), IsZakladatKlientaPlusProjekty = true, Project1 = "DPPO + Poradenství", Project2 = $"Audit {DateTime.Now.Year}" };

            
            if (string.IsNullOrEmpty(countrycode)) countrycode = "CZ";
            if (countrycode != "CZ" && countrycode != "SK") countrycode = "CZ";
            v.Rec.x01CountryCode = countrycode;

            if (_f.App.HostingMode != BL.Singleton.HostingModeEnum.TotalCloud)
            {
                v.Message = "Aplikace není v režimu TotalCloud.";
                return View(v);
            }


            

            return View(v);
        }

        private async Task<bool> OveritCaptcha(CreateLicenseViewModel v)
        {
            string strUserResponse = Request.Form["g-Recaptcha-Response"];
            if (string.IsNullOrEmpty(strUserResponse))
            {
                v.Message = "Musíte nám ověřit, že nejste robot!";
                return false;
            }

            using (var request = new HttpRequestMessage(new HttpMethod("GET"), $"https://www.google.com/recaptcha/api/siteverify?secret={_captcha_secret}&response={strUserResponse}"))
            {
                try
                {
                    var client = new HttpClient();
                    var response = await client.SendAsync(request);

                    var strRet = await response.Content.ReadAsStringAsync();

                    return true;
                }
                catch (Exception ex)
                {
                    v.Message = ex.Message;
                    BO.Code.File.LogError(ex.Message, "anonym", "OveritCaptcha");
                    return false;
                }


            }
                       
        }

        [HttpPost]
        public ActionResult Index(CreateLicenseViewModel v, string oper)
        {
            //_f.CurrentUser = new BO.RunningUser() { j02Login = _f.App.GuruLogin };
            _f.InhaleUserByLogin(_f.App.GuruLogin);

            if (oper == "postback")
            {
                return View(v);
            }
            if (oper == "prefill")
            {
                int intX = GetMaxPidValue("x01License", v) + 1;
                v.Email = "jiri.theimer@marktime.cz";
                v.LoginName = "admin";
                v.LoginDomainPrefix = $"pepik{intX}";
                v.FirstName = "Já";
                v.LastName = $"Pepík{intX}";
                v.p93Company = $"Pepíci{intX} & partneři s.r.o.";
                v.p93RegID = BO.Code.Bas.GetGuid().Substring(0, 8);
                v.UserPassword = "Commander1.";
                v.VerifyPassword = v.UserPassword;
                return View(v);
            }

            if (ModelState.IsValid)
            {
                if (_f.App.HostingMode != BL.Singleton.HostingModeEnum.TotalCloud)
                {
                    v.Message = "Aplikace není v režimu TotalCloud.";
                    return View(v);
                }
                if (!OveritCaptcha(v).Result)
                {
                    return View(v);
                }

                if (string.IsNullOrEmpty(v.p93Company))
                {
                    v.Message = "Chybí zadat název firmy!";
                    return View(v);
                }
                if (string.IsNullOrEmpty(v.LoginDomainPrefix))
                {
                    v.Message = "Chybí zadat doména pro přihlašování!";
                    return View(v);
                }
                if (string.IsNullOrEmpty(v.Email))
                {
                    v.Message = "Chybí zadat E-mail!";
                    return View(v);
                }
                else
                {
                    if (!BO.Code.Bas.IsValidEmail(v.Email))
                    {
                        v.Message = $"E-mail adresa [{v.Email}] není platná!";
                        return View(v);
                    }
                }
                
                if (string.IsNullOrEmpty(v.UserPassword))
                {
                    v.Message = "Chybí zadat Heslo!";
                    return View(v);
                }
                if (string.IsNullOrEmpty(v.FirstName) || string.IsNullOrEmpty(v.LastName))
                {
                    v.Message = "Chybí zadat Jméno nebo Příjmení!";
                    return View(v);
                }

                if (string.IsNullOrEmpty(v.LoginName))
                {

                    v.LoginName = $"{BO.Code.Bas.RemoveDiacritics(v.FirstName)}";
                    //v.Message = "Chybí zadat uživatelské jméno!";
                    //return View(v);
                }

                string strLogin = $"{v.LoginName.ToLower()}@{v.LoginDomainPrefix.ToLower()}.{v.Rec.x01CountryCode.ToLower()}";
                var dt=_f.FBL.GetDataTable($"select * from j02User where j02Login like '%{strLogin}%'");
                if (dt.Rows.Count > 0)
                {
                    v.Message = $"V systému již existuje uživatelský účet [{strLogin}]!<hr>Pro vyřešení problému kontaktujte naší podporu.";
                    return View(v);
                }

                string strDomain = $"{v.LoginDomainPrefix.ToLower()}";
                dt = _f.FBL.GetDataTable($"select * from x01License where x01LoginDomain like '{strDomain}.cz%' OR x01LoginDomain like '{strDomain}.sk%'");                
                if (dt.Rows.Count > 0)
                {
                    v.Message = $"V systému již existuje licence pro doménu [{strDomain}]!<hr>Pro vyřešení problému kontaktujte naší podporu.";
                    return View(v);
                }

                var res = new BL.Code.PasswordSupport().CheckPassword(v.UserPassword);
                if (res.Flag == BO.ResultEnum.Failed)
                {
                    v.Message = res.Message;return View(v);
                }
                if (v.UserPassword != v.VerifyPassword)
                {
                    v.Message = "Heslo nesouhlasí s jeho ověřením.";return View(v);
                }

                v.UserName = strLogin;
                v.Rec.x01LoginDomain = $"{v.LoginDomainPrefix.ToLower()}.{v.Rec.x01CountryCode.ToLower()}";
                v.Rec.x01Name = v.p93Company;
                v.Rec.x01AppName = v.p93Company;
                if (v.Rec.x01AppName.Length > 20)
                {
                    v.Rec.x01AppName = v.Rec.x01AppName.Substring(0, 20);                    
                }
                v.Rec.x01AppHost = "https://app.marktime.net";
                v.Rec.x01ContactEmail = v.Email;
                v.Rec.x01Round2Minutes = 15;
                v.Rec.x01ContactName = v.FirstName + " " + v.LastName;
                if (v.Rec.x01CountryCode == "CZ")
                {
                    v.Rec.j27ID = 2;
                }
                else
                {
                    v.Rec.j27ID = 3;
                }


                SendInfoMail(v);    //zpráva o pokusu o založení nové licence - pro jistotu

                Handle_ImportData(v);

                if (v.DestX01ID > 0)
                {
                                                           
                    dt = _f.FBL.GetDataTable($"select x04ID FROM x04NotepadConfig WHERE x01ID={v.DestX01ID} AND x04Name LIKE 'Výchozí Editor'");                    
                    _f.FBL.RunSql($"UPDATE x01License set x01Guid=NEWID(),x04ID_Default={dt.Rows[0]["x04ID"]},x01RecordsCountP31=0,x01LimitUsers=99 WHERE x01ID={v.DestX01ID}");

                    var recX01 = _f.x01LicenseBL.Load(v.DestX01ID);
                    _f.x01LicenseBL.Save(recX01);

                    var recJ02 = LoadRecJ02(v);
                    _f.FBL.RunSql($"UPDATE j02User set j02MyMenuLinks='{_f.j02UserBL.GetDefaultMenuLinks()}',j02HomePageUrl=null,j02Mobile=null WHERE j02ID={recJ02.pid}");

                    _f.App.RefreshP07List();

                    SendConfirmMail(v, recJ02);

                    return RedirectToAction("Confirm", new { username = recJ02.j02Login });
                }

                return View(v);




            }
            


            v.Message += "<br>Došlo k chybě.";

            return View(v);
        }

        private void SendConfirmMail(CreateLicenseViewModel v,BO.j02User recJ02)
        {            
            string strHtml = BO.Code.File.GetFileContent(_f.App.RootUploadFolder + "\\_distribution\\mail\\send_message_template_cloud_confirm.html");
            strHtml = strHtml.Replace("#username#", recJ02.j02Login);
            string strTo = recJ02.j02Email;
            //strTo = "billing@marktime.cz";
            _f.MailBL.SendMessageWithoutFactory(strHtml, $"MARKTIME účet [{recJ02.j02Login}] založen", strTo, "info@marktime.cz", "noreply@marktime.net", "MARKTIME");

        }

        private void SendInfoMail(CreateLicenseViewModel v)
        {

            string strTo = "jiri.theimer@marktime.cz";
            string s = $"E-mail: {v.Email}<hr>Firma: {v.p93Company}<br> IČO: {v.p93RegID}<hr> Jméno: {v.FirstName}<br> Příjmení: {v.LastName}<br> doména: {v.LoginDomainPrefix}<br> Login: {v.LoginName}";
            _f.MailBL.SendMessageWithoutFactory(s, "Pokus o založení FREE MARKTIME licence",strTo,null, "noreply@marktime.net", "MARKTIME");

        }
        private int Zalozit_Interni_Klient(CreateLicenseViewModel v)
        {
            var rec = new BO.p28Contact() { p28FirstName = v.FirstName,p28LastName=v.LastName, p28IsCompany = false, p28CountryCode = v.Rec.x01CountryCode };
            var recJ02 = LoadRecJ02(v);
            rec.j02ID_Owner = recJ02.pid;

            var dt = _f.FBL.GetDataTable($"select p29ID FROM p29ContactType WHERE x01ID={v.DestX01ID} AND p29Name LIKE 'Interní'");
            rec.p29ID = Convert.ToInt32(dt.Rows[0]["p29ID"]);

            if (!string.IsNullOrEmpty(v.p93RegID))
            {
                var engine = new BL.Code.RejstrikySupport();
                var ret = engine.LoadDefaultZaznam("ico", v.p93RegID, v.Rec.x01CountryCode, new HttpClient());
                try
                {             
                    if (!string.IsNullOrEmpty(ret.Result.ico))
                    {
                        rec.p28RegID = ret.Result.ico;
                        rec.p28IsCompany = true;
                        rec.p28FirstName = null; rec.p28LastName = null;
                        rec.p28CompanyName = ret.Result.name;
                        rec.p28VatID = ret.Result.dic;
                        rec.p28Street1 = ret.Result.street;
                        rec.p28City1 = ret.Result.city;
                        rec.p28PostCode1 = ret.Result.zipcode;
                        if (ret.Result.country == "CZ")
                        {
                            rec.p28Country1 = "Česká republika";
                        }
                        if (ret.Result.country == "SK")
                        {
                            rec.p28Country1 = "Slovenská republika";
                        }
                    }                             
                }
                catch
                {
                    //nic
                }
            }
            _f.InhaleUserByLogin(recJ02.j02Login);
            return _f.p28ContactBL.Save(rec, null, null, null, null, null);

        }
        private int Zalozit_Interni_Projekt(CreateLicenseViewModel v)
        {
            var rec = new BO.p41Project() { p41Name = "Interní",p28ID_Client=v.DestP28ID_Interni };
            var recJ02 = LoadRecJ02(v);
            rec.j02ID_Owner = recJ02.pid;
            
            var dt = _f.FBL.GetDataTable($"select a.p42ID FROM p42ProjectType a WHERE a.x01ID={v.DestX01ID} AND a.p42Name LIKE 'Interní projekt'");
            rec.p42ID = Convert.ToInt32(dt.Rows[0]["p42ID"]);

            _f.InhaleUserByLogin(recJ02.j02Login);
            return _f.p41ProjectBL.Save(rec,null,null,null);
        }

        private int Zalozit_Prvni_Klient(CreateLicenseViewModel v)
        {
            var rec = new BO.p28Contact() { p28CompanyName="Marktime Software s.r.o.", p28IsCompany = true, p28CountryCode = "CZ",p28RegID="25722034",p28VatID="CZ25722034",p28Street1="Slezská 2232/144",p28City1="Praha 3 Vinohrady",p28PostCode1="13000", p28Country1="Česká republika" };
            var recJ02 = LoadRecJ02(v);
            rec.j02ID_Owner = recJ02.pid;

            var dt = _f.FBL.GetDataTable($"select p29ID FROM p29ContactType WHERE x01ID={v.DestX01ID} AND p29Name LIKE 'Klient'");
            rec.p29ID = Convert.ToInt32(dt.Rows[0]["p29ID"]);
            
            _f.InhaleUserByLogin(recJ02.j02Login);
            return _f.p28ContactBL.Save(rec, null, null, null, null, null);

        }
        private int Zalozit_Klientsky_Projekt(CreateLicenseViewModel v,string strP41Name)
        {
            var rec = new BO.p41Project() { p41Name = strP41Name, p28ID_Client = v.DestP28ID_Klient };
            var recJ02 = LoadRecJ02(v);
            rec.j02ID_Owner = recJ02.pid;

            var dt = _f.FBL.GetDataTable($"select a.p42ID FROM p42ProjectType a INNER JOIN p07ProjectLevel b ON a.p07ID=b.p07ID WHERE b.x01ID={v.DestX01ID} AND a.p42Name LIKE 'Klientský projekt'");
            try
            {
                rec.p42ID = Convert.ToInt32(dt.Rows[0]["p42ID"]);
            }
            catch
            {
                var lisP42 = _f.p42ProjectTypeBL.GetList(new BO.myQueryP42());
                rec.p42ID = lisP42.First().pid;
            }
            

            _f.InhaleUserByLogin(recJ02.j02Login);
            return _f.p41ProjectBL.Save(rec, null, null, null);
        }

        private void Handle_ImportData(CreateLicenseViewModel v)
        {
            v.lisKeys = new List<CreateLicenseKey>();
           
            Import_One_Table("x01License", v);
            v.DestX01ID = GetMaxPidValue("x01License", v);
            _f.App.RefreshX01List();
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

            Import_One_Table("p29ContactType", v);

            Import_One_Table("j02User", v);

            v.DestP28ID_Interni = Zalozit_Interni_Klient(v);
            v.DestP41ID_Interni = Zalozit_Interni_Projekt(v);
            v.DestP28ID_Klient=Zalozit_Prvni_Klient(v);
            v.DestP41ID_Klient1 = Zalozit_Klientsky_Projekt(v, "General");
            v.DestP41ID_Klient2 = Zalozit_Klientsky_Projekt(v, $"Účetnictví {DateTime.Now.Year}");
            

            Import_One_Table("p32Activity", v);
            Import_One_Table("o28ProjectRole_Workload", v);
            Import_One_Table("p43ProjectType_Workload", v);
            Import_One_Table("p61ActivityCluster", v);
            Import_One_Table("p62ActivityCluster_Item", v);


            Import_One_Table("j25ReportCategory", v);
            Import_One_Table("x31Report", v);
            Import_One_Table("o27Attachment", v);            

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

            

            Import_One_Table("j40MailAccount", v);



            Import_One_Table("p51PriceList", v);
            Import_One_Table("p52PriceList_Item", v);

            
            Import_One_Table("o17DocMenu", v);
            Import_One_Table("o18DocType", v);
            Import_One_Table("o16DocType_FieldSetting", v);
            Import_One_Table("o20DocTypeEntity", v);


            BO.Code.File.WriteText2File($"{_f.App.RootUploadFolder}\\{v.DestDbName}-{v.Rec.x01LoginDomain}-{BO.Code.Bas.GetGuid()}.sql", _sbLogSql.ToString());

        }


        private void Import_One_Table(string strTable, CreateLicenseViewModel v)
        {

            DataTable dt = null;
            
            wr_sql($"------{v.DestDbName}.dbo.{strTable}");
            try
            {
                dt = Code.basGuru.Load_DataTable_From_File(_f, strTable);
            }
            catch (Exception ex)
            {
                wr_sql($"------{v.DestDbName}.dbo.{strTable}: {ex.Message}");
                return;
            }

            if (dt.Rows.Count == 0) return;
            bool bolIdentity = true;
            

            if (strTable == "x01License")
            {
                bolIdentity = false;
                //dt.Rows[0]["x01Guid"] = BO.Code.Bas.GetGuid();
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
                dt.Rows[0]["x01RobotLogin"] = v.LoginName+"@"+ v.Rec.x01LoginDomain;
                dt.Rows[0]["j27ID"] = v.Rec.j27ID;
                dt.Rows[0]["x01IsAllowPasswordRecovery"] = true;
            }
            if (strTable == "j02User")
            {
                dt.Rows[0]["j02FirstName"] = v.FirstName;
                dt.Rows[0]["j02LastName"] = v.LastName;
                dt.Rows[0]["j02Email"] = v.Email;
                dt.Rows[0]["j02Login"] = v.UserName;
                dt.Rows[0]["j02LangIndex"] = v.UserLangIndex;
                
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
                    if (bolIdentity) sb.AppendLine($"SET IDENTITY_INSERT {v.DestDbName}.dbo.{strTable} ON;");

                    sb.Append($" INSERT INTO {v.DestDbName}.dbo.{strTable} (");
                    sb.Append(string.Join(",", cols.Select(p => p.Key)));
                    sb.Append(")");
                    sb.Append(" VALUES(");
                    for (int i = 0; i < cols.Count(); i++)
                    {
                        if (i > 0) sb.Append(",");
                        string strField = cols[i].Key;
                        string strValue = Code.basGuru.GVN(dbRow[strField], strField, v.DestX01ID);
                        if (strField == strTable.Substring(0,3) + "ID")
                        {
                            strValue = (GetMaxPidValue(strTable, v) + 1).ToString();
                        }
                        
                        if (dbRow[strField] != System.DBNull.Value && v.lisKeys.Where(p => p.Field == strField && p.OrigValue==Convert.ToInt32(dbRow[strField])).Count() > 0)
                        {
                            strValue = v.lisKeys.Where(p => p.Field == strField && p.OrigValue == Convert.ToInt32(dbRow[strField])).First().NewValue.ToString();    //cizí klíč
                        }
                        
                        if (strTable == "o27Attachment" && strField== "o27RecordPid" && dbRow["o27RecordPid"] !=System.DBNull.Value)
                        {
                            strValue = v.lisKeys.Where(p => p.Field == "x31ID" && p.OrigValue == Convert.ToInt32(dbRow["o27RecordPid"])).First().NewValue.ToString();    //cizí klíč
                        }
                        if (strTable == "p32Activity" && strField == "p41ID_Absence")
                        {
                            strValue = v.DestP41ID_Interni.ToString();                            
                        }
                        if (strTable== "b06WorkflowStep" && strField== "b02ID_Target" && dbRow["b02ID_Target"] != System.DBNull.Value)
                        {
                            strValue = v.lisKeys.Where(p => p.Field == "b02ID" && p.OrigValue == Convert.ToInt32(dbRow["b02ID_Target"])).First().NewValue.ToString();    //cizí klíč
                        }
                        if (strTable == "b06WorkflowStep" && strField == "x67ID_Direct" && dbRow["x67ID_Direct"] != System.DBNull.Value && Convert.ToInt32(dbRow["x67ID_Direct"]) != 0)
                        {
                            strValue = v.lisKeys.Where(p => p.Field == "x67ID" && p.OrigValue == Convert.ToInt32(dbRow["x67ID_Direct"])).First().NewValue.ToString();    //cizí klíč
                        }

                        sb.Append(strValue);
                    }
                    sb.Append(");");

                    if (bolIdentity) sb.AppendLine($"; SET IDENTITY_INSERT {v.DestDbName}.dbo.{strTable} OFF");

                    if (_f.FBL.RunSql(sb.ToString()))
                    {
                        
                        var cc = new CreateLicenseKey { Table=strTable,Field = strTable.Substring(0, 3) + "ID" };
                        cc.NewValue = GetMaxPidValue(strTable, v);
                        cc.OrigValue = Convert.ToInt32(dbRow[strTable.Substring(0, 3) + "ID"]);
                        v.lisKeys.Add(cc);

                        if (strTable == "j02User")
                        {
                            string strHash = new BL.Code.PasswordSupport().GetPasswordHash(v.UserPassword, v.UserName, cc.NewValue);
                            _f.FBL.RunSql($"UPDATE j02User set j02PasswordHash='{strHash}' WHERE j02ID={cc.NewValue}");
                            
                        }
                    }

                    wr_sql(sb.ToString());
                }

            }

        }

        private int GetMaxPidValue(string strTable, CreateLicenseViewModel v)
        {
            var dt = _f.FBL.GetDataTable($"select MAX({strTable.Substring(0,3)}ID) as pid from {v.DestDbName}.dbo.{strTable}");
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

        private void wr_sql(string s)
        {
            if (_sbLogSql == null) _sbLogSql = new System.Text.StringBuilder(1000);

            _sbLogSql.AppendLine(s);
        }
    }
}
