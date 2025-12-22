

namespace BL
{
    public class Factory
    {
        public DL.DbHandler db { get; set; }
        public BO.RunningUser CurrentUser { get; set; }     //přihlášený uživatel
        
        public IEnumerable<BO.p07ProjectLevel> lisP07 { get; set; } //projektové úrovně přihlášeného uživatele
        public Singleton.RunningApp App { get; set; }        
        public Singleton.TheEntitiesProvider EProvider { get; set; }        
        public Singleton.TheTranslator Translator { get; set; }

        private BO.x01License _lic;
        public BO.x01License Lic
        {
            get
            {
                if (_lic is null)
                {                    
                    if (this.App.HostingMode == BL.Singleton.HostingModeEnum.SharedApp)
                    {
                        
                       
                        _lic = this.App.lisX01.First(p => p.x01LoginDomain == this.CurrentUser.x01LoginDomain);
                    }
                    else
                    {
                        if (this.App.HostingMode == BL.Singleton.HostingModeEnum.TotalCloud)
                        {
                            _lic = this.App.lisX01.First(p => p.pid == this.CurrentUser.x01ID);
                        }
                        else
                        {
                            _lic = this.App.lisX01.First();
                        }
                        
                    }
                }
                return _lic;
            }            
        }
        public void SetLic(BO.x01License recX01)
        {
            _lic = recX01;
        }
        private int _p07LevelsCount { get; set; }
        

        private ICBL _cbl;
        private IFBL _fbl;
        
        private Ij72TheGridTemplateBL _j72;
        private IDataGridBL _grid;
           
        private Ij02UserBL _j02;
        private Ij04UserRoleBL _j04;
        private Ij05MasterSlaveBL _j05;
        
        private Ij06UserHistoryBL _j06;
        private Ij07PersonPositionBL _j07;

        private Ip28ContactBL _p28;
        private Ip29ContactTypeBL _p29;
        private Ip24ContactGroupBL _p24;
        private Ij11TeamBL _j11;
        
        private Ij18CostUnitBL _j18;
        private Ij25ReportCategoryBL _j25;
        private Ij28BarcodeBL _j28;
        private Ij40MailAccountBL _j40;
        private o42ImapRuleBL _o42;
        private Ij61TextTemplateBL _j61;
        private Io15AutoCompleteBL _o15;
        
        private Io23DocBL _o23;
        private Io27AttachmentBL _o27;
        
        private Io51TagBL _o51;
        private Io53TagGroupBL _o53;
        
        
        private Ip07ProjectLevelBL _p07;
        private Ip53VatRateBL _p53;
        private Ip54OvertimeLevelBL _p54;

        private Ip31WorksheetBL _p31;
        private Ip32ActivityBL _p32;
        private Ip34ActivityGroupBL _p34;
        private Ip36LockPeriodBL _p36;
        private Ip38ActivityTagBL _p38;
        private Ip40WorkSheet_RecurrenceBL _p40;
        private Ip41ProjectBL _p41;
        private Ip42ProjectTypeBL _p42;
        private Ip44ProjectTemplateBL _p44;
        private Ip15LocationBL _p15;
        private Ip51PriceListBL _p51;
        private Ip56TaskBL _p56;
        private Ip57TaskTypeBL _p57;
        private Ip58TaskRecurrenceBL _p58;
        private Ip75InvoiceRecurrenceBL _p75;
        private Ip60TaskTemplateBL _p60;
        private Io21MilestoneTypeBL _o21;
        private Io22MilestoneBL _o22;
        private Io24ReminderBL _o24;
        private Ip11AttendanceBL _p11;
        private Ip12ApproveUserDayBL _p12;

        private Ip61ActivityClusterBL _p61;
        private Ip63OverheadBL _p63;
        private Ip35UnitBL _p35;
        private Ix07IntegrationBL _x07;


        private Ic21FondCalendarBL _c21;
        private Ic26HolidayBL _c26;
        private Ic24DayColorBL _c24;

        private Ip85TempboxBL _p85;

        private Ip91InvoiceBL _p91;
        private Ip90ProformaBL _p90;
        private Ip82Proforma_PaymentBL _p82;

