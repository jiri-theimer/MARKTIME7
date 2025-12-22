using BO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BL
{
    public interface Ij06UserHistoryBL
    {
        public BO.j06UserHistory Load(int pid);
        public IEnumerable<BO.j06UserHistory> GetList(BO.myQuery mq);
        public int Save(BO.j06UserHistory rec, DateTime d1, DateTime d2);
        

    }
    class j06UserHistoryBL : BaseBL, Ij06UserHistoryBL
    {
        public j06UserHistoryBL(BL.Factory mother) : base(mother)
        {

        }


        private string GetSQL1(string strAppend = null)
        {
            sb("SELECT a.*,c21.c21Name,j07.j07Name,");
            sb(_db.GetSQL1_Ocas("j06"));
            sb($" FROM j06UserHistory a INNER JOIN j02User j02x ON a.j02ID=j02x.j02ID INNER JOIN j04UserRole j04x ON j02x.j04ID=j04x.j04ID INNER JOIN x67EntityRole x67x ON j04x.x67ID=x67x.x67ID");
            sb(" LEFT OUTER JOIN c21FondCalendar c21 ON a.c21ID=c21.c21ID LEFT OUTER JOIN j07PersonPosition j07 ON a.j07ID=j07.j07ID");
            sb(strAppend);
            return sbret();
        }
        public BO.j06UserHistory Load(int pid)
        {
            return _db.Load<BO.j06UserHistory>(GetSQL1(" WHERE a.j06ID=@pid"), new { pid = pid });
        }

        public IEnumerable<BO.j06UserHistory> GetList(BO.myQuery mq)
        {
            DL.FinalSqlCommand fq = DL.basQuery.GetFinalSql(GetSQL1(), mq, _mother.CurrentUser);
            return _db.GetList<BO.j06UserHistory>(fq.FinalSql, fq.Parameters);
        }



        public int Save(BO.j06UserHistory rec, DateTime d1, DateTime d2)
        {
            if (!ValidateBeforeSave(rec, d1, d2))
            {
                return 0;
            }
            using (var sc = new System.Transactions.TransactionScope())     //ukládání podléhá transakci{
            {
                var p = new DL.Params4Dapper();
                p.AddInt("pid", rec.pid);
                p.AddInt("j02ID", rec.j02ID, true);
                p.AddInt("j07ID", rec.j07ID, true);
                p.AddInt("c21ID", rec.c21ID, true);
               
                p.AddDateTime("j06ValidFrom", d1);
                p.AddDateTime("j06ValidUntil", d2);

                int intPID = _db.SaveRecord("j06UserHistory", p, rec, false);
                if (intPID > 0)
                {
                    sc.Complete();
                }
                return intPID;
            }


        }
        private bool ValidateBeforeSave(BO.j06UserHistory rec, DateTime d1, DateTime d2)
        {
            if (rec.j02ID == 0)
            {
                this.AddMessage("Chybí [Uživatel].");return false;
            }

            if (rec.c21ID==0 && rec.j07ID==0)
            {
                this.AddMessageTranslated("Missing [c21ID] or [j07ID]."); return false;
            }

            if (d2 > DateTime.Now)
            {
                this.AddMessage("Datum [Platnost do] musí být menší než aktuální datum."); return false;
            }

            var lis = GetList(new BO.myQuery("j06") { IsRecordValid = null });
            if (lis.Any(p => p.pid != rec.pid && p.j02ID==rec.j02ID && p.c21ID == rec.c21ID && p.j07ID == rec.j07ID && ((d1 >= p.ValidFrom && d1 <= p.ValidUntil) || (d2 >= p.ValidFrom && d2 <= p.ValidUntil))))
            {
                this.AddMessage("Pro zadané období již existuje záznam historie."); return false;
            }

            return true;
        }

       

    }
}
