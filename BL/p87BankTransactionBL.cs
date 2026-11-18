using BL.Code;

namespace BL
{
    public interface Ip87BankTransactionBL
    {
        public BO.p87BankTransaction Load(int pid);
        /// <summary>statusflag: -1 = všechny stavy</summary>
        public IEnumerable<BO.p87BankTransaction> GetList(int statusflag, int toprecs);
        public IEnumerable<BO.p87BankTransaction> GetList_o27(int o27id);
        public int GetCountUnpaired();
        public int Save(BO.p87BankTransaction rec);
        /// <summary>Uloží pohyby jednoho e-mailu a jisté úhrady rovnou zapíše. recO27 už musí být uložený.</summary>
        public List<BO.p87BankTransaction> ProcessMail(BO.o27Attachment recO27, List<BO.o42ImapRule> rules, List<BankMailMove> moves);
        public bool AssignInvoice(int p87id, int p91id);
        public bool AssignProforma(int p87id, int p90id);
        public bool Ignore(int p87id);
        public bool Reopen(int p87id);
        /// <summary>Úhrada faktury byla smazána: transakce, ze které vznikla, se vrátí mezi nespárované.</summary>
        public void OnInvoicePaymentDeleted(int p94id);
    }

    class p87BankTransactionBL : BaseBL, Ip87BankTransactionBL
    {
        private const int _OldMailDays = 30;
        private const int _SamePaymentDays = 5;     //úhrada se stejnou částkou do tolika dnů od data e-mailu = táž platba

        public p87BankTransactionBL(BL.Factory mother) : base(mother)
        {

        }

        private string GetSQL1(string strAppend = null, int toprecs = 0)
        {
            sb("SELECT ");
            if (toprecs > 0) sb($"TOP {toprecs} ");
            sb("a.*,o42.o42Name,o27.o27MailSubject,o27.o27MailSenderAddress,p91.p91Code,p91.p91Client,p91.p91Amount_Debt,p90.p90Code,p90.p90Amount_Debt,");
            sb(_db.GetSQL1_Ocas("p87", false, false, true));
            sb(" FROM p87BankTransaction a INNER JOIN o42ImapRule o42 ON a.o42ID=o42.o42ID INNER JOIN j40MailAccount j40 ON o42.j40ID=j40.j40ID");
            sb(" LEFT OUTER JOIN o27Attachment o27 ON a.o27ID=o27.o27ID LEFT OUTER JOIN p91Invoice p91 ON a.p91ID=p91.p91ID LEFT OUTER JOIN p90Proforma p90 ON a.p90ID=p90.p90ID");
            sb(" WHERE (@x01id=0 OR j40.x01ID=@x01id)");    //transakce patří licenci přes poštovní účet pravidla
            sb(strAppend);
            return sbret();
        }

        private int LicenseFilter()
        {
            return _mother.CurrentUser.IsHostingModeTotalCloud ? _mother.CurrentUser.x01ID : 0;
        }

        public BO.p87BankTransaction Load(int pid)
        {
            return _db.Load<BO.p87BankTransaction>(GetSQL1(" AND a.p87ID=@pid"), new { pid = pid, x01id = LicenseFilter() });
        }

        public IEnumerable<BO.p87BankTransaction> GetList(int statusflag, int toprecs)
        {
            string s = (statusflag >= 0 ? " AND a.p87StatusFlag=@flag" : "") + " ORDER BY a.p87ID DESC";
            return _db.GetList<BO.p87BankTransaction>(GetSQL1(s, toprecs), new { flag = statusflag, x01id = LicenseFilter() });
        }

        public IEnumerable<BO.p87BankTransaction> GetList_o27(int o27id)
        {
            return _db.GetList<BO.p87BankTransaction>(GetSQL1(" AND a.o27ID=@o27id ORDER BY a.p87Ordinary"), new { o27id = o27id, x01id = LicenseFilter() });
        }

        public int GetCountUnpaired()
        {
            var lis = GetList(0, 0);
            return (lis == null ? 0 : lis.Count());
        }

