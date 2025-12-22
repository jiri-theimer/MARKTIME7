using BO;

namespace BL
{
    public interface Ip54OvertimeLevelBL
    {
        public BO.p54OvertimeLevel Load(int pid);
        public IEnumerable<BO.p54OvertimeLevel> GetList(BO.myQuery mq);
        public int Save(BO.p54OvertimeLevel rec);
        

    }
    class p54OvertimeLevelBL : BaseBL, Ip54OvertimeLevelBL
    {
        public p54OvertimeLevelBL(BL.Factory mother) : base(mother)
        {

        }


        private string GetSQL1(string strAppend = null)
        {
            sb("SELECT a.*,");
            sb(_db.GetSQL1_Ocas("p54"));
            sb($" FROM p54OvertimeLevel a");
            sb(strAppend);
            
            return sbret();
        }
        public BO.p54OvertimeLevel Load(int pid)
        {
            return _db.Load<BO.p54OvertimeLevel>(GetSQL1(" WHERE a.p54ID=@pid"), new { pid = pid });
        }

        public IEnumerable<BO.p54OvertimeLevel> GetList(BO.myQuery mq)
        {
            DL.FinalSqlCommand fq = DL.basQuery.GetFinalSql(GetSQL1(), mq, _mother.CurrentUser);
            return _db.GetList<BO.p54OvertimeLevel>(fq.FinalSql, fq.Parameters);
        }



        public int Save(BO.p54OvertimeLevel rec)
        {
            if (!ValidateBeforeSave(rec))
            {
                return 0;
            }
            using (var sc = new System.Transactions.TransactionScope())     //ukládání podléhá transakci{
            {
                var p = new DL.Params4Dapper();
                p.AddInt("pid", rec.pid);
               
                p.AddInt("x01ID", rec.x01ID == 0 ? _mother.CurrentUser.x01ID : rec.x01ID);
                p.AddDouble("p54BillingRate", rec.p54BillingRate);
                p.AddDouble("p54InternalRate", rec.p54InternalRate);
                p.AddDouble("p54Ordinary", rec.p54Ordinary);
                p.AddString("p54Name", rec.p54Name);
               
                int intPID = _db.SaveRecord("p54OvertimeLevel", p, rec);
                if (intPID > 0)
                {
                    sc.Complete();
                }
                return intPID;
            }


        }
        private bool ValidateBeforeSave(BO.p54OvertimeLevel rec)
        {

            if (string.IsNullOrEmpty(rec.p54Name))
            {
                this.AddMessage("Chybí vyplnit [Název]."); return false;
            }

            return true;
        }

        

    }
}
