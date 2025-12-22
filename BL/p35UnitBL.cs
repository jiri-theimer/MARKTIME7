

namespace BL
{
    public interface Ip35UnitBL
    {
        public BO.p35Unit Load(int pid);
        public IEnumerable<BO.p35Unit> GetList(BO.myQuery mq);
        public int Save(BO.p35Unit rec);

    }
    class p35UnitBL : BaseBL, Ip35UnitBL
    {
        public p35UnitBL(BL.Factory mother) : base(mother)
        {

        }


        private string GetSQL1(string strAppend = null)
        {
            sb("SELECT a.*,");
            sb(_db.GetSQL1_Ocas("p35"));
            sb(" FROM p35Unit a");
            sb(strAppend);
            return sbret();
        }
        public BO.p35Unit Load(int pid)
        {
            return _db.Load<BO.p35Unit>(GetSQL1(" WHERE a.p35ID=@pid"), new { pid = pid });
        }

        public IEnumerable<BO.p35Unit> GetList(BO.myQuery mq)
        {
            DL.FinalSqlCommand fq = DL.basQuery.GetFinalSql(GetSQL1(), mq, _mother.CurrentUser);
            return _db.GetList<BO.p35Unit>(fq.FinalSql, fq.Parameters);
        }



        public int Save(BO.p35Unit rec)
        {
            if (!ValidateBeforeSave(rec))
            {
                return 0;
            }
            var p = new DL.Params4Dapper();
            p.AddInt("pid", rec.pid);
            p.AddInt("x01ID", rec.x01ID == 0 ? _mother.CurrentUser.x01ID : rec.x01ID, true);
            p.AddString("p35Name", rec.p35Name);
            p.AddString("p35Code", rec.p35Code);
         

            return _db.SaveRecord("p35Unit", p, rec);
        }
        private bool ValidateBeforeSave(BO.p35Unit rec)
        {
            if (string.IsNullOrEmpty(rec.p35Name))
            {
                this.AddMessage("Chybí vyplnit [Název]."); return false;
            }
            if (string.IsNullOrEmpty(rec.p35Code))
            {
                this.AddMessage("Chybí vyplnit [Kód]."); return false;
            }
            return true;
        }

    }
}
