using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BL
{
    public interface Ir02CapacityVersionBL
    {
        public BO.r02CapacityVersion Load(int pid);
        public IEnumerable<BO.r02CapacityVersion> GetList(BO.myQuery mq);
        public int Save(BO.r02CapacityVersion rec);

    }
    class r02CapacityVersionBL : BaseBL, Ir02CapacityVersionBL
    {
        public r02CapacityVersionBL(BL.Factory mother) : base(mother)
        {

        }


        private string GetSQL1(string strAppend = null)
        {
            sb("SELECT a.*,");
            sb(_db.GetSQL1_Ocas("r02"));
            sb(" FROM r02CapacityVersion a");
            sb(strAppend);
            return sbret();
        }
        public BO.r02CapacityVersion Load(int pid)
        {
            return _db.Load<BO.r02CapacityVersion>(GetSQL1(" WHERE a.r02ID=@pid"), new { pid = pid });
        }

        public IEnumerable<BO.r02CapacityVersion> GetList(BO.myQuery mq)
        {
            DL.FinalSqlCommand fq = DL.basQuery.GetFinalSql(GetSQL1(), mq, _mother.CurrentUser);
            return _db.GetList<BO.r02CapacityVersion>(fq.FinalSql, fq.Parameters);
        }



        public int Save(BO.r02CapacityVersion rec)
        {
            if (!ValidateBeforeSave(rec))
            {
                return 0;
            }
            var p = new DL.Params4Dapper();
            p.AddInt("pid", rec.pid);
            p.AddInt("x01ID", rec.x01ID == 0 ? _mother.CurrentUser.x01ID : rec.x01ID, true);
            p.AddString("r02Name", rec.r02Name);
            
            
            p.AddEnumInt("r02WorksheetFlag", rec.r02WorksheetFlag);
            p.AddInt("r02Ordinary", rec.r02Ordinary);
            
            return _db.SaveRecord("r02CapacityVersion", p, rec);

        }
        private bool ValidateBeforeSave(BO.r02CapacityVersion rec)
        {
            if (string.IsNullOrEmpty(rec.r02Name))
            {
                this.AddMessage("Chybí vyplnit [Název]."); return false;
            }
            
           

            return true;
        }

    }
}
