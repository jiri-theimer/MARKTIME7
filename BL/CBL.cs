

using System.Runtime.CompilerServices;

namespace BL
{
    public interface ICBL
    {
        public string DeleteRecord(string entity, int pid);
        public string LoadUserParam(string strKey, string strDefault = null, double maxhoursvalid = 0.00);
        public int LoadUserParamInt(string strKey, int intDefault = 0, double maxhoursvalid = 0.00);
        public bool LoadUserParamBool(string strKey, bool bolDefault);
        public DateTime? LoadUserParamDate(string strKey);
        public bool SetUserParam(string strKey, string strValue);
        public double LoadUserParamValidityHours(string strKey);
        public double LoadUserParamValidityMinutes(string strKey);
        
        public void ClearUserParamsCache();
        public int SaveRecordCode(string codevalue, string prefix, int pid);
        public string GetCurrentRecordCode(string prefix, int pid);
        public System.Data.DataTable GetList_Last10RecordCode(string prefix);
        public DateTime? GetCurrentRecordValidUntil(string prefix, int pid);
        public string GetObjectAlias(string prefix, int pid);
        public string GetObjectAliasType(string prefix, int pid);
        public void SaveLastCallingRecPid(string prefix, int pid, string caller, bool test_if_changed_pid, bool test_if_changedpid_hoursvalidity,string rez);  //uložit informaci o naposledy navštíveném záznamu
        public string MergeTextWithOneDate(string strExpr, DateTime d1, DateTime d2);   //vrátí text sloučený o zkratky [MM], [YYYY], [DD] apod.
        public BO.Result TestIfAllowCreateRecord(string recprefix, int rectype_pid);
        public int GetRecord_j02ID_Owner(string prefix, int pid);   //vrátí ID vlastníka záznamu
        public int GetRecord_j02ID_Creator(string prefix, int pid); //vrátí ID zakladatele záznamu
        public bool SaveGlobalParam(int o58id, int pid, string strValue);
        public string GetGlobalParamValue(int o58id, int pid);
        public string GetGlobalParamValue(string o58key, int pid);
        public IEnumerable<BO.GlobalParam> GetList_GlobalParam(string prefix, int pid);
        public bool Archive(string prefix, int pid);
        public bool Restore(string prefix, int pid);
        public bool UpdateRowColor(string prefix, List<int> pids, int colorindex);
        public bool ClearRowColor(string prefix, List<int> pids);
    }
    class CBL : BaseBL, ICBL
    {
        private IEnumerable<BO.StringPairTimestamp> _qry;
        public CBL(BL.Factory mother) : base(mother)
        {

        }

        private IEnumerable<BO.StringPairTimestamp> _userparams;
        
        public bool Archive(string prefix, int pid)
        {
            return _db.RunSql($"UPDATE {BO.Code.Entity.GetEntity(prefix)} SET {prefix}ValidUntil=DATEADD(MINUTE,-1,GETDATE()) WHERE {prefix}ID=@pid", new { pid = pid });
        }
        public bool Restore(string prefix, int pid)
        {
            return _db.RunSql($"UPDATE {BO.Code.Entity.GetEntity(prefix)} SET {prefix}ValidUntil=convert(datetime,'01.01.3000',104) WHERE {prefix}ID=@pid", new { pid = pid });
        }
        public string DeleteRecord(string entity, int pid)
        {
          
            var pars = new Dapper.DynamicParameters();
            pars.Add("j02id_sys", _db.CurrentUser.pid, System.Data.DbType.Int32);
            pars.Add("pid", pid, System.Data.DbType.Int32);
            pars.Add("err_ret", "", System.Data.DbType.String, System.Data.ParameterDirection.Output);
            string strSP = entity.Substring(0, 3) + "_delete";


            switch (entity)
            {
                case "":
                    break;

                default:
                    return _db.RunSp(strSP, ref pars);
            }

            return "";
        }
        public int GetRecord_j02ID_Owner(string prefix, int pid)
        {
            BO.GetInteger c = _db.Load<BO.GetInteger>($"select j02ID_Owner as Value FROM {BO.Code.Entity.GetEntity(prefix)} WHERE {prefix}ID=@pid", new { pid = pid });
            if (c != null)
            {
                return c.Value;
            }
            return 0;

        }
        public int GetRecord_j02ID_Creator(string prefix, int pid)
        {
            BO.GetString login = _db.Load<BO.GetString>($"select {prefix}UserInsert as Value FROM {BO.Code.Entity.GetEntity(prefix)} WHERE {prefix}ID=@pid", new { pid = pid });
            if (login != null)
            {
                var recJ02 = _mother.j02UserBL.LoadByLogin(login.Value,0,false);
                if (recJ02 != null)
                {
                    return recJ02.pid;
                }
                
            }
            return 0;

        }
        public string GetObjectAlias(string prefix, int pid)
        {
            //BO.GetString c = _db.Load<BO.GetString>("select dbo.get_object_alias(@prefix,@pid) as Value", new { prefix = BO.Code.Entity.GetPrefixDb(prefix), pid = pid });
            BO.GetString c = _db.Load<BO.GetString>("declare @s nvarchar(200) set @s=dbo.get_object_alias(@prefix,@pid); select @s as Value", new { prefix = BO.Code.Entity.GetPrefixDb(prefix), pid = pid });
            if (c != null)
            {
                return c.Value;
            }
            return null;

        }
        public string GetObjectAliasType(string prefix, int pid)
        {
            if (pid == 0 || prefix == null) return null;
            BO.GetString c = _db.Load<BO.GetString>("select dbo.get_object_alias_type(@prefix,@pid) as Value", new { prefix = prefix, pid = pid });
            if (c != null)
            {
                return c.Value;
            }
            return null;

        }