        private Ip95InvoiceRowBL _p95;
        private Ip83UpominkaTypeBL _p83;
        private Ip84UpominkaBL _p84;
        private Ip86BankAccountBL _p86;
        private Ip92InvoiceTypeBL _p92;        
        private Ip93InvoiceHeaderBL _p93;
        private Ip98Invoice_Round_Setting_TemplateBL _p98;
        private Ip80InvoiceAmountStructureBL _p80;
        private Ip89ProformaTypeBL _p89;

        private Im62ExchangeRateBL _m62;

        private Ib20HlidacBL _b20;
        private Ip68StopWatchBL _p68;

        private Ib01WorkflowTemplateBL _b01;
        private Ib02WorkflowStatusBL _b02;
        private Ib06WorkflowStepBL _b06;

        private Ia55RecPageBL _a55;
        private Ia59RecPageLayerBL _a59;
        private Ia58RecPageBoxBL _a58;

        private Ip49FinancialPlanBL _p49;
        private Ir01CapacityBL _r01;
        private Ir02CapacityVersionBL _r02;

        private Io18DocTypeBL _o18;
        private Io17DocMenuBL _o17;
        private Ix51HelpCoreBL _x51;
        private Ix52BlogBL _x52;
        private Ix54WidgetGroupBL _x54;
        private Ix55WidgetBL _x55;
        private Ix67EntityRoleBL _x67;

        private Ix01LicenseBL _x01;
        private Ix04NotepadConfigBL _x04;
        private Ix31ReportBL _x31;
        private Ix38CodeLogicBL _x38;
        private Ix97TranslateBL _x97;
        private IMailBL _mail;
        private Io43InboxBL _o43;

        private Ij79TotalsTemplateBL _j79;

        private Ix27EntityFieldGroupBL _x27;
        private Ix28EntityFieldBL _x28;
        private IWorkflowBL _workflow;
        public Factory(BO.RunningUser ru,Singleton.RunningApp runningapp,Singleton.TheEntitiesProvider ep,Singleton.TheTranslator tt)
        {           
            this.CurrentUser = ru;           
            this.App = runningapp;
            this.EProvider = ep;            
            this.Translator = tt;

            this.db = GetDbHandler(ru.j02Login);
            if (this.db != null)
            {
                this.db.CurrentUser = ru;
            }
            
            
            if (ru.pid == 0 && !string.IsNullOrEmpty(ru.j02Login))
            {
                InhaleUserByLogin(ru.j02Login);
                
            }
            
        }
       
        private DL.DbHandler GetDbHandler(string login)
        {                        
            if (this.App.HostingMode==BL.Singleton.HostingModeEnum.SharedApp)            
            {
                if (login == null) return null;
                //sdílené prostředí jedné app = jedna IIS aplikace obsluhuje X aplikačních databází -> dynamický connectstring podle vzoru: ConnectStringCloudTemplate
                //v cloud provozu je název databáze povinně za zavináčem @
                //v cloud provozu začíná název databáze vždy na výraz 'a7'
                //je třeba vyhodit tečku, protože login je většinou e-mail uživatele!
                //string strConnString = this.App.ConnectString.Replace("app_dbname", "a7" + login.Split("@")[1].Split(".")[0]);    //v loginu z části za zavináčem lze odvodit název databáze

               
                return new DL.DbHandler(this.App.ParseConnectStringFromLogin(login), this.CurrentUser, this.App.LogFolder);
            }
            else
            {
                //jedna IIS aplikace obsluhuje jednu aplikačních databází - napevno connect string z appSettings
                return new DL.DbHandler(this.App.ConnectString, this.CurrentUser, this.App.LogFolder);
            }

            
        }

        
       