        public int Save(BO.p87BankTransaction rec)
        {
            if (rec.o42ID == 0)
            {
                this.AddMessage("Chybí vazba na IMAP pravidlo."); return 0;
            }
            var p = new DL.Params4Dapper();
            p.AddInt("pid", rec.pid);
            p.AddInt("o42ID", rec.o42ID, true);
            p.AddInt("o27ID", rec.o27ID, true);
            p.AddDateTime("p87Date", rec.p87Date);
            p.AddDouble("p87Amount", rec.p87Amount);
            p.AddInt("p87Ordinary", rec.p87Ordinary);
            p.AddString("p87Code", rec.p87Code);
            p.AddEnumInt("p87StatusFlag", rec.p87StatusFlag);
            p.AddInt("p94ID", rec.p94ID, true);
            p.AddInt("p91ID", rec.p91ID, true);
            p.AddInt("p90ID", rec.p90ID, true);
            p.AddInt("p82ID", rec.p82ID, true);
            p.AddString("p87Message", BO.Code.Bas.OM2(rec.p87Message, 990));
            p.AddString("p87BankAccount", BO.Code.Bas.OM2(rec.p87BankAccount, 50));

            return _db.SaveRecord("p87BankTransaction", p, rec, false, true);
        }


        public List<BO.p87BankTransaction> ProcessMail(BO.o27Attachment recO27, List<BO.o42ImapRule> rules, List<BankMailMove> moves)
        {
            var ret = new List<BO.p87BankTransaction>();
            if (recO27 == null || recO27.pid == 0 || rules == null || rules.Count == 0 || moves == null) return ret;

            bool bolP91 = rules.Any(p => p.o42WhatToDoFlag == BO.o42WhatToDoFlagENUM.InvoicePayment);
            bool bolP90 = rules.Any(p => p.o42WhatToDoFlag == BO.o42WhatToDoFlagENUM.ProformaPayment);
            DateTime d = (recO27.o27MailDateMessage ?? recO27.o27MailDateReceived ?? DateTime.Today).Date;

            int x = 0;
            foreach (var move in moves)
            {
                x++;
                var rec = new BO.p87BankTransaction() { o42ID = rules.First().pid, o27ID = recO27.pid, p87Ordinary = x, p87Date = d, p87Amount = move.Amount, p87Code = move.VS, p87BankAccount = move.Account };
                bool bolPay = Match(rec, move, bolP91, bolP90);
                if (!bolPay && rec.p87StatusFlag == BO.p87StatusFlagENUM.Nesparovano && d < DateTime.Today.AddDays(-_OldMailDays))
                {
                    //starý e-mail (typicky při prvním běhu nad plnou schránkou) frontu nespárovaných nezahlcuje; vrátit ho lze ručně
                    rec.p87StatusFlag = BO.p87StatusFlagENUM.Ignorovano;
                    rec.p87Message = $"E-mail starší než {_OldMailDays} dní. " + rec.p87Message;
                }
                if (rec.p90ID > 0 && bolP90)
                {
                    rec.o42ID = rules.First(p => p.o42WhatToDoFlag == BO.o42WhatToDoFlagENUM.ProformaPayment).pid;
                }
                rec.pid = Save(rec);
                if (rec.pid > 0 && bolPay)
                {
                    Pay(rec, BO.p87StatusFlagENUM.SparovanoAutomaticky);
                }
                ret.Add(rec);
            }

            RefreshMailLink(recO27.pid);
            return ret;
        }

