using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BL
{
    public interface Ip86BankAccountBL
    {
        public BO.p86BankAccount Load(int pid);
        public BO.p86BankAccount LoadInvoiceAccount(int p91id);
        public BO.p86BankAccount LoadProformaAccount(int p90id);
        public IEnumerable<BO.p86BankAccount> GetList(BO.myQuery mq);
        public int Save(BO.p86BankAccount rec);

    }
    class p86BankAccountBL : BaseBL, Ip86BankAccountBL
    {
        public p86BankAccountBL(BL.Factory mother) : base(mother)
        {

        }


        private string GetSQL1(string strAppend = null)
        {
            sb("SELECT a.*,");
            sb(_db.GetSQL1_Ocas("p86"));
            sb(" FROM p86BankAccount a");
            sb(strAppend);
            return sbret();
        }
        public BO.p86BankAccount Load(int pid)
        {
            return _db.Load<BO.p86BankAccount>(GetSQL1(" WHERE a.p86ID=@pid"), new { pid = pid });
        }

        public BO.p86BankAccount LoadInvoiceAccount(int p91id)   //vrátí bankovní účet pro fakturu p91id
        {
            return _db.Load<BO.p86BankAccount>(GetSQL1(" WHERE a.p86ID=dbo.p91_get_p86id(@p91id)"), new { p91id = p91id });
        }
        public BO.p86BankAccount LoadProformaAccount(int p90id)   //vrátí bankovní účet pro zálohu p90id
        {
            return _db.Load<BO.p86BankAccount>(GetSQL1(" WHERE a.p86ID=dbo.p90_get_p86id(@p90id)"), new { p90id = p90id });
        }

        public IEnumerable<BO.p86BankAccount> GetList(BO.myQuery mq)
        {
            DL.FinalSqlCommand fq = DL.basQuery.GetFinalSql(GetSQL1(), mq, _mother.CurrentUser);
            return _db.GetList<BO.p86BankAccount>(fq.FinalSql, fq.Parameters);
        }



        public int Save(BO.p86BankAccount rec)
        {
            if (!ValidateBeforeSave(rec))
            {
                return 0;
            }
            var p = new DL.Params4Dapper();
            p.AddInt("pid", rec.pid);
            p.AddInt("x01ID", rec.x01ID == 0 ? _mother.CurrentUser.x01ID : rec.x01ID);
            p.AddString("p86Name", rec.p86Name);
            p.AddString("p86BankName", rec.p86BankName);
            p.AddString("p86Account", rec.p86Account);
            p.AddString("p86Code", rec.p86Code);
            p.AddString("p86SWIFT", rec.p86SWIFT);
            p.AddString("p86IBAN", rec.p86IBAN);
            p.AddString("p86BankAddress", rec.p86BankAddress);

            return _db.SaveRecord("p86BankAccount", p, rec);


        }
        private bool ValidateBeforeSave(BO.p86BankAccount rec)
        {
            if (string.IsNullOrEmpty(rec.p86Name))
            {
                this.AddMessage("Chybí vyplnit [Název]."); return false;
            }
            if (string.IsNullOrEmpty(rec.p86Account))
            {
                this.AddMessage("Chybí vyplnit [Číslo účtu]."); return false;
            }
            var recX01 = _mother.x01LicenseBL.Load(_mother.CurrentUser.x01ID);
            if (!recX01.x01IsAllowDuplicity_p86)
            {
                var lis = GetList(new BO.myQuery("p86") { IsRecordValid = null });
                if (lis.Where(p => p.p86Account.Replace(" ", "") == rec.p86Account.Replace(" ", "") && p.p86Code == rec.p86Code && p.pid != rec.pid).Any())
                {
                    this.AddMessage(string.Format("Číslo účtu {0}/{1} je již vyplněné v jiném bankovním účtu.", rec.p86Account, rec.p86Code));
                    return false;
                }

                if (!string.IsNullOrEmpty(rec.p86IBAN) && lis.Where(p => p.p86IBAN != null && p.p86IBAN.Replace(" ", "") == rec.p86IBAN.Replace(" ", "") && p.pid != rec.pid).Any())
                {
                    this.AddMessage(string.Format("IBAN [{0}] je již vyplněný v jiném bankovním účtu.", rec.p86IBAN));
                    return false;
                }
            }
            


            return true;
        }

    }
}
