using BO;
using Rebex.Net;

namespace BL
{
    public interface Io42ImapRuleBL
    {
        public BO.o42ImapRule Load(int pid);

        public IEnumerable<BO.o42ImapRule> GetList(BO.myQuery mq);
        public int Save(BO.o42ImapRule rec);
        public void Run_ImapRobot();

    }
    class o42ImapRuleBL : BaseBL, Io42ImapRuleBL
    {
        public o42ImapRuleBL(BL.Factory mother) : base(mother)
        {
            Rebex.Licensing.Key = "==FmUGVeH5TmvcatEzt0Z4rBvjoahsK0e0albLXKX2bgBB+JxAEBiGKcZlB73B+U5q3G5YP==";

        }


        private string GetSQL1(string strAppend = null)
        {
            sb("SELECT a.*,j40x.j40Name,j40x.j40ImapLogin,");
            sb(_db.GetSQL1_Ocas("o42"));
            sb(" FROM o42ImapRule a INNER JOIN j40MailAccount j40x ON a.j40ID=j40x.j40ID");
            sb(strAppend);


            return sbret();
        }
        public BO.o42ImapRule Load(int pid)
        {
            return _db.Load<BO.o42ImapRule>(GetSQL1(" WHERE a.o42ID=@pid"), new { pid = pid });
        }

        public IEnumerable<BO.o42ImapRule> GetList(BO.myQuery mq)
        {
            DL.FinalSqlCommand fq = DL.basQuery.GetFinalSql(GetSQL1(), mq, _mother.CurrentUser);
            return _db.GetList<BO.o42ImapRule>(fq.FinalSql, fq.Parameters);
        }



        public int Save(BO.o42ImapRule rec)
        {
            if (!ValidateBeforeSave(rec))
            {
                return 0;
            }
            var p = new DL.Params4Dapper();
            p.AddInt("pid", rec.pid);
            p.AddInt("j40ID", rec.j40ID, true);
            p.AddEnumInt("o42WhatToDoFlag", rec.o42WhatToDoFlag);
            p.AddInt("o18ID_CreateBy", rec.o18ID_CreateBy, true);
            p.AddInt("p41ID_Default", rec.p41ID_Default, true);
            p.AddInt("j02ID_Default", rec.j02ID_Default, true);
            p.AddInt("p28ID_Default", rec.p28ID_Default, true);
            p.AddString("o42Name", rec.o42Name);
            p.AddString("o42Condition_Sender", rec.o42Condition_Sender);
            p.AddString("o42Condition_Subject", rec.o42Condition_Subject);
            p.AddString("o42Condition_Cc", rec.o42Condition_Cc);
            p.AddString("o42Condition_Bcc", rec.o42Condition_Bcc);
            p.AddString("o42Description", rec.o42Description);


            int intPID = _db.SaveRecord("o42ImapRule", p, rec);

            return intPID;
        }

        public bool ValidateBeforeSave(BO.o42ImapRule rec)
        {
            if (string.IsNullOrEmpty(rec.o42Name))
            {
                this.AddMessage("Chybí vyplnit [Název]."); return false;
            }
            if (rec.j40ID == 0)
            {
                this.AddMessage("Chybí vazba na poštovní IMAP účet."); return false;
            }



            return true;
        }


        public void Run_ImapRobot() //běh robota pro pravidelné zpracování imap pravidel
        {
            var qry = GetList(new BO.myQuery("o42"));
            if (qry == null) return;
            foreach (var grp in qry.ToList().GroupBy(p => p.j40ID))
            {
                try
                {
                    Run_OneAccount(grp.Key, grp.ToList());
                }
                catch (Exception e)
                {
                    //chyba jednoho poštovního účtu nesmí zastavit zpracování ostatních
                    BO.Code.File.LogError($"IMAP pravidla, j40ID={grp.Key}: {e.Message}");
                }
            }
        }

