
using System.Data.SqlClient;
using Dapper;
using System.Collections;

namespace DL
{
    public class DbHandler
    {
        public BO.RunningUser CurrentUser { get; set; }
        private string _conString;
        private string _logDir;
        private string _conStringOrig;
        public DbHandler(string connectstring, BO.RunningUser ru, string strLogDir)
        {
            _conString = connectstring;
            this.CurrentUser = ru;

            
            _logDir = strLogDir;
        }

        public void ChangeConnectString(string s)
        {
            if (_conStringOrig == null)
            {
                _conStringOrig = _conString;
            }
            if (string.IsNullOrEmpty(s))
            {
                _conString = _conStringOrig;
            }
            else
            {
                _conString = s;
            }
            
        }
        public void ChangeDbNameInConnectString(string newdbname)
        {
            var lis = BO.Code.Bas.ConvertString2List(_conString, ";");
            for(int i = 0; i < lis.Count(); i++)
            {
                if (lis[i].Contains("database"))
                {
                    lis[i] = "database=" + newdbname;
                }
            }

            _conString = string.Join(";", lis);
            _conStringOrig = _conString;
        }

        public string RunSp(string strProcName, ref Dapper.DynamicParameters pars,bool has_err_ret=true, int? timeout_seconds = null)
        {                        
            using (SqlConnection con = new SqlConnection(_conString))
            {
                try
                {
                    con.Query(strProcName, pars, null, true, timeout_seconds, System.Data.CommandType.StoredProcedure);
                    
                    if (has_err_ret && string.IsNullOrEmpty(pars.Get<string>("err_ret"))==false)
                    {
                        this.CurrentUser.AddMessage(pars.Get<string>("err_ret"));                        
                        return pars.Get<string>("err_ret");
                        
                    }
                    else
                    {                        
                        return "1";
                    }
                    
                }
                catch (Exception e)
                {
                    con.Close();
                    log_error(e, strProcName, pars).GetAwaiter().GetResult();
                    return e.Message;

                }


            }
        }


        public T Load<T>(string strSQL)
        {
            using (SqlConnection con = new SqlConnection(_conString))
            {
                try
                {
                    return con.Query<T>(strSQL).FirstOrDefault();
                }
                catch (Exception e)
                {
                    con.Close();
                    log_error(e, strSQL).GetAwaiter().GetResult();
                    return default(T);
                }

            }
        }
        public T Load<T>(string strSQL, object param)
        {
            using (SqlConnection con = new SqlConnection(_conString))
            {
                
                try
                {
                    return con.Query<T>(strSQL, param).FirstOrDefault();
                }
                catch (Exception e)
                {
                    con.Close();
                    log_error(e, strSQL, param).GetAwaiter().GetResult();
                    return default(T);
                }

            }
        }
        public T Load<T>(string strSQL, Dapper.DynamicParameters pars)
        {
            using (SqlConnection con = new SqlConnection(_conString))
            {
                try
                {
                    return con.Query<T>(strSQL, pars).FirstOrDefault();
                }
                catch (Exception e)
                {
                    con.Close();
                    log_error(e, strSQL, pars).GetAwaiter().GetResult();
                    return default(T);
                }
                

            }
        }
        public IEnumerable<T> GetList<T>(string strSQL)
        {
            using (SqlConnection con = new SqlConnection(_conString))
            {
                
                try
                {

                    
                    if (CurrentUser !=null && CurrentUser.j02IsDebugLog)
                    {
                        log_debug(strSQL, "GetList strSQL", null).GetAwaiter().GetResult();
                    }
                    return con.Query<T>(strSQL);
                }
                catch (Exception e)
                {
                    con.Close();
                    log_error(e, strSQL).GetAwaiter().GetResult();
                    return null;
                }


            }
        }

        
        public IEnumerable<T> GetList<T>(string strSQL, object param)
        {
            using (SqlConnection con = new SqlConnection(_conString))
            {
                try
                {
                    
                    if (CurrentUser !=null && CurrentUser.j02IsDebugLog)
                    {
                        log_debug(strSQL, "GetList param", param).GetAwaiter().GetResult();
                    }
                    return con.Query<T>(strSQL, param);
                }
                catch (Exception e)
                {
                    con.Close();
                    log_error(e, strSQL, param).GetAwaiter().GetResult();
                    return null;
                }

            }
        }
        public IEnumerable<T> GetList<T>(string strSQL, Dapper.DynamicParameters pars)
        {
            //var t0 = DateTime.Now;
            using (SqlConnection con = new SqlConnection(_conString))
            {
                try
                {
                    
                    if (CurrentUser !=null && CurrentUser.j02IsDebugLog)
                    {
                        log_debug(strSQL, "GetList Dapper.DynamicParameters pars", pars).GetAwaiter().GetResult();
                    }
                    return con.Query<T>(strSQL, pars);
                    
                }
                catch (Exception e)
                {
                    con.Close();
                    log_error(e, strSQL, pars).GetAwaiter().GetResult();
                    return null;
                }

            }
        }

        

