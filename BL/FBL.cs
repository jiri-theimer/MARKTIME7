
using System.ComponentModel;
using System.Data;


namespace BL
{
    public interface IFBL
    {
        public BO.j27Currency LoadCurrencyByCode(string j27code);
        public BO.j27Currency LoadCurrencyByID(int j27id);
        public IEnumerable<BO.j27Currency> GetListCurrency();
        public bool AppendSmsLog(string strNumber, string strMessage, string strResult, string strErrorCode, string strError, int intCustomID, int o24ID);
        public int AppendRobotLog(BO.j91RobotLog rec);  //uložení jetí robota na pozadí
        public BO.j91RobotLog GetLastRobotRun(BO.j91RobotTaskFlag flag); //vrátí poslední jetí pro zadaný flag
        public IEnumerable<BO.j91RobotLog> GetListRobotLast20();
        public IEnumerable<BO.j19PaymentType> GetListJ19();
        public IEnumerable<BO.j08CreatePermission> GetListJ08(string recprefix, int recpid);
        public BO.j19PaymentType LoadJ19(int j19id);
        public BO.x21DatePeriod LoadX21(int x21id);
        public IEnumerable<BO.x21DatePeriod> GetListX21(int j02id);
        public bool SaveX21Batch(List<BO.x21DatePeriod> lisX21);
        public IEnumerable<BO.x90Migration> GetListX90();
        public IEnumerable<BO.p27Pctype> GetListP27();
        public DataTable GetDataTable(string sql, string constring = null);
        public bool RunSql(string sql, object param = null, string constring = null);
        public IEnumerable<BO.Sys.SysDbObject> Sys_GetList_SysObjects();
        public void Sys_GenerateCreateUpdateScript(IEnumerable<BO.Sys.SysDbObject> lis);
        public IEnumerable<BO.Sys.sysobject> Sys_GetList_Tables(string dbname);
        public IEnumerable<BO.Sys.syscolumn> Sys_GetList_Columns(string dbname);
        public IEnumerable<BO.Sys.sysindex> Sys_GetList_Indexes(string dbname);
        public IEnumerable<BO.Sys.sysconstraint> Sys_GetList_Constraints(string dbname);
        public IEnumerable<BO.Sys.sysdefval> Sys_GetList_Defvals(string dbname);
        public void InsertUpdate_DynamicSqlViews();
        public IEnumerable<BO.Sys.sp_spaceused_table> GetTabsUsedSpace();
        public BO.Sys.sp_spaceused_db GetDbUsedSpace();
        public void ClearSpaceUsed(string oper, int month_before);
        public IEnumerable<BO.p31TotalByYear> GetListTotalsByYear(BL.p31TableEnum tab);
        public IEnumerable<BO.j97BellsLog> GetList_j97(int j02id, bool? ishidden);
        public bool SetHidden_j97(int j97id, bool bolHidden);
        public void RunRecovery_Permissions();
        public string BackupDatabase(string strDbName, string strDestFolder = null);

    }
    class FBL : BaseBL, IFBL
    {
        public FBL(BL.Factory mother) : base(mother)
        {

        }

        public IEnumerable<BO.x90Migration> GetListX90()
        {
            return _db.GetList<BO.x90Migration>("select * from x90Migration ORDER BY x90Ordinary,x90ID");
        }
        public IEnumerable<BO.p27Pctype> GetListP27()
        {
            return _db.GetList<BO.p27Pctype>($"SELECT {_db.GetSQL1_Ocas("p27")},a.* FROM p27Pctype a WHERE a.x01ID=@x01id", new { x01id = _mother.CurrentUser.x01ID });
        }
        public IEnumerable<BO.j19PaymentType> GetListJ19()
        {
            return _db.GetList<BO.j19PaymentType>("SELECT " + _db.GetSQL1_Ocas("j19") + ",a.* FROM j19PaymentType a");
        }
        public BO.j19PaymentType LoadJ19(int j19id)
        {
            return _db.Load<BO.j19PaymentType>("SELECT " + _db.GetSQL1_Ocas("j19") + ",a.* FROM j19PaymentType a WHERE a.j19ID=@pid", new { pid = j19id });
        }
        public BO.j27Currency LoadCurrencyByCode(string j27code)
        {
            return _db.Load<BO.j27Currency>("select *,j27ID as pid FROM j27Currency WHERE j27Code LIKE @j27code", new { j27code = j27code });
        }
        public BO.j27Currency LoadCurrencyByID(int j27id)
        {
            return _db.Load<BO.j27Currency>("select * FROM j27Currency WHERE j27ID = @j27id", new { j27id = j27id });
        }
        public IEnumerable<BO.j27Currency> GetListCurrency()
        {
            return _db.GetList<BO.j27Currency>("SELECT a.*,a.j27ID as pid FROM j27Currency a WHERE GETDATE() BETWEEN a.j27ValidFrom AND a.j27ValidUntil");
        }

