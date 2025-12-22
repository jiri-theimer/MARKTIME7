
using Irony.Parsing;
using System.Data;

namespace BL
{
    public interface Ip91InvoiceBL
    {
        public BO.p91Invoice Load(int pid);
        public BO.p91Invoice LoadByP31ID(int p31id);
        public BO.p91Invoice LoadByCode(string code);
        public BO.p91Invoice LoadByNumericCode(string numeric);
        public BO.p91Invoice LoadMyLastCreated();
        public BO.p91Invoice LoadCreditNote(int p91id);
        public BO.p91Invoice LoadLastOfClient(int p28id);
        public BO.p91RecDisposition InhaleRecDisposition(int pid, BO.p91Invoice rec = null);
        public IEnumerable<BO.p91Invoice> GetList(BO.myQueryP91 mq, bool ischangelog = false);
        public int Update(BO.p91Invoice rec, List<BO.FreeFieldInput> lisFFI, List<BO.x69EntityRole_Assign> lisX69);
        public int Create(BO.p91Create rec);
        public bool ChangeVat(int p91id, int x15id, double newvatrate);
        public int CreateCreditNote(int p91id, int p92id_creditnote);
        public bool ChangeCurrency(int p91id, int j27id);
        public bool ConvertFromDraft(int p91id);
        public bool RecoveryP91Code(int p91id);
        public bool SaveP99(int p91id, int p90id, int p82id, double percentage);
        public bool DeleteP99(int p99id);
        public IEnumerable<BO.p99Invoice_Proforma> GetList_p99(int p90id, int p91id, int p82id);
        public bool RecalcFPR(DateTime d1, DateTime d2, int p51id = 0);
        public int SaveP94(BO.p94Invoice_Payment rec);
        public IEnumerable<BO.p94Invoice_Payment> GetList_p94(int p91id, int p94id = 0);
        public bool DeleteP94(int p94id, int p91id);
        public void ClearExchangeDate(int p91id, bool recalc);
        public IEnumerable<BO.p91_CenovyRozpis> GetList_CenovyRozpis(int p91id, bool bolIncludeRounding, bool bolIncludeProforma, int langindex);
        public bool Delete(int p91id, string guid, int selectedoper);
        public BO.p91InvoiceSum LoadSumRow(int pid);
        public bool UpdateSlovenskyQrCode(int p91id, BO.p92InvoiceType recP92);
        public bool HasDeletedP31Records(int p91id);
        public BO.Integrace.InputInvoice CreateIntegraceRecord(BO.p91Invoice c);
        public bool SaveImprint(string strSourcePdfFullPath, int p91id, string x31code);
        public IEnumerable<BO.p96Imprint> GetList_p96(int p91id, string p96guid);
        public bool Update_Temp_MultiReport(int p91id, string p91Supplier, string p91Supplier_RegID, string p91Supplier_VatID, string p91Supplier_Street, string p91Supplier_City, string p91Supplier_ZIP, string p91Supplier_Country, string p91Supplier_Registration, string p91Supplier_ICDPH_SK,int p93id, string p93Contact, string p93Email,string p93Referent,string p93Signature);
        public int NajdiVychoziJ61ID(BO.p92InvoiceType recP92, BO.p91Invoice recP91, BO.p28Contact recP28);

    }
    class p91InvoiceBL : BaseBL, Ip91InvoiceBL
    {
        public p91InvoiceBL(BL.Factory mother) : base(mother)
        {

        }