        public System.Data.DataTable GetDataTable(string strSQL, List<DL.Param4DT> pars = null)
        {
           
            System.Data.DataTable dt = new System.Data.DataTable();
            
            using (SqlConnection con = new SqlConnection(_conString))
            {
                con.StatisticsEnabled = false;

                SqlCommand cmd = new SqlCommand();
                cmd.CommandText = strSQL;
                
                
                cmd.Connection = con;
                if (pars != null)
                {
                    foreach (var p in pars)
                    {
                        cmd.Parameters.AddWithValue(p.ParName, p.ParValue);
                    }
                }

                SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                try
                {
                    
                    if (CurrentUser !=null && CurrentUser.j02IsDebugLog)
                    {
                        log_debug(strSQL, "GetDataTable", null).GetAwaiter().GetResult();
                    }
                    adapter.Fill(dt);
                }
                catch (Exception e)
                {
                    string strError = $"GetDataTable: {strSQL}";
                    if (pars != null)
                    {
                        foreach (var p in pars)
                        {
                            strError +=$"\"Param4DT: {p.ParName} = {p.ParValue}";
                        }
                    }
                    else
                    {
                        strError += "\" Bez parametrů Param4DT";
                    }
                   
                    log_error(e, strError,null).GetAwaiter().GetResult();
                   
                }

                con.Close();

                if (con.StatisticsEnabled)
                {
                    var stats = con.RetrieveStatistics();

                    log_statistics(stats, strSQL, "GetDataTable", null).GetAwaiter().GetResult();
                }

                return dt;

            }
        }

        
        public int SaveRecord(string strTable, Params4Dapper p, BO.BaseBO rec,bool isvalidity=true,bool istimestamp=true)
        {
            
            DynamicParameters pars = p.getDynamicDapperPars();
            string strPrefix = strTable.Substring(0, 3);
            var strPidField = $"{strPrefix}ID";
            var s = new System.Text.StringBuilder();
            bool bolInsert = true;
            if (rec.pid > 0) bolInsert = false;
            
            if (bolInsert)
            {
                s.Append(string.Format("INSERT INTO {0} (", strTable));
                if (istimestamp)
                {
                    pars.Add($"{strPrefix}DateInsert", DateTime.Now, System.Data.DbType.DateTime);
                    pars.Add($"{strPrefix}UserInsert", this.CurrentUser.j02Login, System.Data.DbType.String);
                }                
            }
            else
            {
                s.Append(string.Format("UPDATE {0} SET ", strTable));
            }
            if (istimestamp)
            {
                pars.Add($"{strPrefix}DateUpdate", DateTime.Now, System.Data.DbType.DateTime);
                pars.Add($"{strPrefix}UserUpdate", this.CurrentUser.j02Login, System.Data.DbType.String);
            }
            
            if (isvalidity)
            {
                if (rec.ValidFrom == null) rec.ValidFrom = System.DateTime.Now;
                pars.Add($"{strPrefix}ValidFrom", rec.ValidFrom, System.Data.DbType.DateTime);
                if (rec.ValidUntil == null) rec.ValidUntil = new DateTime(3000, 1, 1);
                pars.Add($"{strPrefix}ValidUntil", rec.ValidUntil, System.Data.DbType.DateTime);
            }
            


            //string strF = "", strV = "";
            List<string> sf = new List<string>();
            List<string> sv = new List<string>();
           
            foreach (var strP in pars.ParameterNames.Where(p => p != "pid"))
            {                                
                if (bolInsert)
                {
                    sf.Add(strP);
                    sv.Add($"@{strP}");
                    //strF += $",{strP}";
                    //strV += $",@{strP}";
                }
                else
                {
                    //strV += $",{strP} = @{strP}";
                    sv.Add($"{strP} = @{strP}");
                }
            }
            string strV = string.Join(",", sv);

            //strV = strV.Substring(1, strV.Length - 1);
            if (bolInsert)
            {
                string strF = string.Join(",",sf);
                //strF = strF.Substring(1, strF.Length - 1);
                //s.Append(strF + ") VALUES (" + strV + ")");
                s.Append($"{strF}) VALUES ({strV})");
            }
            else
            {
                s.Append(strV);
                s.Append($" WHERE {strPidField} = @pid");
            }


            using (SqlConnection con = new SqlConnection(_conString))
            {
                foreach (var c in p.getCatalog().Where(p => p.ParamType == "string" && p.ParValue != null && p.ParValue.ToString().Length > 5))
                {
                    //kontrola velikosti obsahu string polí
                    int intMaxSize= con.Query<BO.GetInteger>("select dbo.getfieldsize(@fld, @tbl) as Value",new { fld = c.ParName, tbl = strTable }).FirstOrDefault().Value;
                    if (intMaxSize>0 && c.ParValue.ToString().Length > intMaxSize)
                    {
                        CurrentUser.AddMessage($"Délka pole ** {c.ParName} ** může být maximálně {intMaxSize} znaků. Vy posíláte {c.ParValue.ToString().Length} znaků.");
                        return 0;
                    }
                }

                try
                {
                    if (bolInsert)
                    {
                        s.Append("; SELECT CAST(SCOPE_IDENTITY() as int) as Value");

                        return con.Query<BO.GetInteger>(s.ToString(), pars).FirstOrDefault().Value;
                    }
                    else
                    {
                        if (con.Execute(s.ToString(), pars) > 0)
                        {
                            return pars.Get<int>("pid");
                        }
                    }
                }
                catch (Exception e)
                {

                    log_error(e, s.ToString(), pars).GetAwaiter().GetResult();
                }


            }


            return 0;
        }


       
        public bool RunSql(string strSQL, object param = null, int? timeout_seconds = null)
        {
            using (SqlConnection con = new SqlConnection(_conString))
            {
                con.StatisticsEnabled = false;
                
                try
                {
                    
                    if (con.Execute(strSQL, param,null,timeout_seconds) > 0)
                    {
                        
                        

                        if (CurrentUser !=null && CurrentUser.j02IsDebugLog)
                        {
                            log_debug(strSQL, "RunSql", param).GetAwaiter().GetResult();
                        }                        
                        return true;
                    }
                }
                catch (Exception e)
                {
                    log_error(e, strSQL, param).GetAwaiter().GetResult();
                    return false;
                }
                if (con.StatisticsEnabled)
                {
                    con.Close();
                    var stats = con.RetrieveStatistics();

                    log_statistics(stats, strSQL, "RunSql", param).GetAwaiter().GetResult();
                }
            }


            return false;
        }

