using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BL
{
    public interface Ix27EntityFieldGroupBL
    {
        public BO.x27EntityFieldGroup Load(int pid);
        public IEnumerable<BO.x27EntityFieldGroup> GetList(BO.myQuery mq);
        public int Save(BO.x27EntityFieldGroup rec);

    }
    class x27EntityFieldGroupBL : BaseBL, Ix27EntityFieldGroupBL
    {
        public x27EntityFieldGroupBL(BL.Factory mother) : base(mother)
        {

        }


        private string GetSQL1(string strAppend = null)
        {
            sb("SELECT a.*,");
            sb(_db.GetSQL1_Ocas("x27"));
            sb(" FROM x27EntityFieldGroup a");
            sb(strAppend);
            return sbret();
        }
        public BO.x27EntityFieldGroup Load(int pid)
        {
            return _db.Load<BO.x27EntityFieldGroup>(GetSQL1(" WHERE a.x27ID=@pid"), new { pid = pid });
        }

        public IEnumerable<BO.x27EntityFieldGroup> GetList(BO.myQuery mq)
        {
            DL.FinalSqlCommand fq = DL.basQuery.GetFinalSql(GetSQL1(), mq, _mother.CurrentUser);
            return _db.GetList<BO.x27EntityFieldGroup>(fq.FinalSql, fq.Parameters);
        }



        public int Save(BO.x27EntityFieldGroup rec)
        {
            if (!ValidateBeforeSave(rec))
            {
                return 0;
            }
            var p = new DL.Params4Dapper();
            p.AddInt("pid", rec.pid);
            p.AddInt("x01ID", rec.x01ID == 0 ? _mother.CurrentUser.x01ID : rec.x01ID, true);
            p.AddString("x27Name", rec.x27Name);
            p.AddInt("x27Ordinary", rec.x27Ordinary);
           
            return _db.SaveRecord("x27EntityFieldGroup", p, rec);
         
        }
        private bool ValidateBeforeSave(BO.x27EntityFieldGroup rec)
        {
            if (string.IsNullOrEmpty(rec.x27Name))
            {
                this.AddMessage("Chybí vyplnit [Název]."); return false;
            }


            return true;
        }

    }
}
