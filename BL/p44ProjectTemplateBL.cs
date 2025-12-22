

namespace BL
{
    public interface Ip44ProjectTemplateBL
    {
        public BO.p44ProjectTemplate Load(int pid);
        public IEnumerable<BO.p44ProjectTemplate> GetList(BO.myQuery mq);
        public IEnumerable<BO.p44ProjectTemplate> GetList_ProjectCreate();
        public int Save(BO.p44ProjectTemplate rec);

    }
    class p44ProjectTemplateBL : BaseBL, Ip44ProjectTemplateBL
    {
        public p44ProjectTemplateBL(BL.Factory mother) : base(mother)
        {

        }


        private string GetSQL1(string strAppend = null)
        {
            sb("SELECT a.*,");
            sb(_db.GetSQL1_Ocas("p44"));
            sb(" FROM p44ProjectTemplate a LEFT OUTER JOIN p41Project p41x ON a.p41ID_Pattern=p41x.p41ID");
            sb(strAppend);
            return sbret();
        }
        public BO.p44ProjectTemplate Load(int pid)
        {
            return _db.Load<BO.p44ProjectTemplate>(GetSQL1(" WHERE a.p44ID=@pid"), new { pid = pid });
        }

        public IEnumerable<BO.p44ProjectTemplate> GetList(BO.myQuery mq)
        {
            DL.FinalSqlCommand fq = DL.basQuery.GetFinalSql(GetSQL1(), mq, _mother.CurrentUser);
            return _db.GetList<BO.p44ProjectTemplate>(fq.FinalSql, fq.Parameters);
        }

        public IEnumerable<BO.p44ProjectTemplate> GetList_ProjectCreate()
        {
            if (_mother.CurrentUser.TestPermission(BO.PermValEnum.GR_p41_Creator))
            {
                return GetList(new BO.myQuery("p44"));  //může zakládat projekty bez omezení
            }

            string s = " WHERE p41x.p42ID = -1";
            var lisP42 = _mother.p42ProjectTypeBL.GetList_ProjectCreate();
            if (lisP42.Count() > 0)
            {
                s = $" WHERE p41x.p42ID IN ({string.Join(",", lisP42.Select(p => p.pid))})";               
            }
          
            DL.FinalSqlCommand fq = DL.basQuery.GetFinalSql(GetSQL1(s), new BO.myQuery("p44"), _mother.CurrentUser);

            return _db.GetList<BO.p44ProjectTemplate>(fq.FinalSql, fq.Parameters);
        }

        public int Save(BO.p44ProjectTemplate rec)
        {
            if (!ValidateBeforeSave(rec))
            {
                return 0;
            }
            var p = new DL.Params4Dapper();
            p.AddInt("pid", rec.pid);
            p.AddInt("x01ID", rec.x01ID == 0 ? _mother.CurrentUser.x01ID : rec.x01ID, true);
            p.AddEnumInt("p44FixingFlag", rec.p44FixingFlag);
            p.AddString("p44Name", rec.p44Name);
            p.AddInt("p41ID_Pattern", rec.p41ID_Pattern,true);
            p.AddInt("p44Ordinary", rec.p44Ordinary);
            p.AddBool("p44IsRoles", rec.p44IsRoles);
            p.AddBool("p44IsP56", rec.p44IsP56);
            p.AddBool("p44IsRoles", rec.p44IsRoles);
            p.AddBool("p44IsP40", rec.p44IsP40);
            p.AddBool("p44IsO22", rec.p44IsO22);
            p.AddBool("p44IsClient", rec.p44IsClient);
            p.AddBool("p44IsBilling", rec.p44IsBilling);
            p.AddBool("p44IsB20", rec.p44IsB20);
            p.AddBool("p44IsO24", rec.p44IsO24);
            p.AddBool("p44IsJ18ID", rec.p44IsJ18ID);
            p.AddBool("p44IsTags", rec.p44IsTags);

            return _db.SaveRecord("p44ProjectTemplate", p, rec);
        }
        private bool ValidateBeforeSave(BO.p44ProjectTemplate rec)
        {
            if (string.IsNullOrEmpty(rec.p44Name))
            {
                this.AddMessage("Chybí vyplnit [Název]."); return false;
            }
            if (rec.p41ID_Pattern==0)
            {
                this.AddMessage("Chybí vyplnit vzorový projekt."); return false;
            }
            return true;
        }

    }

}