        public void InhaleUserByLogin(string strLogin)
        {
            if (this.db==null) this.db = GetDbHandler(strLogin);
            CurrentUser = this.db.Load<BO.RunningUser>("exec dbo.j02_load_sysuser @login", new { login = strLogin });
            
            
            if (this.CurrentUser != null)
            {
                db.CurrentUser = this.CurrentUser;
                switch (App.HostingMode)
                {
                    case Singleton.HostingModeEnum.SharedApp:
                        lisP07 = this.App.lisP07.Where(p => p.x01LoginDomain == this.CurrentUser.x01LoginDomain);
                        break;
                    case Singleton.HostingModeEnum.None:
                        lisP07 = this.App.lisP07.Where(p => p.x01ID == this.CurrentUser.x01ID);
                        break;
                    case Singleton.HostingModeEnum.TotalCloud:
                        CurrentUser.IsHostingModeTotalCloud = true;
                        lisP07 = this.App.lisP07.Where(p => p.x01ID == this.CurrentUser.x01ID);
                        break;

                }
                
                if (CurrentUser.IsMobileDisplay())  //mobilní rozhraní
                {
                    CurrentUser.j02GridCssBitStream = 564;   //napevno grid nastavení
                }
                
            }            
        }

        public IEnumerable<BO.o17DocMenu> GetListO17()
        {
            if (App.HostingMode == BL.Singleton.HostingModeEnum.SharedApp)
            {
                return this.App.lisO17.Where(p => p.x01LoginDomain == this.CurrentUser.x01LoginDomain);
            }
            else
            {

                return this.App.lisO17.Where(p => p.x01ID == this.CurrentUser.x01ID);
            }
        }

        public void InhaleUserByEmail(string strEmail)      //využití v ověření podle GOOGLE nebo Office365
        {
            //z e-mail adresy se nepozná cloud databáze!
            this.CurrentUser = null;
            if (this.App.HostingMode !=BL.Singleton.HostingModeEnum.None)
            {
                var ret = GetDbHandler(strEmail).Load<BO.GetString>("select j02Login as Value FROM j02User where j02Email like @email", new { email = strEmail });
                if (ret != null)
                {
                    InhaleUserByLogin(ret.Value);
                }

            }
        }

        public int p07LevelsCount   //vrací počet otevřených projektových úrovní - minimum=1
        { get
            {
                if (_p07LevelsCount == 0)
                {
                    _p07LevelsCount = this.lisP07.Where(p => !p.isclosed).Count();
                }
                return _p07LevelsCount;
            }
        }

        public string getP07Level(int levelindex, bool singular)    //vrací název otevřené projektové úrovně na úrovni levelindex
        {
            if (levelindex == 0)
            {
                return "Projekt";
            }
            var lis = this.lisP07.Where(p => p.p07Level == levelindex && !p.isclosed);
            if (lis.Count() == 0) return null;

            if (singular)
            {
                return lis.First().p07Name;

            }
            else
            {
                return lis.First().p07NamePlural;
            }

        }

        public string getP07Level_Inflection(int levelindex)
        {
            var lis = this.lisP07.Where(p => p.p07Level == levelindex && !p.isclosed);
            if (lis.Count() == 0) return null;

            return lis.First().p07NameInflection;
        }

        public string TempFolder
        {
            get
            {
                return $"{this.UploadFolder}\\TEMP";
            }
        }

        public string ReportFolder
        {
            get
            {
                return $"{this.UploadFolder}\\X31";
            }
        }
        public string WwwUsersFolder
        {
            get
            {
                //return $"{this.App.WwwRootFolder}\\_users\\{this.Lic.x01Guid}";
                return $"{this.App.RootUploadFolder}\\_users\\{this.Lic.x01Guid}";
            }
        }
        public string NotepadFolder
        {
            get
            {
                //return $"{this.App.WwwRootFolder}\\_users\\{this.Lic.x01Guid}\\NOTEPAD";

                return $"{this.App.RootUploadFolder}\\_users\\{this.Lic.x01Guid}\\NOTEPAD";
            }
        }
        public string PluginsFolder
        {
            get
            {
                return $"{this.App.RootUploadFolder}\\_users\\{this.Lic.x01Guid}\\PLUGINS";
                
            }
        }

        public string UploadFolder
        {
            get
            {
                if (this.Lic.x01LoginDomain != null)
                {
                    return $"{this.App.RootUploadFolder}\\{this.Lic.x01LoginDomain}";
                }
                else
                {
                    return this.App.RootUploadFolder;
                }
                
                
            }
        }

        

        

