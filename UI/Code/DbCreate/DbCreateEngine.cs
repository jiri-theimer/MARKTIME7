using System.Data;

namespace UI.Code.DbCreate
{
    /// <summary>
    /// Založení nové zákaznické databáze po krocích: SQL databáze -> tabulky -> SQL objekty -> výchozí data -> licence -> kontakty -> sestavy.
    /// Každý krok lze spustit opakovaně (po opravě příčiny chyby naváže tam, kde skončil).
    /// Volá se z CreateLicenseController.RunStep, každý krok v samostatném http požadavku.
    /// </summary>
    public class DbCreateEngine
    {
        private readonly BL.Factory _f;
        private readonly DbCreateJob _job;
        private const int _x01ID = 1;   //v nové databázi je vždy jediná licence
        private bool _userInserted;     //první uživatel byl vložen v tomto běhu kroku [data]

        //ID záznamů z distribučních dat (_distribution\*.dt):
        private const int _p29ID_Interni = 15;
        private const int _p29ID_Klient = 9;
        private const int _p42ID_InterniProjekt = 5;
        private const int _p42ID_KlientskyProjekt = 1;
        private const int _x67ID_ManazerProjektu = 13;
        private const int _x67ID_ClenProjektu = 14;
        private const int _x67ID_VidiVsechnyHodiny = 107;

        public DbCreateEngine(BL.Factory f, DbCreateJob job)
        {
            _f = f;
            _job = job;
        }

        private DbCreateRequest R
        {
            get
            {
                return _job.Request;
            }
        }
        private string DistributionFolder
        {
            get
            {
                return _f.App.RootUploadFolder + "\\_distribution";
            }
        }
        private bool IsSharedApp
        {
            get
            {
                return _f.App.HostingMode == BL.Singleton.HostingModeEnum.SharedApp;
            }
        }

        private DL.DbCreateTasks Master()
        {
            return new DL.DbCreateTasks(_job.MasterConnectString);
        }
        private DL.DbCreateTasks Target()
        {
            return new DL.DbCreateTasks(_job.TargetConnectString);
        }

        public static List<DbCreateStep> BuildSteps(DbCreateRequest r)
        {
            var lis = new List<DbCreateStep>
            {
                new DbCreateStep() { Key = "preflight", Title = "Kontrola zadání a serveru" },
                new DbCreateStep() { Key = "database", Title = "Založení SQL databáze" },
                new DbCreateStep() { Key = "schema", Title = "Tabulky" },
                new DbCreateStep() { Key = "sqlobjects", Title = "Procedury, funkce a pohledy" },
                new DbCreateStep() { Key = "data", Title = "Výchozí číselníky a nastavení" },
                new DbCreateStep() { Key = "license", Title = "Licence a první uživatel" },
                new DbCreateStep() { Key = "contacts", Title = r.CreateClient ? "Interní kontakt, první klient a projekty" : "Interní kontakt a projekt" },
                new DbCreateStep() { Key = "reports", Title = "Tiskové sestavy" },
                new DbCreateStep() { Key = "finish", Title = "Dokončení" }
            };
            return lis;
        }

        public void RunStep(DbCreateStep step)
        {
            var sw = System.Diagnostics.Stopwatch.StartNew();
            step.Details.Clear();
            step.Message = null;
            step.Status = DbCreateStepStatus.Ok;

            try
            {
                switch (step.Key)
                {
                    case "preflight": Step_Preflight(step); break;
                    case "database": Step_Database(step); break;
                    case "schema": Step_Schema(step); break;
                    case "sqlobjects": Step_SqlObjects(step); break;
                    case "data": Step_Data(step); break;
                    case "license": Step_License(step); break;
                    case "contacts": Step_Contacts(step); break;
                    case "reports": Step_Reports(step); break;
                    case "finish": Step_Finish(step); break;
                    default:
                        throw new Exception($"Neznámý krok: {step.Key}");
                }
            }
            catch (Exception e)
            {
                step.Status = DbCreateStepStatus.Error;
                step.Message = e.Message;
                BO.Code.File.LogError($"{_job.DbName} | {step.Key} | {e}", "dbcreate", "DbCreateEngine.RunStep");
            }

            sw.Stop();
            step.DurationMs = sw.ElapsedMilliseconds;
        }

        private void Warn(DbCreateStep step, string strDetail)
        {
            step.Details.Add(strDetail);
            if (step.Status == DbCreateStepStatus.Ok)
            {
                step.Status = DbCreateStepStatus.Warning;
            }
        }

        // ── 1. kontrola ─────────────────────────────────────────────────────────────

