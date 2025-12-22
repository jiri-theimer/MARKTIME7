
using BL;
using BO;
using Microsoft.AspNetCore.Mvc;

using UI.Models;
using UI.Models.Guru;


namespace UI.Controllers
{
    public class GuruController : Controller
    {
        private BL.Factory _f;


        public GuruController(BL.Factory f)
        {
            _f = f;

        }


        private IActionResult ViewOnlyForVerified(object v)
        {
            return View(v); //pro testování
            //if (HttpContext.User.Identity.Name == _f.App.GuruLogin)
            //{
            //    //ověřený guru uživatel - nemusí být v j02User
            //    return View(v);
            //}

            //v = new StopPageViewModel() { Message = "Nejsi GURU!" };
            //return View("_StopPage", v);
        }


        public IActionResult Index()
        {

            return ViewOnlyForVerified(new BaseViewModel());
        }

        public IActionResult CreateDb()
        {

            return ViewOnlyForVerified(new BaseViewModel());
        }

        public BO.Result KillDb(string dbname, string connstring)
        {
            if (string.IsNullOrEmpty(connstring))
            {
                connstring = "server=SQL2017.mycore.cloud\\MARKTIME;database=master;uid=MARKTIME;pwd=58PMapN2jhBvdblxqnIB;";
            }
            else
            {
                connstring = connstring.Replace("\\\\", "\\");
            }
            
            _f.db.ChangeConnectString(connstring);
            _f.db.RunSql($"USE [{dbname}]; ALTER DATABASE {dbname} SET SINGLE_USER WITH ROLLBACK IMMEDIATE; USE [master]; DROP DATABASE [{dbname}]");
          


            return new BO.Result(false, "Provedeno");
        }

        public BO.Result CreateSqlDb(string dbname, string connstring)
        {
            if (string.IsNullOrEmpty(connstring))
            {
                connstring = "server=SQL2017.mycore.cloud\\MARKTIME;database=master;uid=MARKTIME;pwd=58PMapN2jhBvdblxqnIB;";
            }
            else
            {
                connstring = connstring.Replace("\\\\", "\\");
            }

            _f.db.ChangeConnectString(connstring);
            bool bolOK = _f.db.RunSql2($"create database [{dbname}]");
            if (bolOK)
            {

                //string strConString = $"server=SQL2017.mycore.cloud\\MARKTIME;database={dbname};uid=MARKTIME;pwd=58PMapN2jhBvdblxqnIB;";
                string strConString = connstring.Replace("=master;", $"={dbname};");
                _f.db.ChangeConnectString(strConString);

                string s = BO.Code.File.GetFileContent(_f.App.RootUploadFolder + "\\_distribution\\sys\\create_db.sql");
                if (System.IO.File.Exists("c:\\DEV\\MARKTIME7-DM\\EXPORT\\create_db.sql"))
                {
                    s = BO.Code.File.GetFileContent("c:\\DEV\\MARKTIME7-DM\\EXPORT\\create_db.sql");
                }

                var arr = BO.Code.Bas.ConvertString2List(s, "GO\r\n");

                foreach (string strSQL in arr)
                {
                    if (!string.IsNullOrEmpty(strSQL) && strSQL.Length > 5)
                    {
                        if (!_f.db.RunSql2(strSQL, null))
                        {
                            bolOK = false;
                        }


                    }
                }

                s = BO.Code.File.GetFileContent(_f.App.RootUploadFolder + "\\_distribution\\sys\\sql_sp_funct_views.sql");
                arr = BO.Code.Bas.ConvertString2List(s, "GO\r\n");
                foreach (string strSQL in arr)
                {
                    if (!string.IsNullOrEmpty(strSQL) && strSQL.Length > 5)
                    {
                        if (!_f.db.RunSql2(strSQL, null))
                        {
                            bolOK = false;
                        }


                    }
                }



            }

            return new BO.Result(!bolOK, (bolOK ? "Vygenerováno bez chyb." : "Nějaké chyby!"));

        }

