

namespace BL
{
    public enum p31TableEnum
    {
        p31Worksheet = 0,
        p31Worksheet_Log = 1,
        p31Worksheet_Temp = 2,
        p31Worksheet_Del = 3
    }
    public interface Ip31WorksheetBL
    {
        public BO.p31Worksheet Load(int pid, bool isdelrecord = false);
        public BO.p31Worksheet LoadByExternalPID(string externalpid);

        public BO.p31Worksheet LoadTempRecord(int pid, string guidTempData);
        public BO.p31Worksheet LoadMyLastCreated(bool bolLoadTheSameProjectTypeIfNoData, int intP41ID = 0, int intP34ID = 0);
        public BO.p31WorksheetEntryInput CovertRec2Input(BO.p31Worksheet rec, bool time_entry_by_minutes);
        public int SaveOrigRecord(BO.p31WorksheetEntryInput rec, BO.p33IdENUM p33ID, List<BO.FreeFieldInput> lisFF);
        public List<BO.p31ValidateBeforeSave> ValidateBeforeSaveOrigRecord(BO.p31WorksheetEntryInput rec);
        public IEnumerable<BO.p31Worksheet> GetList(BO.myQueryP31 mq);
        public void UpdateExternalCode(int pid, string strCodeValue);
        public bool UpdateText(int pid, string strText);
        public bool UpdateTempText(int pid, string strText, string tempguid);
        public BO.p31RecDisposition InhaleRecDisposition(BO.p31Worksheet rec);
        public bool UpdateInvoice(int p91id, List<BO.p31WorksheetInvoiceChange> lisP31, List<BO.FreeFieldInput> lisFFI);
        public bool RemoveFromInvoice(int p91id, List<int> p31ids);
        public bool RemoveFromApprove(List<int> p31ids);
        public bool Move2Invoice(int p91id_dest, int p31id);
        //public bool Move2Bin(bool move2bin, List<int> p31ids);
        public bool Move2ExcludeBillingFlag(int flag, List<int> p31ids);
        public bool Append2Invoice(int p91id_dest, List<int> pids);
        public bool ValidateVatRate(double vatrate, int p41id, DateTime d, int j27id);
        public IEnumerable<BO.p31WorksheetTimelineDay> GetList_TimelineDays(List<int> j02ids, DateTime d1, DateTime d2, int j70id, int p31statequery, int j72id_query);
        public IEnumerable<BO.p31QuickStat> GetList_QuickStat(string groupbyfield, string record_prefix, int record_pid, DateTime? d1, DateTime? d2, string approve_guid);
        public BO.p31Rate LoadRate(BO.p51TypeFlagENUM flag, DateTime d, int j02id, int p41id, int p32id, int p54id);
        public BO.p31BudgetCompare LoadBudgetCompare(string prefix, int pid, BO.Model.TimesheetCostRateEnum nss, double dblSimulacniNakladovaSazba, double dblProcentoZFakSazby);
        public BO.p72IdENUM Get_p72ID_NonBillableWork(int p31id);
        public bool Save_Approving(BO.p31WorksheetApproveInput c, bool istempdata, bool isvalidatebefore);
        public bool Validate_Before_Save_Approving(BO.p31WorksheetApproveInput c, bool istempdata);
        public void DeleteTempRecord(string guid, int p31id);
        public int Move2Project(int p41id_dest, List<int> pids,int p32id_dest=0);
        public int Recalc(List<int> pids,int ratetype); //1: fakturační sazby, 2: nákladové sazby, 3 režijní sazby, 4 efektivní sazba
        public bool Move_To_Del(string p85guid, int intYear = 0);
        public bool Move_From_Del(string p85guid, int intYear = 0);


    }
    class p31WorksheetBL : BaseBL, Ip31WorksheetBL
    {

        public p31WorksheetBL(BL.Factory mother) : base(mother)
        {

        }




        private string GetSQL1(string strAppend = null, int intTopcRecs = 0, bool istestcloud = false, p31TableEnum tab = p31TableEnum.p31Worksheet)
        {
            if (intTopcRecs > 0)
            {
                sb("SELECT TOP " + intTopcRecs.ToString());
            }
            else
            {
                sb("SELECT");
            }
            sb(" a.p41ID,a.j02ID,a.p32ID,a.o23ID,a.j02ID_Owner,a.j02ID_ApprovedBy,a.p31Code,a.p70ID,a.p71ID,a.p72ID_AfterApprove,a.p72ID_AfterTrimming,a.j27ID_Billing_Orig,a.j27ID_Billing_Invoiced,a.j27ID_Billing_Invoiced_Domestic,a.j27ID_Internal,a.p91ID,a.p31Date,a.p31DateUntil,a.p31HoursEntryFlag,a.p31Approved_When,a.p31Text,a.p31Value_Orig");

            sb(",CASE WHEN a.p91ID IS NULL AND a.p31ExcludeBillingFlag IS NULL AND p32x.p32IsBillable=1 then case when a.p71ID=1 then p31Hours_Approved_Billing else p31Hours_Orig end END as Nevyuctovano_Hodiny");
            sb(",CASE WHEN a.p91ID IS NULL AND a.p31ExcludeBillingFlag IS NULL then case when a.p71ID=1 then p31Amount_WithoutVat_Approved else p31Amount_WithoutVat_Orig end END as Nevyuctovano_Castka");
            sb(",CASE WHEN a.p91ID IS NULL AND a.p71ID=1 AND a.p72ID_AfterApprove=7 THEN a.p31Hours_Approved_Billing END as Schvaleno_Hodiny_Pozdeji");
            sb(",a.p31Value_Trimmed,a.p31Value_Approved_Billing,a.p31Value_Approved_Internal,a.p31Value_Invoiced,a.p31Amount_WithoutVat_Orig,a.p31Amount_WithVat_Orig,a.p31Amount_Vat_Orig,a.p31VatRate_Orig,a.p31Amount_WithoutVat_FixedCurrency,a.p31Amount_WithoutVat_Invoiced,a.p31Amount_WithVat_Invoiced,a.p31Amount_Vat_Invoiced,a.p31VatRate_Invoiced,a.p31Amount_WithoutVat_Invoiced_Domestic,a.p31Amount_WithVat_Invoiced_Domestic,a.p31Amount_Vat_Invoiced_Domestic,a.p31Minutes_Orig,a.p31Minutes_Trimmed,a.p31Minutes_Approved_Billing,a.p31Minutes_Approved_Internal,a.p31Minutes_Invoiced");
            sb(",a.p31Hours_Orig,a.p31Hours_Trimmed,a.p31Hours_Approved_Billing,a.p31Hours_Approved_Internal,a.p31Hours_Invoiced,a.p31HHMM_Orig,a.p31HHMM_Trimmed,a.p31HHMM_Approved_Billing,a.p31HHMM_Approved_Internal,a.p31HHMM_Invoiced,a.p31Rate_Billing_Orig,a.p31Rate_Internal_Orig,a.p31Rate_Billing_Approved,a.p31Rate_Internal_Approved,a.p31Rate_Billing_Invoiced,a.p31Amount_WithoutVat_Approved,a.p31Amount_WithVat_Approved,a.p31Amount_Vat_Approved,a.p31VatRate_Approved,a.p31ExchangeRate_Domestic,a.p31ExchangeRate_Invoice,a.p31Amount_Internal");

            sb(",a.p31DateTimeFrom_Orig,a.p31DateTimeUntil_Orig,a.p31Value_Orig_Entried,a.p31Calc_Pieces,a.p31Calc_PieceAmount,a.p35ID,a.j19ID,a.p49ID");
            sb(",j02.j02LastName,j02.j02FirstName,p32x.p32Name,p32x.p34ID,p32x.p32IsBillable,p32x.p32AbsenceFlag,p32x.p32AbsenceBreakFlag,p32x.p32ManualFeeFlag,p32x.p32Color,p34x.p33ID,p34x.p34Name,p34x.p34IncomeStatementFlag,isnull(p41x.p41NameShort,p41x.p41Name) as p41Name,p41x.p41Code,p41x.p28ID_Client,p28Client.p28Name as ClientName,p28Client.p28ShortName");   //02owner.j02Name as Owner
            sb(",p91x.p91Code,p91x.p91IsDraft,p70.p70Name,p71.p71Name,p72approve.p72Name as approve_p72Name,j27billing_orig.j27Code as j27Code_Billing_Orig,p32x.p95ID,p95.p95Name,a.p31ApprovingLevel,a.p31Value_FixPrice,a.p28ID_Supplier,a.p31IsInvoiceManual,a.p28ID_ContactPerson");  //supplier.p28Name as SupplierName

            sb(",a.p31MarginHidden,a.p31MarginTransparent,a.p31PostRecipient,a.p31PostCode,a.p31PostFlag,a.p31TimerTimestamp,a.p31ExternalCode");
            sb(",p41x.p42ID,p42.p42Name,a.p31BitStream,a.p56ID,p56.p56Name,p56.p56Code,a.p31ExcludeBillingFlag,a.p54ID,a.b05ID_Last,a.p40ID_Source,a.p40ID_FixPrice,p40.p40Name,a.p51ID_BillingRate,a.p51ID_CostRate,a.p31TextInternal,p41x.p41BillingFlag,a.p31Ordinary");
            sb(",a.p31MasterID");
            sb(",");
            sb(_db.GetSQL1_Ocas("p31"));

            switch (tab)
            {
                case p31TableEnum.p31Worksheet_Temp:
                    sb(" FROM p31Worksheet_Temp a"); break;
                case p31TableEnum.p31Worksheet_Log:
                    sb(",a.RowID FROM p31Worksheet_Log a"); break;
                case p31TableEnum.p31Worksheet_Del:
                    sb(",1 as IsDeleted FROM p31Worksheet_Del a"); break;
                default:
                    sb(" FROM p31Worksheet a"); break;
            }

            //if (istemprecord)
            //{                
            //    sb(" FROM p31Worksheet_Temp a");
            //}
            //else
            //{
            //    if (ischangelog)
            //    {
            //        sb(",a.RowID FROM p31Worksheet_Log a");
            //    }
            //    else
            //    {
            //        sb(" FROM p31Worksheet a");
            //    }                               
            //}            

            sb(" INNER JOIN j02User j02 ON a.j02ID=j02.j02ID INNER JOIN p32Activity p32x ON a.p32ID=p32x.p32ID");
            sb($" INNER JOIN p34ActivityGroup p34x ON p32x.p34ID=p34x.p34ID AND p34x.x01ID={_mother.CurrentUser.x01ID} INNER JOIN p41Project p41x ON a.p41ID=p41x.p41ID INNER JOIN p42ProjectType p42 ON p41x.p42ID=p42.p42ID");
            sb(" LEFT OUTER JOIN p28Contact p28Client ON p41x.p28ID_Client=p28Client.p28ID");
            sb(" LEFT OUTER JOIN p56Task p56 ON a.p56ID=p56.p56ID LEFT OUTER JOIN p91Invoice p91x ON a.p91ID=p91x.p91ID");
            sb(" LEFT OUTER JOIN p70BillingStatus p70 ON a.p70ID=p70.p70ID LEFT OUTER JOIN p71ApproveStatus p71 ON a.p71ID=p71.p71ID LEFT OUTER JOIN p72PreBillingStatus p72approve ON a.p72ID_AfterApprove=p72approve.p72ID");
            //sb(" LEFT OUTER JOIN j02User j02owner ON a.j02ID_Owner=j02owner.j02ID");    //LEFT OUTER JOIN p28Contact supplier ON a.p28ID_Supplier=supplier.p28ID

            sb(" LEFT OUTER JOIN j27Currency j27billing_orig ON a.j27ID_Billing_Orig=j27billing_orig.j27ID");
            sb(" LEFT OUTER JOIN p95InvoiceRow p95 ON p32x.p95ID=p95.p95ID");
            sb(" LEFT OUTER JOIN p40WorkSheet_Recurrence p40 ON a.p40ID_FixPrice=p40.p40ID");

            if (istestcloud)
            {
                sb(this.AppendCloudQuery(strAppend, "p34x.x01ID"));
            }
            else
            {
                sb(strAppend);
            }


            return sbret();
        }
        public BO.p31Worksheet Load(int pid, bool isdelrecord = false)
        {
            if (pid == 0)
            {
                return null;
            }
            BO.p31Worksheet rec = null;
            if (!isdelrecord)
            {
                rec = _db.Load<BO.p31Worksheet>(GetSQL1(" WHERE a.p31ID=@pid"), new { pid = pid });
            }
            if (rec == null || isdelrecord)
            {
                return _db.Load<BO.p31Worksheet>(GetSQL1(" WHERE a.p31ID=@pid", 0, false, p31TableEnum.p31Worksheet_Del), new { pid = pid });   //záznam v archivu
            }

            return rec;


        }


