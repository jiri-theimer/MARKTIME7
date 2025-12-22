using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BL
{
    public interface Ip92InvoiceTypeBL
    {
        public BO.p92InvoiceType Load(int pid);
        public IEnumerable<BO.p92InvoiceType> GetList(BO.myQuery mq);
        public int Save(BO.p92InvoiceType rec);

    }
    class p92InvoiceTypeBL : BaseBL, Ip92InvoiceTypeBL
    {
        public p92InvoiceTypeBL(BL.Factory mother) : base(mother)
        {

        }


        private string GetSQL1(string strAppend = null)
        {
            sb("SELECT a.*,x15.x15Name,j27.j27Code,j61.j61Name,p93.p93Name,p83.p83Name,");
            sb(_db.GetSQL1_Ocas("p92"));
            sb(" FROM p92InvoiceType a LEFT OUTER JOIN x15VatRateType x15 ON a.x15ID=x15.x15ID LEFT OUTER JOIN j27Currency j27 ON a.j27ID=j27.j27ID");
            sb(" LEFT OUTER JOIN p93InvoiceHeader p93 ON a.p93ID=p93.p93ID LEFT OUTER JOIN j61TextTemplate j61 ON a.j61ID=j61.j61ID LEFT OUTER JOIN p83UpominkaType p83 ON a.p83ID=p83.p83ID");
            sb(strAppend);
            return sbret();
        }
        public BO.p92InvoiceType Load(int pid)
        {
            return _db.Load<BO.p92InvoiceType>(GetSQL1(" WHERE a.p92ID=@pid"), new { pid = pid });
        }

        public IEnumerable<BO.p92InvoiceType> GetList(BO.myQuery mq)
        {
            if (mq.explicit_orderby == null) mq.explicit_orderby = "a.p92Ordinary";
            DL.FinalSqlCommand fq = DL.basQuery.GetFinalSql(GetSQL1(), mq, _mother.CurrentUser);
            return _db.GetList<BO.p92InvoiceType>(fq.FinalSql, fq.Parameters);
        }



        public int Save(BO.p92InvoiceType rec)
        {
            if (!ValidateBeforeSave(rec))
            {
                return 0;
            }
            var p = new DL.Params4Dapper();
            p.AddInt("pid", rec.pid);
            p.AddInt("x01ID", rec.x01ID == 0 ? _mother.CurrentUser.x01ID : rec.x01ID);
            p.AddString("p92Name", rec.p92Name);
            p.AddString("p92Code", rec.p92Code);
            p.AddEnumInt("p92TypeFlag", rec.p92TypeFlag);
            p.AddEnumInt("p92QrCodeFlag", rec.p92QrCodeFlag);
            p.AddInt("p92Ordinary", rec.p92Ordinary);

                     
            p.AddByte("p92FilesTab", rec.p92FilesTab);
            p.AddByte("p92RolesTab", rec.p92RolesTab);

            p.AddInt("j27ID", rec.j27ID, true);
            
            p.AddEnumInt("x15ID", rec.x15ID,true);
            p.AddInt("x38ID", rec.x38ID, true);
            p.AddInt("p98ID", rec.p98ID, true);
            

            p.AddInt("p93ID", rec.p93ID, true);
            p.AddInt("j19ID", rec.j19ID, true);
            p.AddInt("b01ID", rec.b01ID, true);
            p.AddInt("j61ID", rec.j61ID, true);
            p.AddInt("p83ID", rec.p83ID, true);
            p.AddInt("x31ID_Invoice", rec.x31ID_Invoice, true);
            p.AddInt("x31ID_Attachment", rec.x31ID_Attachment, true);
            p.AddInt("x31ID_Letter", rec.x31ID_Letter, true);

            p.AddInt("p80ID", rec.p80ID, true);
            p.AddInt("p32ID_CreditNote", rec.p32ID_CreditNote, true);
            
            p.AddString("p92ReportConstantPreText1", rec.p92ReportConstantPreText1);
            p.AddString("p92InvoiceDefaultText1", rec.p92InvoiceDefaultText1);
            p.AddString("p92InvoiceDefaultText2", rec.p92InvoiceDefaultText2);
            p.AddString("p92ReportConstantText", rec.p92ReportConstantText);

            p.AddString("p92RepDocName", rec.p92RepDocName);
            p.AddString("p92RepDocNumber", rec.p92RepDocNumber);


            int intPID = _db.SaveRecord("p92InvoiceType", p, rec);

            return intPID;


        }
        private bool ValidateBeforeSave(BO.p92InvoiceType rec)
        {
            if (string.IsNullOrEmpty(rec.p92Name))
            {
                this.AddMessage("Chybí vyplnit [Název]."); return false;
            }
            if (rec.p93ID == 0)
            {
                this.AddMessage("Chybí vyplnit [Vystavovatel faktury]."); return false;
            }
            if (rec.x38ID == 0)
            {
                this.AddMessage("Chybí vyplnit [Číselná řada]."); return false;
            }
            if (rec.j27ID == 0)
            {
                this.AddMessage("Chybí vyplnit výchozí měnu faktury."); return false;
            }

            if (rec.p92TypeFlag == BO.p92TypeFlagENUM.CreditNote && rec.p32ID_CreditNote==0)
            {
                this.AddMessage("Pro opravný doklad chybí aktivita dobropisované částky.");return false;
            }

            return true;
        }

    }
}