        public DataTable GetDataTable(string sql, string constring = null)
        {
            _db.ChangeConnectString(constring);
            return _db.GetDataTable(sql);


        }
        public bool RunSql(string sql, object param = null, string constring = null)
        {

            _db.ChangeConnectString(constring);
            return _db.RunSql(sql, param);
        }
        public IEnumerable<BO.Sys.SysDbObject> Sys_GetList_SysObjects()
        {
            string s = "SELECT ID,name,xtype,schema_ver as version,convert(text,null) as content FROM sysobjects WHERE rtrim(xtype) IN ('V','FN','P','TR','IF') AND name not like 'dt_%' and name not like 'zzz%' and (name not like 'sys%' or name not like 'system_%') order by xtype,name";
            var lis = _db.GetList<BO.Sys.SysDbObject>(s);
            foreach (var c in lis)
            {
                string strContent = "";
                var dt = _db.GetDataTable("select colid,text FROM syscomments where id=" + c.ID.ToString() + " order by colid");
                foreach (DataRow dbrow in dt.Rows)
                {
                    strContent += dbrow["text"];
                }
                c.Content = strContent;
                c.xType = c.xType.Trim();
            }
            return lis;
        }


        public void Sys_GenerateCreateUpdateScript(IEnumerable<BO.Sys.SysDbObject> lis)
        {
            var sb = new System.Text.StringBuilder();
            foreach (var c in lis)
            {
                sb.AppendLine("if exists(select 1 from sysobjects where id = object_id('" + c.Name + "') and type = '" + c.xType + "')");
                switch (c.xType)
                {
                    case "P":
                        sb.AppendLine(" drop procedure " + c.Name);
                        break;
                    case "FN":
                    case "IF":
                        sb.AppendLine(" drop function " + c.Name);
                        break;
                    case "V":
                        sb.AppendLine(" drop view " + c.Name);
                        break;
                }
                sb.AppendLine("GO");
                sb.AppendLine("");
                sb.AppendLine("");
                sb.AppendLine(c.Content);
                sb.AppendLine("");
                sb.AppendLine("GO");

                //System.IO.File.WriteAllText(_mother.TempFolder + "\\sql_sp_funct_views.sql", sb.ToString());
                System.IO.File.WriteAllText("c:\\DEV\\MARKTIME7-DM\\COREDATA\\sql_sp_funct_views.sql", sb.ToString());
                System.IO.File.WriteAllText($"{_mother.App.RootUploadFolder}\\_distribution\\sys\\sql_sp_funct_views.sql", sb.ToString());
            }
        }

        public bool AppendSmsLog(string strNumber, string strMessage, string strResult, string strErrorCode, string strError, int intCustomID, int o24ID)
        {
            var p = new DL.Params4Dapper();
            p.AddInt("x01ID", _mother.CurrentUser.x01ID);
            p.AddDateTime("j93Date", DateTime.Now);
            p.AddString("j93Number", strNumber);
            p.AddString("j93Message", strMessage);
            p.AddString("j93Result", strResult);
            p.AddString("j93ErrorMessage", strError);
            p.AddString("j93ErrorCode", strErrorCode);
            p.AddInt("j93CustomID", intCustomID);
            p.AddInt("o24ID", o24ID, true);

            return _db.RunSql("INSERT INTO j93SmsLog(x01ID,j93Date,j93Number,j93Message,j93Result,j93ErrorMessage,j93ErrorCode,j93CustomID,o24ID) VALUES(@x01ID,@j93Date,@j93Number,@j93Message,@j93Result,@j93ErrorMessage,@j93ErrorCode,@j93CustomID,@o24ID)", p.getDynamicDapperPars());

        }