        public BO.p31Worksheet LoadByExternalPID(string externalpid)
        {
            return _db.Load<BO.p31Worksheet>(GetSQL1(" WHERE a.p31ExternalPID=@externalpid", 0, _mother.CurrentUser.IsHostingModeTotalCloud), new { externalpid = externalpid });
        }
        public BO.p31Worksheet LoadTempRecord(int pid, string guidTempData)
        {
            return _db.Load<BO.p31Worksheet>(GetSQL1(" WHERE a.p31ID=@p31id AND a.p31Guid=@guid", 0, false, p31TableEnum.p31Worksheet_Temp), new { p31id = pid, guid = guidTempData });
        }
        public BO.p31Worksheet LoadMyLastCreated(bool bolLoadTheSameProjectTypeIfNoData, int intP41ID = 0, int intP34ID = 0)
        {
            string s = " WHERE a.j02ID_Owner=@j02id";
            var pars = new Dapper.DynamicParameters();
            pars.Add("j02id", _mother.CurrentUser.pid, System.Data.DbType.Int32);
            if (intP41ID > 0)
            {
                s += " AND a.p41ID=@p41id";
                pars.Add("p41id", intP41ID, System.Data.DbType.Int32);
            }
            if (intP34ID > 0)
            {
                s += " AND p32x.p34ID=@p34id";
                pars.Add("p34id", intP34ID, System.Data.DbType.Int32);
            }

            s += " ORDER BY a.p31ID DESC";
            var rec = _db.Load<BO.p31Worksheet>(GetSQL1(s, 1), pars);
            if (bolLoadTheSameProjectTypeIfNoData && rec == null && intP41ID > 0)
            {
                s = " WHERE a.j02ID_Owner=@j02id AND p41x.p42ID IN (SELECT p42ID FROM p41Project WHERE p41ID=@p41id) ORDER BY a.p31ID DESC";
                rec = _db.Load<BO.p31Worksheet>(GetSQL1(s, 1), pars);
            }

            return rec;
        }



        public IEnumerable<BO.p31Worksheet> GetList(BO.myQueryP31 mq)
        {
            p31TableEnum tab = p31TableEnum.p31Worksheet;
            if (mq.ischangelog) tab = p31TableEnum.p31Worksheet_Log;
            if (mq.IsRecordValid == false) tab = p31TableEnum.p31Worksheet_Del;
            if (mq.tempguid != null) tab = p31TableEnum.p31Worksheet_Temp;

            DL.FinalSqlCommand fq = DL.basQuery.GetFinalSql(GetSQL1(null, mq.TopRecordsOnly, false, tab), mq, _mother.CurrentUser);

            return _db.GetList<BO.p31Worksheet>(fq.FinalSql, fq.Parameters);
        }

        public void UpdateExternalCode(int pid, string strCodeValue)
        {
            BL.Code.p31Support.UpdateExternalCode(_mother, _db, pid, strCodeValue);
        }


