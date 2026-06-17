using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BL
{
    public interface Ip80InvoiceAmountStructureBL
    {
        public BO.p80InvoiceAmountStructure Load(int pid);
        public IEnumerable<BO.p80InvoiceAmountStructure> GetList(BO.myQuery mq);
        public int Save(BO.p80InvoiceAmountStructure rec);

    }
    class p80InvoiceAmountStructureBL : BaseBL, Ip80InvoiceAmountStructureBL
    {
        public p80InvoiceAmountStructureBL(BL.Factory mother) : base(mother)
        {

        }


        private string GetSQL1(string strAppend = null)
        {
            sb("SELECT a.*,");
            sb(_db.GetSQL1_Ocas("p80"));
            sb(" FROM p80InvoiceAmountStructure a");
            sb(strAppend);
            return sbret();
        }
        public BO.p80InvoiceAmountStructure Load(int pid)
        {
            return _db.Load<BO.p80InvoiceAmountStructure>(GetSQL1(" WHERE a.p80ID=@pid"), new { pid = pid });
        }

        public IEnumerable<BO.p80InvoiceAmountStructure> GetList(BO.myQuery mq)
        {
            DL.FinalSqlCommand fq = DL.basQuery.GetFinalSql(GetSQL1(), mq, _mother.CurrentUser);
            return _db.GetList<BO.p80InvoiceAmountStructure>(fq.FinalSql, fq.Parameters);
        }



        public int Save(BO.p80InvoiceAmountStructure rec)
        {
            if (!ValidateBeforeSave(rec))
            {
                return 0;
            }
            var p = new DL.Params4Dapper();
            p.AddInt("pid", rec.pid);
            p.AddInt("x01ID", rec.x01ID == 0 ? _mother.CurrentUser.x01ID : rec.x01ID);
            p.AddString("p80Name", rec.p80Name);
            p.AddBool("p80IsExpenseSeparate", rec.p80IsExpenseSeparate);
            p.AddBool("p80IsFeeSeparate", rec.p80IsFeeSeparate);
            p.AddBool("p80IsTimeSeparate", rec.p80IsTimeSeparate);
            p.AddString("p80GroupByPrefix", rec.p80GroupByPrefix);
            

            return _db.SaveRecord("p80InvoiceAmountStructure", p, rec);

        }
        private bool ValidateBeforeSave(BO.p80InvoiceAmountStructure rec)
        {
            if (string.IsNullOrEmpty(rec.p80Name))
            {
                this.AddMessage("Chybí vyplnit [Název]."); return false;
            }


            return true;
        }

    }
}
