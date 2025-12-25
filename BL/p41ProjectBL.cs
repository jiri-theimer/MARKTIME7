

namespace BL
{
    public interface Ip41ProjectBL
    {
        public BO.p41Project Load(int pid);
        public BO.p41Project LoadLastCreated(int levelindex);
        public BO.p41Project LoadByCode(string strCode, int pid_exclude);
        public BO.p41Project LoadByGuid(string externalpid);
        public IEnumerable<BO.p41Project> GetList(BO.myQueryP41 mq, bool ischangelog = false);
        public int Save(BO.p41Project rec, List<BO.FreeFieldInput> lisFFI, List<BO.x69EntityRole_Assign> lisX69, List<BO.p26ProjectContact> lisP26,string billingmemo=null);
        public BO.p41ProjectSum LoadSumRow(int pid);
        public BO.p41RecDisposition InhaleRecDisposition(int pid, BO.p41Project rec = null);
        public bool ExistWaitingWorksheetForInvoicing(int p41id);
        public bool ExistWaitingWorksheetForApproving(int p41id);
        public int GetBillingLangIndex(BO.p41Project recP41);
        public string GetBillingLangFlagHtml(int intLangIndex);
        public IEnumerable<BO.p26ProjectContact> GetList_p26(int p41id);
        public IEnumerable<BO.r04CapacityResource> GetList_r04(int p41id, int j02id);
        public IEnumerable<BO.r04CapacityResource> GetList_r04(List<int> p41ids, List<int> j02ids);
        public bool Save_r04list(int p41id, List<BO.r04CapacityResource> lis);
        public IEnumerable<BO.p41MyTop10> GetList_MyTop10(int j02id, int toprecs);  //seznam naposledy vykazovaných projektů
        public IEnumerable<BO.p41BoxRec> GetList_p41InProjectBox(int p28id);
        public IEnumerable<BO.p41BoxRec> GetList_p41InProjectBox(List<int> p28ids);
        public IEnumerable<BO.p41BoxRec> GetList_p41InProjectBox(int p41id_level4, int p41id_level3, int p41id_level2, int p41id_level1);
        public string LoadBillingMemo(int p41id);
        

    }
    class p41ProjectBL : BaseBL, Ip41ProjectBL
    {
        public p41ProjectBL(BL.Factory mother) : base(mother)
        {

        }


