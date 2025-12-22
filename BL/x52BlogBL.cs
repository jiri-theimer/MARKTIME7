
namespace BL
{
    public interface Ix52BlogBL
    {
        public BO.x52Blog Load(int pid);
       
        public string LoadHtmlContent(int pid);
        public IEnumerable<BO.x52Blog> GetList(BO.myQuery mq);
        public int Save(BO.x52Blog rec);


    }
    class x52BlogBL : BaseBL, Ix52BlogBL
    {
        public x52BlogBL(BL.Factory mother) : base(mother)
        {

        }


        private string GetSQL1(string strAppend = null)
        {
            sb("SELECT a.x52ID,a.x52Name,a.x52Date,a.x52Ordinary,a.x52Html,x52Symbol,a.x52Editorial,");

            sb(_db.GetSQL1_Ocas("x52"));
            if (_mother.App.HostingMode == BL.Singleton.HostingModeEnum.None)
            {
                sb(" FROM x52Blog a");
            }
            else
            {
                sb(" FROM a7marktime.dbo.x52Blog a");
            }
            
            sb(strAppend);
            return sbret();
        }
        public BO.x52Blog Load(int pid)
        {
            return _db.Load<BO.x52Blog>(GetSQL1(" WHERE a.x52ID=@pid"), new { pid = pid });
        }
      
        public string LoadHtmlContent(int pid)
        {
            return _db.Load<BO.GetString>("SELECT a.x52Html as Value FROM x52Blog a WHERE a.x52ID=@pid", new { pid = pid }).Value;
        }
        public IEnumerable<BO.x52Blog> GetList(BO.myQuery mq)
        {
            if (mq.explicit_orderby == null) mq.explicit_orderby = "a.x52Ordinary,a.x52Date DESC";
            DL.FinalSqlCommand fq = DL.basQuery.GetFinalSql(GetSQL1(), mq, _mother.CurrentUser);
            return _db.GetList<BO.x52Blog>(fq.FinalSql, fq.Parameters);
        }



        public int Save(BO.x52Blog rec)
        {
            if (ValidateBeforeSave(rec) == false)
            {
                return 0;
            }
            var p = new DL.Params4Dapper();
            p.AddInt("pid", rec.pid);
            p.AddString("x52Name", rec.x52Name);
            p.AddDateTime("x52Date", rec.x52Date);           
            p.AddString("x52Html", rec.x52Html);
            p.AddString("x52Editorial", rec.x52Editorial);
            p.AddInt("x52Ordinary", rec.x52Ordinary);
            p.AddString("x52Symbol", rec.x52Symbol);

            int intPID = _db.SaveRecord("x52Blog", p, rec);
            if (intPID > 0)
            {
                
            }


            return intPID;
        }

        public bool ValidateBeforeSave(BO.x52Blog rec)
        {
            if (string.IsNullOrEmpty(rec.x52Name))
            {
                this.AddMessage("Chybí vyplnit [Název]."); return false;
            }


            return true;
        }

    }
}