        public int SaveOrigRecord(BO.p31WorksheetEntryInput rec, BO.p33IdENUM p33ID, List<BO.FreeFieldInput> lisFF)
        {
            if (rec.p41ID == 0)
            {
                rec.SetError("Chybí projekt.");
                this.AddMessage(rec.ErrorMessage); return 0;
            }
            if (rec.j02ID == 0)
            {
                rec.SetError("Chybí osobní profil.");
                this.AddMessage(rec.ErrorMessage); return 0;
            }
            if (rec.p34ID == 0)
            {
                rec.SetError("Chybí sešit aktivity.");
                this.AddMessage(rec.ErrorMessage); return 0;
            }
            if (rec.p32ID == 0)
            {
                var recP34 = _mother.p34ActivityGroupBL.Load(rec.p34ID);
                if (recP34.p34ActivityEntryFlag == BO.p34ActivityEntryFlagENUM.AktivitaJePovinna)
                {
                    rec.SetError(String.Format("Sešit [{0}] vyžaduje zadat aktivitu.", recP34.p34Name));
                    this.AddMessage(rec.ErrorMessage); return 0;
                }
                //zkusit najít výchozí systémovou aktivitu, pokud se aktivita nemá zadávat - je to ta první aktivita v pořadí                
                var lisP32 = _mother.p32ActivityBL.GetList(new BO.myQueryP32() { p34id = rec.p34ID, IsRecordValid = true }).OrderBy(p => p.p32Ordinary);
                if (lisP32.Count() > 0)
                {
                    rec.p32ID = lisP32.First().pid;
                }


            }
            if (rec.pid == 0 && string.IsNullOrEmpty(rec.Value_Orig_Entried))
            {
                rec.Value_Orig_Entried = rec.Value_Orig;
            }


            var vlds = BL.Code.p31Support.ValidateBeforeSaveOrigRecord(_mother, _db, rec);

            foreach (BO.p31ValidateBeforeSave vld in vlds)
            {
                if (!string.IsNullOrEmpty(vld.ErrorMessage))
                {
                    this.AddMessage(vld.ErrorMessage); return 0;
                }
                if (rec.p72ID_AfterTrimming != BO.p72IdENUM._NotSpecified)
                {
                    if (!rec.ValidateTrimming(rec.p72ID_AfterTrimming, rec.Value_Trimmed, vld.p33ID))
                    {
                        this.AddMessage(rec.ErrorMessage); return 0;
                    }
                }

                switch (vld.p33ID)
                {
                    case BO.p33IdENUM.Cas:
                        if (!rec.ValidateEntryTime(vld.Round2Minutes, vld.p31Date))
                        {
                            this.AddMessage(rec.ErrorMessage); return 0;
                        }

                        if (rec.p72ID_AfterTrimming == BO.p72IdENUM.Fakturovat && rec.p31Hours_Orig == rec.p31Hours_Trimmed)
                        {

                        }
                        if (rec.p72ID_AfterTrimming == BO.p72IdENUM.Fakturovat && rec.p32ID > 0)
                        {

                        }
                        break;
                    case BO.p33IdENUM.Kusovnik:
                        if (!rec.ValidateEntryKusovnik())
                        {
                            this.AddMessage(rec.ErrorMessage); return 0;
                        }
                        break;
                    case BO.p33IdENUM.PenizeBezDPH:
                        if (rec.j27ID_Billing_Orig == 0)
                        {
                            rec.SetError("Chybí měna.");
                            this.AddMessage(rec.ErrorMessage); return 0;
                        }
                        if (rec.Amount_WithoutVat_Orig == 0)
                        {
                            rec.SetError("Částka nesmí být NULA.");
                            this.AddMessage(rec.ErrorMessage); return 0;
                        }
                        rec.RecalcEntryAmount(rec.Amount_WithoutVat_Orig, vld.VatRate); //dopočítat částku vč. DPH
                        rec.VatRate_Orig = vld.VatRate;
                        break;
                    case BO.p33IdENUM.PenizeVcDPHRozpisu:
                        if (rec.j27ID_Billing_Orig == 0)
                        {
                            rec.SetError("Chybí měna.");
                            this.AddMessage(rec.ErrorMessage); return 0;
                        }
                        if (rec.Amount_WithoutVat_Orig == 0 && rec.Amount_WithVat_Orig == 0)
                        {
                            rec.SetError("Částka nesmí být NULA.");
                            this.AddMessage(rec.ErrorMessage); return 0;
                        }
                        rec.SetAmounts();
                        if (rec.VatRate_Orig != 0 && (rec.Amount_WithVat_Orig == 0 || rec.Amount_Vat_Orig == 0))
                        {
                            rec.RecalcEntryAmount(rec.Amount_WithoutVat_Orig, rec.VatRate_Orig);
                        }
                        if (Math.Abs((rec.p31Amount_WithoutVat_Orig + rec.p31Amount_Vat_Orig) - rec.p31Amount_WithVat_Orig) > 0.02)
                        {
                            rec.SetError(string.Format("Součet základu a částky DPH se liší od celkové částky vč. DPH! Rozdíl: {0}", rec.p31Amount_WithoutVat_Orig + rec.p31Amount_Vat_Orig - rec.p31Amount_WithVat_Orig));
                            this.AddMessage(rec.ErrorMessage);
                            return 0;
                        }

                        break;
                }
            }



            int intPID = BL.Code.p31Support.SaveOrigRecord(_mother, _db, rec, p33ID, lisFF);


            return intPID;
        }

        public List<BO.p31ValidateBeforeSave> ValidateBeforeSaveOrigRecord(BO.p31WorksheetEntryInput rec)
        {
            return BL.Code.p31Support.ValidateBeforeSaveOrigRecord(_mother, _db, rec);
        }

        public bool UpdateInvoice(int p91id, List<BO.p31WorksheetInvoiceChange> lisP31, List<BO.FreeFieldInput> lisFFI)
        {
            if (lisP31.Count() == 0)
            {
                this.AddMessage("Na vstupu chybí úkon."); return false;
            }
            if (lisP31.Any(p => p.p70ID == BO.p70IdENUM.Nic))
            {
                this.AddMessage("Na vstupu je minimálně jeden úkon, který postrádá fakturační status."); return false;
            }
            if (lisP31.Any(p => p.p70ID == BO.p70IdENUM.Vyfakturovano && p.p32ManualFeeFlag == 0 && (p.p33ID == BO.p33IdENUM.Cas || p.p33ID == BO.p33IdENUM.Kusovnik) && (p.InvoiceRate == 0 || p.InvoiceValue == 0)))
            {
                this.AddMessage("Na vstupu je minimálně jeden časový úkon pro fakturaci s nulovou sazbou nebo nulovým počtem hodin."); return false;
            }
            if (lisP31.Any(p => p.p70ID == BO.p70IdENUM.Vyfakturovano && p.p32ManualFeeFlag == 1 && p.p33ID == BO.p33IdENUM.Cas && (p.ManualFee == 0 || p.InvoiceValue == 0)))
            {
                this.AddMessage("Na vstupu je minimálně jeden časový úkon pro fakturaci s nulovým pevným honorářem nebo s nulovým počtem hodin."); return false;
            }
            if (lisP31.Any(p => p.p70ID == BO.p70IdENUM.Vyfakturovano && (p.p33ID == BO.p33IdENUM.PenizeBezDPH || p.p33ID == BO.p33IdENUM.PenizeVcDPHRozpisu) && p.InvoiceValue == 0))
            {
                this.AddMessage("Na vstupu je minimálně jeden peněžní úkon pro fakturaci s nulovou částkou."); return false;
            }
            //var recP91 = _mother.p91InvoiceBL.Load(p91id);

            //foreach(var c in lisP31.Where(p => p.p70ID==BO.p70IdENUM.Vyfakturovano && p.InvoiceVatRate > 0))
            //{
            //    var recP31 = Load(c.p31ID);
            //    if (!ValidateVatRate(c.InvoiceVatRate, recP31.p41ID, recP91.p91DateSupply, recP91.j27ID))
            //    {
            //        this.AddMessageTranslated(string.Format("DPH sazba {0}% není platná pro projekt ({1}), datum ({2}) a měnu ({3}).",c.InvoiceVatRate,recP31.p41Name,recP91.p91DateSupply,recP91.j27Code));
            //        return false;
            //    }
            //}
            var guid = BO.Code.Bas.GetGuid();
            foreach (var c in lisP31)
            {
                var ctemp = new BO.p85Tempbox()
                {
                    p85GUID = guid
                    ,
                    p85Prefix = "p31"
                    ,
                    p85DataPID = c.p31ID
                    ,
                    p85OtherKey1 = (int)c.p70ID
                    ,
                    p85Message = c.TextUpdate
                    ,
                    p85FreeFloat01 = c.InvoiceValue
                    ,
                    p85FreeFloat02 = c.InvoiceRate
                    ,
                    p85FreeFloat03 = c.InvoiceVatRate
                    ,
                    p85FreeNumber04 = c.InvoiceVatAmount
                    ,
                    p85FreeNumber01 = c.FixPriceValue * 10000000
                    ,
                    p85FreeBoolean01 = c.p31IsInvoiceManual
                    ,
                    p85FreeText08 = c.TextInternalUpdate
                    ,
                    p85FreeText09 = c.p31Code
                    ,
                    p85OtherKey2=c.p31Ordinary
                };
                _mother.p85TempboxBL.Save(ctemp);

                var pars = new Dapper.DynamicParameters();
                pars.Add("p91id", p91id, System.Data.DbType.Int32);
                pars.Add("guid", guid, System.Data.DbType.String);
                pars.Add("j02id_sys", _db.CurrentUser.pid, System.Data.DbType.Int32);
                pars.Add("err_ret", "", System.Data.DbType.String, System.Data.ParameterDirection.Output);

                if (_db.RunSp("p31_change_invoice", ref pars, true) != "1")
                {
                    return false;
                }
                if (lisFFI != null)
                {
                    DL.BAS.SaveFreeFields(_db, c.p31ID, lisFFI);
                }

            }



            return true;
        }