        private string GetSQL1(string strAppend = null, int intTopRecs = 0, string strAppendAtEnd = null, bool ischangelog = false, bool istestcloud = false)
        {
            sb("SELECT");
            if (intTopRecs > 0) sb($" TOP {intTopRecs}");
            sb(" a.p42ID,a.p07ID,a.j02ID_Owner,a.p41Name,a.p41NameShort,a.p41Code,a.p28ID_Client,a.b02ID,a.j18ID,a.p61ID,a.p92ID,p72ID_BillableHours,a.p72ID_NonBillable,a.p15ID");
            sb(",a.p51ID_Billing,a.p51ID_Internal,p51billing.p51Name as p51Name_Billing");
            sb(",a.p41InvoiceDefaultText1,a.p41InvoiceDefaultText2,a.p41InvoiceMaturityDays,a.p41PlanFrom,a.p41PlanUntil");
            sb(",a.p41Plan_Hours_Billable,a.p41Plan_Hours_Nonbillable,a.p41Plan_Hours,a.p41Plan_Expenses,a.p41Plan_Revenue");
            sb(",p28client.p28Name as Client,p07x.p07Level,p07x.p07Name,a.p41ExternalCode,a.p41BillingMemo200");
            sb(",a.p41TreeLevel,a.p41TreeIndex,a.p41TreePrev,a.p41TreeNext,a.p41TreePath");
            sb(",p42x.p42Name,p42x.p42IsP54,b02.b02Name,b02.b02Color,j18.j18Name,j18.j18CountryCode,p92.p92Name,a.p41BillingLangIndex,p28client.p28BillingLangIndex,p28client.p28BillingMemo200");
            sb(",a.p41ID_P07Level1,a.p41ID_P07Level2,a.p41ID_P07Level3,a.p41ID_P07Level4,a.p41ParentID,a.p41Plan_Internal_Rate,a.p41Plan_Internal_Fee");
            sb(",j02owner.j02Name as Owner,p42x.b01ID,a.p41IsStopNotify");
            sb(",p42x.x38ID,a.p41BitStream,a.p41CapacityStream,a.p41BillingFlag,p42x.p61ID as p61ID_Byp42ID,a.p41WorksheetOperFlag,a.p28ID_Billing,a.p41Round2Minutes,a.p41AccountingIds");
            sb($",{_db.GetSQL1_Ocas("p41")}");
            if (!ischangelog)
            {
                sb(",a.p41Cache_p31Count");
            }
            sb($" FROM {(ischangelog ? "p41Project_Log a" : "p41Project a")} INNER JOIN p42ProjectType p42x ON a.p42ID=p42x.p42ID INNER JOIN p07ProjectLevel p07x ON a.p07ID=p07x.p07ID");
            sb(" LEFT OUTER JOIN p28Contact p28client ON a.p28ID_Client=p28client.p28ID");
            sb(" LEFT OUTER JOIN b02WorkflowStatus b02 ON a.b02ID=b02.b02ID");
            sb(" LEFT OUTER JOIN p92InvoiceType p92 ON a.p92ID=p92.p92ID");
            sb(" LEFT OUTER JOIN p51PriceList p51billing ON a.p51ID_Billing=p51billing.p51ID");
            sb(" LEFT OUTER JOIN j18CostUnit j18 ON a.j18ID=j18.j18ID");

            sb(" LEFT OUTER JOIN j02User j02owner ON a.j02ID_Owner=j02owner.j02ID");

            if (istestcloud)
            {
                sb(this.AppendCloudQuery(strAppend, "p07x.x01ID"));
            }
            else
            {
                sb(strAppend);
            }



            sb(strAppendAtEnd);

            return sbret();
        }
        public BO.p41Project Load(int pid)
        {
            return _db.Load<BO.p41Project>(GetSQL1(" WHERE a.p41ID=@pid"), new { pid = pid });
        }
        public BO.p41Project LoadLastCreated(int levelindex)
        {
            if (levelindex == 0)
            {
                return _db.Load<BO.p41Project>(GetSQL1(" WHERE a.p41UserInsert=@login", 1, " ORDER BY a.p41ID DESC"), new { login = _mother.CurrentUser.j02Login });
            }
            else
            {
                return _db.Load<BO.p41Project>(GetSQL1(" WHERE a.p41UserInsert=@login AND p07x.p07Level=@level", 1, " ORDER BY a.p41ID DESC"), new { login = _mother.CurrentUser.j02Login, level = levelindex });
            }

        }
        public BO.p41Project LoadByCode(string strCode, int pid_exclude)
        {
            if (pid_exclude > 0)
            {
                return _db.Load<BO.p41Project>(GetSQL1(" WHERE a.p41Code LIKE @code AND a.p41ID<>@pid_exclude", 0, null, false, _mother.CurrentUser.IsHostingModeTotalCloud), new { code = strCode, pid_exclude = pid_exclude });
            }
            else
            {
                return _db.Load<BO.p41Project>(GetSQL1(" WHERE a.p41Code LIKE @code", 0, null, false, _mother.CurrentUser.IsHostingModeTotalCloud), new { code = strCode });
            }
        }
        public BO.p41Project LoadByGuid(string guid)
        {
            return _db.Load<BO.p41Project>(GetSQL1(" WHERE a.p41Guid = @guid"), new { guid = guid });
        }
        public BO.p41Project LoadByExternalPID(string externalpid)
        {
            return _db.Load<BO.p41Project>(GetSQL1(" WHERE a.p41ExternalPID=@externalpid", 0, null, false, _mother.CurrentUser.IsHostingModeTotalCloud), new { externalpid = externalpid });
        }
        public IEnumerable<BO.p41BoxRec> GetList_p41InProjectBox(int p28id)
        {
            return _db.GetList<BO.p41BoxRec>("select a.p41ID,ISNULL(a.p41NameShort,a.p41Name) as Project,a.p41Code,a.p41ValidUntil,p07x.p07Level,p42x.p42Name,a.p28ID_Client FROM p41Project a INNER JOIN p42ProjectType p42x ON a.p42ID=p42x.p42ID INNER JOIN p07ProjectLevel p07x ON a.p07ID=p07x.p07ID WHERE a.p28ID_Client=@p28id ORDER BY a.p41Name", new { p28id = p28id });
        }
        public IEnumerable<BO.p41BoxRec> GetList_p41InProjectBox(List<int> p28ids)
        {
            return _db.GetList<BO.p41BoxRec>($"select a.p41ID,ISNULL(a.p41NameShort,a.p41Name) as Project,a.p41Code,a.p41ValidUntil,p07x.p07Level,p42x.p42Name,p28.p28Name as Client,a.p28ID_Client FROM p41Project a INNER JOIN p42ProjectType p42x ON a.p42ID=p42x.p42ID INNER JOIN p07ProjectLevel p07x ON a.p07ID=p07x.p07ID LEFT OUTER JOIN p28Contact p28 ON a.p28ID_Client=p28.p28ID WHERE a.p28ID_Client IN ({string.Join(",",p28ids)}) ORDER BY a.p41Name");
        }
        public IEnumerable<BO.p41BoxRec> GetList_p41InProjectBox(int p41id_level4,int p41id_level3, int p41id_level2,int p41id_level1)
        {
            sbinit();
            sb("select a.p41ID,ISNULL(a.p41NameShort,a.p41Name) as Project,a.p41Code,a.p41ValidUntil,a.p41ParentID, a.p41ID_P07Level1,a.p41ID_P07Level2,a.p41ID_P07Level3,a.p41ID_P07Level4,a.p41TreeIndex,a.p41TreeLevel,a.p41TreePrev,a.p41TreeNext,p07x.p07Level,p42x.p42Name,p28.p28Name as Client,a.p28ID_Client");
            sb(" FROM p41Project a INNER JOIN p42ProjectType p42x ON a.p42ID=p42x.p42ID INNER JOIN p07ProjectLevel p07x ON a.p07ID=p07x.p07ID LEFT OUTER JOIN p28Contact p28 ON a.p28ID_Client=p28.p28ID WHERE");
            sb(" a.p41ID_P07Level4 = @l4");
            if (p41id_level3 > 0)
            {
                sb(" OR a.p41ID_P07Level3 = @l3");
            }
            if (p41id_level2 > 0)
            {
                sb(" OR a.p41ID_P07Level2 = @l2");
            }
            if (p41id_level1 > 0)
            {
                sb(" OR a.p41ID_P07Level1 = @l1");
            }
            sb(" ORDER BY a.p41TreeIndex");

            return _db.GetList<BO.p41BoxRec>(sbret(), new { l4 = p41id_level4,l3=p41id_level3,l2= p41id_level2, l1= p41id_level1 });
        }
        public IEnumerable<BO.p41Project> GetList(BO.myQueryP41 mq, bool ischangelog = false)
        {
           
            DL.FinalSqlCommand fq = DL.basQuery.GetFinalSql(GetSQL1(null, mq.TopRecordsOnly, null, ischangelog), mq, _mother.CurrentUser);
            return _db.GetList<BO.p41Project>(fq.FinalSql, fq.Parameters);
        }