        private string GetCreateDbScriptPath()
        {
            var s = _f.App.Configuration.GetSection("DbCreate")["CreateDbScriptPath"];
            if (!string.IsNullOrEmpty(s) && System.IO.File.Exists(s))
            {
                return s;
            }
            if (System.IO.File.Exists("c:\\DEV\\MARKTIME7-DM\\EXPORT\\create_db.sql"))
            {
                return "c:\\DEV\\MARKTIME7-DM\\EXPORT\\create_db.sql";   //vývojářský stroj - stejné chování jako původní /Guru/CreateSqlDb
            }
            return DistributionFolder + "\\sys\\create_db.sql";
        }
        private string GetSqlObjectsScriptPath()
        {
            return DistributionFolder + "\\sys\\sql_sp_funct_views.sql";
        }

        private void Step_Preflight(DbCreateStep step)
        {
            if (!DL.DbCreateTasks.IsValidDbName(_job.DbName))
            {
                throw new Exception($"Neplatný název databáze [{_job.DbName}].");
            }

            var missing = new List<string>();
            foreach (var strPath in new[] { GetCreateDbScriptPath(), GetSqlObjectsScriptPath(), DistributionFolder + "\\x01License.dt", DistributionFolder + "\\j02User.dt", DistributionFolder + "\\plugins\\company_logo.png" })
            {
                if (!System.IO.File.Exists(strPath))
                {
                    missing.Add(strPath);
                }
            }
            if (missing.Count > 0)
            {
                throw new Exception("Na serveru chybí distribuční soubory: " + string.Join(", ", missing));
            }
            if (!System.IO.Directory.Exists(DistributionFolder + "\\trdx"))
            {
                Warn(step, $"Chybí složka s tiskovými sestavami {DistributionFolder}\\trdx - sestavy se nezkopírují.");
            }

            var master = Master();
            try
            {
                master.Scalar<int>("SELECT 1");
            }
            catch (Exception e)
            {
                throw new Exception("Nepodařilo se připojit k SQL serveru: " + e.Message);
            }

            if (master.DatabaseExists(_job.DbName) && !_job.DbCreatedByJob)
            {
                var target = Target();
                if (!_job.IsGuru)
                {
                    throw new Exception($"Doména [{R.LoginDomain}] je už obsazená. Zvolte prosím jinou.");
                }
                if (target.ObjectExists("dbo.x01License") && target.CountRows("x01License") > 0)
                {
                    //v databázi už licence je: pokračovat lze jen tehdy, když jde o nedokončené založení téže domény
                    string strDomain = target.Scalar<string>("SELECT TOP 1 x01LoginDomain FROM x01License ORDER BY x01ID");
                    if (!string.Equals(strDomain, R.LoginDomain, StringComparison.OrdinalIgnoreCase))
                    {
                        throw new Exception($"Databáze [{_job.DbName}] už existuje a obsahuje licenci [{strDomain}]. Zvolte jinou doménu.");
                    }
                    _job.IsResume = true;
                    Warn(step, $"Databáze [{_job.DbName}] už obsahuje licenci [{strDomain}] - průvodce naváže na nedokončené založení a doplní, co chybí.");
                }
                else
                {
                    Warn(step, $"Databáze [{_job.DbName}] už na serveru existuje (bez licence) - průvodce ji použije a doplní, co v ní chybí.");
                }
            }

            if (IsSharedApp)
            {
                int x = 0;
                try
                {
                    x = master.Scalar<int>("SELECT COUNT(*) FROM a7_cloudheader.dbo.x01License WHERE x01LoginDomain=@dom OR x01LoginDomain LIKE @prefix", new { dom = R.LoginDomain, prefix = R.DomainPrefix + ".%" });
                }
                catch (Exception e)
                {
                    throw new Exception("Nepodařilo se ověřit doménu v databázi a7_cloudheader: " + e.Message);
                }
                if (x > 0 && !_job.LicenseSaved && !_job.IsResume)
                {
                    throw new Exception($"Licence s doménou [{R.DomainPrefix}] už v cloudu existuje. Zvolte jinou doménu.");
                }
            }

            step.Message = $"Server {DL.DbCreateTasks.GetServerName(_job.MasterConnectString)}, databáze {_job.DbName}";
        }

        // ── 2. SQL databáze ─────────────────────────────────────────────────────────

        private void Step_Database(DbCreateStep step)
        {
            var master = Master();
            if (master.DatabaseExists(_job.DbName))
            {
                step.Status = DbCreateStepStatus.Skipped;
                step.Message = "Databáze už existuje.";
                return;
            }

            master.CreateDatabase(_job.DbName);
            _job.DbCreatedByJob = true;
            step.Message = $"Založena databáze {_job.DbName}";
        }

        // ── 3. tabulky ──────────────────────────────────────────────────────────────

