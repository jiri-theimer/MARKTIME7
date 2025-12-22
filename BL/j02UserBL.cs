

namespace BL
{
    public interface Ij02UserBL
    {
        public BO.j02User Load(int pid);
        public BO.Result TestUniqueLoginOverLicenses(string strLogin, int pid_exclude);
        public BO.j02User LoadByLogin(string strLogin, int pid_exclude,bool istestcloud);
        public BO.j02User LoadByEmail(string strEmail, int pid_exclude, bool istestcloud);
        public BO.j02User LoadByCode(string strCode, int pid_exclude);
        public BO.j02User LoadVirtualUser(int j02id);
        public BO.RunningUser LoadRunningUser(int j02id);
        public IEnumerable<BO.j02User> GetList(BO.myQueryJ02 mq);
        public int Save(BO.j02User rec, List<BO.FreeFieldInput> lisFFI);
        public bool ValidateBeforeSave(BO.j02User rec);
        public void UpdateCurrentUserPing(BO.j92PingLog c);
        public void Update_j02Ping_Timestamp(int j02id, DateTime d);
        public void RecoveryUserCache(int j02id);
        public void RecoveryUserCache_StopWatch(int j02id);
        public void ClearAllUsersCache();
        public void TruncateUserParams(int j02id);
        public BO.Result SaveNewPassword(int j02id, string strNewPassword, bool bolIsMustChangePassword);
        public BO.j02UserSum LoadSumRow(int pid);
        
        public bool IsChangedLastLoginUserAgent(int j02id);    //zda se změnil od posledního přihlášení j90ClientBrowser
        public bool UpdateSmsVerifyCode(int j02id, string strSmsCode);
        public bool ClearSmsVerifyCode(int j02id);
        public String LoadLastActivityByUser(int j02id);
        
        public string GetDefaultMenuLinks();
        public int LoadBitstreamFromUserCache(string prefix, int rectypepid);    //načte hodnotu bistream uživatele ze záznam s prefix  z tabulky j09UserExtensionCache
        public bool SaveBitStreamUserCache(string prefix, int bitstream, int rectypepid, int recpid);
        public void UpdateAccessFailedCount(int j02id, int pocet);
        public bool CreateVirtualRecord(int j02id); //vygenerovat virtuálního uživatele
        public void UpdateLastReadNews_Timestamp(int j02id);    //aktualizovat čas posledního shlédnutí novinek



    }
    class j02UserBL : BaseBL, Ij02UserBL
    {
        public j02UserBL(BL.Factory mother) : base(mother)
        {

        }


        private string GetSQL1(string strAppend = null, bool istestcloud = false)
        {
            sb("SELECT a.*,");
            sb(_db.GetSQL1_Ocas("j02"));            
            sb(",j04x.j04Name,j07.j07Name,j18.j18Name,c21.c21Name,c21.c21ScopeFlag,x67x.x01ID,a.j02Cache_p31Count");            
            sb(" FROM j02User a");
            sb(" INNER JOIN j04UserRole j04x ON a.j04ID = j04x.j04ID INNER JOIN x67EntityRole x67x ON j04x.x67ID=x67x.x67ID");
            sb(" LEFT OUTER JOIN j07PersonPosition j07 ON a.j07ID=j07.j07ID");
            sb(" LEFT OUTER JOIN j18CostUnit j18 ON a.j18ID=j18.j18ID LEFT OUTER JOIN c21FondCalendar c21 ON a.c21ID=c21.c21ID");
            
            if (istestcloud)
            {
                sb(this.AppendCloudQuery(strAppend, "x67x.x01ID"));
            }
            else
            {
                sb(strAppend);
            }
            
            return sbret();

            
        }
        public BO.j02User Load(int intPID)
        {
            return _db.Load<BO.j02User>(GetSQL1(" WHERE a.j02ID=@pid"), new { pid = intPID });
        }
        public BO.j02User LoadVirtualUser(int j02id)
        {
            return _db.Load<BO.j02User>(GetSQL1(" WHERE a.j02VirtualParentID=@pid"), new { pid = j02id });
        }
        

