
using BO.Rejstrik;


namespace BL
{
    public interface Ib20HlidacBL
    {
        public BO.b20Hlidac Load(int pid);
        public IEnumerable<BO.b20Hlidac> GetList(BO.myQuery mq);
        public int Save(BO.b20Hlidac rec);
        public IEnumerable<BO.b21HlidacBinding> GetListB21(string rec_prefix, int rec_pid);
        public IEnumerable<BO.b21HlidacBinding> GetListB21_ValidNotClosedRecs(string rec_prefix);
        public void SaveB21List(string record_prefix, int record_pid, List<BO.b21HlidacBinding> lisB21);
        public IEnumerable<BO.j94HlidacLog> GetList_j94(int b21id, int toprecs);
        public bool Handle_NastalaUdalostHlidace(BO.b20Hlidac recB20,BO.b21HlidacBinding recB21,ref System.Text.StringBuilder sb,ref List<string> notify2emails, HttpClient hc = null, System.Text.StringBuilder sbText = null, bool force = false);
        public string RunSystemNow(int b20id, bool reset, BO.j40MailAccount smtp_account);
        public bool SendNotification(System.Text.StringBuilder sb, List<string> emails, BO.b20Hlidac recB20, BO.j40MailAccount smtp_account);
        public string Deliver(BO.b20Hlidac recB20, System.Text.StringBuilder sbHtml, System.Text.StringBuilder sbText, List<string> emails, BO.j40MailAccount smtp_account);
        public int SendPending(BO.j40MailAccount smtp_account);

    }
    class b20HlidacBL : BaseBL, Ib20HlidacBL
    {
        public b20HlidacBL(BL.Factory mother) : base(mother)
        {

        }


        private string GetSQL1(string strAppend = null)
        {
            sb("SELECT a.*,");
            sb(_db.GetSQL1_Ocas("b20"));
            sb(" FROM b20Hlidac a");
            sb(this.AppendCloudQuery(strAppend));
            return sbret();
        }
        public BO.b20Hlidac Load(int pid)
        {
            return _db.Load<BO.b20Hlidac>(GetSQL1(" WHERE a.b20ID=@pid"), new { pid = pid });
        }


        public IEnumerable<BO.b20Hlidac> GetList(BO.myQuery mq)
        {
            if (mq.explicit_orderby == null) { mq.explicit_orderby = "a.b20Ordinary"; };
            DL.FinalSqlCommand fq = DL.basQuery.GetFinalSql(GetSQL1(), mq, _mother.CurrentUser);
            return _db.GetList<BO.b20Hlidac>(fq.FinalSql, fq.Parameters);
        }



        public int Save(BO.b20Hlidac rec)
        {
            if (ValidateBeforeSave(rec) == false)
            {
                return 0;
            }
            var p = new DL.Params4Dapper();
            p.AddInt("pid", rec.pid);
            p.AddEnumInt("b20TypeFlag", rec.b20TypeFlag);
            p.AddString("b20Name", rec.b20Name);
            p.AddString("b20Entity", rec.b20Entity);
            p.AddInt("b20Ordinary", rec.b20Ordinary);
            p.AddString("b20Par1Name", rec.b20Par1Name);
            p.AddString("b20Par2Name", rec.b20Par2Name);
            p.AddString("b20EntityAlias", rec.b20EntityAlias);

            if (rec.b20TypeFlag == BO.b20TypeFlagEnum.TestBySql)
            {
                p.AddString("b20RunSql", rec.b20RunSql);
                p.AddString("b20TestSql", rec.b20TestSql);
            }
            else
            {
                p.AddString("b20RunSql",null);
                p.AddString("b20TestSql", null);
            }
            

            p.AddInt("j61ID", rec.j61ID, true);
            p.AddInt("x01ID", rec.x01ID == 0 ? _mother.CurrentUser.x01ID : rec.x01ID);
            p.AddInt("b20NextRunAfterMinutes", rec.b20NextRunAfterMinutes);
            p.AddString("b20TestTimeFrom", rec.b20TestTimeFrom);
            p.AddString("b20TestTimeUntil", rec.b20TestTimeUntil);

            p.AddString("b20NotifyReceivers", rec.b20Entity == "x01" ? null : rec.b20NotifyReceivers);     //hlídač za celou databázi notifikuje jen tým (j11ID_Notify)
            p.AddString("b20NotifyMessage", rec.b20NotifyMessage);

            p.AddBool("b20IsNotifyRecordOwner", rec.b20IsNotifyRecordOwner);
            p.AddInt("j11ID_Notify", rec.j11ID_Notify, true);
            p.AddInt("x67ID_Notify2", rec.x67ID_Notify2, true);
            p.AddInt("x67ID_Notify1", rec.x67ID_Notify1, true);
            p.AddString("b20ListEntity", rec.IsSystem && rec.b20TypeFlag == BO.b20TypeFlagEnum.TestBySql && !string.IsNullOrWhiteSpace(rec.b20ListEntity) ? rec.b20ListEntity.Trim() : null);
            int intPID = _db.SaveRecord("b20Hlidac", p, rec);
            if (intPID > 0)
            {
                SyncSystemBinding(intPID, rec.b20Entity == "x01", rec.x01ID == 0 ? _mother.CurrentUser.x01ID : rec.x01ID);
            }
            return intPID;
        }

