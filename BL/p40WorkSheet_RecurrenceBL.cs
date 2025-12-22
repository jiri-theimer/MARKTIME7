

namespace BL
{
    public interface Ip40WorkSheet_RecurrenceBL
    {
        public BO.p40WorkSheet_Recurrence Load(int pid);
        public BO.p39WorkSheet_Recurrence_Plan LoadP39(int p39id);
        public BO.p39WorkSheet_Recurrence_Plan LoadP39_FirstWaiting(int p40id, DateTime datNow);
        public IEnumerable<BO.p40WorkSheet_Recurrence> GetList(BO.myQueryP40 mq);
        public IEnumerable<BO.p39WorkSheet_Recurrence_Plan> GetList_p39(int p40id, int days_inhistory = 0, int p41id = 0);
        public IEnumerable<BO.p39WorkSheet_Recurrence_Plan> GetList_p39_waiting_on_generate(DateTime d1, DateTime d2,List<int> p40ids=null);
        public int Save(BO.p40WorkSheet_Recurrence rec);
        public BO.p31WorksheetEntryInput Convert_p39_to_p31(BO.p39WorkSheet_Recurrence_Plan recP39);
        public int Generate_Recurrence_Instance(BO.p39WorkSheet_Recurrence_Plan c);
        public int Generate_Clear(BO.p39WorkSheet_Recurrence_Plan c);

    }

    class p40WorkSheet_RecurrenceBL : BaseBL, Ip40WorkSheet_RecurrenceBL
    {
        public p40WorkSheet_RecurrenceBL(BL.Factory mother) : base(mother)
        {

        }

        private string GetSQL1(string strAppend = null)
        {
            sb("SELECT a.*,");
            sb("p32x.p32Name,p34x.p34Name,j02x.j02Name,j27x.j27Code,isnull(p41x.p41NameShort,p41x.p41Name) as Project,p28x.p28Name as Client,p34x.p33ID,cerpano.Hodiny as Cerpano_Hodiny,cerpano.Honorar as Cerpano_Honorar,");
            sb(_db.GetSQL1_Ocas("p40"));
            sb(" FROM p40WorkSheet_Recurrence a INNER JOIN p34ActivityGroup p34x ON a.p34ID=p34x.p34ID INNER JOIN p41Project p41x ON a.p41ID=p41x.p41ID");
            sb(" INNER JOIN p32Activity p32x ON a.p32ID=p32x.p32ID INNER JOIN j02User j02x ON a.j02ID=j02x.j02ID");
            sb(" LEFT OUTER JOIN j27Currency j27x ON a.j27ID=j27x.j27ID LEFT OUTER JOIN p28Contact p28x ON p41x.p28ID_Client=p28x.p28ID");
            sb(" LEFT OUTER JOIN dbo.view_p40_cerpano cerpano ON a.p40ID=cerpano.p40ID");

            sb(strAppend);
            return sbret();
        }
        public BO.p40WorkSheet_Recurrence Load(int pid)
        {
            return _db.Load<BO.p40WorkSheet_Recurrence>(GetSQL1(" WHERE a.p40ID=@pid"), new { pid = pid });
        }

        public IEnumerable<BO.p40WorkSheet_Recurrence> GetList(BO.myQueryP40 mq)
        {
            DL.FinalSqlCommand fq = DL.basQuery.GetFinalSql(GetSQL1(), mq, _mother.CurrentUser);
            return _db.GetList<BO.p40WorkSheet_Recurrence>(fq.FinalSql, fq.Parameters);
           
        }

        public BO.p39WorkSheet_Recurrence_Plan LoadP39_FirstWaiting(int p40id,DateTime datNow)
        {
            string s = "select top 1 a.*,p31.p31Text,p31.p31DateInsert,p31.p31Date FROM p39WorkSheet_Recurrence_Plan a LEFT OUTER JOIN p31Worksheet p31 ON a.p31ID_NewInstance=p31.p31ID WHERE a.p40ID=@p40id AND a.p31ID_NewInstance IS NULL AND a.p39DateCreate BETWEEN dateadd(day,-2,@dat) AND @dat ORDER BY a.p39DateCreate";
            return _db.Load<BO.p39WorkSheet_Recurrence_Plan>(s, new { p40id = p40id,dat=datNow });
        }

