using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BL
{
    public interface Ip57TaskTypeBL
    {
        public BO.p57TaskType Load(int pid);
        public IEnumerable<BO.p57TaskType> GetList(BO.myQuery mq);
        public int Save(BO.p57TaskType rec);

    }
    class p57TaskTypeBL : BaseBL, Ip57TaskTypeBL
    {
        public p57TaskTypeBL(BL.Factory mother) : base(mother)
        {

        }


        private string GetSQL1(string strAppend = null)
        {
            sb("SELECT a.*,b01.b01Name,");
            sb(_db.GetSQL1_Ocas("p57"));
            sb(" FROM p57TaskType a LEFT OUTER JOIN b01WorkflowTemplate b01 ON a.b01ID=b01.b01ID");
            sb(strAppend);
            return sbret();
        }
        public BO.p57TaskType Load(int pid)
        {
            return _db.Load<BO.p57TaskType>(GetSQL1(" WHERE a.p57ID=@pid"), new { pid = pid });
        }

        public IEnumerable<BO.p57TaskType> GetList(BO.myQuery mq)
        {
            DL.FinalSqlCommand fq = DL.basQuery.GetFinalSql(GetSQL1(), mq, _mother.CurrentUser);
            return _db.GetList<BO.p57TaskType>(fq.FinalSql, fq.Parameters);
        }



        public int Save(BO.p57TaskType rec)
        {
            if (!ValidateBeforeSave(rec))
            {
                return 0;
            }
            var p = new DL.Params4Dapper();
            p.AddInt("pid", rec.pid);
            p.AddInt("x01ID", rec.x01ID == 0 ? _mother.CurrentUser.x01ID : rec.x01ID, true);
            p.AddString("p57Name", rec.p57Name);
            p.AddEnumInt("p57PlanScope", rec.p57PlanScope);            
            p.AddInt("p57Ordinary", rec.p57Ordinary);            
            p.AddEnumInt("p57ProjectFlag", rec.p57ProjectFlag);
            p.AddInt("x38ID", rec.x38ID, true);
            p.AddInt("b01ID", rec.b01ID, true);
            p.AddEnumInt("p57HelpdeskFlag", rec.p57HelpdeskFlag);
            return _db.SaveRecord("p57TaskType", p, rec);

        }
        private bool ValidateBeforeSave(BO.p57TaskType rec)
        {
            if (string.IsNullOrEmpty(rec.p57Name))
            {
                this.AddMessage("Chybí vyplnit [Název]."); return false;
            }


            return true;
        }

    }
}