        //logování přihlášení musí být zde, protože se logují i neńsspěšné pokusy
        public void Write2AccessLog(BO.j90LoginAccessLog c) //zápis úspěšných přihlášení i neúspěšných pokusů o přihlášení
        {                                             
            string s = "INSERT INTO j90LoginAccessLog(j02ID,j90Date,j90ClientBrowser,j90BrowserFamily,j90Platform,j90BrowserDeviceType,j90BrowserDeviceFamily,j90ScreenPixelsWidth,j90ScreenPixelsHeight,j90BrowserInnerWidth,j90BrowserInnerHeight,j90LoginMessage,j90LoginName,j90CookieExpiresInHours,j90AppClient,j90UserHostAddress,x01ID)";
            s += " VALUES(@j02id,GETDATE(),@useragent,@browser,@os,@devicetype,@devicefamily,@aw,@ah,@iw,@ih,@mes,@loginname,@cookieexpire,'7.0',@host,@x01id)";

            var db = GetDbHandler(c.j90LoginName);
            if (db != null)
            {
                db.RunSql(s, new { j02id = BO.Code.Bas.TestIntAsDbKey(c.j02ID), useragent = c.j90ClientBrowser, browser = c.j90BrowserFamily, os = c.j90Platform, devicetype = c.j90BrowserDeviceType, devicefamily = c.j90BrowserDeviceFamily, aw = c.j90ScreenPixelsWidth, ah = c.j90ScreenPixelsHeight, iw = c.j90BrowserInnerWidth, ih = c.j90BrowserInnerHeight, mes = c.j90LoginMessage, loginname = c.j90LoginName, cookieexpire = c.j90CookieExpiresInHours, host = c.j90UserHostAddress, x01id = c.x01ID });
            }
            
        }
        
        

        public string tra(string strExpression)   //lokalizace do ostatních jazyků
        {
            if (this.CurrentUser.j02LangIndex == 0) return strExpression;
            return this.Translator.DoTranslate(strExpression, this.CurrentUser.j02LangIndex);
        }

        public string trawi(string strExpression,int langindex)   //lokalizace do ostatních jazyků
        {
            if (langindex == 0) return strExpression;
            return this.Translator.DoTranslate(strExpression, langindex);
        }
        public string GetFirstNotifyMessage()
        {
            if (CurrentUser.Messages4Notify == null) return null;
            return CurrentUser.Messages4Notify[0].Value;
        }
        
        public ICBL CBL
        {
            get
            {
                if (_cbl == null) _cbl = new CBL(this);
                return _cbl;
            }
        }
        public IFBL FBL
        {
            get
            {
                if (_fbl == null) _fbl = new FBL(this);
                return _fbl;
            }
        }
        
        public Ij72TheGridTemplateBL j72TheGridTemplateBL
        {
            get
            {
                if (_j72 == null) _j72 = new j72TheGridTemplateBL(this);
                return _j72;
            }
        }
        public IDataGridBL gridBL
        {
            get
            {
                if (_grid == null) _grid = new DataGridBL(this);
                return _grid;
            }
        }

        public Ij79TotalsTemplateBL j79TotalsTemplateBL
        {
            get
            {
                if (_j79 == null) _j79 = new j79TotalsTemplateBL(this);
                return _j79;
            }
        }

        public Ij02UserBL j02UserBL
        {
            get
            {
                if (_j02 == null) _j02 = new j02UserBL(this);
                return _j02;
            }
        }
        
        public Ij04UserRoleBL j04UserRoleBL
        {
            get
            {
                if (_j04 == null) _j04 = new j04UserRoleBL(this);
                return _j04;
            }
        }
        public Ij05MasterSlaveBL j05MasterSlaveBL
        {
            get
            {
                if (_j05 == null) _j05 = new j05MasterSlaveBL(this);
                return _j05;
            }
        }
      
        public Ij06UserHistoryBL j06UserHistoryBL
        {
            get
            {
                if (_j06 == null) _j06 = new j06UserHistoryBL(this);
                return _j06;
            }
        }
        public Ij07PersonPositionBL j07PersonPositionBL
        {
            get
            {
                if (_j07 == null) _j07 = new j07PersonPositionBL(this);
                return _j07;
            }
        }