        public int AppendRobotLog(BO.j91RobotLog rec)
        {
            var p = new DL.Params4Dapper();
            p.AddInt("pid", rec.pid);
            p.AddInt("x01ID", _mother.CurrentUser.x01ID);
            p.AddDateTime("j91Date", DateTime.Now);
            if (rec.j91BatchGuid == Guid.Empty)
            {
                rec.j91BatchGuid = Guid.NewGuid();
            }
            p.AddString("j91BatchGuid", rec.j91BatchGuid.ToString());
            p.AddEnumInt("j91TaskFlag", rec.j91TaskFlag);
            p.AddString("j91InfoMessage", rec.j91InfoMessage);
            p.AddString("j91ErrorMessage", rec.j91ErrorMessage);
            p.AddString("j91Account", rec.j91Account);

            return _db.SaveRecord("j91RobotLog", p, rec, false, false);
        }

        public BO.j91RobotLog GetLastRobotRun(BO.j91RobotTaskFlag flag)
        {
            return _db.Load<BO.j91RobotLog>("select TOP 1 * FROM j91RobotLog WHERE j91TaskFlag=@flg AND x01ID=@x01id AND j91ErrorMessage IS NULL ORDER BY j91ID DESC", new { flg = (int)flag, x01id = _mother.CurrentUser.x01ID });
        }

        public IEnumerable<BO.j97BellsLog> GetList_j97(int j02id, bool? ishidden)
        {
            string s = "select TOP 200 *,j97ID as pid FROM j97BellsLog WHERE j02ID=@j02id AND j97Date>DATEADD(DAY,-100,GETDATE())";
            if (ishidden != null)
            {
                s = $"{s} AND j97IsHidden={BO.Code.Bas.GB((bool)ishidden)}";
            }
            return _db.GetList<BO.j97BellsLog>($"{s} ORDER BY j97Date DESC", new { j02id = _mother.CurrentUser.pid });
        }
        public bool SetHidden_j97(int j97id, bool bolHidden)
        {
            if (_db.RunSql("UPDATE j97BellsLog set j97IsHidden=@b WHERE j97ID=@pid", new { b = bolHidden, pid = j97id }))
            {
                _db.RunSql("UPDATE j02User SET j02Cache_BellsCount = (select COUNT(j97ID) FROM j97BellsLog WHERE j02ID=@j02id AND j97IsHidden=0 AND j97Date>DATEADD(DAY,-100,GETDATE())) WHERE j02ID=@j02id", new { j02Id = _mother.CurrentUser.pid });
            }

            return true;
        }

        public IEnumerable<BO.j91RobotLog> GetListRobotLast20()
        {
            return _db.GetList<BO.j91RobotLog>("select TOP 20 * FROM j91RobotLog WHERE x01ID=@x01id ORDER BY j91ID DESC", new { x01id = _mother.CurrentUser.x01ID });
        }

        public BO.x21DatePeriod LoadX21(int x21id)
        {
            return _db.Load<BO.x21DatePeriod>("SELECT " + _db.GetSQL1_Ocas("x21", false, false, false) + ",a.* FROM x21DatePeriod a WHERE a.x21ID=@pid AND a.x01ID=@x01id", new { pid = x21id, x01id = _mother.CurrentUser.x01ID });
        }
        public IEnumerable<BO.x21DatePeriod> GetListX21(int j02id)
        {
            return _db.GetList<BO.x21DatePeriod>("SELECT " + _db.GetSQL1_Ocas("x21", false, false, false) + ",a.* FROM x21DatePeriod a WHERE a.j02ID=@j02id AND a.x01ID=@x01id ORDER BY a.x21ValidFrom,a.x21ValidUntil", new { j02id = j02id, x01id = _mother.CurrentUser.x01ID });
        }