        public BO.Result TestUniqueLoginOverLicenses(string strLogin,int pid_exclude)
        {
            if (_db.Load<BO.GetInteger>("select j02ID as Value FROM j02User WHERE UPPER(j02Login)=UPPER(@login) AND j02ID<>@pid_exclude", new { login = strLogin, pid_exclude = pid_exclude }) != null)
            {
                //duplicitu loginu je třeba testovat napříč všemi licenci v db!!!
                
                return new BO.Result(true,$"V systému již existuje jiný uživatel s přihlašovacím jménem [{strLogin}].");
            }
          
            return new BO.Result(false);
        }
        public BO.j02User LoadByLogin(string strLogin, int pid_exclude, bool istestcloud)
        {
            strLogin = BO.Code.Bas.OcistiSQL_WildCards(strLogin);

            if (pid_exclude > 0)
            {
                return _db.Load<BO.j02User>(GetSQL1(" WHERE a.j02Login LIKE @login AND a.j02ID<>@pid_exclude",istestcloud), new { login = strLogin, pid_exclude = pid_exclude });
            }
            else
            {
                return _db.Load<BO.j02User>(GetSQL1(" WHERE a.j02Login LIKE @login",istestcloud), new { login = strLogin });
            }            
        }

        public BO.j02User LoadByEmail(string strEmail, int pid_exclude, bool istestcloud)
        {
            strEmail = BO.Code.Bas.OcistiSQL_WildCards(strEmail).Trim();
            return _db.Load<BO.j02User>(GetSQL1(" WHERE a.j02Email LIKE @email AND a.j02ID<>@pid_exclude",istestcloud), new { email = strEmail, pid_exclude = pid_exclude });
        }
        public BO.j02User LoadByCode(string strCode, int pid_exclude)
        {
            strCode = BO.Code.Bas.OcistiSQL_WildCards(strCode).Trim();
            return _db.Load<BO.j02User>(GetSQL1(" WHERE a.j02Code LIKE @code AND a.j02ID<>@pid_exclude", _mother.CurrentUser.IsHostingModeTotalCloud), new { code = strCode, pid_exclude = pid_exclude });
        }

        public BO.RunningUser LoadRunningUser(int j02id)
        {
            string strLogin = _db.Load<BO.GetString>("select j02Login as Value from j02user where j02ID=@j02id", new { j02id = j02id }).Value;
            return _db.Load<BO.RunningUser>("exec dbo.j02_load_sysuser @login", new { login = strLogin });
        }
        public IEnumerable<BO.j02User> GetList(BO.myQueryJ02 mq)
        {            
            DL.FinalSqlCommand fq = DL.basQuery.GetFinalSql(GetSQL1(), mq, _mother.CurrentUser);
            return _db.GetList<BO.j02User>(fq.FinalSql, fq.Parameters);
        }

        public BO.j02UserSum LoadSumRow(int pid)
        {
            return _db.Load<BO.j02UserSum>("EXEC dbo.j02_inhale_sumrow @j03id_sys,@pid", new { j03id_sys = _mother.CurrentUser.pid, pid = pid });
        }