        public bool RemoveFromApprove(List<int> p31ids)
        {
            if (p31ids == null || p31ids.Count() == 0)
            {
                this.AddMessage("Na vstupu chybí úkon."); return false;
            }
            var guid = BO.Code.Bas.GetGuid();
            _db.RunSql("INSERT INTO p85TempBox(p85GUID,p85Prefix,p85DataPID) SELECT @guid,'p31',p31ID FROM p31Worksheet WHERE p31ID IN (" + string.Join(",", p31ids) + ")", new { guid = guid });

            var pars = new Dapper.DynamicParameters();
            pars.Add("guid", guid, System.Data.DbType.String);
            pars.Add("j02id_sys", _db.CurrentUser.pid, System.Data.DbType.Int32);
            pars.Add("err_ret", "", System.Data.DbType.String, System.Data.ParameterDirection.Output);

            if (_db.RunSp("p31_remove_approve", ref pars, true) != "1")
            {
                return false;
            }
            return true;
        }

        public bool RemoveFromInvoice(int p91id, List<int> p31ids)
        {
            if (p31ids == null || p31ids.Count() == 0)
            {
                this.AddMessage("Na vstupu chybí úkon."); return false;
            }
            if (p91id == 0)
            {
                this.AddMessageTranslated("p91id missing"); return false;
            }
            var lis = GetList(new BO.myQueryP31() { p91id = p91id });
            if (lis.Count() <= p31ids.Count())
            {
                this.AddMessage("Vyúčtování musí obsahovat minimálně jednu položku. Nemůžete vyjmout všechny úkony z vyúčtování."); return false;
            }

            var guid = BO.Code.Bas.GetGuid();
            _db.RunSql("INSERT INTO p85TempBox(p85GUID,p85Prefix,p85DataPID) SELECT @guid,'p31',p31ID FROM p31Worksheet WHERE p31ID IN (" + string.Join(",", p31ids) + ")", new { guid = guid });

            var pars = new Dapper.DynamicParameters();
            pars.Add("p91id", p91id, System.Data.DbType.Int32);
            pars.Add("guid", guid, System.Data.DbType.String);
            pars.Add("j02id_sys", _db.CurrentUser.pid, System.Data.DbType.Int32);
            pars.Add("err_ret", "", System.Data.DbType.String, System.Data.ParameterDirection.Output);

            if (_db.RunSp("p31_remove_invoice", ref pars, true) != "1")
            {
                return false;
            }
            return true;
        }
        public int Move2Project(int p41id_dest, List<int> pids, int p32id_dest = 0)
        {
            if (pids == null || pids.Count() == 0)
            {
                this.AddMessage("Na vstupu chybí úkony."); return 0;
            }
            int x = 0;

            using (var sc = new System.Transactions.TransactionScope())
            {
                foreach (int p31id in pids)
                {
                    string s = "UPDATE p31Worksheet SET p41ID=@p41id,p31DateUpdate=getdate(),p31UserUpdate=@login WHERE p31ID=@pid";
                    if (p32id_dest > 0)
                    {
                        s = "UPDATE p31Worksheet SET p41ID=@p41id,p32ID=@p32id,p31DateUpdate=getdate(),p31UserUpdate=@login WHERE p31ID=@pid";
                    }
                    if (_db.RunSql(s, new { p41id = p41id_dest, login = _mother.CurrentUser.j02Login, pid = p31id,p32id=p32id_dest }))
                    {
                        var pars = new Dapper.DynamicParameters();
                        {
                            pars.Add("p31id", p31id, System.Data.DbType.Int32);
                            pars.Add("j02id_sys", _mother.CurrentUser.pid, System.Data.DbType.Int32);
                        }
                        if (_db.RunSp("p31_aftersave", ref pars, false) == "1")
                        {
                            x += 1;
                        }
                    }
                }
                sc.Complete();
            }

            return x;
        }
        public int Recalc(List<int> pids, int intRateType)
        {
            if (pids == null || pids.Count() == 0)
            {
                this.AddMessage("Na vstupu chybí úkony."); return 0;
            }
            int x = 0;
            foreach (int p31id in pids)
            {
                var pars = new Dapper.DynamicParameters();
                {
                    pars.Add("p31id", p31id, System.Data.DbType.Int32);
                    pars.Add("j02id_sys", _mother.CurrentUser.pid, System.Data.DbType.Int32);
                    pars.Add("ratetype", intRateType, System.Data.DbType.Int32);
                }
                if (_db.RunSp("p31_recalc_rate", ref pars, false) == "1")
                {
                    x += 1;
                }
            }
            return x;
        }
        public bool Move2Invoice(int p91id_dest, int p31id)
        {
            if (p31id == 0)
            {
                this.AddMessage("Na vstupu chybí úkon."); return false;
            }
            if (p91id_dest == 0)
            {
                this.AddMessageTranslated("Chybí cílové vyúčtování (faktura)."); return false;
            }

            var pars = new Dapper.DynamicParameters();
            pars.Add("p31id", p31id, System.Data.DbType.Int32);
            pars.Add("p91id_dest", p91id_dest, System.Data.DbType.Int32);
            pars.Add("j02id_sys", _db.CurrentUser.pid, System.Data.DbType.Int32);
            pars.Add("err_ret", "", System.Data.DbType.String, System.Data.ParameterDirection.Output);

            if (_db.RunSp("p31_move_to_another_invoice", ref pars, true) != "1")
            {
                return false;
            }
            return true;
        }
        public bool Append2Invoice(int p91id_dest, List<int> pids)
        {
            if (p91id_dest == 0)
            {
                this.AddMessage("Chybí cílové vyúčtování (faktura)."); return false;
            }
            if (pids == null || pids.Count() == 0)
            {
                this.AddMessageTranslated("Na vstupu chybí úkony."); return false;
            }
            var guid = BO.Code.Bas.GetGuid();
            _db.RunSql("INSERT INTO p85TempBox(p85GUID,p85Prefix,p85DataPID) SELECT @guid,'p31',p31ID FROM p31Worksheet WHERE p31ID IN (" + string.Join(",", pids) + ")", new { guid = guid });

            var pars = new Dapper.DynamicParameters();
            pars.Add("p91id", p91id_dest, System.Data.DbType.Int32);
            pars.Add("guid", guid, System.Data.DbType.String);
            pars.Add("j02id_sys", _db.CurrentUser.pid, System.Data.DbType.Int32);
            pars.Add("err_ret", "", System.Data.DbType.String, System.Data.ParameterDirection.Output);

            if (_db.RunSp("p31_append_invoice", ref pars, true) != "1")
            {
                return false;
            }
            return true;
        }
        
        public bool Move2ExcludeBillingFlag(int flag, List<int> p31ids)
        {
            if (flag > 0)
            {
                return _db.RunSql($"UPDATE p31Worksheet SET p31ExcludeBillingFlag={flag} WHERE p91ID IS NULL AND p31ID IN (" + string.Join(",", p31ids) + ")");
            }
            else
            {
                return _db.RunSql("UPDATE p31Worksheet SET p31ExcludeBillingFlag=NULL,p71ID=NULL,p72ID_AfterApprove=NULL WHERE p91ID IS NULL AND p31ID IN (" + string.Join(",", p31ids) + ")");
            }

        }

