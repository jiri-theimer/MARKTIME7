using BO;


namespace BL
{
    public interface Ip56TaskBL
    {
        public BO.p56Task Load(int pid);
        public IEnumerable<BO.p56Task> GetList(BO.myQueryP56 mq, bool ischangelog = false);
        public int Save(BO.p56Task rec, List<BO.FreeFieldInput> lisFFI, List<BO.x69EntityRole_Assign> lisX69);
        public BO.p56TaskSum LoadSumRow(int pid);
        public BO.p56RecDisposition InhaleRecDisposition(int pid, BO.p56Task rec = null);
        public int CreateToDoList(string p55name, List<int> p56ids);
        public BO.p55TodoList LoadToDoList(int p55id);
        public int RenameToDoList(int p55id, string newname);
        public IEnumerable<BO.p56TaskDayline> GetList_Dayline(BO.myQueryP56 mq, int intX67ID);
        public IEnumerable<BO.p55TodoList> GetList_TodoList();


    }
    class p56TaskBL : BaseBL, Ip56TaskBL
    {
        public p56TaskBL(BL.Factory mother) : base(mother)
        {

        }


        private string GetSQL1(string strAppend = null, string strAppendColumns = null, bool ischangelog = false, bool istestcloud = false)
        {
            sb("SELECT a.p41ID,a.p57ID,a.j02ID_Owner,a.p56Name,a.p56Code,a.p56Ordinary,a.p56PlanFrom,a.p56PlanUntil,a.p56Plan_Hours,a.p56Plan_Expenses,a.p56Plan_Revenue,a.p56ExternalCode,a.p56Plan_Internal_Fee");
            sb(",p28client.p28Name as ProjectClient,p57x.p57Name,isnull(p41.p41NameShort,p41.p41Name) as p41Name,p41.p41Code,j02owner.j02LastName+' '+j02owner.j02FirstName as Owner");
            sb("," + _db.GetSQL1_Ocas("p56"));
            sb(",a.b02ID,b02.b02Name,b02.b02Color,p57x.b01ID,a.p15ID");
            sb(",a.p56Notepad,a.x04ID,a.p56PlanFlag,a.p56OutlookStatus,a.p56IsStopNotify,b05.b05RecordPid,b05.b05RecordEntity,a.p55ID,p55.p55Name,p57x.x38ID,a.p60ID");
            if (strAppendColumns != null)
            {
                sb(strAppendColumns);
            }
            if (ischangelog)
            {
                sb(" FROM p56Task_Log a");
            }
            else
            {
                sb(" FROM p56Task a");
            }

            sb(" INNER JOIN p57TaskType p57x ON a.p57ID=p57x.p57ID LEFT OUTER JOIN p41Project p41 ON a.p41ID=p41.p41ID LEFT OUTER JOIN p55TodoList p55 ON a.p55ID=p55.p55ID");
            sb(" LEFT OUTER JOIN p28Contact p28client ON p41.p28ID_Client=p28client.p28ID");
            sb(" LEFT OUTER JOIN b02WorkflowStatus b02 ON a.b02ID=b02.b02ID");
            sb(" LEFT OUTER JOIN j02User j02owner ON a.j02ID_Owner=j02owner.j02ID");
            sb(" LEFT OUTER JOIN p56Task_FreeField p56free ON a.p56ID=p56free.p56ID");
            sb(" LEFT OUTER JOIN b05Workflow_History b05 ON a.p56ID=b05.p56ID");
            if (istestcloud)
            {
                sb(AppendCloudQuery(strAppend, "p57x.x01ID"));
            }
            else
            {
                sb(strAppend);
            }


            return sbret();
        }
        public BO.p56Task Load(int pid)
        {
            return _db.Load<BO.p56Task>(GetSQL1(" WHERE a.p56ID=@pid"), new { pid = pid });
        }



        public IEnumerable<BO.p56Task> GetList(BO.myQueryP56 mq, bool ischangelog = false)
        {
            DL.FinalSqlCommand fq = DL.basQuery.GetFinalSql(GetSQL1(null,null, ischangelog), mq, _mother.CurrentUser);
            return _db.GetList<BO.p56Task>(fq.FinalSql, fq.Parameters);
        }

        public IEnumerable<BO.p56TaskDayline> GetList_Dayline(BO.myQueryP56 mq, int intX67ID)
        {
            DL.FinalSqlCommand fq = DL.basQuery.GetFinalSql(GetSQL1($" INNER JOIN x69EntityRole_Assign m1 ON a.p56ID=m1.x69RecordPid AND m1.x69RecordEntity='p56' AND m1.x67ID={intX67ID}", ",m1.j02ID,m1.j11ID,m1.x69IsAllUsers", false, _mother.CurrentUser.IsHostingModeTotalCloud), mq, _mother.CurrentUser);
            return _db.GetList<BO.p56TaskDayline>(fq.FinalSql, fq.Parameters);
        }

