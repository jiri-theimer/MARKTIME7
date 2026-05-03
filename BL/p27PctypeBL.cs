

namespace BL
{
    public interface Ip27PctypeBL
    {
        public BO.p27Pctype Load(int pid);
        public IEnumerable<BO.p27Pctype> GetList(BO.myQuery mq);
        public int Save(BO.p27Pctype rec);

    }
    class p27PctypeBL : BaseBL, Ip27PctypeBL
    {
        public p27PctypeBL(BL.Factory mother) : base(mother)
        {

        }


        private string GetSQL1(string strAppend = null)
        {
            sb("SELECT a.*,");
            sb(_db.GetSQL1_Ocas("p27"));
            sb(" FROM p27Pctype a");
            sb(strAppend);
            return sbret();
        }
        public BO.p27Pctype Load(int pid)
        {
            return _db.Load<BO.p27Pctype>(GetSQL1(" WHERE a.p27ID=@pid"), new { pid = pid });
        }

        public IEnumerable<BO.p27Pctype> GetList(BO.myQuery mq)
        {
            DL.FinalSqlCommand fq = DL.basQuery.GetFinalSql(GetSQL1(), mq, _mother.CurrentUser);
            return _db.GetList<BO.p27Pctype>(fq.FinalSql, fq.Parameters);
        }



        public int Save(BO.p27Pctype rec)
        {
            if (!ValidateBeforeSave(rec))
            {
                return 0;
            }
            var p = new DL.Params4Dapper();
            p.AddInt("pid", rec.pid);
            p.AddInt("x01ID", rec.x01ID == 0 ? _mother.CurrentUser.x01ID : rec.x01ID, true);
            p.AddString("p27Name", rec.p27Name);
            

            return _db.SaveRecord("p27Pctype", p, rec);
        }
        private bool ValidateBeforeSave(BO.p27Pctype rec)
        {
            if (string.IsNullOrEmpty(rec.p27Name))
            {
                this.AddMessage("Chybí vyplnit [Název]."); return false;
            }
          
            return true;
        }

    }
}