        public BO.p31RecDisposition InhaleRecDisposition(BO.p31Worksheet rec)
        {
            var c = new BO.p31RecDisposition();

            if (rec.isdeleted)
            {
                c.LockedReasonMessage = "Záznam byl přesunutý do archivu.";

            }

            var pars = new Dapper.DynamicParameters();
            pars.Add("j02id_sys", _db.CurrentUser.pid, System.Data.DbType.Int32);
            pars.Add("pid", rec.pid, System.Data.DbType.Int32);
            pars.Add("record_disposition", null, System.Data.DbType.Int32, System.Data.ParameterDirection.Output);
            pars.Add("record_state", null, System.Data.DbType.Int32, System.Data.ParameterDirection.Output);
            pars.Add("msg_locked", null, System.Data.DbType.String, System.Data.ParameterDirection.Output, 1000);

            if (_db.RunSp("p31_inhale_disposition", ref pars, false) == "1")
            {
                c.RecordState = (BO.p31RecordState)pars.Get<Int32>("record_state");
                c.LockedReasonMessage = pars.Get<string>("msg_locked");

                switch (pars.Get<Int32>("record_disposition"))
                {
                    case 1:
                        c.ReadAccess = true; break;
                    case 2:
                        c.OwnerAccess = true; c.ReadAccess = true; break;
                    case 3:
                        c.CanApprove = true; c.ReadAccess = true; break;
                    case 4:
                        c.CanApproveAndEdit = true; c.ReadAccess = true; c.OwnerAccess = true; break;
                }

                if (c.RecordState == BO.p31RecordState.ExcludedFromBilling)
                {
                    c.CanApprove = false; c.CanApproveAndEdit = false;   //vyloučeno z vyúčtování
                }

            }
            if (_mother.CurrentUser.IsAdmin)
            {
                c.OwnerAccess = true; c.ReadAccess = true;
            }

            if (c.RecordState == BO.p31RecordState.Editing && !c.OwnerAccess)
            {
                c.OwnerAccess = _mother.CurrentUser.TestPermission(BO.PermValEnum.GR_P31_Owner); //uživatel má právo vlastníka všech worksheet záznamů v db
            }




            return c;
        }

