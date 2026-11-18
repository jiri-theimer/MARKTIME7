

namespace BL
{
    public interface Ix07IntegrationBL
    {
        public BO.x07Integration Load(int pid);
        public IEnumerable<BO.x07Integration> GetList(BO.myQuery mq);
        public int Save(BO.x07Integration rec);
        public BO.x07Config LoadConfig(BO.x07Integration rec);
        public IEnumerable<BO.x08IntegrationBinding> GetList_x08(int x07id, string entity, List<int> pids);
        public void SaveBinding(int x07id, string entity, int pid, string externalpid);
        public void AppendLog(int x07id, string entity, int countall, int countok, string message);
        public IEnumerable<BO.x09IntegrationLog> GetList_x09(int x07id, int toprecs);

    }
    class x07IntegrationBL : BaseBL, Ix07IntegrationBL
    {
        public x07IntegrationBL(BL.Factory mother) : base(mother)
        {

        }


        private string GetSQL1(string strAppend = null)
        {
            sb("SELECT a.*,");
            sb(_db.GetSQL1_Ocas("x07"));
            sb(" FROM x07Integration a");
            sb(strAppend);
            return sbret();
        }
        public BO.x07Integration Load(int pid)
        {
            return _db.Load<BO.x07Integration>(GetSQL1(" WHERE a.x07ID=@pid"), new { pid = pid });
        }

        public IEnumerable<BO.x07Integration> GetList(BO.myQuery mq)
        {
            DL.FinalSqlCommand fq = DL.basQuery.GetFinalSql(GetSQL1(), mq, _mother.CurrentUser);
            return _db.GetList<BO.x07Integration>(fq.FinalSql, fq.Parameters);
        }



        public int Save(BO.x07Integration rec)
        {
            if (!ValidateBeforeSave(rec))
            {
                return 0;
            }
            var p = new DL.Params4Dapper();
            p.AddInt("pid", rec.pid);
            p.AddInt("x01ID", rec.x01ID == 0 ? _mother.CurrentUser.x01ID : rec.x01ID, true);
            p.AddString("x07Name", rec.x07Name);
            p.AddString("x07Token", rec.x07Token);
            p.AddString("x07Login", rec.x07Login);
            p.AddString("x07Password", rec.x07Password);
            p.AddInt("x07Ordinary", rec.x07Ordinary);
            p.AddEnumInt("x07Flag", rec.x07Flag);
            p.AddString("x07Url", string.IsNullOrWhiteSpace(rec.x07Url) ? null : rec.x07Url.Trim().TrimEnd('/'));
            p.AddString("x07Config", rec.x07Config);

            return _db.SaveRecord("x07Integration", p, rec);
        }
        private bool ValidateBeforeSave(BO.x07Integration rec)
        {
            
            if (string.IsNullOrEmpty(rec.x07Name))
            {
                this.AddMessage("Chybí vyplnit [Název]."); return false;
            }
            if (rec.x07Flag == BO.x07FlagEnum.AbraFlexi && !string.IsNullOrWhiteSpace(rec.x07Url) && !rec.x07Url.Trim().StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            {
                this.AddMessage("[Adresa serveru] musí začínat https://."); return false;   //přihlašovací údaje se neposílají nešifrovaně
            }
            
            return true;
        }


        public BO.x07Config LoadConfig(BO.x07Integration rec)
        {
            if (rec == null || string.IsNullOrWhiteSpace(rec.x07Config)) return new BO.x07Config();
            try
            {
                return System.Text.Json.JsonSerializer.Deserialize<BO.x07Config>(rec.x07Config) ?? new BO.x07Config();
            }
            catch
            {
                return new BO.x07Config();
            }
        }

        public IEnumerable<BO.x08IntegrationBinding> GetList_x08(int x07id, string entity, List<int> pids)
        {
            if (pids == null || pids.Count == 0) return new List<BO.x08IntegrationBinding>();
            return _db.GetList<BO.x08IntegrationBinding>($"SELECT * FROM x08IntegrationBinding WHERE x07ID=@x07id AND x08RecordEntity=@entity AND x08RecordPid IN ({string.Join(",", pids)})", new { x07id = x07id, entity = entity });
        }

        public void SaveBinding(int x07id, string entity, int pid, string externalpid)
        {
            _db.RunSql("DELETE FROM x08IntegrationBinding WHERE x07ID=@x07id AND x08RecordEntity=@entity AND x08RecordPid=@pid", new { x07id = x07id, entity = entity, pid = pid });
            _db.RunSql("INSERT INTO x08IntegrationBinding(x07ID,x08RecordEntity,x08RecordPid,x08ExternalPid,x08DateSync) VALUES(@x07id,@entity,@pid,@ext,GETDATE())", new { x07id = x07id, entity = entity, pid = pid, ext = externalpid });
        }

        //zápis do protokolu + stav posledního běhu na kartě integrace
        public void AppendLog(int x07id, string entity, int countall, int countok, string message)
        {
            if (message != null && message.Length > 4000) message = message.Substring(0, 4000);
            _db.RunSql("INSERT INTO x09IntegrationLog(x07ID,x09Date,x09User,x09RecordEntity,x09CountAll,x09CountOk,x09Message) VALUES(@x07id,GETDATE(),@usr,@entity,@countall,@countok,@message)",
                new { x07id = x07id, usr = _mother.CurrentUser.j02Login, entity = entity, countall = countall, countok = countok, message = message });
            string strErr = countok < countall ? (message ?? "Chyba") : null;
            if (strErr != null && strErr.Length > 1000) strErr = strErr.Substring(0, 1000);
            _db.RunSql("UPDATE x07Integration SET x07LastRun=GETDATE(),x07LastError=@err WHERE x07ID=@x07id", new { x07id = x07id, err = strErr });
        }

        public IEnumerable<BO.x09IntegrationLog> GetList_x09(int x07id, int toprecs)
        {
            return _db.GetList<BO.x09IntegrationLog>($"SELECT TOP {toprecs} * FROM x09IntegrationLog WHERE x07ID=@x07id ORDER BY x09ID DESC", new { x07id = x07id });
        }

    }
}
