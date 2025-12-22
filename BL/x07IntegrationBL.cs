

namespace BL
{
    public interface Ix07IntegrationBL
    {
        public BO.x07Integration Load(int pid);
        public IEnumerable<BO.x07Integration> GetList(BO.myQuery mq);
        public int Save(BO.x07Integration rec);

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

            return _db.SaveRecord("x07Integration", p, rec);
        }
        private bool ValidateBeforeSave(BO.x07Integration rec)
        {
            
            if (string.IsNullOrEmpty(rec.x07Name))
            {
                this.AddMessage("Chybí vyplnit [Název]."); return false;
            }
            
            return true;
        }

    }
}
