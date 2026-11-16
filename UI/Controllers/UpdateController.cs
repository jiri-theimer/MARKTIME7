using Microsoft.AspNetCore.Mvc;
using UI.Models;
using System.Text.Json;


namespace UI.Controllers
{
    public class UpdateController : BaseController
    {
        private System.Text.StringBuilder _sb { get; set; }
        private System.Text.StringBuilder _sbNakonec { get; set; }

       
        public IActionResult Index()
        {
            var v = new UpdateViewModel();

            //if (Factory.App.HostingMode == BL.Singleton.HostingModeEnum.None)
            //{
            //    v.DestDbName = "dbtest";
            //}

            RefreshState(v);

            return ViewTup(v, BO.PermValEnum.GR_Admin);
        }

        [HttpPost]
        public IActionResult Index(UpdateViewModel v)
        {
            if (!Factory.CurrentUser.TestPermission(BO.PermValEnum.GR_Admin))
            {
                return this.StopPage(false, "Nemáte oprávnění.");     //stejné oprávnění jako pro zobrazení stránky (GET)
            }
            if (v.MyPassword != GetUpdatePassword())
            {
                this.AddMessageTranslated("Chybí zadat tajné heslo.");
                RefreshState(v, false);
                return View(v);
            }
            if (!string.IsNullOrEmpty(v.DestDbName) && !System.Text.RegularExpressions.Regex.IsMatch(v.DestDbName, @"^[A-Za-z0-9_\-\.]+$"))
            {
                this.AddMessageTranslated("Název cílové databáze obsahuje nepovolené znaky.");     //název jde do SQL jako [název].dbo.tabulka
                v.DestDbName = null;
                RefreshState(v, false);
                return View(v);
            }
            //strukturu cílové databáze (4 dotazy) načítat jen pro operace, které ji porovnávají
            bool bolNeedDest = v.IsPostback && (v.PostbackOper == "rozdily" || v.PostbackOper == "navic");
            RefreshState(v, bolNeedDest);
            if (v.IsPostback)
            {
                if (v.PostbackOper == "changedb")
                {
                    v.RunResult = null;
                    return View(v);
                }
                if (string.IsNullOrEmpty(v.DestDbName))
                {
                    this.AddMessageTranslated($"Na vstupu chybí název cílové databáze.");
                    return View(v);
                }
                if(v.DestDbName.ToLower()== "fitwork_dev" && v.PostbackOper != "navic")
                {
                    this.AddMessageTranslated("Databáze [FITWORK_DEV] je chráněná proti nechtěnému zásahu skriptem.");
                    return View(v);
                }
                v.ResultFlag = v.PostbackOper;

                if (v.PostbackOper == "navic")
                {                    
                    Handle_Navic(v);                    
                }
                if (v.PostbackOper == "reports")
                {
                    Handle_Reports(v);
                }
                if (v.PostbackOper == "rozdily")
                {                    
                    v.RunResult = null;
                    Handle_NewTables(v);
                    Handle_Differences(v);
                    if (_sb != null)
                    {
                        v.RunResult = _sb.ToString();
                    }

                    if (_sbNakonec != null)
                    {
                        _sbNakonec.Insert(0, "\r\n\r\n----Spustit nakonec----\r\n\r\n");
                        v.RunResult += _sbNakonec.ToString();
                    }
                    if (v.RunResult == null)
                    {
                        v.RunResult =$"---Žádné nalezené rozdíly v cílové db [{v.DestDbName}].";
                    }
                }
                if (v.PostbackOper == "rozdily-alldbs")
                {
                    v.RunResult = null;
                    foreach (var c in v.lisX01_CloudHeader)
                    {
                        v.DestDbName = c.CloudDatabaseName;
                        LoadDest(v);        //jen struktura další databáze; zdrojové JSON soubory a seznam databází zůstávají načtené
                        Handle_NewTables(v);
                        Handle_Differences(v);
                        
                    }
                    if (_sb != null)
                    {
                        v.RunResult = _sb.ToString();
                    }
                    if (_sbNakonec != null)
                    {
                        _sbNakonec.Insert(0, "\r\n\r\n----Spustit nakonec----\r\n\r\n");
                        v.RunResult += _sbNakonec.ToString();
                    }
                    if (v.RunResult == null)
                    {
                        v.RunResult = $"---Žádné nalezené rozdíly v cílové db [{v.DestDbName}].";
                    }

                }
                if (v.PostbackOper== "cleartempfiles")
                {
                    //odstranit všechny TEMP složky
                    foreach (var c in v.lisX01_CloudHeader)
                    {
                        var strTempFolder = $"{Factory.App.RootUploadFolder}\\{c.x01LoginDomain}\\TEMP";
                        if (System.IO.Directory.Exists(strTempFolder))
                        {
                            var files = BO.Code.File.GetFileListFromDir(strTempFolder, "*.*", SearchOption.AllDirectories, true);
                            for(int i=0;i<files.Count();i++)
                            {
                                System.IO.File.Delete(files[i]);
                            }
                            //System.IO.Directory.Delete(strTempFolder);
                            //System.IO.Directory.CreateDirectory(strTempFolder);
                        }

                        

                    }
                }
                if (v.PostbackOper == "sql_sp_funct_views" || v.PostbackOper == "sql_sp_funct_views-full")
                {
                    var lisSp = ParseSpScript(BO.Code.File.GetFileContent(Factory.App.RootUploadFolder + "\\_distribution\\sys\\sql_sp_funct_views.sql"));
                    Handle_SpScript(v, lisSp, v.PostbackOper == "sql_sp_funct_views-full");
                    v.RunResult = null;
                }
                if (v.PostbackOper == "sql_sp_funct_views-alldbs")
                {
                    //soubor načíst a rozebrat jednou, ne pro každou databázi
                    var lisSp = ParseSpScript(BO.Code.File.GetFileContent(Factory.App.RootUploadFolder + "\\_distribution\\sys\\sql_sp_funct_views.sql"));
                    foreach (var c in v.lisX01_CloudHeader)
                    {
                        v.DestDbName = c.CloudDatabaseName;
                        Handle_SpScript(v, lisSp, false);
                    }
                    v.RunResult = null;
                    
                }
                if (v.PostbackOper == "runresult")
                {
                    Handle_RunResult(v);
                    v.RunResult = null;
                }
                return View(v);
            }

            if (ModelState.IsValid)
            {
                
               
            }
            
            return View(v);
        }