        public bool RunSql2(string strSQL, object param = null, int? timeout_seconds = null)    //zde nezáleží, zda con.Execute vrací číslo nebo 0
        {
            using (SqlConnection con = new SqlConnection(_conString))
            {
                try
                {
                    con.Execute(strSQL, param, null, timeout_seconds);
                    return true;
                }
                catch(Exception e)
                {
                    log_error(e, strSQL, param).GetAwaiter().GetResult();
                    return false;
                }
                
            }
           
        }

        public string GetSQL1_Ocas(string strPrefix, bool isthegrid = false, bool isvalidity = true,bool istimestamp=true)
        {
            
            string s = null;
            if (isvalidity)
            {
                if (isthegrid)
                {
                    //s = $"a.{strPrefix}ID as pid,convert(bit,CASE WHEN GETDATE() BETWEEN a.{strPrefix}ValidFrom AND a.{strPrefix}ValidUntil THEN 0 ELSE 1 end) as isclosed";
                    s = $"a.{strPrefix}ID as pid,convert(bit,CASE WHEN a.{strPrefix}ValidUntil > GETDATE() THEN 0 ELSE 1 end) as isclosed";
                }
                else
                {
                    //s = $"a.{strPrefix}ID as pid,CASE WHEN GETDATE() BETWEEN a.{strPrefix}ValidFrom AND a.{strPrefix}ValidUntil THEN 0 ELSE 1 end as isclosed,'{strPrefix}' as entity,a.{strPrefix}DateInsert as DateInsert,a.{strPrefix}UserInsert as UserInsert,a.{strPrefix}DateUpdate as DateUpdate,a.{strPrefix}UserUpdate as UserUpdate,a.{strPrefix}ValidFrom as ValidFrom,a.{strPrefix}ValidUntil as ValidUntil";
                    s = $"a.{strPrefix}ID as pid,CASE WHEN a.{strPrefix}ValidUntil>GETDATE() THEN 0 ELSE 1 end as isclosed,'{strPrefix}' as entity,a.{strPrefix}DateInsert as DateInsert,a.{strPrefix}UserInsert as UserInsert,a.{strPrefix}DateUpdate as DateUpdate,a.{strPrefix}UserUpdate as UserUpdate,a.{strPrefix}ValidFrom as ValidFrom,a.{strPrefix}ValidUntil as ValidUntil";
                }

            }
            else
            {
                if (isthegrid)
                {
                    s = $"a.{strPrefix}ID as pid,convert(bit,0) as isclosed";
                }
                else
                {
                    if (istimestamp)
                    {
                        s = $"a.{strPrefix}ID as pid,0 as isclosed,'{strPrefix}' as entity,a.{strPrefix}DateInsert as DateInsert,a.{strPrefix}UserInsert as UserInsert,a.{strPrefix}DateUpdate as DateUpdate,a.{strPrefix}UserUpdate as UserUpdate";
                    }
                    else
                    {
                        s = $"a.{strPrefix}ID as pid,0 as isclosed,'{strPrefix}' as entity";
                    }


                }

            }
            if (isthegrid)
            {
                switch (strPrefix)
                {
                    case "p91":
                        s = $"{s},a.p91IsDraft,a.p91DateMaturity,a.p91Amount_Debt,a.p91Amount_TotalDue,a.p91LockFlag,a.p91RowColorFlag,b02x.b02Color,a.p84ID_Last";
                        break;
                    case "p31":
                        s = $"{s},a.p72ID_AfterApprove,a.p72ID_AfterTrimming,a.p70ID,a.p71ID,a.p91ID,p91x.p91IsDraft,p34x.p33ID,p34x.p34IncomeStatementFlag,p32x.p32IsBillable,a.p31Rate_Billing_Orig,a.p31Amount_WithoutVat_Orig,p41x.p41BillingFlag,a.p31ExcludeBillingFlag,a.b05ID_Last,a.p31RowColorFlag,a.o23ID";
                        break;
                    case "p41":
                    case "le5":
                    case "le4":
                    case "le3":
                    case "le2":
                    case "le1":
                        s = $"{s},a.p41BillingFlag,b02x.b02Color,a.p41RowColorFlag,p07x.p07Level";
                        break;
                    case "p28":
                        s = $"{s},a.p28TreePrev as treeprev,a.p28TreeNext as treenext,a.p28RowColorFlag";
                        break;
                    case "o43":
                        s = $"{s},a.o43IsFlagged as IsFlagged,a.o43IsDraft as IsDraft,a.o43IsDeleted as IsDeleted,a.o43IsSeen as IsSeen,a.o43AttachmentsCount as AttachmentsCount";
                        break;
                    case "j02":
                        s = $"{s},isnull(a.j02IsLoginManualLocked,0) as IsLoginManualLocked,isnull(a.j02IsLoginAutoLocked,0) as IsLoginAutoLocked,a.j02VirtualParentID";
                            break;
                    case "p56":
                        s += ",b02x.b02Color";
                        break;
                    case "o22":
                        s += ",o21x.o21Color";
                        break;
                    case "p32":
                        s += ",a.p32IsBillable";
                        break;
                    case "b05":
                        s = $"{s},a.b05Tab1Flag & 4 as tab1flag4";
                        break;
                    case "p49":
                        s = $"{s},a.p49StatusFlag,case when p34x.p34IncomeStatementFlag=2 then 'P' else 'V' end as strana";
                        break;
                    case "p40":
                        s = $"{s},p39miss.p39ID_Max,case when p39miss.p39DateCreate_Min<DATEADD(DAY,-31,GETDATE()) then p39miss.p39DateCreate_Min end as p39DateCreate_MimoDosah ,case when a.p40LastSupplyDate<GETDATE() then 1 end as Vyprsel,case when p41x.p41ValidUntil<GETDATE() then 1 end as ProjektUzavren";
                        break;
                    case "o23":
                        s = $"{s},b02x.b02Color,a.o23RowColorFlag,a.o27ID_Last";
                        break;
                    case "x31":
                        s += ",a.x31IsScheduling as IsScheduling";
                        break;


                }
            }

          
            return s;
            

        }

