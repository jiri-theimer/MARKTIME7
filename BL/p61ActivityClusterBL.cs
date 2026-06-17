

namespace BL
{
    public interface Ip61ActivityClusterBL
    {
        public BO.p61ActivityCluster Load(int pid);
        public IEnumerable<BO.p61ActivityCluster> GetList(BO.myQuery mq);
        public int Save(BO.p61ActivityCluster rec, List<int> p32ids);

    }
    class p61ActivityClusterBL : BaseBL, Ip61ActivityClusterBL
    {
        public p61ActivityClusterBL(BL.Factory mother) : base(mother)
        {

        }


        private string GetSQL1(string strAppend = null)
        {
            sb("SELECT a.*,");
            sb(_db.GetSQL1_Ocas("p61"));
            sb(" FROM p61ActivityCluster a");
            sb(strAppend);
            return sbret();
        }
        public BO.p61ActivityCluster Load(int pid)
        {
            return _db.Load<BO.p61ActivityCluster>(GetSQL1(" WHERE a.p61ID=@pid"), new { pid = pid });
        }

        public IEnumerable<BO.p61ActivityCluster> GetList(BO.myQuery mq)
        {            
            DL.FinalSqlCommand fq = DL.basQuery.GetFinalSql(GetSQL1(), mq, _mother.CurrentUser);
            return _db.GetList<BO.p61ActivityCluster>(fq.FinalSql, fq.Parameters);
        }



        public int Save(BO.p61ActivityCluster rec, List<int> p32ids)
        {
            if (!ValidateBeforeSave(rec))
            {
                return 0;
            }
            using (var sc = new System.Transactions.TransactionScope()) //ukládání podléhá transakci
            {
                var p = new DL.Params4Dapper();
                p.AddInt("pid", rec.pid);
                p.AddInt("x01ID", rec.x01ID == 0 ? _mother.CurrentUser.x01ID : rec.x01ID);
                p.AddString("p61Name", rec.p61Name);

                int intPID = _db.SaveRecord("p61ActivityCluster", p, rec);
                if (p32ids != null)
                {
                    if (rec.pid > 0)
                    {
                        _db.RunSql("DELETE FROM p62ActivityCluster_Item WHERE p61ID=@pid", new { pid = intPID });
                    }
                    if (p32ids.Count > 0)
                    {
                        _db.RunSql("INSERT INTO p62ActivityCluster_Item(p61ID,p32ID) SELECT @pid,p32ID FROM p32Activity WHERE p32ID IN (" + string.Join(",", p32ids) + ")", new { pid = intPID });
                    }
                }
                sc.Complete();
                return intPID;
            }
                
            
        }
        public bool ValidateBeforeSave(BO.p61ActivityCluster rec)
        {
            if (string.IsNullOrEmpty(rec.p61Name))
            {
                this.AddMessage("Chybí vyplnit [Název]."); return false;
            }

            return true;
        }

    }
}