        private void Step_Schema(DbCreateStep step)
        {
            var target = Target();
            if (target.ObjectExists("dbo.x01License") && target.ObjectExists("dbo.j02User"))
            {
                step.Status = DbCreateStepStatus.Skipped;
                step.Message = "Tabulky už v databázi jsou.";
                return;
            }

            string s = BO.Code.File.GetFileContent(GetCreateDbScriptPath());
            var errors = target.RunScript(s, out int intBatches);
            foreach (var err in errors.Take(30))
            {
                Warn(step, err);
            }

            if (!target.ObjectExists("dbo.x01License") || !target.ObjectExists("dbo.j02User"))
            {
                throw new Exception($"Skript create_db.sql nevytvořil základní tabulky (chyb: {errors.Count}). {errors.FirstOrDefault()}");
            }

            int intTables = target.Scalar<int>("SELECT COUNT(*) FROM sys.tables");
            step.Message = $"{intTables} tabulek" + (errors.Count > 0 ? $", {errors.Count} chyb ve skriptu" : "");
        }

        // ── 4. procedury, funkce, pohledy ───────────────────────────────────────────

        private void Step_SqlObjects(DbCreateStep step)
        {
            var target = Target();
            if (_job.SqlObjectsDone)
            {
                step.Status = DbCreateStepStatus.Skipped;
                step.Message = "SQL objekty už jsou vygenerované.";
                return;
            }

            string s = BO.Code.File.GetFileContent(GetSqlObjectsScriptPath());
            var errors = target.RunScript(s, out int intBatches);
            foreach (var err in errors.Take(30))
            {
                Warn(step, err);
            }

            if (!target.ObjectExists("dbo.j02_load_sysuser"))
            {
                throw new Exception($"Skript sql_sp_funct_views.sql nevytvořil proceduru j02_load_sysuser (chyb: {errors.Count}). {errors.FirstOrDefault()}");
            }

            _job.SqlObjectsDone = true;
            int intObjects = target.Scalar<int>("SELECT COUNT(*) FROM sys.objects WHERE type IN ('P','FN','IF','TF','V')");
            step.Message = $"{intObjects} objektů" + (errors.Count > 0 ? $", {errors.Count} chyb ve skriptu" : "");
        }

        // ── 5. výchozí data ─────────────────────────────────────────────────────────

        private static readonly string[] _tables = new[]
        {
            "j27Currency","x15VatRateType","x24DataType","j19PaymentType","p70BillingStatus","p71ApproveStatus","p72PreBillingStatus","o33MediumType","o58GlobalParam","p33ActivityInputType",
            "x01License",
            "c24DayColor","c21FondCalendar","b20Hlidac",
            "x67EntityRole","x68EntityRole_Permission","j04UserRole","j07PersonPosition",
            "b01WorkflowTemplate","b02WorkflowStatus","b06WorkflowStep","b08WorkflowReceiverToStep",
            "r02CapacityVersion","x04NotepadConfig","o15AutoComplete",
            "p53VatRate",
            "x38CodeLogic","o21MilestoneType","p57TaskType","p07ProjectLevel","p42ProjectType","p80InvoiceAmountStructure",
            "p35Unit","p95InvoiceRow",
            "p38ActivityTag","p34ActivityGroup","p32Activity","o28ProjectRole_Workload","p43ProjectType_Workload","p61ActivityCluster","p62ActivityCluster_Item",
            "j25ReportCategory","x31Report","o27Attachment",
            "p29ContactType",
            "p86BankAccount","p93InvoiceHeader","p88InvoiceHeader_BankAccount",
            "p92InvoiceType","p89ProformaType",
            "x55Widget","x54WidgetGroup","x57WidgetToGroup","x59WidgetToUser",
            "o53TagGroup","o51Tag",
            "c26Holiday",
            "j02User",
            "j40MailAccount",
            "p51PriceList","p52PriceList_Item",
            "o17DocMenu","o18DocType","o16DocType_FieldSetting","o20DocTypeEntity"
        };