        public Ip28ContactBL p28ContactBL
        {
            get
            {
                if (_p28 == null) _p28 = new p28ContactBL(this);
                return _p28;
            }
        }
        public Ip29ContactTypeBL p29ContactTypeBL
        {
            get
            {
                if (_p29 == null) _p29 = new p29ContactTypeBL(this);
                return _p29;
            }
        }
        public Ip24ContactGroupBL p24ContactGroupBL
        {
            get
            {
                if (_p24 == null) _p24 = new p24ContactGroupBL(this);
                return _p24;
            }
        }
        public Ij11TeamBL j11TeamBL
        {
            get
            {
                if (_j11 == null) _j11 = new j11TeamBL(this);
                return _j11;
            }
        }
        
        public Ij18CostUnitBL j18CostUnitBL
        {
            get
            {
                if (_j18 == null) _j18 = new j18CostUnitBL(this);
                return _j18;
            }
        }
        public Ij25ReportCategoryBL j25ReportCategoryBL
        {
            get
            {
                if (_j25 == null) _j25 = new j25ReportCategoryBL(this);
                return _j25;
            }
        }
        public Ij28BarcodeBL j28BarcodeBL
        {
            get
            {
                if (_j28 == null) _j28 = new j28BarcodeBL(this);
                return _j28;
            }
        }

        public Io42ImapRuleBL o42ImapRuleBL
        {
            get
            {
                if (_o42 == null) _o42 = new o42ImapRuleBL(this);
                return _o42;
            }
        }
        public Ij40MailAccountBL j40MailAccountBL
        {
            get
            {
                if (_j40 == null) _j40 = new j40MailAccountBL(this);
                return _j40;
            }
        }
        public Ij61TextTemplateBL j61TextTemplateBL
        {
            get
            {
                if (_j61 == null) _j61 = new j61TextTemplateBL(this);
                return _j61;
            }
        }
        public Io15AutoCompleteBL o15AutoCompleteBL
        {
            get
            {
                if (_o15 == null) _o15 = new o15AutoCompleteBL(this);
                return _o15;
            }
        }
        
       
     
        
        public Io27AttachmentBL o27AttachmentBL
        {
            get
            {
                if (_o27 == null) _o27 = new o27AttachmentBL(this);
                return _o27;
            }
        }

        public Io23DocBL o23DocBL
        {
            get
            {
                if (_o23 == null) _o23 = new o23DocBL(this);
                return _o23;
            }
        }

        public Io51TagBL o51TagBL
        {
            get
            {
                if (_o51 == null) _o51 = new o51TagBL(this);
                return _o51;
            }
        }
        public Io53TagGroupBL o53TagGroupBL
        {
            get
            {
                if (_o53 == null) _o53 = new o53TagGroupBL(this);
                return _o53;
            }
        }
        public Ip07ProjectLevelBL p07ProjectLevelBL
        {
            get
            {
                if (_p07 == null) _p07 = new p07ProjectLevelBL(this);
                return _p07;
            }
        }
        public Ip54OvertimeLevelBL p54OvertimeLevelBL
        {
            get
            {
                if (_p54 == null) _p54 = new p54OvertimeLevelBL(this);
                return _p54;
            }
        }
        public Ip53VatRateBL p53VatRateBL
        {
            get
            {
                if (_p53 == null) _p53 = new p53VatRateBL(this);
                return _p53;
            }
        }