        /// <summary>
        /// Hlídač za celou databázi (b20Entity = "x01") se k záznamům nepřiřazuje: jeho jedinou vazbu na licenci (b21RecordEntity = 'x01')
        /// zakládá a ruší uložení hlídače. Na vazbě visí log j94HlidacLog a robot podle ní hlídač testuje.
        /// </summary>
        private void SyncSystemBinding(int b20id, bool issystem, int x01id)
        {
            if (issystem)
            {
                _db.RunSql("IF NOT EXISTS(SELECT 1 FROM b21HlidacBinding WHERE b20ID=@pid AND b21RecordEntity='x01') INSERT INTO b21HlidacBinding(b20ID,b21RecordEntity,b21RecordPid) VALUES(@pid,'x01',@x01id)", new { pid = b20id, x01id = x01id });
            }
            else
            {
                _db.RunSql("DELETE FROM j94HlidacLog WHERE b21ID IN (SELECT b21ID FROM b21HlidacBinding WHERE b20ID=@pid AND b21RecordEntity='x01'); DELETE FROM b21HlidacBinding WHERE b20ID=@pid AND b21RecordEntity='x01'", new { pid = b20id });
            }
        }

        /// <summary>Typy záznamů, které smí vracet SQL hlídače za celou databázi.</summary>
        public static readonly string[] ListEntities = new[] { "p41", "p28", "p56", "p91", "o23", "p90", "j02" };

        public bool ValidateBeforeSave(BO.b20Hlidac rec)
        {
            if (string.IsNullOrEmpty(rec.b20Name))
            {
                this.AddMessage("Chybí vyplnit [Název]."); return false;
            }
            if (string.IsNullOrEmpty(rec.b20Entity))
            {
                this.AddMessage("Chybí vyplnit [Entita]."); return false;
            }
            if ((rec.b20TypeFlag == BO.b20TypeFlagEnum.ZmenaAdresy || rec.b20TypeFlag == BO.b20TypeFlagEnum.Insolvence) && rec.b20NextRunAfterMinutes < 1000)
            {
                this.AddMessageTranslated("Chybí vyplnit [Testovat pravidelně každých X minut]. Minimální hodnota u tohoto typu hlídače je 1000 minut."); return false;
            }
            if (rec.IsSystem && rec.b20TypeFlag != BO.b20TypeFlagEnum.TestBySql)
            {
                this.AddMessage("Hlídač za celou databázi (entita Systém) může být pouze typu [Testování výsledku SQL dotazu]."); return false;
            }
            if (rec.IsSystem && !string.IsNullOrWhiteSpace(rec.b20ListEntity) && !ListEntities.Contains(rec.b20ListEntity.Trim()))
            {
                this.AddMessage("Neplatný typ záznamů v seznamu."); return false;
            }
            if (rec.IsCyklus && rec.b20NextRunAfterMinutes < 5)
            {
                this.AddMessage("Chybí vyplnit [Testovat pravidelně každých X minut]. Minimální hodnota: 5 minut.");return false;
            }

            
            return true;
        }

        private string GetSQL1_b21(string strAppend = null)
        {
            sb("SELECT a.*,a.b21ID as pid,b20.b20TypeFlag,b20.b20Par1Name,b20.b20Par2Name,b20.b20NotifyReceivers,b20.b20Name,b20.b20TypeFlag");
            sb(" FROM b21HlidacBinding a INNER JOIN b20Hlidac b20 ON a.b20ID=b20.b20ID");
            if (_mother.CurrentUser.IsHostingModeTotalCloud)
            {                
                sb($" AND b20.x01ID={_mother.CurrentUser.x01ID}");
            }
            sb(strAppend);
            return sbret();
        }
        public IEnumerable<BO.b21HlidacBinding> GetListB21(string rec_prefix, int rec_pid)
        {
            return _db.GetList<BO.b21HlidacBinding>(GetSQL1_b21(" WHERE a.b21RecordEntity=@prefix AND a.b21RecordPid=@pid"), new { prefix = rec_prefix, pid = rec_pid });
        }

        public IEnumerable<BO.b21HlidacBinding> GetListB21_ValidNotClosedRecs(string rec_prefix)
        {
            string s = " WHERE a.b21RecordEntity=@prefix";
            s += $" AND a.b21RecordPid IN (select {rec_prefix}ID FROM {BO.Code.Entity.GetEntity(rec_prefix)} WHERE GETDATE() BETWEEN {rec_prefix}ValidFrom AND {rec_prefix}ValidUntil)";

            return _db.GetList<BO.b21HlidacBinding>(GetSQL1_b21(s), new { prefix = rec_prefix });
        }
        public IEnumerable<BO.j94HlidacLog> GetList_j94(int b21id, int toprecs)
        {
            return _db.GetList<BO.j94HlidacLog>($"select TOP {toprecs} * from j94HlidacLog where b21ID=@b21id ORDER BY j94ID DESC", new { b21id = b21id });
        }

