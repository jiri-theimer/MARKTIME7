

namespace BL
{
    public interface Ip83UpominkaTypeBL
    {
        public BO.p83UpominkaType Load(int pid);
        public IEnumerable<BO.p83UpominkaType> GetList(BO.myQuery mq);
        public int Save(BO.p83UpominkaType rec);
        public BO.p83UpominkaType LoadTypSdruzeneUpominky(int p78id);

    }
    class p83UpominkaTypeBL : BaseBL, Ip83UpominkaTypeBL
    {
        public p83UpominkaTypeBL(BL.Factory mother) : base(mother)
        {

        }


        private string GetSQL1(string strAppend = null)
        {
            sb("SELECT a.*,");
            sb(_db.GetSQL1_Ocas("p83"));
            sb(" FROM p83UpominkaType a");
            sb(strAppend);
            return sbret();
        }
        public BO.p83UpominkaType Load(int pid)
        {
            return _db.Load<BO.p83UpominkaType>(GetSQL1(" WHERE a.p83ID=@pid"), new { pid = pid });
        }

        public BO.p83UpominkaType LoadTypSdruzeneUpominky(int p78id)
        {
            return _db.Load<BO.p83UpominkaType>(GetSQL1(" INNER JOIN p84Upominka p84 ON a.p83ID=p84.p83ID INNER JOIN p79UpominkaSdruzenaBinding p79 ON p84.p84ID=p79.p84ID WHERE p79.p78ID=@p78id"), new { p78id = p78id });
        }


        public IEnumerable<BO.p83UpominkaType> GetList(BO.myQuery mq)
        {
            if (mq.explicit_orderby == null) mq.explicit_orderby = "a.p83Ordinary";
            DL.FinalSqlCommand fq = DL.basQuery.GetFinalSql(GetSQL1(), mq, _mother.CurrentUser);
            return _db.GetList<BO.p83UpominkaType>(fq.FinalSql, fq.Parameters);
        }



        public int Save(BO.p83UpominkaType rec)
        {
            if (!ValidateBeforeSave(rec))
            {
                return 0;
            }
            var p = new DL.Params4Dapper();
            p.AddInt("pid", rec.pid);
            p.AddInt("x01ID", rec.x01ID == 0 ? _mother.CurrentUser.x01ID : rec.x01ID);
            p.AddString("p83Name", rec.p83Name);
            p.AddInt("p83Ordinary", rec.p83Ordinary);
            p.AddInt("x31ID_Index1", rec.x31ID_Index1, true);
            p.AddInt("x31ID_Index2", rec.x31ID_Index2, true);
            p.AddInt("x31ID_Index3", rec.x31ID_Index3, true);
            p.AddString("p83Name_Index1", rec.p83Name_Index1);
            p.AddString("p83Name_Index2", rec.p83Name_Index2);
            p.AddString("p83Name_Index3", rec.p83Name_Index3);
            p.AddString("p83TextA_Index1", rec.p83TextA_Index1);
            p.AddString("p83TextA_Index2", rec.p83TextA_Index2);
            p.AddString("p83TextA_Index3", rec.p83TextA_Index3);
            p.AddString("p83TextB_Index1", rec.p83TextB_Index1);
            p.AddString("p83TextB_Index2", rec.p83TextB_Index2);
            p.AddString("p83TextB_Index3", rec.p83TextB_Index3);

            p.AddInt("p83Days_Index1", rec.p83Days_Index1);
            p.AddInt("p83Days_Index2", rec.p83Days_Index2);
            p.AddInt("p83Days_Index3", rec.p83Days_Index3);
            p.AddInt("j61ID_Index1", rec.j61ID_Index1, true);
            p.AddInt("j61ID_Index2", rec.j61ID_Index2, true);
            p.AddInt("j61ID_Index3", rec.j61ID_Index3, true);

            p.AddBool("p83IsAutoSdruzena", rec.p83IsAutoSdruzena);
            p.AddInt("x31ID_Sdruzena", rec.x31ID_Sdruzena, true);
            p.AddInt("j61ID_Sdruzena", rec.j61ID_Sdruzena, true);

            return _db.SaveRecord("p83UpominkaType", p, rec);
            

        }
        private bool ValidateBeforeSave(BO.p83UpominkaType rec)
        {
            if (string.IsNullOrEmpty(rec.p83Name))
            {
                this.AddMessage("Chybí vyplnit [Název]."); return false;
            }
           
            if (rec.p83Days_Index1 <=0)
            {
                this.AddMessage("Počet dnů po splatnosti faktury."); return false;
            }

            return true;
        }

    }
}