        private string GetSQL1(string strAppend = null, int toprec = 0, bool ischangelog = false, bool istestcloud = false)
        {
            if (toprec == 0)
            {
                sb("SELECT ");
            }
            else
            {
                sb($"SELECT TOP {toprec} ");
            }
            sb(_db.GetSQL1_Ocas("p91"));
            sb(",a.p92ID,a.p28ID,a.j27ID,a.j19ID,a.j02ID_Owner,a.p41ID_First,a.p91ID_CreditNoteBind,a.p98ID,a.p63ID,a.p80ID,a.x15ID,a.b02ID,a.p91FixedVatRate,a.p91Code,a.p91IsDraft,a.p91Date,a.p91DateBilled,a.p91DateMaturity,a.p91DateSupply,a.p91DateExchange,a.p91ExchangeRate");

            sb(",a.p91Datep31_From,a.p91Datep31_Until,a.p91Amount_WithoutVat,a.p91Amount_Vat,a.p91Amount_Billed,a.p91Amount_WithVat,a.p91Amount_Debt,a.p91RoundFitAmount,a.p91Text1,a.p91Text2,a.p91ProformaAmount,a.p91ProformaBilledAmount,a.p91Amount_WithoutVat_None,a.p91VatRate_Low,a.p91Amount_WithVat_Low,a.p91Amount_WithoutVat_Low,a.p91Amount_Vat_Low");
            sb(",a.p91VatRate_Standard,a.p91Amount_WithVat_Standard,a.p91Amount_WithoutVat_Standard,a.p91Amount_Vat_Standard,a.p91VatRate_Special,a.p91Amount_WithVat_Special,a.p91Amount_WithoutVat_Special,a.p91Amount_Vat_Special,a.p91Amount_TotalDue");
            sb(",p28client.p28Name,p28client.p28CountryCode,p92x.p92Name,p92x.p93ID,isnull(p41.p41NameShort,p41.p41Name) as p41Name,b02.b02Name,j02owner.j02LastName+' '+j02owner.j02FirstName as Owner,j27.j27Code,p92x.p92TypeFlag,p92x.b01ID,p28client.p28CompanyName");
            sb(",a.p91Client,a.p91Client_RegID,a.p91Client_VatID,a.p91ClientAddress1_Street,a.p91ClientAddress1_City,a.p91ClientAddress1_ZIP,a.p91ClientAddress1_Country,a.p91ClientAddress2,a.p91LockFlag,a.p91Client_ICDPH_SK");
            sb(",a.p91Supplier,a.p91Supplier_RegID,a.p91Supplier_VatID,a.p91Supplier_Street,a.p91Supplier_City,a.p91Supplier_ZIP,a.p91Supplier_Country,a.p91Supplier_Registration,a.p91Supplier_ICDPH_SK,a.p91ClientAddress1_Before");
            sb(",a.p91BitStream,a.p91Guid,p92x.x38ID,a.p91PortalFlag,p92x.p83ID,a.p84ID_Last,a.p91VatCodePohoda");
            if (ischangelog)
            {
                sb(" FROM p91Invoice_Log a");
            }
            else
            {
                sb(" FROM p91Invoice a");
            }

            sb(" INNER JOIN p92InvoiceType p92x ON a.p92ID=p92x.p92ID");
            sb(" INNER JOIN j27Currency j27 ON a.j27ID=j27.j27ID");
            sb(" LEFT OUTER JOIN p41Project p41 ON a.p41ID_First=p41.p41ID");
            sb(" LEFT OUTER JOIN p28Contact p28client ON a.p28ID=p28client.p28ID");
            sb(" LEFT OUTER JOIN b02WorkflowStatus b02 ON a.b02ID=b02.b02ID");
            sb(" LEFT OUTER JOIN j02User j02owner ON a.j02ID_Owner=j02owner.j02ID");

            if (istestcloud)
            {
                sb(this.AppendCloudQuery(strAppend, "p92x.x01ID"));
            }
            else
            {
                sb(strAppend);
            }


            return sbret();
        }
        public BO.p91Invoice Load(int pid)
        {
            return _db.Load<BO.p91Invoice>(GetSQL1(" WHERE a.p91ID=@pid"), new { pid = pid });
        }
        public BO.p91Invoice LoadByP31ID(int p31id)
        {
            return _db.Load<BO.p91Invoice>(GetSQL1(" WHERE a.p91ID IN (SELECT p91ID FROM p31Worksheet WHERE p31ID=@pid)"), new { pid = p31id });
        }
        public BO.p91Invoice LoadByCode(string code)
        {
            return _db.Load<BO.p91Invoice>(GetSQL1(" WHERE a.p91Code LIKE @code", 0, false, _mother.CurrentUser.IsHostingModeTotalCloud), new { code = code });
        }
        public BO.p91Invoice LoadByNumericCode(string numeric)
        {
            return _db.Load<BO.p91Invoice>(GetSQL1(" WHERE convert(bigint,a.p91CodeNumeric) = convert(bigint,dbo.get_only_numerics(@code))", 0, false, _mother.CurrentUser.IsHostingModeTotalCloud), new { code = numeric });

        }
        public BO.p91Invoice LoadMyLastCreated()
        {
            return _db.Load<BO.p91Invoice>(GetSQL1(" WHERE a.j02ID_Owner=@j02id_owner ORDER BY a.p91ID DESC", 1), new { j02id_owner = _mother.CurrentUser.pid });
        }
        public bool HasDeletedP31Records(int p91id)
        {
            if (_db.Load<BO.GetInteger>("if exists(select 1 as Value FROM p31Worksheet_Del WHERE p91ID=@p91id) select 1 as Value else select 0 as Value", new { p91id = p91id }).Value == 1)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        public BO.p91Invoice LoadCreditNote(int p91id)
        {
            sb(GetSQL1());
            sb(" INNER JOIN p91Invoice sourcedoc ON a.p91ID_CreditNoteBind=sourcedoc.p91ID");
            sb(" WHERE sourcedoc.p91ID=@p91id");
            return _db.Load<BO.p91Invoice>(sbret(), new { p91id = p91id });
        }
        public BO.p91Invoice LoadLastOfClient(int p28id)
        {
            return _db.Load<BO.p91Invoice>(GetSQL1(" WHERE a.p28ID=@p28id AND a.p91Amount_WithoutVat>0 ORDER BY a.p91ID DESC", 1), new { p28id = p28id });
        }

        public IEnumerable<BO.p91Invoice> GetList(BO.myQueryP91 mq, bool ischangelog = false)
        {
            if (mq.explicit_orderby == null) mq.explicit_orderby = "a.p91ID DESC";
            DL.FinalSqlCommand fq = DL.basQuery.GetFinalSql(GetSQL1(null, mq.TopRecordsOnly, ischangelog), mq, _mother.CurrentUser);
            return _db.GetList<BO.p91Invoice>(fq.FinalSql, fq.Parameters);
        }

        public bool SaveImprint(string strSourcePdfFullPath, int p91id, string x31code)
        {
            x31code = x31code.Replace(".", "_");
            long intSize = BO.Code.File.GetFileInfo(strSourcePdfFullPath).Length;

            string strDir = $"{DateTime.Now.Year}\\{DateTime.Now.Month}";
            string strFullDir = $"{_mother.UploadFolder}\\p91\\PdfImprint\\{DateTime.Now.Year}\\{DateTime.Now.Month}";
            var rec = _mother.p91InvoiceBL.Load(p91id);
            if (!System.IO.Directory.Exists(strFullDir))
            {
                System.IO.Directory.CreateDirectory(strFullDir);
            }

            string strFileName = $"{rec.p91Code}-{x31code}-{rec.p91Supplier}-{rec.p91Supplier_RegID}-{rec.p91Client}-{rec.p91Client_RegID}";
            strFileName = BO.Code.File.ConvertToSafeFileName(strFileName, 250);
            if (System.IO.File.Exists($"{strFullDir}\\{strFileName}.pdf"))
            {
                System.IO.File.Copy(strSourcePdfFullPath, $"{strFullDir}\\{strFileName}.pdf", true);
                return _db.RunSql("UPDATE p96Imprint SET p96FileSize=@size,p96DateUpdate=GETDATE(),p96UserUpdate=@login,p96Guid=NEWID(),p96Supplier=@supplier,p96Supplier_RegID=@supplier_ico,p96Client=@client,p96Client_RegID=@client_ico,p96Code=@code WHERE p96FileName = @file", new { pid = p91id, file = $"{strFileName}.pdf", dir = strDir, size = intSize, x31code = x31code, login = _mother.CurrentUser.j02Login, supplier = rec.p91Supplier, supplier_ico = rec.p91Supplier_RegID, client = rec.p91Client, client_ico = rec.p91Client_RegID, code = rec.p91Code });
            }
            else
            {
                System.IO.File.Copy(strSourcePdfFullPath, $"{strFullDir}\\{strFileName}.pdf");
                return _db.RunSql("INSERT INTO p96Imprint(p91ID,p96FileName,p96ArchiveFolder,p96FileSize,p96ReportCode,p96DateInsert,p96DateUpdate,p96UserInsert,p96UserUpdate,p96Guid,p96Supplier,p96Supplier_RegID,p96Client,p96Client_RegID,p96Code) VALUES(@pid,@file,@dir,@size,@x31code,GETDATE(),GETDATE(),@login,@login,NEWID(),@supplier,@supplier_ico,@client,@client_ico,@code)", new { pid = p91id, file = $"{strFileName}.pdf", dir = strDir, size = intSize, x31code = x31code, login = _mother.CurrentUser.j02Login, supplier = rec.p91Supplier, supplier_ico = rec.p91Supplier_RegID, client = rec.p91Client, client_ico = rec.p91Client_RegID, code = rec.p91Code });
            }



        }
        public int Create(BO.p91Create rec)
        {
            using (var sc = new System.Transactions.TransactionScope())
            {
                var pars = new Dapper.DynamicParameters();

                pars.Add("guid", rec.TempGUID, DbType.String);
                pars.Add("j02id_sys", _mother.CurrentUser.pid, DbType.Int32);
                pars.Add("p28id", BO.Code.Bas.TestIntAsDbKey(rec.p28ID), DbType.Int32);
                pars.Add("p92id", BO.Code.Bas.TestIntAsDbKey(rec.p92ID), DbType.Int32);
                pars.Add("p91isdraft", rec.IsDraft, DbType.Boolean);
                pars.Add("p91date", rec.DateIssue, DbType.DateTime);
                pars.Add("p91datematurity", rec.DateMaturity, DbType.DateTime);
                pars.Add("p91datesupply", rec.DateSupply, DbType.DateTime);
                pars.Add("p91datep31_from", rec.DateP31_From, DbType.DateTime);
                pars.Add("p91datep31_until", rec.DateP31_Until, DbType.DateTime);
                pars.Add("p91text1", rec.InvoiceText1, DbType.String);
                pars.Add("p91text2", rec.InvoiceText2, DbType.String);

                pars.Add("ret_p91id", 0, DbType.Int32, ParameterDirection.Output);
                pars.Add("err_ret", null, DbType.String, ParameterDirection.Output, 1000);


                if (_db.RunSp("p91_create", ref pars) == "1")
                {
                    int intPID = pars.Get<int>("ret_p91id");

                    var recP92 = _mother.p92InvoiceTypeBL.Load(rec.p92ID);
                    if (recP92.b01ID > 0)
                    {
                        _mother.WorkflowBL.InitWorkflowStatus(intPID, "p91");   //nahodit úvodní workflow stav záznamu
                    }



                    if (recP92.p92QrCodeFlag == BO.p92QrCodeFlagENUM.Slovensko || recP92.p92QrCodeFlag == BO.p92QrCodeFlagENUM.SlovenskoBezSplatnosti)
                    {
                        UpdateSlovenskyQrCode(intPID, recP92);
                    }

                    sc.Complete();
                    return intPID;
                }


                return 0;

            }
        }

        public bool Delete(int p91id, string guid, int selectedoper)
        {
            if (p91id == 0 || string.IsNullOrEmpty(guid) || selectedoper == 0)
            {
                this.AddMessageTranslated("p91id or guid or selectedoper missing"); return false;
            }
            using (var sc = new System.Transactions.TransactionScope())
            {
                _db.RunSql("DELETE FROM p85TempBox WHERE p85GUID=@guid AND p85Prefix='p31'", new { guid = guid });
                _db.RunSql("INSERT INTO p85TempBox(p85GUID,p85Prefix,p85DataPID,p85OtherKey1) SELECT @guid,'p31',p31ID,@oper FROM p31Worksheet WHERE p91ID=@p91id", new { guid = guid, oper = selectedoper, p91id = p91id });

                if (_db.RunSql("exec dbo.p91_delete @j02id_sys,@pid,@guid,@err_ret OUTPUT", new { j02id_sys = _mother.CurrentUser.pid, pid = p91id, guid = guid, err_ret = "" }))
                {
                    sc.Complete();
                    return true;
                }
                else
                {
                    return false;
                }
            }

        }

        public bool UpdateSlovenskyQrCode(int p91id, BO.p92InvoiceType recP92)
        {
            var rec = Load(p91id);
            if (recP92 == null)
            {
                recP92 = _mother.p92InvoiceTypeBL.Load(rec.p92ID);
            }

            if (recP92.p92QrCodeFlag == BO.p92QrCodeFlagENUM.Slovensko || recP92.p92QrCodeFlag == BO.p92QrCodeFlagENUM.SlovenskoBezSplatnosti)
            {
                var recP93 = _mother.p93InvoiceHeaderBL.Load(recP92.p93ID);
                var recP86 = _mother.p86BankAccountBL.LoadInvoiceAccount(p91id);

                try
                {
                    var qrcode = new BL.Code.SlovenskoQrCode().LoadQrCode(rec, recP86, recP93, (recP92.p92QrCodeFlag == BO.p92QrCodeFlagENUM.SlovenskoBezSplatnosti ? false : true)).Result;

                    return _db.RunSql("UPDATE p91Invoice SET p91QrCode=@code WHERE p91ID=@pid", new { code = qrcode, pid = p91id });
                }
                catch (Exception ex)
                {
                    BO.Code.File.LogError(ex.Message, _mother.CurrentUser.j02Login, "UpdateSlovenskyQrCode");
                }

            }

            return false;
        }

        public int Update(BO.p91Invoice rec, List<BO.FreeFieldInput> lisFFI, List<BO.x69EntityRole_Assign> lisX69)
        {
            if (!ValidateBeforeUpdate(rec))
            {
                return 0;
            }
            using (var sc = new System.Transactions.TransactionScope())
            {
                var p = new DL.Params4Dapper();
                p.AddInt("pid", rec.pid);
                p.AddBool("p91IsDraft", rec.p91IsDraft);

                p.AddInt("p92ID", rec.p92ID, true);
                p.AddInt("p28ID", rec.p28ID, true);
                p.AddInt("j27ID", rec.j27ID, true);
                p.AddInt("j19ID", rec.j19ID, true);
                if (rec.j02ID_Owner == 0) rec.j02ID_Owner = _mother.CurrentUser.pid;
                p.AddInt("j02ID_Owner", rec.j02ID_Owner, true);

                p.AddInt("p98ID", rec.p98ID, true);
                p.AddInt("p63ID", rec.p63ID, true);
                p.AddInt("p80ID", rec.p80ID, true);


                p.AddInt("p91LockFlag", rec.p91LockFlag);

                p.AddString("p91Code", rec.p91Code);
                p.AddString("p91Text1", rec.p91Text1);
                p.AddString("p91Text2", rec.p91Text2);

                p.AddDateTime("p91Date", rec.p91Date);
                p.AddDateTime("p91DateMaturity", rec.p91DateMaturity);
                p.AddDateTime("p91DateSupply", rec.p91DateSupply);
                p.AddDateTime("p91Datep31_From", rec.p91Datep31_From);
                p.AddDateTime("p91Datep31_Until", rec.p91Datep31_Until);

                p.AddString("p91Client", rec.p91Client);
                p.AddString("p91Client_RegID", rec.p91Client_RegID);
                p.AddString("p91Client_VatID", rec.p91Client_VatID);
                p.AddString("p91ClientAddress1_Street", rec.p91ClientAddress1_Street);
                p.AddString("p91ClientAddress1_City", rec.p91ClientAddress1_City);
                p.AddString("p91ClientAddress1_ZIP", rec.p91ClientAddress1_ZIP);
                p.AddString("p91ClientAddress1_Country", rec.p91ClientAddress1_Country);
                p.AddString("p91ClientAddress1_Before", rec.p91ClientAddress1_Before);

                p.AddString("p91ClientAddress2", rec.p91ClientAddress2);
                p.AddString("p91Client_ICDPH_SK", rec.p91Client_ICDPH_SK);

                p.AddString("p91Supplier", rec.p91Supplier);
                p.AddString("p91Supplier_RegID", rec.p91Supplier_RegID);
                p.AddString("p91Supplier_VatID", rec.p91Supplier_VatID);
                p.AddString("p91Supplier_Street", rec.p91Supplier_Street);
                p.AddString("p91Supplier_City", rec.p91Supplier_City);
                p.AddString("p91Supplier_ZIP", rec.p91Supplier_ZIP);
                p.AddString("p91Supplier_Country", rec.p91Supplier_Country);
                p.AddString("p91Supplier_Registration", rec.p91Supplier_Registration);
                p.AddString("p91Supplier_ICDPH_SK", rec.p91Supplier_ICDPH_SK);


                p.AddEnumInt("p91PortalFlag", rec.p91PortalFlag, true);
                p.AddString("p91VatCodePohoda", rec.p91VatCodePohoda);

                p.AddInt("p91BitStream", rec.p91BitStream);

                int intPID = _db.SaveRecord("p91Invoice", p, rec);
                if (intPID > 0)
                {
                    if (!DL.BAS.SaveFreeFields(_db, intPID, lisFFI))
                    {
                        return 0;
                    }
                    if (lisX69 != null && !DL.BAS.SaveX69(_db, "p91", intPID, lisX69))
                    {
                        this.AddMessageTranslated("Error: DL.BAS.SaveX69");
                        return 0;
                    }

                    var recP92 = _mother.p92InvoiceTypeBL.Load(rec.p92ID);

                    if (recP92.p92QrCodeFlag == BO.p92QrCodeFlagENUM.Slovensko || recP92.p92QrCodeFlag == BO.p92QrCodeFlagENUM.SlovenskoBezSplatnosti)
                    {
                        UpdateSlovenskyQrCode(intPID, recP92);
                    }

                    if (_db.RunSql("exec dbo.p91_aftersave @p91id,@j02id_sys,@recalc_amount", new { p91id = intPID, j02id_sys = _mother.CurrentUser.pid, recalc_amount = true }))
                    {
                        sc.Complete();
                        return intPID;
                    }



                }

                return intPID;
            }


        }
        private bool ValidateBeforeUpdate(BO.p91Invoice rec)
        {
            if (string.IsNullOrEmpty(rec.p91Code))
            {
                rec.p91Code = "TEMP" + BO.Code.Bas.GetGuid();    //dočasný kód, bude později nahrazen
            }
            if (rec.j27ID == 0)
            {
                this.AddMessage("Chybí vyplnit [Měna]."); return false;
            }
            if (rec.p92ID == 0)
            {
                this.AddMessage("Chybí vyplnit [Typ faktury]."); return false;
            }
            if (rec.p28ID == 0)
            {
                this.AddMessage("Chybí vyplnit [Klient]."); return false;
            }


            return true;
        }

        public bool ChangeVat(int p91id, int x15id, double newvatrate)
        {
            if (x15id > 1 && newvatrate <= 0)
            {
                this.AddMessage("Musíte zadat hodnotu DPH sazby (%)."); return false;
            }
            using (var sc = new System.Transactions.TransactionScope())
            {
                var pars = new Dapper.DynamicParameters();

                pars.Add("p91id", p91id, DbType.Int32);
                pars.Add("j02id_sys", _mother.CurrentUser.pid, DbType.Int32);
                pars.Add("x15id", x15id, DbType.Int32);
                pars.Add("newvatrate", newvatrate, DbType.Double);
                pars.Add("x01id", _mother.CurrentUser.x01ID, DbType.Int32);
                pars.Add("err_ret", null, DbType.String, ParameterDirection.Output, 1000);

                if (_db.RunSp("p91_change_vat", ref pars) == "1")
                {
                    sc.Complete();

                    return true;
                }
                else
                {
                    return false;
                }



            }
        }


        private void Handle_RecalcAmount(int p91id)
        {
            _db.RunSql("exec dbo.p91_recalc_amount @p91id", new { p91id = p91id });
        }



        public int CreateCreditNote(int p91id, int p92id_creditnote)
        {
            string strSP = "p91_create_creditnote";

            using (var sc = new System.Transactions.TransactionScope())
            {
                var pars = new Dapper.DynamicParameters();

                pars.Add("p91id_bind", p91id, DbType.Int32);
                pars.Add("j02id_sys", _mother.CurrentUser.pid, DbType.Int32);
                pars.Add("p92id_creditnote", p92id_creditnote, DbType.Int32);
                pars.Add("ret_p91id", 0, DbType.Int32, ParameterDirection.Output);
                pars.Add("err_ret", null, DbType.String, ParameterDirection.Output, 1000);

                if (_db.RunSp(strSP, ref pars) == "1")
                {
                    sc.Complete();

                    return pars.Get<int>("ret_p91id");
                }
                else
                {
                    return 0;
                }



            }
        }

        public bool ChangeCurrency(int p91id, int j27id)
        {
            using (var sc = new System.Transactions.TransactionScope())
            {
                var pars = new Dapper.DynamicParameters();

                pars.Add("p91id", p91id, DbType.Int32);
                pars.Add("j02id_sys", _mother.CurrentUser.pid, DbType.Int32);
                pars.Add("j27id", j27id, DbType.Int32);
                pars.Add("err_ret", null, DbType.String, ParameterDirection.Output, 1000);

                if (_db.RunSp("p91_change_currency", ref pars) == "1")
                {
                    sc.Complete();

                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        public bool ConvertFromDraft(int p91id)
        {
            using (var sc = new System.Transactions.TransactionScope())
            {
                var pars = new Dapper.DynamicParameters();
                pars.Add("p91id", p91id, DbType.Int32);
                pars.Add("j02id_sys", _mother.CurrentUser.pid, DbType.Int32);
                pars.Add("err_ret", null, DbType.String, ParameterDirection.Output, 1000);

                if (_db.RunSp("p91_convertdraft", ref pars) == "1")
                {
                    sc.Complete();

                    return true;
                }
                else
                {
                    return false;
                }
            }
        }
        public bool RecoveryP91Code(int p91id)
        {
            using (var sc = new System.Transactions.TransactionScope())
            {
                var pars = new Dapper.DynamicParameters();
                pars.Add("p91id", p91id, DbType.Int32);
                pars.Add("j02id_sys", _mother.CurrentUser.pid, DbType.Int32);
                pars.Add("err_ret", null, DbType.String, ParameterDirection.Output, 1000);

                if (_db.RunSp("p91_recovery_p91code", ref pars) == "1")
                {
                    sc.Complete();

                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        public bool SaveP99(int p91id, int p90id, int p82id, double percentage)
        {
            if (p90id == 0)
            {
                this.AddMessage("Na vstupu chybí vybrat zálohovou fakturu."); return false;
            }
            if (p82id == 0)
            {
                this.AddMessage("Na vstupu chybí vybrat úhradu zálohové faktury."); return false;
            }
            using (var sc = new System.Transactions.TransactionScope())
            {
                var pars = new Dapper.DynamicParameters();

                pars.Add("p91id", p91id, DbType.Int32);
                pars.Add("p90id", p90id, DbType.Int32);
                pars.Add("p82id", p82id, DbType.Int32);
                pars.Add("percentage", percentage, DbType.Double);
                pars.Add("j02id_sys", _mother.CurrentUser.pid, DbType.Int32);
                pars.Add("err_ret", null, DbType.String, ParameterDirection.Output, 1000);

                if (_db.RunSp("p91_proforma_save", ref pars) == "1")
                {
                    sc.Complete();

                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        public bool DeleteP99(int p99id)
        {
            using (var sc = new System.Transactions.TransactionScope())
            {
                var pars = new Dapper.DynamicParameters();

                pars.Add("p99id", p99id, DbType.Int32);
                pars.Add("j02id_sys", _mother.CurrentUser.pid, DbType.Int32);
                pars.Add("err_ret", null, DbType.String, ParameterDirection.Output, 1000);

                if (_db.RunSp("p91_proforma_delete", ref pars) == "1")
                {
                    sc.Complete();

                    return true;
                }
                else
                {
                    return false;
                }
            }
        }
        public IEnumerable<BO.p96Imprint> GetList_p96(int p91id, string p96guid)
        {
            sb("select a.*,b.x31ID,b.x31Name,");
            sb(_db.GetSQL1_Ocas("p96", false, false, true));
            sb(" from p96Imprint a LEFT OUTER JOIN x31Report b ON a.p96ReportCode=b.x31Code");
            if (p96guid != null)
            {
                sb(" WHERE a.p96Guid=@guid");
            }
            else
            {
                sb(" WHERE a.p91ID=@p91id");
            }
            sb(" ORDER BY a.p96ID DESC");
            return _db.GetList<BO.p96Imprint>(sbret(), new { p91id = p91id, guid = p96guid });
        }
        public IEnumerable<BO.p99Invoice_Proforma> GetList_p99(int p90id, int p91id, int p82id)
        {
            sb("SELECT a.*,p90.p90Code,p91.p91Code,p82.p82Code,p82.p90ID,p92.x31ID_Invoice,p89.x31ID_Payment,");
            sb(_db.GetSQL1_Ocas("p99", false, false, true));
            sb(" FROM p99Invoice_Proforma a INNER JOIN p82Proforma_Payment p82 ON a.p82ID=p82.p82ID INNER JOIN p91Invoice p91 ON a.p91ID=p91.p91ID INNER JOIN p90Proforma p90 ON p82.p90ID=p90.p90ID");
            sb(" LEFT OUTER JOIN p92InvoiceType p92 ON p91.p92ID=p92.p92ID");
            sb(" LEFT OUTER JOIN p89ProformaType p89 ON p90.p89ID=p89.p89ID");
            object pars = null;

            if (p90id > 0)
            {
                sb(" WHERE p82.p90ID=@p90id");
                pars = new { p90id = p90id };
            }
            if (p91id > 0)
            {
                sb(" WHERE a.p91ID=@p91id");
                pars = new { p91id = p91id };
            }
            if (p82id > 0)
            {
                sb(" WHERE a.p82ID=@p82id");
                pars = new { p82id = p82id };
            }
            return _db.GetList<BO.p99Invoice_Proforma>(sbret(), pars);
        }

        public bool RecalcFPR(DateTime d1, DateTime d2, int p51id = 0)
        {
            return _db.RunSql("exec dbo.p91_fpr_recalc_all_invoices @d1,@d2,@p51id", new { d1 = d1, d2 = d2, p51id = p51id });
        }


        public int SaveP94(BO.p94Invoice_Payment rec)
        {
            if (rec.p94Amount == 0)
            {
                this.AddMessage("Částka nesmí být nula."); return 0;
            }

            var p = new DL.Params4Dapper();
            p.AddInt("pid", rec.pid);
            p.AddInt("p91ID", rec.p91ID, true);
            p.AddDateTime("p94Date", rec.p94Date);
            p.AddDouble("p94Amount", rec.p94Amount);
            p.AddString("p94Code", rec.p94Code);
            p.AddString("p94Description", rec.p94Description);

            int intPID = _db.SaveRecord("p94Invoice_Payment", p, rec, false, true);
            if (intPID > 0)
            {
                _db.RunSql("UPDATE p84Upominka SET p84ValidUntil=GETDATE() WHERE p91ID=@p91id;UPDATE p91Invoice SET p84ID_Last=null WHERE p91ID=@p91id", new { p91id = rec.p91ID });
                Handle_RecalcAmount(rec.p91ID);
            }

            return intPID;

        }

        public IEnumerable<BO.p94Invoice_Payment> GetList_p94(int p91id, int p94id = 0)
        {
            sb("select a.*,");
            sb(_db.GetSQL1_Ocas("p94", false, false, true));
            if (p94id > 0)
            {
                sb(" FROM p94Invoice_Payment a WHERE a.p94ID=@p94id");
                return _db.GetList<BO.p94Invoice_Payment>(sbret(), new { p94id = p94id });
            }
            else
            {
                sb(" FROM p94Invoice_Payment a WHERE a.p91ID=@p91id ORDER BY a.p94Date DESC");
                return _db.GetList<BO.p94Invoice_Payment>(sbret(), new { p91id = p91id });
            }



        }
        public bool DeleteP94(int p94id, int p91id)
        {
            if (_db.RunSql("DELETE FROM p94Invoice_Payment WHERE p94ID=@p94id", new { p94id = p94id }))
            {
                Handle_RecalcAmount(p91id);
                return true;
            }
            else
            {
                return false;
            }
        }

        public void ClearExchangeDate(int p91id, bool recalc)
        {
            _db.RunSql("update p91Invoice set p91DateExchange=null,p91ExchangeRate=null WHERE p91ID=@p91id", new { p91id = p91id });
            if (recalc)
            {
                Handle_RecalcAmount(p91id);
            }
        }

        public IEnumerable<BO.p91_CenovyRozpis> GetList_CenovyRozpis(int p91id, bool bolIncludeRounding, bool bolIncludeProforma, int langindex)
        {

            return _db.GetList<BO.p91_CenovyRozpis>("exec dbo.p91_get_cenovy_rozpis @pid,@include_rounding,@include_proforma,@langindex", new { pid = p91id, include_rounding = bolIncludeRounding, include_proforma = bolIncludeProforma, langindex = langindex });

        }
        private IEnumerable<BO.p91_CenovyRozpis> GetList_CenovyRozpis_ZCH(int p91id)
        {

            return _db.GetList<BO.p91_CenovyRozpis>("exec dbo.p91_get_cenovy_rozpis_zch @pid,@include_rounding,@include_proforma,@langindex", new { pid = p91id, include_rounding = true, include_proforma = true, langindex = 0 });

        }


        public BO.p91RecDisposition InhaleRecDisposition(int pid, BO.p91Invoice rec = null)
        {
            var c = new BO.p91RecDisposition() { a55ID = _mother.CurrentUser.a55ID_p91 };
            if (pid == 0 && rec == null)
            {
                return c;
            }

            if (_mother.CurrentUser.IsAdmin)
            {
                c.OwnerAccess = true; c.ReadAccess = true;
                return c;
            }
            if (rec == null) rec = Load(pid);
            if (rec == null)
            {
                return null;
            }
            bool bolNoAllowEditRec = BO.Code.Bas.bit_compare_or(rec.p91LockFlag, 8);    //Zákaz upravovat/odstraňovat kartu vyúčtování (s výjimkou administrátora

            if (rec.j02ID_Owner == _mother.CurrentUser.pid || _mother.CurrentUser.TestPermission(BO.PermValEnum.GR_P91_Owner)) //je vlastník nebo má globální roli vlastit všechny faktury
            {
                c.OwnerAccess = !bolNoAllowEditRec; c.ReadAccess = true;
                return c;
            }
            var lisX67 = _mother.x67EntityRoleBL.GetList_One_Invoice(rec.pid, _mother.CurrentUser.pid,_mother.CurrentUser.j11IDs);
            foreach(var role in lisX67)
            {
                if (BO.Code.Bas.TestPermissionInRoleValue(role.x67RoleValue, BO.PermValEnum.p91_Owner))
                {
                    c.OwnerAccess = !bolNoAllowEditRec; c.ReadAccess = true;  //vlastník
                    if (role.a55ID > 0) c.a55ID = role.a55ID;
                    return c;
                }

                if (BO.Code.Bas.TestPermissionInRoleValue(role.x67RoleValue, BO.PermValEnum.p91_Reader))   //čtenář
                {
                    c.ReadAccess = true;
                    if (role.a55ID > 0) c.a55ID = role.a55ID;
                }
            }
            //var lisX69 = _mother.x67EntityRoleBL.GetList_X69_OneInvoice(rec, true);
            //foreach (var role in lisX69)
            //{
            //    if (BO.Code.Bas.TestPermissionInRoleValue(role.x67RoleValue, BO.PermValEnum.p91_Owner))
            //    {
            //        c.OwnerAccess = !bolNoAllowEditRec; c.ReadAccess = true;  //vlastník
            //        if (role.a55ID > 0) c.a55ID = role.a55ID;
            //        return c;
            //    }

            //    if (BO.Code.Bas.TestPermissionInRoleValue(role.x67RoleValue, BO.PermValEnum.p91_Reader))   //čtenář
            //    {
            //        c.ReadAccess = true;
            //        if (role.a55ID > 0) c.a55ID = role.a55ID;
            //    }
            //}

            if (!c.ReadAccess) c.ReadAccess = _mother.CurrentUser.TestPermission(BO.PermValEnum.GR_P91_Reader);

            if (!c.ReadAccess && rec.p41ID_First > 0)
            {
                var perm = _mother.p41ProjectBL.InhaleRecDisposition(rec.p41ID_First);    //oprávnění číst vyúčtování z projektové role
                c.ReadAccess = perm.p91_Read;
            }

            return c;
        }

        public BO.p91InvoiceSum LoadSumRow(int pid)
        {
            return _db.Load<BO.p91InvoiceSum>("EXEC dbo.p91_inhale_sumrow @j02id_sys,@pid", new { j02id_sys = _mother.CurrentUser.pid, pid = pid });
        }



        public BO.Integrace.InputInvoice CreateIntegraceRecord(BO.p91Invoice c)
        {
            var rec = new BO.Integrace.InputInvoice() { p91ID = c.pid, p91Code = c.p91Code, p91Guid = c.p91Guid.ToString(), j27ID = c.j27ID, p91Date = c.p91Date, p91DateSupply = c.p91DateSupply, p91DateMaturity = c.p91DateMaturity, p91DateBilled = c.p91DateBilled };
            rec.Implementace = _mother.App.Implementation;
           
            rec.p92TypeFlag = (int)c.p92TypeFlag; rec.p92ID = c.p92ID;
            rec.p91Text1 = c.p91Text1;
            rec.p91Text2 = c.p91Text2;
            rec.PredkontaceIS = _mother.CBL.GetGlobalParamValue("p92AccountingIds", c.p92ID);    //předkontace na úrovni celé faktury
            if (c.p41ID_First > 0)
            {
                var recP41 = _mother.p41ProjectBL.Load(c.p41ID_First);  //zkusit najít předkontaci celé faktury v nastavení fakturovaného projektu.
                if (recP41.p41AccountingIds != null)
                {
                    rec.PredkontaceIS = recP41.p41AccountingIds;
                }
            }
            if (c.p91VatCodePohoda == null)
            {
                rec.KlasifikaceDphIS = _mother.CBL.GetGlobalParamValue("p92ClassificationVatIds", c.p92ID); //členění dph
            }
            else
            {
                rec.KlasifikaceDphIS = c.p91VatCodePohoda;
            }
            
            rec.j27ID_Domestic = _mother.Lic.j27ID; rec.j27Code_Domestic = _mother.FBL.LoadCurrencyByID(_mother.Lic.j27ID).j27Code;
            rec.j27ID = c.j27ID; rec.j27Code = c.j27Code;
            rec.p91DateExchange = c.p91DateExchange; rec.p91ExchangeRate = c.p91ExchangeRate;
            rec.p91Client = c.p91Client; rec.p91Client_RegID = c.p91Client_RegID; rec.p91Client_VatID = c.p91Client_VatID; rec.p91ClientAddress1_City = c.p91ClientAddress1_City; rec.p91ClientAddress1_Street = c.p91ClientAddress1_Street; rec.p91ClientAddress1_ZIP = c.p91ClientAddress1_ZIP; rec.p91ClientAddress1_Country = c.p91ClientAddress1_Country;
            rec.p91Client_ICDPH_SK = c.p91Client_ICDPH_SK;
            rec.p91Amount_WithoutVat_Standard = c.p91Amount_WithoutVat_Standard; rec.p91Amount_Vat_Standard = c.p91Amount_Vat_Standard; rec.p91Amount_WithVat_Standard = c.p91Amount_WithVat_Standard; rec.p91VatRate_Standard = c.p91VatRate_Standard;
            rec.p91Amount_WithoutVat_Low = c.p91Amount_WithoutVat_Low; rec.p91Amount_Vat_Low = c.p91Amount_Vat_Low; rec.p91Amount_WithVat_Low = c.p91Amount_WithVat_Low; rec.p91VatRate_Low = c.p91VatRate_Low;
            rec.p91Amount_WithoutVat_None = c.p91Amount_WithoutVat_None; rec.p91Amount_TotalDue = c.p91Amount_TotalDue; rec.p91ProformaAmount = c.p91Amount_TotalDue; rec.p91ProformaBilledAmount = c.p91ProformaBilledAmount;
            var recP86 = _mother.p86BankAccountBL.LoadInvoiceAccount(c.pid);
            if (recP86 != null)
            {
                rec.p86Account = recP86.p86Account; rec.p86Code = recP86.p86Code; rec.p86BankName = recP86.p86BankName; rec.p86IBAN = recP86.p86IBAN; rec.p86SWIFT = recP86.p86SWIFT;
            }
            rec.p93ID = c.p93ID;
            var recP93 = _mother.p93InvoiceHeaderBL.Load(c.p93ID);
            if (recP93.p93CountryCode == null) recP93.p93CountryCode = "CZ";
            if (recP93.p93Country == null) recP93.p93CountryCode = "Česká republika";
            rec.p93CountryCode = recP93.p93CountryCode; rec.p93Company = recP93.p93Company; rec.p93VatID = recP93.p93VatID; rec.p93RegID = recP93.p93RegID; rec.p93City = recP93.p93City; rec.p93Street = recP93.p93Street; rec.p93Zip = recP93.p93Zip; rec.p93Country = recP93.p93Country;
            rec.p93Referent = recP93.p93Referent; rec.p93Contact = recP93.p93Contact; rec.p93Email = recP93.p93Email;
            rec.p28ID = c.p28ID; rec.p28CountryCode = c.p28CountryCode; rec.p41ID_First = c.p41ID_First;
            if (rec.p41ID_First > 0)
            {
                var recP41 = _mother.p41ProjectBL.Load(rec.p41ID_First);
                rec.Project = recP41.FullName; rec.p41Code = recP41.p41Code;
                if (recP41.j18ID > 0)
                {
                    var recJ18 = _mother.j18CostUnitBL.Load(recP41.j18ID);
                    rec.j18ID = recP41.j18ID; rec.j18Code = recJ18.j18Code;
                }
            }
            if (rec.Implementace == "zch")  //zch prasárna
            {
                rec.ZchPartner = _db.Load<BO.GetString>($"select dbo.zzz_p28_getonerole_inline({rec.p28ID},56) as Value").Value;
            }

            string strPredkontaceDef = _mother.CBL.GetGlobalParamValue("p92AccountingIds", rec.p92ID);
            string strCinnostISDef = _mother.CBL.GetGlobalParamValue("p92ActivityIds", rec.p92ID);

            var rozpis = GetList_CenovyRozpis(c.pid, true, true, 0);
            if (rec.Implementace == "zch")
            {
                rozpis = GetList_CenovyRozpis_ZCH(c.pid);   //ZCH má pro pohodu svůj cenový rozpis
            }
            rec.InvoiceRows = new List<BO.Integrace.InputInvoiceRow>();
            var lisP53 = _mother.p53VatRateBL.GetList(new BO.myQuery("p53") { IsRecordValid = null });
            foreach (var row in rozpis)
            {
                var line = new BO.Integrace.InputInvoiceRow() { Oddil = row.Oddil, BezDPH = row.BezDPH, DPH = row.DPH, DPHSazba = row.DPHSazba, VcDPH = row.VcDPH, Poradi = row.Poradi, RowPID = row.RowPID, j27Code = row.j27Code };

                if (row.p31ID > 0)
                {
                    if (row.DPHSazba == 0)
                    {
                        line.x15ID = 1;  //nulová DPH
                    }
                    else
                    {
                        if (lisP53.Any(p => p.p53Value == row.DPHSazba))
                        {
                            line.x15ID = (int)lisP53.First(p => p.p53Value == row.DPHSazba).x15ID;
                        }
                        else
                        {
                            if (row.DPHSazba < 20)
                            {
                                line.x15ID = 2; //odhad: Snížená dph sazba
                            }
                            else
                            {
                                line.x15ID = 3; //odhad: Základní dph sazba
                            }
                        }
                    }
                    line.p31ID = row.p31ID;
                    var recP31 = _mother.p31WorksheetBL.Load(row.p31ID);
                    line.p31Code = recP31.p31Code;
                    var recP32 = _mother.p32ActivityBL.Load(recP31.p32ID);
                    string ss = _mother.CBL.GetGlobalParamValue("p32AccountingIds", recP32.pid);
                    if (ss == null && recP32.p95ID > 0)
                    {
                        ss = _mother.CBL.GetGlobalParamValue("p95AccountingIds", recP32.p95ID);
                    }
                    if (ss == null)
                    {
                        ss = strPredkontaceDef;
                    }
                    line.PredkontaceIS = ss;
                    ss = _mother.CBL.GetGlobalParamValue("p32ActivityIds", recP32.pid);
                    if (ss == null && recP32.p95ID > 0)
                    {
                        ss = _mother.CBL.GetGlobalParamValue("p95ActivityIds", recP32.p95ID);
                    }
                    if (ss == null)
                    {
                        ss = strCinnostISDef;
                    }
                    line.CinnostIS = ss;
                }
                else
                {
                    if (row.p95ID > 0)
                    {
                        line.PredkontaceIS = _mother.CBL.GetGlobalParamValue("p95AccountingIds", row.p95ID);
                        if (line.PredkontaceIS == null)
                        {
                            line.PredkontaceIS = strPredkontaceDef;
                        }
                        line.CinnostIS = _mother.CBL.GetGlobalParamValue("p95ActivityIds", row.p95ID);
                        if (line.CinnostIS == null)
                        {
                            line.CinnostIS = strCinnostISDef;
                        }
                    }
                }
                rec.InvoiceRows.Add(line);

            }

            return rec;
        }

        public bool Update_Temp_MultiReport(int p91id, string p91Supplier, string p91Supplier_RegID, string p91Supplier_VatID, string p91Supplier_Street, string p91Supplier_City, string p91Supplier_ZIP, string p91Supplier_Country, string p91Supplier_Registration, string p91Supplier_ICDPH_SK, int p93id, string p93Contact, string p93Email,string p93Referent,string p93Signature)
        {

            //slouží pro ukládání dočasných dat při generování multi pdf reportu faktury
            var pars1 = new
            {
                pid = p93id,
                p93Contact = p93Contact,
                p93Email = p93Email,
                p93Referent= p93Referent,
                p93Signature= p93Signature
            };

            _db.RunSql("UPDATE p93InvoiceHeader set p93Contact=@p93Contact,p93Email=@p93Email,p93Referent=@p93Referent,p93Signature=@p93Signature WHERE p93ID=@pid", pars1);

            var pars2 = new
            {
                pid = p91id,
                p91Supplier = p91Supplier,
                p91Supplier_RegID = p91Supplier_RegID,
                p91Supplier_VatID = p91Supplier_VatID,
                p91Supplier_Street = p91Supplier_Street,
                p91Supplier_City = p91Supplier_City,
                p91Supplier_ZIP = p91Supplier_ZIP,
                p91Supplier_Country = p91Supplier_Country,
                p91Supplier_Registration = p91Supplier_Registration,
                p91Supplier_ICDPH_SK = p91Supplier_ICDPH_SK
            };

            return _db.RunSql("update p91Invoice SET p91Supplier=@p91Supplier,p91Supplier_RegID=@p91Supplier_RegID,p91Supplier_VatID=@p91Supplier_VatID,p91Supplier_Street=@p91Supplier_Street,p91Supplier_City=@p91Supplier_City,p91Supplier_ZIP=@p91Supplier_ZIP,p91Supplier_Country=@p91Supplier_Country,p91Supplier_Registration=@p91Supplier_Registration,p91Supplier_ICDPH_SK=@p91Supplier_ICDPH_SK WHERE p91ID=@pid", pars2);



        }

        public int NajdiVychoziJ61ID(BO.p92InvoiceType recP92, BO.p91Invoice recP91, BO.p28Contact recP28)
        {
            int j61id = 0;
            if (j61id == 0 && recP28 != null && recP28.j61ID_Invoice > 0)   //klient má svojí výchozí poštovní šablonu
            {
                j61id = recP28.j61ID_Invoice;
            }
            if (j61id == 0 && recP92.j61ID > 0)
            {
                j61id = recP92.j61ID;
            }
            if (j61id == 0)
            {
                j61id = _mother.CBL.LoadUserParamInt("mail-p91-j61id");
            }

            return j61id;
        }

    }
}