        public string LoadBillingMemo(int p41id)
        {
            return _db.Load<BO.GetString>("select p41BillingMemo as Value FROM p41Project WHERE p41ID=@pid", new { pid = p41id }).Value;
        }
       
        public IEnumerable<BO.p41MyTop10> GetList_MyTop10(int j02id,int toprecs)
        {
            
            return _db.GetList<BO.p41MyTop10>($"SELECT TOP {toprecs} a.p41ID,MAX(a.p31DateInsert) as LastDate,MAX(isnull(p28.p28Name+' - ','')+isnull(p41.p41NameShort,p41.p41Name)) as Project FROM p31Worksheet a INNER JOIN p41Project p41 ON a.p41ID=p41.p41ID LEFT OUTER JOIN p28Contact p28 ON p41.p28ID_Client=p28.p28ID WHERE a.j02ID=@j02id AND a.p31Date>DATEADD(MONTH,-12,GETDATE()) GROUP BY a.p41ID ORDER BY MAX(a.p31DateInsert) DESC", new {j02id=j02id});
        }

        public int Save(BO.p41Project rec, List<BO.FreeFieldInput> lisFFI, List<BO.x69EntityRole_Assign> lisX69, List<BO.p26ProjectContact> lisP26, string billingmemo=null)
        {
            if (rec.p42ID == 0)
            {
                return this.ZeroMessage("Chybí typ projektu.");
            }
            if (rec.p07ID == 0)
            {
                if (_mother.lisP07==null || _mother.lisP07.Count() == 0)
                {
                    var lisP07 = _mother.p07ProjectLevelBL.GetList(new BO.myQuery("p07"));
                    rec.p07ID = lisP07.First(p => p.p07Level == 5).pid;
                }
                else
                {
                    rec.p07ID = _mother.lisP07.Where(p => p.p07Level == 5).First().pid;
                }
                    
            }
            var recP42 = _mother.p42ProjectTypeBL.Load(rec.p42ID);
            if (rec.p41BillingFlag == BO.p41BillingFlagEnum.NuloveSazbyBezWip || rec.p41BillingFlag == BO.p41BillingFlagEnum.NuloveSazbyWip)
            {
                rec.p51ID_Billing = 0;
            }
            if (!ValidateBeforeSave(rec, recP42, lisP26))
            {
                return 0;
            }
            if (!_mother.x67EntityRoleBL.Validate_lisX69_BeforeAssign(lisX69))
            {
                return 0;
            }

            using (var sc = new System.Transactions.TransactionScope())     //ukládání podléhá transakci
            {
                var p = new DL.Params4Dapper();
                p.AddInt("pid", rec.pid);
                p.AddInt("p41ParentID", rec.p41ParentID, true);

                p.AddInt("j02ID_Owner", rec.j02ID_Owner, true);
                p.AddInt("p28ID_Client", rec.p28ID_Client, true);
                p.AddInt("p28ID_Billing", rec.p28ID_Billing, true);

                p.AddInt("p42ID", rec.p42ID, true);
                p.AddInt("p07ID", rec.p07ID, true);
                p.AddInt("p51ID_Billing", rec.p51ID_Billing, true);
                p.AddInt("p51ID_Internal", rec.p51ID_Internal, true);

                p.AddEnumInt("p72ID_BillableHours", rec.p72ID_BillableHours, true);
                p.AddEnumInt("p72ID_NonBillable", rec.p72ID_NonBillable, true);
                p.AddEnumInt("p41BillingFlag", rec.p41BillingFlag);
                p.AddEnumInt("p41WorksheetOperFlag", rec.p41WorksheetOperFlag);

                p.AddInt("p92ID", rec.p92ID, true);
                p.AddInt("j18ID", rec.j18ID, true);
                p.AddInt("p61ID", rec.p61ID, true);
                p.AddInt("p15ID", rec.p15ID, true);
                p.AddDouble("p41Plan_Hours_Billable", rec.p41Plan_Hours_Billable);
                p.AddDouble("p41Plan_Hours_Nonbillable", rec.p41Plan_Hours_Nonbillable);
                p.AddDouble("p41Plan_Hours", rec.p41Plan_Hours_Billable + rec.p41Plan_Hours_Nonbillable);

                p.AddDouble("p41Plan_Expenses", rec.p41Plan_Expenses);
                p.AddDouble("p41Plan_Revenue", rec.p41Plan_Revenue);
                p.AddDouble("p41Plan_Internal_Rate", rec.p41Plan_Internal_Rate);
                p.AddDouble("p41Plan_Internal_Fee", rec.p41Plan_Internal_Fee);

                p.AddInt("p41InvoiceMaturityDays", rec.p41InvoiceMaturityDays);
                p.AddString("p41InvoiceDefaultText1", rec.p41InvoiceDefaultText1);
                p.AddString("p41InvoiceDefaultText2", rec.p41InvoiceDefaultText2);

                
                p.AddInt("p41BitStream", rec.p41BitStream);
                p.AddBool("p41IsStopNotify", rec.p41IsStopNotify);
                p.AddInt("p41CapacityStream", rec.p41CapacityStream);

                p.AddString("p41Name", rec.p41Name);
                p.AddString("p41Code", rec.p41Code);
                p.AddString("p41NameShort", rec.p41NameShort);
                p.AddString("p41ExternalCode", rec.p41ExternalCode);
                if (rec.p41Guid == Guid.Empty)
                {
                    rec.p41Guid = Guid.NewGuid();
                }
                p.AddString("p41Guid", rec.p41Guid.ToString());
                

                p.AddDateTime("p41PlanFrom", rec.p41PlanFrom);
                if (rec.p41PlanUntil != null)
                {
                    rec.p41PlanUntil = Convert.ToDateTime(rec.p41PlanUntil).AddDays(1).AddSeconds(-1);
                }
                p.AddDateTime("p41PlanUntil", rec.p41PlanUntil);

                p.AddInt("p41BillingLangIndex", rec.p41BillingLangIndex);
                p.AddInt("p41Round2Minutes", rec.p41Round2Minutes);
                p.AddString("p41AccountingIds", rec.p41AccountingIds);
                p.AddString("p41BillingMemo200", rec.p41BillingMemo200);
                if (rec.p41BillingMemo200==null || billingmemo != null)
                {
                    p.AddString("p41BillingMemo", billingmemo);
                }
                

                int intPID = _db.SaveRecord("p41Project", p, rec);
                if (intPID > 0)
                {
                    if (!DL.BAS.SaveFreeFields(_db, intPID, lisFFI))
                    {
                        return 0;
                    }
                    if (lisX69 != null && !DL.BAS.SaveX69(_db, "p41", intPID, lisX69))
                    {
                        this.AddMessageTranslated("Error: DL.BAS.SaveX69");
                        return 0;
                    }
                    if (lisP26 != null)
                    {
                        foreach (var c in lisP26)
                        {
                            if (c.IsSetAsDeleted)
                            {
                                if (c.pid > 0)
                                {
                                    _db.RunSql("DELETE FROM p26ProjectContact WHERE p26ID=@pid", new { pid = c.pid });
                                }
                            }
                            else
                            {
                                var recP26 = new BO.p26ProjectContact();
                                if (c.pid > 0)
                                {
                                    recP26 = _db.Load<BO.p26ProjectContact>("select a.*," + _db.GetSQL1_Ocas("p26", false, false) + " from p26ProjectContact a WHERE a.p26ID=@pid", new { pid = c.pid });
                                }
                                p = new DL.Params4Dapper();
                                p.AddInt("pid", recP26.pid);
                                p.AddInt("p41ID", intPID, true);
                                p.AddString("p26Name", c.p26Name);
                                p.AddInt("p28ID", c.p28ID);
                                p.AddInt("p27ID", c.p27ID);

                                _db.SaveRecord("p26ProjectContact", p, recP26, false, true);
                            }
                        }
                    }

                    var pars = new Dapper.DynamicParameters();
                    {
                        pars.Add("p41id", intPID, System.Data.DbType.Int32);
                        pars.Add("j02id_sys", _mother.CurrentUser.pid, System.Data.DbType.Int32);
                    }

                    if (_db.RunSp("p41_aftersave", ref pars, false) == "1")
                    {

                        if (recP42.b01ID > 0 && rec.b02ID == 0)
                        {
                            _mother.WorkflowBL.InitWorkflowStatus(intPID, "p41");   //nahodit úvodní workflow stav záznamu
                        }

                        sc.Complete();
                    }
                    else
                    {
                        return 0;
                    }
                }
                return intPID;
            }

        }

