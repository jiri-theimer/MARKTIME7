using System.Data;

namespace UI.Code.DbCreate
{
    /// <summary>
    /// Založení nové licence v režimu TotalCloud: všechny licence žijí v jedné databázi a rozlišuje je x01ID.
    /// Volá se z CreateLicenseFreeController.Create po ověření e-mailu.
    ///
    /// Zásady oproti původnímu řešení:
    ///  - ID nových řádků vrací přímo INSERT (SCOPE_IDENTITY), nikdy se nečtou přes MAX(id). Do databáze současně
    ///    zapisují ostatní zákazníci, takže MAX by mohl vrátit cizí řádek.
    ///  - Řádek licence (x01ID není identity) se zakládá v jedné krátké transakci se zámkem tabulky.
    ///  - Všechny zápisy BL vrstvy běží až po kontrole, že přihlášený uživatel patří do nové licence.
    ///  - Chyby se nepolykají: nepovedený řádek se zapíše do varování, bez licence, role nebo uživatele se končí.
    ///  - Když založení selže, rozpracovaná licence se uzavře a přejmenuje, aby e-mail nezůstal obsazený.
    ///
    /// Celé založení není jedna SQL transakce: záznamy zakládané přes BL (kontakty, projekty, licence) si otevírají
    /// vlastní spojení a dlouhá transakce by ve sdílené databázi blokovala ostatní zákazníky.
    /// </summary>
    public class TotalCloudRegisterEngine
    {
        private readonly BL.Factory _f;
        private readonly TotalCloudRegisterRequest R;
        private readonly TotalCloudRegisterResult _res = new TotalCloudRegisterResult();
        private readonly System.Text.StringBuilder _sql = new System.Text.StringBuilder(4000);
        private readonly Dictionary<string, Dictionary<int, int>> _keys = new Dictionary<string, Dictionary<int, int>>(StringComparer.Ordinal);  //pole primárního klíče -> (původní ID z distribuce -> nové ID)
        private readonly Dictionary<string, TableMeta> _meta = new Dictionary<string, TableMeta>();
        private readonly Dictionary<string, List<int>> _dangling = new Dictionary<string, List<int>>();     //sloupec -> původní ID, která nejsou v distribuci

        private int _x01ID;
        private int _j02ID;
        private int _p28ID_Interni;
        private int _p41ID_Interni;
        private int _p28ID_Klient;
        private TotalCloudProfile _profile;

        //tabulky se plní ve stejném pořadí jako dosud; pořadí je dané cizími klíči
        private static readonly string[] _tablesBeforeContacts = new[]
        {
            "c24DayColor","c21FondCalendar","b20Hlidac",
            "x67EntityRole","x68EntityRole_Permission","j04UserRole","j07PersonPosition",
            "b01WorkflowTemplate","b02WorkflowStatus","b06WorkflowStep","b08WorkflowReceiverToStep",
            "r02CapacityVersion","x04NotepadConfig","o15AutoComplete",
            "p53VatRate","x38CodeLogic","o21MilestoneType","p57TaskType","p07ProjectLevel","p42ProjectType","p80InvoiceAmountStructure",
            "p35Unit","p95InvoiceRow",
            "p38ActivityTag","p34ActivityGroup",
            "p29ContactType",
            "j02User"
        };
        private static readonly string[] _tablesAfterContacts = new[]
        {
            "p32Activity","o28ProjectRole_Workload","p43ProjectType_Workload","p61ActivityCluster","p62ActivityCluster_Item",
            "j25ReportCategory","x31Report","o27Attachment",
            "p86BankAccount","p93InvoiceHeader","p88InvoiceHeader_BankAccount",
            "p92InvoiceType","p89ProformaType",
            "x55Widget","x54WidgetGroup","x57WidgetToGroup","x59WidgetToUser",
            "o53TagGroup","o51Tag",
            "c26Holiday",
            "j40MailAccount",
            "p51PriceList","p52PriceList_Item",
            "o17DocMenu","o18DocType","o16DocType_FieldSetting","o20DocTypeEntity"
        };

        private class TableMeta
        {
            public bool Exists;
            public string IdentityColumn;
            public HashSet<string> Columns = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            public HashSet<string> ReadOnlyColumns = new HashSet<string>(StringComparer.OrdinalIgnoreCase);   //počítané sloupce a rowversion
            public Dictionary<string, int> MaxLength = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        }

        /// <summary>Hodnota, která se do INSERTu vloží jako SQL výraz, ne jako parametr.</summary>
        private class SqlRaw
        {
            public string Sql;
            public SqlRaw(string s) { Sql = s; }
        }

        public TotalCloudRegisterEngine(BL.Factory f, TotalCloudRegisterRequest r)
        {
            _f = f;
            R = r;
        }

        private DL.DbCreateTasks Db()
        {
            return new DL.DbCreateTasks(_f.App.ConnectString, 120);
        }