        public IActionResult GenerateScripts()
        {

            return ViewOnlyForVerified(new BaseViewModel());
        }

        public BO.Result GenerateCreateUpdateScript()
        {
            var lis = _f.FBL.Sys_GetList_SysObjects();
            _f.FBL.Sys_GenerateCreateUpdateScript(lis);

            return new BO.Result(false, $"Soubor byl vygenerován (do {_f.App.RootUploadFolder}\\_distribution\\sys + c:\\DEV\\MARKTIME7-DM\\COREDATA)");
        }

        private BO.Result Handle_Save_Json_Script(IEnumerable<Object> lis, string strFileName)
        {
            BO.Code.File.WriteText2File($"{_f.App.RootUploadFolder}\\_distribution\\{strFileName}", BO.Code.basJson.SerializeData(lis));

            return new BO.Result(false, $"Výstup byl uložen (do {_f.App.RootUploadFolder}\\_distribution\\{strFileName})");
        }

        public BO.Result GenerateScript_x90Migration()
        {
            _f.InhaleUserByLogin("lamos@dev01.cz");
            Handle_Save_Json_Script(_f.FBL.GetListX90(), "x90Migration.json");

            Handle_Convert_DataTable_To_Json("o58GlobalParam", "o58Key not like 'j72Columns'", false);

            return Handle_Convert_DataTable_To_Json("x90Migration", "x90IsUse=1", false);
        }

        public BO.Result GenerateScript_x55Widget()
        {
            _f.InhaleUserByLogin("lamos@dev01.cz");
            Handle_Save_Json_Script(_f.x54WidgetGroupBL.GetList(new BO.myQuery("x54") { IsRecordValid = true }), "x54WidgetGroup.json");
            Handle_Convert_DataTable_To_Json("x54WidgetGroup");
            Handle_Convert_DataTable_To_Json("x57WidgetToGroup",null, false,null);
            Handle_Convert_DataTable_To_Json("x59WidgetToUser", null, false, null);
            Handle_Save_Json_Script(_f.x55WidgetBL.GetList(new BO.myQuery("x55") { IsRecordValid = true }), "x55Widget.json");

            return Handle_Convert_DataTable_To_Json("x55Widget");
        }

        public BO.Result GenerateScript_Stitky()
        {
            Handle_Convert_DataTable_To_Json("o53TagGroup");
            return Handle_Convert_DataTable_To_Json("o51Tag");

        }

        public BO.Result GenerateScript_Workflow()
        {
            Handle_Convert_DataTable_To_Json("b01WorkflowTemplate");
            Handle_Convert_DataTable_To_Json("b02WorkflowStatus");
            Handle_Convert_DataTable_To_Json("b08WorkflowReceiverToStep",null,false);
            Handle_Convert_DataTable_To_Json("b11WorkflowMessageToStep",null,false);
            return Handle_Convert_DataTable_To_Json("b06WorkflowStep");

        }

        public BO.Result GenerateScript_b20Hlidac()
        {
            _f.InhaleUserByLogin("lamos@dev01.cz");
            Handle_Save_Json_Script(_f.b20HlidacBL.GetList(new BO.myQuery("b20") { IsRecordValid = true }), "b20Hlidac.json");

            return Handle_Convert_DataTable_To_Json("b20Hlidac");
        }

        public BO.Result GenerateScript_x01License()
        {
            return Handle_Convert_DataTable_To_Json("x01License");
        }
        public BO.Result GenerateScript_j02User()
        {
            return Handle_Convert_DataTable_To_Json("j02User", "j02Login='lamos@dev01.cz'");
        }

