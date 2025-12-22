

using BO;


namespace BL
{
    public interface Ip75InvoiceRecurrenceBL
    {
        public BO.p75InvoiceRecurrence Load(int pid);
        public IEnumerable<BO.p75InvoiceRecurrence> GetList(BO.myQueryP75 mq);
        public int Save(BO.p75InvoiceRecurrence rec);
        public IEnumerable<BO.p76InvoiceRecurrence_Plan> GetList_p76(int p75id = 0, int p76id = 0, int p56id = 0);
        public IEnumerable<BO.p76InvoiceRecurrence_Plan> GetList_p76_waiting_on_generate(DateTime d1, DateTime d2);
        public int Generate_Recurrence_Instance(int p75id, int p76id);
        public BO.p75InvoiceRecurrenceSum LoadSumRow(int pid);
        public bool UpdateGeneratedInvoice_NewInstance(int p76id, int p91id);

    }

    class p75InvoiceRecurrenceBL : BaseBL, Ip75InvoiceRecurrenceBL
    {
        public p75InvoiceRecurrenceBL(BL.Factory mother) : base(mother)
        {

        }


        private string GetSQL1(string strAppend = null)
        {
            sb("SELECT a.*");
            sb(",j02owner.j02LastName+' '+j02owner.j02FirstName as Owner");
            sb("," + _db.GetSQL1_Ocas("p75"));
            sb(" FROM p75InvoiceRecurrence a");
            sb(" LEFT OUTER JOIN p41Project p41 ON a.p41ID=p41.p41ID");
            sb(" LEFT OUTER JOIN j02User j02owner ON a.j02ID_Owner=j02owner.j02ID");
            
            sb(strAppend);
            return sbret();
        }
        public BO.p75InvoiceRecurrence Load(int pid)
        {
            return _db.Load<BO.p75InvoiceRecurrence>(GetSQL1(" WHERE a.p75ID=@pid"), new { pid = pid });
        }

        

        public IEnumerable<BO.p75InvoiceRecurrence> GetList(BO.myQueryP75 mq)
        {
            DL.FinalSqlCommand fq = DL.basQuery.GetFinalSql(GetSQL1(), mq, _mother.CurrentUser);
            return _db.GetList<BO.p75InvoiceRecurrence>(fq.FinalSql, fq.Parameters);
        }

        public bool UpdateGeneratedInvoice_NewInstance(int p76id,int p91id)
        {
            return _db.RunSql("UPDATE p76InvoiceRecurrence_Plan set p91ID_NewInstance=@p91id WHERE p76ID=@p76id", new {p91id=p91id,p76id=p76id});
        }
        public BO.p75InvoiceRecurrenceSum LoadSumRow(int pid)
        {
            return _db.Load<BO.p75InvoiceRecurrenceSum>("EXEC dbo.p75_inhale_sumrow @j02id_sys,@pid", new { j02id_sys = _mother.CurrentUser.pid, pid = pid });
        }