        // ------------------------------------------------------------------------------------------------
        // Načítání
        //   zdroj  = cílová struktura z JSON souborů v _distribution\sys (načítá se jednou za požadavek)
        //   cíl    = skutečná struktura databáze (4 dotazy do systémových tabulek)
        // Indexy (Indexes.json, sp_helpindex pro každou tabulku = přes 200 dotazů na databázi) se nenačítají: porovnání je nepoužívá.
        // ------------------------------------------------------------------------------------------------
        private bool _sourceLoaded;
        private HashSet<string> _destIndexKeys;                 //tabulka|název indexu
        private Dictionary<string, string> _destIndexByCols;    //tabulka|sloupce → název indexu
        private Dictionary<int, string> _srcConstraintNameById;
        private Dictionary<int, string> _srcDefById;
        private Dictionary<string, string> _srcDefByName;
        private ILookup<int, BO.Sys.syscolumn> _srcColsByTabId;
        private HashSet<string> _srcTabs;
        private HashSet<string> _srcColKeys;

        private HashSet<string> _destTabs;
        private HashSet<string> _destTabsWithCols;
        private Dictionary<string, BO.Sys.syscolumn> _destColByKey;
        private Dictionary<int, string> _destDefById;
        private Dictionary<string, string> _destDefaultConstraintTab;   //název DEFAULT omezení → tabulka

        private static string L(string s) => (s ?? "").ToLowerInvariant();
        private static string K(string tab, string col) => L(tab) + "|" + L(col);

        private string GetUpdatePassword()
        {
            //appsettings.json: "App": { "UpdatePassword": "…" }; bez nastavení platí původní heslo
            string s = Factory.App.Configuration.GetSection("App")["UpdatePassword"];
            return string.IsNullOrEmpty(s) ? "gogo" : s;
        }

        private void RefreshState(UpdateViewModel v, bool bolLoadDest = true)
        {
            LoadSource(v);
            if (bolLoadDest) LoadDest(v);

            if (v.lisX01_CloudHeader == null && Factory.App.HostingMode == BL.Singleton.HostingModeEnum.SharedApp)
            {
                Factory.db.ChangeConnectString(Factory.App.ConnectStringCloudHeader);
                v.lisX01_CloudHeader = Factory.x01LicenseBL.GetList(new BO.myQuery("x01")).OrderBy(p => p.x01LoginDomain).ToList();
                Factory.db.ChangeConnectString(null);
            }
        }