        public BO.Result GenerateScript_Common()
        {
            _f.InhaleUserByLogin("lamos@dev01.cz");

            Handle_Convert_DataTable_To_Json("o33MediumType");
            Handle_Convert_DataTable_To_Json("j27Currency");
            Handle_Convert_DataTable_To_Json("x15VatRateType");
            Handle_Convert_DataTable_To_Json("x24DataType");


            Handle_Convert_DataTable_To_Json("p53VatRate");
            Handle_Convert_DataTable_To_Json("o15AutoComplete");
            Handle_Convert_DataTable_To_Json("j19PaymentType");
            Handle_Convert_DataTable_To_Json("x38CodeLogic");
            Handle_Convert_DataTable_To_Json("x04NotepadConfig");
            Handle_Convert_DataTable_To_Json("p95InvoiceRow", "p95Code like 'DIST%'");
            Handle_Convert_DataTable_To_Json("c21FondCalendar");
            Handle_Convert_DataTable_To_Json("c24DayColor");
            Handle_Convert_DataTable_To_Json("p35Unit");
            
            Handle_Convert_DataTable_To_Json("p80InvoiceAmountStructure");


            Handle_Convert_DataTable_To_Json("p70BillingStatus");
            Handle_Convert_DataTable_To_Json("p71ApproveStatus");
            Handle_Convert_DataTable_To_Json("p72PreBillingStatus");
            Handle_Convert_DataTable_To_Json("p33ActivityInputType");
            //Handle_Convert_DataTable_To_Json("p34ActivityGroup", "p34Code like 'DIST%'");
            Handle_Convert_DataTable_To_Json("p34ActivityGroup");
            //Handle_Convert_DataTable_To_Json("p32Activity", "p34ID IN (select p34ID FROM p34ActivityGroup WHERE p34Code like 'DIST%')");
            Handle_Convert_DataTable_To_Json("p32Activity");
            Handle_Convert_DataTable_To_Json("o28ProjectRole_Workload", null, false);
            //Handle_Convert_DataTable_To_Json("o28ProjectRole_Workload", "p34ID IN (select p34ID FROM p34ActivityGroup WHERE p34Code like 'DIST%')");
            Handle_Convert_DataTable_To_Json("p38ActivityTag");
            Handle_Convert_DataTable_To_Json("p61ActivityCluster");
            Handle_Convert_DataTable_To_Json("p62ActivityCluster_Item", null, false);

            Handle_Convert_DataTable_To_Json("o18DocType");
            Handle_Convert_DataTable_To_Json("o16DocType_FieldSetting", null, false);
            Handle_Convert_DataTable_To_Json("o20DocTypeEntity", null, false);
            Handle_Convert_DataTable_To_Json("o17DocMenu");


            Handle_Convert_DataTable_To_Json("p07ProjectLevel", null, false);

            Handle_Convert_DataTable_To_Json("j07PersonPosition");
            Handle_Convert_DataTable_To_Json("j04UserRole");
            Handle_Convert_DataTable_To_Json("x67EntityRole");
            Handle_Convert_DataTable_To_Json("x68EntityRole_Permission",null,false);


            Handle_Convert_DataTable_To_Json("o21MilestoneType");
            Handle_Convert_DataTable_To_Json("p57TaskType");
            Handle_Convert_DataTable_To_Json("p29ContactType");
            Handle_Convert_DataTable_To_Json("p42ProjectType");
            Handle_Convert_DataTable_To_Json("p43ProjectType_Workload", null, false);

            Handle_Convert_DataTable_To_Json("p93InvoiceHeader");
            Handle_Convert_DataTable_To_Json("p89ProformaType");
            Handle_Convert_DataTable_To_Json("r02CapacityVersion");

            Handle_Convert_DataTable_To_Json("p86BankAccount");
            Handle_Convert_DataTable_To_Json("p93InvoiceHeader");
            Handle_Convert_DataTable_To_Json("p88InvoiceHeader_BankAccount", null, false);
            Handle_Convert_DataTable_To_Json("p92InvoiceType");

            Handle_Convert_DataTable_To_Json("c26Holiday", "YEAR(c26Date)>=YEAR(GETDATE())");

            Handle_Convert_DataTable_To_Json("j40MailAccount", "j40SmtpEmail='app@marktime.net'");


            Handle_Convert_DataTable_To_Json("p51PriceList", "p51TypeFlag IN (5,2)");
            Handle_Convert_DataTable_To_Json("p52PriceList_Item", "p51ID IN (select p51ID FROM p51PriceList WHERE p51TypeFlag IN (5,2))", false);

            return new BO.Result(false, $"Soubory jsou uloženy v {_f.App.RootUploadFolder}\\_distribution");

        }