        private void Step_Data(DbCreateStep step)
        {
            var target = Target();
            int intTables = 0; int intRows = 0; int intSkipped = 0;

            foreach (string strTable in _tables)
            {
                int x = Import_One_Table(target, strTable, step);
                if (x < 0)
                {
                    intSkipped++;
                }
                else
                {
                    intTables++;
                    intRows += x;
                }
            }

            if (target.CountRows("x01License") == 0)
            {
                throw new Exception("Nepodařilo se založit záznam licence (x01License). " + step.Details.FirstOrDefault());
            }
            if (target.CountRows("j02User") == 0)
            {
                throw new Exception("Nepodařilo se založit prvního uživatele (j02User). " + step.Details.FirstOrDefault());
            }

            if (!_userInserted)
            {
                //uživatel vznikl při dřívějším pokusu - heslo sjednotit s aktuálním zadáním
                int intJ02ID = target.Scalar<int>("SELECT ISNULL((SELECT TOP 1 j02ID FROM j02User WHERE j02Login=@login),0)", new { login = R.UserLogin });
                if (intJ02ID == 0)
                {
                    throw new Exception($"V databázi už je první uživatel s jiným přihlašovacím jménem než [{R.UserLogin}]. Zadejte stejné přihlašovací jméno jako při prvním pokusu.");
                }
                string strHash = new BL.Code.PasswordSupport().GetPasswordHash(R.Password, R.UserLogin, intJ02ID);
                target.Exec("UPDATE j02User SET j02PasswordHash=@hash WHERE j02ID=@pid", new { hash = strHash, pid = intJ02ID });
            }

            step.Message = $"Naplněno {intTables} tabulek, {intRows} řádků" + (intSkipped > 0 ? $", {intSkipped} tabulek přeskočeno (už obsahují data)" : "");
        }

        /// <summary>Vrací počet vložených řádků, -1 pokud byla tabulka přeskočena.</summary>
        private int Import_One_Table(DL.DbCreateTasks target, string strTable, DbCreateStep step)
        {
            try
            {
                if (target.CountRows(strTable) > 0)
                {
                    return -1;  //tabulka již obsahuje data - byla dříve naimportovaná
                }
            }
            catch (Exception e)
            {
                Warn(step, $"{strTable}: {e.Message}");
                return -1;
            }

            wr_sql($"------dbo.{strTable}");
            DataTable dt = null;
            try
            {
                dt = basGuru.Load_DataTable_From_File(_f, strTable);
            }
            catch (Exception e)
            {
                wr_sql($"------dbo.{strTable}: {e.Message}");
                Warn(step, $"{strTable}: nelze načíst distribuční soubor {strTable}.dt ({e.Message})");
                return -1;
            }

            if (dt == null || dt.Rows.Count == 0) return -1;

            if (strTable == "x01License")
            {
                dt.Rows[0]["x01ApiKey"] = BO.Code.Bas.GetGuid();
                dt.Rows[0]["x01LoginDomain"] = R.LoginDomain;
                dt.Rows[0]["x01Name"] = R.LicenseName;
                dt.Rows[0]["x01AppName"] = R.AppName;
                dt.Rows[0]["x01AppHost"] = R.AppHost;
                dt.Rows[0]["x01CountryCode"] = R.CountryCode;
                dt.Rows[0]["x01LangIndex"] = R.LangIndex;
                dt.Rows[0]["x01ContactEmail"] = R.Email;
                dt.Rows[0]["x01ContactName"] = R.CompanyName;
                dt.Rows[0]["x01Round2Minutes"] = R.Round2Minutes;
                dt.Rows[0]["x01LimitUsers"] = R.LimitUsers;
                dt.Rows[0]["j27ID"] = R.CurrencyID;
                dt.Rows[0]["x04ID_Default"] = 2;
                dt.Rows[0]["x01IsAllowPasswordRecovery"] = true;
            }
            if (strTable == "j02User")
            {
                dt.Rows[0]["j02FirstName"] = R.FirstName;
                dt.Rows[0]["j02LastName"] = R.LastName;
                dt.Rows[0]["j02Email"] = R.Email;
                dt.Rows[0]["j02Login"] = R.UserLogin;
                dt.Rows[0]["j02LangIndex"] = R.LangIndex;
                string strHash = new BL.Code.PasswordSupport().GetPasswordHash(R.Password, R.UserLogin, Convert.ToInt32(dt.Rows[0]["j02ID"].ToString()));
                dt.Rows[0]["j02PasswordHash"] = strHash;
            }

            bool bolIdentity = target.TableHasIdentity(strTable);
            var cols = basGuru.GetColumns(dt);
            int intRows = 0; int intErrors = 0;

            foreach (DataRow dbRow in dt.Rows)
            {
                if (strTable == "c26Holiday" && dbRow["c26CountryCode"] != System.DBNull.Value && dbRow["c26CountryCode"].ToString() != R.CountryCode)
                {
                    continue;   //svátky pouze pro zemi licence
                }

                var sb = new System.Text.StringBuilder();
                if (bolIdentity) sb.AppendLine($"SET IDENTITY_INSERT dbo.{strTable} ON;");
                sb.Append($"INSERT INTO dbo.{strTable} (");
                sb.Append(string.Join(",", cols.Select(p => p.Key)));
                sb.Append(") VALUES(");
                for (int i = 0; i < cols.Count(); i++)
                {
                    if (i > 0) sb.Append(",");
                    sb.Append(basGuru.GVN(dbRow[cols[i].Key], cols[i].Key, _x01ID));
                }
                sb.AppendLine(");");
                if (bolIdentity) sb.AppendLine($"SET IDENTITY_INSERT dbo.{strTable} OFF;");

                string strErr = target.TryExec(sb.ToString());
                if (strErr == null)
                {
                    intRows++;
                    wr_sql(strTable == "j02User" ? "-- j02User: insert prvního uživatele (bez výpisu kvůli hash hesla)" : sb.ToString());
                }
                else
                {
                    intErrors++;
                    wr_sql($"-- CHYBA: {strErr}");
                    wr_sql(sb.ToString());
                    if (intErrors <= 3)
                    {
                        Warn(step, $"{strTable}: {strErr}");
                    }
                }
            }
            if (intErrors > 3)
            {
                Warn(step, $"{strTable}: celkem {intErrors} řádků se nepodařilo vložit.");
            }

            if (strTable == "j02User" && intRows > 0) _userInserted = true;

            return intRows;
        }