        public bool ValidateVatRate(double vatrate, int p41id, DateTime d, int j27id)
        {
            var ret = _db.Load<BO.GetBool>("select dbo.p31_testvat(@vatrate,@p41id,@dat,@j27id) as Value", new { vatrate = vatrate, p41id = p41id, dat = d, j27id = j27id });
            return ret.Value;
        }
        public BO.p31BudgetCompare LoadBudgetCompare(string prefix, int pid,BO.Model.TimesheetCostRateEnum nss, double dblSimulacniNakladovaSazba,double dblProcentoZFakSazby)
        {

            sb("SELECT MIN(a.j27ID_Billing_Orig) as j27ID_Min,MAX(a.j27ID_Billing_Orig) as j27ID_Max");
            sb(",sum(a.p31Hours_Orig) as Hodiny");
            sb(",sum(case when p32.p32IsBillable=1 then a.p31Hours_Orig end) as HodinyFa");
            sb(",sum(case when p32.p32IsBillable=0 then a.p31Hours_Orig end) as HodinyNefa");
            switch (nss)
            {                
                case BO.Model.TimesheetCostRateEnum.RezijniSazba:
                    sb(",SUM(case when p34.p33ID=1 then a.p31Hours_Orig*a.p31Rate_Overhead end) as Honorar_Naklad");
                    sb(",SUM(case when a.p91ID IS NOT NULL AND p34.p33ID=1 then a.p31Hours_Orig*a.p31Rate_Overhead end) as Vyuctovano_Honorar_Naklad");
                    break;
                case BO.Model.TimesheetCostRateEnum.SimulacniSazbaProjekt:
                    sb(",SUM(case when p34.p33ID=1 then a.p31Hours_Orig*p41.p41Plan_Internal_Rate end) as Honorar_Naklad");
                    sb(",SUM(case when a.p91ID IS NOT NULL AND p34.p33ID=1 then a.p31Hours_Orig*p41.p41Plan_Internal_Rate end) as Vyuctovano_Honorar_Naklad");
                    break;
                case BO.Model.TimesheetCostRateEnum.SimulacniSazbaUzivatel:
                    sb(",SUM(case when p34.p33ID=1 then a.p31Hours_Orig*j02.j02Plan_Internal_Rate end) as Honorar_Naklad");
                    sb(",SUM(case when a.p91ID IS NOT NULL AND p34.p33ID=1 then a.p31Hours_Orig*j02.j02Plan_Internal_Rate end) as Vyuctovano_Honorar_Naklad");
                    break;
                case BO.Model.TimesheetCostRateEnum.ProcentoFakturacniSazby:
                    sb($",SUM(case when p34.p33ID=1 then a.p31Hours_Orig*a.p31Rate_Billing_Orig*{BO.Code.Bas.GN(dblProcentoZFakSazby)}/100 end) as Honorar_Naklad");
                    sb($",SUM(case when a.p91ID IS NOT NULL AND p34.p33ID=1 then a.p31Hours_Orig*a.p31Rate_Billing_Orig*{BO.Code.Bas.GN(dblProcentoZFakSazby)}/100 end) as Vyuctovano_Honorar_Naklad");
                    break;
                default:
                    sb(",SUM(case when p34.p33ID=1 then a.p31Hours_Orig*a.p31Rate_Internal_Orig end) as Honorar_Naklad");
                    sb(",SUM(case when a.p91ID IS NOT NULL AND p34.p33ID=1 then a.p31Hours_Orig*a.p31Rate_Internal_Orig end) as Vyuctovano_Honorar_Naklad");
                    break;
            }
            
            sb(",SUM(case when p34.p33ID=1 then a.p31Amount_WithoutVat_Orig end) as Honorar");

            sb(",SUM(case when p34.p33ID=3 then a.p31Value_Orig end) as Kusovnik");
            sb(",SUM(case when p34.p33ID=3 then a.p31Value_Orig*a.p31Rate_Internal_Orig end) as Kusovnik_Naklad");
            sb(",SUM(case when p34.p33ID=3 then a.p31Amount_WithoutVat_Orig end) as Kusovnik_Honorar");            

            sb(",sum(case when p34.p33ID IN (2,5) AND p34.p34IncomeStatementFlag=1 then a.p31Amount_WithoutVat_Orig end) as Vydaje");
            sb(",sum(case when p32.p32IsBillable=1 AND p34.p33ID IN (2,5) AND p34.p34IncomeStatementFlag=1 then a.p31Amount_WithoutVat_Orig end) as VydajeFa");
            sb(",sum(case when p32.p32IsBillable=0 AND p34.p33ID IN (2,5) AND p34.p34IncomeStatementFlag=1 then a.p31Amount_WithoutVat_Orig end) as VydajeNefa");
            sb(",sum(case when p34.p33ID IN (2,5) AND p34.p34IncomeStatementFlag=2 then a.p31Amount_WithoutVat_Orig end) as Odmeny");
            
            
            sb(",SUM(case when a.p91ID IS NOT NULL then a.p31Amount_WithoutVat_Invoiced_Domestic end) as Vyuctovano_BezDph");
            sb(",SUM(case when a.p91ID IS NOT NULL AND p34.p33ID=1 then a.p31Amount_WithoutVat_Invoiced_Domestic end) as Vyuctovano_Honorar");
            sb(",SUM(case when a.p91ID IS NOT NULL AND p34.p33ID=1 then a.p31Hours_Invoiced end) as Vyuctovano_Hodiny");
            

            sb(",sum(case when a.p91ID IS NOT NULL AND p34.p33ID IN (2,5) AND p34.p34IncomeStatementFlag=1 then a.p31Amount_WithoutVat_Invoiced_Domestic end) as Vyuctovano_Vydaje");
            sb(",sum(case when a.p91ID IS NOT NULL AND p34.p33ID IN (2,5) AND p34.p34IncomeStatementFlag=2 then a.p31Amount_WithoutVat_Invoiced_Domestic end) as Vyuctovano_Odmeny");
            
            sb(",SUM(case when a.p91ID IS NOT NULL AND p34.p33ID IN (2,5) AND p34.p34IncomeStatementFlag=1 then a.p31Amount_WithoutVat_Orig end) as Vyuctovano_Vydaje_Vykazane");
            sb(",SUM(case when a.p91ID IS NOT NULL AND p34.p33ID=1 then a.p31Hours_Orig end) as Vyuctovano_Hodiny_Vykazane");
            sb(",SUM(case when a.p91ID IS NOT NULL AND p34.p33ID=1 AND a.p70ID=6 then a.p31Hours_Orig end) as Vyuctovano_Hodiny_6");
            sb(",SUM(case when a.p91ID IS NOT NULL AND p34.p33ID=1 AND a.p70ID=2 then a.p31Hours_Orig end) as Vyuctovano_Hodiny_2");
            sb(",SUM(case when a.p91ID IS NOT NULL AND p34.p33ID=1 AND a.p70ID=3 then a.p31Hours_Orig end) as Vyuctovano_Hodiny_3");

            
            sb(",SUM(case when a.p91ID IS NOT NULL AND p34.p33ID=3 then a.p31Value_Orig*a.p31Rate_Internal_Orig end) as Vyuctovano_Kusovnik_Naklad");
            sb(",sum(case when a.p91ID IS NOT NULL AND p34.p33ID=3 then a.p31Amount_WithoutVat_Invoiced_Domestic end) as Vyuctovano_Kusovnik");


            sb(" FROM p31Worksheet a");
            sb(" INNER JOIN p32Activity p32 ON a.p32ID=p32.p32ID");
            sb(" INNER JOIN p34ActivityGroup p34 ON p32.p34ID=p34.p34ID");
            sb(" INNER JOIN p41Project p41 ON a.p41ID=p41.p41ID");
            sb(" INNER JOIN j02User j02 ON a.j02ID=j02.j02ID");
            switch (prefix)
            {
                case "p41":
                    sb(" WHERE a.p41ID=@pid");
                    break;
                case "p28":
                    sb(" WHERE p41.p28ID_Client=@pid AND GETDATE() BETWEEN p41.p41ValidFrom AND p41.p41ValidUntil");
                    break;
                case "p56":
                    sb(" WHERE a.p56ID=@pid");
                    break;
                case "p91":
                    sb(" WHERE a.p91ID=@pid");
                    break;

            }

            var ret = _db.Load<BO.p31BudgetCompare>(sbret(), new { pid = pid });

            if (nss==BO.Model.TimesheetCostRateEnum.SimulacniSazbaRucne)
            {
                ret.Honorar_Naklad = ret.Hodiny * dblSimulacniNakladovaSazba;
                ret.Vyuctovano_Honorar_Naklad = ret.Vyuctovano_Hodiny_Vykazane * dblSimulacniNakladovaSazba;
            }

            return ret;

        }
        public IEnumerable<BO.p31QuickStat> GetList_QuickStat(string groupby, string record_prefix, int record_pid, DateTime? d1, DateTime? d2, string approve_guid)
        {
            switch (groupby)
            {
                case "p41":
                    sb("SELECT a.p41ID as groupby_pid,min(isnull(p41x.p41NameShort,p41x.p41Name)) as groupby_alias"); break;
                case "p28":
                    sb("SELECT p41x.p28ID_Client as groupby_pid,min(p28Client.p28Name) as groupby_alias"); break;
                case "j02":
                    sb("SELECT a.j02ID as groupby_pid,min(j02.j02LastName+' '+j02.j02FirstName) as groupby_alias"); break;
                case "p32":
                    sb("SELECT a.p32ID as groupby_pid,min(p32x.p32Name) as groupby_alias"); break;
                case "p34":
                    sb("SELECT p32x.p34ID as groupby_pid,min(p34x.p34Name) as groupby_alias"); break;
                case "p95":
                    sb("SELECT p32x.p95ID as groupby_pid,min(p95.p95Name) as groupby_alias"); break;
                case "j27":
                    sb("SELECT a.j27ID_Billing_Orig as groupby_pid,min(j27billing_orig.j27Code) as groupby_alias"); break;
                case "p91":
                    sb("SELECT a.p91ID as groupby_pid,min(p91.p91Code) as groupby_alias"); break;
                case "p56":
                    sb("SELECT a.p56ID as groupby_pid,min(p56.p56Name) as groupby_alias"); break;
                case "p31rate_billing_orig":
                    sb("SELECT a.p31Rate_Billing_Orig as groupby_pid,min(convert(varchar(10),a.p31Rate_Billing_Orig)) as groupby_alias"); break;

            }
            sb(",a.j27ID_Billing_Orig as j27code_orig,min(j27billing_orig.j27Code) as j27code_orig");
            //sb(",a.j27ID_Billing_Invoiced_Domestic as j27id_vyfa,min(j27billing_vyfa.j27Code) as j27code_vyfa");
            sb(",sum(a.p31Hours_Invoiced) as hvyf");
            sb(",sum(a.p31Hours_Orig) as hvyk");
            sb(",sum(a.p31Amount_WithoutVat_Orig) as bezdph_vyka");
            sb(",sum(a.p31Amount_WithoutVat_Invoiced_Domestic) as bezdph_vyfa");
            sb(",COUNT(a.p31ID) as pocet");
            sb(",sum(case when a.p70ID=6 then a.p31Hours_Orig end) as hvyf6");
            sb(",sum(case when a.p70ID IN (2,3) then a.p31Hours_Orig end) as hvyf23");

            sb(",sum(CASE WHEN p41x.p41BillingFlag<99 AND a.p91ID IS NULL AND a.p31ExcludeBillingFlag IS NULL AND p32x.p32IsBillable=1 then case when a.p71ID=1 then a.p31Hours_Approved_Billing else a.p31Hours_Orig end END) as nevyuct_h");
            sb(",sum(CASE WHEN p41x.p41BillingFlag<99 AND a.p91ID IS NULL AND a.p31ExcludeBillingFlag IS NULL AND p32x.p32IsBillable=1 then case when a.p71ID=1 then a.p31Amount_WithoutVat_Approved else a.p31Amount_WithoutVat_Orig end END) as nevyuct_castka");
            if (approve_guid != null)
            {
                sb(" FROM p31Worksheet_Temp a");
            }
            else
            {
                sb(" FROM p31Worksheet a");
            }

            sb(" INNER JOIN j02User j02 ON a.j02ID=j02.j02ID INNER JOIN p32Activity p32x ON a.p32ID=p32x.p32ID");
            sb(" INNER JOIN p34ActivityGroup p34x ON p32x.p34ID=p34x.p34ID INNER JOIN p41Project p41x ON a.p41ID=p41x.p41ID");
            sb(" LEFT OUTER JOIN p28Contact p28Client ON p41x.p28ID_Client=p28Client.p28ID");
            sb(" LEFT OUTER JOIN p56Task p56 ON a.p56ID=p56.p56ID LEFT OUTER JOIN p91Invoice p91x ON a.p91ID=p91x.p91ID");
            sb(" LEFT OUTER JOIN p91Invoice p91 ON a.p91ID=p91.p91ID");
            sb(" LEFT OUTER JOIN j27Currency j27billing_orig ON a.j27ID_Billing_Orig=j27billing_orig.j27ID");
            //sb(" LEFT OUTER JOIN j27Currency j27billing_vyfa ON a.j27ID_Billing_Invoiced_Domestic=j27billing_vyfa.j27ID");
            sb(" LEFT OUTER JOIN p95InvoiceRow p95 ON p32x.p95ID=p95.p95ID");

            var mq = new BO.myQueryP31() { global_d1 = d1, global_d2 = d2, MyRecordsDisponible = true, CurrentUser = _mother.CurrentUser, tempguid = approve_guid };

            switch (record_prefix)
            {
                case "p41":
                case "le5":
                    mq.p41id = record_pid; break;
                case "p28":
                    mq.p28id = record_pid; break;
                case "p91":
                    mq.p91id = record_pid; break;
                case "j02":
                    mq.j02id = record_pid; break;
                case "p56":
                    mq.p56id = record_pid; break;
                case "o23":
                    mq.o23id = record_pid; break;
                case "le4":
                    mq.leindex = 4; mq.lepid = record_pid; break;
                case "le3":
                    mq.leindex = 3; mq.lepid = record_pid; break;
                case "le2":
                    mq.leindex = 2; mq.lepid = record_pid; break;
                case "le1":
                    mq.leindex = 1; mq.lepid = record_pid; break;
            }

            switch (groupby)
            {
                case "p41":
                    mq.explicit_sqlgroupby = "a.p41ID,a.j27ID_Billing_Orig"; break;
                case "p28":
                    mq.explicit_sqlgroupby = "p41x.p28ID_Client,a.j27ID_Billing_Orig"; break;
                case "j02":
                    mq.explicit_sqlgroupby = "a.j02ID,a.j27ID_Billing_Orig"; break;
                case "p32":
                    mq.explicit_sqlgroupby = "a.p32ID,a.j27ID_Billing_Orig"; break;
                case "p34":
                    mq.explicit_sqlgroupby = "p32x.p34ID,a.j27ID_Billing_Orig"; break;
                case "p95":
                    mq.explicit_sqlgroupby = "p32x.p95ID,a.j27ID_Billing_Orig"; break;
                case "j27":
                    mq.explicit_sqlgroupby = "a.j27ID_Billing_Orig"; break;
                case "p91":
                    mq.explicit_sqlgroupby = "a.p91ID,a.j27ID_Billing_Orig"; break;
                case "p56":
                    mq.explicit_sqlgroupby = "a.p56ID,a.j27ID_Billing_Orig"; break;
                case "p31rate_billing_orig":
                    mq.explicit_sqlgroupby = "a.p31Rate_Billing_Orig,a.j27ID_Billing_Orig"; break;
            }


            DL.FinalSqlCommand fq = DL.basQuery.GetFinalSql(sbret(), mq, _mother.CurrentUser);

            return _db.GetList<BO.p31QuickStat>(fq.FinalSql, fq.Parameters);

        }
        public IEnumerable<BO.p31WorksheetTimelineDay> GetList_TimelineDays(List<int> j02ids, DateTime d1, DateTime d2, int j70id, int p31statequery, int j72id_query)
        {
            if (j02ids == null || j02ids.Count() == 0) j02ids = new List<int>() { _mother.CurrentUser.pid };
            sb("SELECT a.j02ID,min(j02.j02LastName+' '+j02.j02FirstName) as Person");
            sb(",a.p31Date,sum(a.p31Hours_Orig) as Hours,sum(case when p32x.p32IsBillable=1 then a.p31Hours_Orig end) as Hours_Billable,sum(case when p32x.p32IsBillable=0 then a.p31Hours_Orig end) as Hours_NonBillable,count(case when p34x.p33id in (2,5) then 1 end) as Moneys,count(case when p34x.p33id=3 then 1 end) as Pieces");
            sb(",convert(varchar(10),a.p31Date,104) as p31DateString,max(p32x.p32Color) as p32Color");
            sb(" FROM p31Worksheet a");
            sb(" INNER JOIN j02User j02 ON a.j02ID=j02.j02ID INNER JOIN p32Activity p32x ON a.p32ID=p32x.p32ID");
            sb(" INNER JOIN p34ActivityGroup p34x ON p32x.p34ID=p34x.p34ID INNER JOIN p41Project p41x ON a.p41ID=p41x.p41ID");
            sb(" LEFT OUTER JOIN p91Invoice p91x ON a.p91ID=p91x.p91ID");

            //sb(" WHERE a.p31Date BETWEEN @d1 AND @d2 AND GETDATE() BETWEEN j02.j02ValidFrom AND j02.j02ValidUntil");
            //sb($" AND a.j02ID IN ({string.Join(",", j02ids)})");

            var mq = new BO.myQueryP31() { MyRecordsDisponible = true, global_d1 = d1, global_d2 = d2, j02ids = j02ids, CurrentUser = _mother.CurrentUser };

            if (p31statequery > 0)
            {
                mq.p31statequery = p31statequery;
            }
            if (j72id_query > 0)
            {
                mq.lisJ73 = _mother.j72TheGridTemplateBL.GetList_j73(j72id_query, "p31", 0);
            }

            sb($" WHERE {mq.ParseSqlWhere()}");

            sb(" GROUP BY a.j02ID, a.p31Date");
            sb(" ORDER BY min(j02.j02LastName),min(j02.j02FirstName)");

            return _db.GetList<BO.p31WorksheetTimelineDay>(sbret(), new { d1 = d1, d2 = d2, j02id_query = _mother.CurrentUser.pid });
        }