        /// <summary>Text pro uživatele, pokud zadání nelze přijmout; NULL = v pořádku. Volat před Run().</summary>
        public string Validate()
        {
            if (_f.App.HostingMode != BL.Singleton.HostingModeEnum.TotalCloud)
            {
                return "Aplikace není v režimu TotalCloud.";
            }
            var db = Db();
            if (db.Scalar<int>("SELECT COUNT(*) FROM j02User WHERE j02Login=@login OR j02Email=@login", new { login = R.Login }) > 0)
            {
                return "Účet s tímto e-mailem už existuje. Přihlaste se, případně použijte obnovu hesla.";
            }
            if (db.Scalar<int>("SELECT COUNT(*) FROM x01License WHERE x01LoginDomain=@domain", new { domain = R.Login }) > 0)
            {
                return "Pro tento e-mail už licence existuje. Kontaktujte prosím naši podporu.";
            }
            foreach (string strField in new[] { "j02User.j02Login", "j02User.j02Email", "x01License.x01LoginDomain", "x01License.x01ContactEmail" })
            {
                var arr = strField.Split(".");
                int intMax = GetMaxLength(arr[0], arr[1]);
                if (intMax > 0 && R.Login.Length > intMax)
                {
                    return $"E-mailová adresa je příliš dlouhá (nejvýše {intMax} znaků).";
                }
            }
            return null;
        }

        public TotalCloudRegisterResult Run()
        {
            _res.Login = R.Login;
            _profile = TotalCloudProfile.Find(R.ProfileKey);
            wr_sql($"-- registrace TotalCloud {DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss")}, {R.Login}, obor: {_profile.Key}, IP: {R.RemoteIp}");

            try
            {
                Step_License();
                Step_Tables(_tablesBeforeContacts);
                Step_User();
                Step_Contacts();
                Step_Tables(_tablesAfterContacts);
                Step_Finish();
                foreach (var kv in _dangling)
                {
                    Warn($"{kv.Key}: {kv.Value.Count}× odkaz na záznam, který distribuce neobsahuje (ID {string.Join(", ", kv.Value.Distinct().Take(5))}). Hodnota zůstala beze změny a může ukazovat na záznam jiné licence.");
                }
                _res.Ok = true;
            }
            catch (Exception e)
            {
                _res.Ok = false;
                _res.Error = e.Message;
                wr_sql($"-- CHYBA: {e.Message}");
                try
                {
                    BO.Code.File.LogError(e.ToString(), R.Login, "TotalCloudRegisterEngine");
                }
                catch
                {
                    //log není pro výsledek podstatný
                }
                Rollback();
            }

            _res.x01ID = _x01ID;
            _res.j02ID = _j02ID;
            SaveSqlLog();
            return _res;
        }

        // ── pomocné ─────────────────────────────────────────────────────────────────

        private void wr_sql(string s)
        {
            _sql.AppendLine(s);
        }