        public string GetDefaultMenuLinks()
        {
            return "dashboard|p31calendar|p31dayline|approving|p31totals|absence|p41|p28|p91|p56|x31|j02|p31grid|admin";
        }
       
        
        public int Save(BO.j02User rec, List<BO.FreeFieldInput> lisFFI)
        {            
            if (!ValidateBeforeSave(rec))
            {
                return 0;
            }
            var p = new DL.Params4Dapper();
            p.AddInt("pid", rec.pid);
            p.AddInt("j07ID", rec.j07ID,true);
            p.AddInt("j04ID", rec.j04ID,true);
            
            p.AddInt("j18ID", rec.j18ID, true);
            p.AddInt("c21ID", rec.c21ID, true);
            p.AddInt("p72ID_NonBillable", rec.p72ID_NonBillable, true);
            
            
            p.AddString("j02Login", rec.j02Login);
            p.AddString("j02Email", rec.j02Email);
            p.AddString("j02CountryCode", rec.j02CountryCode);

            p.AddString("j02FirstName", rec.j02FirstName);
            p.AddString("j02LastName", rec.j02LastName);
            p.AddString("j02TitleBeforeName", rec.j02TitleBeforeName);
            p.AddString("j02TitleAfterName", rec.j02TitleAfterName);
            p.AddString("j02EmailSignature", rec.j02EmailSignature);
            p.AddString("j02InvoiceSignatureText", rec.j02InvoiceSignatureText);
            p.AddString("j02InvoiceSignatureFile", rec.j02InvoiceSignatureFile);
            p.AddBool("j02IsMustChangePassword", rec.j02IsMustChangePassword);
            p.AddBool("j02IsLoginManualLocked", rec.j02IsLoginManualLocked);
            p.AddEnumInt("j02TwoFactorVerifyFlag", rec.j02TwoFactorVerifyFlag);
            p.AddEnumInt("j02WorksheetOperFlag", rec.j02WorksheetOperFlag);

            p.AddEnumInt("j02Ping_DeviceTypeFlag", rec.j02Ping_DeviceTypeFlag);
            p.AddInt("j02Ping_InnerWidth", rec.j02Ping_InnerWidth);
            p.AddInt("j02Ping_InnerHeight", rec.j02Ping_InnerHeight);
            p.AddInt("j02LangIndex", rec.j02LangIndex);
            p.AddInt("j02FontSizeFlag", rec.j02FontSizeFlag);
            p.AddBool("j02IsDebugLog", rec.j02IsDebugLog);
            
            p.AddDateTime("j02LiveChatTimestamp", rec.j02LiveChatTimestamp);
            p.AddDateTime("j02HiddenSessionTimestamp", rec.j02HiddenSessionTimestamp);
            p.AddString("j02HomePageUrl",rec.j02HomePageUrl);
            p.AddEnumInt("j02ModalWindowsFlag", rec.j02ModalWindowsFlag);
            p.AddInt("j02NotifySubscriberFlag", rec.j02NotifySubscriberFlag);

            p.AddString("j02Mobile", rec.j02Mobile);
            p.AddString("j02Code", rec.j02Code);

            p.AddInt("j02TimesheetEntryDaysBackLimit", rec.j02TimesheetEntryDaysBackLimit);
            p.AddString("j02TimesheetEntryDaysBackLimit_p34IDs", rec.j02TimesheetEntryDaysBackLimit_p34IDs);

            p.AddInt("j02BitStream", rec.j02BitStream);
        
            p.AddInt("j02HesBitStream", rec.j02HesBitStream);
            p.AddString("j02DefaultHoursFormat", rec.j02DefaultHoursFormat);
            p.AddString("j02SkinBackColor", rec.j02SkinBackColor);
            p.AddString("j02SkinForeColor", rec.j02SkinForeColor);
            p.AddString("j02SkinSelColor", rec.j02SkinSelColor);
            p.AddInt("j02PerformanceFlag", rec.j02PerformanceFlag);
            p.AddInt("j02AutocompleteFlag", rec.j02AutocompleteFlag);
            p.AddInt("j02MySearchBitStream", rec.j02MySearchBitStream);
            p.AddDouble("j02Plan_Internal_Rate", rec.j02Plan_Internal_Rate);

            if (rec.pid == 0 && rec.j02GridCssBitStream == 0) rec.j02GridCssBitStream = 50; //výchozí nastavení zebry
            p.AddInt("j02GridCssBitStream", rec.j02GridCssBitStream);

            p.AddInt("j02UIBitStream", rec.j02UIBitStream);
           
            if (rec.pid==0 && string.IsNullOrEmpty(rec.j02MyMenuLinks))
            {
                rec.j02MyMenuLinks = this.GetDefaultMenuLinks();    //nový uživatel
            }

            p.AddString("j02MyMenuLinks", rec.j02MyMenuLinks);

            var intPID = _db.SaveRecord("j02User", p, rec);
            if (intPID > 0)
            {
                DL.BAS.SaveFreeFields(_db, intPID, lisFFI);

                _db.RunSql("exec dbo.j02_aftersave @j02id,@j02id_sys", new { j02id = intPID, j02id_sys = _mother.CurrentUser.pid });


                RecoveryUserCache(intPID);
            }
            return intPID;
        }

        public BO.Result SaveNewPassword(int j02id,string strNewPassword,bool bolIsMustChangePassword)
        {
            var rec = Load(j02id);
            if (rec == null)
            {
                return new BO.Result(true,"User not found.");
            }
            string strHash = new BL.Code.PasswordSupport().GetPasswordHash(strNewPassword, rec.j02Login, rec.pid);
            if (_db.RunSql("UPDATE j02User set j02PasswordHash=@s,j02PasswordLastChange=GETDATE(),j02SmsVerifyCode=null,j02SmsVerifyCreate=null WHERE j02ID=@pid", new { s = strHash,pid=rec.pid }))
            {
                if (bolIsMustChangePassword)
                {
                    _db.RunSql("UPDATE j02User SET j02IsMustChangePassword=1 WHERE j02ID=@pid", new {pid=rec.pid});
                }
            }
            
            return new BO.Result(false);
        }

