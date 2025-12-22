
using System;

namespace BL
{
    public interface Ip24ContactGroupBL
    {
        public BO.p24ContactGroup Load(int pid);

        public IEnumerable<BO.p24ContactGroup> GetList(BO.myQueryP24 mq);
        public int Save(BO.p24ContactGroup rec, List<int> p28ids);
        public bool Append(int p24id, List<int> p28ids);


    }
    class p24ContactGroupBL : BaseBL, Ip24ContactGroupBL
    {
        public p24ContactGroupBL(BL.Factory mother) : base(mother)
        {

        }


        private string GetSQL1(string strAppend = null)
        {
            sb("SELECT a.*,");
            sb(_db.GetSQL1_Ocas("p24"));
            sb(" FROM p24ContactGroup a");
            sb(strAppend);
            return sbret();
        }
        public BO.p24ContactGroup Load(int pid)
        {
            return _db.Load<BO.p24ContactGroup>(GetSQL1(" WHERE a.p24ID=@pid"), new { pid = pid });
        }



        public IEnumerable<BO.p24ContactGroup> GetList(BO.myQueryP24 mq)
        {
            DL.FinalSqlCommand fq = DL.basQuery.GetFinalSql(GetSQL1(), mq, _mother.CurrentUser);
            return _db.GetList<BO.p24ContactGroup>(fq.FinalSql, fq.Parameters);
        }

        public bool Append(int p24id, List<int> p28ids)
        {
            if (p28ids.Count > 0)
            {
                return _db.RunSql($"INSERT INTO p25ContactGroupBinding(p24ID,p28ID) SELECT @pid,p28ID FROM p28Contact WHERE p28ID IN ({string.Join(",", p28ids)})", new { pid = p24id });
            }

            return false;
        }
        
        public int Save(BO.p24ContactGroup rec, List<int> p28ids)
        {
            if (!ValidateBeforeSave(rec))
            {
                return 0;
            }
            var p = new DL.Params4Dapper();
            p.AddInt("pid", rec.pid);
            p.AddInt("x01ID", rec.x01ID == 0 ? _mother.CurrentUser.x01ID : rec.x01ID, true);
            p.AddString("p24Name", rec.p24Name);
            p.AddString("p24Email", rec.p24Email);
            

            int intPID = _db.SaveRecord("p24ContactGroup", p, rec);
            if (rec.pid > 0)
            {
                _db.RunSql("DELETE FROM p25ContactGroupBinding WHERE p24ID=@pid", new { pid = intPID });
            }
            if (p28ids.Count > 0)
            {
                _db.RunSql($"INSERT INTO p25ContactGroupBinding(p24ID,p28ID) SELECT @pid,p28ID FROM p28Contact WHERE p28ID IN ({string.Join(",", p28ids)})", new { pid = intPID });
            }
            

            return intPID;
        }
        public bool ValidateBeforeSave(BO.p24ContactGroup rec)
        {
            if (string.IsNullOrEmpty(rec.p24Name))
            {
                this.AddMessage("Chybí vyplnit [Název skupiny]."); return false;
            }

            return true;
        }





    }
}