        private void LoadSource(UpdateViewModel v)
        {
            if (_sourceLoaded) return;
            string strDir = Factory.App.RootUploadFolder + "\\_distribution\\sys\\";
            v.sourceTabs = BO.Code.basJson.DeserializeList_From_File<BO.Sys.sysobject>(strDir + "Tables.json") ?? new List<BO.Sys.sysobject>();
            v.sourceCols = BO.Code.basJson.DeserializeList_From_File<BO.Sys.syscolumn>(strDir + "Columns.json") ?? new List<BO.Sys.syscolumn>();
            v.sourceConstraints = BO.Code.basJson.DeserializeList_From_File<BO.Sys.sysconstraint>(strDir + "Constraints.json") ?? new List<BO.Sys.sysconstraint>();
            v.sourceDefVals = BO.Code.basJson.DeserializeList_From_File<BO.Sys.sysdefval>(strDir + "Defvals.json") ?? new List<BO.Sys.sysdefval>();
            v.sourceInds = System.IO.File.Exists(strDir + "Indexes.json") ? (BO.Code.basJson.DeserializeList_From_File<BO.Sys.sysindex>(strDir + "Indexes.json") ?? new List<BO.Sys.sysindex>()) : new List<BO.Sys.sysindex>();

            _srcConstraintNameById = new Dictionary<int, string>();
            foreach (var c in v.sourceConstraints) _srcConstraintNameById.TryAdd(c.id, c.name);
            _srcDefById = new Dictionary<int, string>();
            _srcDefByName = new Dictionary<string, string>();
            foreach (var c in v.sourceDefVals)
            {
                _srcDefById.TryAdd(c.id, c.text);
                _srcDefByName.TryAdd(L(c.name), c.text);
            }
            _srcColsByTabId = v.sourceCols.ToLookup(p => p.id);
            _srcTabs = new HashSet<string>(v.sourceTabs.Select(p => L(p.name)));
            _srcColKeys = new HashSet<string>(v.sourceCols.Select(p => K(p.tablename, p.name)));
            _sourceLoaded = true;
        }

        private void LoadDest(UpdateViewModel v)
        {
            if (string.IsNullOrEmpty(v.DestDbName)) return;

            v.destTabs = Factory.FBL.Sys_GetList_Tables(v.DestDbName).ToList();
            v.destCols = Factory.FBL.Sys_GetList_Columns(v.DestDbName).ToList();
            v.destConstraints = Factory.FBL.Sys_GetList_Constraints(v.DestDbName).ToList();
            v.destDefVals = Factory.FBL.Sys_GetList_Defvals(v.DestDbName).ToList();
            v.destInds = Factory.FBL.Sys_GetList_Indexes(v.DestDbName).ToList();    //jeden dotaz

            _destIndexKeys = new HashSet<string>(v.destInds.Select(p => K(p.tabname, p.index_name)));
            _destIndexByCols = new Dictionary<string, string>();
            foreach (var c in v.destInds) _destIndexByCols.TryAdd(IndexColsKey(c), c.index_name);

            _destTabs = new HashSet<string>(v.destTabs.Select(p => L(p.name)));
            _destTabsWithCols = new HashSet<string>(v.destCols.Select(p => L(p.tablename)));
            _destColByKey = new Dictionary<string, BO.Sys.syscolumn>();
            foreach (var c in v.destCols) _destColByKey.TryAdd(K(c.tablename, c.name), c);
            _destDefById = new Dictionary<int, string>();
            foreach (var c in v.destDefVals) _destDefById.TryAdd(c.id, c.text);
            _destDefaultConstraintTab = new Dictionary<string, string>();
            foreach (var c in v.destConstraints.Where(p => p.xtype == "D")) _destDefaultConstraintTab.TryAdd(L(c.name), c.tablename);
        }

        // výchozí hodnota sloupce podle cílové struktury: přesně podle ID omezení, podle názvu sloupce jen jako záloha
        private string GetDefVal_Source(BO.Sys.syscolumn col)
        {
            if (col.cdefault != 0 && _srcDefById.TryGetValue(col.cdefault, out string s)) return s;
            return _srcDefByName.TryGetValue(L(col.name), out s) ? s : null;
        }

        // výchozí hodnota sloupce ve skutečné databázi (podle jeho vlastního omezení, ne podle stejnojmenného sloupce jiné tabulky)
        private string GetDefVal_Dest(BO.Sys.syscolumn coldest)
        {
            if (coldest == null || coldest.cdefault == 0) return null;
            return _destDefById.TryGetValue(coldest.cdefault, out string s) ? s : null;
        }

