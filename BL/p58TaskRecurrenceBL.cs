

namespace BL
{
    public interface Ip58TaskRecurrenceBL
    {
        public BO.p58TaskRecurrence Load(int pid);
        public IEnumerable<BO.p58TaskRecurrence> GetList(BO.myQueryP58 mq);
        public int Save(BO.p58TaskRecurrence rec, List<BO.x69EntityRole_Assign> lisX69);
        public IEnumerable<BO.p59TaskRecurrence_Plan> GetList_p59(int p58id=0, int p59id = 0,int p56id=0);
        public IEnumerable<BO.p59TaskRecurrence_Plan> GetList_p59_waiting_on_generate(DateTime d1, DateTime d2);
        public int Generate_Recurrence_Instance(int p58id, int p59id);
        public BO.p58TaskRecurrenceSum LoadSumRow(int pid);

    }

    class p58TaskRecurrenceBL : BaseBL, Ip58TaskRecurrenceBL
    {
        public p58TaskRecurrenceBL(BL.Factory mother) : base(mother)
        {

        }


        private string GetSQL1(string strAppend = null)
        {
            sb("SELECT a.*");
            sb(",j02owner.j02LastName+' '+j02owner.j02FirstName as Owner,b05.b05RecordPid,b05.b05RecordEntity");
            sb("," + _db.GetSQL1_Ocas("p58"));                        
            sb(" FROM p58TaskRecurrence a");
            sb(" INNER JOIN p57TaskType p57x ON a.p57ID=p57x.p57ID LEFT OUTER JOIN p41Project p41 ON a.p41ID=p41.p41ID");            
            sb(" LEFT OUTER JOIN j02User j02owner ON a.j02ID_Owner=j02owner.j02ID");
            sb(" LEFT OUTER JOIN b05Workflow_History b05 ON a.p58ID=b05.p58ID");
            sb(strAppend);
            return sbret();
        }
        public BO.p58TaskRecurrence Load(int pid)
        {
            return _db.Load<BO.p58TaskRecurrence>(GetSQL1(" WHERE a.p58ID=@pid"), new { pid = pid });
        }

        public BO.p58TaskRecurrenceSum LoadSumRow(int pid)
        {
            return _db.Load<BO.p58TaskRecurrenceSum>("EXEC dbo.p58_inhale_sumrow @j02id_sys,@pid", new { j02id_sys = _mother.CurrentUser.pid, pid = pid });
        }

        public IEnumerable<BO.p58TaskRecurrence> GetList(BO.myQueryP58 mq)
        {
            DL.FinalSqlCommand fq = DL.basQuery.GetFinalSql(GetSQL1(), mq, _mother.CurrentUser);
            return _db.GetList<BO.p58TaskRecurrence>(fq.FinalSql, fq.Parameters);
        }



