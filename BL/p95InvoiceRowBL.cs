using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BL
{
    public interface Ip95InvoiceRowBL
    {
        public BO.p95InvoiceRow Load(int pid);
        public IEnumerable<BO.p95InvoiceRow> GetList(BO.myQuery mq);
        public int Save(BO.p95InvoiceRow rec);

    }
    class p95InvoiceRowBL : BaseBL, Ip95InvoiceRowBL
    {
        public p95InvoiceRowBL(BL.Factory mother) : base(mother)
        {

        }


        private string GetSQL1(string strAppend = null)
        {
            sb("SELECT a.*,");
            sb(_db.GetSQL1_Ocas("p95"));
            sb(" FROM p95InvoiceRow a");
            sb(strAppend);
            return sbret();
        }
        public BO.p95InvoiceRow Load(int pid)
        {
            return _db.Load<BO.p95InvoiceRow>(GetSQL1(" WHERE a.p95ID=@pid"), new { pid = pid });
        }

        public IEnumerable<BO.p95InvoiceRow> GetList(BO.myQuery mq)
        {
            DL.FinalSqlCommand fq = DL.basQuery.GetFinalSql(GetSQL1(), mq, _mother.CurrentUser);
            return _db.GetList<BO.p95InvoiceRow>(fq.FinalSql, fq.Parameters);
        }



        public int Save(BO.p95InvoiceRow rec)
        {
            if (!ValidateBeforeSave(rec))
            {
                return 0;
            }
            var p = new DL.Params4Dapper();
            p.AddInt("pid", rec.pid);
            p.AddInt("x01ID", rec.x01ID == 0 ? _mother.CurrentUser.x01ID : rec.x01ID, true);
            p.AddString("p95Name", rec.p95Name);
            p.AddString("p95Code", rec.p95Code);
            p.AddInt("p95Ordinary", rec.p95Ordinary);
            p.AddString("p95Name_BillingLang1", rec.p95Name_BillingLang1);
            p.AddString("p95Name_BillingLang2", rec.p95Name_BillingLang2);
            p.AddString("p95Name_BillingLang3", rec.p95Name_BillingLang3);
            p.AddString("p95Name_BillingLang4", rec.p95Name_BillingLang4);
            
            

            return _db.SaveRecord("p95InvoiceRow", p, rec);
        }
        private bool ValidateBeforeSave(BO.p95InvoiceRow rec)
        {
            if (string.IsNullOrEmpty(rec.p95Name))
            {
                this.AddMessage("Chybí vyplnit [Název]."); return false;
            }

            return true;
        }

    }
}