        public Ic21FondCalendarBL c21FondCalendarBL
        {
            get
            {
                if (_c21 == null) _c21 = new c21FondCalendarBL(this);
                return _c21;
            }
        }
        public Ic26HolidayBL c26HolidayBL
        {
            get
            {
                if (_c26 == null) _c26 = new c26HolidayBL(this);
                return _c26;
            }
        }
        public Ic24DayColorBL c24DayColorBL
        {
            get
            {
                if (_c24 == null) _c24 = new c24DayColorBL(this);
                return _c24;
            }
        }
        public Ip31WorksheetBL p31WorksheetBL
        {
            get
            {
                if (_p31 == null) _p31 = new p31WorksheetBL(this);
                return _p31;
            }
        }
        public Ip32ActivityBL p32ActivityBL
        {
            get
            {
                if (_p32 == null) _p32 = new p32ActivityBL(this);
                return _p32;
            }
        }
        public Ip34ActivityGroupBL p34ActivityGroupBL
        {
            get
            {
                if (_p34 == null) _p34 = new p34ActivityGroupBL(this);
                return _p34;
            }
        }
        public Ip36LockPeriodBL p36LockPeriodBL
        {
            get
            {
                if (_p36 == null) _p36 = new p36LockPeriodBL(this);
                return _p36;
            }
        }
        public Ip38ActivityTagBL p38ActivityTagBL
        {
            get
            {
                if (_p38 == null) _p38 = new p38ActivityTagBL(this);
                return _p38;
            }
        }
        public Ip40WorkSheet_RecurrenceBL p40WorkSheet_RecurrenceBL
        {
            get
            {
                if (_p40 == null) _p40 = new p40WorkSheet_RecurrenceBL(this);
                return _p40;
            }
        }
        public Ip41ProjectBL p41ProjectBL
        {
            get
            {
                if (_p41 == null) _p41 = new p41ProjectBL(this);
                return _p41;
            }
        }
        public Ip42ProjectTypeBL p42ProjectTypeBL
        {
            get
            {
                if (_p42 == null) _p42 = new p42ProjectTypeBL(this);
                return _p42;
            }
        }
        public Ip44ProjectTemplateBL p44ProjectTemplateBL
        {
            get
            {
                if (_p44 == null) _p44 = new p44ProjectTemplateBL(this);
                return _p44;
            }
        }
        public Ip15LocationBL p15LocationBL
        {
            get
            {
                if (_p15 == null) _p15 = new p15LocationBL(this);
                return _p15;
            }
        }
        public Ip51PriceListBL p51PriceListBL
        {
            get
            {
                if (_p51 == null) _p51 = new p51PriceListBL(this);
                return _p51;
            }
        }
        public Ip56TaskBL p56TaskBL
        {
            get
            {
                if (_p56 == null) _p56 = new p56TaskBL(this);
                return _p56;
            }
        }
        public Ip58TaskRecurrenceBL p58TaskRecurrenceBL
        {
            get
            {
                if (_p58 == null) _p58 = new p58TaskRecurrenceBL(this);
                return _p58;
            }
        }
        public Ip75InvoiceRecurrenceBL p75InvoiceRecurrenceBL
        {
            get
            {
                if (_p75 == null) _p75 = new p75InvoiceRecurrenceBL(this);
                return _p75;
            }
        }
        public Ip60TaskTemplateBL p60TaskTemplateBL
        {
            get
            {
                if (_p60 == null) _p60 = new p60TaskTemplateBL(this);
                return _p60;
            }
        }
        public Io21MilestoneTypeBL o21MilestoneTypeBL
        {
            get
            {
                if (_o21 == null) _o21 = new o21MilestoneTypeBL(this);
                return _o21;
            }
        }
        public Io22MilestoneBL o22MilestoneBL
        {
            get
            {
                if (_o22 == null) _o22 = new o22MilestoneBL(this);
                return _o22;
            }
        }
        public Io24ReminderBL o24ReminderBL
        {
            get
            {
                if (_o24 == null) _o24 = new o24ReminderBL(this);
                return _o24;
            }
        }
        public Ip57TaskTypeBL p57TaskTypeBL
        {
            get
            {
                if (_p57 == null) _p57 = new p57TaskTypeBL(this);
                return _p57;
            }
        }
        public Ip11AttendanceBL p11AttendanceBL
        {
            get
            {
                if (_p11 == null) _p11 = new p11AttendanceBL(this);
                return _p11;
            }
        }
        public Ip12ApproveUserDayBL p12ApproveUserDayBL
        {
            get
            {
                if (_p12 == null) _p12 = new p12ApproveUserDayBL(this);
                return _p12;
            }
        }
        public Ip61ActivityClusterBL p61ActivityClusterBL
        {
            get
            {
                if (_p61 == null) _p61 = new p61ActivityClusterBL(this);
                return _p61;
            }
        }

        public Ip63OverheadBL p63OverheadBL
        {
            get
            {
                if (_p63 == null) _p63 = new p63OverheadBL(this);
                return _p63;
            }
        }
        public Ip35UnitBL p35UnitBL
        {
            get
            {
                if (_p35 == null) _p35 = new p35UnitBL(this);
                return _p35;
            }
        }