        public void SaveB21List(string record_prefix, int record_pid, List<BO.b21HlidacBinding> lisB21)
        {

            foreach (var c in lisB21)
            {
                if (c.IsSetAsDeleted)
                {
                    if (c.pid > 0)
                    {
                        _db.RunSql("DELETE FROM j94HlidacLog WHERE b21ID=@pid; DELETE FROM b21HlidacBinding WHERE b21ID=@pid", new { pid = c.pid });
                    }
                }
                else
                {
                    var rec = new BO.b21HlidacBinding();
                    if (c.pid > 0)
                    {
                        rec = _db.Load<BO.b21HlidacBinding>(GetSQL1_b21(" WHERE a.b21ID=@pid"), new { pid = c.pid });
                    }
                    var p = new DL.Params4Dapper();
                    p.AddInt("pid", rec.pid);
                    p.AddInt("b20ID", c.b20ID, true);
                    p.AddString("b21RecordEntity", record_prefix);
                    p.AddInt("b21RecordPid", record_pid, true);
                    p.AddDouble("b21Par1", c.b21Par1);
                    p.AddDouble("b21Par2", c.b21Par2);
                    p.AddString("b21NotifyMessage", c.b21NotifyMessage);
                    p.AddString("b21NotifyReceivers", c.b21NotifyReceivers);

                    _db.SaveRecord("b21HlidacBinding", p, rec, false, false);
                }
            }

        }

        /// <param name="sbText">nepovinné: popis události prostým textem pro interní chat (HTML v sb je pro e-mail)</param>
        /// <param name="force">ruční spuštění: netestuje se časové okno ani interval od posledního testu</param>
        public bool Handle_NastalaUdalostHlidace(BO.b20Hlidac recB20,BO.b21HlidacBinding recB21,ref System.Text.StringBuilder sb, ref List<string> notify2emails, HttpClient hc = null, System.Text.StringBuilder sbText = null, bool force = false)
        {            
            var lisLog = GetList_j94(recB21.pid, 1);
            if (!force && recB20.b20TestTimeFrom !=null && recB20.b20TestTimeUntil != null)   //hlídač se má spouštět pouze v čase od-do
            {
                int secs1 = BO.Code.Time.ConvertTimeToSeconds(recB20.b20TestTimeFrom);
                int secs2 = BO.Code.Time.ConvertTimeToSeconds(recB20.b20TestTimeUntil);
                if (DateTime.Now < DateTime.Today.AddSeconds(secs1) || DateTime.Now>DateTime.Today.AddSeconds(secs2))
                {
                    return false;
                }
                
            }
            if (!force && lisLog.Count() > 0 && recB20.b20NextRunAfterMinutes>0)
            {
                double ellapsed_minutes = (DateTime.Now - lisLog.First().j94Date).TotalMinutes;
                if (ellapsed_minutes < Convert.ToDouble(recB20.b20NextRunAfterMinutes))
                {
                    //ještě nenastal čas k otestování
                    return false;
                }
            }
            bool bolAchieved = false; string strRobotValue = null;
            bool bolList = recB20.IsSystemList;     //hlídač za celou databázi: SQL vrací seznam záznamů
            if (recB20.b20TypeFlag == BO.b20TypeFlagEnum.TestBySql)
            {
                if (bolList)
                {
                    bolAchieved = Handle_TestovatHlidace_List(ref sb, sbText, recB20, recB21, ref strRobotValue);
                }
                else
                {
                    bolAchieved = Handle_TestovatHlidace_Sql(ref sb, recB20, recB21, lisLog, ref strRobotValue);    //hlídač typu testování obsahu sql dotazu
                }
            }
            if (recB20.b20TypeFlag == BO.b20TypeFlagEnum.ZmenaAdresy && recB20.b20Entity=="p28")
            {
                bolAchieved = Handle_TestovatHlidace_ZmenaAdresy(ref sb,hc,recB20, recB21, lisLog, ref strRobotValue);    //hlídač typu změny sídla kontaktu


            }

            _db.RunSql("INSERT INTO j94HlidacLog(b21id,j94Date,j94IsAchieved,j94RobotValue,j94Par1,j94Par2,j94Message) VALUES(@b21id,GETDATE(),@b,@val,@par1,@par2,@message)", new { b21id = recB21.pid, b = bolAchieved, val = strRobotValue, par1 = recB21.b21Par1, par2 = recB21.b21Par2, message = recB21.b21NotifyMessage });


            if (bolAchieved && recB20.b20RunSql != null)
            {
                //hlídač má výstupní sql dotaz ke spuštění
                string strSQL = recB20.b20RunSql.Replace("@pid", recB21.b21RecordPid.ToString()).Replace("@par1", recB21.b21Par1.ToString()).Replace("@par2", recB21.b21Par2.ToString());
                _mother.FBL.RunSql(strSQL);
            }
            if (bolAchieved && sbText != null && !bolList)
            {
                AppendEventText(sbText, recB20, recB21, strRobotValue);
            }
            if (bolAchieved)
            {
                //hlídač za celou databázi nemá vlastníka záznamu ani role v záznamu (licence nemá j02ID_Owner) - příjemci jsou jen členové týmu (j11ID_Notify)
                int intJ02ID_Owner = (recB20.b20IsNotifyRecordOwner && !recB20.IsSystem ? _mother.CBL.GetRecord_j02ID_Owner(recB20.b20Entity, recB21.b21RecordPid) : 0);

                var emails = _mother.MailBL.GetMailList(intJ02ID_Owner, recB20.j11ID_Notify, recB20.IsSystem ? 0 : recB20.x67ID_Notify1,0,0, recB21.b21RecordPid, recB20.b20Entity,0);
                if (recB21.b21NotifyReceivers != null)
                {
                    recB21.b21NotifyReceivers = recB21.b21NotifyReceivers.Replace(";", ",");    //ručně zadaní příjemci v notifikovaném záznamu
                    emails.InsertRange(0,BO.Code.Bas.ConvertString2List(recB21.b21NotifyReceivers));
                    
                }
                if (emails !=null && emails.Count() > 0)
                {
                    notify2emails.InsertRange(0, emails);
                }
                if (recB20.b20NotifyReceivers != null && !recB20.IsSystem)
                {
                    notify2emails.Insert(0, recB20.b20NotifyReceivers);
                }
            }
            return bolAchieved;
        }

