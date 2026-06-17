using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BL
{
    public interface Io21MilestoneTypeBL
    {
        public BO.o21MilestoneType Load(int pid);
        public IEnumerable<BO.o21MilestoneType> GetList(BO.myQuery mq);
        public int Save(BO.o21MilestoneType rec);

    }
    class o21MilestoneTypeBL : BaseBL, Io21MilestoneTypeBL
    {
        public o21MilestoneTypeBL(BL.Factory mother) : base(mother)
        {

        }


        private string GetSQL1(string strAppend = null)
        {
            sb("SELECT a.*,");
            sb(_db.GetSQL1_Ocas("o21"));
            sb(" FROM o21MilestoneType a");
            sb(strAppend);
            return sbret();
        }
        public BO.o21MilestoneType Load(int pid)
        {
            return _db.Load<BO.o21MilestoneType>(GetSQL1(" WHERE a.o21ID=@pid"), new { pid = pid });
        }

        public IEnumerable<BO.o21MilestoneType> GetList(BO.myQuery mq)
        {
            if (mq.explicit_orderby == null) mq.explicit_orderby = "a.o21Ordinary";
            DL.FinalSqlCommand fq = DL.basQuery.GetFinalSql(GetSQL1(), mq, _mother.CurrentUser);
            return _db.GetList<BO.o21MilestoneType>(fq.FinalSql, fq.Parameters);
        }



        public int Save(BO.o21MilestoneType rec)
        {
            if (!ValidateBeforeSave(rec))
            {
                return 0;
            }
            var p = new DL.Params4Dapper();
            p.AddInt("pid", rec.pid);
            p.AddInt("x01ID", rec.x01ID == 0 ? _mother.CurrentUser.x01ID : rec.x01ID, true);
            p.AddString("o21Name", rec.o21Name);            
            p.AddEnumInt("o21TypeFlag", rec.o21TypeFlag);
            p.AddInt("o21Ordinary", rec.o21Ordinary);
            p.AddString("o21Color", rec.o21Color);
            p.AddBool("o21IsP41Compulsory", rec.o21IsP41Compulsory);
            return _db.SaveRecord("o21MilestoneType", p, rec);

        }
        private bool ValidateBeforeSave(BO.o21MilestoneType rec)
        {
            if (string.IsNullOrEmpty(rec.o21Name))
            {
                this.AddMessage("Chybí vyplnit [Název]."); return false;
            }


            return true;
        }

    }
}