        private void wr_sql(string s)
        {
            _job.SqlLog.AppendLine(s);
        }

        // ── napojení BL vrstvy na novou databázi ────────────────────────────────────

        private void AttachTarget()
        {
            var ru = new BO.RunningUser() { j02Login = "setup" };
            _f.db = new DL.DbHandler(_job.TargetConnectString, ru, _f.App.LogFolder);
            _f.InhaleUserByLogin(R.UserLogin);  //aktuální uživatel je nutný
            if (_f.CurrentUser == null)
            {
                throw new Exception($"V nové databázi se nepodařilo načíst uživatele [{R.UserLogin}]. {ru.GetLastMessageNotify()}");
            }
            var recX01 = _f.x01LicenseBL.Load(_f.CurrentUser.x01ID);
            if (recX01 == null)
            {
                throw new Exception("V nové databázi se nepodařilo načíst licenci.");
            }
            _f.SetLic(recX01);  //_f.Lic musí ukazovat na licenci v nové databázi
        }

        private string TakeMessages()
        {
            if (_f.CurrentUser == null || _f.CurrentUser.Messages4Notify == null || _f.CurrentUser.Messages4Notify.Count == 0)
            {
                return null;
            }
            string s = string.Join(" | ", _f.CurrentUser.Messages4Notify.Select(p => p.Value));
            _f.CurrentUser.Messages4Notify.Clear();
            return s;
        }

        private void TryExecWarn(DL.DbCreateTasks target, DbCreateStep step, string strSQL, object param = null)
        {
            string strErr = target.TryExec(strSQL, param);
            if (strErr != null)
            {
                Warn(step, $"{strSQL}: {strErr}");
            }
        }

        private string GetCountryName()
        {
            switch (R.CountryCode)
            {
                case "CZ": return "Česká republika";
                case "SK": return "Slovenská republika";
                default: return null;
            }
        }

        // ── 6. licence a první uživatel ─────────────────────────────────────────────

