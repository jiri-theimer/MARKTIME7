using System;
using System.Collections.Generic;
using System.Linq;

namespace BL
{
    public interface Ip82Proforma_PaymentBL
    {
        public BO.p82Proforma_Payment Load(int pid);
        public IEnumerable<BO.p82Proforma_Payment> GetList(int p90id);
        public int Save(BO.p82Proforma_Payment rec);

    }
    class p82Proforma_PaymentBL : BaseBL, Ip82Proforma_PaymentBL
    {
        public p82Proforma_PaymentBL(BL.Factory mother) : base(mother)
        {

        }


        private string GetSQL1(string strAppend = null)
        {
            sb("SELECT a.*,");
            sb(_db.GetSQL1_Ocas("p82",false,false,true));
            sb(" FROM p82Proforma_Payment a");
            sb(strAppend);
            return sbret();
        }
        public BO.p82Proforma_Payment Load(int pid)
        {
            return _db.Load<BO.p82Proforma_Payment>(GetSQL1(" WHERE a.p82ID=@pid"), new { pid = pid });
        }

        public IEnumerable<BO.p82Proforma_Payment> GetList(int p90id)
        {            
            return _db.GetList<BO.p82Proforma_Payment>(GetSQL1(" WHERE a.p90ID=@p90id"), new { p90id = p90id });
        }



        public int Save(BO.p82Proforma_Payment rec)
        {
            var recP90 = _mother.p90ProformaBL.Load(rec.p90ID);

            if (!ValidateBeforeSave(rec, recP90))
            {
                return 0;
            }

            using (var sc = new System.Transactions.TransactionScope())
            {
                var p = new DL.Params4Dapper();
                p.AddInt("pid", rec.pid);
                p.AddInt("p90ID", rec.p90ID, true);
                p.AddString("p82Code", rec.p82Code);
                p.AddDateTime("p82Date", rec.p82Date);
                p.AddDateTime("p82DateIssue", rec.p82DateIssue);
                p.AddDouble("p82Amount", rec.p82Amount);
                if (rec.p82Amount != (rec.p82Amount_WithoutVat + rec.p82Amount_Vat))
                {
                    if (recP90.p90VatRate > 0)
                    {
                        rec.p82Amount_WithoutVat = Math.Round(rec.p82Amount / (1 + recP90.p90VatRate / 100), 2);
                        rec.p82Amount_Vat = rec.p82Amount - rec.p82Amount_WithoutVat;
                    }
                    else
                    {
                        rec.p82Amount_WithoutVat = rec.p82Amount;
                        rec.p82Amount_Vat = 0;
                    }
                }
                p.AddDouble("p82Amount_WithoutVat", rec.p82Amount_WithoutVat);
                p.AddDouble("p82Amount_Vat", rec.p82Amount_Vat);
                p.AddString("p82Text", rec.p82Text);

                int intPID = _db.SaveRecord("p82Proforma_Payment", p, rec,false,true);
                if (intPID > 0)
                {
                    if (_db.RunSql("exec dbo.p90_aftersave @p90id,@j02id_sys", new { p90id = rec.p90ID, j02id_sys = _mother.CurrentUser.pid }))
                    {
                        sc.Complete();
                    }
                }

                return intPID;
            }
                

        }
        private bool ValidateBeforeSave(BO.p82Proforma_Payment rec,BO.p90Proforma recP90)
        {
            if (rec.p90ID == 0)
            {
                this.AddMessage("Chybí vazba na zálohu."); return false;
            }
            if (rec.p82Amount <= 0)
            {
                this.AddMessage("Částka úhrady musí být větší než nula."); return false;
            }
            if (rec.p82Date == null)
            {
                this.AddMessage("Chybí datum úhrady."); return false;
            }
            
            var lisP82 = GetList(rec.p90ID).Where(p => p.pid != rec.pid);
            if ((lisP82.Sum(p=>p.p82Amount)+ rec.p82Amount) > recP90.p90Amount)
            {
                this.AddMessage("Celková částka úhrad zálohy je vyšší než částka zálohy.");return false;
            }

           
            return true;
        }

    }
}
