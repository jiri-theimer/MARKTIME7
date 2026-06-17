using System;
using System.Collections.Generic;
using System.Text;

namespace BL
{
    public interface Io15AutoCompleteBL
    {
        public BO.o15AutoComplete Load(int pid);
        public BO.o15AutoComplete LoadByValue(string strValue, BO.AutoCompleteFlag flag, int intExcludePid);
        public IEnumerable<BO.o15AutoComplete> GetList(BO.myQuery mq);
        public int Save(BO.o15AutoComplete rec);

    }
    class o15AutoCompleteBL : BaseBL, Io15AutoCompleteBL
    {
        public o15AutoCompleteBL(BL.Factory mother) : base(mother)
        {

        }


        private string GetSQL1(string strAppend = null)
        {
            sb("SELECT a.*,");
            sb(_db.GetSQL1_Ocas("o15"));
            sb(" FROM o15AutoComplete a");
            sb(strAppend);
            return sbret();
        }
        public BO.o15AutoComplete Load(int pid)
        {
            return _db.Load<BO.o15AutoComplete>(GetSQL1(" WHERE a.o15ID=@pid"), new { pid = pid });
        }
        public BO.o15AutoComplete LoadByValue(string strValue, BO.AutoCompleteFlag flag,int intExcludePid)
        {
            int x = (int)flag;
            return _db.Load<BO.o15AutoComplete>(GetSQL1(" WHERE a.o15Value LIKE @val AND a.o15Flag=@flag AND a.o15ID<>@excludepid AND a.x01ID=@x01id"), new { val = strValue, flag = x,excludepid= intExcludePid,x01id=_mother.CurrentUser.x01ID });
        }

        public IEnumerable<BO.o15AutoComplete> GetList(BO.myQuery mq)
        {
            DL.FinalSqlCommand fq = DL.basQuery.GetFinalSql(GetSQL1(), mq, _mother.CurrentUser);
            return _db.GetList<BO.o15AutoComplete>(fq.FinalSql, fq.Parameters);
        }



        public int Save(BO.o15AutoComplete rec)
        {
            if (ValidateBeforeSave(rec) == false)
            {
                return 0;
            }
            var p = new DL.Params4Dapper();            
            p.AddInt("pid", rec.o15ID);
            p.AddInt("x01ID", rec.x01ID == 0 ? _mother.CurrentUser.x01ID : rec.x01ID);
            p.AddString("o15Value", rec.o15Value);
            p.AddEnumInt("o15Flag", rec.o15Flag);
            p.AddInt("o15Ordinary", rec.o15Ordinary);
            

            int intPID = _db.SaveRecord("o15AutoComplete", p, rec);



            return intPID;
        }

        public bool ValidateBeforeSave(BO.o15AutoComplete rec)
        {
            if (string.IsNullOrEmpty(rec.o15Value))
            {
                this.AddMessage("Chybí vyplnit [Hodnota položky]."); return false;
            }
            if (LoadByValue(rec.o15Value,rec.o15Flag,rec.pid) != null)
            {
                this.AddMessage("Hodnota položky musí být unikátní."); return false;
            }

            return true;
        }

    }
}