        private BO.Result Handle_Convert_DataTable_To_Json(string strTable, string strAddW = null, bool bolValidity = true, string strFileName = null)
        {
            if (strFileName == null)
            {
                strFileName = $"{strTable}.dt";
            }

            var strSQL = $"select * from {strTable} WHERE GETDATE() BETWEEN {strTable.Substring(0, 3)}ValidFrom AND {strTable.Substring(0, 3)}ValidUntil";
            if (!bolValidity)
            {
                strSQL = $"select * from {strTable} WHERE 1=1";
            }
            if (strAddW != null)
            {
                strSQL += " AND " + strAddW;
            }
            var dt = _f.FBL.GetDataTable(strSQL);
            if (strTable == "x01License")   //aby to šlo jednoduše naimportovat do prázdné databáze
            {
                dt.Rows[0]["x04ID_Default"] = System.DBNull.Value;
                dt.Rows[0]["b02ID"] = System.DBNull.Value;
                dt.Rows[0]["x01Guid"] = System.DBNull.Value;
                dt.Rows[0]["x01ApiKey"] = System.DBNull.Value;
            }
            if (strTable == "j02User")   //aby to šlo jednoduše naimportovat do prázdné databáze
            {
                dt.Rows[0]["j02EmailSignature"] = System.DBNull.Value;
                dt.Rows[0]["j02MyMenuLinks"] = System.DBNull.Value;
                dt.Rows[0]["j02InvoiceSignatureText"] = System.DBNull.Value;
                dt.Rows[0]["j02Ping_Timestamp"] = System.DBNull.Value;
                dt.Rows[0]["j02Cache_j11IDs"] = System.DBNull.Value;
                dt.Rows[0]["j02Cache_TimeStamp"] = System.DBNull.Value;
                dt.Rows[0]["j02PasswordHash"] = System.DBNull.Value;
                dt.Rows[0]["j02GridCssBitStream"] = 50;

            }
            string strJson = Newtonsoft.Json.JsonConvert.SerializeObject(dt, Newtonsoft.Json.Formatting.Indented);
            BO.Code.File.WriteText2File($"{_f.App.RootUploadFolder}\\_distribution\\{strFileName}", strJson);

            return new BO.Result(false, $"Soubor {strFileName}.dt byl uložen do {_f.App.RootUploadFolder}\\_distribution");
        }

        public BO.Result GenerateScript_x51HelpCore()
        {
            Handle_Convert_DataTable_To_Json("x51HelpCore");

            _f.InhaleUserByLogin("lamos@dev01.cz");
            var lis = _f.o27AttachmentBL.GetList(new BO.myQueryO27()).Where(p => p.o27Entity == "x51");

            foreach (var c in lis)
            {

                string s = _f.WwwUsersFolder + "\\" + c.o27WwwRootFolder + "\\" + c.o27ArchiveFileName;
                if (System.IO.File.Exists(s))
                {
                    System.IO.File.Copy(s, $"{_f.App.RootUploadFolder}\\_distribution\\x51\\" + c.o27ArchiveFileName, true);

                }
            }

            Handle_Convert_DataTable_To_Json("o27Attachment", "o27Entity='x51'", true, "o27Attachment_x51.json");

            return new BO.Result(false, "Vygenerováno");
        }

        public BO.Result GenerateScript_x31Report()
        {
            Handle_Convert_DataTable_To_Json("j25ReportCategory");
            Handle_Convert_DataTable_To_Json("x31Report");

            _f.InhaleUserByLogin("lamos@dev01.cz");
            var lis = _f.x31ReportBL.GetList(new BO.myQueryX31() { IsRecordValid = true });

            foreach (var c in lis)
            {
                string s = _f.UploadFolder + "\\X31\\" + c.ReportFileName;
                if (System.IO.File.Exists(s))
                {
                    System.IO.File.Copy(s, $"{_f.App.RootUploadFolder}\\_distribution\\trdx\\" + c.ReportFileName, true);
                    c.Rezerva = "1";
                }
            }

            Handle_Convert_DataTable_To_Json("o27Attachment", "o27Entity='x31'");

            Handle_Save_Json_Script(_f.j25ReportCategoryBL.GetList(new BO.myQuery("j25")), "j25ReportCategory.json");

            return Handle_Save_Json_Script(lis, "x31Report.json");
            


        }