        private void Handle_Col_Constraint_DefValue(UpdateViewModel v,BO.Sys.syscolumn col)
        {
            if (col.cdefault == 0) return;

            string strDefval = GetDefVal_Source(col);
            if (strDefval == null) return;
            if (!_srcConstraintNameById.TryGetValue(col.cdefault, out string strConstraintName) || string.IsNullOrEmpty(strConstraintName))
            {
                strConstraintName = $"DEF_{col.tablename}_{col.name}";
            }
            if (_destDefaultConstraintTab != null && _destDefaultConstraintTab.TryGetValue(L(strConstraintName), out string strTheTab))
            {
                wrl_nakonec($"ALTER TABLE [{v.DestDbName}].dbo.{strTheTab} DROP CONSTRAINT [{strConstraintName}]");
                wrl_nakonec("GO");
                wrl_nakonec("");
            }
            wrl_nakonec($"ALTER TABLE [{v.DestDbName}].dbo.{col.tablename} ADD CONSTRAINT [{strConstraintName}] DEFAULT ({strDefval}) FOR [{col.name}]");
            wrl_nakonec("GO");
            wrl_nakonec("");
        }

        // řádkům s prázdnou hodnotou doplnit výchozí hodnotu (výraz omezení se použije tak, jak je – už obsahuje závorky i apostrofy)
        private void Handle_Col_FillDefValue(UpdateViewModel v, BO.Sys.syscolumn col, string strDefvalSource)
        {
            wrl_nakonec($"UPDATE [{v.DestDbName}].dbo.{col.tablename} SET {col.name} = {strDefvalSource} WHERE {col.name} IS NULL");
            wrl_nakonec("GO");
        }

        private static string SqlTypeSuffix(BO.Sys.syscolumn col)
        {
            switch (col.type_name)
            {
                case "decimal":
                case "numeric":
                    return $" ({col.xprec},{col.xscale})";
                case "varchar":
                case "char":
                case "nvarchar":
                case "nchar":
                case "varbinary":
                    return col.length == -1 ? " (MAX)" : $" ({col.length})";
            }
            return "";
        }

        private void Handle_Differences(UpdateViewModel v)
        {
            foreach (var col in v.sourceCols)
            {
                if (!_destTabsWithCols.Contains(L(col.tablename)))
                {
                    continue;   //pole, jehož tabulka vůbec není v db (novou tabulku řeší Handle_NewTables)
                }
                _destColByKey.TryGetValue(K(col.tablename, col.name), out BO.Sys.syscolumn coldest);

                if (coldest == null)
                {
                    //nový sloupec
                    if (col.iscomputed)
                    {
                        wrl($"ALTER TABLE [{v.DestDbName}].dbo.{col.tablename} ADD {col.name} AS {col.formula}");
                        wrl("GO");
                        continue;
                    }
                    wrl($"ALTER TABLE [{v.DestDbName}].dbo.{col.tablename} ADD {col.name} [{col.type_name}]{SqlTypeSuffix(col)}{(col.isidentity ? " IDENTITY (1, 1)" : " NULL")}");
                    wrl("GO");
                    if (col.cdefault != 0)
                    {
                        //výchozí hodnota hned v témže běhu (dříve až při druhém spuštění porovnání)
                        string strDefvalNew = GetDefVal_Source(col);
                        if (strDefvalNew != null)
                        {
                            Handle_Col_Constraint_DefValue(v, col);
                            Handle_Col_FillDefValue(v, col, strDefvalNew);
                        }
                    }
                    continue;
                }

                //sloupec existuje
                if (col.iscomputed)
                {
                    if (col.formula != coldest.formula)
                    {
                        wrl_nakonec($"ALTER TABLE [{v.DestDbName}].dbo.{col.tablename} DROP COLUMN {col.name}");
                        wrl_nakonec("GO");
                        wrl_nakonec($"ALTER TABLE [{v.DestDbName}].dbo.{col.tablename} ADD {col.name} AS {col.formula}");
                        wrl_nakonec("GO");
                    }
                    continue;
                }

                bool bolDecimal = (col.type_name == "decimal" || col.type_name == "numeric");
                if (col.length != coldest.length || col.type_name != coldest.type_name || col.xprec != coldest.xprec || (bolDecimal && col.xscale != coldest.xscale))
                {
                    //typ / délka / přesnost. NULL / NOT NULL se zachovává podle skutečného stavu sloupce:
                    //bez uvedení by ALTER COLUMN udělal ze sloupce NOT NULL sloupec NULL.
                    wrl($"ALTER TABLE [{v.DestDbName}].dbo.{col.tablename} ALTER COLUMN {col.name} {col.type_name}{SqlTypeSuffix(col)}{(coldest.isnullable ? " NULL" : " NOT NULL")}");
                    wrl("GO");
                }

                if (col.cdefault != 0)
                {
                    string strDefvalSource = GetDefVal_Source(col);
                    string strDefvalDest = GetDefVal_Dest(coldest);
                    if (strDefvalSource != null && strDefvalSource != strDefvalDest)
                    {
                        Handle_Col_Constraint_DefValue(v, col);
                        Handle_Col_FillDefValue(v, col, strDefvalSource);
                    }
                }
            }

            Handle_Indexes(v);
        }

