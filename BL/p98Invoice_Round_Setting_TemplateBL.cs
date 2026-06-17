using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BL
{
    public interface Ip98Invoice_Round_Setting_TemplateBL
    {
        public BO.p98Invoice_Round_Setting_Template Load(int pid);
        public IEnumerable<BO.p98Invoice_Round_Setting_Template> GetList(BO.myQuery mq);
        public int Save(BO.p98Invoice_Round_Setting_Template rec, List<BO.p97Invoice_Round_Setting> lisP97);
        public IEnumerable<BO.p97Invoice_Round_Setting> GetList_p97(int p98id);

    }
    class p98Invoice_Round_Setting_TemplateBL : BaseBL, Ip98Invoice_Round_Setting_TemplateBL
    {
        public p98Invoice_Round_Setting_TemplateBL(BL.Factory mother) : base(mother)
        {

        }


        private string GetSQL1(string strAppend = null)
        {
            sb("SELECT a.*,");
            sb(_db.GetSQL1_Ocas("p98"));
            sb(" FROM p98Invoice_Round_Setting_Template a");
            sb(strAppend);
            return sbret();
        }
        public BO.p98Invoice_Round_Setting_Template Load(int pid)
        {
            return _db.Load<BO.p98Invoice_Round_Setting_Template>(GetSQL1(" WHERE a.p98ID=@pid"), new { pid = pid });
        }

        public IEnumerable<BO.p98Invoice_Round_Setting_Template> GetList(BO.myQuery mq)
        {
            DL.FinalSqlCommand fq = DL.basQuery.GetFinalSql(GetSQL1(), mq, _mother.CurrentUser);
            return _db.GetList<BO.p98Invoice_Round_Setting_Template>(fq.FinalSql, fq.Parameters);
        }

        public IEnumerable<BO.p97Invoice_Round_Setting> GetList_p97(int p98id)
        {
            sb("select a.*");
            sb(" FROM p97Invoice_Round_Setting a");
            sb(" WHERE a.p98ID=@p98id");
            return _db.GetList<BO.p97Invoice_Round_Setting>(sbret(), new { p98id = p98id });
        }

        public int Save(BO.p98Invoice_Round_Setting_Template rec, List<BO.p97Invoice_Round_Setting> lisP97)
        {
            if (!ValidateBeforeSave(rec, lisP97))
            {
                return 0;
            }
            using (var sc = new System.Transactions.TransactionScope())
            {
                var p = new DL.Params4Dapper();                
                p.AddInt("pid", rec.pid);
                p.AddInt("x01ID", rec.x01ID == 0 ? _mother.CurrentUser.x01ID : rec.x01ID);
                p.AddString("p98Name", rec.p98Name);
                p.AddBool("p98IsDefault", rec.p98IsDefault);
                p.AddBool("p98IsIncludeInVat", rec.p98IsIncludeInVat);

                int intPID = _db.SaveRecord("p98Invoice_Round_Setting_Template", p, rec);
                if (intPID > 0)
                {
                    if (rec.pid > 0)
                    {
                        _db.RunSql("DELETE FROM p97Invoice_Round_Setting WHERE p98ID=@pid", new { pid = intPID });
                    }
                    foreach (var c in lisP97)
                    {
                        p = new DL.Params4Dapper();
                        p.AddInt("p98ID", intPID, true);
                        p.AddInt("j27ID", c.j27ID, true);
                        p.AddEnumInt("p97AmountFlag", c.p97AmountFlag);
                        p.AddInt("p97Scale", c.p97Scale);
                       
                        _db.SaveRecord("p97Invoice_Round_Setting", p, c, true, true);
                    }
                    if (rec.p98IsDefault)
                    {
                        _db.RunSql("UPDATE p98Invoice_Round_Setting_Template set p98IsDefault=0 WHERE p98ID<>@pid", new { pid = intPID });
                    }
                    sc.Complete();                    
                }

                return intPID;
            }
                

        }
        private bool ValidateBeforeSave(BO.p98Invoice_Round_Setting_Template rec, List<BO.p97Invoice_Round_Setting> lisP97)
        {
            if (string.IsNullOrEmpty(rec.p98Name))
            {
                this.AddMessage("Chybí vyplnit [Název]."); return false;
            }
            if (lisP97.Count() == 0)
            {
                this.AddMessage("V pravidle musí být minimálně jeden řádek (měna).");return false;
            }
            if (lisP97.Count()>1 && lisP97.GroupBy(p => p.j27ID).Count() == 1)
            {
                this.AddMessage("Pro jednu měnu může existovat pouze jeden řádek.");return false;
            }

            return true;
        }

    }
}
