using BO;
using BO.Rejstrik;
using DocumentFormat.OpenXml.Drawing;
using DocumentFormat.OpenXml.Vml;
using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

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
        public bool Handle_NastalaUdalostHlidace(BO.b20Hlidac recB20,BO.b21HlidacBinding recB21,ref System.Text.StringBuilder sb,ref List<string> notify2emails, HttpClient hc = null);
        public bool SendNotification(System.Text.StringBuilder sb, List<string> emails, BO.b20Hlidac recB20, BO.j40MailAccount smtp_account);

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

            p.AddString("b20NotifyReceivers", rec.b20NotifyReceivers);
            p.AddString("b20NotifyMessage", rec.b20NotifyMessage);

            p.AddBool("b20IsNotifyRecordOwner", rec.b20IsNotifyRecordOwner);
            p.AddInt("j11ID_Notify", rec.j11ID_Notify, true);
            p.AddInt("x67ID_Notify2", rec.x67ID_Notify2, true);
            p.AddInt("x67ID_Notify1", rec.x67ID_Notify1, true);
            int intPID = _db.SaveRecord("b20Hlidac", p, rec);
            if (intPID > 0)
            {


            }
            return intPID;
        }

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

        public bool Handle_NastalaUdalostHlidace(BO.b20Hlidac recB20,BO.b21HlidacBinding recB21,ref System.Text.StringBuilder sb, ref List<string> notify2emails, HttpClient hc = null)
        {            
            var lisLog = GetList_j94(recB21.pid, 1);
            if (recB20.b20TestTimeFrom !=null && recB20.b20TestTimeUntil != null)   //hlídač se má spouštět pouze v čase od-do
            {
                int secs1 = BO.Code.Time.ConvertTimeToSeconds(recB20.b20TestTimeFrom);
                int secs2 = BO.Code.Time.ConvertTimeToSeconds(recB20.b20TestTimeUntil);
                if (DateTime.Now < DateTime.Today.AddSeconds(secs1) || DateTime.Now>DateTime.Today.AddSeconds(secs2))
                {
                    return false;
                }
                
            }
            if (lisLog.Count() > 0 && recB20.b20NextRunAfterMinutes>0)
            {
                double ellapsed_minutes = (DateTime.Now - lisLog.First().j94Date).TotalMinutes;
                if (ellapsed_minutes < Convert.ToDouble(recB20.b20NextRunAfterMinutes))
                {
                    //ještě nenastal čas k otestování
                    return false;
                }
            }
            bool bolAchieved = false; string strRobotValue = null;
            if (recB20.b20TypeFlag == BO.b20TypeFlagEnum.TestBySql)
            {
                bolAchieved = Handle_TestovatHlidace_Sql(ref sb,recB20, recB21, lisLog, ref strRobotValue);    //hlídač typu testování obsahu sql dotazu

                
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
            if (bolAchieved)
            {
                int intJ02ID_Owner = (recB20.b20IsNotifyRecordOwner ? _mother.CBL.GetRecord_j02ID_Owner(recB20.b20Entity, recB21.b21RecordPid) : 0);

                var emails = _mother.MailBL.GetMailList(intJ02ID_Owner, recB20.j11ID_Notify, recB20.x67ID_Notify1,0,0, recB21.b21RecordPid, recB20.b20Entity,0);
                if (recB21.b21NotifyReceivers != null)
                {
                    recB21.b21NotifyReceivers = recB21.b21NotifyReceivers.Replace(";", ",");    //ručně zadaní příjemci v notifikovaném záznamu
                    emails.InsertRange(0,BO.Code.Bas.ConvertString2List(recB21.b21NotifyReceivers));
                    
                }
                if (emails !=null && emails.Count() > 0)
                {
                    notify2emails.InsertRange(0, emails);
                }
                if (recB20.b20NotifyReceivers != null)
                {
                    notify2emails.Insert(0, recB20.b20NotifyReceivers);
                }
            }
            return bolAchieved;
        }

        private bool Handle_TestovatHlidace_ZmenaAdresy(ref System.Text.StringBuilder sb, HttpClient hc,BO.b20Hlidac recB20, BO.b21HlidacBinding recB21, IEnumerable<BO.j94HlidacLog> lisLog, ref string strRobotValue)
        {
            var cr = new BL.Code.RejstrikySupport();
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