        private async Task log_statistics(IDictionary stats, string strSQL, string strProc, object param = null)
        {

            var filePath = $"{_logDir}\\sql-statistics-{CurrentUser.j02Login}-{DateTime.Now.ToString("yyyy.MM.dd")}.log";

            using (System.IO.FileStream stream = new System.IO.FileStream(filePath, System.IO.FileMode.Append, System.IO.FileAccess.Write, System.IO.FileShare.Write, 4096, true))
            using (System.IO.StreamWriter sw = new System.IO.StreamWriter(stream))
            {

                await sw.WriteLineAsync("------------------------------" + Environment.NewLine);
                await sw.WriteLineAsync(DateTime.Now.ToString() + ", proc: " + strProc + Environment.NewLine);
                await sw.WriteLineAsync("sql: " + strSQL + Environment.NewLine);
                if (param != null)
                {
                    await sw.WriteLineAsync("PARAMs: " + param.ToString());
                }

                foreach (DictionaryEntry de in stats)
                {
                    await sw.WriteLineAsync($"{de.Key}: {de.Value}");
                }
                if (CurrentUser != null)
                {
                    await sw.WriteLineAsync("CURRENT USER-login: " + CurrentUser.j02Login + ", USER-name:" + CurrentUser.FullNameAsc + Environment.NewLine);
                }
            }
            


        }

