

namespace BL
{
    public interface Ip12ApproveUserDayBL
    {
        public BO.p12ApproveUserDay Load(int pid);
        public IEnumerable<BO.p12ApproveUserDay> GetList(BO.myQueryP12 mq);
        public int Save(BO.p12ApproveUserDay rec);

    }
    class p12ApproveUserDayBL : BaseBL, Ip12ApproveUserDayBL
    {
        public p12ApproveUserDayBL(BL.Factory mother) : base(mother)
        {

        }


        private string GetSQL1(string strAppend = null)
        {
            sb("SELECT a.*,j02.j02LastName+' '+j02.j02FirstName as Person,");
            sb(_db.GetSQL1_Ocas("p12",false,false,true));
            sb(" FROM p12ApproveUserDay a INNER JOIN j02User j02 ON a.j02ID=j02.j02ID");
            if (_mother.CurrentUser.IsHostingModeTotalCloud)
            {
                sb(" INNER JOIN j04UserRole j04x ON j02.j04ID = j04x.j04ID INNER JOIN x67EntityRole x67x ON j04x.x67ID=x67x.x67ID");
                sb(this.AppendCloudQuery(strAppend, "x67x.x01ID"));
            }
            else
            {
                sb(strAppend);
            }
            return sbret();
        }
        public BO.p12ApproveUserDay Load(int pid)
        {
            return _db.Load<BO.p12ApproveUserDay>(GetSQL1(" WHERE a.p12ID=@pid"), new { pid = pid });
        }

        public IEnumerable<BO.p12ApproveUserDay> GetList(BO.myQueryP12 mq)
        {
            
            DL.FinalSqlCommand fq = DL.basQuery.GetFinalSql(GetSQL1(), mq, _mother.CurrentUser);
            return _db.GetList<BO.p12ApproveUserDay>(fq.FinalSql, fq.Parameters);
        }



        public int Save(BO.p12ApproveUserDay rec)
        {
            if (!ValidateBeforeSave(rec))
            {
                return 0;
            }
            var p = new DL.Params4Dapper();
            p.AddInt("pid", rec.pid);
            p.AddInt("j02ID", rec.j02ID,true);
            p.AddInt("j02ID_ApprovedBy", rec.j02ID_ApprovedBy, true);
            p.AddString("p12Memo", rec.p12Memo);
            p.AddDateTime("p12Date", rec.p12Date);
            p.AddEnumInt("p12StatusFlag", rec.p12StatusFlag);

            return _db.SaveRecord("p12ApproveUserDay", p, rec,false,true);
        }
        private bool ValidateBeforeSave(BO.p12ApproveUserDay rec)
        {
            if (rec.j02ID == 0)
            {
                this.AddMessage("Chybí vyplnit [Uživatel]."); return false;
            }
            
            return true;
        }

    }
}