        private void Run_OneAccount(int j40id, List<BO.o42ImapRule> rules)
        {
            var recJ40 = _mother.j40MailAccountBL.Load(j40id);
            if (recJ40 == null) return;
            if (_mother.CurrentUser.IsHostingModeTotalCloud && recJ40.x01ID != _mother.CurrentUser.x01ID) return;  //poštovní účet jiné licence
            var imapclient = _mother.o27AttachmentBL.ConnectToImapServer(recJ40, "Inbox");
            if (imapclient == null) return;

            try
            {
                imapclient.Settings.UsePeekForGetMessage = true;    //nenahazovat IsSeen flag na načtené zprávy

                var messages = _mother.o27AttachmentBL.GetMessageList(imapclient, 200, null, null);
                if (messages == null) return;
                var known = _mother.o27AttachmentBL.GetImapMessageIds();    //zprávy, které už pravidla uložila

                foreach (var c in messages.ToList())
                {
                    if (c.MessageId == null || string.IsNullOrEmpty(c.MessageId.Id) || known.Contains(c.MessageId.Id)) continue;
                    var hits = rules.Where(p => IsRuleMatch(p, c)).ToList();
                    if (hits.Count == 0) continue;

                    try
                    {
                        var payrules = hits.Where(p => p.o42WhatToDoFlag == o42WhatToDoFlagENUM.InvoicePayment || p.o42WhatToDoFlag == o42WhatToDoFlagENUM.ProformaPayment).ToList();
                        if (payrules.Count > 0)
                        {
                            if (Handle_BankMail(imapclient, c, payrules)) known.Add(c.MessageId.Id);
                            continue;
                        }
                        foreach (var rule in hits)
                        {
                            if (Handle_BindMail(imapclient, c, rule))
                            {
                                known.Add(c.MessageId.Id);
                                break;
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        BO.Code.File.LogError($"IMAP pravidla, zpráva {c.MessageId.Id}: {e.Message}");
                    }
                }
            }
            finally
            {
                imapclient.Disconnect();
            }
        }

        private bool HasCondition(BO.o42ImapRule rule)
        {
            return (rule.o42Condition_Subject != null || rule.o42Condition_Sender != null || rule.o42Condition_Cc != null || rule.o42Condition_Bcc != null);
        }

        private bool IsRuleMatch(BO.o42ImapRule rule, ImapMessageInfo c)
        {
            if (!HasCondition(rule)) return true;   //pravidlo bez podmínky platí pro všechny zprávy

            if (rule.o42Condition_Subject != null && c.Subject != null && c.Subject.Contains(rule.o42Condition_Subject)) return true;
            if (rule.o42Condition_Sender != null && c.Sender != null && c.Sender.Address.Contains(rule.o42Condition_Sender)) return true;
            if (rule.o42Condition_Cc != null && c.CC.Count() > 0 && c.CC.Contains(rule.o42Condition_Cc)) return true;
            if (rule.o42Condition_Bcc != null && c.Bcc.Count() > 0 && c.Bcc.Contains(rule.o42Condition_Bcc)) return true;

            return false;
        }

        /// <summary>Pravidla, která zprávu jen uloží do FILEBOXu k projektu / kontaktu / uživateli.</summary>
        private bool Handle_BindMail(Imap imapclient, ImapMessageInfo c, BO.o42ImapRule rule)
        {
            string strEntity = null; int intPid = 0;
            switch (rule.o42WhatToDoFlag)
            {
                case o42WhatToDoFlagENUM.BindWithProject:
                    strEntity = "p41"; intPid = rule.p41ID_Default;
                    break;
                case o42WhatToDoFlagENUM.BindWithContact:
                    strEntity = "p28"; intPid = rule.p28ID_Default;
                    break;
                case o42WhatToDoFlagENUM.BindWithUser:
                    strEntity = "j02"; intPid = rule.j02ID_Default;
                    break;
            }
            if (strEntity == null || intPid == 0) return false;

            var rec = _mother.o27AttachmentBL.GetRecordFromMessage(imapclient, c);
            rec.j40ID = rule.j40ID;
            rec.o27Entity = strEntity;
            rec.o27RecordPid = intPid;
            return (_mother.o27AttachmentBL.Save(rec) > 0);
        }

        /// <summary>
        /// Oznámení banky o platbě (pravidla Úhrada faktury / Úhrada zálohy). E-mail se uloží do FILEBOXu vždy, jeho pohyby do p87BankTransaction;
        /// jistou úhradu (sedí variabilní symbol i částka, platba je příchozí) zapíše p87BankTransactionBL rovnou.
        /// </summary>
        private bool Handle_BankMail(Imap imapclient, ImapMessageInfo c, List<BO.o42ImapRule> payrules)
        {
            var rec = _mother.o27AttachmentBL.GetRecordFromMessage(imapclient, c);
            rec.j40ID = payrules[0].j40ID;
            if (payrules[0].j02ID_Default > 0) rec.j02ID_Owner = payrules[0].j02ID_Default;    //komu patří nespárované platby ve frontě FILEBOXu

            var moves = BL.Code.BankMailParser.Parse(rec.o27MailSubject, BL.Code.BankMailParser.GetText(rec.o27MailBodyText, rec.o27MailBodyHtml));

            if (!payrules.Any(p => HasCondition(p)))
            {
                //pravidlo bez podmínky čte celou schránku: uloží se jen zpráva, která vypadá jako příchozí platba s variabilním symbolem
                if (!moves.Any(p => p.Direction == 1 && p.VS != null))
                {
                    DeleteEmlFile(rec);
                    return false;
                }
            }

            if (moves.Count == 0)
            {
                //zpráva od banky bez částky (není to oznámení o platbě): uloží se k pravidlu, aby se nečetla znovu
                rec.o27Entity = "o42";
                rec.o27RecordPid = payrules[0].pid;
                return (_mother.o27AttachmentBL.Save(rec) > 0);
            }

            rec.o27Entity = null;
            rec.o27RecordPid = 0;
            rec.pid = _mother.o27AttachmentBL.Save(rec);
            if (rec.pid == 0)
            {
                DeleteEmlFile(rec);
                return false;
            }

            var lis = _mother.p87BankTransactionBL.ProcessMail(rec, payrules, moves);
            BO.Code.File.LogInfo($"IMAP pravidla, bankovní e-mail {c.MessageId.Id}: pohybů {lis.Count}, zapsaných úhrad {lis.Count(p => p.p87StatusFlag == BO.p87StatusFlagENUM.SparovanoAutomaticky)}, nespárovaných {lis.Count(p => p.p87StatusFlag == BO.p87StatusFlagENUM.Nesparovano)}");
            return true;
        }

        private void DeleteEmlFile(BO.o27Attachment rec)
        {
            try
            {
                string s = $"{_mother.UploadFolder}\\{rec.o27ArchiveFolder}\\{rec.o27ArchiveFileName}";
                if (System.IO.File.Exists(s)) System.IO.File.Delete(s);
            }
            catch
            {
                //soubor zůstane na disku, zpracování to nebrání
            }
        }
    }
}
