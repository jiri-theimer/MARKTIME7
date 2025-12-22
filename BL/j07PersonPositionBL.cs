using System;
using System.Collections.Generic;
using System.Text;
namespace BL
{
    public interface Ij07PersonPositionBL
    {
        public BO.j07PersonPosition Load(int pid);
        public IEnumerable<BO.j07PersonPosition> GetList(BO.myQuery mq);
        public int Save(BO.j07PersonPosition rec);

    }
    class j07PersonPositionBL : BaseBL, Ij07PersonPositionBL
    {
        public j07PersonPositionBL(BL.Factory mother) : base(mother)
        {

        }


        private string GetSQL1(string strAppend = null)
        {
            sb("SELECT a.*,");
            sb(_db.GetSQL1_Ocas("j07"));
            sb(" FROM j07PersonPosition a");
            sb(strAppend);
            return sbret();
        }
        public BO.j07PersonPosition Load(int pid)
        {
            return _db.Load<BO.j07PersonPosition>(GetSQL1(" WHERE a.j07ID=@pid"), new { pid = pid });
        }

        public IEnumerable<BO.j07PersonPosition> GetList(BO.myQuery mq)
        {
            DL.FinalSqlCommand fq = DL.basQuery.GetFinalSql(GetSQL1(), mq, _mother.CurrentUser);
            return _db.GetList<BO.j07PersonPosition>(fq.FinalSql, fq.Parameters);
        }



        public int Save(BO.j07PersonPosition rec)
        {
            if (!ValidateBeforeSave(rec))
            {
                return 0;
            }
            var p = new DL.Params4Dapper();
            p.AddInt("pid", rec.pid);
            p.AddInt("x01ID", rec.x01ID == 0 ? _mother.CurrentUser.x01ID : rec.x01ID);
            p.AddString("j07Name", rec.j07Name);
            p.AddInt("j07Ordinary", rec.j07Ordinary);
            p.AddString("j07Name_BillingLang1", rec.j07Name_BillingLang1);
            p.AddString("j07Name_BillingLang2", rec.j07Name_BillingLang2);
            p.AddString("j07Name_BillingLang3", rec.j07Name_BillingLang3);
            p.AddString("j07Name_BillingLang4", rec.j07Name_BillingLang4);
           
            return _db.SaveRecord("j07PersonPosition", p, rec);
        }
        private bool ValidateBeforeSave(BO.j07PersonPosition rec)
        {
            if (string.IsNullOrEmpty(rec.j07Name))
            {
                this.AddMessage("Chybí vyplnit [Název]."); return false;
            }

            return true;
        }

    }
}
