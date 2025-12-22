using System;
using System.Collections.Generic;
using System.Text;

namespace BL
{
    public interface Ix97TranslateBL
    {
        public BO.x97Translate Load(int pid);        
        public IEnumerable<BO.x97Translate> GetList(BO.myQuery mq);
        public int Save(BO.x97Translate rec);


    }
    class x97TranslateBL : BaseBL, Ix97TranslateBL
    {
        public x97TranslateBL(BL.Factory mother) : base(mother)
        {

        }


        private string GetSQL1(string strAppend = null)
        {
            sb("SELECT a.x97ID,a.x97Code,a.x97Orig,a.x97Lang1,a.x97Lang2,");
            sb(_db.GetSQL1_Ocas("x97",true,false,true));
            sb(" FROM x97Translate a");
            sb(strAppend);
            return sbret();
        }
        public BO.x97Translate Load(int pid)
        {
            return _db.Load<BO.x97Translate>(GetSQL1(" WHERE a.x97ID=@pid"), new { pid = pid });
        }
       
        public IEnumerable<BO.x97Translate> GetList(BO.myQuery mq)
        {
            DL.FinalSqlCommand fq = DL.basQuery.GetFinalSql(GetSQL1(), mq, _mother.CurrentUser);
            return _db.GetList<BO.x97Translate>(fq.FinalSql, fq.Parameters);
        }



        public int Save(BO.x97Translate rec)
        {
            if (ValidateBeforeSave(rec) == false)
            {
                return 0;
            }
            var p = new DL.Params4Dapper();
            p.AddInt("pid", rec.x97ID);
            p.AddString("x97Code", rec.x97Code);
            p.AddString("x97Orig", rec.x97Orig);
            p.AddString("x97Lang1", rec.x97Lang1);
            p.AddString("x97Lang2", rec.x97Lang2);
            p.AddString("x97Lang3", rec.x97Lang3);
            p.AddString("x97Lang4", rec.x97Lang4);
            p.AddString("x97OrigSource", rec.x97OrigSource);
           
            int intPID = _db.SaveRecord("x97Translate", p, rec,false,true);



            return intPID;
        }

        public bool ValidateBeforeSave(BO.x97Translate rec)
        {
            if (string.IsNullOrEmpty(rec.x97Code))
            {
                this.AddMessage("Chybí vyplnit [Originál]."); return false;
            }


            return true;
        }

    }
}