        private async Task log_debug(string strSQL, string strProc, object param = null)
        {
            
            var filePath = $"{_logDir}\\sql-debug-{CurrentUser.j02Login}-{DateTime.Now.ToString("yyyy.MM.dd")}.log";

            using (System.IO.FileStream stream = new System.IO.FileStream(filePath, System.IO.FileMode.Append, System.IO.FileAccess.Write, System.IO.FileShare.Write, 4096, true))
            using (System.IO.StreamWriter sw = new System.IO.StreamWriter(stream))
            {

                await sw.WriteLineAsync("------------------------------"+Environment.NewLine);
                await sw.WriteLineAsync(DateTime.Now.ToString() + ", proc: " + strProc+ Environment.NewLine);
                await sw.WriteLineAsync("sql: " + strSQL+ Environment.NewLine);
                if (param != null)
                {
                    await sw.WriteLineAsync("PARAMs: " + param.ToString());
                }
            }

            
        }

        private async Task log_debug(string strSQL, string strProc, DynamicParameters pars)
        {
            
            var filePath = $"{_logDir}\\sql-debug-{CurrentUser.j02Login}-{DateTime.Now.ToString("yyyy.MM.dd")}.log";

            using (System.IO.FileStream stream = new System.IO.FileStream(filePath, System.IO.FileMode.Append, System.IO.FileAccess.Write, System.IO.FileShare.Write, 4096,true))
            using (System.IO.StreamWriter sw = new System.IO.StreamWriter(stream))
            {

                await sw.WriteLineAsync("------------------------------" + Environment.NewLine);
                await sw.WriteLineAsync(DateTime.Now.ToString() + ", proc: " + strProc + Environment.NewLine);
                await sw.WriteLineAsync("sql: " + strSQL + Environment.NewLine);
                if (pars != null)
                {
                    string strVal = "";
                    foreach (var strP in pars.ParameterNames)
                    {
                        if (pars.Get<dynamic>(strP) != null)
                        {
                            strVal = pars.Get<dynamic>(strP).ToString();
                        }
                        else
                        {
                            strVal = "NULL";
                        }
                        await sw.WriteLineAsync("PARAM: " + strP + ", VALUE: " + strVal+Environment.NewLine);
                        
                    }
                }
            }

            
        }
        private async Task log_error(Exception e, string strSQL, DynamicParameters pars)
        {
            if (CurrentUser != null) CurrentUser.AddMessage(e.Message);

            var filePath = string.Format("{0}\\sql-error-{1}.log", _logDir, DateTime.Now.ToString("yyyy.MM.dd"));

            using (System.IO.FileStream stream = new System.IO.FileStream(filePath, System.IO.FileMode.Append, System.IO.FileAccess.Write, System.IO.FileShare.Write, 4096, true))
            using (System.IO.StreamWriter sw = new System.IO.StreamWriter(stream))
            {
                await sw.WriteLineAsync("------------------------------" + Environment.NewLine);
                await sw.WriteLineAsync(DateTime.Now.ToString() + Environment.NewLine);

                if (CurrentUser != null)
                {
                    await sw.WriteLineAsync("CURRENT USER-login: " + CurrentUser.j02Login + ", USER-name:" + CurrentUser.FullNameAsc + Environment.NewLine);
                }

                await sw.WriteLineAsync("SQL:" + strSQL + Environment.NewLine);                

                if (pars != null)
                {
                    string strVal = "";
                    foreach (var strP in pars.ParameterNames)
                    {

                        if (pars.Get<dynamic>(strP) != null)
                        {
                            strVal = pars.Get<dynamic>(strP).ToString();
                        }
                        else
                        {
                            strVal = "NULL";
                        }
                        await sw.WriteLineAsync("PARAM: " + strP + ", VALUE: " + strVal + Environment.NewLine);                        

                    }
                }

                await sw.WriteLineAsync("ERROR:" + e.Message + Environment.NewLine);


            }


        }