        public Ix07IntegrationBL x07IntegrationBL
        {
            get
            {
                if (_x07 == null) _x07 = new x07IntegrationBL(this);
                return _x07;
            }
        }

        public Ip85TempboxBL p85TempboxBL
        {
            get
            {
                if (_p85 == null) _p85 = new p85TempboxBL(this);
                return _p85;
            }
        }

        public Ip91InvoiceBL p91InvoiceBL
        {
            get
            {
                if (_p91 == null) _p91 = new p91InvoiceBL(this);
                return _p91;
            }
        }

        public Ip90ProformaBL p90ProformaBL
        {
            get
            {
                if (_p90 == null) _p90 = new p90ProformaBL(this);
                return _p90;
            }
        }

        public Ip82Proforma_PaymentBL p82Proforma_PaymentBL
        {
            get
            {
                if (_p82 == null) _p82 = new p82Proforma_PaymentBL(this);
                return _p82;
            }
        }

        public Ip95InvoiceRowBL p95InvoiceRowBL
        {
            get
            {
                if (_p95 == null) _p95 = new p95InvoiceRowBL(this);
                return _p95;
            }
        }
        public Ip86BankAccountBL p86BankAccountBL
        {
            get
            {
                if (_p86 == null) _p86 = new p86BankAccountBL(this);
                return _p86;
            }
        }
        public Ip93InvoiceHeaderBL p93InvoiceHeaderBL
        {
            get
            {
                if (_p93 == null) _p93 = new p93InvoiceHeaderBL(this);
                return _p93;
            }
        }
        public Ip80InvoiceAmountStructureBL p80InvoiceAmountStructureBL
        {
            get
            {
                if (_p80 == null) _p80 = new p80InvoiceAmountStructureBL(this);
                return _p80;
            }
        }
        public Ip83UpominkaTypeBL p83UpominkaTypeBL
        {
            get
            {
                if (_p83 == null) _p83 = new p83UpominkaTypeBL(this);
                return _p83;
            }
        }
        public Ip84UpominkaBL p84UpominkaBL
        {
            get
            {
                if (_p84 == null) _p84 = new p84UpominkaBL(this);
                return _p84;
            }
        }
        public Ip92InvoiceTypeBL p92InvoiceTypeBL
        {
            get
            {
                if (_p92 == null) _p92 = new p92InvoiceTypeBL(this);
                return _p92;
            }
        }
        public Ip98Invoice_Round_Setting_TemplateBL p98Invoice_Round_Setting_TemplateBL
        {
            get
            {
                if (_p98 == null) _p98 = new p98Invoice_Round_Setting_TemplateBL(this);
                return _p98;
            }
        }

        public Ia55RecPageBL a55RecPageBL
        {
            get
            {
                if (_a55 == null) _a55 = new a55RecPageBL(this);
                return _a55;
            }
        }
        public Ia59RecPageLayerBL a59RecPageLayerBL
        {
            get
            {
                if (_a59 == null) _a59 = new a59RecPageLayerBL(this);
                return _a59;
            }
        }
        public Ia58RecPageBoxBL a58RecPageBoxBL
        {
            get
            {
                if (_a58 == null) _a58 = new a58RecPageBoxBL(this);
                return _a58;
            }
        }

        public Ip49FinancialPlanBL p49FinancialPlanBL
        {
            get
            {
                if (_p49 == null) _p49 = new p49FinancialPlanBL(this);
                return _p49;
            }
        }
        public Ir01CapacityBL r01CapacityBL
        {
            get
            {
                if (_r01 == null) _r01 = new r01CapacityBL(this);
                return _r01;
            }
        }
        public Ir02CapacityVersionBL r02CapacityVersionBL
        {
            get
            {
                if (_r02 == null) _r02 = new r02CapacityVersionBL(this);
                return _r02;
            }
        }
        public Ip68StopWatchBL p68StopWatchBL
        {
            get
            {
                if (_p68 == null) _p68 = new p68StopWatchBL(this);
                return _p68;
            }
        }
        public Ib20HlidacBL b20HlidacBL
        {
            get
            {
                if (_b20 == null) _b20 = new b20HlidacBL(this);
                return _b20;
            }
        }