        /// <summary>Najde k pohybu doklad. Vrací true, když je shoda jistá a úhradu lze zapsat bez zásahu uživatele.</summary>
        private bool Match(BO.p87BankTransaction rec, BankMailMove move, bool bolP91, bool bolP90)
        {
            rec.p87StatusFlag = BO.p87StatusFlagENUM.Nesparovano;
            if (move.Direction == -1)
            {
                rec.p87StatusFlag = BO.p87StatusFlagENUM.Ignorovano;
                rec.p87Message = "Odchozí platba.";
                return false;
            }

            var codes = (move.VS != null ? new List<string>() { move.VS } : move.Codes);
            var lisP91 = new List<BO.p91Invoice>(); var lisP90 = new List<BO.p90Proforma>();
            foreach (string code in codes)
            {
                if (bolP91)
                {
                    var c = FindP91(code);
                    if (c != null && !lisP91.Any(p => p.pid == c.pid)) lisP91.Add(c);
                }
                if (bolP90)
                {
                    var c = FindP90(code);
                    if (c != null && !lisP90.Any(p => p.pid == c.pid)) lisP90.Add(c);
                }
            }

            var exactP91 = lisP91.Where(p => p.p91Amount_Debt > 0 && Math.Abs(p.p91Amount_Debt - move.Amount) < 0.005).ToList();
            var exactP90 = lisP90.Where(p => p.p90Amount_Debt > 0 && Math.Abs(p.p90Amount_Debt - move.Amount) < 0.005).ToList();

            if (exactP91.Count + exactP90.Count == 1)
            {
                if (exactP91.Count == 1) rec.p91ID = exactP91[0].pid; else rec.p90ID = exactP90[0].pid;
                if (move.Direction == 1) return true;

                rec.p87Message = "Z e-mailu nelze poznat, zda jde o příchozí platbu. Variabilní symbol i částka dokladu odpovídají.";
                return false;
            }
            if (exactP91.Count + exactP90.Count > 1)
            {
                rec.p87Message = "Platbě odpovídá více dokladů: " + string.Join(", ", exactP91.Select(p => p.p91Code).Concat(exactP90.Select(p => p.p90Code)));
                return false;
            }

            if (move.VS == null)
            {
                rec.p87Message = "Platba bez variabilního symbolu.";
                return false;
            }
            if (lisP91.Count > 0)
            {
                var c = lisP91[0];
                rec.p91ID = c.pid;
                if (c.p91Amount_Debt <= 0)
                {
                    //uhrazená faktura: další úhrada se nikdy nezapisuje
                    var lisP94 = _mother.p91InvoiceBL.GetList_p94(c.pid);
                    var recP94 = (lisP94 == null ? null : lisP94.FirstOrDefault(p => Math.Abs(p.p94Amount - move.Amount) < 0.005 && Math.Abs((p.p94Date.Date - (rec.p87Date ?? DateTime.Today).Date).TotalDays) <= _SamePaymentDays));
                    if (recP94 != null)
                    {
                        //stejná úhrada už je na faktuře zapsaná (ručně nebo jiným importem): není co řešit
                        rec.p87StatusFlag = BO.p87StatusFlagENUM.Ignorovano;
                        rec.p87Message = $"Faktura {c.p91Code} už má tuto úhradu zapsanou ({BO.Code.Bas.ObjectDate2String(recP94.p94Date, "dd.MM.yyyy")}).";
                    }
                    else
                    {
                        rec.p87Message = $"Faktura {c.p91Code} je už uhrazená, úhrada se nezapsala. Může jít o dvojí platbu.";
                    }
                    return false;
                }
                rec.p87Message = $"Částka platby neodpovídá dluhu faktury {c.p91Code} ({BO.Code.Bas.GN(c.p91Amount_Debt)}).";
                return false;
            }
            if (lisP90.Count > 0)
            {
                var c = lisP90[0];
                rec.p90ID = c.pid;
                if (c.p90Amount_Debt <= 0)
                {
                    //uhrazená záloha: další úhrada se nikdy nezapisuje
                    var lisP82 = _mother.p82Proforma_PaymentBL.GetList(c.pid);
                    var recP82 = (lisP82 == null ? null : lisP82.FirstOrDefault(p => Math.Abs(p.p82Amount - move.Amount) < 0.005 && p.p82Date != null && Math.Abs((Convert.ToDateTime(p.p82Date).Date - (rec.p87Date ?? DateTime.Today).Date).TotalDays) <= _SamePaymentDays));
                    if (recP82 != null)
                    {
                        rec.p87StatusFlag = BO.p87StatusFlagENUM.Ignorovano;
                        rec.p87Message = $"Záloha {c.p90Code} už má tuto úhradu zapsanou ({BO.Code.Bas.ObjectDate2String(recP82.p82Date, "dd.MM.yyyy")}).";
                    }
                    else
                    {
                        rec.p87Message = $"Záloha {c.p90Code} je už uhrazená, úhrada se nezapsala. Může jít o dvojí platbu.";
                    }
                    return false;
                }
                rec.p87Message = $"Částka platby neodpovídá dluhu zálohy {c.p90Code} ({BO.Code.Bas.GN(c.p90Amount_Debt)}).";
                return false;
            }

            rec.p87Message = $"K variabilnímu symbolu {move.VS} nebyl nalezen žádný doklad.";
            return false;
        }