        public DateTime? GetCurrentRecordValidUntil(string prefix, int pid)
        {
            
            var ce = _mother.EProvider.ByPrefix(prefix);
            
            if (ce==null || ce.IsWithoutValidity)
            {
                return new DateTime(3000, 1, 1);
            }
            string strTab = BO.Code.Entity.GetEntity(prefix);
            BO.GetDate c = _db.Load<BO.GetDate>($"select {prefix}ValidUntil as Value FROM {strTab} WHERE {prefix}ID=@pid", new { pid = pid });
            return c.Value;
        }

        public string GetCurrentRecordCode(string prefix, int pid)
        {
            string strTab = BO.Code.Entity.GetEntity(prefix);

            BO.GetString c = _db.Load<BO.GetString>($"select {prefix}Code as Value FROM {strTab} WHERE {prefix}ID=@pid", new { pid = pid });
            return c.Value;
        }
        public int SaveRecordCode(string codevalue, string prefix, int pid)
        {
            codevalue = codevalue.Trim();
            if (string.IsNullOrEmpty(codevalue))
            {
                this.AddMessage("Kód záznamu nesmí být prázdný."); return 0;
            }
            string strTab = BO.Code.Entity.GetEntity(prefix);
            var dupl = _db.Load<BO.GetInteger>($"select {prefix}ID as Value FROM {strTab} WHERE {prefix}Code LIKE @code AND {prefix}ID<>@pid", new {code=codevalue,pid=pid});
            if (dupl !=null)
            {
                this.AddMessage("Kód záznamu musí být unikátní."); return 0;
            }

            if (_db.RunSql($"UPDATE {strTab} SET {prefix}Code=@code WHERE {prefix}ID=@pid", new { code = codevalue, pid = pid }))
            {
                return pid;
            }
            else
            {
                return 0;
            }
        }
        public System.Data.DataTable GetList_Last10RecordCode(string prefix)
        {
            string strTab = BO.Code.Entity.GetEntity(prefix);
            string s = null;
            switch (prefix)
            {
                case "j02":
                    s = $"SELECT TOP 10 a.j02Code, a.j02Name,a.j02Email,a.j02UserInsert,a.j02DateInsert FROM j02User a INNER JOIN j04UserRole b ON a.j04ID=b.j04ID INNER JOIN x67EntityRole c ON b.x67ID=c.x67ID WHERE c.x01ID={_mother.CurrentUser.x01ID} ORDER BY a.j02ID DESC"; break;
                case "p41":
                    s = $"select top 10 a.p41Code,a.p41Name,b.p42Name,a.p41UserInsert,a.p41DateInsert FROM p41Project a INNER JOIN p42ProjectType b ON a.p42ID=b.p42ID INNER JOIN p07ProjectLevel c ON a.p07ID=c.p07ID WHERE c.x01ID={_mother.CurrentUser.x01ID} ORDER BY a.p41ID DESC"; break;
                case "p28":
                    s = $"SELECT TOP 10 a.p28Code, a.p28Name,b.p29Name,p28UserInsert,p28DateInsert FROM p28Contact a INNER JOIN p29ContactType b ON a.p29ID=b.p29ID WHERE b.x01ID={_mother.CurrentUser.x01ID} ORDER BY a.p28ID DESC"; break;

                case "o23":
                    s = $"select top 10 o23Code,o23Name,o18Name,o23UserInsert,o23DateInsert FROM o23Doc a INNER JOIN o18DocType o18 ON a.o18ID=o18.o18ID WHERE a.o23IsDraft=0 AND o18.x01ID={_mother.CurrentUser.x01ID} ORDER BY a.o23ID DESC"; break;
                case "p91":
                    s = $"select top 10 p91Code,p91Client,p92Name,p91UserInsert,p91DateInsert FROM p91Invoice a INNER JOIN p92InvoiceType b ON a.p92ID=b.p92ID WHERE b.x01ID={_mother.CurrentUser.x01ID} AND a.p91IsDraft=0 ORDER BY p91ID DESC"; break;
                case "p90":
                    s = $"select top 10 p90Code,null,p89Name,p90UserInsert,p90DateInsert FROM p90Proforma a INNER JOIN p89ProformaType b ON a.p89ID=b.p89ID WHERE b.x01ID={_mother.CurrentUser.x01ID} AND a.p90IsDraft=0 ORDER BY p90ID DESC"; break;
                case "p82":
                    s = $"select top 10 p82Code,NULL,NULL,p82UserInsert,p82DateInsert FROM p82Proforma_Payment a INNER JOIN p90Proforma b ON a.p90ID=b.p90ID INNER JOIN p89ProformaType c ON b.p89ID=c.p89ID WHERE c.x01ID={_mother.CurrentUser.x01ID} ORDER BY p82ID DESC"; break;
                case "p56":
                    s = $"SELECT TOP 10 a.p56Code, a.p56Name,b.p57Name,p56UserInsert,p56DateInsert FROM p56Task a INNER JOIN p57TaskType b ON a.p57ID=b.p57ID WHERE b.x01ID={_mother.CurrentUser.x01ID} ORDER BY a.p56ID DESC"; break;

            }

            return _db.GetDataTable(s);

        }