        public bool ValidateBeforeSave(BO.p41Project rec, BO.p42ProjectType recP42, List<BO.p26ProjectContact> lisP26 = null)
        {
            if (rec.pid == 0 && !_mother.CurrentUser.TestPermission(BO.PermValEnum.GR_p41_Creator))
            {
                return this.FalsehMessage("Nemáte oprávnění zakládat projekty.");
            }
            if (rec.j02ID_Owner == 0)
            {
                this.AddMessage("Chybí vyplnit [Vlastník záznamu]."); return false;
            }
            if (string.IsNullOrEmpty(rec.p41Name))
            {
                return this.FalsehMessage("Chybí vyplnit [Název].");
            }
            if (rec.p41PlanFrom != null && rec.p41PlanUntil == null)
            {
                return this.FalsehMessage("Pokud je plán zahájení vyplněný, musíte vyplnit i datum plánu dokončení.");
            }
            if (rec.p41PlanFrom != null && rec.p41PlanUntil != null)
            {
                if (rec.p41PlanFrom > rec.p41PlanUntil)
                {
                    return this.FalsehMessage("Datum plánovaného zahájení je větší než datum plánovaného dokončení.");
                }
                if ((Convert.ToDateTime(rec.p41PlanUntil) - Convert.ToDateTime(rec.p41PlanFrom)).TotalDays > 2000)
                {
                    return this.FalsehMessage("Období časového plánu projektu může být technicky maximálně 2000 dní.");
                }

            }

            if (_mother.CBL.TestIfAllowCreateRecord("p41", rec.p42ID).Flag == BO.ResultEnum.Failed)
            {
                return this.FalsehMessage("Nemáte oprávnění zakládat projekty tohoto typu.");
            }

            if (lisP26 != null && lisP26.Any(p => p.p28ID == 0 && p.IsSetAsDeleted == false))
            {
                return this.FalsehMessage("Ve svázaných kontaktech projektu chybí vazba na záznam kontaktu.");
            }

            if (rec.ValidUntil < DateTime.Now && rec.pid > 0 && (int)rec.p41BillingFlag<99)
            {
                //pokus o přesun projektu do archivu
                switch (recP42.p42ArchiveFlag)
                {
                    case BO.p42ArchiveFlagENUM.NoArchive_Waiting_Approve:
                        if (ExistWaitingWorksheetForInvoicing(rec.pid))
                        {
                            return this.FalsehMessage("Projekt nelze přesunout do achivu, dokud v něm existují nevyfakturované úkony. Tuto ochranu může změnit administrátor v nastavení typu projektu.");
                        }
                        break;
                    case BO.p42ArchiveFlagENUM.NoArchive_Waiting_Invoice:
                        if (ExistWaitingWorksheetForInvoicing(rec.pid))
                        {
                            return this.FalsehMessage("Projekt nelze přesunout do achivu, dokud v něm existují rozpracované úkony. Rozpracované úkony lze přesunout do archivu nebo tuto ochranu může změnit administrátor v nastavení typu projektu.");
                        }
                        break;
                }
            }


            return true;
        }
       