        public bool ValidateBeforeSave(BO.j02User rec)
        {
            if (string.IsNullOrEmpty(rec.j02Email))
            {
                this.AddMessage("Chybí vyplnit [E-mail]."); return false;
            }
            else
            {
                if (!BO.Code.Bas.IsValidEmail(rec.j02Email))
                {
                    this.AddMessage("Neplatná e-mail adresa."); return false;

                }
                

            }
            if (string.IsNullOrEmpty(rec.j02Login))
            {
                this.AddMessage("Chybí vyplnit [Login]."); return false;
            }
            if (string.IsNullOrEmpty(rec.j02FirstName))
            {
                this.AddMessage("Chybí vyplnit [Jméno]."); return false;
            }
            if (string.IsNullOrEmpty(rec.j02LastName))
            {
                this.AddMessage("Chybí vyplnit [Příjmení]."); return false;
            }
            if (rec.j04ID==0)
            {
                this.AddMessage("Chybí vyplnit [Aplikační role]."); return false;
            }
            if (rec.j02Login.Contains(" ") || rec.j02Login.Contains("%"))
            {
                this.AddMessage("Přihlašovací jméno nesmí obsahovat znak [mezera] nebo [procento]."); return false;
            }
            var arr = rec.j02Login.Split("@");
            if (_mother.Lic.x01LoginDomain != null)
            {
                if (arr.Count() != 2 || arr[1] != _mother.Lic.x01LoginDomain || string.IsNullOrEmpty(arr[0]))
                {
                    this.AddMessage("Přihlašovací jméno musí obsahovat doménu:" + " " + _mother.Lic.x01LoginDomain); return false;
                }
            }
            

            if (LoadByEmail(rec.j02Email, rec.pid, _mother.CurrentUser.IsHostingModeTotalCloud) != null)
            {
                this.AddMessageTranslated(string.Format(_mother.tra("E-mail adresa [{0}] již je obsazena jiným uživatelem."), rec.j02Email));
                return false;
            }
            if (!string.IsNullOrEmpty(rec.j02Code))
            {
                rec.j02Code = rec.j02Code.Trim();
                if (LoadByCode(rec.j02Code, rec.pid) != null)
                {
                    this.AddMessageTranslated(string.Format(_mother.tra("Kód [{0}] již je obsazen jiným uživatelem."), rec.j02Code));
                    return false;
                }                
            }
            

            var test = TestUniqueLoginOverLicenses(rec.j02Login, rec.pid);
            if (test.Flag==BO.ResultEnum.Failed)
            {
                this.AddMessageTranslated(test.Message);
                return false;
            }

            

            return true;
        }

        public bool CreateVirtualRecord(int j02id)
        {
            return _db.RunSql("exec dbo.j02_create_virtualuser @j02id_sys,@pid,null", new { j02id_sys = _mother.CurrentUser.pid, pid = j02id });

            
        }

        public void ClearAllUsersCache()
        {
            _db.RunSql("update j02User set j02Cache_TimeStamp=null WHERE j04ID IN (select a.j04ID FROM j04UserRole a INNER JOIN x67EntityRole b ON a.x67ID=b.x67ID WHERE b.x01ID=@x01id)", new {x01id=_mother.CurrentUser.x01ID});
            
        }
        public void RecoveryUserCache(int j02id)
        {
           
            _db.RunSql("exec dbo.j02_recovery_cache @j02id", new { j02id = j02id});
        }
        public void RecoveryUserCache_StopWatch(int j02id)
        {
            _db.RunSql("exec dbo.j02_recovery_cache_stopwatch @j02id", new { j02id = j02id });
        }

        public void UpdateLastReadNews_Timestamp(int j02id)
        {
            _db.RunSql("update j02User set j02LastReadNews_Timestamp=GETDATE() WHERE j02ID=@j02id", new { j02id = j02id });
        }
        public void UpdateAccessFailedCount(int j02id,int pocet)
        {
            _db.RunSql("update j02User set j02AccessFailedCount=@pocet,j02AutoLockedWhen=null,j02IsLoginAutoLocked=0 WHERE j02ID=@j02id", new { j02id = j02id, pocet = pocet });
            if (pocet >= 5)
            {
                _db.RunSql("update j02User set j02IsLoginAutoLocked=1,j02AutoLockedWhen=GETDATE() WHERE j02ID=@j02id", new { j02id = j02id });
            }
        }