        private BO.p91Invoice FindP91(string code)
        {
            var c = _mother.p91InvoiceBL.LoadByCode(code);
            if (c == null) c = _mother.p91InvoiceBL.LoadByNumericCode(code);
            if (c != null && c.p91IsDraft) return null;
            return c;
        }
        private BO.p90Proforma FindP90(string code)
        {
            var c = _mother.p90ProformaBL.LoadByCode(code);
            if (c == null) c = _mother.p90ProformaBL.LoadByNumericCode(code);
            if (c != null && c.p90IsDraft) return null;
            return c;
        }

        /// <summary>Zapíše úhradu faktury (p94) nebo zálohy (p82) podle rec.p91ID / rec.p90ID a uloží výsledek do transakce.</summary>
        private bool Pay(BO.p87BankTransaction rec, BO.p87StatusFlagENUM flag)
        {
            DateTime d = rec.p87Date ?? DateTime.Today;
            string strText = "E-mail z banky" + (rec.p87BankAccount != null ? ", protiúčet " + rec.p87BankAccount : "");
            bool bolOK = false;

            //pojistka pro automatické i ruční párování: na uhrazený doklad se další úhrada nezapisuje
            bool bolPaid = false;
            if (rec.p91ID > 0)
            {
                var recP91 = _mother.p91InvoiceBL.Load(rec.p91ID);
                bolPaid = (recP91 == null || recP91.p91Amount_Debt <= 0);
            }
            else if (rec.p90ID > 0)
            {
                var recP90 = _mother.p90ProformaBL.Load(rec.p90ID);
                bolPaid = (recP90 == null || recP90.p90Amount_Debt <= 0);
            }
            if (bolPaid)
            {
                this.AddMessage("Doklad je už uhrazený, další úhrada se nezapsala.");
                rec.p87StatusFlag = BO.p87StatusFlagENUM.Nesparovano;
                rec.p87Message = "Doklad je už uhrazený, další úhrada se nezapsala.";
                Save(rec);
                return false;
            }

            if (rec.p91ID > 0)
            {
                var c = new BO.p94Invoice_Payment() { p91ID = rec.p91ID, p94Amount = rec.p87Amount, p94Date = d, p94Code = rec.p87Code, p94Description = strText };
                rec.p94ID = _mother.p91InvoiceBL.SaveP94(c);
                bolOK = (rec.p94ID > 0);
            }
            else if (rec.p90ID > 0)
            {
                var c = new BO.p82Proforma_Payment() { p90ID = rec.p90ID, p82Amount = rec.p87Amount, p82Date = d, p82Text = strText };
                rec.p82ID = _mother.p82Proforma_PaymentBL.Save(c);
                bolOK = (rec.p82ID > 0);
            }

            if (bolOK)
            {
                rec.p87StatusFlag = flag;
                rec.p87Message = null;
            }
            else
            {
                rec.p87StatusFlag = BO.p87StatusFlagENUM.Nesparovano;
                rec.p87Message = "Úhradu se nepodařilo zapsat.";
            }
            Save(rec);
            return bolOK;
        }


        public bool AssignInvoice(int p87id, int p91id)
        {
            var rec = LoadPending(p87id);
            if (rec == null) return false;
            var recP91 = _mother.p91InvoiceBL.Load(p91id);
            if (recP91 == null || recP91.p91IsDraft)
            {
                this.AddMessage("Faktura nebyla nalezena."); return false;
            }
            rec.p91ID = recP91.pid; rec.p90ID = 0;
            if (!Pay(rec, BO.p87StatusFlagENUM.SparovanoRucne)) return false;
            RefreshMailLink(rec.o27ID);
            return true;
        }

        public bool AssignProforma(int p87id, int p90id)
        {
            var rec = LoadPending(p87id);
            if (rec == null) return false;
            var recP90 = _mother.p90ProformaBL.Load(p90id);
            if (recP90 == null || recP90.p90IsDraft)
            {
                this.AddMessage("Záloha nebyla nalezena."); return false;
            }
            rec.p90ID = recP90.pid; rec.p91ID = 0;
            if (!Pay(rec, BO.p87StatusFlagENUM.SparovanoRucne)) return false;
            RefreshMailLink(rec.o27ID);
            return true;
        }

