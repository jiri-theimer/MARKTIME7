using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BL
{
    public interface Ij61TextTemplateBL
    {
        public BO.j61TextTemplate Load(int pid);
        public IEnumerable<BO.j61TextTemplate> GetList(BO.myQueryJ61 mq);
        public int Save(BO.j61TextTemplate rec);

    }
    class j61TextTemplateBL : BaseBL, Ij61TextTemplateBL
    {
        public j61TextTemplateBL(BL.Factory mother) : base(mother)
        {

        }


        private string GetSQL1(string strAppend = null)
        {
            sb("SELECT a.*,own.j02LastName+' '+own.j02FirstName as Owner,");
            sb(_db.GetSQL1_Ocas("j61"));
            sb(" FROM j61TextTemplate a LEFT OUTER JOIN j02User own ON a.j02ID_Owner=own.j02ID");
            sb(strAppend);
            return sbret();
        }
        public BO.j61TextTemplate Load(int pid)
        {
            return _db.Load<BO.j61TextTemplate>(GetSQL1(" WHERE a.j61ID=@pid"), new { pid = pid });
        }

        public IEnumerable<BO.j61TextTemplate> GetList(BO.myQueryJ61 mq)
        {
            DL.FinalSqlCommand fq = DL.basQuery.GetFinalSql(GetSQL1(), mq, _mother.CurrentUser);
            return _db.GetList<BO.j61TextTemplate>(fq.FinalSql, fq.Parameters);
        }



        public int Save(BO.j61TextTemplate rec)
        {
            if (!ValidateBeforeSave(rec))
            {
                return 0;
            }
            using (var sc = new System.Transactions.TransactionScope())
            {
                var p = new DL.Params4Dapper();
                p.AddInt("pid", rec.pid);
                p.AddInt("x01ID", rec.x01ID == 0 ? _mother.CurrentUser.x01ID : rec.x01ID);
                
                p.AddString("j61Entity", rec.j61Entity);
                p.AddString("j61Name", rec.j61Name);
                p.AddInt("j02ID_Owner", rec.j02ID_Owner, true);
                p.AddString("j61MailBody", rec.j61MailBody);
                p.AddInt("x04ID", rec.x04ID,true);
                p.AddString("j61MailSubject", rec.j61MailSubject);
                p.AddString("j61MailTO", rec.j61MailTO);
                p.AddString("j61MailCC", rec.j61MailCC);
                p.AddString("j61MailBCC", rec.j61MailBCC);
                p.AddString("j61MessageFields", rec.j61MessageFields);
                p.AddString("j61HtmlTemplateFile", rec.j61HtmlTemplateFile);
                p.AddString("j61GridColumns", rec.j61GridColumns);
                p.AddEnumInt("j61RecordLinkFlag", rec.j61RecordLinkFlag);
                p.AddEnumInt("j61GridColumnsFlag", rec.j61GridColumnsFlag);
                p.AddEnumInt("j61UserSignatureFlag", rec.j61UserSignatureFlag);
                p.AddBool("j61IsPublic", rec.j61IsPublic);
                p.AddString("j61SqlSource", rec.j61SqlSource);

                p.AddInt("j61Ordinary", rec.j61Ordinary);

                int intPID = _db.SaveRecord("j61TextTemplate", p, rec);
                if (intPID > 0)
                {
                    sc.Complete();
                }

                return intPID;
            }
                

        }
        private bool ValidateBeforeSave(BO.j61TextTemplate rec)
        {
            if (string.IsNullOrEmpty(rec.j61Name))
            {
                this.AddMessage("Chybí vyplnit [Název]."); return false;
            }
            if (string.IsNullOrEmpty(rec.j61Entity))
            {
                this.AddMessage("Chybí vyplnit [Entita]."); return false;
            }
            if (rec.j02ID_Owner == 0)
            {
                rec.j02ID_Owner = _mother.CurrentUser.pid;
            }

            return true;
        }

    }
}