        public BO.p41ProjectSum LoadSumRow(int pid)
        {
            if (_mother.CurrentUser.j02PerformanceFlag == 1)
            {
                return new BO.p41ProjectSum() { tabname = _mother.tra("Záznam") };
            }
            else
            {
                return _db.Load<BO.p41ProjectSum>("EXEC dbo.p41_inhale_sumrow @j02id_sys,@pid", new { j02id_sys = _mother.CurrentUser.pid, pid = pid });
            }
                                    
        }

        public BO.p41RecDisposition InhaleRecDisposition(int pid, BO.p41Project rec = null)
        {
            if (rec == null && pid == 0) return null;
            if (rec == null) rec = Load(pid);
            if (rec == null) return null;
            var c = new BO.p41RecDisposition();
            if (rec.p07Level == 5) c.a55ID = _mother.CurrentUser.a55ID_le5;
            if (rec.p07Level <= 4) c.a55ID = _mother.CurrentUser.a55ID_le4;

            if (_mother.CurrentUser.IsAdmin)
            {
                c.OwnerAccess = true; c.ReadAccess = true; c.p91_DraftCreate = true; c.p31_MoveToOtherProject = true; c.p31_RecalcRates = true; c.p91_Read = true;
                return c;
            }
            if (rec.j02ID_Owner == _mother.CurrentUser.pid || _mother.CurrentUser.TestPermission(BO.PermValEnum.GR_p41_Owner))
            {
                c.OwnerAccess = true; c.ReadAccess = true; c.p31_MoveToOtherProject = true; c.p31_RecalcRates = true;
            }

            var lisX69 = _mother.x67EntityRoleBL.GetList_X69_OneProject(rec, true);
           
            foreach (var role in lisX69)
            {
                
                if (BO.Code.Bas.TestPermissionInRoleValue(role.x67RoleValue, BO.PermValEnum.p41_Owner))   //vlastník
                {
                    c.OwnerAccess = true; c.ReadAccess = true;  //vlastník
                    c.p31_MoveToOtherProject = true; c.p31_RecalcRates = true;
                    if (role.a55ID > 0) c.a55ID = role.a55ID;
                }
                if (BO.Code.Bas.TestPermissionInRoleValue(role.x67RoleValue, BO.PermValEnum.p41_Reader))   //čtenář
                {
                    c.ReadAccess = true;
                    if (role.a55ID > 0) c.a55ID = role.a55ID;
                }
                if (!c.p91_DraftCreate)
                {
                    c.p91_DraftCreate = BO.Code.Bas.TestPermissionInRoleValue(role.x67RoleValue, BO.PermValEnum.p41_CreateDraftInvoice);
                }
                if (!c.p91_Create)
                {
                    c.p91_Create = BO.Code.Bas.TestPermissionInRoleValue(role.x67RoleValue, BO.PermValEnum.p41_CreateInvoice);
                }
                if (!c.p91_Read)
                {
                    c.p91_Read = BO.Code.Bas.TestPermissionInRoleValue(role.x67RoleValue, BO.PermValEnum.p41_ReadInvoice);
                }
                
                


                
            }

            if (!c.ReadAccess)
            {
                c.ReadAccess = _mother.CurrentUser.TestPermission(BO.PermValEnum.GR_p41_Reader);    //čtenář všech projektů
            }

            return c;
        }