        // ------------------------------------------------------------------------------------------------
        // Indexy: do skriptu jdou jen indexy, které v cílové db chybí (podle názvu). Primární klíče se neřeší (vznikají s tabulkou).
        // Existující index se nikdy nemaže ani nepřestavuje.
        // ------------------------------------------------------------------------------------------------
        private static string IndexColsKey(BO.Sys.sysindex c)
        {
            return L(c.tabname) + "|" + L(c.index_keys).Replace(" ", "") + "|" + L(c.index_includes).Replace(" ", "") + "|" + L(c.index_filter).Replace(" ", "");
        }
        private static string IndexCols2Sql(string s)
        {
            //"a, b(-)" → "[a], [b] DESC"
            return string.Join(", ", (s ?? "").Split(',').Select(p => p.Trim()).Where(p => p.Length > 0).Select(p => p.EndsWith("(-)") ? $"[{p.Substring(0, p.Length - 3).Trim()}] DESC" : $"[{p}]"));
        }
        private void Handle_Indexes(UpdateViewModel v)
        {
            if (v.sourceInds == null || _destIndexKeys == null) return;
            foreach (var c in v.sourceInds)
            {
                string strDesc = L(c.index_description);    //starší Indexes.json (z sp_helpindex) má vlastnosti jen v popisu
                if (c.is_primary_key || strDesc.Contains("primary key") || strDesc.Contains("unique key")) continue;
                if (string.IsNullOrEmpty(c.index_keys) || string.IsNullOrEmpty(c.index_name)) continue;
                if (!_srcTabs.Contains(L(c.tabname))) continue;
                if (_destIndexKeys.Contains(K(c.tabname, c.index_name))) continue;
                if (_destIndexByCols.TryGetValue(IndexColsKey(c), out string strSame))
                {
                    wrl_nakonec($"----Index [{c.index_name}], tabulka [{c.tabname}]: v [{v.DestDbName}] už je index se stejnými sloupci pod názvem [{strSame}].");
                    continue;
                }
                bool bolUnique = c.is_unique || strDesc.Contains("unique");
                bool bolClustered = c.is_clustered || strDesc.StartsWith("clustered");
                string strSql = $"CREATE {(bolUnique ? "UNIQUE " : "")}{(bolClustered ? "CLUSTERED" : "NONCLUSTERED")} INDEX [{c.index_name}] ON [{v.DestDbName}].dbo.{c.tabname} ({IndexCols2Sql(c.index_keys)})";
                if (!string.IsNullOrEmpty(c.index_includes)) strSql += $" INCLUDE ({IndexCols2Sql(c.index_includes)})";
                if (!string.IsNullOrEmpty(c.index_filter)) strSql += $" WHERE {c.index_filter}";
                wrl_nakonec(strSql);
                wrl_nakonec("GO");
                wrl_nakonec("");
            }
        }

