

namespace BL
{
    public interface Ip78UpominkaSdruzenaBL
    {
        public BO.p78UpominkaSdruzena Load(int pid);
        public IEnumerable<BO.p78UpominkaSdruzena> GetList(BO.myQueryP78 mq);
        public int Save(BO.p78UpominkaSdruzena rec, List<int> p84ids);

    }
    class p78UpominkaSdruzenaBL : BaseBL, Ip78UpominkaSdruzenaBL
    {
        public p78UpominkaSdruzenaBL(BL.Factory mother) : base(mother)
        {

        }


        private string GetSQL1(string strAppend = null)
        {
            sb("SELECT a.*,p28x.p28Name,j02owner.j02Name as Owner,");
            sb(_db.GetSQL1_Ocas("p78"));
            sb(" FROM p78UpominkaSdruzena a INNER JOIN p28Contact p28x ON a.p28ID=p28x.p28ID LEFT OUTER JOIN j02User j02owner ON a.j02ID_Owner=j02owner.j02ID");
            sb(strAppend);
            return sbret();
        }
        public BO.p78UpominkaSdruzena Load(int pid)
        {
            return _db.Load<BO.p78UpominkaSdruzena>(GetSQL1(" WHERE a.p78ID=@pid"), new { pid = pid });
        }

        public IEnumerable<BO.p78UpominkaSdruzena> GetList(BO.myQueryP78 mq)
        {
            DL.FinalSqlCommand fq = DL.basQuery.GetFinalSql(GetSQL1(), mq, _mother.CurrentUser);
            return _db.GetList<BO.p78UpominkaSdruzena>(fq.FinalSql, fq.Parameters);
        }



        public int Save(BO.p78UpominkaSdruzena rec,List<int> p84ids)
        {
            if (!ValidateBeforeSave(rec))
            {
                return 0;
            }
            var p = new DL.Params4Dapper();
            p.AddInt("pid", rec.pid);
            p.AddInt("p28ID", rec.p28ID, true);
            if (rec.j02ID_Owner == 0) rec.j02ID_Owner = _mother.CurrentUser.pid;
            p.AddInt("j02ID_Owner", rec.j02ID_Owner, true);
            p.AddString("p78Name", rec.p78Name);
            p.AddString("p78Code", rec.p78Code);
            p.AddString("p78TextA", rec.p78TextA);
            p.AddString("p78TextB", rec.p78TextB);
            p.AddDateTime("p78Date", rec.p78Date);

            p.AddString("p78Client", rec.p78Client);
            p.AddString("p78Client_RegID", rec.p78Client_RegID);
            p.AddString("p78Client_VatID", rec.p78Client_VatID);
            p.AddString("p78ClientAddress1_Street", rec.p78ClientAddress1_Street);
            p.AddString("p78ClientAddress1_City", rec.p78ClientAddress1_City);
            p.AddString("p78ClientAddress1_ZIP", rec.p78ClientAddress1_ZIP);
            p.AddString("p78ClientAddress1_Country", rec.p78ClientAddress1_Country);
            p.AddString("p78ClientAddress1_Before", rec.p78ClientAddress1_Before);
            
            p.AddString("p78Client_ICDPH_SK", rec.p78Client_ICDPH_SK);


            var intPID= _db.SaveRecord("p78UpominkaSdruzena", p, rec);
            if (intPID > 0 && p84ids !=null)                
            {
                if (rec.pid > 0)
                {
                    _db.RunSql("DELETE FROM p79UpominkaSdruzenaBinding WHERE p78ID=@pid", new { pid = intPID });
                }
                if (p84ids.Count > 0)
                {
                    _db.RunSql("INSERT INTO p79UpominkaSdruzenaBinding(p78ID,p84ID) SELECT @pid,p84ID FROM p84Upominka WHERE p84ID IN (" + string.Join(",", p84ids) + ")", new { pid = intPID });
                }
            }

            return intPID;
        }
        private bool ValidateBeforeSave(BO.p78UpominkaSdruzena rec)
        {
            if (string.IsNullOrEmpty(rec.p78Name))
            {
                this.AddMessage("Chybí vyplnit [Název]."); return false;
            }
            if (string.IsNullOrEmpty(rec.p78Code))
            {
                this.AddMessage("Chybí vyplnit [Kód]."); return false;
            }
            return true;
        }

        
    }
}