        public bool ExistWaitingWorksheetForInvoicing(int p41id)
        {
            if (_db.Load<BO.GetInteger>("select top 1 p31ID as Value FROM p31worksheet WHERE p41ID=@pid AND p91ID IS NULL AND isnull(p31ExcludeBillingFlag,0)=0", new { pid = p41id }) != null)
            {
                return true;
            }
            return false;
        }
        public bool ExistWaitingWorksheetForApproving(int p41id)
        {
            if (_db.Load<BO.GetInteger>("select top 1 p31ID FROM p31worksheet WHERE p41ID=@pid AND p71ID IS NULL AND p91ID IS NULL AND isnull(p31ExcludeBillingFlag,0)=0", new { pid = p41id }) != null)
            {
                return true;
            }
            return false;
        }


        public int GetBillingLangIndex(BO.p41Project recP41)
        {
            if (recP41 == null) return 0;

            int intRetLang = recP41.p41BillingLangIndex > 0 ? recP41.p41BillingLangIndex : recP41.p28BillingLangIndex;
            if (intRetLang == 0) return 0;

            return intRetLang;
           
        }

        public string GetBillingLangFlagHtml(int intLangIndex)
        {
            if (intLangIndex == 0) return null;

            string s = null;
            if (intLangIndex == 1) s = _mother.Lic.x01BillingFlag1;
            if (intLangIndex == 2) s = _mother.Lic.x01BillingFlag2;
            if (intLangIndex == 3) s = _mother.Lic.x01BillingFlag3;
            if (intLangIndex == 4) s = _mother.Lic.x01BillingFlag4;

            if (s != null && File.Exists(_mother.App.WwwRootFolder + "\\images\\flags\\" + s))
            {
                return $"<strong>{_mother.Lic.GetBillingLang(intLangIndex)}</strong><img src='/images/flags/{s}'/>";
            }
            else
            {
                return $"<strong>{_mother.Lic.GetBillingLang(intLangIndex)}</strong>";
            }


        }