        private void Handle_NewTables(UpdateViewModel v)
        {
            
            foreach(var tab in v.sourceTabs)
            {
                if (!_destTabs.Contains(L(tab.name)))
                {
                    int x = 1;
                    wrl($"CREATE TABLE [{v.DestDbName}].dbo.{tab.name}");
                    wrl("(");
                    var qry = _srcColsByTabId[tab.id].ToList();bool bolPK = false;

                    foreach (var col in qry)
                    {
                        Handle_Col_Constraint_DefValue(v, col);
                        
                        
                        wr($"[{col.name}] {col.type_name}");
                        if (!col.isidentity)
                        {
                            switch (col.type_name)
                            {
                                case "decimal":
                                    wr($" ({col.prec},{col.scale})");
                                    break;
                                case "char":
                                case "nchar":
                                    wr($" ({col.length})");
                                    break;
                                case "varchar":
                                case "nvarchar":                                
                                    if (col.length == -1)
                                    {
                                        wr($" (MAX)");
                                    }
                                    else
                                    {
                                        wr($" ({col.length})");
                                    }
                                    
                                    break;
                            }
                            if (col.isnullable)
                            {
                                wr(" NULL");
                            }
                        }
                        else
                        {
                            wr(" IDENTITY (1, 1)");
                        }

                        if (!bolPK && col.name.ToLower() == "rowid")
                        {
                            wr($" CONSTRAINT {col.name}_PK_SCRIPT_{tab.name} PRIMARY KEY CLUSTERED");
                            bolPK = true;
                        }
                        if (!bolPK && col.name.ToLower() == tab.name.Substring(0, 3) + "id")
                        {
                            wr($" CONSTRAINT {col.name}_PK_SCRIPT_{tab.name} PRIMARY KEY CLUSTERED");
                            bolPK = true;
                        }
                        
                        if (x<qry.Count())
                        {
                            wrl(",");
                            
                        }

                        x += 1;

                    }
                    wrl("");
                    wrl(")");
                    wrl("GO");
                    wrl("");
                }
            }

            

        }

        private void Handle_Navic(UpdateViewModel v)
        {
            v.RunResult = null;
            wrl($"----Cílová databáze: [{v.DestDbName}].");
            wrl("");
            int x = 0;
            foreach (var tab in v.destTabs)
            {
                if (!_srcTabs.Contains(L(tab.name)))
                {
                    wrl($"----Tabulka navíc: [{tab.name}].");
                    x += 1;
                }
            }
            foreach (var col in v.destCols)
            {
                if (!_srcTabs.Contains(L(col.tablename)))
                {
                    continue;   //celá tabulka je navíc – už vypsáno výše
                }
                if (!_srcColKeys.Contains(K(col.tablename, col.name)))
                {
                    wrl($"----Sloupec navíc: [{col.name}] [{col.type_name}], tabulka: [{col.tablename}].");
                    x += 1;
                }
            }
            if (x == 0)
            {
                wrl($"---Nic navíc jsem nenašel v cílové db [{v.DestDbName}].");
            }
            v.RunResult = _sb.ToString();
        }

        private void wrl(string s)
        {
            if (_sb == null) _sb = new System.Text.StringBuilder(2000);

            _sb.AppendLine(s);
        }
        private void wr(string s)
        {
            if (_sb == null) _sb = new System.Text.StringBuilder(2000);

            _sb.Append(s);
            if (s == "GO")
            {
                _sb.AppendLine("");
            }
        }


        private void wrl_nakonec(string s)
        {
            if (_sbNakonec == null) _sbNakonec = new System.Text.StringBuilder();

            _sbNakonec.AppendLine(s);

            if (s == "GO")
            {
                _sbNakonec.AppendLine("");
            }
        }

        // dávky odděluje řádek, na kterém je jen GO (bez ohledu na konce řádků a mezery)
        private static readonly System.Text.RegularExpressions.Regex _rxGo = new System.Text.RegularExpressions.Regex(@"^[ \t]*GO[ \t]*;?[ \t]*\r?$",
            System.Text.RegularExpressions.RegexOptions.Multiline | System.Text.RegularExpressions.RegexOptions.IgnoreCase | System.Text.RegularExpressions.RegexOptions.Compiled);

        private void Handle_RunResult(UpdateViewModel v)
        {
            if (string.IsNullOrEmpty(v.RunResult) || string.IsNullOrEmpty(v.DestDbName)) return;
            var strConString = Factory.App.ParseConnectStringFromDbname(v.DestDbName);

            System.IO.File.WriteAllText($"{this.Factory.TempFolder}\\Update_RunResult_{v.DestDbName}_{DateTime.Now.ToString("dd-MM-yyyy-HH-mm-ss")}.txt",$"---connect-string: {strConString}\r\n\r\n"+ v.RunResult);

            RunBatches(v, _rxGo.Split(v.RunResult), null);
        }