        public int Save(BO.p75InvoiceRecurrence rec)
        {
            if (!ValidateBeforeSave(rec))
            {
                return 0;
            }

            using (var sc = new System.Transactions.TransactionScope())
            {
                var p = new DL.Params4Dapper();
                p.AddInt("pid", rec.pid);
                p.AddInt("p41ID", rec.p41ID, true);
                p.AddInt("p28ID", rec.p28ID, true);
                if (rec.j02ID_Owner == 0) rec.j02ID_Owner = _mother.CurrentUser.pid;
                p.AddInt("j02ID_Owner", rec.j02ID_Owner, true);
                p.AddEnumInt("p75RecurrenceType", rec.p75RecurrenceType, true);
                p.AddString("p75Name", rec.p75Name);
                p.AddString("p75InvoiceText", rec.p75InvoiceText);
                p.AddBool("p75IsDraft", rec.p75IsDraft);
                
                p.AddDateTime("p75BaseDateStart", rec.p75BaseDateStart);
                p.AddInt("p75Generate_DaysToBase_D", rec.p75Generate_DaysToBase_D);                
                p.AddDateTime("p75BaseDateEnd", rec.p75BaseDateEnd);
               
                
                p.AddInt("p75DateSupplyFlag", rec.p75DateSupplyFlag);
                p.AddInt("p75DateMaturityDaysAfter", rec.p75DateMaturityDaysAfter);
                p.AddString("p75InvoiceText", rec.p75InvoiceText);

                p.AddInt("p75PeriodFlag", rec.p75PeriodFlag);

                int intPID = _db.SaveRecord("p75InvoiceRecurrence", p, rec);
                if (intPID > 0)
                {

                    

                    var pars = new Dapper.DynamicParameters();
                    {
                        pars.Add("p75id", intPID, System.Data.DbType.Int32);
                        pars.Add("j02id_sys", _mother.CurrentUser.pid, System.Data.DbType.Int32);
                    }

                    if (_db.RunSp("p75_aftersave", ref pars, false) == "1")
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
        private bool ValidateBeforeSave(BO.p75InvoiceRecurrence rec)
        {
            
            if (rec.p41ID == 0 && rec.p28ID==0)
            {
                this.AddMessage("Chybí [Projekt] nebo [Klient]."); return false;
            }
            if (rec.p41ID > 0 && rec.p28ID > 0)
            {
                this.AddMessage("Zadejte buď [Projekt] nebo [Klient]."); return false;
            }
            if (rec.p41ID > 0)
            {
                rec.p28ID = 0;
            }
            if (rec.p41ID > 0 && _mother.p41ProjectBL.Load(rec.p41ID).p92ID==0)
            {
                this.AddMessage("Ve fakturačním nastavení projektu chybí vyplnit [Typ faktury]."); return false;
            }
            
            if (rec.p28ID > 0 && _mother.p28ContactBL.Load(rec.p28ID).p92ID == 0)
            {
                this.AddMessage("Ve fakturačním nastavení klienta chybí vyplnit [Typ faktury]."); return false;
            }
            if (rec.p28ID > 0)
            {
                var lisP41 = _mother.p41ProjectBL.GetList(new BO.myQueryP41("p41") { p28id = rec.p28ID, });
                if (lisP41.Count() == 0)
                {
                    this.AddMessage("Tento klient nemá ani jeden otevřený projekt."); return false;
                }
            }
            if (string.IsNullOrEmpty(rec.p75Name))
            {
                this.AddMessage("Chybí vyplnit [Název]."); return false;
            }
            if (rec.p75BaseDateStart == null)
            {
                this.AddMessage("Chybí první rozhodné datum."); return false;
            }
            if (rec.p75BaseDateEnd == null)
            {
                rec.p75BaseDateEnd = rec.p75BaseDateStart.Value.AddYears(1);
            }
            if (rec.pid == 0 && rec.j02ID_Owner == 0) rec.j02ID_Owner = _mother.CurrentUser.pid;

           

            if (BO.Code.Recurrence.GetPocetCyklu(Convert.ToDateTime(rec.p75BaseDateStart), Convert.ToDateTime(rec.p75BaseDateEnd), rec.p75RecurrenceType) > 200)
            {
                this.AddMessage("Definujete příliš dlouhé období. Snižte datum posledního rozhodného datumu."); return false;
            }


            return true;
        }


        public IEnumerable<BO.p76InvoiceRecurrence_Plan> GetList_p76(int p75id = 0, int p76id = 0, int p91id = 0)
        {
            sb("select a.*,p91.p91Code,p91.p91DateInsert,p91.p91DateSupply,p75.p75Name,p91.p91Text1");
            sb(" FROM p76InvoiceRecurrence_Plan a LEFT OUTER JOIN p91Invoice p91 ON a.p91ID_NewInstance=p91.p91ID LEFT OUTER JOIN p75InvoiceRecurrence p75 ON a.p75ID=p75.p75ID");
            sb(" WHERE 1=1");
            if (p75id > 0)
            {
                sb(" AND a.p75ID=" + p75id.ToString());
            }
            if (p76id > 0)
            {
                sb(" AND a.p76ID=" + p76id.ToString());
            }
            if (p91id > 0)
            {
                sb(" AND a.p91ID_NewInstance=" + p91id.ToString());
            }
            return _db.GetList<BO.p76InvoiceRecurrence_Plan>(sbret());
        }

        public IEnumerable<BO.p76InvoiceRecurrence_Plan> GetList_p76_waiting_on_generate(DateTime d1, DateTime d2)
        {
            sb("select a.*,p91.p91Code,p91.p91DateInsert,p91.p91DateSupply,p75.p75Name");
            sb(" FROM p76InvoiceRecurrence_Plan a INNER JOIN p75InvoiceRecurrence p75 ON a.p75ID=p75.p75ID LEFT OUTER JOIN p91Invoice p91 ON a.p91ID_NewInstance=p91.p91ID");
            sb(" WHERE GETDATE() BETWEEN p75.p75ValidFrom AND p75.p75ValidUntil AND a.p91ID_NewInstance IS NULL AND a.p76DateCreate BETWEEN @d1 AND @d2");

            return _db.GetList<BO.p76InvoiceRecurrence_Plan>(sbret(), new { d1 = d1, d2 = d2 });
        }

        public int Generate_Recurrence_Instance(int p75id, int p76id)
        {
            var recp75 = Load(p75id);
            if (p76id == 0 || recp75 == null) return 0;
            var recp76 = GetList_p76(p75id, p76id).First();
            if (recp76.p91ID_NewInstance > 0)
            {
                this.AddMessage("Toto plánované vyúčtování již bylo dříve vygenerovánp.");
                return 0;
            }
            DateTime? gd1 = null;
            DateTime? gd2 = null;
            switch (recp75.p75PeriodFlag)
            {
                case 1: //měsíc                    
                    gd1 = new DateTime(recp76.p76BaseDate.Value.Year, recp76.p76BaseDate.Value.Month, 1);
                    gd2 = new DateTime(recp76.p76BaseDate.Value.Year, recp76.p76BaseDate.Value.Month, 1).AddMonths(1).AddDays(-1);
                    break;
                case 2: //kvartál                    
                    var d1 = recp76.p76BaseDate.Value;
                    int x = (int)System.Math.Ceiling((decimal)d1.Month / 3);
                    gd1 = new DateTime(d1.Year, (3 * x) - 2, 1);
                    gd2 = d1.AddMonths(3).AddDays(-1);
                    break;
                case 3: //rok                    
                    gd1 = new DateTime(recp76.p76BaseDate.Value.Year, 1, 1);
                    gd2 = new DateTime(recp76.p76BaseDate.Value.Year, 12, 31);
                    break;
                default:    //nefiltrovat období
                    break;
            }
            var mq = new BO.myQueryP31() {p41id=recp75.p41ID,p28id=recp75.p28ID, iswip = true };    //rozpracované úkony
            if (gd1 != null)
            {
                mq.period_field = "p31Date";mq.global_d1 = gd1; mq.global_d2 = gd2;
            }
            
            var lisP31 = _mother.p31WorksheetBL.GetList(mq);
            foreach(var c in lisP31)
            {
                var rec = new p31WorksheetApproveInput() { p31ID = c.pid, p33ID = c.p33ID, p32ID = c.p32ID,p71id=BO.p71IdENUM.Schvaleno,p31Text=c.p31Text,Rate_Billing_Approved=c.p31Rate_Billing_Orig,Rate_Internal_Approved=c.p31Rate_Internal_Orig,p31TextInternal=c.p31TextInternal,p31Date=c.p31Date };
                if (c.p32IsBillable)
                {
                    if (c.p31Amount_WithoutVat_Orig > 0)
                    {
                        rec.p72id = BO.p72IdENUM.Fakturovat;
                    }
                    else
                    {
                        rec.p72id = BO.p72IdENUM.ZahrnoutDoPausalu;
                    }

                }
                else
                {
                    rec.p72id = BO.p72IdENUM.ZahrnoutDoPausalu;
                }
                switch (c.p33ID)
                {
                    case p33IdENUM.Cas:
                    case p33IdENUM.Kusovnik:
                        rec.Value_Approved_Billing = c.p31Hours_Orig;
                        rec.Value_Approved_Internal = c.p31Hours_Orig;
                        break;
                    default:
                        rec.Value_Approved_Billing = c.p31Amount_WithoutVat_Orig;
                        rec.Value_Approved_Internal = c.p31Amount_WithoutVat_Orig;
                        break;
                }
                _mother.p31WorksheetBL.Save_Approving(rec,false,true);
            }
            mq = new BO.myQueryP31() { p41id = recp75.p41ID, p28id = recp75.p28ID, isapproved_and_wait4invoice = true}; //schválené
            if (gd1 != null)
            {
                mq.period_field = "p31Date"; mq.global_d1 = gd1; mq.global_d2 = gd2;
            }
            lisP31 = _mother.p31WorksheetBL.GetList(mq);
            if (lisP31.Count() == 0)
            {
                return 0; //není co fakturovat
            }
            var recP91 = new BO.p91Create() { DateIssue=recp76.p76DateCreate, IsDraft =recp75.p75IsDraft, TempGUID = BO.Code.Bas.GetGuid(),p28ID=recp75.p28ID, DateSupply=(DateTime) recp76.p76DateSupply,DateMaturity=(DateTime) recp76.p76DateMaturity,InvoiceText1=recp76.p76Name };
            recP91.DateP31_From = lisP31.Min(p => p.p31Date);
            recP91.DateP31_Until = lisP31.Max(p => p.p31Date);
            if (recP91.p28ID > 0)
            {
                var recP28 = _mother.p28ContactBL.Load(recP91.p28ID);
                recP91.p92ID = recP28.p92ID;
            }
            if (recP91.p28ID == 0 && recp75.p41ID>0)
            {
                var recP41 = _mother.p41ProjectBL.Load(recp75.p41ID);
                recP91.p92ID = recP41.p92ID;
                if (recP41.p28ID_Billing > 0)
                {
                    recP91.p28ID = recP41.p28ID_Billing;
                }
                else
                {
                    recP91.p28ID = recP41.p28ID_Client;
                }
            }
            foreach(var c in lisP31)
            {
                _mother.p85TempboxBL.Save(new BO.p85Tempbox() { p85GUID = recP91.TempGUID, p85Prefix = "p31", p85DataPID = c.pid,p85IsDeleted=false });
            }

            var intP91ID = _mother.p91InvoiceBL.Create(recP91);
            if (intP91ID > 0)
            {
                _mother.p75InvoiceRecurrenceBL.UpdateGeneratedInvoice_NewInstance(recp76.p76ID, intP91ID);
                
            }
            

            return intP91ID;

        }

    }
}