        public IEnumerable<BO.p26ProjectContact> GetList_p26(int p41id)
        {
            string s = $"select a.*,{_db.GetSQL1_Ocas("p26", false, false)}, p28.p28Name,p27.p27Name FROM p26ProjectContact a INNER JOIN p28Contact p28 ON a.p28ID=p28.p28ID LEFT OUTER JOIN p27Pctype p27 ON a.p27ID=p27.p27ID WHERE a.p41ID=@p41id";

            return _db.GetList<BO.p26ProjectContact>(s, new { p41id = p41id });
        }

        public IEnumerable<BO.r04CapacityResource> GetList_r04(int p41id, int j02id)
        {
            string s = "SELECT a.*," + _db.GetSQL1_Ocas("r04", false, false) + ", j02.j02LastName+' '+j02.j02FirstName as Person,ISNULL(p41.p41NameShort,p41.p41Name) as Project,p41.p41CapacityStream,p28.p28Name as Client FROM r04CapacityResource a INNER JOIN j02User j02 ON a.j02ID=j02.j02ID INNER JOIN p41Project p41 ON a.p41ID=p41.p41ID LEFT OUTER JOIN p28Contact p28 ON p41.p28ID_Client=p28.p28ID WHERE GETDATE() BETWEEN j02.j02ValidFrom AND j02.j02ValidUntil";
            if (p41id > 0)
            {
                s += " AND a.p41ID=@p41id";
            }
            if (j02id > 0)
            {
                s += " AND a.j02ID=@j02id";
            }
            if (p41id > 0)
            {
                s += " ORDER BY p28.p28Name,p41.p41Name";
            }
            else
            {
                s += " ORDER BY j02.j02LastName";
            }


            return _db.GetList<BO.r04CapacityResource>(s, new { p41id = p41id, j02id = j02id });
        }
        public IEnumerable<BO.r04CapacityResource> GetList_r04(List<int> p41ids, List<int> j02ids)
        {
            string s = "SELECT a.*," + _db.GetSQL1_Ocas("r04", false, false) + $", j02.j02LastName+' '+j02.j02FirstName as Person,ISNULL(p41.p41NameShort,p41.p41Name) as Project,p41.p41CapacityStream,p28.p28Name as Client FROM r04CapacityResource a INNER JOIN j02User j02 ON a.j02ID=j02.j02ID INNER JOIN p41Project p41 ON a.p41ID=p41.p41ID LEFT OUTER JOIN p28Contact p28 ON p41.p28ID_Client=p28.p28ID WHERE GETDATE() BETWEEN j02.j02ValidFrom AND j02.j02ValidUntil";
            if (p41ids != null && p41ids.Count() > 0)
            {
                s += $" AND a.p41ID IN ({string.Join(",", p41ids)})";
                s += " ORDER BY a.p41ID, j02.j02LastName";
            }
            if (j02ids != null && j02ids.Count() > 0)
            {
                s += $" AND a.j02ID IN ({string.Join(",", j02ids)})";
                s += " ORDER BY p28.p28Name,p41.p41Name";
            }

            return _db.GetList<BO.r04CapacityResource>(s);
        }
        public bool Save_r04list(int p41id, List<BO.r04CapacityResource> lis)
        {
            if (lis.DistinctBy(p => p.j02ID).Count() < lis.Count())
            {
                this.AddMessageTranslated("Duplicitní zadání uživatele.");
                return false;
            }
            int x = 1;
            foreach (var c in lis)
            {
                if (c.j02ID == 0)
                {
                    this.AddMessageTranslated($"Řádek #{x}: Na vstupu jsou neúplná data.");
                    return false;
                }
                x += 1;
            }

            _db.RunSql("DELETE FROM r04CapacityResource WHERE p41ID=@pid", new { pid = p41id });


            foreach (var c in lis)
            {
                _db.RunSql("INSERT INTO r04CapacityResource(p41ID,j02ID,r04Text,r04HoursFa,r04HoursNeFa,r04HoursTotal,r04UserInsert,r04UserUpdate,r04DateInsert,r04DateUpdate,r04WorksheetFlag) VALUES(@p41id,@j02id,@txt,@fa,@nefa,@total,@login,@login,@now,@now,@ef)", new
                {
                    p41id = p41id,
                    j02id = c.j02ID,
                    txt = c.r04Text,
                    fa = c.r04HoursFa,
                    nefa = c.r04HoursNeFa,
                    total = c.r04HoursFa + c.r04HoursNeFa,
                    login = _mother.CurrentUser.j02Login,
                    now = DateTime.Now,
                    ef = c.r04WorksheetFlag
                }
                );
            }



            _db.RunSql("exec dbo.p41_recalc_capacity @p41id", new { p41id = p41id });


            return true;

        }
    }
}