        private void RunBatches(UpdateViewModel v, IEnumerable<string> batches, string strInfo)
        {
            var strConString = Factory.App.ParseConnectStringFromDbname(v.DestDbName);
            int intOK = 0; var errs = new List<string>();
            var sw = System.Diagnostics.Stopwatch.StartNew();
            try
            {
                Factory.db.ChangeConnectString(strConString);
                foreach (var s in batches)
                {
                    string strSql = s.Trim();
                    if (strSql.Length <= 5 || strSql.StartsWith("----Spustit nakonec----")) continue;
                    if (Factory.db.RunSql2(strSql, null, 600))      //RunSql2: neúspěch = jen skutečná chyba SQL (text chyby je v logu chyb)
                    {
                        intOK++;
                    }
                    else
                    {
                        string strFirstLine = strSql.Split('\n').Select(p => p.Trim()).FirstOrDefault(p => p.Length > 0 && !p.StartsWith("--")) ?? "";
                        errs.Add(strFirstLine.Length > 160 ? strFirstLine.Substring(0, 160) + "…" : strFirstLine);
                    }
                }
            }
            finally
            {
                Factory.db.ChangeConnectString(null);   //zbytek požadavku zase proti databázi přihlášeného uživatele
            }

            if (errs.Count > 0)
            {
                this.AddMessageTranslated($"[{v.DestDbName}]: {errs.Count} z {intOK + errs.Count} dávek skončilo chybou (podrobnosti v logu chyb):<br>" + string.Join("<br>", errs.Take(15).Select(p => System.Net.WebUtility.HtmlEncode(p))) + (errs.Count > 15 ? "<br>…" : ""));
            }
            else
            {
                this.AddMessageTranslated($"[{v.DestDbName}]: {intOK} dávek, {sw.Elapsed.TotalSeconds:0.0} s, bez chyb.{(strInfo == null ? "" : " " + strInfo)}", "info");
            }
        }

        // ------------------------------------------------------------------------------------------------
        // sql_sp_funct_views.sql: pro každý objekt dvojice dávek "if exists … drop" + "CREATE …".
        // Do databáze se pouští jen objekty, které v ní chybí nebo mají jiný text (jeden dotaz na sys.sql_modules).
        // Nezměněné procedury se tak nemažou a nevytvářejí znovu.
        // ------------------------------------------------------------------------------------------------
        private class SpItem
        {
            public string Name;         //null = dávka mimo dvojici, pouští se vždy
            public string DropSql;
            public string CreateSql;
        }
        private static readonly System.Text.RegularExpressions.Regex _rxSpDrop = new System.Text.RegularExpressions.Regex(@"^\s*if exists\s*\(\s*select 1 from sysobjects where id = object_id\('(?<n>[^']+)'\)",
            System.Text.RegularExpressions.RegexOptions.IgnoreCase | System.Text.RegularExpressions.RegexOptions.Compiled);

        private static string NormSql(string s) => (s ?? "").Replace("\r\n", "\n").Replace("\r", "\n").Trim();

        private List<SpItem> ParseSpScript(string strScript)
        {
            var ret = new List<SpItem>();
            SpItem pending = null;
            foreach (var s in _rxGo.Split(strScript ?? ""))
            {
                string strSql = s.Trim();
                if (strSql.Length <= 5) continue;
                var m = _rxSpDrop.Match(strSql);
                if (m.Success)
                {
                    if (pending != null) ret.Add(new SpItem() { CreateSql = pending.DropSql });  //drop bez těla
                    pending = new SpItem() { Name = m.Groups["n"].Value, DropSql = strSql };
                    continue;
                }
                if (pending != null)
                {
                    pending.CreateSql = strSql;
                    ret.Add(pending);
                    pending = null;
                }
                else
                {
                    ret.Add(new SpItem() { CreateSql = strSql });
                }
            }
            if (pending != null) ret.Add(new SpItem() { CreateSql = pending.DropSql });
            return ret;
        }

        private void Handle_SpScript(UpdateViewModel v, List<SpItem> lis, bool bolAll)
        {
            if (string.IsNullOrEmpty(v.DestDbName) || lis == null || lis.Count == 0) return;

            Dictionary<string, string> dest = null;
            if (!bolAll)
            {
                dest = new Dictionary<string, string>();
                var lisDest = Factory.db.GetList<BO.Sys.SysDbObject>($"SELECT o.name as Name,m.definition as Content FROM [{v.DestDbName}].sys.objects o INNER JOIN [{v.DestDbName}].sys.sql_modules m ON o.object_id=m.object_id WHERE o.type IN ('P','FN','IF','TF','V')");
                if (lisDest == null)
                {
                    this.AddMessageTranslated($"[{v.DestDbName}]: nepodařilo se načíst stávající procedury a funkce, databáze přeskočena.");
                    return;
                }
                foreach (var c in lisDest) dest.TryAdd(L(c.Name), NormSql(c.Content));
            }

            var batches = new List<string>(); int intSame = 0; int intChanged = 0;
            foreach (var c in lis)
            {
                if (c.Name == null)
                {
                    batches.Add(c.CreateSql);
                    continue;
                }
                if (dest != null && dest.TryGetValue(L(c.Name), out string strDest) && strDest == NormSql(c.CreateSql))
                {
                    intSame++;
                    continue;
                }
                intChanged++;
                batches.Add(c.DropSql);
                batches.Add(c.CreateSql);
            }
            if (batches.Count == 0)
            {
                this.AddMessageTranslated($"[{v.DestDbName}]: všech {intSame} procedur, funkcí a pohledů je aktuálních, nic se nespouštělo.", "info");
                return;
            }
            RunBatches(v, batches, $"Aktualizováno objektů: {intChanged}, beze změny: {intSame}.");
        }

