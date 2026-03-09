using BO;
using System.Text.RegularExpressions;

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
            var lisO42 = GetList(new BO.myQuery("o42"));
            foreach (var rule in lisO42)
            {
                var recJ40 = _mother.j40MailAccountBL.Load(rule.j40ID);
                var imapclient = _mother.o43InboxBL.ConnectToImapServer(recJ40, "Inbox");
                if (imapclient == null)
                {
                    return;
                }
                imapclient.Settings.UsePeekForGetMessage = true;    //nenahazovat IsSeen flag na načtené zprávy

                var messages = _mother.o43InboxBL.GetMessageList(imapclient, 20, null, null);


                foreach (var c in messages)
                {
                    var rec = _mother.o43InboxBL.LoadByMessageId(c.MessageId.Id);
                    if (rec != null)
                    {
                        continue;
                    }
                    bool bolGo = true;
                    if (rule.o42Condition_Subject != null || rule.o42Condition_Sender != null || rule.o42Condition_Cc != null || rule.o42Condition_Bcc != null)
                    {
                        bolGo = false;
                    }
                    if (rule.o42Condition_Subject != null && c.Subject != null && c.Subject.Contains(rule.o42Condition_Subject))
                    {
                        bolGo = true;
                    }
                    if (rule.o42Condition_Sender != null && c.Sender != null && c.Sender.Address.Contains(rule.o42Condition_Sender))
                    {
                        bolGo = true;
                    }
                    if (rule.o42Condition_Cc != null && c.CC.Count() > 0 && c.CC.Contains(rule.o42Condition_Cc))
                    {
                        bolGo = true;
                    }
                    if (rule.o42Condition_Bcc != null && c.Bcc.Count() > 0 && c.Bcc.Contains(rule.o42Condition_Bcc))
                    {
                        bolGo = true;
                    }

                    if (bolGo)
                    {
                        bolGo = false;
                        rec = _mother.o43InboxBL.GetRecordFromMessage(imapclient, c);
                        rec.j40ID = rule.j40ID;

                        switch (rule.o42WhatToDoFlag)
                        {
                            case o42WhatToDoFlagENUM.BindWithProject:
                                if (rule.p41ID_Default > 0)
                                {
                                    rec.p41ID = rule.p41ID_Default;
                                    bolGo = true;
                                }
                                break;
                            case o42WhatToDoFlagENUM.BindWithContact:
                                if (rule.p28ID_Default > 0)
                                {
                                    rec.p28ID = rule.p28ID_Default;
                                    bolGo = true;
                                }
                                break;
                            case o42WhatToDoFlagENUM.BindWithUser:
                                if (rule.p28ID_Default > 0)
                                {
                                    rec.j02ID = rule.j02ID_Default;
                                    bolGo = true;
                                }
                                break;
                            case o42WhatToDoFlagENUM.InvoicePayment:    //úhrada faktury
                                rec.p91ID = Handle_UhradaFaktury(rec, rule);
                                if (rec.p91ID > 0)
                                {                                    
                                    bolGo = true;
                                }
                                break;
                            case o42WhatToDoFlagENUM.ProformaPayment:
                                rec.p90ID = Handle_UhradaZalohy(rec, rule);
                                if (rec.p90ID > 0)
                                {
                                    bolGo = true;
                                }
                                break;
                        }


                        if (bolGo)
                        {
                            _mother.o43InboxBL.Save(rec);
                        }
                        
                    }
                }

                imapclient.Disconnect();


            }
        }

        private void Najit_VS_A_Castku(BO.o43Inbox rec, BO.o42ImapRule rule,ref string strVS,ref double dblCastka )
        {
            string word_vs = _mother.CBL.GetGlobalParamValue(7, rule.pid);
            string word_castka = _mother.CBL.GetGlobalParamValue(8, rule.pid);

            if (word_vs == null || word_castka == null)
            {
                return;
            }
            
            string s = rec.o43BodyText;
            if (string.IsNullOrEmpty(s))
            {
                var htmldoc = new HtmlAgilityPack.HtmlDocument();
                htmldoc.LoadHtml(rec.o43BodyHtml);
                s = htmldoc.DocumentNode.InnerText; //převod html do plaintextu

            }
            var lines = BO.Code.Bas.ConvertString2List(s, "\r");
            foreach (var line in lines)
            {
                if (line.Contains(word_vs))
                {
                    var a = line.Split(word_vs);
                    a[1] = a[1].Replace("\"", "").Replace(".", "").Replace("\n", "");
                    if (a[1].Length > 15)
                    {
                        a[1] = a[1].Substring(0, 15);
                    }
                    strVS = ParseIntFromString(a[1]);
                }
                if (line.Contains(word_castka))
                {

                    var a = line.Split(word_castka);
                    a[1] = a[1].Replace("\"", "").Replace(".", "").Replace("\n", "");
                    if (a[1].Length > 20)
                    {
                        a[1] = a[1].Substring(0, 15);
                    }
                    s = a[1].Replace("\"", "").Replace(" ", "").Replace(".", "").Replace("\n", "").Replace("CZK", "").Replace("EUR", "").Replace("USD", "").Replace("Kč", "").Replace("-", "").Replace("+", "");
                    s = ParseAmountFromString(s);
                    dblCastka = BO.Code.Bas.InDouble(s);
                }

                if (!string.IsNullOrEmpty(strVS) && dblCastka != 0)
                {
                    return; //nalezeno
                    
                }

            }

            
        }

        private int Handle_UhradaFaktury(BO.o43Inbox rec,BO.o42ImapRule rule)  //vrací p91id faktury, kam náleží spárovaná úhrada
        {
           
            string strVS = null;double dblCastka = 0;
            Najit_VS_A_Castku(rec, rule,ref strVS,ref dblCastka);

            if (strVS !=null && dblCastka != 0)
            {
                var recP91 = _mother.p91InvoiceBL.LoadByCode(strVS);
                if (recP91 == null && BO.Code.Bas.InInt(strVS) > 0)
                {
                    string strNumericVS = strVS.TrimStart("0".ToCharArray());
                    recP91 = _mother.p91InvoiceBL.LoadByNumericCode(strNumericVS);
                }
                if (recP91 != null && recP91.p91Amount_Debt>0)
                {
                    var recP94 = new BO.p94Invoice_Payment() { p91ID = recP91.pid, p94Amount = dblCastka, p94Date = DateTime.Today };
                    if (_mother.p91InvoiceBL.SaveP94(recP94) > 0)
                    {
                        return recP91.pid;
                    }
                }
            }
                       

            return 0;
        }

        private int Handle_UhradaZalohy(BO.o43Inbox rec, BO.o42ImapRule rule)  //vrací p90id zálohy, kam náleží spárovaná úhrada
        {
            string strVS = null; double dblCastka = 0;
            Najit_VS_A_Castku(rec, rule, ref strVS, ref dblCastka);

            if (strVS != null && dblCastka != 0)
            {
                var recP90 = _mother.p90ProformaBL.LoadByCode(strVS);
                if (recP90 == null && BO.Code.Bas.InInt(strVS) > 0)
                {
                    string strNumericVS = strVS.TrimStart("0".ToCharArray());
                    recP90 = _mother.p90ProformaBL.LoadByNumericCode(strNumericVS);
                }
                if (recP90 != null && recP90.p90Amount_Debt>0)
                {
                    var recP82 = new BO.p82Proforma_Payment() { p90ID = recP90.pid, p82Amount = dblCastka, p82Date = DateTime.Today };
                    if (_mother.p82Proforma_PaymentBL.Save(recP82) > 0)
                    {
                        return recP90.pid;
                    }
                }
            }


            return 0;
        }

        private string ParseIntFromString(string s)
        {
            var reg = new Regex("[^0-9]");
            s = reg.Replace(s, "");
            return s;
        }
        private string ParseAmountFromString(string s)
        {
            var reg = new Regex(@"[^-?\d{1,3}(,\d{3})*(\.\d\d)?$|^\.\d\d$]");
            s = reg.Replace(s, "");

            return s;
        }
    }
}