        public IEnumerable<BO.j08CreatePermission> GetListJ08(string recprefix, int recpid)
        {
            return _db.GetList<BO.j08CreatePermission>("select * FROM j08CreatePermission WHERE j08RecordEntity=@prefix AND j08RecordPid=@pid", new { prefix = recprefix, pid = recpid });
        }
        public bool SaveX21Batch(List<BO.x21DatePeriod> lisX21)
        {
            if (lisX21.Any(p => string.IsNullOrEmpty(p.x21Name) && !p.IsTempDeleted))
            {
                this.AddMessage("V pojmenovaném období chybí název."); return false;
            }
            if (lisX21.Any(p => !p.IsTempDeleted && (p.x21ValidFrom.Year <= 1900 || p.x21ValidUntil.Year <= 1900)))
            {
                this.AddMessage("V pojmenovaném období chybí rok."); return false;
            }

            using (var sc = new System.Transactions.TransactionScope())
            {
                foreach (var c in lisX21)
                {
                    if (c.IsTempDeleted)
                    {
                        if (c.pid > 0)
                        {
                            _db.RunSql("DELETE FROM x21DatePeriod WHERE x21ID=@pid", new { pid = c.pid });
                        }
                    }
                    else
                    {
                        if (c.pid == 0)
                        {
                            int intMax = _db.GetIntegerFromSql("select max(x21ID) FROM x21DatePeriod") + 1;
                            if (intMax <= 60) intMax = 61;
                            _db.RunSql("SET IDENTITY_INSERT x21DatePeriod ON; INSERT INTO x21DatePeriod(x21ID,x21Name,x21ValidFrom,x21ValidUntil,j02ID,x01ID) VALUES(@x21id,@x21name,@d1,@d2,@j02id,@x01id); SET IDENTITY_INSERT x21DatePeriod OFF", new { x21id = intMax, x21name = c.x21Name, d1 = c.x21ValidFrom, d2 = c.x21ValidUntil, j02id = _mother.CurrentUser.pid, x01id = _mother.CurrentUser.x01ID });
                        }
                        else
                        {
                            _db.RunSql("UPDATE x21DatePeriod SET j02ID=@j02id,x21Name=@x21name,x21ValidFrom=@d1,x21ValidUntil=@d2 WHERE x21ID=@pid", new { pid = c.pid, x21name = c.x21Name, d1 = c.x21ValidFrom, d2 = c.x21ValidUntil, j02id = _mother.CurrentUser.pid });
                        }

                    }

                }
                sc.Complete();
            }


            return true;
        }
        public IEnumerable<BO.p31TotalByYear> GetListTotalsByYear(BL.p31TableEnum tab)
        {
            string strTab = "p31Worksheet";
            if (tab == BL.p31TableEnum.p31Worksheet_Del)
            {
                strTab = "p31Worksheet_Del";
            }
            if (tab == BL.p31TableEnum.p31Worksheet_Log)
            {
                strTab = "p31Worksheet_Log";
            }
            return _db.GetList<BO.p31TotalByYear>($"SELECT YEAR(p31Date) as Rok,COUNT(*) as Pocet FROM {strTab} GROUP BY YEAR(p31Date)");
        }
        public void ClearSpaceUsed(string oper, int month_before)
        {
            DateTime? datBefore = null;
            if (month_before > 0)
            {
                datBefore = DateTime.Today.AddMonths(-1 * month_before);
            }

            switch (oper.ToLower())
            {
                case "outbox":
                    if (datBefore != null)
                    {
                        _db.RunSql("DELETE FROM x43MailQueue_Recipient WHERE x40ID IN (select x40ID FROM x40MailQueue WHERE x40DateInsert<@d)", new { d = datBefore });
                        _db.RunSql("DELETE FROM x40MailQueue WHERE x40ID IN (select x40ID FROM x40MailQueue WHERE x40DateInsert<@d)", new { d = datBefore });
                    }
                    else
                    {
                        _db.RunSql("truncate table x43MailQueue_Recipient");
                        _db.RunSql("delete from x40MailQueue");

                    }

                    break;
                case "temp":
                    _db.RunSql("truncate table p85TempBox");
                    _db.RunSql("truncate table p31Worksheet_Temp");


                    if (System.IO.Directory.Exists(_mother.TempFolder)) //smazat obsah temp složky
                    {
                        var files = BO.Code.File.GetFileListFromDir(_mother.TempFolder, "*.*", SearchOption.AllDirectories, true);

                        for (int i = 0; i < files.Count(); i++)
                        {
                            System.IO.File.Delete(files[i]);

                        }
                    }

                    break;
                case "eml":

                    if (System.IO.Directory.Exists(_mother.UploadFolder + "\\eml")) //smazat obsah eml složky
                    {
                        var files = BO.Code.File.GetFileListFromDir(_mother.UploadFolder + "\\eml", "*.*", SearchOption.AllDirectories, true);
                        for (int i = 0; i < files.Count(); i++)
                        {
                            System.IO.File.Delete(files[i]);

                        }
                        System.IO.Directory.Delete(_mother.UploadFolder + "\\eml", true);
                        System.IO.Directory.CreateDirectory(_mother.UploadFolder + "\\eml");

                    }

                    break;
                case "p96":
                    _db.RunSql("truncate table p96Imprint");

                    if (System.IO.Directory.Exists(_mother.UploadFolder + "\\p91\\PdfImprint")) //smazat pdf archiv
                    {
                        var files = BO.Code.File.GetFileListFromDir(_mother.UploadFolder + "\\p91\\PdfImprint", "*.*", SearchOption.AllDirectories, true);
                        for (int i = 0; i < files.Count(); i++)
                        {
                            System.IO.File.Delete(files[i]);

                        }
                        System.IO.Directory.Delete(_mother.UploadFolder + "\\p91\\PdfImprint", true);
                        System.IO.Directory.CreateDirectory(_mother.UploadFolder + "\\p91\\PdfImprint");

                    }

                    break;
                case "j92":
                    if (datBefore != null)
                    {
                        _db.RunSql("DELETE FROM j92PingLog WHERE j92Date<@d", new { d = datBefore });
                        _db.RunSql("DELETE FROM j90LoginAccessLog WHERE j90Date<@d", new { d = datBefore });
                        _db.RunSql("DELETE FROM j91RobotLog WHERE j91Date<@d", new { d = datBefore });
                    }
                    else
                    {
                        _db.RunSql("truncate table j92PingLog");
                        _db.RunSql("truncate table j90LoginAccessLog");
                        _db.RunSql("truncate table j91RobotLog");
                    }
                    _db.RunSql("update j02User set j02Ping_Timestamp=null");
                    break;

                case "p31log":
                    if (datBefore != null)
                    {
                        _db.RunSql("DELETE FROM p31Worksheet_Log WHERE ISNULL(RowDate,p31DateUpdate)<@d", new { d = datBefore });
                    }
                    else
                    {
                        _db.RunSql("truncate table p31Worksheet_Log");
                    }
                    break;
                case "updatelog":
                    if (datBefore != null)
                    {
                        _db.RunSql("DELETE FROM p28Contact_log WHERE ISNULL(RowDate,p28DateUpdate)<@d", new { d = datBefore });
                        _db.RunSql("DELETE FROM o23Doc_Log WHERE ISNULL(RowDate,o23DateUpdate)<@d", new { d = datBefore });
                        _db.RunSql("DELETE FROM o43Inbox_Log WHERE ISNULL(RowDate,o43DateUpdate)<@d", new { d = datBefore });
                        _db.RunSql("DELETE FROM p56Task_Log WHERE ISNULL(RowDate,p56DateUpdate)<@d", new { d = datBefore });
                        _db.RunSql("DELETE FROM p41Project_Log WHERE ISNULL(RowDate,p41DateUpdate)<@d", new { d = datBefore });
                        _db.RunSql("DELETE FROM p91Invoice_Log WHERE ISNULL(RowDate,p91DateUpdate)<@d", new { d = datBefore });
                    }
                    else
                    {
                        _db.RunSql("truncate table p28Contact_log");
                        _db.RunSql("truncate table o23Doc_Log");
                        _db.RunSql("truncate table o43Inbox_Log");
                        _db.RunSql("truncate table p56Task_Log");
                        _db.RunSql("truncate table p41Project_Log");
                        _db.RunSql("truncate table p91Invoice_Log");
                    }

                    break;
            }
        }
        public IEnumerable<BO.Sys.sp_spaceused_table> GetTabsUsedSpace()
        {
            return _db.GetList<BO.Sys.sp_spaceused_table>("exec dbo.show_table_sizes");
        }
        public BO.Sys.sp_spaceused_db GetDbUsedSpace()
        {
            var lis = _db.GetList<BO.Sys.sp_spaceused_db>("EXEC sp_spaceused @oneresultset = 1");
            if (lis.Count() > 0)
            {
                return lis.First();
            }

            return null;

        }
       

        