        // ── hlídač za celou databázi se seznamem záznamů ────────────────────────────

        /// <summary>Nejvýš tolik záznamů ze SQL se bere v úvahu.</summary>
        public const int ListMaxRecords = 20000;
        /// <summary>Tolik nově zachycených záznamů se v notifikaci vyjmenuje.</summary>
        public const int ListShown = 20;
        /// <summary>Do odkazu na přehled se vejde nejvýš tolik záznamů (délka adresy).</summary>
        public const int ListLinkMax = 150;

        /// <summary>
        /// Hlídač za celou databázi: b20TestSql vrací seznam záznamů (1. sloupec = ID záznamu typu b20ListEntity, nepovinný 2. sloupec = hodnota k zobrazení).
        /// Událost = v seznamu jsou záznamy, které v něm při minulém testu nebyly. Notifikace je jedna za hlídač a obsahuje jen nově zachycené.
        /// Stav se drží v j94HlidacLog po záznamech (j94RecordEntity, j94RecordPid): poslední řádek záznamu s j94IsAchieved=1 = záznam je zachycen,
        /// 0 = ze seznamu vypadl (když se do něj vrátí, ohlásí se znovu). Vypadlé záznamy se nehlásí.
        /// </summary>
        private bool Handle_TestovatHlidace_List(ref System.Text.StringBuilder sb, System.Text.StringBuilder sbText, BO.b20Hlidac recB20, BO.b21HlidacBinding recB21, ref string strRobotValue)
        {
            string e = recB20.b20ListEntity.Trim();
            if (!ListEntities.Contains(e)) return false;
            string strTable = BO.Code.Entity.GetEntity(e);

            var strSQL = recB20.b20TestSql.Replace("@pid", recB21.b21RecordPid.ToString()).Replace("@par1", recB21.b21Par1.ToString()).Replace("@par2", recB21.b21Par2.ToString());
            var dt = _mother.FBL.GetDataTable(strSQL);

            var current = new Dictionary<int, string>();    //ID záznamu → hodnota k zobrazení
            foreach (System.Data.DataRow dbrow in dt.Rows)
            {
                if (dbrow[0] == System.DBNull.Value || !int.TryParse(dbrow[0].ToString(), out int intPid) || intPid <= 0 || current.ContainsKey(intPid)) continue;
                current[intPid] = (dt.Columns.Count > 1 && dbrow[1] != System.DBNull.Value) ? dbrow[1].ToString() : null;
                if (current.Count >= ListMaxRecords) break;
            }
            strRobotValue = current.Count.ToString();   //do logu testu: kolik záznamů pravidlo právě splňuje

            //záznamy zachycené při minulých testech (poslední řádek záznamu říká "zachycen")
            var lisActive = _db.GetList<BO.GetInteger>(@"SELECT a.j94RecordPid as Value FROM j94HlidacLog a
WHERE a.b21ID=@b21id AND a.j94RecordEntity=@e AND a.j94RecordPid IS NOT NULL AND a.j94IsAchieved=1
AND a.j94ID=(SELECT MAX(x.j94ID) FROM j94HlidacLog x WHERE x.b21ID=a.b21ID AND x.j94RecordEntity=a.j94RecordEntity AND x.j94RecordPid=a.j94RecordPid)", new { b21id = recB21.pid, e = e });
            var active = lisActive == null ? new HashSet<int>() : new HashSet<int>(lisActive.Select(p => p.Value));

            var left = active.Where(p => !current.ContainsKey(p)).ToList();
            var fresh = current.Keys.Where(p => !active.Contains(p)).ToList();

            //jen záznamy, které v tabulce entity skutečně existují (ID, které by SQL vracelo omylem, by se hlásilo při každém testu)
            var valid = new HashSet<int>();
            foreach (var chunk in fresh.Chunk(1000))
            {
                var lis = _db.GetList<BO.GetInteger>($"SELECT {e}ID as Value FROM {strTable} WHERE {e}ID IN ({string.Join(",", chunk)})");
                if (lis != null) foreach (var c in lis) valid.Add(c.Value);
            }
            fresh = fresh.Where(p => valid.Contains(p)).ToList();

            foreach (var chunk in fresh.Chunk(1000))
            {
                _db.RunSql($"INSERT INTO j94HlidacLog(b21ID,j94Date,j94IsAchieved,j94RecordEntity,j94RecordPid) SELECT @b21id,GETDATE(),1,@e,{e}ID FROM {strTable} WHERE {e}ID IN ({string.Join(",", chunk)})", new { b21id = recB21.pid, e = e });
            }
            foreach (var chunk in left.Chunk(1000))
            {
                _db.RunSql($"INSERT INTO j94HlidacLog(b21ID,j94Date,j94IsAchieved,j94RecordEntity,j94RecordPid) SELECT DISTINCT @b21id,GETDATE(),0,@e,a.j94RecordPid FROM j94HlidacLog a WHERE a.b21ID=@b21id AND a.j94RecordEntity=@e AND a.j94RecordPid IN ({string.Join(",", chunk)})", new { b21id = recB21.pid, e = e });
            }
            if (fresh.Count == 0) return false;

            //notifikace: počet, prvních ListShown záznamů, odkaz na přehled
            string strAlias = recB20.b20EntityAlias ?? BO.Code.Entity.GetAlias(e);
            var shown = new List<string>();
            foreach (int intPid in fresh.Take(ListShown))
            {
                string s = _mother.CBL.GetObjectAlias(e, intPid) ?? $"#{intPid}";
                if (!string.IsNullOrEmpty(current[intPid])) s += $": {current[intPid]}";
                shown.Add(s);
            }
            string strMore = fresh.Count > ListShown ? $"… a dalších {fresh.Count - ListShown}" : null;
            string strLink = null;
            string strHost = _mother.Lic?.x01AppHost;
            if (!string.IsNullOrEmpty(strHost) && strHost.StartsWith("http", StringComparison.OrdinalIgnoreCase))
            {
                strLink = $"{strHost.TrimEnd('/')}/TheGrid/FlatView?prefix={e}&myqueryinline=pids|list_int|{string.Join(",", fresh.Take(ListLinkMax))}";
            }
            string strLinkLabel = fresh.Count > ListLinkMax ? $"Otevřít prvních {ListLinkMax} nově zachycených v přehledu" : "Otevřít nově zachycené v přehledu";

            sb.AppendLine("<table>");
            sb.AppendLine($"<tr><td>Nově zachyceno ({System.Net.WebUtility.HtmlEncode(strAlias)}):</td><td><strong style='color:navy;'>{fresh.Count}</strong></td></tr>");
            sb.AppendLine($"<tr><td>Pravidlo nyní splňuje celkem:</td><td><code>{current.Count}</code></td></tr>");
            sb.AppendLine("</table>");
            sb.AppendLine("<ul>");
            foreach (string s in shown) sb.AppendLine($"<li>{System.Net.WebUtility.HtmlEncode(s)}</li>");
            if (strMore != null) sb.AppendLine($"<li>{strMore}</li>");
            sb.AppendLine("</ul>");
            if (strLink != null) sb.AppendLine($"<p><a href=\"{System.Net.WebUtility.HtmlEncode(strLink)}\">{strLinkLabel}</a></p>");
            sb.AppendLine("<hr>");

            if (sbText != null)
            {
                sbText.AppendLine($"Nově zachyceno ({strAlias}): {fresh.Count}, pravidlo nyní splňuje celkem {current.Count}.");
                foreach (string s in shown) sbText.AppendLine("• " + s);
                if (strMore != null) sbText.AppendLine(strMore);
                if (strLink != null) sbText.AppendLine($"{strLinkLabel}: {strLink}");
                sbText.AppendLine();
            }
            return true;
        }

        private bool Handle_TestovatHlidace_ZmenaAdresy(ref System.Text.StringBuilder sb, HttpClient hc,BO.b20Hlidac recB20, BO.b21HlidacBinding recB21, IEnumerable<BO.j94HlidacLog> lisLog, ref string strRobotValue)
        {
            var cr = new BL.Code.RejstrikySupport(null);
            var recP28 = _mother.p28ContactBL.Load(recB21.b21RecordPid);
            DefaultZaznam c = null;string strPole = null;
            try
            {
                if (recP28.p28RegID != null)
                {
                    c = cr.LoadDefaultZaznam("ico", recP28.p28RegID, recP28.p28CountryCode, hc).Result; strPole = $"IČO: {recP28.p28RegID}";
                }
                else
                {
                    if (recP28.p28VatID != null)
                    {
                        c = cr.LoadDefaultZaznam("dic", recP28.p28VatID, recP28.p28CountryCode, hc).Result; strPole = $"DIČ: {recP28.p28VatID} ";
                    }
                }
            }
            catch
            {
                
            }
            
            if (c == null) return false;

            strRobotValue = c.fulladdress;
            bool b = false;
            if (BO.Code.Bas.Diff2String(recP28.p28Street1, c.street)) b = true;
            if (!b && BO.Code.Bas.Diff2String(recP28.p28City1, c.city)) b = true;
            if (!b && BO.Code.Bas.Diff2String(recP28.p28PostCode1, c.zipcode)) b = true;

            if (b)
            {
                wrl_predmet_udalosti(ref sb, recB20, recB21,strPole);
                
                sb.AppendLine($"<tr><td>Adresa #1 v kartě kontaktu:</td> <td><code>{recP28.p28City1}, {recP28.p28Street1}, {recP28.p28PostCode1}</code></td></tr>");
                sb.AppendLine($"<tr><td>Aktuální adresa v rejstříku:</td> <td><strong style='color:navy;'>{c.city}, {c.street}, {c.zipcode}</strong></td></tr>");
                sb.AppendLine("</table><hr>");
            }

            return b;

        }
       
        private bool Handle_TestovatHlidace_Sql(ref System.Text.StringBuilder sb, BO.b20Hlidac recB20,BO.b21HlidacBinding recB21,IEnumerable<BO.j94HlidacLog> lisLog,ref string strRobotValue)
        {
            bool bolNastalaUdalost = false;
            strRobotValue = null;
            var strSQL = recB20.b20TestSql.Replace("@pid", recB21.b21RecordPid.ToString()).Replace("@par1", recB21.b21Par1.ToString()).Replace("@par2", recB21.b21Par2.ToString());
            var dt = _mother.FBL.GetDataTable(strSQL);
            if (dt.Rows.Count > 0)
            {
                //dotaz vrátil záznam, je třeba otestovat, zda nastala událost
                bolNastalaUdalost = (lisLog.Count() == 0 ? true : false);

                if (dt.Rows[0][0] != System.DBNull.Value) strRobotValue = dt.Rows[0][0].ToString();

                if (!bolNastalaUdalost)
                {
                    var c = lisLog.First();
                    if (strRobotValue != c.j94RobotValue || c.j94Par1 != recB21.b21Par1 || c.j94Par2 != recB21.b21Par2)
                    {
                        bolNastalaUdalost = true;
                    }
                }

                if (bolNastalaUdalost)
                {
                    wrl_predmet_udalosti(ref sb, recB20, recB21,null);
                   
                    if (strRobotValue != null)
                    {
                        sb.AppendLine($"<tr><td>Aktuální hodnota:</td> <td><strong style='color:navy;'>{strRobotValue}</strong></td>");
                    }
                    sb.AppendLine("</table><hr>");
                }
            }

            return bolNastalaUdalost;
        }

        

        private void wrl_predmet_udalosti(ref System.Text.StringBuilder sb,BO.b20Hlidac recB20,BO.b21HlidacBinding recB21,string strHledanoPodle)
        {
            if (recB21.b21NotifyMessage != null)
            {
                sb.AppendLine($"<h4>{recB21.b21NotifyMessage}</h4>");
            }
            sb.AppendLine("<table>");
            sb.AppendLine($"<tr><td>{(recB20.b20EntityAlias==null ? BO.Code.Entity.GetAlias(recB20.b20Entity) : recB20.b20EntityAlias)}:</td><td> <strong>{_mother.CBL.GetObjectAlias(recB20.b20Entity, recB21.b21RecordPid)}</strong><span style='margin-left:20px;'>{strHledanoPodle}</pan></td>");
            if (recB20.b20Par1Name != null)
            {
                sb.AppendLine($"<tr><td>{recB20.b20Par1Name}:</td> <td><code>{recB21.b21Par1}</code></td>");
            }
            if (recB20.b20Par2Name != null)
            {
                sb.AppendLine($"<tr><td>{recB20.b20Par2Name}:</td> <td><code>{recB21.b21Par2}</code></td>");
            }
        }


        // ── ruční spuštění hlídače za celou databázi ────────────────────────────────

        /// <summary>
        /// Ruční spuštění hlídače za celou databázi z jeho karty (administrátor): otestuje hned, bez ohledu na interval a časové okno,
        /// a případnou událost doručí stejně jako robot (chat + e-mail). Testuje se uložený stav hlídače.
        /// </summary>
        /// <param name="reset">true = nejdřív zapomenout dosavadní log (zachycené záznamy, poslední hodnotu) - test se chová jako úplně první</param>
        /// <returns>popis výsledku pro uživatele</returns>
        public string RunSystemNow(int b20id, bool reset, BO.j40MailAccount smtp_account)
        {
            var recB20 = Load(b20id);
            if (recB20 == null) return "Hlídač nebyl nalezen.";
            if (!recB20.IsSystem || recB20.b20TypeFlag != BO.b20TypeFlagEnum.TestBySql) return "Ručně lze spustit jen hlídač za celou databázi (entita Systém).";
            if (string.IsNullOrWhiteSpace(recB20.b20TestSql)) return "Hlídač nemá vyplněný testovací SQL dotaz.";

            int intX01ID = recB20.x01ID == 0 ? _mother.CurrentUser.x01ID : recB20.x01ID;
            SyncSystemBinding(recB20.pid, true, intX01ID);
            var recB21 = GetListB21("x01", intX01ID).FirstOrDefault(p => p.b20ID == recB20.pid);
            if (recB21 == null) return "Hlídač nemá vazbu na databázi – uložte jeho kartu a zkuste to znovu.";

            if (reset)
            {
                _db.RunSql("DELETE FROM j94HlidacLog WHERE b21ID=@pid", new { pid = recB21.pid });
            }

            var sb = new System.Text.StringBuilder();
            var sbText = new System.Text.StringBuilder();
            var emails = new List<string>();
            bool bolAchieved;
            try
            {
                bolAchieved = Handle_NastalaUdalostHlidace(recB20, recB21, ref sb, ref emails, null, sbText, true);
            }
            catch (Exception e)
            {
                return "Test hlídače skončil chybou (nejspíš chyba v SQL dotazu): " + e.Message;
            }

            string strValue = GetList_j94(recB21.pid, 1).FirstOrDefault()?.j94RobotValue;
            string strState = recB20.IsSystemList ? $"Pravidlo nyní splňuje záznamů: {strValue ?? "0"}." : (strValue == null ? "SQL nevrátil žádný řádek." : $"Aktuální hodnota: {strValue}.");
            if (!bolAchieved) return "Test proběhl, žádná nová událost – nic se neodeslalo. " + strState;

            sb.Insert(0, recB20.b20NotifyMessage ?? $"<h2>Hlídač zachytil událost [{recB20.b20Name}]!</h2>");
            sb.AppendLine($"<small>Čas: {DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss")} (ruční spuštění)</small>");
            return "Hlídač zachytil událost. " + strState + " " + Deliver(recB20, sb, sbText, emails, smtp_account);
        }