        private void Step_License(DbCreateStep step)
        {
            var target = Target();
            target.TryExec("UPDATE x01License SET x01Guid=NEWID() WHERE x01Guid IS NULL"); //x01Guid je potřeba před založením složek licence

            AttachTarget();

            if (!_job.LicenseSaved)
            {
                var recX01 = _f.x01LicenseBL.Load(_f.CurrentUser.x01ID);
                recX01.x01RobotLogin = R.UserLogin;
                if (_f.x01LicenseBL.Save(recX01) == 0)   //založit/aktualizovat souvislosti s licencí + složky + a7_cloudheader
                {
                    throw new Exception("Licenci se nepodařilo uložit: " + TakeMessages());
                }
                _job.LicenseSaved = true;
                _f.SetLic(_f.x01LicenseBL.Load(_f.CurrentUser.x01ID));
            }

            if (IsSharedApp)
            {
                //projektové úrovně a menu dokumentů nové licence musí být i v a7_cloudheader
                try
                {
                    var cDL = new DL.HostingTasks(_f.db);
                    cDL.UpdateCloudHeader_P07();
                    cDL.UpdateCloudHeader_O17(R.LoginDomain);
                }
                catch (Exception e)
                {
                    Warn(step, "Aktualizace a7_cloudheader: " + e.Message);
                }
            }
            _f.App.RefreshP07List();

            TryExecWarn(target, step, "UPDATE p51PriceList SET p51ValidFrom=@d", new { d = new DateTime(DateTime.Now.Year, 1, 1) });
            foreach (var c in _f.p51PriceListBL.GetList(new BO.myQueryP51()))
            {
                TryExecWarn(target, step, "exec dbo.p51_aftersave @p51id,@j02id", new { p51id = c.pid, j02id = _f.CurrentUser.pid });    //aktualizace ceníků
            }
            TryExecWarn(target, step, "UPDATE p50OfficePriceList SET p50ValidFrom=@d", new { d = new DateTime(DateTime.Now.Year, 1, 1) });

            for (int i = -1; i < 4; i++)
            {
                TryExecWarn(target, step, $"exec dbo.c11_yearrecovery {DateTime.Now.Year - i}");
            }

            var dt = target.GetDataTable("SELECT TOP 1 c21ID FROM c21FondCalendar");
            foreach (DataRow dbRow in dt.Rows)
            {
                TryExecWarn(target, step, $"exec dbo.c21_aftersave {Convert.ToInt32(dbRow["c21ID"])},3");
            }

            //hlavička vystavovatele faktur
            var recP93 = _f.p93InvoiceHeaderBL.GetList(new BO.myQuery("p93")).FirstOrDefault();
            if (recP93 == null)
            {
                Warn(step, "V distribučních datech chybí hlavička vystavovatele faktur (p93InvoiceHeader).");
            }
            else
            {
                recP93.p93Name = R.CompanyName;
                recP93.p93Company = R.CompanyName;
                recP93.p93RegID = R.RegID;
                recP93.p93VatID = R.VatID;
                recP93.p93Street = R.Street;
                recP93.p93City = R.City;
                recP93.p93Zip = R.Zip;
                if (!string.IsNullOrEmpty(R.Registration)) recP93.p93Registration = R.Registration;
                if (!string.IsNullOrEmpty(R.IcDphSk)) recP93.p93ICDPH_SK = R.IcDphSk;
                if (GetCountryName() != null) recP93.p93Country = GetCountryName();

                if (_f.p93InvoiceHeaderBL.Save(recP93, null) == 0)
                {
                    Warn(step, "Hlavičku vystavovatele faktur se nepodařilo uložit: " + TakeMessages());
                }
            }

            //první uživatel
            var recJ02 = _f.j02UserBL.GetList(new BO.myQueryJ02()).First();
            recJ02.j02MyMenuLinks = _f.j02UserBL.GetDefaultMenuLinks(); //výchozí menu uživatele
            recJ02.j02HomePageUrl = null;
            if (_f.j02UserBL.Save(recJ02, null) == 0)
            {
                Warn(step, "Prvního uživatele se nepodařilo aktualizovat: " + TakeMessages());
            }

            step.Message = $"Licence {R.LicenseName}, uživatel {R.UserLogin}";
        }

        // ── 7. interní kontakt, první klient, projekty ──────────────────────────────