        public IEnumerable<BO.Sys.sysobject> Sys_GetList_Tables(string dbname)
        {
            return _db.GetList<BO.Sys.sysobject>($"SELECT id,name,xtype FROM [{dbname}].dbo.sysobjects WHERE xtype='U' and name<>'dtproperties' and [name] not like 'zzz%' order by name");
        }
        public IEnumerable<BO.Sys.sysconstraint> Sys_GetList_Constraints(string dbname)
        {
            return _db.GetList<BO.Sys.sysconstraint>($"select parent.id as tableid,parent.name as tablename, a.id,a.name,LTRIM(RTRIM(a.xtype)) as xtype FROM [{dbname}].dbo.sysobjects a INNER JOIN [{dbname}].dbo.sysobjects parent ON a.parent_obj=parent.id WHERE a.xtype IN ('D','F','PK')");
        }
        public IEnumerable<BO.Sys.sysdefval> Sys_GetList_Defvals(string dbname)
        {
            return _db.GetList<BO.Sys.sysdefval>($"SELECT A.id,B.name, A.text FROM [{dbname}].dbo.syscomments A INNER JOIN [{dbname}].dbo.syscolumns B ON A.id = B.cdefault");
        }
        public IEnumerable<BO.Sys.syscolumn> Sys_GetList_Columns(string dbname)
        {
            sb("select a.[name], a.[id], a.[xtype], a.[typestat], a.[xusertype],case when (b.name='nvarchar' or b.name='nchar') and a.[length]<>-1 then a.[length]/2 else a.[length] end as length");
            sb(" , a.[xprec], a.[xscale], a.[colid], a.[xoffset], a.[bitpos], a.[reserved], a.[colstat], a.[cdefault], a.[domain], a.[number], a.[colorder], a.[autoval], a.[offset], a.[collationid], a.[language], a.[status], a.[type], a.[usertype], a.[printfmt], a.[prec], a.[scale], a.[iscomputed], a.[isoutparam], a.[isnullable], a.[collation], a.[tdscollation]");
            sb(",b.name as type_name,c.name as tablename,computed.text as formula");
            sb($" from [{dbname}].dbo.syscolumns a INNER JOIN (select * from [{dbname}].dbo.systypes where name not like 'sysname') b ON a.xtype=b.xtype INNER JOIN [{dbname}].dbo.sysobjects c ON a.ID=c.ID");
            sb($" LEFT OUTER JOIN (select a1.id,a1.colorder,a2.text FROM [{dbname}].dbo.syscolumns a1 INNER JOIN [{dbname}].dbo.syscomments a2 ON a1.id=a2.id AND a1.colorder=a2.number WHERE a1.iscomputed=1) computed");
            sb(" ON a.id=computed.id AND a.colorder=computed.colorder");
            sb($" where a.ID IN (select id from [{dbname}].dbo.sysobjects where xtype='U' and [name]<>'dtproperties' and [name] not like 'zzz%') AND a.id is not null");
            sb(" ORDER BY c.name,a.colorder");

            return _db.GetList<BO.Sys.syscolumn>(sbret());
        }
        public IEnumerable<BO.Sys.sysindex> Sys_GetList_Indexes(string dbname)
        {
            var ret = new List<BO.Sys.sysindex>();
            var lisTabs = _db.GetList<BO.Sys.sysobject>($"SELECT id,name FROM [{dbname}].dbo.sysobjects WHERE xtype='U' and name<>'dtproperties' and [name] not like 'zzz%' order by name");
            foreach (var tab in lisTabs)
            {
                var dt = _db.GetDataTable($"exec [{dbname}].dbo.sp_helpindex {tab.name}");
                foreach (System.Data.DataRow dbrow in dt.Rows)
                {
                    var c = new BO.Sys.sysindex() { tabid = tab.id, tabname = tab.name };
                    c.index_name = dbrow["index_name"].ToString();
                    if (dbrow["index_description"] != System.DBNull.Value)
                    {
                        c.index_description = dbrow["index_description"].ToString();
                    }
                    c.index_keys = dbrow["index_keys"].ToString();
                    ret.Add(c);
                }
            }

            return ret.AsEnumerable();
        }


