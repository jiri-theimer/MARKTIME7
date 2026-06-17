
namespace BL
{
    public interface Ip68StopWatchBL
    {
        public BO.p68StopWatch Load(int pid);
        public IEnumerable<BO.p68StopWatch> GetList(int j02id);
        public int Save(BO.p68StopWatch rec);
        public void Clear(int j02id);
        

    }
    class p68StopWatchBL : BaseBL, Ip68StopWatchBL
    {
        public p68StopWatchBL(BL.Factory mother) : base(mother)
        {

        }


        private string GetSQL1(string strAppend = null)
        {
            sb("SELECT a.*,");
            sb("isnull(p41x.p41NameShort,p41x.p41Name) as p41Name,p41x.p28ID_Client,p28Client.p28Name,p32x.p32Name,");
            sb(_db.GetSQL1_Ocas("p68"));
            sb(" FROM p68StopWatch a LEFT OUTER JOIN p41Project p41x ON a.p41ID=p41x.p41ID LEFT OUTER JOIN p28Contact p28Client ON p41x.p28ID_Client=p28Client.p28ID");
            if (_mother.CurrentUser.IsHostingModeTotalCloud)
            {
                sb(" INNER JOIN j02User j02x ON a.j02ID=j02x.j02ID INNER JOIN j04UserRole j04x ON j02x.j04ID=j04x.j04ID INNER JOIN x67EntityRole x67x ON j04x.x67ID=x67x.x67ID");
            }
            sb(" LEFT OUTER JOIN p32Activity p32x ON a.p32ID=p32x.p32ID");
            sb(strAppend);
            return sbret();
        }
        public BO.p68StopWatch Load(int pid)
        {
            return _db.Load<BO.p68StopWatch>(GetSQL1(" WHERE a.p68ID=@pid"), new { pid = pid });
            
        }

        public IEnumerable<BO.p68StopWatch> GetList(int j02id)
        {
            return _db.GetList<BO.p68StopWatch>(GetSQL1(" WHERE a.j02ID=@j02id ORDER BY a.p68Ordinary"), new { j02id = j02id });

            //DL.FinalSqlCommand fq = DL.basQuery.GetFinalSql(GetSQL1($" WHERE a.j02ID={j02id}"), new BO.myQuery("p68") {explicit_orderby="a.p68Ordinary"}, _mother.CurrentUser);
            //return _db.GetList<BO.p68StopWatch>(fq.FinalSql, fq.Parameters);
        }

        public void Clear(int j02id)
        {
            _db.RunSql("DELETE FROM p68StopWatch WHERE j02ID=@j02id", new { j02id = j02id });
            _mother.j02UserBL.RecoveryUserCache_StopWatch(j02id);
        }
        

        public int Save(BO.p68StopWatch rec)
        {
            if (!ValidateBeforeSave(rec))
            {
                return 0;
            }
            var p = new DL.Params4Dapper();
            p.AddInt("pid", rec.pid);
            p.AddInt("j02ID", rec.j02ID, true);
            p.AddInt("p41ID", rec.p41ID, true);
            p.AddInt("p56ID", rec.p56ID, true);
            p.AddInt("p32ID", rec.p32ID, true);
            p.AddInt("p68Ordinary", rec.p68Ordinary);
            p.AddString("p68Text", rec.p68Text);
            p.AddDateTime("p68LastStart", rec.p68LastStart);
            p.AddDateTime("p68LastEnd", rec.p68LastEnd);
            p.AddBool("p68IsRunning", rec.p68IsRunning);
            p.AddInt("p68Duration", rec.p68Duration);

            int intPID = _db.SaveRecord("p68StopWatch", p, rec);
            if (intPID>0 && rec.pid==0)
            {
                _mother.j02UserBL.RecoveryUserCache_StopWatch(rec.j02ID);
            }

            return intPID;
        }
        private bool ValidateBeforeSave(BO.p68StopWatch rec)
        {
            if (rec.j02ID==0)
            {
                this.AddMessageTranslated("j02id missing."); return false;
            }
           
            return true;
        }

    }
}