        public void Update_j02Ping_Timestamp(int j02id,DateTime d)
        {
            _db.RunSql("UPDATE j02User set j02Ping_Timestamp=@d WHERE j02ID=@pid", new { pid = _mother.CurrentUser.pid,d=d });    //explicitně aktualizovat čas j02Ping_Timestamp
        }
        public void UpdateCurrentUserPing(BO.j92PingLog c) //zápis pravidelně po 2 minutách do PING logu
        {
            
            string s = "INSERT INTO j92PingLog(j02ID,j92Date,j92BrowserUserAgent,j92BrowserFamily,j92BrowserOS,j92BrowserDeviceType,j92BrowserDeviceFamily,j92BrowserAvailWidth,j92BrowserAvailHeight,j92BrowserInnerWidth,j92BrowserInnerHeight,j92RequestUrl)";
            s += " VALUES(@j02id,GETDATE(),@useragent,@browser,@os,@devicetype,@devicefamily,@aw,@ah,@iw,@ih,@requesturl)";
            _db.RunSql(s, new { j02id = _mother.CurrentUser.pid, useragent = c.j92BrowserUserAgent, browser = c.j92BrowserFamily, os = c.j92BrowserOS, devicetype = c.j92BrowserDeviceType, devicefamily = c.j92BrowserDeviceFamily, aw = c.j92BrowserAvailWidth, ah = c.j92BrowserAvailHeight, iw = c.j92BrowserInnerWidth, ih = c.j92BrowserInnerHeight, requesturl=c.j92RequestURL });


            _db.RunSql("UPDATE j02User set j02Ping_Timestamp=GETDATE(),j02Ping_InnerWidth=@w,j02Ping_InnerHeight=@h,j02Ping_DeviceTypeFlag=@devicetypeflag WHERE j02ID=@pid", new { pid = _mother.CurrentUser.pid, w=c.j92BrowserAvailWidth, h=c.j92BrowserAvailHeight,devicetypeflag=(c.j92BrowserDeviceType=="Phone" ? 2 : 1) });    //ping aktualizace
            var xx=c.j92BrowserDeviceType=="Phone" ? 2: 0;


            if (_mother.CurrentUser.j02LiveChatTimestamp != null)   //hlídat, aby se automaticky vypnul live-chat box po 20ti minutách
            {
                if (_mother.CurrentUser.j02LiveChatTimestamp.Value.AddMinutes(20) < DateTime.Now)
                {                    
                    var rec = Load(_mother.CurrentUser.pid);
                    rec.j02LiveChatTimestamp = null;   //vypnout smartsupp
                    Save(rec,null);
                }
            }

        }

       
        public void TruncateUserParams(int j02id)
        {
            if (j02id <= 0) j02id = _mother.CurrentUser.pid;
            _db.RunSql("DELETE FROM x36UserParam WHERE j02ID=@j02id", new { j02id = j02id });
        }


        public bool IsChangedLastLoginUserAgent(int j02id)    //zda se změnil od posledního přihlášení j90ClientBrowser
        {
            return _db.Load<BO.GetBool>("select dbo.j02_ischanged_useragent(@pid) as Value", new { pid = j02id }).Value;

        }
        public bool UpdateSmsVerifyCode(int j02id, string strSmsCode)
        {
            return _db.RunSql("UPDATE j02User set j02SmsVerifyCode=@s,j02SmsVerifyCreate=GETDATE() WHERE j02ID=@pid", new { s = strSmsCode, pid = j02id });
        }
        public bool ClearSmsVerifyCode(int j02id)
        {
            return _db.RunSql("UPDATE j02User set j02SmsVerifyCode=null,j02SmsVerifyCreate=null WHERE j02ID=@pid", new { pid = j02id });
        }

        public String LoadLastActivityByUser(int j02id)
        {
            string s = "select top 1 '<mark>'+dbo.GetDDMMYYYYHHMM(a.p31DateInsert)+'</mark><var>'+p32.p32Name+'</var>' as Value FROM p31Worksheet a INNER JOIN p32Activity p32 ON a.p32ID=p32.p32ID WHERE a.j02ID=@j02id ORDER BY a.p31ID DESC";
            BO.GetString c = _db.Load<BO.GetString>(s, new { j02id = j02id });
            if (c == null)
            {
                return null;
            }
            return c.Value;
        }

        public int LoadBitstreamFromUserCache(string prefix,int rectypepid)
        {
            BO.GetInteger c = _db.Load<BO.GetInteger>("select j09Bitstream as Value FROM j09UserExtensionCache WHERE j02ID=@j02id AND j09Entity=@prefix AND j09RecordTypePid=@rectypepid", new { j02id = _mother.CurrentUser.pid, prefix = prefix, rectypepid = rectypepid });
            
            return (c == null ? 0 : c.Value);
        }

        public bool SaveBitStreamUserCache(string prefix,int bitstream,int rectypepid,int recpid)
        {
            return _db.RunSql("EXEC dbo.j09_insert_update @j02id,@prefix,@rectypepid,@recpid,@bitstream", new { j02id = _mother.CurrentUser.pid, prefix = prefix, rectypepid= rectypepid,recpid=recpid, bitstream= bitstream });
        }
        
    }
}