        public BO.p39WorkSheet_Recurrence_Plan LoadP39(int p39id)
        {
            string s = "select top 1 a.*,p31.p31Text,p31.p31DateInsert,p31.p31Date FROM p39WorkSheet_Recurrence_Plan a LEFT OUTER JOIN p31Worksheet p31 ON a.p31ID_NewInstance=p31.p31ID WHERE a.p39ID=@p39id";
            return _db.Load<BO.p39WorkSheet_Recurrence_Plan>(s, new { p39id = p39id });
        }

        public IEnumerable<BO.p39WorkSheet_Recurrence_Plan> GetList_p39(int p40id,int days_inhistory = 0,int p41id=0)
        {
            sb("select a.*,p31.p31Text,p31.p31DateInsert,p31.p31Date");           
            sb(" FROM p39WorkSheet_Recurrence_Plan a LEFT OUTER JOIN p31Worksheet p31 ON a.p31ID_NewInstance=p31.p31ID");
            
            if (p41id > 0)
            {
                sb(" INNER JOIN p40WorkSheet_Recurrence p40 ON a.p40ID=p40.p40ID WHERE p40.p41ID=@p41id");
            }
            if (p40id > 0)
            {
                sb(" WHERE a.p40ID=@p40id");
            }


            if (days_inhistory > 0)
            {
                //sb($" AND p39Date BETWEEN DATEADD(MONTH,{-1 * days_plusminus},GETDATE()) AND DATEADD(MONTH,{days_plusminus},GETDATE())");
                sb($" AND (a.p39Date >= DATEADD(MONTH,{-1 * days_inhistory},GETDATE()) OR a.p31ID_NewInstance IS NULL AND a.p39DateCreate<GETDATE())");
            }
            return _db.GetList<BO.p39WorkSheet_Recurrence_Plan>(sbret(), new { p40id = p40id,p41id=p41id });
        }

        public IEnumerable<BO.p39WorkSheet_Recurrence_Plan> GetList_p39_waiting_on_generate(DateTime d1,DateTime d2, List<int> p40ids = null)
        {
            sb("select a.*,p31.p31Text,p31.p31DateInsert,p31.p31Date,p41.p41Name,p28.p28Name as Client,p40.p40Value");
            sb(" FROM p39WorkSheet_Recurrence_Plan a INNER JOIN p40WorkSheet_Recurrence p40 ON a.p40ID=p40.p40ID LEFT OUTER JOIN p31Worksheet p31 ON a.p31ID_NewInstance=p31.p31ID");
            sb(" INNER JOIN p41Project p41 ON p40.p41ID=p41.p41ID LEFT OUTER JOIN p28Contact p28 ON p41.p28ID_Client=p28.p28ID");
            sb(" WHERE GETDATE() BETWEEN p40.p40ValidFrom AND p40.p40ValidUntil AND GETDATE() BETWEEN p41.p41ValidFrom AND p41.p41ValidUntil AND a.p31ID_NewInstance IS NULL AND a.p39DateCreate BETWEEN @d1 AND @d2");
            if (p40ids !=null && p40ids.Count() > 0)
            {
                sb($" AND a.p40ID IN ({string.Join(",",p40ids)})");
            }

            return _db.GetList<BO.p39WorkSheet_Recurrence_Plan>(sbret(), new { d1 = d1, d2 = d2 });
        }