        private async Task log_error(Exception e, string strSQL, object param = null)
        {
            if (CurrentUser != null) CurrentUser.AddMessage(e.Message);
            var filePath = string.Format("{0}\\sql-error-{1}.log", _logDir, DateTime.Now.ToString("yyyy.MM.dd"));

            using (System.IO.FileStream stream = new System.IO.FileStream(filePath, System.IO.FileMode.Append, System.IO.FileAccess.Write, System.IO.FileShare.Write, 4096, true))
            using (System.IO.StreamWriter sw = new System.IO.StreamWriter(stream))
            {
                await sw.WriteLineAsync("------------------------------" + Environment.NewLine);
                await sw.WriteLineAsync(DateTime.Now.ToString() + Environment.NewLine);

                if (CurrentUser != null)
                {                    
                    await sw.WriteLineAsync("CURRENT USER-login: " + CurrentUser.j02Login+ ", USER-name:" + CurrentUser.FullNameAsc + Environment.NewLine);                    
                }

                await sw.WriteLineAsync("SQL:" + strSQL + Environment.NewLine);
                if (param != null)
                {
                    await sw.WriteLineAsync("PARAMS:" + param.ToString() + Environment.NewLine);
                }
                
                await sw.WriteLineAsync("ERROR:" + e.Message + Environment.NewLine);

                
            }


        }


        public string ParseMergeSQL(string strSQL, string strPIDValue,string par1=null,string par2=null,int intJ02ID_Sys=0)
        {
            if (intJ02ID_Sys > 0)
            {
                strSQL = strSQL.Replace("@j02id_sys", intJ02ID_Sys.ToString(), StringComparison.OrdinalIgnoreCase);
            }
            if (!string.IsNullOrEmpty(strPIDValue))
            {                
                strSQL = strSQL.Replace("#pid#", strPIDValue, StringComparison.OrdinalIgnoreCase);
                strSQL = strSQL.Replace("[%pid%]", strPIDValue, StringComparison.OrdinalIgnoreCase);
                strSQL = strSQL.Replace("@pid", strPIDValue, StringComparison.OrdinalIgnoreCase);
            }
            
            if (par1 != null)
            {
                par1 = BO.Code.Bas.OcistitSQL(par1);
                strSQL = strSQL.Replace("#par1#", par1, StringComparison.OrdinalIgnoreCase);
                strSQL = strSQL.Replace("@par1", par1, StringComparison.OrdinalIgnoreCase);
            }
            if (par2 != null)
            {
                par2 =BO.Code.Bas.OcistitSQL(par2);
                strSQL = strSQL.Replace("#par2#", par2, StringComparison.OrdinalIgnoreCase);
                strSQL = strSQL.Replace("@par2", par2, StringComparison.OrdinalIgnoreCase);
            }            
            return BO.Code.Bas.OcistitSQL(strSQL);
        }


        public SqlDataReader GetDataReader(string sql)  //datareader je rychlý, ale nepoužívá cache pro opakující se dotazy a pro ukládání v rámci transakce!!!
        {
            SqlConnection con = new SqlConnection(_conString);
            SqlCommand cmd = new SqlCommand();
            cmd.CommandText = sql;
            cmd.Connection = con;
            con.Open();
            return cmd.ExecuteReader(); //je třeba nechat connection otevřené a v klientovi ho zavřít!

        }

        public int GetIntegerFromSql(string strSQL)
        {
            var dt = GetDataTable(strSQL);
                          
            if (dt.Rows.Count==0)
            {                
                return 0;
            }
            try
            {
                return Convert.ToInt32(dt.Rows[0][0]);
                
            }
            catch
            {
                return 0;
            }
            
        }

        

    }
}
