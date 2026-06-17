using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BL
{
    public interface Ij25ReportCategoryBL
    {
        public BO.j25ReportCategory Load(int pid);
        public BO.j25ReportCategory LoadByCode(string strCode, int pid_exclude);
        public IEnumerable<BO.j25ReportCategory> GetList(BO.myQuery mq);
        public int Save(BO.j25ReportCategory rec);

    }
    class j25ReportCategoryBL : BaseBL, Ij25ReportCategoryBL
    {
        public j25ReportCategoryBL(BL.Factory mother) : base(mother)
        {

        }


        private string GetSQL1(string strAppend = null)
        {
            sb("SELECT a.*,");
            sb(_db.GetSQL1_Ocas("j25"));
            sb(" FROM j25ReportCategory a");
            sb(strAppend);
            return sbret();
        }
        public BO.j25ReportCategory Load(int pid)
        {
            return _db.Load<BO.j25ReportCategory>(GetSQL1(" WHERE a.j25ID=@pid"), new { pid = pid });
        }
        public BO.j25ReportCategory LoadByCode(string strCode, int pid_exclude)
        {
            if (pid_exclude > 0)
            {
                return _db.Load<BO.j25ReportCategory>(GetSQL1(" WHERE a.j25Code LIKE @code AND a.j25ID<>@pid_exclude AND a.x01ID=@x01id"), new { code = strCode, pid_exclude = pid_exclude, x01id = _mother.CurrentUser.x01ID });
            }
            else
            {
                return _db.Load<BO.j25ReportCategory>(GetSQL1(" WHERE a.j25Code LIKE @code AND a.x01ID=@x01id"), new { code = strCode,x01id=_mother.CurrentUser.x01ID });
            }
        }

        public IEnumerable<BO.j25ReportCategory> GetList(BO.myQuery mq)
        {
            DL.FinalSqlCommand fq = DL.basQuery.GetFinalSql(GetSQL1(), mq, _mother.CurrentUser);
            return _db.GetList<BO.j25ReportCategory>(fq.FinalSql, fq.Parameters);
        }



        public int Save(BO.j25ReportCategory rec)
        {
            if (!ValidateBeforeSave(rec))
            {
                return 0;
            }
            var p = new DL.Params4Dapper();
            p.AddInt("pid", rec.pid);
            p.AddInt("x01ID", rec.x01ID == 0 ? _mother.CurrentUser.x01ID : rec.x01ID, true);
            p.AddString("j25Name", rec.j25Name);
            p.AddString("j25Code", rec.j25Code);
            p.AddInt("j25Ordinary", rec.j25Ordinary);

            return _db.SaveRecord("j25ReportCategory", p, rec);

        }
        private bool ValidateBeforeSave(BO.j25ReportCategory rec)
        {
            if (string.IsNullOrEmpty(rec.j25Name))
            {
                this.AddMessage("Chybí vyplnit [Název]."); return false;
            }


            return true;
        }

    }
}