        public Ib01WorkflowTemplateBL b01WorkflowTemplateBL
        {
            get
            {
                if (_b01 == null) _b01 = new b01WorkflowTemplateBL(this);
                return _b01;
            }
        }
        public Ib02WorkflowStatusBL b02WorkflowStatusBL
        {
            get
            {
                if (_b02 == null) _b02 = new b02WorkflowStatusBL(this);
                return _b02;
            }
        }
        public Ib06WorkflowStepBL b06WorkflowStepBL
        {
            get
            {
                if (_b06 == null) _b06 = new b06WorkflowStepBL(this);
                return _b06;
            }
        }


        public Im62ExchangeRateBL m62ExchangeRateBL
        {
            get
            {
                if (_m62 == null) _m62 = new m62ExchangeRateBL(this);
                return _m62;
            }
        }

        public Ip89ProformaTypeBL p89ProformaTypeBL
        {
            get
            {
                if (_p89 == null) _p89 = new p89ProformaTypeBL(this);
                return _p89;
            }
        }
        public Io18DocTypeBL o18DocTypeBL
        {
            get
            {
                if (_o18 == null) _o18 = new o18DocTypeBL(this);
                return _o18;
            }
        }
        public Io17DocMenuBL o17DocMenuBL
        {
            get
            {
                if (_o17 == null) _o17 = new o17DocMenuBL(this);
                return _o17;
            }
        }

        public Ix01LicenseBL x01LicenseBL
        {
            get
            {
                if (_x01 == null) _x01 = new x01LicenseBL(this);
                return _x01;
            }
        }

        public Ix04NotepadConfigBL x04NotepadConfigBL
        {
            get
            {
                if (_x04 == null) _x04 = new x04NotepadConfigBL(this);
                return _x04;
            }
        }

        public Ix31ReportBL x31ReportBL
        {
            get
            {
                if (_x31 == null) _x31 = new x31ReportBL(this);
                return _x31;
            }
        }
        public Ix38CodeLogicBL x38CodeLogicBL
        {
            get
            {
                if (_x38 == null) _x38 = new x38CodeLogicBL(this);
                return _x38;
            }
        }
        public Ix51HelpCoreBL x51HelpCoreBL
        {
            get
            {
                if (_x51 == null) _x51 = new x51HelpCoreBL(this);
                return _x51;
            }
        }
        public Ix52BlogBL x52BlogBL
        {
            get
            {
                if (_x52 == null) _x52 = new x52BlogBL(this);
                return _x52;
            }
        }
        public Ix54WidgetGroupBL x54WidgetGroupBL
        {
            get
            {
                if (_x54 == null) _x54 = new x54WidgetGroupBL(this);
                return _x54;
            }
        }
        public Ix55WidgetBL x55WidgetBL
        {
            get
            {
                if (_x55 == null) _x55 = new x55WidgetBL(this);
                return _x55;
            }
        }
        public Ix67EntityRoleBL x67EntityRoleBL
        {
            get
            {
                if (_x67 == null) _x67 = new x67EntityRoleBL(this);
                return _x67;
            }
        }
        public Ix97TranslateBL x97TranslateBL
        {
            get
            {
                if (_x97 == null) _x97 = new x97TranslateBL(this);
                return _x97;
            }
        }

     
        public IMailBL MailBL
        {
            get
            {
                if (_mail == null) _mail = new MailBL(this);
                return _mail;
            }
        }

        public Io43InboxBL o43InboxBL
        {
            get
            {
                if (_o43 == null) _o43 = new o43InboxBL(this);
                return _o43;
            }
        }


        public Ix27EntityFieldGroupBL x27EntityFieldGroupBL
        {
            get
            {
                if (_x27 == null) _x27 = new x27EntityFieldGroupBL(this);
                return _x27;
            }
        }
        public Ix28EntityFieldBL x28EntityFieldBL
        {
            get
            {
                if (_x28 == null) _x28 = new x28EntityFieldBL(this);
                return _x28;
            }
        }

        public IWorkflowBL WorkflowBL
        {
            get
            {
                if (_workflow == null) _workflow = new WorkflowBL(this);
                return _workflow;
            }
        }
    }
}