        private void Handle_UpdateParamsCache()
        {
            
            
            if (_userparams == null)
            {
                _userparams = _db.GetList<BO.StringPairTimestamp>("SELECT x36Key as [Key],x36Value as [Value],x36DateUpdate as [DateUpdate] FROM x36UserParam WHERE j02ID=@j02id", new { j02id = _db.CurrentUser.pid });
            }
           
            
        }
        public double LoadUserParamValidityHours(string strKey)
        {
            Handle_UpdateParamsCache();
            if (_userparams.Any(p => p.Key == strKey))
            {
                DateTime d = _userparams.First(p => p.Key == strKey).DateUpdate;
                return (DateTime.Now - d).TotalHours;
            }
            return 0;
        }
        public double LoadUserParamValidityMinutes(string strKey)
        {
            Handle_UpdateParamsCache();
            if (_userparams.Any(p => p.Key == strKey))
            {
                DateTime d = _userparams.First(p => p.Key == strKey).DateUpdate;
                return (DateTime.Now - d).TotalMinutes;
            }
            return 0;
        }
        public string LoadUserParam(string strKey, string strDefault = null, double maxhoursvalid = 0.00 )
        {
            Handle_UpdateParamsCache();

         
            if (maxhoursvalid == 0.00)
            {
                _qry = _userparams.Where(p => p.Key == strKey);
            }
            else
            {
                _qry = _userparams.Where(p => p.Key == strKey && p.DateUpdate > DateTime.Now.AddHours(-1 * maxhoursvalid)); //hledat hodnotu maximálně starou maxhoursvalid hodin
            }

            if (_qry.Count() > 0)
            {
                return _qry.First().Value;
            }
            else
            {
                return strDefault;
            }


        }
        public int LoadUserParamInt(string strKey, int intDefault, double maxhoursvalid)
        {
            string s = LoadUserParam(strKey, null, maxhoursvalid);
            if (s == null)
            {
                return intDefault;
            }
            else
            {
                return BO.Code.Bas.InInt(s);
            }
        }
        public bool LoadUserParamBool(string strKey, bool bolDefault)
        {
            string s = LoadUserParam(strKey);
            if (s == null)
            {
                return bolDefault;
            }
            else
            {
                if (s == "1" || s == "true" || s == "True")
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }
        public DateTime? LoadUserParamDate(string strKey)
        {
            string s = LoadUserParam(strKey);
            if (String.IsNullOrEmpty(s))
            {
                return null;
            }
            else
            {
                try
                {
                    return BO.Code.Bas.String2Date(s);
                }
                catch
                {
                    return null;
                }
                
            }
        }
        public bool SetUserParam(string strKey, string strValue)
        {
            var pars = new Dapper.DynamicParameters();
            pars.Add("j02id", _db.CurrentUser.pid, System.Data.DbType.Int32);
            pars.Add("x36key", strKey, System.Data.DbType.String);
            pars.Add("x36value", strValue, System.Data.DbType.String);

            if (_db.RunSp("x36userparam_save", ref pars, false) == "1")
            {
                return true;
            }
            else
            {
                return false;
            }
        }        

        public void ClearUserParamsCache()
        {
            _userparams = null;
        }

        public void SaveLastCallingRecPid(string prefix, int pid, string caller,bool test_if_changed_pid,bool test_if_changedpid_hoursvalidity,string rez)
        {
            //uložit naposledy navštívený záznam
            if (pid == 0 || prefix == null) return;
            if (caller == "grid" || caller == "recpage")
            {
                if (test_if_changed_pid)    //testovat změnu hodnoty aktuálního a naposledy navštíveného záznamu
                {
                    int lastpid = LoadUserParamInt($"recpage-{prefix}-{rez}-pid", 0, 0);
                    if (pid == lastpid)
                    {
                        if (!test_if_changedpid_hoursvalidity)
                        {
                            return; //netestovat dobu od naposledy navštíveného záznamu
                        }
                        else
                        {
                            if (LoadUserParamValidityHours($"recpage-{prefix}-{rez}-pid") < 12)
                            {
                                return; //doba posledního navštívení záznamu pod 12 hodin
                            }
                        }
                    }
                    
                }
                SetUserParam($"recpage-{prefix}-{rez}-pid", pid.ToString());  //uložit info o návštěvě záznamu
               
            }

        }

        public string MergeTextWithOneDate(string strExpr,DateTime d1,DateTime d2)
        {
            if (string.IsNullOrEmpty(strExpr)) return strExpr;

            string s = _db.Load<BO.GetString>("SELECT dbo.get_parsed_text_with_period(@expr,@d,0) as Value",new { expr = strExpr, d = d1 }).Value;
            if (strExpr.Contains("1]"))
            {
                s=_db.Load<BO.GetString>("SELECT dbo.get_parsed_text_with_period(@expr,@d,1) as Value", new { expr = s, d = d1 }).Value;
            }
            if (strExpr.Contains("2]"))
            {
                s = _db.Load<BO.GetString>("SELECT dbo.get_parsed_text_with_period(@expr,@d,2) as Value", new { expr = s, d = d2 }).Value;
            }

            return s;
        }

        
        public BO.Result TestIfAllowCreateRecord(string recprefix,int rectype_pid)
        {
            if (string.IsNullOrEmpty(recprefix))
            {
                return new BO.Result(true, "recprefix is null");
            }
            string strTypePrefix = null;
            switch (recprefix)
            {
                case "p28":
                    if (_mother.CurrentUser.TestPermission(BO.PermValEnum.GR_p28_Creator))
                    {
                        return new BO.Result(false);
                    }
                    if (rectype_pid == 0)
                    {
                        return new BO.Result(true, "Na vstupu chybí typ kontaktu.");
                    }
                    strTypePrefix = "p29";
                    break;
                case "p41":
                    if (_mother.CurrentUser.TestPermission(BO.PermValEnum.GR_p41_Creator))
                    {
                        return new BO.Result(false);
                    }
                    if (rectype_pid == 0)
                    {
                        return new BO.Result(true, "Na vstupu chybí typ projektu.");
                    }
                    strTypePrefix = "p42";
                    break;
                case "o23":
                    if (_mother.CurrentUser.TestPermission(BO.PermValEnum.GR_o23_Creator))
                    {
                        return new BO.Result(false);
                    }
                    if (rectype_pid == 0)
                    {
                        return new BO.Result(true, "Na vstupu chybí typ dokumentu.");
                    }
                    strTypePrefix = "o18";
                    break;
                case "p90":
                    if (_mother.CurrentUser.TestPermission(BO.PermValEnum.GR_p90_Creator))
                    {
                        return new BO.Result(false);
                    }
                    if (rectype_pid == 0)
                    {
                        return new BO.Result(true, "Na vstupu chybí typ zálohy.");
                    }
                    strTypePrefix = "p89";
                    break;
            }

            

            string s = "SELECT j08ID as Value FROM j08CreatePermission WHERE j08RecordPid=@recpid AND j08RecordEntity=@recprefix AND (j08IsAllUsers=1 OR j02ID=@j02id OR j04ID=@j04id";
            if (_mother.CurrentUser.j11IDs != null)
            {
                s += $" OR j11ID IN ({_mother.CurrentUser.j11IDs})";
            }
            s += ")";

            var ret=_db.Load<BO.GetInteger>(s, new { recpid = rectype_pid,recprefix= strTypePrefix, j02id=_mother.CurrentUser.pid,j04id=_mother.CurrentUser.j04ID });
            if (ret !=null && ret.Value > 0)
            {
                return new BO.Result(false);
            }
            
            return new BO.Result(true,"Nemáte oprávnění pro založení záznamu tohoto typu.");

        }

        public bool SaveGlobalParam(int o58id,int pid,string strValue)
        {
            return _db.RunSql("exec dbo.o59_save @o58id,@record_pid,@val", new {o58id=o58id, record_pid = pid, val= strValue });
        }
        public string GetGlobalParamValue(int o58id, int pid)
        {
            BO.GetString val = _db.Load<BO.GetString>("SELECT o59Value as Value FROM o59GlobalParamBinding WHERE o58ID=@o58id AND o59RecordPid=@record_pid", new { o58id = o58id, record_pid = pid });
            return (val == null ? null : val.Value);
        }
        public string GetGlobalParamValue(string o58key, int pid)
        {
            BO.GetString val = _db.Load<BO.GetString>("SELECT a.o59Value as Value FROM o59GlobalParamBinding a INNER JOIN o58GlobalParam b ON a.o58ID=b.o58ID WHERE b.o58Key LIKE @klic AND o59RecordPid=@record_pid", new { klic = o58key, record_pid = pid });
            return (val == null ? null : val.Value);
        }

        public IEnumerable<BO.GlobalParam> GetList_GlobalParam(string prefix,int pid)
        {
            if (prefix==null && pid == 0)
            {
                return _db.GetList<BO.GlobalParam>("SELECT a.o58ID,a.o58Entity,a.o58Key,a.o58Name,b.o59RecordPid,b.o59Value FROM o58GlobalParam a LEFT OUTER JOIN o59GlobalParamBinding b ON a.o58ID=b.o58ID");
            }
        
            return _db.GetList<BO.GlobalParam>("SELECT a.o58ID,a.o58Entity,a.o58Key,a.o58Name,b.o59RecordPid,b.o59Value FROM o58GlobalParam a LEFT OUTER JOIN (select * from o59GlobalParamBinding WHERE o59RecordPid=@pid) b ON a.o58ID=b.o58ID WHERE a.o58Entity=@prefix ORDER BY a.o58Ordinary", new { prefix = prefix, pid = pid });
        }

        public bool UpdateRowColor(string prefix,List<int> pids,int colorindex)
        {
            if (prefix == "le5")
            {
                prefix = "p41";
            }
            var s = $"UPDATE {BO.Code.Entity.GetEntity(prefix)} SET {prefix}RowColorFlag={colorindex} WHERE {prefix}ID IN ({string.Join(",",pids)})";

            return _db.RunSql(s);
        }
        public bool ClearRowColor(string prefix, List<int> pids)
        {
            if (prefix == "le5")
            {
                prefix = "p41";
            }
            var s = $"UPDATE {BO.Code.Entity.GetEntity(prefix)} SET {prefix}RowColorFlag=NULL WHERE {prefix}ID IN ({string.Join(",", pids)})";

            return _db.RunSql(s);
        }
    }
}