        // ── doručení: interní chat + e-mail ─────────────────────────────────────────

        private static readonly string[] _recPageEntities = new[] { "p28", "p41", "p56", "p91" };

        /// <summary>Popis jedné zachycené události prostým textem (zpráva v interním chatu).</summary>
        private void AppendEventText(System.Text.StringBuilder sbText, BO.b20Hlidac recB20, BO.b21HlidacBinding recB21, string strRobotValue)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(recB21.b21NotifyMessage)) sbText.AppendLine(recB21.b21NotifyMessage.Trim());
                sbText.AppendLine($"{(recB20.b20EntityAlias == null ? BO.Code.Entity.GetAlias(recB20.b20Entity) : recB20.b20EntityAlias)}: {_mother.CBL.GetObjectAlias(recB20.b20Entity, recB21.b21RecordPid)}");
                if (recB20.b20Par1Name != null) sbText.AppendLine($"{recB20.b20Par1Name}: {recB21.b21Par1}");
                if (recB20.b20Par2Name != null) sbText.AppendLine($"{recB20.b20Par2Name}: {recB21.b21Par2}");
                if (!string.IsNullOrEmpty(strRobotValue))
                {
                    sbText.AppendLine($"{(recB20.b20TypeFlag == BO.b20TypeFlagEnum.ZmenaAdresy ? "Aktuální adresa v rejstříku" : "Aktuální hodnota")}: {strRobotValue}");
                }
                string strHost = _mother.Lic?.x01AppHost;
                if (!string.IsNullOrEmpty(strHost) && strHost.StartsWith("http", StringComparison.OrdinalIgnoreCase) && _recPageEntities.Contains(recB20.b20Entity))
                {
                    sbText.AppendLine($"{strHost.TrimEnd('/')}/Record/RecPage?prefix={recB20.b20Entity}&pid={recB21.b21RecordPid}");     //chat z adresy udělá odkaz
                }
                sbText.AppendLine();
            }
            catch
            {
                //text pro chat nesmí shodit testování hlídače
            }
        }

        /// <summary>
        /// Doručí zachycené události jednoho hlídače: do interního chatu (příjemcům, kteří jsou uživatelé s chatem) a e-mailem (všem příjemcům).
        /// E-mail, který se nepodařilo odeslat (chybí SMTP účet, chyba odeslání), se uloží a odešle při dalším běhu robota (<see cref="SendPending"/>) -
        /// událost se totiž podruhé neohlásí, hlídač ji už má v j94HlidacLog.
        /// Vrací popis výsledku pro log robota.
        /// </summary>
        public string Deliver(BO.b20Hlidac recB20, System.Text.StringBuilder sbHtml, System.Text.StringBuilder sbText, List<string> emails, BO.j40MailAccount smtp_account)
        {
            emails = (emails ?? new List<string>()).Where(p => !string.IsNullOrWhiteSpace(p))
                .SelectMany(p => p.Split(new[] { ';', ',' }, StringSplitOptions.RemoveEmptyEntries)).Select(p => p.Trim()).Where(p => p != "")
                .Distinct(StringComparer.OrdinalIgnoreCase).ToList();
            if (emails.Count == 0) return "Hlídač nemá žádného příjemce - upozornění nebylo odesláno.";

            int intChat = 0;
            if (sbText != null && sbText.Length > 0)
            {
                string strHead = string.IsNullOrWhiteSpace(recB20.b20NotifyMessage) ? $"Hlídač zachytil událost [{recB20.b20Name}]" : BO.Code.Bas.Html2Text(recB20.b20NotifyMessage);
                intChat = _mother.ChatBL.PostNotification(emails, "🔔 " + (strHead ?? "").Trim() + "\n\n" + sbText.ToString().Trim());
            }

            bool bolMail = false;
            try
            {
                bolMail = smtp_account != null && SendNotification(sbHtml, emails, recB20, smtp_account);
            }
            catch
            {
                bolMail = false;
            }
            if (bolMail) return $"Upozornění odesláno: e-mail ({emails.Count}), chat ({intChat}).";

            bool bolSaved = SavePending(recB20, sbHtml, emails);
            return $"E-mail se nepodařilo odeslat ({(smtp_account == null ? "chybí výchozí SMTP účet" : "chyba odeslání")}), chat ({intChat}). "
                 + (bolSaved ? "E-mail je uložen a odešle se při dalším běhu robota." : "E-mail se nepodařilo uložit k pozdějšímu odeslání!");
        }

        // neodeslané e-maily hlídačů: p85Tempbox, p85Prefix='b20mail', p85GUID='b20mail-{x01ID}', p85DataPID=b20ID, p85Message=JSON
        private const string PendingPrefix = "b20mail";
        private const int PendingDays = 7;
        private string PendingGuid => $"{PendingPrefix}-{_mother.CurrentUser.x01ID}";

        private class PendingMail
        {
            public List<string> to { get; set; }
            public string body { get; set; }
        }

        private bool SavePending(BO.b20Hlidac recB20, System.Text.StringBuilder sbHtml, List<string> emails)
        {
            try
            {
                return _mother.p85TempboxBL.Save(new BO.p85Tempbox()
                {
                    p85GUID = PendingGuid,
                    p85Prefix = PendingPrefix,
                    p85DataPID = recB20.pid,
                    p85OtherKey1 = _mother.CurrentUser.x01ID,
                    p85FreeDate01 = DateTime.Now,
                    p85Message = System.Text.Json.JsonSerializer.Serialize(new PendingMail() { to = emails, body = sbHtml.ToString() })
                }) > 0;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Odešle e-maily hlídačů, které se dříve nepodařilo odeslat. Volá robot před testováním hlídačů.
        /// Starší než 7 dní se zahodí. Vrací počet odeslaných.
        /// </summary>
        public int SendPending(BO.j40MailAccount smtp_account)
        {
            if (smtp_account == null) return 0;
            int intSent = 0;
            try
            {
                var lis = _mother.p85TempboxBL.GetList(PendingGuid, false, PendingPrefix)?.ToList();
                if (lis == null) return 0;
                foreach (var c in lis)
                {
                    PendingMail m = null;
                    try { m = System.Text.Json.JsonSerializer.Deserialize<PendingMail>(c.p85Message ?? ""); } catch { m = null; }
                    var recB20 = c.p85DataPID > 0 ? Load(c.p85DataPID) : null;
                    bool bolOld = c.p85FreeDate01 == null || c.p85FreeDate01.Value.AddDays(PendingDays) < DateTime.Now;
                    if (m == null || m.to == null || m.to.Count == 0 || recB20 == null || bolOld)
                    {
                        _mother.p85TempboxBL.VirtualDelete(c.pid); continue;
                    }
                    if (SendNotification(new System.Text.StringBuilder(m.body), m.to, recB20, smtp_account))
                    {
                        _mother.p85TempboxBL.VirtualDelete(c.pid);
                        intSent += 1;
                    }
                }
            }
            catch (Exception e)
            {
                BO.Code.File.LogError(e.Message, _mother.CurrentUser?.j02Login, "b20HlidacBL.SendPending");
            }
            return intSent;
        }

        public bool SendNotification(System.Text.StringBuilder sb, List<string> emails, BO.b20Hlidac recB20,BO.j40MailAccount smtp_account)
        {
            
            if (smtp_account == null) return false;

            var recX40 = new BO.x40MailQueue()
            {
                x40RecordEntity = "b20",
                x40RecordPid = recB20.pid,
                x40IsRecordLink = false,
                x40IsHtmlBody = true,
                x40Body = sb.ToString(),
                x40Subject = "MARKTIME: " + recB20.b20Name,
                x40Recipient = string.Join(";", emails.Distinct()),              
                j40ID = smtp_account.pid
            };


            _mother.MailBL.ClearAttachments();  //pro jistotu vyčistit přílohy z předchozích mail zpráv
            _mother.MailBL.Set_SubjectInBody("Hlídač pracuje!");            
            var ret = _mother.MailBL.SendMessage(recX40, false,null);
                        
            return ret.issuccess;
        }

    }
}