        public BO.p31WorksheetEntryInput CovertRec2Input(BO.p31Worksheet rec, bool time_entry_by_minutes)
        {
            if (rec == null) return null;

            var c = new BO.p31WorksheetEntryInput() { pid = rec.pid, j02ID = rec.j02ID, p41ID = rec.p41ID, p34ID = rec.p34ID, p32ID = rec.p32ID, o23ID = rec.o23ID, p56ID = rec.p56ID };
            c.Addp31Date(rec.p31Date); c.p31Text = rec.p31Text; c.p31TextInternal = rec.p31TextInternal;
            c.Value_Orig = rec.p31Value_Orig.ToString(); c.p31HoursEntryflag = rec.p31HoursEntryFlag;
            if (time_entry_by_minutes && (c.p31HoursEntryflag == BO.p31HoursEntryFlagENUM.Hodiny || c.p31HoursEntryflag == BO.p31HoursEntryFlagENUM.Minuty))
            {
                c.Value_Orig = rec.p31Minutes_Orig.ToString();  //hodiny zobrazovat v minutách
            }
            c.TimeFrom = rec.TimeFrom; c.TimeUntil = rec.TimeUntil;
            c.p31MasterID = rec.p31MasterID;

            c.j27ID_Billing_Orig = rec.j27ID_Billing_Orig;
            c.Amount_Vat_Orig = rec.p31Amount_Vat_Orig; c.Amount_WithoutVat_Orig = rec.p31Amount_WithoutVat_Orig; c.Amount_WithVat_Orig = rec.p31Amount_WithVat_Orig;
            c.VatRate_Orig = rec.p31VatRate_Orig;

            c.ValidUntil = rec.ValidUntil; c.ValidFrom = rec.ValidFrom;
            c.DateInsert = rec.DateInsert; c.DateUpdate = rec.DateUpdate;
            c.UserInsert = rec.UserInsert; c.UserUpdate = rec.UserUpdate;
            c.p35ID = rec.p35ID;
            c.p49ID = rec.p49ID;
            c.p28ID_Supplier = rec.p28ID_Supplier;

            if (rec.p33ID == BO.p33IdENUM.PenizeBezDPH || rec.p33ID == BO.p33IdENUM.PenizeVcDPHRozpisu)
            {
                c.p31Code = rec.p31Code;
                c.j19ID = rec.j19ID;
                c.p31MarginHidden = rec.p31MarginHidden; c.p31MarginTransparent = rec.p31MarginTransparent;                
                c.p31PostCode = rec.p31PostCode;
                c.p31PostRecipient = rec.p31PostRecipient;
                c.p31PostFlag = rec.p31PostFlag;
                c.p31Calc_PieceAmount = rec.p31Calc_PieceAmount;
                c.p31Calc_Pieces = rec.p31Calc_Pieces;

            }
            else
            {
                c.p54ID = rec.p54ID;
                c.p40ID_FixPrice = rec.p40ID_FixPrice;
                c.ManualFee = rec.p31Amount_WithoutVat_Orig;    //pro jistotu, že by aktivita byla nastavena na zadávání pevného honoráře
            }

            c.p72ID_AfterTrimming = rec.p72ID_AfterTrimming;
            if (rec.p72ID_AfterTrimming != BO.p72IdENUM._NotSpecified)
            {
                c.Value_Trimmed = rec.p31Value_Trimmed.ToString();
                if (rec.IsRecommendedHHMM_Trimmed())
                {
                    c.p31HHMM_Trimmed = BO.Code.Time.ShowAsHHMM(c.Value_Trimmed);
                    c.Value_Trimmed = c.p31HHMM_Trimmed;
                }
            }
            c.p31BitStream = rec.p31BitStream;
            c.p28ID_ContactPerson = rec.p28ID_ContactPerson;

            return c;

        }