        private void Handle_Reports(UpdateViewModel v)
        {
            var strPath = Factory.App.RootUploadFolder + "\\_distribution\\x31Report.json";
            string s = BO.Code.File.GetFileContent(strPath);

            

            var lisX31Source = JsonSerializer.Deserialize<List<BO.x31Report>>(s);
            

            foreach (var c in v.lisX01_CloudHeader)
            {
                v.DestDbName = c.CloudDatabaseName;
                var strConString = Factory.App.ParseConnectStringFromDbname(v.DestDbName);


                Factory.db.ChangeConnectString(strConString);
                var lisX31Dest = Factory.x31ReportBL.GetList(new BO.myQueryX31() { IsRecordValid=null});
                string strDestDir = Factory.App.RootUploadFolder + "\\" + c.x01LoginDomain + "\\x31";

                foreach (var recSource in lisX31Source)
                {
                    if (System.IO.File.Exists(Factory.App.RootUploadFolder + "\\_distribution\\trdx\\" + recSource.ReportFileName))
                    {                        

                        if (lisX31Dest.Where(p => p.x31Code.ToLower() == recSource.x31Code.ToLower()).Count() == 0)
                        {
                            var rec = new BO.x31Report() { x31Name = recSource.x31Name, x31Code = recSource.x31Code, x31Entity = recSource.x31Entity, x31IsPeriodRequired = recSource.x31IsPeriodRequired, x31FormatFlag = recSource.x31FormatFlag, x31Ordinary = recSource.x31Ordinary,x01ID=c.pid };
                            int intX31ID = Factory.x31ReportBL.Save(rec, null);
                            if (intX31ID > 0)
                            {

                                if (!System.IO.File.Exists(strDestDir + "\\" + recSource.ReportFileName))
                                {
                                    System.IO.File.Copy(Factory.App.RootUploadFolder + "\\_distribution\\trdx\\" + recSource.ReportFileName, strDestDir + "\\" + recSource.ReportFileName, true);

                                    s = "INSERT INTO o27Attachment(o27Entity,o27RecordPid,o27OriginalFileName,o27FileExtension,o27ArchiveFileName,o27ArchiveFolder,o27ContentType,o27UserInsert,o27UserUpdate,o27DateUpdate,o27Guid,x01ID)";
                                    s += $" VALUES('x31',{intX31ID},'{recSource.ReportFileName}','.trdx','{recSource.ReportFileName}','x31','application/trdx','guru','guru',GETDATE(),NEWID(),{c.pid})";
                                    Factory.FBL.RunSql(s, null, strConString);
                                }
                            }
                        }
                        var recJ25 = Factory.j25ReportCategoryBL.LoadByCode(recSource.j25Code,0);
                        if (recJ25 == null)
                        {
                            recJ25 = new BO.j25ReportCategory() { j25Name = recSource.j25Name, j25Code = recSource.j25Code,j25Ordinary=recSource.j25Ordinary };
                            Factory.j25ReportCategoryBL.Save(recJ25);
                            recJ25 = Factory.j25ReportCategoryBL.LoadByCode(recSource.j25Code, 0);
                        }
                        var recX31 = Factory.x31ReportBL.LoadByCode(recSource.x31Code, 0);
                        if (recX31 !=null && recJ25 != null && recX31.j25ID !=recJ25.pid)
                        {
                            recX31.j25ID = recJ25.pid;
                            Factory.x31ReportBL.Save(recX31, null);
                            recJ25.ValidUntil = new DateTime(3000, 1, 1);
                            
                            Factory.j25ReportCategoryBL.Save(recJ25);
                        }
                    }
                    
                }

                

            }
            Factory.db.ChangeConnectString(null);   //zbytek požadavku zase proti databázi přihlášeného uživatele
        }
    }
}