        public void RunRecovery_Permissions()
        {
            _db.RunSql("exec dbo.x72_recovery_all");    //manažerské oprávnění k projektům
            _db.RunSql("exec dbo.x71_recovery_all");    //přiřazené role k záznamům
            _db.RunSql("exec dbo.x73_recovery_all");    //manažerské oprávnění k fakturám
            _db.RunSql("exec dbo.x74_recovery_all");    //oprávnění k dokumentům
            _db.RunSql("exec dbo.x75_recovery_all");    //oprávnění ke kontaktům
        }
        public void InsertUpdate_DynamicSqlViews()
        {
            var files = BO.Code.File.GetFileNamesInDir($"{_mother.App.RootUploadFolder}\\_distribution\\dynamicsql", "*.sql", true);
            foreach (string file in files)
            {
                var strMatrika = BO.Code.File.GetFileContent(file);
                var strFileName = BO.Code.File.GetFileInfo(file).Name;
                strFileName = strFileName.Replace(BO.Code.File.GetFileInfo(file).Extension, "");
                var strPrefix = strFileName.Substring(0, 2);
                var fields = InsertUpdate_getDateFields(strPrefix);

                foreach (string strField in fields)
                {
                    var sb = new System.Text.StringBuilder();
                    string s = strMatrika.Replace("{period_field}", strField);
                    var strViewName = $"{strFileName.Replace(".", "_")}_{strField.Split(".")[1]}";


                    sb.AppendLine($"if exists(select 1 from sysobjects where id = object_id('{strViewName}') and type = 'IF')");
                    sb.AppendLine($"drop function {strViewName}");
                    _db.RunSql2(sb.ToString());

                    sb = new System.Text.StringBuilder();
                    sb.AppendLine($"CREATE function [dbo].[{strViewName}] (@x01id int,@d1 datetime,@d2 datetime)");
                    sb.AppendLine(" returns table");
                    sb.AppendLine(" AS");
                    sb.AppendLine(" return (");
                    sb.AppendLine("");
                    sb.AppendLine(s);
                    sb.AppendLine("");
                    sb.AppendLine(")");

                    _db.RunSql2(sb.ToString());

                }



            }
        }