        public BO.p31Rate LoadRate(BO.p51TypeFlagENUM flag, DateTime d, int j02id, int p41id, int p32id, int p54id)
        {
            var ret = new BO.p31Rate() { flag = flag };

            var pars = new Dapper.DynamicParameters();
            pars.Add("date_rate", d, System.Data.DbType.DateTime);
            pars.Add("p51TypeFlag", (int)flag, System.Data.DbType.Int32);
            pars.Add("p41id", p41id, System.Data.DbType.Int32);
            pars.Add("j02id", j02id, System.Data.DbType.Int32);
            pars.Add("p32id", p32id, System.Data.DbType.Int32);
            pars.Add("p54id", p54id, System.Data.DbType.Int32);
            pars.Add("p51id", null, System.Data.DbType.Int32);
            pars.Add("ret_j27id", null, System.Data.DbType.Int32, System.Data.ParameterDirection.Output);
            pars.Add("ret_rate", null, System.Data.DbType.Double, System.Data.ParameterDirection.Output);
            pars.Add("ret_p51id", null, System.Data.DbType.Int32, System.Data.ParameterDirection.Output);

            if (_db.RunSp("p31_getrate_tu", ref pars, false) == "1")
            {
                int? intJ27ID = pars.Get<int?>("ret_j27id");
                if (intJ27ID == null)
                {
                    ret.j27ID = _mother.Lic.j27ID; ;
                }
                else
                {
                    ret.j27ID = pars.Get<int>("ret_j27id");
                }

                ret.Value = pars.Get<double>("ret_rate");

                int? intP51ID = pars.Get<int?>("ret_p51id");
                if (intP51ID == null)
                {
                    ret.p51ID = 0;
                }
                else
                {
                    ret.p51ID = (int)intP51ID;
                }
                if (ret.j27ID > 0) ret.j27Code = _mother.FBL.LoadCurrencyByID(ret.j27ID).j27Code;
            }

            return ret;
        }

        public BO.p72IdENUM Get_p72ID_NonBillableWork(int p31id)
        {
            var pars = new Dapper.DynamicParameters();
            pars.Add("p31id", p31id, System.Data.DbType.Int32);
            pars.Add("ret_p72id", null, System.Data.DbType.Int32, System.Data.ParameterDirection.Output);

            if (_db.RunSp("p31_inhale_p72id_nonbillable", ref pars, false) == "1")
            {
                return (BO.p72IdENUM)pars.Get<int>("ret_p72id");

            }
            else
            {
                return BO.p72IdENUM.SkrytyOdpis;
            }
        }

        public bool Validate_Before_Save_Approving(BO.p31WorksheetApproveInput c, bool istempdata)
        {
            if (c.p71id == BO.p71IdENUM.Nic) return true;   //vrátit do rozpracovanosti
            if (c.p71id == BO.p71IdENUM.Neschvaleno) return true;   //nahozeno jako neschváleno
            if (istempdata && string.IsNullOrEmpty(c.Guid))
            {
                this.AddMessage("Pro temp data musí být předán GUID_TempData."); return false;
            }
            if (!istempdata && !string.IsNullOrEmpty(c.Guid))
            {
                this.AddMessage("Je předáván GUID_TempData, ale bolTempData=false."); return false;
            }
            if (c.p71id == BO.p71IdENUM.Schvaleno && c.p72id == BO.p72IdENUM._NotSpecified)
            {
                this.AddMessage("Schválený úkon musí mít přiřazen některý z fakturačních statusů."); return false;
            }

            if (c.p72id == BO.p72IdENUM.Fakturovat || c.p72id == BO.p72IdENUM.FakturovatPozdeji)
            {
                if (c.p33ID == BO.p33IdENUM.Cas && c.Value_Approved_Billing == 0)
                {
                    this.AddMessage("Hodiny k fakturaci nesmí být NULA."); return false;
                }
                if (c.p33ID == BO.p33IdENUM.Kusovnik && c.Value_Approved_Billing == 0)
                {
                    this.AddMessage("Počet kusů k fakturaci nesmí být NULA."); return false;
                }
                if ((c.p33ID == BO.p33IdENUM.Cas || c.p33ID == BO.p33IdENUM.Kusovnik) && c.Rate_Billing_Approved == 0 && c.p32ManualFeeFlag == 0)
                {

                    this.AddMessage("Sazba k fakturaci nesmí být NULA."); return false;
                }
                if ((c.p33ID == BO.p33IdENUM.PenizeBezDPH || c.p33ID == BO.p33IdENUM.PenizeVcDPHRozpisu) && c.Value_Approved_Billing == 0)
                {
                    this.AddMessage("Částka bez DPH nesmí být NULA."); return false;
                }

            }

            return true;
        }

        public void DeleteTempRecord(string guid, int p31id)
        {
            _db.RunSql("DELETE FROM p31Worksheet_Temp where p31Guid=@guid AND p31ID=@pid", new { guid = guid, pid = p31id });
        }



        public bool Save_Approving(BO.p31WorksheetApproveInput c, bool istempdata, bool isvalidatebefore)
        {
            if (isvalidatebefore)
            {
                if (!Validate_Before_Save_Approving(c, istempdata)) return false;
            }



            var pars = new Dapper.DynamicParameters();
            if (!string.IsNullOrEmpty(c.Guid))
            {
                pars.Add("guid", c.Guid, System.Data.DbType.String);
            }
            pars.Add("p31id", c.p31ID, System.Data.DbType.Int32);
            pars.Add("j02id_sys", _mother.CurrentUser.pid, System.Data.DbType.Int32);
            if (c.p71id == BO.p71IdENUM.Nic)
            {
                pars.Add("p71id", null, System.Data.DbType.Int32);
                pars.Add("p72id", null, System.Data.DbType.Int32);
            }
            else
            {
                pars.Add("p71id", (int)c.p71id, System.Data.DbType.Int32);
                if (c.p72id == BO.p72IdENUM._NotSpecified)
                {
                    pars.Add("p72id", null, System.Data.DbType.Int32);
                }
                else
                {
                    pars.Add("p72id", (int)c.p72id, System.Data.DbType.Int32);
                }
            }

            pars.Add("approvingset", null, System.Data.DbType.String);
            pars.Add("value_approved_internal", c.Value_Approved_Internal, System.Data.DbType.Double);
            pars.Add("value_approved_billing", c.Value_Approved_Billing, System.Data.DbType.Double);
            pars.Add("rate_billing_approved", c.Rate_Billing_Approved, System.Data.DbType.Double);
            pars.Add("rate_internal_approved", c.Rate_Internal_Approved, System.Data.DbType.Double);
            pars.Add("p31Text", c.p31Text, System.Data.DbType.String);
            pars.Add("vatrate_approved", c.VatRate_Approved, System.Data.DbType.Double);
            pars.Add("dat_p31date", c.p31Date, System.Data.DbType.DateTime);
            pars.Add("approving_level", c.p31ApprovingLevel, System.Data.DbType.Int32);
            pars.Add("value_fixprice", c.p31Value_FixPrice, System.Data.DbType.Double);
            pars.Add("manualfee_approved", c.ManualFee_Approved, System.Data.DbType.Double);
            pars.Add("err_ret", "", System.Data.DbType.String, System.Data.ParameterDirection.Output);

            if (!string.IsNullOrEmpty(c.Guid))
            {
                //TEMP - dočasná data
                if (_db.RunSp("p31_save_approving_temp", ref pars, true) == "1")
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                //uložení schvalování do ostrých dat
                if (_db.RunSp("p31_save_approving", ref pars, true) == "1")
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }

        }


        public bool UpdateText(int pid, string strText)
        {
            if (string.IsNullOrEmpty(strText))
            {
                this.AddMessage("Text úkonu je prázdný.");
                return false;
            }
            return _db.RunSql("UPDATE p31Worksheet set p31Text=@s WHERE p31ID=@pid", new { s = strText, pid = pid });
        }
        public bool UpdateTempText(int pid, string strText, string tempguid)
        {
            if (string.IsNullOrEmpty(strText))
            {
                this.AddMessage("Text úkonu je prázdný.");
                return false;
            }
            if (string.IsNullOrEmpty(tempguid))
            {
                this.AddMessage("Chybí guid.");
                return false;
            }
            return _db.RunSql("UPDATE p31Worksheet_Temp set p31Text=@s WHERE p31ID=@pid AND p31Guid=@guid", new { s = strText, pid = pid, guid = tempguid });
        }
        

        public bool Move_To_Del(string p85guid, int intYear)
        {
            if (intYear > 0)
            {
                return _db.RunSql($"exec dbo.p31_move_to_del_year {_mother.CurrentUser.pid},'{intYear}',null");
            }
            else
            {
                return _db.RunSql($"exec dbo.p31_move_to_del {_mother.CurrentUser.pid},'{p85guid}',null");
            }

        }
        public bool Move_From_Del(string p85guid, int intYear)
        {
            if (intYear > 0)
            {
                return _db.RunSql($"exec dbo.p31_move_from_del_year {_mother.CurrentUser.pid},'{intYear}',null");
            }
            else
            {
                return _db.RunSql($"exec dbo.p31_move_from_del {_mother.CurrentUser.pid},'{p85guid}',null");
            }

        }



    }
}
