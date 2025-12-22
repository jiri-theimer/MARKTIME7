
namespace BL
{
    public interface Ip63OverheadBL
    {
        public BO.p63Overhead Load(int pid);
        public IEnumerable<BO.p63Overhead> GetList(BO.myQuery mq);
        public int Save(BO.p63Overhead rec);

    }
    class p63OverheadBL : BaseBL, Ip63OverheadBL
    {
        public p63OverheadBL(BL.Factory mother) : base(mother)
        {

        }


        private string GetSQL1(string strAppend = null)
        {
            sb("SELECT a.*,p32.p32Name,");
            sb(_db.GetSQL1_Ocas("p63"));
            sb(" FROM p63Overhead a LEFT OUTER JOIN p32Activity p32 ON a.p32ID=p32.p32ID");
            sb(strAppend);
            return sbret();
        }
        public BO.p63Overhead Load(int pid)
        {
            return _db.Load<BO.p63Overhead>(GetSQL1(" WHERE a.p63ID=@pid"), new { pid = pid });
        }

        public IEnumerable<BO.p63Overhead> GetList(BO.myQuery mq)
        {
            DL.FinalSqlCommand fq = DL.basQuery.GetFinalSql(GetSQL1(), mq, _mother.CurrentUser);
            return _db.GetList<BO.p63Overhead>(fq.FinalSql, fq.Parameters);
        }



        public int Save(BO.p63Overhead rec)
        {
            if (!ValidateBeforeSave(rec))
            {
                return 0;
            }
            var p = new DL.Params4Dapper();
            p.AddInt("pid", rec.pid);
            p.AddInt("x01ID", rec.x01ID == 0 ? _mother.CurrentUser.x01ID : rec.x01ID);
            p.AddString("p63Name", rec.p63Name);
            p.AddInt("p32ID", rec.p32ID, true);
            p.AddDouble("p63PercentRate", rec.p63PercentRate);
            p.AddBool("p63IsIncludeExpense", rec.p63IsIncludeExpense);
            p.AddBool("p63IsIncludeFees", rec.p63IsIncludeFees);
            p.AddBool("p63IsIncludeTime", rec.p63IsIncludeTime);

            return _db.SaveRecord("p63Overhead", p, rec);

        }
        private bool ValidateBeforeSave(BO.p63Overhead rec)
        {
            if (string.IsNullOrEmpty(rec.p63Name))
            {
                this.AddMessage("Chybí vyplnit [Název]."); return false;
            }
            if (rec.p32ID==0)
            {
                this.AddMessage("Chybí vyplnit [Aktivita]."); return false;
            }
            if (rec.p63PercentRate == 0)
            {
                this.AddMessage("Nulové procento přirážky."); return false;
            }

            return true;
        }

    }
}