        private List<string> InsertUpdate_getDateFields(string prefix)
        {
            var lis = new List<string>();
            lis.Add("a.p31Date");
            switch (prefix)
            {
                case "p41":
                    lis.Add("p41.p41PlanFrom");
                    lis.Add("p41.p41PlanUntil");
                    lis.Add("p41.p41DateInsert");


                    break;
                case "p28":
                    lis.Add("p28.p28DateInsert");
                    lis.Add("p91.p91DateSupply");
                    lis.Add("p91.p91Date");
                    break;
                case "p56":
                    lis.Add("p28.p56DateInsert");
                    lis.Add("p56.p56PlanFrom");
                    lis.Add("p56.p56PlanUntil");
                    lis.Add("p91.p91DateSupply");
                    lis.Add("p91.p91Date");
                    break;
                case "j02":
                    lis.Add("j02.j02DateInsert");
                    lis.Add("p91.p91DateSupply");
                    lis.Add("p91.p91Date");
                    break;
                case "p91":
                    lis.Add("p91.p91DateInsert");
                    lis.Add("p91.p91DateSupply");
                    lis.Add("p91.p91DateMaturity");
                    lis.Add("p91.p91Date");
                    break;
            }

            return lis;
        }

        public string BackupDatabase(string strDbName, string strDestFolder = null)
        {
            string strBakFileName = $"{strDbName}_{(int)DateTime.Now.DayOfWeek}.bak";
            if (_mother.App.HostingMode == Singleton.HostingModeEnum.SharedApp || _mother.App.HostingMode == Singleton.HostingModeEnum.TotalCloud)
            {
                strDestFolder = "H:\\SQLServer\\MSSQL14.MARKTIME\\MSSQL\\Backup\\_temp";
            }
            else
            {
                if (strDestFolder == null)
                {
                    strDestFolder = _mother.UploadFolder + "\\db_backup";
                }
                if (!System.IO.Directory.Exists(strDestFolder))
                {
                    System.IO.Directory.CreateDirectory(strDestFolder);
                }
            }
            
            if (string.IsNullOrEmpty(strDestFolder))
            {
                return null;
            }
            _db.RunSql($"BACKUP DATABASE [{strDbName}] TO DISK = '{strDestFolder}\\{strBakFileName}' WITH INIT, SKIP");

            return strBakFileName;


        }
    }
}
