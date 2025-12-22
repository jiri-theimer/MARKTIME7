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
            if (v.MyPassword != "gogo")
            {
                this.AddMessageTranslated("Chybí zadat tajné heslo.");
                return View(v);
            }
            RefreshState(v);
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
                        RefreshState(v);
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
                if (v.PostbackOper== "sql_sp_funct_views")
                {
                    v.RunResult = BO.Code.File.GetFileContent(Factory.App.RootUploadFolder+ "\\_distribution\\sys\\sql_sp_funct_views.sql");
                    Handle_RunResult(v);
                }
                if (v.PostbackOper == "sql_sp_funct_views-alldbs")
                {
                    foreach (var c in v.lisX01_CloudHeader)
                    {
                        v.DestDbName = c.CloudDatabaseName;
                        
                        v.RunResult = BO.Code.File.GetFileContent(Factory.App.RootUploadFolder + "\\_distribution\\sys\\sql_sp_funct_views.sql");
                        Handle_RunResult(v);

                    }
                    
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

        private void RefreshState(UpdateViewModel v)
        {
            v.sourceTabs = BO.Code.basJson.DeserializeList_From_File<BO.Sys.sysobject>(Factory.App.RootUploadFolder + "\\_distribution\\sys\\Tables.json").AsEnumerable();
            v.sourceCols = BO.Code.basJson.DeserializeList_From_File<BO.Sys.syscolumn>(Factory.App.RootUploadFolder + "\\_distribution\\sys\\Columns.json").AsEnumerable();
            v.sourceInds = BO.Code.basJson.DeserializeList_From_File<BO.Sys.sysindex>(Factory.App.RootUploadFolder + "\\_distribution\\sys\\Indexes.json").AsEnumerable();
            v.sourceConstraints = BO.Code.basJson.DeserializeList_From_File<BO.Sys.sysconstraint>(Factory.App.RootUploadFolder + "\\_distribution\\sys\\Constraints.json").AsEnumerable();
            v.sourceDefVals = BO.Code.basJson.DeserializeList_From_File<BO.Sys.sysdefval>(Factory.App.RootUploadFolder + "\\_distribution\\sys\\Defvals.json").AsEnumerable();

            if (!string.IsNullOrEmpty(v.DestDbName))
            {
                v.destTabs = Factory.FBL.Sys_GetList_Tables(v.DestDbName);
                v.destCols = Factory.FBL.Sys_GetList_Columns(v.DestDbName);
                v.destInds = Factory.FBL.Sys_GetList_Indexes(v.DestDbName);
                v.destConstraints = Factory.FBL.Sys_GetList_Constraints(v.DestDbName);
                v.destDefVals = Factory.FBL.Sys_GetList_Defvals(v.DestDbName);
            }
            
            
            if (Factory.App.HostingMode == BL.Singleton.HostingModeEnum.SharedApp)
            {
                
                Factory.db.ChangeConnectString(Factory.App.ConnectStringCloudHeader);
                v.lisX01_CloudHeader = Factory.x01LicenseBL.GetList(new BO.myQuery("x01")).OrderBy(p => p.x01LoginDomain);
                Factory.db.ChangeConnectString(null);
            }
        }

        private void Handle_Col_Constraint_DefValue(UpdateViewModel v,BO.Sys.syscolumn col)
        {
            if (col.cdefault == 0) return;

            string strConstraintName = GetConstraintName(v, col.cdefault);
            string strDefval = GetDefVal_Source(v, col.name, col.cdefault);
            if (v.destConstraints.Where(p => p.xtype == "D" && p.name.ToLower() == strConstraintName.ToLower()).Count() > 0)
            {
                string strTheTab = v.destConstraints.Where(p => p.xtype == "D" && p.name.ToLower() == strConstraintName.ToLower()).First().tablename;
                wrl_nakonec($"ALTER TABLE [{v.DestDbName}].dbo.{strTheTab} DROP CONSTRAINT [{strConstraintName}]");
                wrl_nakonec("GO");
                wrl_nakonec("");
            }
            wrl_nakonec($"ALTER TABLE [{v.DestDbName}].dbo.{col.tablename} ADD CONSTRAINT [{strConstraintName}] DEFAULT ({strDefval}) FOR [{col.name}]");
            wrl_nakonec("GO");
            wrl_nakonec("");
        }

        
        private void Handle_Differences(UpdateViewModel v)
        {
            foreach (var col in v.sourceCols)
            {
                
                if (v.destCols.Where(p => p.tablename.ToLower() == col.tablename.ToLower()).Count() == 0)
                {
                    continue;   //pole, jehož tabulka vůbec není v db
                }
                bool bolFound = false;bool bolGoOn = false;string strPrimarySQL = null;
                var qry = v.destCols.Where(p => p.tablename.ToLower() == col.tablename.ToLower() && p.name.ToLower() == col.name.ToLower());
                BO.Sys.syscolumn coldest = null;
                if (qry.Count() > 0)
                {
                    bolFound = true;
                    coldest = qry.First();
                }
                if (!bolFound)
                {
                    
                    if (!col.iscomputed)
                    {
                        strPrimarySQL = $"ALTER TABLE [{v.DestDbName}].dbo.{col.tablename} ADD {col.name} [{col.type_name}]";
                        bolGoOn = true;
                    }
                    else
                    {
                        wrl($"ALTER TABLE [{v.DestDbName}].dbo.{col.tablename} ADD {col.name} [{col.type_name}] AS {col.formula}");
                        wrl("GO");
                    }
                    
                }
                else
                {
                    if (!col.iscomputed)
                    {
                        
                        //if (col.type_name !=coldest.type_name || col.xprec != coldest.xprec || col.xscale != coldest.xscale || (col.cdefault==0 && coldest.cdefault !=0) || (col.cdefault != 0 && coldest.cdefault == 0))
                        //{
                        //    strPrimarySQL = $"ALTER TABLE {v.DestDbName}.dbo.{col.tablename} ALTER COLUMN {col.name} {col.type_name}";
                        //    bolGoOn = true;
                        //}
                        
                        if (!bolGoOn && (col.length != coldest.length || col.type_name != coldest.type_name || col.xprec != coldest.xprec))
                        {
                            strPrimarySQL = $"ALTER TABLE [{v.DestDbName}].dbo.{col.tablename} ALTER COLUMN {col.name} {col.type_name}";
                            bolGoOn = true;
                        }
                        if (!bolGoOn && col.cdefault != 0)
                        {
                            //je třeba otestovat rozdíl nastavení výchozí hodnoty v poli
                            bolGoOn = true;
                        }
                        
                    }
                    else
                    {
                        if (col.formula != coldest.formula)
                        {
                            wrl_nakonec($"ALTER TABLE [{v.DestDbName}].dbo.{col.tablename} DROP COLUMN {col.name}");
                            wrl_nakonec("GO");
                            wrl_nakonec($"ALTER TABLE [{v.DestDbName}].dbo.{col.tablename} ADD {col.name} AS {col.formula}");
                            wrl_nakonec("GO");

                        }
                    }
                }

                if (bolGoOn)
                {
                    if (bolFound && col.cdefault != 0)
                    {
                        string strDefvalSource = GetDefVal_Source(v, col.name, col.cdefault);
                        string strDefvalDest = GetDefVal_Dest(v, col.name, col.cdefault);
                        if (strDefvalSource != strDefvalDest)
                        {
                            Handle_Col_Constraint_DefValue(v, col);
                        }
                        if (strDefvalSource != null && strDefvalSource != strDefvalDest)
                        {
                            if (col.type_name == "varchar" || col.type_name == "nvarchar" || col.type_name == "char" || col.type_name == "nchar" || col.type_name == "ntext")
                            {
                                wrl_nakonec($"UPDATE [{v.DestDbName}].dbo.{col.tablename} SET {col.name} = '{strDefvalSource}' WHERE {col.name} IS NULL");
                                wrl_nakonec("GO");
                            }
                            else
                            {
                                wrl_nakonec($"UPDATE [{v.DestDbName}].dbo.{col.tablename} SET {col.name} = {strDefvalSource} WHERE {col.name} IS NULL");
                                wrl_nakonec("GO");
                            }

                        }
                    }


                    if (strPrimarySQL != null)
                    {
                        if (col.type_name == "decimal")
                        {
                            strPrimarySQL += $" ({col.xprec},{col.xscale})";
                        }
                        if (col.type_name == "varchar" || col.type_name == "char" || col.type_name == "nvarchar" || col.type_name == "nchar")
                        {
                            if (col.length == -1)
                            {
                                strPrimarySQL += " (MAX)";
                            }
                            else
                            {
                                strPrimarySQL += $" ({col.length})";
                            }
                            if (col.isidentity)
                            {
                                strPrimarySQL += " IDENTITY (1, 1)";
                            }
                            if (col.isnullable)
                            {
                                strPrimarySQL += " NULL";
                            }
                        }
                        wrl(strPrimarySQL);
                        wrl("GO");
                    }
                }
                //if (bolGoOn && bolFound && col.cdefault != 0)
                //{
                //    string strDefvalSource = GetDefVal_Source(v, col.name, col.cdefault);
                //    string strDefvalDest = GetDefVal_Dest(v, col.name, col.cdefault);
                //    if (strDefvalSource != null && strDefvalSource != strDefvalDest)
                //    {
                //        if (col.type_name == "varchar" || col.type_name == "nvarchar" || col.type_name == "char" || col.type_name == "nchar" || col.type_name == "ntext")
                //        {
                //            wrl_nakonec($"UPDATE {v.DestDbName}.dbo.{col.tablename} SET {col.name} = '{strDefvalSource}' WHERE {col.name} IS NULL");
                //            wrl_nakonec("GO");
                //        }
                //        else
                //        {
                //            wrl_nakonec($"UPDATE {v.DestDbName}.dbo.{col.tablename} SET {col.name} = {strDefvalSource} WHERE {col.name} IS NULL");
                //            wrl_nakonec("GO");
                //        }
                        
                //    }
                //}
                
            }
        }

        private void Handle_NewTables(UpdateViewModel v)
        {
            
            foreach(var tab in v.sourceTabs)
            {
                if (!v.destTabs.Any(p => p.name.ToLower() == tab.name.ToLower()))
                {
                    int x = 1;
                    wrl($"CREATE TABLE [{v.DestDbName}].dbo.{tab.name}");
                    wrl("(");
                    var qry = v.sourceCols.Where(p => p.id == tab.id);bool bolPK = false;

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
                if (v.sourceTabs.Where(p => p.name.ToLower() == tab.name.ToLower()).Count() == 0)
                {
                    wrl($"----Tabulka navíc: [{tab.name}].");
                    x += 1;
                }
            }
            foreach (var col in v.destCols)
            {
                if (v.sourceCols.Where(p => p.tablename.ToLower() == col.tablename.ToLower()).Count() == 0)
                {
                    continue;
                }
                var qry = v.sourceCols.Where(p => p.tablename.ToLower() == col.tablename.ToLower() && p.name.ToLower() == col.name.ToLower());

                if (qry.Count() == 0)
                {
                    wrl($"----Sloupec navíc: [{col.name}] [{col.type_name}], tabulka: [{col.tablename}].");
                    x += 1;
                }
            }
            if (x > 0)
            {
                v.RunResult = $"---Nic jsem nenašel v cílové db [{v.DestDbName}].";
            }
            v.RunResult = _sb.ToString();
           
            
        }

        private string GetConstraintName(UpdateViewModel v,int intConstraintID)
        {
            if (v.sourceConstraints.Where(p => p.id == intConstraintID).Count() > 0)
            {
                return v.sourceConstraints.Where(p => p.id == intConstraintID).First().name;
            }

            return null;
        }
        private string GetDefVal_Source(UpdateViewModel v,string strColName,int intDefVal = 0)
        {
            
            if (v.sourceDefVals.Where(p => p.name.ToLower() == strColName.ToLower()).Count() > 0)
            {
                return v.sourceDefVals.Where(p => p.name.ToLower() == strColName.ToLower()).First().text;
            }
            if (intDefVal>0 && v.sourceDefVals.Where(p => p.id == intDefVal).Count() > 0)
            {
                return v.sourceDefVals.Where(p => p.id == intDefVal).First().text;
            }

            return null;
        }
        private string GetDefVal_Dest(UpdateViewModel v, string strColName, int intDefVal = 0)
        {

            if (v.destDefVals.Where(p => p.name.ToLower() == strColName.ToLower()).Count() > 0)
            {
                return v.destDefVals.Where(p => p.name.ToLower() == strColName.ToLower()).First().text;
            }
            if (intDefVal > 0 && v.destDefVals.Where(p => p.id == intDefVal).Count() > 0)
            {
                return v.destDefVals.Where(p => p.id == intDefVal).First().text;
            }

            return null;
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

        private void Handle_RunResult(UpdateViewModel v)
        {
            var strConString = Factory.App.ParseConnectStringFromDbname(v.DestDbName);

            System.IO.File.WriteAllText($"{this.Factory.TempFolder}\\Update_RunResult_{DateTime.Now.ToString("dd-MM-yyyy-hh-mm-ss")}.txt",$"---connect-string: {strConString}\r\n\r\n"+ v.RunResult);
            var arr = BO.Code.Bas.ConvertString2List(v.RunResult, "GO\r\n");
            
            

            foreach (var s in arr)
            {
                if (!string.IsNullOrEmpty(s) && s.Length>5)
                {
                    
                    Factory.FBL.RunSql(s,null, strConString);
                }
            }
          
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
        }
    }
}
