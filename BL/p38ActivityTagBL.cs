

namespace BL
{
    public interface Ip38ActivityTagBL
    {
        public BO.p38ActivityTag Load(int pid);
        public IEnumerable<BO.p38ActivityTag> GetList(BO.myQuery mq);
        public int Save(BO.p38ActivityTag rec);

    }
    class p38ActivityTagBL : BaseBL, Ip38ActivityTagBL
    {
        public p38ActivityTagBL(BL.Factory mother) : base(mother)
        {

        }


        private string GetSQL1(string strAppend = null)
        {
            sb("SELECT a.*,");
            sb(_db.GetSQL1_Ocas("p38"));
            sb(" FROM p38ActivityTag a");
            sb(strAppend);
            return sbret();
        }
        public BO.p38ActivityTag Load(int pid)
        {
            return _db.Load<BO.p38ActivityTag>(GetSQL1(" WHERE a.p38ID=@pid"), new { pid = pid });
        }

        public IEnumerable<BO.p38ActivityTag> GetList(BO.myQuery mq)
        {
            DL.FinalSqlCommand fq = DL.basQuery.GetFinalSql(GetSQL1(), mq, _mother.CurrentUser);
            return _db.GetList<BO.p38ActivityTag>(fq.FinalSql, fq.Parameters);
        }



        public int Save(BO.p38ActivityTag rec)
        {
            if (!ValidateBeforeSave(rec))
            {
                return 0;
            }
            var p = new DL.Params4Dapper();
            p.AddInt("pid", rec.pid);
            p.AddInt("x01ID", rec.x01ID == 0 ? _mother.CurrentUser.x01ID : rec.x01ID, true);
            p.AddString("p38Name", rec.p38Name);
            
            p.AddInt("p38Ordinary", rec.p38Ordinary);
           
            return _db.SaveRecord("p38ActivityTag", p, rec);
        }
        private bool ValidateBeforeSave(BO.p38ActivityTag rec)
        {
            if (string.IsNullOrEmpty(rec.p38Name))
            {
                this.AddMessage("Chybí vyplnit [Název]."); return false;
            }

            return true;
        }

    }
}