        private void Step_Contacts(DbCreateStep step)
        {
            var target = Target();
            AttachTarget();
            var recJ02 = _f.j02UserBL.GetList(new BO.myQueryJ02()).First();
            var lis = new List<string>();

            if (_job.IsResume)
            {
                //při navázání na dřívější pokus nezakládat kontakty a projekty podruhé
                if (_job.InternalP28ID == 0) _job.InternalP28ID = FindPid(target, "SELECT TOP 1 p28ID FROM p28Contact WHERE p29ID=@p29id ORDER BY p28ID", new { p29id = _p29ID_Interni });
                if (_job.InternalP41ID == 0 && _job.InternalP28ID > 0) _job.InternalP41ID = FindPid(target, "SELECT TOP 1 p41ID FROM p41Project WHERE p28ID_Client=@p28id ORDER BY p41ID", new { p28id = _job.InternalP28ID });
                if (_job.ClientP28ID == 0 && R.ClientCompanyName != null)
                {
                    _job.ClientP28ID = FindPid(target, "SELECT TOP 1 p28ID FROM p28Contact WHERE p29ID=@p29id AND p28CompanyName=@name ORDER BY p28ID", new { p29id = _p29ID_Klient, name = R.ClientCompanyName });
                    if (_job.ClientP28ID > 0)
                    {
                        if (R.Project1 != null) _job.ClientProject1Done = FindPid(target, "SELECT TOP 1 p41ID FROM p41Project WHERE p28ID_Client=@p28id AND p41Name=@name", new { p28id = _job.ClientP28ID, name = R.Project1 }) > 0;
                        if (R.Project2 != null) _job.ClientProject2Done = FindPid(target, "SELECT TOP 1 p41ID FROM p41Project WHERE p28ID_Client=@p28id AND p41Name=@name", new { p28id = _job.ClientP28ID, name = R.Project2 }) > 0;
                    }
                }
            }

            if (_job.InternalP28ID == 0)
            {
                var recP28 = new BO.p28Contact() { p29ID = _p29ID_Interni, j02ID_Owner = recJ02.pid, p28CountryCode = R.CountryCode, p28IsCompany = true, p28CompanyName = R.CompanyName, p28RegID = R.RegID, p28VatID = R.VatID, p28Street1 = R.Street, p28City1 = R.City, p28PostCode1 = R.Zip, p28ICDPH_SK = R.IcDphSk };
                recP28.p28Country1 = GetCountryName();
                _job.InternalP28ID = _f.p28ContactBL.Save(recP28, null, null, null, null, null);
                if (_job.InternalP28ID == 0)
                {
                    throw new Exception("Interní kontakt organizace se nepodařilo založit: " + TakeMessages());
                }
            }
            lis.Add("interní kontakt");

            if (_job.InternalP41ID == 0)
            {
                var recP41 = new BO.p41Project() { j02ID_Owner = recJ02.pid, p28ID_Client = _job.InternalP28ID, p41Name = "Interní", p42ID = _p42ID_InterniProjekt };
                var lisX69 = new List<BO.x69EntityRole_Assign>()
                {
                    new BO.x69EntityRole_Assign() { x67ID = _x67ID_ManazerProjektu, j02ID = recJ02.pid },
                    new BO.x69EntityRole_Assign() { x67ID = _x67ID_ClenProjektu, x69IsAllUsers = true },
                    new BO.x69EntityRole_Assign() { x67ID = _x67ID_VidiVsechnyHodiny, x69IsAllUsers = true }
                };
                _job.InternalP41ID = _f.p41ProjectBL.Save(recP41, null, lisX69, null);
                if (_job.InternalP41ID == 0)
                {
                    throw new Exception("Projekt [Interní] se nepodařilo založit: " + TakeMessages());
                }
                TryExecWarn(target, step, $"UPDATE p32Activity SET p41ID_Absence={_job.InternalP41ID} WHERE p41ID_Absence IS NOT NULL");
            }
            lis.Add("projekt Interní");

            if (R.CreateClient && !string.IsNullOrEmpty(R.ClientCompanyName))
            {
                if (_job.ClientP28ID == 0)
                {
                    var recP28 = new BO.p28Contact() { j02ID_Owner = recJ02.pid, p29ID = _p29ID_Klient, p28CountryCode = R.CountryCode, p28IsCompany = true, p28CompanyName = R.ClientCompanyName, p28RegID = R.ClientRegID, p28VatID = R.ClientVatID, p28Street1 = R.ClientStreet, p28City1 = R.ClientCity, p28PostCode1 = R.ClientZip, p28ICDPH_SK = R.ClientIcDphSk };
                    _job.ClientP28ID = _f.p28ContactBL.Save(recP28, null, null, null, null, null);
                    if (_job.ClientP28ID == 0)
                    {
                        Warn(step, $"Klienta [{R.ClientCompanyName}] se nepodařilo založit: " + TakeMessages());
                    }
                }
                if (_job.ClientP28ID > 0)
                {
                    lis.Add($"klient {R.ClientCompanyName}");
                    if (!_job.ClientProject1Done && !string.IsNullOrEmpty(R.Project1))
                    {
                        _job.ClientProject1Done = Zalozit_Klientsky_Projekt(step, recJ02.pid, R.Project1);
                    }
                    if (!_job.ClientProject2Done && !string.IsNullOrEmpty(R.Project2))
                    {
                        _job.ClientProject2Done = Zalozit_Klientsky_Projekt(step, recJ02.pid, R.Project2);
                    }
                    if (_job.ClientProject1Done) lis.Add($"projekt {R.Project1}");
                    if (_job.ClientProject2Done) lis.Add($"projekt {R.Project2}");
                }
            }

            step.Message = "Založeno: " + string.Join(", ", lis);
        }

        private int FindPid(DL.DbCreateTasks target, string strSQL, object param)
        {
            try
            {
                return target.Scalar<int>($"SELECT ISNULL(({strSQL}),0)", param);
            }
            catch
            {
                return 0;
            }
        }

        private bool Zalozit_Klientsky_Projekt(DbCreateStep step, int intJ02ID, string strP41Name)
        {
            var recP41 = new BO.p41Project() { j02ID_Owner = intJ02ID, p28ID_Client = _job.ClientP28ID, p41Name = strP41Name, p42ID = _p42ID_KlientskyProjekt };
            var lisX69 = new List<BO.x69EntityRole_Assign>()
            {
                new BO.x69EntityRole_Assign() { x67ID = _x67ID_ManazerProjektu, j02ID = intJ02ID },
                new BO.x69EntityRole_Assign() { x67ID = _x67ID_ClenProjektu, x69IsAllUsers = true }
            };
            if (_f.p41ProjectBL.Save(recP41, null, lisX69, null) == 0)
            {
                Warn(step, $"Projekt [{strP41Name}] se nepodařilo založit: " + TakeMessages());
                return false;
            }
            return true;
        }

        // ── 8. tiskové sestavy ──────────────────────────────────────────────────────