        public BO.Result GenerateSysScript()
        {
            var lisTables = _f.FBL.Sys_GetList_Tables("a7MARKTIME_EMPTY");
            BO.Code.File.WriteText2File($"{_f.App.RootUploadFolder}\\_distribution\\sys\\Tables.json", BO.Code.basJson.SerializeData(lisTables));

            var lisCols = _f.FBL.Sys_GetList_Columns("a7MARKTIME_EMPTY");
            BO.Code.File.WriteText2File($"{_f.App.RootUploadFolder}\\_distribution\\sys\\Columns.json", BO.Code.basJson.SerializeData(lisCols));

            var lisIndexes = _f.FBL.Sys_GetList_Indexes("a7MARKTIME_EMPTY");
            BO.Code.File.WriteText2File($"{_f.App.RootUploadFolder}\\_distribution\\sys\\Indexes.json", BO.Code.basJson.SerializeData(lisIndexes));

            var lisDefvals = _f.FBL.Sys_GetList_Defvals("a7MARKTIME_EMPTY");
            BO.Code.File.WriteText2File($"{_f.App.RootUploadFolder}\\_distribution\\sys\\Defvals.json", BO.Code.basJson.SerializeData(lisDefvals));

            var lisConstraints = _f.FBL.Sys_GetList_Constraints("a7MARKTIME_EMPTY");
            BO.Code.File.WriteText2File($"{_f.App.RootUploadFolder}\\_distribution\\sys\\Constraints.json", BO.Code.basJson.SerializeData(lisConstraints));

            return new BO.Result(false, $"Sys konfigurace byla uložena (do {_f.App.RootUploadFolder}\\_distribution\\sys)");
        }








        private int ZalozitTypKontaktu(BO.p29ScopeFlagENUM scope, int intX01ID, string strP29Name)
        {
            return _f.p29ContactTypeBL.Save(new BO.p29ContactType() { p29ScopeFlag = scope, x01ID = intX01ID, p29Name = strP29Name }, null, null);
        }
        private int ZalozitPozici(int intX01ID, string strJ07Name)
        {
            return _f.j07PersonPositionBL.Save(new BO.j07PersonPosition() { x01ID = intX01ID, j07Name = strJ07Name });
        }

        public IActionResult Import(int x01id)
        {
            var v = new Models.Guru.ImportViewModel { x01ID_Dest = x01id };

            RefreshStateImport(v);
            return View(v);
        }

        private void RefreshStateImport(Models.Guru.ImportViewModel v)
        {
            v.Rec_Dest = _f.x01LicenseBL.Load(v.x01ID_Dest);
            v.Rec_Source = _f.x01LicenseBL.Load(v.x01ID_Source);
        }

        [HttpPost]
        public ActionResult Import(Models.Guru.ImportViewModel v, string oper)
        {
            RefreshStateImport(v);
            if (oper == "postback")
            {

                return View(v);
            }
            if (v.Rec_Source == null || v.Rec_Dest == null)
            {
                v.Message = "Chybí zadat zdrojovou nebo cílovou licenci!";
                return View(v);
            }

            return View(v);
        }



        public BO.Rejstrik.DefaultZaznam LoadRejstrikSubjekt(string strPole, string strHodnota, string strCountryCode)
        {
            var engine = new BL.Code.RejstrikySupport();

            var ret = engine.LoadDefaultZaznam(strPole, strHodnota, strCountryCode, new HttpClient());
            try
            {
                return ret.Result;
            }
            catch
            {

                return null;
            }

        }





    }
}
