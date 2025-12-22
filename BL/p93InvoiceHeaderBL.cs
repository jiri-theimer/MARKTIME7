using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BL
{
    public interface Ip93InvoiceHeaderBL
    {
        public BO.p93InvoiceHeader Load(int pid);

        public IEnumerable<BO.p93InvoiceHeader> GetList(BO.myQuery mq);
        public int Save(BO.p93InvoiceHeader rec, List<BO.p88InvoiceHeader_BankAccount> lisP88);
        public IEnumerable<BO.p88InvoiceHeader_BankAccount> GetList_p88(int p93id);

    }
    class p93InvoiceHeaderBL : BaseBL, Ip93InvoiceHeaderBL
    {
        public p93InvoiceHeaderBL(BL.Factory mother) : base(mother)
        {

        }


        private string GetSQL1(string strAppend = null)
        {
            sb("SELECT a.*,");
            sb(_db.GetSQL1_Ocas("p93"));
            sb(" FROM p93InvoiceHeader a");
            sb(strAppend);
            return sbret();
        }
        public BO.p93InvoiceHeader Load(int pid)
        {
            return _db.Load<BO.p93InvoiceHeader>(GetSQL1(" WHERE a.p93ID=@pid"), new { pid = pid });
        }

        public IEnumerable<BO.p93InvoiceHeader> GetList(BO.myQuery mq)
        {
            DL.FinalSqlCommand fq = DL.basQuery.GetFinalSql(GetSQL1(), mq, _mother.CurrentUser);
            return _db.GetList<BO.p93InvoiceHeader>(fq.FinalSql, fq.Parameters);
        }
        public IEnumerable<BO.p88InvoiceHeader_BankAccount> GetList_p88(int p93id)
        {
            sb("select a.*");            
            sb(" FROM p88InvoiceHeader_BankAccount a");          
            sb(" WHERE a.p93ID=@p93id");
            return _db.GetList<BO.p88InvoiceHeader_BankAccount>(sbret(), new { p93id = p93id });
        }


        public int Save(BO.p93InvoiceHeader rec,List<BO.p88InvoiceHeader_BankAccount> lisP88)
        {
            if (!ValidateBeforeSave(rec, lisP88))
            {
                return 0;
            }
            using (var sc = new System.Transactions.TransactionScope())
            {
                var p = new DL.Params4Dapper();
                p.AddInt("pid", rec.pid);
                p.AddInt("x01ID", rec.x01ID == 0 ? _mother.CurrentUser.x01ID : rec.x01ID);
                p.AddString("p93Name", rec.p93Name);
                p.AddString("p93Company", rec.p93Company);
                p.AddString("p93City", rec.p93City);
                p.AddString("p93Street", rec.p93Street);
                p.AddString("p93Zip", rec.p93Zip);
                p.AddString("p93RegID", rec.p93RegID);
                p.AddString("p93VatID", rec.p93VatID);
                p.AddString("p93Contact", rec.p93Contact);
                p.AddString("p93Registration", rec.p93Registration);
                p.AddString("p93Referent", rec.p93Referent);
                p.AddString("p93Signature", rec.p93Signature);
                p.AddString("p93FreeText01", rec.p93FreeText01);
                p.AddString("p93FreeText02", rec.p93FreeText02);
                p.AddString("p93FreeText03", rec.p93FreeText03);
                p.AddString("p93FreeText04", rec.p93FreeText04);
                p.AddString("p93LogoFile", rec.p93LogoFile);
                p.AddString("p93Country", rec.p93Country);
                p.AddString("p93CountryCode", rec.p93CountryCode);
                p.AddString("p93Email", rec.p93Email);
                p.AddString("p93SignatureFile", rec.p93SignatureFile);
                p.AddString("p93ICDPH_SK", rec.p93ICDPH_SK);
                p.AddInt("j27ID_Domestic", rec.j27ID_Domestic, true);
                p.AddString("p93PfxCertificate", rec.p93PfxCertificate);
                if (rec.p93PfxCertificate == null)
                {
                    p.AddString("p93PfxPassword",null);
                }
                else
                {
                    if (!string.IsNullOrEmpty(rec.p93PfxPassword))
                    {
                        p.AddString("p93PfxPassword", rec.p93PfxPassword);
                    }
                }
                
                
                

                int intPID = _db.SaveRecord("p93InvoiceHeader", p, rec);
                if (intPID > 0 && lisP88 !=null)
                {
                    if (rec.pid > 0)
                    {
                        _db.RunSql("DELETE FROM p88InvoiceHeader_BankAccount WHERE p93ID=@pid", new { pid = intPID });
                    }
                    foreach(var c in lisP88)
                    {
                        if (!_db.RunSql("INSERT INTO p88InvoiceHeader_BankAccount(p93ID,p86ID,j27ID) VALUES(@pid,@p86id,@j27id)", new { pid = intPID, p86id = c.p86ID, j27id = c.j27ID }))
                        {
                            return 0;
                        }
                    }
                }
                sc.Complete();
                return intPID;
            }
                


        }
        private bool ValidateBeforeSave(BO.p93InvoiceHeader rec, List<BO.p88InvoiceHeader_BankAccount> lisP88)
        {
            if (string.IsNullOrEmpty(rec.p93Name))
            {
                this.AddMessage("Chybí vyplnit [Název]."); return false;
            }
            if (string.IsNullOrEmpty(rec.p93CountryCode))
            {
                this.AddMessage("Chybí vyplnit [ISO kód státu]."); return false;
            }

            if (lisP88 != null)
            {
                if (lisP88.Where(p=>p.j27ID==0 || p.p86ID == 0).Any())
                {
                    this.AddMessage("V nastavení bankovních účtů je nevyplněná měna nebo účet.");return false;
                }
                foreach(var c in lisP88.GroupBy(p => p.j27ID))
                {
                    if (c.Count() > 1)
                    {
                        this.AddMessage(string.Format("Pro měnu {0} není možné definovat více bankovních účtů.",_mother.FBL.LoadCurrencyByID(c.First().j27ID).j27Code)); return false;
                    }
                }
                
            }


            return true;
        }

    }

}