        private void Step_Reports(DbCreateStep step)
        {
            AttachTarget();
            string strSourceDir = DistributionFolder + "\\trdx";
            string strDestDir = $"{_f.App.RootUploadFolder}\\{R.LoginDomain}\\X31";
            if (!System.IO.Directory.Exists(strDestDir))
            {
                System.IO.Directory.CreateDirectory(strDestDir);
            }

            int intCopied = 0; int intMissing = 0;
            var lisX31 = _f.x31ReportBL.GetList(new BO.myQueryX31());
            foreach (var rec in lisX31)
            {
                if (string.IsNullOrEmpty(rec.ReportFileName)) continue;

                if (System.IO.File.Exists(strSourceDir + "\\" + rec.ReportFileName))
                {
                    System.IO.File.Copy(strSourceDir + "\\" + rec.ReportFileName, strDestDir + "\\" + rec.ReportFileName, true);
                    intCopied++;
                }
                else
                {
                    intMissing++;
                    if (intMissing <= 10) Warn(step, $"V distribuci chybí soubor sestavy {rec.ReportFileName}");
                }
            }

            step.Message = $"Zkopírováno {intCopied} sestav" + (intMissing > 0 ? $", {intMissing} chybí" : "");
        }

        // ── 9. dokončení ────────────────────────────────────────────────────────────

        private void Step_Finish(DbCreateStep step)
        {
            AttachTarget(); //zároveň ověřuje, že se první uživatel v nové databázi načte

            //obnovit singleton seznamy aplikace, aby se dalo hned přihlásit
            _f.App.RefreshX01List();
            _f.App.RefreshP07List();
            _f.App.RefreshO17List();

            if (IsSharedApp && !_f.App.lisX01.Any(p => p.x01LoginDomain == R.LoginDomain))
            {
                Warn(step, $"Licence [{R.LoginDomain}] se nepropsala do a7_cloudheader - přihlášení nebude fungovat, dokud se nepropíše (restart aplikace ji dorovná).");
            }

            try
            {
                string strLogPath = $"{_f.App.LogFolder}\\dbcreate-{_job.DbName}-{DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss")}.sql";
                BO.Code.File.WriteText2File(strLogPath, _job.SqlLog.ToString());
                _job.SqlLog.Clear();
            }
            catch (Exception e)
            {
                Warn(step, "SQL log se nepodařilo uložit: " + e.Message);
            }

            string strFrom = _f.App.Configuration.GetSection("DbCreate")["MailFrom"];
            if (string.IsNullOrEmpty(strFrom)) strFrom = "noreply@marktime.net";

            if (R.SendWelcomeMail)
            {
                try
                {
                    string s = $"<p>Dobrý den,</p><p>pro firmu <b>{System.Net.WebUtility.HtmlEncode(R.CompanyName)}</b> byla založena databáze systému MARKTIME.</p>";
                    s += $"<p>Adresa aplikace: <a href='{R.AppHost}'>{R.AppHost}</a><br>Přihlašovací jméno: <b>{R.UserLogin}</b></p>";
                    s += "<p>Heslo z bezpečnostních důvodů e-mailem neposíláme. Pokud ho neznáte, použijte na přihlašovací stránce obnovu hesla.</p>";
                    var res = _f.MailBL.SendMessageWithoutFactory(s, $"MARKTIME účet [{R.UserLogin}] založen", R.Email, null, strFrom, "MARKTIME");
                    if (!res.issuccess)
                    {
                        Warn(step, "Uvítací e-mail se nepodařilo odeslat: " + res.Message);
                    }
                }
                catch (Exception e)
                {
                    Warn(step, "Uvítací e-mail se nepodařilo odeslat: " + e.Message);
                }
            }

            string strNotify = _f.App.Configuration.GetSection("DbCreate")["NotifyEmail"];
            if (!string.IsNullOrEmpty(strNotify))
            {
                try
                {
                    string s = $"Databáze: {_job.DbName}<br>Firma: {System.Net.WebUtility.HtmlEncode(R.CompanyName)}<br>IČO: {System.Net.WebUtility.HtmlEncode(R.RegID)}<br>Login: {R.UserLogin}<br>E-mail: {R.Email}<br>Založil: {(_job.IsGuru ? "guru" : "anonym")} ({_job.RemoteIp})";
                    _f.MailBL.SendMessageWithoutFactory(s, $"Nová MARKTIME databáze [{_job.DbName}]", strNotify, null, strFrom, "MARKTIME");
                }
                catch
                {
                    //informační zpráva není pro výsledek podstatná
                }
            }

            _job.Request.Password = null;   //heslo už v paměti serveru není potřeba
            _job.Finished = true;
            step.Message = $"Hotovo. Přihlášení: {R.UserLogin}";
        }
    }
}
