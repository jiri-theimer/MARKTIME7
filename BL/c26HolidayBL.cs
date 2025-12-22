using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BL
{
    public interface Ic26HolidayBL
    {
        public BO.c26Holiday Load(int pid);
        public IEnumerable<BO.c26Holiday> GetList(BO.myQueryC26 mq);
        public int Save(BO.c26Holiday rec);

    }
    class c26HolidayBL : BaseBL, Ic26HolidayBL
    {
        public c26HolidayBL(BL.Factory mother) : base(mother)
        {

        }


        private string GetSQL1(string strAppend = null)
        {
            sb("SELECT a.*,");
            sb(_db.GetSQL1_Ocas("c26"));
            sb(" FROM c26Holiday a");
            sb(strAppend);
            return sbret();
        }
        public BO.c26Holiday Load(int pid)
        {
            return _db.Load<BO.c26Holiday>(GetSQL1(" WHERE a.c26ID=@pid"), new { pid = pid });
        }

        public IEnumerable<BO.c26Holiday> GetList(BO.myQueryC26 mq)
        {
            DL.FinalSqlCommand fq = DL.basQuery.GetFinalSql(GetSQL1(), mq, _mother.CurrentUser);
            return _db.GetList<BO.c26Holiday>(fq.FinalSql, fq.Parameters);
        }



        public int Save(BO.c26Holiday rec)
        {
            if (!ValidateBeforeSave(rec))
            {
                return 0;
            }
            var p = new DL.Params4Dapper();
            p.AddInt("pid", rec.pid);
            p.AddString("c26Name", rec.c26Name);
            p.AddString("c26CountryCode", rec.c26CountryCode);

            p.AddInt("x01ID", rec.x01ID == 0 ? _mother.CurrentUser.x01ID : rec.x01ID, true);
            p.AddDateTime("c26Date", rec.c26Date);
            
            int intPID = _db.SaveRecord("c26Holiday", p, rec);
            if (intPID > 0)
            {
                _db.RunSql("exec dbo.c26_aftersave @c26id,@j02id_sys", new { c26id = intPID, j02id_sys = _mother.CurrentUser.pid });

            }
            return intPID;
        }
        private bool ValidateBeforeSave(BO.c26Holiday rec)
        {
            if (string.IsNullOrEmpty(rec.c26Name))
            {
                this.AddMessage("Chybí vyplnit [Název]."); return false;
            }
            if (string.IsNullOrEmpty(rec.c26CountryCode))
            {
                this.AddMessage("Chybí vyplnit [Kód země]."); return false;
            }

            return true;
        }

        

    }
}