        public int Save(BO.p56Task rec, List<BO.FreeFieldInput> lisFFI, List<BO.x69EntityRole_Assign> lisX69)
        {
            if (!ValidateBeforeSave(rec))
            {
                return 0;
            }
            if (!_mother.x67EntityRoleBL.Validate_lisX69_BeforeAssign(lisX69))
            {
                return 0;
            }
            
            int intPID = 0;
            using (var sc = new System.Transactions.TransactionScope())
            {
                var p = new DL.Params4Dapper();
                p.AddInt("pid", rec.pid);
                p.AddInt("p57ID", rec.p57ID, true);
                p.AddInt("p41ID", rec.p41ID, true);
                p.AddInt("p55ID", rec.p55ID, true);
                p.AddInt("p15ID", rec.p15ID, true);
                if (rec.j02ID_Owner == 0) rec.j02ID_Owner = _mother.CurrentUser.pid;
                p.AddInt("j02ID_Owner", rec.j02ID_Owner, true);

                p.AddString("p56Name", rec.p56Name);
                p.AddString("p56Code", rec.p56Code);

                p.AddString("p56Notepad", rec.p56Notepad);
                p.AddInt("x04ID", rec.x04ID, true);
                p.AddInt("p56Ordinary", rec.p56Ordinary);

                p.AddDateTime("p56PlanFrom", rec.p56PlanFrom);
                p.AddDateTime("p56PlanUntil", rec.p56PlanUntil);
                p.AddDouble("p56Plan_Hours", rec.p56Plan_Hours);
                p.AddDouble("p56Plan_Expenses", rec.p56Plan_Expenses);
                p.AddDouble("p56Plan_Revenue", rec.p56Plan_Revenue);
                p.AddDouble("p56Plan_Internal_Fee", rec.p56Plan_Internal_Fee);
                p.AddEnumInt("p56PlanFlag", rec.p56PlanFlag, true);
                p.AddInt("p60ID", rec.p60ID, true);

                p.AddBool("p56IsStopNotify", rec.p56IsStopNotify);

                intPID = _db.SaveRecord("p56Task", p, rec);
                if (intPID > 0)
                {
                    if (!DL.BAS.SaveFreeFields(_db, intPID, lisFFI))
                    {
                        return 0;
                    }
                    if (lisX69 != null && !DL.BAS.SaveX69(_db, "p56", intPID, lisX69))
                    {
                        this.AddMessageTranslated("Error: DL.BAS.SaveX69");
                        return 0;
                    }


                    var pars = new Dapper.DynamicParameters();
                    {
                        pars.Add("p56id", intPID, System.Data.DbType.Int32);
                        pars.Add("j02id_sys", _mother.CurrentUser.pid, System.Data.DbType.Int32);
                    }

                    if (_db.RunSp("p56_aftersave", ref pars, false) == "1")
                    {
                        sc.Complete();


                    }
                    else
                    {
                        return 0;
                    }
                }

            }

            if (intPID > 0 && (rec.pid == 0 || rec.b02ID == 0))
            {
                _mother.WorkflowBL.InitWorkflowStatus(intPID, "p56");
            }
            return intPID;

        }
        private bool ValidateBeforeSave(BO.p56Task rec)
        {
            if (string.IsNullOrEmpty(rec.p56Name))
            {
                this.AddMessage("Chybí vyplnit [Název]."); return false;
            }
            if (rec.p57ID == 0)
            {
                this.AddMessage("Chybí [Typ úkolu]."); return false;
            }
            var recP57 = _mother.p57TaskTypeBL.Load(rec.p57ID);
            if (rec.p41ID==0 && recP57.p57ProjectFlag==BO.p57ProjectFlagEnum.ProjectCompulsory)
            {
                this.AddMessage("Chybí vazba na projekt."); return false;
            }
            if (rec.pid == 0 && rec.j02ID_Owner == 0) rec.j02ID_Owner = _mother.CurrentUser.pid;

            if (rec.p56PlanUntil != null)
            {
                if (Convert.ToDateTime(rec.p56PlanUntil).ToString("HH:mm") == "00:00")
                {
                    rec.p56PlanUntil = Convert.ToDateTime(rec.p56PlanUntil).AddDays(1).AddSeconds(-1);
                }
            }
            if (rec.p56PlanUntil != null && rec.p56PlanFrom != null)
            {
                if (rec.p56PlanUntil < rec.p56PlanFrom)
                {
                    this.AddMessage("Čas plánovaného zahájení úkolu nesmí být větší než Termín úkolu."); return false;
                }
            }

            if ((recP57.p57PlanScope == BO.p57PlanScopeEnum.PlanHodinJePovinny || recP57.p57PlanScope == BO.p57PlanScopeEnum.PlanJePovinny) && (rec.p56Plan_Hours <= 0))
            {
                this.AddMessage("Chybí zadat plán hodin."); return false;
            }
            if ((recP57.p57PlanScope == BO.p57PlanScopeEnum.PlanVydajuJePovinny || recP57.p57PlanScope == BO.p57PlanScopeEnum.PlanJePovinny) && (rec.p56Plan_Expenses <= 0))
            {
                this.AddMessage("Chybí zadat plán peněžních výdajů."); return false;
            }


            if (rec.p56PlanUntil != null && rec.p56PlanFrom != null)
            {
                if (rec.p56PlanUntil < rec.p56PlanFrom)
                {
                    this.AddMessage("Čas začátku nesmí být větší než čas konce."); return false;
                }
            }


            return true;
        }