        public int Generate_Clear(BO.p39WorkSheet_Recurrence_Plan c)
        {
            _db.RunSql("UPDATE p39WorkSheet_Recurrence_Plan set p39ErrorMessage_NewInstance=null, p31ID_NewInstance=null WHERE p39ID=@p39id", new { p39id = c.p39ID });
            return 1;
        }
        public int Generate_Recurrence_Instance(BO.p39WorkSheet_Recurrence_Plan c)  //vrací p31id vygenerovaného úkonu
        {
            var recP31 = Convert_p39_to_p31(c);
            var vlds = _mother.p31WorksheetBL.ValidateBeforeSaveOrigRecord(recP31);
            if (string.IsNullOrEmpty(vlds.First().ErrorMessage))
            {
                var recP34 = _mother.p34ActivityGroupBL.Load(recP31.p34ID);
                int intP31ID = _mother.p31WorksheetBL.SaveOrigRecord(recP31, recP34.p33ID, null);
                if (intP31ID > 0)
                {
                    _db.RunSql("UPDATE p39WorkSheet_Recurrence_Plan set p39ErrorMessage_NewInstance=null, p31ID_NewInstance=@p31id WHERE p39ID=@p39id", new { p31id = intP31ID, p39id = c.p39ID });
                    _db.RunSql("if exists(select j97ID FROM j97BellsLog WHERE j97RecordEntity='p40' AND j97RecordPid=@p40id) DELETE FROM j97BellsLog WHERE j97RecordEntity='p40' AND j97RecordPid=@p40id", new { p40id = c.p40ID });
                }
                
                return intP31ID;
            }
            else
            {
                _db.RunSql("UPDATE p39WorkSheet_Recurrence_Plan set p39ErrorMessage_NewInstance=@err WHERE p39ID=@p39id", new { err = vlds.First().ErrorMessage, p39id = c.p39ID });
                return 0;
            }
        }

        public int Save(BO.p40WorkSheet_Recurrence rec)
        {
            if (!ValidateBeforeSave(rec))
            {
                return 0;
            }
            var p = new DL.Params4Dapper();
            p.AddInt("pid", rec.pid);
            p.AddEnumInt("p40RecurrenceType", rec.p40RecurrenceType, true);
            p.AddInt("p41ID", rec.p41ID, true);
            p.AddInt("p34ID", rec.p34ID, true);
            p.AddInt("p32ID", rec.p32ID, true);
            p.AddInt("j02ID", rec.j02ID, true);
            p.AddInt("j27ID", rec.j27ID, true);
            p.AddEnumInt("x15ID", rec.x15ID, true);
            p.AddString("p40Name", rec.p40Name);
            p.AddString("p40Text", rec.p40Text);
            p.AddString("p40TextInternal", rec.p40TextInternal);
            p.AddDouble("p40Value", rec.p40Value);
            p.AddDouble("p40FreeHours", rec.p40FreeHours);
            p.AddDouble("p40FreeFee", rec.p40FreeFee);
            p.AddDateTime("p40FirstSupplyDate", rec.p40FirstSupplyDate);
            p.AddDateTime("p40LastSupplyDate", rec.p40LastSupplyDate);
            p.AddInt("p40GenerateDayAfterSupply", rec.p40GenerateDayAfterSupply);

            int intPID = _db.SaveRecord("p40WorkSheet_Recurrence", p, rec);
            if (intPID > 0)
            {
                _db.RunSql("exec dbo.p40_aftersave @p40id,@j02id_sys", new { p40id = intPID, j02id_sys = _mother.CurrentUser.pid });
            }

            return intPID;

        }
        private bool ValidateBeforeSave(BO.p40WorkSheet_Recurrence rec)
        {
            if (string.IsNullOrEmpty(rec.p40Name))
            {
                this.AddMessage("Chybí [Název předpisu]."); return false;
            }
            if (string.IsNullOrEmpty(rec.p40Text))
            {
                this.AddMessage("Chybí [Maska textu úkonu]."); return false;
            }
            if (rec.p40FirstSupplyDate == null)
            {
                this.AddMessage("Chybí specifikace prvního rozhodného datumu."); return false;
            }
            if (rec.p40LastSupplyDate == null)
            {
                this.AddMessage("Chybí specifikace posledního rozhodného datumu."); return false;
            }
            if (rec.p40LastSupplyDate.Value < rec.p40FirstSupplyDate.Value)
            {
                this.AddMessage("Období generování je nelogické."); return false;
            }
            int intPocetCyklu = BO.Code.Recurrence.GetPocetCyklu(Convert.ToDateTime(rec.p40FirstSupplyDate), Convert.ToDateTime(rec.p40LastSupplyDate), rec.p40RecurrenceType);
            if (intPocetCyklu > 200)
            {
                this.AddMessageTranslated($"Definujete příliš dlouhé období. Počet cyklů ({intPocetCyklu}) je větší než 200. Snižte poslední rozhodné datum, později ho můžete prodloužit."); return false;
            }
            
            if (rec.p40Value == 0)
            {
                this.AddMessage("[Hodnota] nesmí být NULA."); return false;
            }
            if (rec.p40FreeHours !=0 && rec.p40FreeFee !=0)
            {
                this.AddMessage("Nelze definovat volné hodiny a zároveň objem honoráře zdarma."); return false;
            }
            
            if (rec.p41ID==0)
            {
                this.AddMessage("Chybí [Projekt]."); return false;
            }
            if (rec.p34ID == 0)
            {
                this.AddMessage("Chybí [Sešit]."); return false;
            }
            if (rec.p40FreeHours != 0 || rec.p40FreeFee != 0)
            {
                var recP34 = _mother.p34ActivityGroupBL.Load(rec.p34ID);
                if (recP34.p33ID == BO.p33IdENUM.Cas || recP34.p33ID == BO.p33IdENUM.Kusovnik)
                {
                    this.AddMessage("Volné hodiny nebo honorář zdarma zde nemá smysl."); return false;
                }
                
            }
            if (rec.p32ID == 0)
            {
                this.AddMessage("Chybí [Aktivita]."); return false;
            }
            if (rec.j27ID == 0)
            {
                this.AddMessage("Chybí [Měna]."); return false;
            }
            if (rec.j02ID == 0)
            {
                this.AddMessage("Chybí [Uživatel]."); return false;
            }


            return true;
        }