        public bool Ignore(int p87id)
        {
            var rec = LoadPending(p87id);
            if (rec == null) return false;
            rec.p87StatusFlag = BO.p87StatusFlagENUM.Ignorovano;
            if (Save(rec) == 0) return false;
            RefreshMailLink(rec.o27ID);
            return true;
        }

        public bool Reopen(int p87id)
        {
            var rec = Load(p87id);
            if (rec == null || rec.p87StatusFlag != BO.p87StatusFlagENUM.Ignorovano)
            {
                this.AddMessage("Vrátit lze jen ignorovanou transakci."); return false;
            }
            rec.p87StatusFlag = BO.p87StatusFlagENUM.Nesparovano;
            if (Save(rec) == 0) return false;
            RefreshMailLink(rec.o27ID);
            return true;
        }

        public void OnInvoicePaymentDeleted(int p94id)
        {
            if (p94id == 0) return;
            var qry = _db.GetList<BO.p87BankTransaction>(GetSQL1(" AND a.p94ID=@p94id"), new { p94id = p94id, x01id = LicenseFilter() });
            if (qry == null) return;
            foreach (var rec in qry.ToList())
            {
                rec.p94ID = 0;
                rec.p87StatusFlag = BO.p87StatusFlagENUM.Nesparovano;
                rec.p87Message = "Úhrada faktury zapsaná z této platby byla smazána.";
                Save(rec);
                RefreshMailLink(rec.o27ID);
            }
        }

        private BO.p87BankTransaction LoadPending(int p87id)
        {
            var rec = Load(p87id);
            if (rec == null)
            {
                this.AddMessage("Bankovní transakce nebyla nalezena."); return null;
            }
            if (rec.p87StatusFlag != BO.p87StatusFlagENUM.Nesparovano)
            {
                this.AddMessage("Bankovní transakce už je vyřízená."); return null;
            }
            return rec;
        }

        /// <summary>
        /// Vazba e-mailu ve FILEBOXu podle stavu jeho pohybů: nespárovaný pohyb = e-mail bez vazby (fronta k zatřídění),
        /// jinak první uhrazená faktura / záloha, jinak IMAP pravidlo (odchozí a ignorované platby).
        /// </summary>
        private void RefreshMailLink(int o27id)
        {
            if (o27id == 0) return;
            var recO27 = _mother.o27AttachmentBL.Load(o27id);
            if (recO27 == null) return;
            if (recO27.o27Entity != null && recO27.o27Entity != "p91" && recO27.o27Entity != "p90" && recO27.o27Entity != "o42") return;    //uživatel e-mail zatřídil jinam

            var qry = GetList_o27(o27id);
            if (qry == null) return;
            var lis = qry.ToList();
            if (lis.Count == 0) return;
            string strEntity = null; int intPid = 0;
            if (!lis.Any(p => p.p87StatusFlag == BO.p87StatusFlagENUM.Nesparovano))
            {
                var paid = lis.Where(p => p.p87StatusFlag == BO.p87StatusFlagENUM.SparovanoAutomaticky || p.p87StatusFlag == BO.p87StatusFlagENUM.SparovanoRucne).ToList();
                if (paid.Any(p => p.p91ID > 0))
                {
                    strEntity = "p91"; intPid = paid.First(p => p.p91ID > 0).p91ID;
                }
                else if (paid.Any(p => p.p90ID > 0))
                {
                    strEntity = "p90"; intPid = paid.First(p => p.p90ID > 0).p90ID;
                }
                else
                {
                    strEntity = "o42"; intPid = lis[0].o42ID;
                }
            }
            if (recO27.o27Entity == strEntity && recO27.o27RecordPid == intPid) return;

            _db.RunSql("UPDATE o27Attachment SET o27Entity=@entity,o27RecordPid=@pid WHERE o27ID=@o27id", new { entity = strEntity, pid = (intPid == 0 ? (int?)null : intPid), o27id = o27id });
        }
    }
}