        public BO.p56TaskSum LoadSumRow(int pid)
        {
            return _db.Load<BO.p56TaskSum>("EXEC dbo.p56_inhale_sumrow @j02id_sys,@pid", new { j02id_sys = _mother.CurrentUser.pid, pid = pid });
        }

        public BO.p56RecDisposition InhaleRecDisposition(int pid, BO.p56Task rec = null)
        {
            var c = new BO.p56RecDisposition();
            if (!_mother.CurrentUser.j04IsModule_p56) return c; //bez přístupu do modulu ÚKOLY
            if (pid > 0 && rec == null) rec = _mother.p56TaskBL.Load(pid);

            if (rec.j02ID_Owner == _mother.CurrentUser.j02ID || _mother.CurrentUser.IsAdmin || _mother.CurrentUser.TestPermission(PermValEnum.GR_p56_Owner))
            {
                c.OwnerAccess = true; c.ReadAccess = true;
                return c;
            }
            if (_mother.CurrentUser.TestPermission(PermValEnum.GR_p56_Reader))
            {
                c.ReadAccess = true;
            }


            var lisX69 = _mother.x67EntityRoleBL.GetList_X69_OneTask(rec, true);
            foreach (var role in lisX69)
            {
                if (BO.Code.Bas.TestPermissionInRoleValue(role.x67RoleValue, BO.PermValEnum.p56_Owner))
                {
                    c.OwnerAccess = true; c.ReadAccess = true;  //vlastník
                    if (role.a55ID > 0) c.a55ID = role.a55ID;
                    return c;
                }
                if (BO.Code.Bas.TestPermissionInRoleValue(role.x67RoleValue, BO.PermValEnum.p56_Reader))
                {
                    c.ReadAccess = true;
                    if (role.a55ID > 0) c.a55ID = role.a55ID;
                }
            }

            


            return c;
        }

        public int CreateToDoList(string p55name, List<int> p56ids)
        {
            var p = new DL.Params4Dapper();
            p.AddInt("pid", 0);
            p.AddString("p55Name", p55name);
            p.AddBool("p55IsPublic", false);
            p.AddInt("j02ID_Owner", _mother.CurrentUser.pid, true);

            var c = new BO.p55TodoList() { p55Name = p55name, j02ID_Owner = _mother.CurrentUser.pid };
            int intP55ID = _db.SaveRecord("p55TodoList", p, c);

            _db.RunSql("UPDATE p56Task set p55ID=@pid WHERE p56ID IN (" + String.Join(",", p56ids) + ")", new { pid = intP55ID });

            return intP55ID;
        }

        public IEnumerable<BO.p55TodoList> GetList_TodoList()
        {
            if (_mother.CurrentUser.IsHostingModeTotalCloud)
            {
                return _db.GetList<BO.p55TodoList>($"SELECT {_db.GetSQL1_Ocas("p55")},a.* FROM p55TodoList a INNER JOIN j02User j02x ON a.j02ID_Owner=j02x.j02ID INNER JOIN j04UserRole j04x ON j02x.j04ID=j04x.j04ID INNER JOIN x67EntityRole x67x ON j04x.x67ID=x67x.x67ID WHERE x67x.x01ID=@x01id_hostingmode AND (a.p55IsPublic=1 OR j02ID_Owner=@j02id)", new { j02id = _mother.CurrentUser.pid, x01id_hostingmode = _mother.CurrentUser.x01ID });
            }
            else
            {
                return _db.GetList<BO.p55TodoList>($"SELECT {_db.GetSQL1_Ocas("p55")},a.* FROM p55TodoList a WHERE a.p55IsPublic=1 OR j02ID_Owner=@j02id", new { j02id = _mother.CurrentUser.pid });
            }

        }

        public BO.p55TodoList LoadToDoList(int p55id)
        {
            return _db.Load<BO.p55TodoList>("SELECT " + _db.GetSQL1_Ocas("p55") + ",a.* FROM p55TodoList a WHERE a.p55ID=@pid", new { pid = p55id });
        }
        public int RenameToDoList(int p55id, string newname)
        {
            if (string.IsNullOrEmpty(newname))
            {
                return 0;
            }
            var p = new DL.Params4Dapper();
            p.AddInt("pid", p55id);
            p.AddString("p55Name", newname);

            var c = LoadToDoList(p55id);
            return _db.SaveRecord("p55TodoList", p, c);


        }
    }
}
