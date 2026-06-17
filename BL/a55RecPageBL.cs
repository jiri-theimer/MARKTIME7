using System;
using System.Collections.Generic;
using System.Text;


namespace BL
{
    public interface Ia55RecPageBL
    {
        public BO.a55RecPage Load(int pid);
        public IEnumerable<BO.a55RecPage> GetList(BO.myQuery mq);
        public int Save(BO.a55RecPage rec);


    }

    class a55RecPageBL : BaseBL, Ia55RecPageBL
    {
        public a55RecPageBL(BL.Factory mother) : base(mother)
        {

        }


        private string GetSQL1(string strAppend = null)
        {
            sb("SELECT a.*,");
            sb(_db.GetSQL1_Ocas("a55"));
            sb(" FROM a55RecPage a");
            sb(strAppend);
            return sbret();
        }
        public BO.a55RecPage Load(int pid)
        {
            return _db.Load<BO.a55RecPage>(GetSQL1(" WHERE a.a55ID=@pid"), new { pid = pid });
        }


        public IEnumerable<BO.a55RecPage> GetList(BO.myQuery mq)
        {
            if (mq.explicit_orderby == null) { mq.explicit_orderby = "a.a55Name"; };
            DL.FinalSqlCommand fq = DL.basQuery.GetFinalSql(GetSQL1(), mq, _mother.CurrentUser);
            return _db.GetList<BO.a55RecPage>(fq.FinalSql, fq.Parameters);
        }



        public int Save(BO.a55RecPage rec)
        {
            if (ValidateBeforeSave(rec) == false)
            {
                return 0;
            }
            
            var p = new DL.Params4Dapper();
            p.AddInt("pid", rec.pid);
            p.AddInt("x01ID", rec.x01ID == 0 ? _mother.CurrentUser.x01ID : rec.x01ID);
            p.AddString("a55Name", rec.a55Name);            
            p.AddString("a55Entity", rec.a55Entity);           
            p.AddString("a55CssFile", rec.a55CssFile);
           
            int intPID = _db.SaveRecord("a55RecPage", p, rec);



            return intPID;
        }

        public bool ValidateBeforeSave(BO.a55RecPage rec)
        {
            if (string.IsNullOrEmpty(rec.a55Name))
            {
                this.AddMessage("Chybí vyplnit [Název]."); return false;
            }
            if (string.IsNullOrEmpty(rec.a55Entity))
            {
                this.AddMessage("Chybí vyplnit [Entita]."); return false;
            }

            return true;
        }

    }
}