        public int Save(BO.p58TaskRecurrence rec, List<BO.x69EntityRole_Assign> lisX69)
        {
            if (!ValidateBeforeSave(rec))
            {
                return 0;
            }

            using (var sc = new System.Transactions.TransactionScope())
            {
                var p = new DL.Params4Dapper();
                p.AddInt("pid", rec.pid);
                p.AddInt("p57ID", rec.p57ID, true);
                p.AddInt("p41ID", rec.p41ID, true);
                if (rec.j02ID_Owner == 0) rec.j02ID_Owner = _mother.CurrentUser.pid;
                p.AddInt("j02ID_Owner", rec.j02ID_Owner, true);
                p.AddEnumInt("p58RecurrenceType", rec.p58RecurrenceType, true);
                p.AddString("p58Name", rec.p58Name);
            
                p.AddString("p58Notepad", rec.p58Notepad);
                p.AddInt("x04ID", rec.x04ID, true);

                p.AddDateTime("p58BaseDateStart", rec.p58BaseDateStart);
                p.AddInt("p58Generate_DaysToBase_D", rec.p58Generate_DaysToBase_D);
                p.AddInt("p58Generate_DaysToBase_M", rec.p58Generate_DaysToBase_M);
                p.AddDateTime("p58BaseDateEnd", rec.p58BaseDateEnd);

                p.AddBool("p58IsPlanFrom", rec.p58IsPlanFrom);
                p.AddInt("p58PlanFrom_D2Base", rec.p58PlanFrom_D2Base);
                p.AddInt("p58PlanFrom_M2Base", rec.p58PlanFrom_M2Base);

                p.AddBool("p58IsPlanUntil", rec.p58IsPlanUntil);
                p.AddInt("p58PlanUntil_D2Base", rec.p58PlanUntil_D2Base);
                p.AddInt("p58PlanUntil_M2Base", rec.p58PlanUntil_M2Base);

              
                p.AddDouble("p58Plan_Hours", rec.p58Plan_Hours);
                p.AddDouble("p58Plan_Expenses", rec.p58Plan_Expenses);
                p.AddDouble("p58Plan_Revenue", rec.p58Plan_Revenue);
                p.AddEnumInt("p58PlanFlag", rec.p58PlanFlag, true);
                p.AddBool("p58IsStopNotify", rec.p58IsStopNotify);

                int intPID = _db.SaveRecord("p58TaskRecurrence", p, rec);
                if (intPID > 0)
                {
                    
                    if (lisX69 != null && !DL.BAS.SaveX69(_db, "p58", intPID, lisX69))
                    {
                        this.AddMessageTranslated("Error: DL.BAS.SaveX69");
                        return 0;
                    }

                   
                    var pars = new Dapper.DynamicParameters();
                    {
                        pars.Add("p58id", intPID, System.Data.DbType.Int32);
                        pars.Add("j02id_sys", _mother.CurrentUser.pid, System.Data.DbType.Int32);
                    }

                    if (_db.RunSp("p58_aftersave", ref pars, false) == "1")
                    {
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
        private bool ValidateBeforeSave(BO.p58TaskRecurrence rec)
        {
            if (string.IsNullOrEmpty(rec.p58Name))
            {
                this.AddMessage("Chybí vyplnit [Název]."); return false;
            }
            if (rec.p57ID == 0)
            {
                this.AddMessage("Chybí [Typ úkolu]."); return false;
            }
            var recP57 = _mother.p57TaskTypeBL.Load(rec.p57ID);
            if (recP57.p57ProjectFlag==BO.p57ProjectFlagEnum.ProjectCompulsory)
            {
                this.AddMessage("Chybí vazba na projekt."); return false;
            }
            if (rec.p58BaseDateStart==null)
            {
                this.AddMessage("Chybí první rozhodné datum."); return false;
            }
            if (rec.p58BaseDateEnd == null)
            {
                rec.p58BaseDateEnd = rec.p58BaseDateStart.Value.AddYears(1);
            }
            if (rec.pid == 0 && rec.j02ID_Owner == 0) rec.j02ID_Owner = _mother.CurrentUser.pid;

            if ((recP57.p57PlanScope == BO.p57PlanScopeEnum.PlanHodinJePovinny || recP57.p57PlanScope == BO.p57PlanScopeEnum.PlanJePovinny) && (rec.p58Plan_Hours <= 0))
            {
                this.AddMessage("Chybí zadat plán hodin."); return false;
            }
            if ((recP57.p57PlanScope == BO.p57PlanScopeEnum.PlanVydajuJePovinny || recP57.p57PlanScope == BO.p57PlanScopeEnum.PlanJePovinny) && (rec.p58Plan_Expenses <= 0))
            {
                this.AddMessage("Chybí zadat plán peněžních výdajů."); return false;
            }

            if (BO.Code.Recurrence.GetPocetCyklu(Convert.ToDateTime(rec.p58BaseDateStart), Convert.ToDateTime(rec.p58BaseDateEnd), rec.p58RecurrenceType) > 200)
            {
                this.AddMessage("Definujete příliš dlouhé období. Snižte datum posledního rozhodného datumu."); return false;
            }


            return true;
        }


        public IEnumerable<BO.p59TaskRecurrence_Plan> GetList_p59(int p58id=0,int p59id=0,int p56id=0)
        {
            sb("select a.*,p56.p56Name,p56.p56DateInsert,p56.p56PlanUntil,p58.p58Name");
            sb(" FROM p59TaskRecurrence_Plan a LEFT OUTER JOIN p56Task p56 ON a.p56ID_NewInstance=p56.p56ID LEFT OUTER JOIN p58TaskRecurrence p58 ON a.p58ID=p58.p58ID");
            sb(" WHERE 1=1");
            if (p58id > 0)
            {
                sb(" AND a.p58ID="+p58id.ToString());
            }            
            if (p59id > 0)
            {
                sb(" AND a.p59ID=" + p59id.ToString());
            }
            if (p56id > 0)
            {
                sb(" AND a.p56ID_NewInstance=" + p56id.ToString());
            }
            return _db.GetList<BO.p59TaskRecurrence_Plan>(sbret());
        }

        public IEnumerable<BO.p59TaskRecurrence_Plan> GetList_p59_waiting_on_generate(DateTime d1,DateTime d2)
        {
            sb("select a.*,p56.p56Name,p56.p56DateInsert,p56.p56PlanUntil");
            sb(" FROM p59TaskRecurrence_Plan a INNER JOIN p58TaskRecurrence p58 ON a.p58ID=p58.p58ID LEFT OUTER JOIN p56Task p56 ON a.p56ID_NewInstance=p56.p56ID");
            sb(" WHERE GETDATE() BETWEEN p58.p58ValidFrom AND p58.p58ValidUntil AND a.p56ID_NewInstance IS NULL AND a.p59DateCreate BETWEEN @d1 AND @d2");
            
            return _db.GetList<BO.p59TaskRecurrence_Plan>(sbret(), new { d1 = d1,d2=d2 });
        }

        public int Generate_Recurrence_Instance(int p58id,int p59id)
        {            
            var recP58 = Load(p58id);
            if (p59id == 0 || recP58==null) return 0;
            var recP59 = GetList_p59(p58id, p59id).First();
            if (recP59.p56ID_NewInstance > 0)
            {
                this.AddMessage("Tento plánovaný úkol již byl dříve vygenerován.");
                return 0;
            }

            var recP56 = new BO.p56Task() {
                p57ID = recP58.p57ID, p56Name = recP59.p59Name, p56PlanUntil = recP59.p59PlanUntil, p41ID=recP58.p41ID
                ,p56PlanFrom = recP59.p59PlanFrom, j02ID_Owner = recP58.j02ID_Owner
                ,p56Notepad=recP58.p58Notepad,x04ID=recP58.x04ID
                ,p56PlanFlag=recP58.p58PlanFlag,p56Plan_Hours=recP58.p58Plan_Hours,p56Plan_Expenses=recP58.p58Plan_Expenses,p56Plan_Revenue=recP58.p58Plan_Revenue
                ,p56Ordinary=recP58.p58Ordinary,p56IsStopNotify=recP58.p58IsStopNotify
                
            };

            var lisX69 = _mother.x67EntityRoleBL.GetList_X69("p58", recP58.pid);
            int intP56ID = _mother.p56TaskBL.Save(recP56, null, lisX69.ToList());
            if (intP56ID > 0)
            {
                if (recP58.b05RecordEntity != null && recP58.b05RecordPid > 0)
                {
                    var recB05 = new BO.b05Workflow_History() { p56ID = intP56ID, b05IsManualStep = true, b05RecordEntity = recP58.b05RecordEntity, b05RecordPid = recP58.b05RecordPid };
                    _mother.WorkflowBL.Save2History(recB05);
                }

                _db.RunSql("UPDATE p59TaskRecurrence_Plan set p56ID_NewInstance=@p56id,p59ErrorMessage_NewInstance=null WHERE p59ID=@p59id", new { p56id =intP56ID,p59id= p59id });

                var lisO24 = _mother.o24ReminderBL.GetList("p58", p58id);
                if (lisO24.Count() > 0) //doplnit připomenutí úkolu ze šablony
                {
                    foreach(var c in lisO24)
                    {
                        var recO24 = new BO.o24Reminder() { o24RecordEntity = "p56", o24RecordPid = intP56ID,o24Unit=c.o24Unit,j02ID=c.j02ID,x67ID=c.x67ID,j11ID=c.j11ID,o24Count=c.o24Count,o24Memo=c.o24Memo,o24MediumFlag=c.o24MediumFlag,x01ID=c.x01ID };
                        recO24.o24RecordDate = recP56.p56PlanUntil;
                        _mother.o24ReminderBL.Save(recO24);
                    }
                }
            }
            else
            {
                _db.RunSql("UPDATE p59TaskRecurrence_Plan set p59ErrorMessage_NewInstance=@err WHERE p59ID=@p59id", new {err=_mother.GetFirstNotifyMessage(), p59id=p59id});
            }

            return intP56ID;

        }

    }
}