        private void Warn(string s)
        {
            _res.Warnings.Add(s);
            wr_sql($"-- VAROVÁNÍ: {s}");
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

        private void TryExecWarn(string strSQL, object param = null)
        {
            string strErr = Db().TryExec(strSQL, param);
            if (strErr != null)
            {
                Warn($"{strSQL}: {strErr}");
            }
        }

        private int FindPid(string strSQL, object param)
        {
            try
            {
                return Db().Scalar<int>($"SELECT ISNULL(({strSQL}),0)", param);
            }
            catch (Exception e)
            {
                Warn($"{strSQL}: {e.Message}");
                return 0;
            }
        }

        /// <summary>
        /// Najde podle názvu záznam mezi těmi, které tento běh právě založil. Hledá se přes nová ID, ne přes x01ID,
        /// takže výsledek nezávisí na tom, zda distribuční soubor tabulky sloupec x01ID obsahuje.
        /// </summary>
        private int FindNewPid(string strTable, string strPK, string strNameField, string strName)
        {
            var ids = NewKeys(strPK);
            if (ids.Count == 0) return 0;
            return FindPid($"SELECT TOP 1 {strPK} FROM {strTable} WHERE {strPK} IN ({string.Join(",", ids)}) AND {strNameField} LIKE @name ORDER BY {strPK}", new { name = strName });
        }

        private TableMeta GetMeta(string strTable)
        {
            if (_meta.ContainsKey(strTable)) return _meta[strTable];

            var c = new TableMeta();
            //název tabulky je vždy z pevného seznamu v tomto souboru
            var dt = Db().GetDataTable($"SELECT c.name,c.is_identity,c.is_computed,t.name AS typename,c.max_length FROM sys.columns c INNER JOIN sys.types t ON c.user_type_id=t.user_type_id WHERE c.object_id=OBJECT_ID('dbo.{strTable}')");
            foreach (DataRow row in dt.Rows)
            {
                string strCol = row["name"].ToString();
                string strType = row["typename"].ToString().ToLower();
                c.Exists = true;
                c.Columns.Add(strCol);
                if (Convert.ToBoolean(row["is_identity"])) c.IdentityColumn = strCol;
                if (Convert.ToBoolean(row["is_computed"]) || strType == "timestamp" || strType == "rowversion") c.ReadOnlyColumns.Add(strCol);
                if (strType == "varchar" || strType == "char" || strType == "nvarchar" || strType == "nchar")
                {
                    int intLen = Convert.ToInt32(row["max_length"]);
                    if (intLen > 0 && (strType == "nvarchar" || strType == "nchar")) intLen = intLen / 2;
                    c.MaxLength[strCol] = intLen;   //-1 = (max)
                }
            }
            _meta[strTable] = c;
            return c;
        }

        /// <summary>Délka textového sloupce ve znacích; 0 = neznámá nebo neomezená.</summary>
        private int GetMaxLength(string strTable, string strColumn)
        {
            var c = GetMeta(strTable);
            if (c.MaxLength.ContainsKey(strColumn) && c.MaxLength[strColumn] > 0)
            {
                return c.MaxLength[strColumn];
            }
            return 0;
        }

        /// <summary>Zkrátí text na délku sloupce, aby dlouhý název firmy nebo jméno neshodily INSERT.</summary>
        private string Fit(string strTable, string strColumn, string strValue)
        {
            if (string.IsNullOrEmpty(strValue)) return strValue;
            strValue = strValue.Trim();
            int intMax = GetMaxLength(strTable, strColumn);
            if (intMax > 0 && strValue.Length > intMax)
            {
                return strValue.Substring(0, intMax);
            }
            return strValue;
        }

        private void AddKey(string strField, int intOrig, int intNew)
        {
            if (!_keys.ContainsKey(strField)) _keys[strField] = new Dictionary<int, int>();
            _keys[strField][intOrig] = intNew;
        }

        /// <summary>Nové ID k původnímu ID z distribuce; 0 = záznam nebyl naimportován.</summary>
        private int MapKey(string strField, int intOrig)
        {
            if (_keys.ContainsKey(strField) && _keys[strField].ContainsKey(intOrig))
            {
                return _keys[strField][intOrig];
            }
            return 0;
        }

        private List<int> NewKeys(string strField)
        {
            if (!_keys.ContainsKey(strField)) return new List<int>();
            return _keys[strField].Values.ToList();
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

        // ── vložení jednoho řádku ───────────────────────────────────────────────────

        /// <summary>
        /// Vloží řádek z distribuce a vrátí ID nového záznamu (0 u tabulky bez primárního klíče {prefix}ID).
        /// Hodnoty z distribuce jdou do SQL stejným převodem jako dosud (basGuru.GVN), hodnoty od uživatele
        /// (overrides) vždy jako parametry. Při chybě vyhazuje výjimku.
        /// </summary>
        private int InsertRow(string strTable, DataTable dt, DataRow dbRow, Dictionary<string, object> overrides)
        {
            var meta = GetMeta(strTable);
            string strPK = strTable.Substring(0, 3) + "ID";
            bool bolIdentity = (meta.IdentityColumn != null);
            bool bolManualKey = (!bolIdentity && meta.Columns.Contains(strPK));

            var fields = new List<string>();
            var values = new List<string>();
            var pars = new Dictionary<string, object>();

            if (bolManualKey)
            {
                fields.Add($"[{strPK}]");
                values.Add("@newpid");
            }

            foreach (DataColumn col in dt.Columns)
            {
                string strField = col.ColumnName;
                if (!meta.Columns.Contains(strField) || meta.ReadOnlyColumns.Contains(strField)) continue;
                if (bolIdentity && string.Equals(strField, meta.IdentityColumn, StringComparison.OrdinalIgnoreCase)) continue;
                if (bolManualKey && string.Equals(strField, strPK, StringComparison.OrdinalIgnoreCase)) continue;

                string strValue;
                if (overrides != null && overrides.ContainsKey(strField))
                {
                    object o = overrides[strField];
                    if (o == null)
                    {
                        strValue = "NULL";
                    }
                    else if (o is SqlRaw)
                    {
                        strValue = ((SqlRaw)o).Sql;
                    }
                    else
                    {
                        string strPar = "p" + pars.Count.ToString();
                        pars.Add(strPar, o);
                        strValue = "@" + strPar;
                    }
                }
                else
                {
                    strValue = GetDistributionValue(strTable, strField, dbRow);
                }

                fields.Add($"[{strField}]");
                values.Add(strValue);
            }

            if (strTable != "x01License" && meta.Columns.Contains("x01ID") && !dt.Columns.Contains("x01ID") && !meta.ReadOnlyColumns.Contains("x01ID"))
            {
                //tabulka patří licenci, ale distribuční soubor sloupec x01ID nemá (např. p42ProjectType)
                fields.Add("[x01ID]");
                values.Add(_x01ID.ToString());
            }

            if (overrides != null)
            {
                //hodnoty, které distribuční soubor neobsahuje, ale tabulka je má
                foreach (var kv in overrides)
                {
                    if (dt.Columns.Contains(kv.Key) || !meta.Columns.Contains(kv.Key) || meta.ReadOnlyColumns.Contains(kv.Key) || kv.Value == null) continue;
                    if (bolIdentity && string.Equals(kv.Key, meta.IdentityColumn, StringComparison.OrdinalIgnoreCase)) continue;

                    fields.Add($"[{kv.Key}]");
                    if (kv.Value is SqlRaw)
                    {
                        values.Add(((SqlRaw)kv.Value).Sql);
                    }
                    else
                    {
                        string strPar = "p" + pars.Count.ToString();
                        pars.Add(strPar, kv.Value);
                        values.Add("@" + strPar);
                    }
                }
            }

            string strInsert = $"INSERT INTO dbo.[{strTable}] ({string.Join(",", fields)}) VALUES ({string.Join(",", values)});";
            string strSQL;
            if (bolIdentity)
            {
                strSQL = strInsert + " SELECT CAST(SCOPE_IDENTITY() AS int);";
            }
            else if (bolManualKey)
            {
                //tabulka bez identity: nové ID se přidělí pod zámkem tabulky v jedné krátké transakci
                strSQL = $"SET XACT_ABORT ON; BEGIN TRAN; DECLARE @newpid int; SELECT @newpid=ISNULL(MAX([{strPK}]),0)+1 FROM dbo.[{strTable}] WITH (TABLOCKX, HOLDLOCK); {strInsert} COMMIT TRAN; SELECT @newpid;";
            }
            else
            {
                strSQL = strInsert + " SELECT 0;";
            }

            wr_sql(strSQL);
            int intNewID = Db().Scalar<int>(strSQL, pars.Count > 0 ? pars : null);
            if ((bolIdentity || bolManualKey) && intNewID == 0)
            {
                throw new Exception("Řádek byl vložen, ale databáze nevrátila jeho ID.");
            }
            return intNewID;
        }

        /// <summary>Hodnota sloupce z distribučního souboru jako SQL výraz, včetně přepočtu cizích klíčů na nová ID.</summary>
        private string GetDistributionValue(string strTable, string strField, DataRow dbRow)
        {
            object raw = dbRow[strField];
            string strValue = basGuru.GVN(raw, strField, _x01ID);

            if (strTable == "p32Activity" && strField == "p41ID_Absence")
            {
                //absence se vykazují do projektu Interní nové licence; ostatní aktivity projekt absence nemají
                bool bolAbsence = (strValue != "NULL");
                if (!bolAbsence && dbRow.Table.Columns.Contains("p32AbsenceFlag") && dbRow["p32AbsenceFlag"] != System.DBNull.Value)
                {
                    bolAbsence = (basGuru.GVN(dbRow["p32AbsenceFlag"], "p32AbsenceFlag", _x01ID) != "0");
                }
                return (bolAbsence && _p41ID_Interni > 0) ? _p41ID_Interni.ToString() : "NULL";
            }
            if (raw == System.DBNull.Value) return strValue;

            string strKeyField = null;  //primární klíč tabulky, na kterou sloupec odkazuje
            if (_keys.ContainsKey(strField)) strKeyField = strField;
            if (strTable == "o27Attachment" && strField == "o27RecordPid") strKeyField = "x31ID";
            if (strTable == "b06WorkflowStep" && strField == "b02ID_Target") strKeyField = "b02ID";
            if (strTable == "b06WorkflowStep" && strField == "x67ID_Direct") strKeyField = "x67ID";
            if (strKeyField == null) return strValue;

            int intOrig;
            try
            {
                intOrig = Convert.ToInt32(raw);
            }
            catch
            {
                return strValue;
            }
            if (intOrig == 0) return strValue;

            int intNew = MapKey(strKeyField, intOrig);
            if (intNew > 0) return intNew.ToString();

            if (strKeyField != strField)
            {
                //odkaz, který musí vést na záznam nové licence
                throw new Exception($"sloupec {strField} odkazuje na {strKeyField}={intOrig}, který nebyl naimportován");
            }
            //záznam s tímto ID distribuce neobsahuje: hodnota zůstává jako dosud, ale správce se o tom dozví
            wr_sql($"-- pozor: {strTable}.{strField}={intOrig} odkazuje na záznam, který není v distribuci; hodnota ponechána");
            string strKey = $"{strTable}.{strField}";
            if (!_dangling.ContainsKey(strKey)) _dangling[strKey] = new List<int>();
            _dangling[strKey].Add(intOrig);
            return strValue;
        }

        // ── 1. licence ──────────────────────────────────────────────────────────────

        private void Step_License()
        {
            wr_sql("------x01License");
            DataTable dt = basGuru.Load_DataTable_From_File(_f, "x01License");
            if (dt == null || dt.Rows.Count == 0)
            {
                throw new Exception("Distribuční soubor x01License.dt je prázdný.");
            }

            string strAppName = R.CompanyName.Trim();
            if (strAppName.Length > 20) strAppName = strAppName.Substring(0, 20);

            var o = new Dictionary<string, object>();
            o["x01Guid"] = new SqlRaw("NEWID()");
            o["x01ApiKey"] = BO.Code.Bas.GetGuid().Substring(0, 8).ToUpper();
            o["x01LoginDomain"] = R.Login;
            o["x01Name"] = Fit("x01License", "x01Name", R.CompanyName);
            o["x01AppName"] = Fit("x01License", "x01AppName", strAppName);
            o["x01AppHost"] = R.AppHost;
            o["x01CountryCode"] = R.CountryCode;
            o["x01LangIndex"] = 0;
            o["x01ContactEmail"] = R.Email;
            o["x01ContactName"] = Fit("x01License", "x01ContactName", R.FirstName + " " + R.LastName);
            o["x01Round2Minutes"] = 15;
            o["x01LimitUsers"] = 99;
            o["x01RecordsCountP31"] = 0;
            o["x01RobotLogin"] = R.Login;
            o["j27ID"] = (R.CountryCode == "CZ" ? 2 : 3);     //CZK / EUR
            o["x01IsAllowPasswordRecovery"] = true;

            _x01ID = InsertRow("x01License", dt, dt.Rows[0], o);
            if (_x01ID == 0)
            {
                throw new Exception("Nepodařilo se založit záznam licence (x01License).");
            }
            _res.Tables++; _res.Rows++;

            _f.App.RefreshX01List();    //nová licence musí být v seznamu licencí aplikace dřív, než se pod ní pracuje přes BL
        }

        // ── 2. a 5. výchozí data ────────────────────────────────────────────────────

        private void Step_Tables(string[] tables)
        {
            foreach (string strTable in tables)
            {
                Import_One_Table(strTable);

                if (strTable == "j04UserRole" && NewKeys("j04ID").Count == 0)
                {
                    throw new Exception("Nepodařilo se založit aplikační role (j04UserRole) - bez role se uživatel nepřihlásí.");
                }
                if (strTable == "j02User" && _j02ID == 0)
                {
                    throw new Exception("Nepodařilo se založit uživatele (j02User).");
                }
            }
        }

        private void Import_One_Table(string strTable)
        {
            wr_sql($"------{strTable}");
            if (!System.IO.File.Exists($"{_f.App.RootUploadFolder}\\_distribution\\{strTable}.dt"))
            {
                wr_sql($"-- distribuční soubor {strTable}.dt neexistuje, tabulka přeskočena");
                return;
            }
            DataTable dt = null;
            try
            {
                dt = basGuru.Load_DataTable_From_File(_f, strTable);
            }
            catch (Exception e)
            {
                Warn($"{strTable}: nelze načíst distribuční soubor {strTable}.dt ({e.Message})");
                return;
            }
            if (dt == null || dt.Rows.Count == 0) return;

            var meta = GetMeta(strTable);
            if (!meta.Exists)
            {
                Warn($"{strTable}: tabulka v databázi neexistuje.");
                return;
            }
            var lisMissing = new List<string>();
            foreach (DataColumn col in dt.Columns)
            {
                if (!meta.Columns.Contains(col.ColumnName)) lisMissing.Add(col.ColumnName);
            }
            if (lisMissing.Count > 0)
            {
                Warn($"{strTable}: distribuční soubor obsahuje sloupce, které v tabulce nejsou ({string.Join(", ", lisMissing)}); byly vynechány.");
            }

            if (strTable != "x01License" && meta.Columns.Contains("x01ID") && !dt.Columns.Contains("x01ID"))
            {
                wr_sql($"-- distribuční soubor {strTable}.dt nemá sloupec x01ID, doplňuje se x01ID nové licence");
            }

            string strPK = strTable.Substring(0, 3) + "ID";
            bool bolHasOrigKey = dt.Columns.Contains(strPK);
            int intRows = 0; int intErrors = 0;

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                DataRow dbRow = dt.Rows[i];
                Dictionary<string, object> o = null;

                if (strTable == "c26Holiday" && dt.Columns.Contains("c26CountryCode") && dbRow["c26CountryCode"] != System.DBNull.Value && dbRow["c26CountryCode"].ToString() != R.CountryCode)
                {
                    continue;   //svátky pouze pro zemi licence
                }
                if (strTable == "j02User")
                {
                    if (i > 0)
                    {
                        Warn("j02User: distribuční soubor obsahuje více uživatelů, založen byl jen první.");
                        break;
                    }
                    o = new Dictionary<string, object>();
                    o["j02FirstName"] = Fit("j02User", "j02FirstName", R.FirstName);
                    o["j02LastName"] = Fit("j02User", "j02LastName", R.LastName);
                    o["j02Email"] = R.Email;
                    o["j02Login"] = R.Login;
                    o["j02LangIndex"] = 0;
                }

                try
                {
                    int intNewID = InsertRow(strTable, dt, dbRow, o);
                    intRows++;
                    if (intNewID > 0 && bolHasOrigKey && dbRow[strPK] != System.DBNull.Value)
                    {
                        AddKey(strPK, Convert.ToInt32(dbRow[strPK]), intNewID);
                    }
                    if (strTable == "j02User") _j02ID = intNewID;
                }
                catch (Exception e)
                {
                    intErrors++;
                    wr_sql($"-- CHYBA: {e.Message}");
                    if (intErrors <= 3)
                    {
                        Warn($"{strTable}: {e.Message}");
                    }
                }
            }
            if (intErrors > 3)
            {
                Warn($"{strTable}: celkem {intErrors} řádků se nepodařilo vložit.");
            }

            _res.Tables++;
            _res.Rows += intRows;
        }

        // ── 3. první uživatel ───────────────────────────────────────────────────────

        private void Step_User()
        {
            string strHash = new BL.Code.PasswordSupport().GetPasswordHash(R.Password, R.Login, _j02ID);
            Db().Exec("UPDATE j02User SET j02PasswordHash=@hash WHERE j02ID=@pid", new { hash = strHash, pid = _j02ID });

            _f.App.RefreshP07List();    //projektové úrovně nové licence se čtou ze seznamu v paměti aplikace
            AttachUser();
        }

        /// <summary>
        /// Přepne BL vrstvu na nového uživatele. Od této chvíle zapisuje BL do nové licence; kontrola x01ID je pojistka,
        /// aby se při jakékoli chybě nic nezapsalo do licence jiného zákazníka.
        /// </summary>
        private void AttachUser()
        {
            _f.InhaleUserByLogin(R.Login);
            if (_f.CurrentUser == null)
            {
                throw new Exception($"Nového uživatele [{R.Login}] se nepodařilo načíst (j02_load_sysuser).");
            }
            if (_f.CurrentUser.x01ID != _x01ID || _f.CurrentUser.pid != _j02ID)
            {
                throw new Exception($"Uživatel [{R.Login}] se načetl s jinou licencí (x01ID={_f.CurrentUser.x01ID}, čekáno {_x01ID}).");
            }
            var recX01 = _f.App.lisX01.FirstOrDefault(p => p.pid == _x01ID);
            if (recX01 == null)
            {
                throw new Exception("Nová licence není v seznamu licencí aplikace.");
            }
            _f.SetLic(recX01);
        }

        // ── 4. interní kontakt, ukázkový klient, projekty ───────────────────────────

        private void Step_Contacts()
        {
            //interní kontakt = firma uživatele
            int intP29ID = FindNewPid("p29ContactType", "p29ID", "p29Name", "Interní");
            if (intP29ID == 0)
            {
                Warn("Typ kontaktu [Interní] v distribuci chybí - interní kontakt a projekt Interní nebyly založeny.");
            }
            else
            {
                var recP28 = new BO.p28Contact() { p29ID = intP29ID, j02ID_Owner = _j02ID, p28CountryCode = R.CountryCode, p28IsCompany = true, p28CompanyName = R.CompanyName, p28RegID = R.RegID, p28VatID = R.VatID, p28Street1 = R.Street, p28City1 = R.City, p28PostCode1 = R.Zip };
                if (!string.IsNullOrEmpty(R.City)) recP28.p28Country1 = GetCountryName();
                _p28ID_Interni = _f.p28ContactBL.Save(recP28, null, null, null, null, null);
                if (_p28ID_Interni == 0)
                {
                    Warn("Interní kontakt se nepodařilo založit: " + TakeMessages());
                }
            }

            if (_p28ID_Interni > 0)
            {
                int intP42ID = FindNewPid("p42ProjectType", "p42ID", "p42Name", "Interní projekt");
                if (intP42ID == 0)
                {
                    Warn("Typ projektu [Interní projekt] v distribuci chybí - projekt Interní nebyl založen.");
                }
                else
                {
                    var recP41 = new BO.p41Project() { p41Name = "Interní", p28ID_Client = _p28ID_Interni, j02ID_Owner = _j02ID, p42ID = intP42ID };
                    _p41ID_Interni = _f.p41ProjectBL.Save(recP41, null, null, null);
                    if (_p41ID_Interni == 0)
                    {
                        Warn("Projekt [Interní] se nepodařilo založit: " + TakeMessages());
                    }
                }
            }

            //ukázkový klient a projekty podle oboru
            int intP29ID_Klient = FindNewPid("p29ContactType", "p29ID", "p29Name", "Klient");
            if (intP29ID_Klient == 0)
            {
                Warn("Typ kontaktu [Klient] v distribuci chybí - ukázkový klient nebyl založen.");
                return;
            }
            var recKlient = new BO.p28Contact() { p29ID = intP29ID_Klient, j02ID_Owner = _j02ID, p28CountryCode = R.CountryCode, p28IsCompany = true, p28CompanyName = _profile.ClientName };
            _p28ID_Klient = _f.p28ContactBL.Save(recKlient, null, null, null, null, null);
            if (_p28ID_Klient == 0)
            {
                Warn($"Ukázkového klienta [{_profile.ClientName}] se nepodařilo založit: " + TakeMessages());
                return;
            }

            int intP42ID_Klient = FindNewPid("p42ProjectType", "p42ID", "p42Name", "Klientský projekt");
            if (intP42ID_Klient == 0)
            {
                intP42ID_Klient = NewKeys("p42ID").FirstOrDefault();
            }
            if (intP42ID_Klient == 0)
            {
                Warn("V distribuci chybí typ projektu - ukázkové projekty nebyly založeny.");
                return;
            }
            foreach (string strName in _profile.Projects)
            {
                var recP41 = new BO.p41Project() { p41Name = strName, p28ID_Client = _p28ID_Klient, j02ID_Owner = _j02ID, p42ID = intP42ID_Klient };
                if (_f.p41ProjectBL.Save(recP41, null, null, null) == 0)
                {
                    Warn($"Projekt [{strName}] se nepodařilo založit: " + TakeMessages());
                }
            }
        }

        // ── 6. dokončení ────────────────────────────────────────────────────────────

        private void Step_Finish()
        {
            //výchozí konfigurace editoru poznámek
            int intX04ID = FindNewPid("x04NotepadConfig", "x04ID", "x04Name", "Výchozí Editor");
            if (intX04ID == 0)
            {
                intX04ID = NewKeys("x04ID").FirstOrDefault();
            }
            if (intX04ID > 0)
            {
                TryExecWarn("UPDATE x01License SET x04ID_Default=@x04id WHERE x01ID=@x01id", new { x04id = intX04ID, x01id = _x01ID });
            }
            else
            {
                Warn("V distribuci chybí konfigurace editoru poznámek (x04NotepadConfig).");
            }

            //uložení licence přes BL: x01_aftersave, složky licence, obnova seznamu licencí
            AttachUser();
            var recX01 = _f.x01LicenseBL.Load(_x01ID);
            if (recX01 == null)
            {
                throw new Exception("Novou licenci se nepodařilo načíst.");
            }
            recX01.x01RobotLogin = R.Login;
            if (_f.x01LicenseBL.Save(recX01) == 0)
            {
                Warn("Licenci se nepodařilo uložit přes BL (složky licence nemusí existovat): " + TakeMessages());
            }
            _f.App.RefreshX01List();
            AttachUser();   //x01LicenseBL.Save přepisuje login přihlášeného uživatele na [guru]

            //první uživatel: výchozí menu
            TryExecWarn("UPDATE j02User SET j02MyMenuLinks=@links,j02HomePageUrl=NULL,j02Mobile=NULL WHERE j02ID=@pid", new { links = _f.j02UserBL.GetDefaultMenuLinks(), pid = _j02ID });

            Finish_AdminRole();
            Finish_InvoiceHeader();
            Finish_PriceLists();
            Finish_Calendars();
            Finish_ProfileActivities();

            _f.App.RefreshP07List();
            _f.App.RefreshO17List();
        }

        /// <summary>
        /// Aplikační role prvního uživatele (administrátor) dostane všechna oprávnění, která aplikace zná,
        /// a modul FILEBOX. Seznam oprávnění se bere z BO.Code.Entity.GetAllPermissions, takže nové oprávnění
        /// přidané do aplikace dostanou nové licence samy, bez úpravy distribučních souborů.
        /// </summary>
        private void Finish_AdminRole()
        {
            try
            {
                int intJ04ID = Db().Scalar<int>("SELECT ISNULL((SELECT j04ID FROM j02User WHERE j02ID=@pid),0)", new { pid = _j02ID });
                if (intJ04ID == 0 || !NewKeys("j04ID").Contains(intJ04ID))
                {
                    Warn("Aplikační roli prvního uživatele se nepodařilo určit - oprávnění zůstala podle distribuce.");
                    return;
                }
                var rec = _f.j04UserRoleBL.Load(intJ04ID);
                if (rec == null || rec.x01ID != _x01ID)
                {
                    Warn("Aplikační role prvního uživatele nepatří do nové licence - oprávnění zůstala podle distribuce.");
                    return;
                }
                rec.j04IsModule_o27 = true;
                var perms = BO.Code.Entity.GetAllPermissions("j04").Select(p => (int)p.Value).Where(p => p > 0).Distinct().ToList();
                if (_f.j04UserRoleBL.Save(rec, perms, null) == 0)   //x54ids=null: widgety role zůstávají podle distribuce
                {
                    Warn("Aplikační roli administrátora se nepodařilo uložit se všemi oprávněními: " + TakeMessages());
                    return;
                }
                AttachUser();   //načíst uživatele znovu, už s úplnými oprávněními
            }
            catch (Exception e)
            {
                Warn("Aplikační role administrátora: " + e.Message);
            }
        }

        /// <summary>Hlavička vystavovatele faktur dostane údaje firmy, aby první faktura nevyšla s údaji z distribuce.</summary>
        private void Finish_InvoiceHeader()
        {
            try
            {
                foreach (int intP93ID in NewKeys("p93ID"))
                {
                    var rec = _f.p93InvoiceHeaderBL.Load(intP93ID);
                    if (rec == null) continue;
                    rec.p93Name = R.CompanyName;
                    rec.p93Company = R.CompanyName;
                    rec.p93RegID = R.RegID;
                    rec.p93VatID = R.VatID;
                    rec.p93Street = R.Street;
                    rec.p93City = R.City;
                    rec.p93Zip = R.Zip;
                    if (GetCountryName() != null) rec.p93Country = GetCountryName();
                    if (_f.p93InvoiceHeaderBL.Save(rec, null) == 0)
                    {
                        Warn("Hlavičku vystavovatele faktur se nepodařilo uložit: " + TakeMessages());
                    }
                    break;  //pouze první hlavička
                }
            }
            catch (Exception e)
            {
                Warn("Hlavička vystavovatele faktur: " + e.Message);
            }
        }

        /// <summary>Ceníky platí od začátku roku, aby se ocenily i úkony vykázané zpětně.</summary>
        private void Finish_PriceLists()
        {
            foreach (int intP51ID in NewKeys("p51ID"))
            {
                TryExecWarn("UPDATE p51PriceList SET p51ValidFrom=@d WHERE p51ID=@pid", new { d = new DateTime(DateTime.Now.Year, 1, 1), pid = intP51ID });
                TryExecWarn("exec dbo.p51_aftersave @p51id,@j02id", new { p51id = intP51ID, j02id = _j02ID });
            }
        }

        private void Finish_Calendars()
        {
            foreach (int intC21ID in NewKeys("c21ID"))
            {
                TryExecWarn("exec dbo.c21_aftersave @c21id,@j02id", new { c21id = intC21ID, j02id = _j02ID });
            }
        }

        /// <summary>
        /// Aktivity podle oboru. Nová aktivita přebírá sešit, fakturační oddíl, jednotku a DPH od první
        /// fakturovatelné hodinové aktivity z distribuce; aktivita, jejíž název už v licenci je, se nezakládá.
        /// </summary>
        private void Finish_ProfileActivities()
        {
            if (_profile.Activities == null || _profile.Activities.Length == 0) return;
            try
            {
                var lis = new List<BO.p32Activity>();
                foreach (int intP32ID in NewKeys("p32ID"))
                {
                    var c = _f.p32ActivityBL.Load(intP32ID);
                    if (c != null) lis.Add(c);
                }
                var recTemplate = lis.OrderBy(p => p.p32Ordinary).ThenBy(p => p.pid).FirstOrDefault(p => p.p33ID == (int)BO.p33IdENUM.Cas && p.p32IsBillable && p.p32AbsenceFlag == BO.p32AbsenceFlagENUM._None && p.p32SystemFlag == BO.p32SystemFlagENUM._None);
                if (recTemplate == null)
                {
                    Warn("Aktivity podle oboru nebyly založeny: v distribuci není fakturovatelná hodinová aktivita, ze které by převzaly nastavení.");
                    return;
                }
                foreach (string strName in _profile.Activities)
                {
                    if (lis.Any(p => string.Equals(p.p32Name, strName, StringComparison.CurrentCultureIgnoreCase))) continue;

                    var rec = new BO.p32Activity() { p32Name = strName, p34ID = recTemplate.p34ID, p95ID = recTemplate.p95ID, p35ID = recTemplate.p35ID, p38ID = recTemplate.p38ID, x15ID = recTemplate.x15ID, p32IsBillable = true, p32Ordinary = recTemplate.p32Ordinary };
                    if (_f.p32ActivityBL.Save(rec) == 0)
                    {
                        Warn($"Aktivitu [{strName}] se nepodařilo založit: " + TakeMessages());
                    }
                }
            }
            catch (Exception e)
            {
                Warn("Aktivity podle oboru: " + e.Message);
            }
        }

        // ── selhání a log ───────────────────────────────────────────────────────────

        /// <summary>
        /// Rozpracovanou licenci nelze spolehlivě smazat (část záznamů zakládá BL), proto se uzavře a přejmenuje:
        /// e-mail se uvolní pro další pokus a správce má v databázi stopu, podle které dohledá příčinu.
        /// </summary>
        private void Rollback()
        {
            if (_x01ID == 0) return;

            string strGuid = BO.Code.Bas.GetGuid().Substring(0, 8);
            string strKey = $"failed-{_x01ID}-{strGuid}";
            var db = Db();
            string strErr;
            if (_j02ID > 0)
            {
                //přejmenování uvolní e-mail pro další pokus, uzavření zabrání přihlášení
                strErr = db.TryExec("UPDATE j02User SET j02Login=@login,j02Email=@email WHERE j02ID=@pid", new { login = strKey, email = strKey + "@registrace.invalid", pid = _j02ID });
                if (strErr != null) wr_sql($"-- CHYBA při přejmenování uživatele: {strErr}");
                strErr = db.TryExec("UPDATE j02User SET j02ValidUntil=GETDATE() WHERE j02ID=@pid", new { pid = _j02ID });
                if (strErr != null) wr_sql($"-- CHYBA při uzavření uživatele: {strErr}");
            }
            strErr = db.TryExec("UPDATE x01License SET x01LoginDomain=@domain,x01RobotLogin=@domain,x01Name=@name WHERE x01ID=@pid", new { domain = strKey, name = Fit("x01License", "x01Name", $"NEDOKONČENO {strGuid} {R.CompanyName}"), pid = _x01ID });
            if (strErr != null) wr_sql($"-- CHYBA při přejmenování licence: {strErr}");
            strErr = db.TryExec("UPDATE x01License SET x01ValidUntil=GETDATE() WHERE x01ID=@pid", new { pid = _x01ID });
            if (strErr != null) wr_sql($"-- CHYBA při uzavření licence: {strErr}");

            wr_sql($"-- rozpracovaná licence x01ID={_x01ID} byla uzavřena a přejmenována na {strKey}");
            try
            {
                _f.App.RefreshX01List();
            }
            catch
            {
                //seznam licencí se obnoví při dalším založení nebo restartu
            }
        }

        private void SaveSqlLog()
        {
            try
            {
                string strPath = $"{_f.App.LogFolder}\\register-{(_res.Ok ? "ok" : "failed")}-x01id{_x01ID}-{DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss")}.sql";
                BO.Code.File.WriteText2File(strPath, _sql.ToString());
            }
            catch
            {
                //log není pro výsledek podstatný
            }
        }
    }
}