        public BO.p31WorksheetEntryInput Convert_p39_to_p31(BO.p39WorkSheet_Recurrence_Plan recP39)
        {
            var recP40 = _mother.p40WorkSheet_RecurrenceBL.Load(recP39.p40ID);
            var recP34 = _mother.p34ActivityGroupBL.Load(recP40.p34ID);

            var rec = new BO.p31WorksheetEntryInput() { p40ID_Source=recP39.p40ID, p31Text = recP39.p39Text,p31TextInternal=recP39.p39TextInternal, j02ID = recP40.j02ID, p41ID = recP40.p41ID, p34ID = recP40.p34ID, p32ID = recP40.p32ID };
            rec.Addp31Date(recP39.p39Date);
            rec.Value_Orig = recP40.p40Value.ToString();
            rec.Value_Orig_Entried = recP40.p40Value.ToString();
            
            if (recP34.p33ID == BO.p33IdENUM.PenizeVcDPHRozpisu || recP34.p33ID == BO.p33IdENUM.PenizeBezDPH)
            {
                rec.j27ID_Billing_Orig = recP40.j27ID;
                rec.x15ID = recP40.x15ID;
                rec.VatRate_Orig = _mother.p53VatRateBL.NajdiSazbu(rec.p31Date.First(), rec.x15ID, rec.j27ID_Billing_Orig, _mother.Lic.x01CountryCode);
                rec.Amount_WithoutVat_Orig = recP40.p40Value;
                if (recP34.p33ID == BO.p33IdENUM.PenizeVcDPHRozpisu || recP40.x15ID != BO.x15IdEnum.Nic)
                {
                    rec.Amount_Vat_Orig = rec.VatRate_Orig / 100 * recP40.p40Value;
                    rec.Amount_WithVat_Orig = rec.Amount_WithoutVat_Orig + rec.Amount_Vat_Orig;
                }
                
            }

            if (recP34.p33ID == BO.p33IdENUM.Cas)
            {
                rec.p31HoursEntryflag = BO.p31HoursEntryFlagENUM.Hodiny;
            }

            return rec;
        }
    }
}

