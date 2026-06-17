using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BL
{
    public interface Io17DocMenuBL
    {
        public BO.o17DocMenu Load(int pid);
        public IEnumerable<BO.o17DocMenu> GetList(BO.myQuery mq);
        public int Save(BO.o17DocMenu rec, List<BO.x69EntityRole_Assign> lisX69);

    }
    class o17DocMenuBL : BaseBL, Io17DocMenuBL
    {
        public o17DocMenuBL(BL.Factory mother) : base(mother)
        {

        }


        private string GetSQL1(string strAppend = null)
        {
            sb("SELECT a.*,");
            sb(_db.GetSQL1_Ocas("o17"));
            sb(" FROM o17DocMenu a");
            sb(strAppend);
            return sbret();
        }
        public BO.o17DocMenu Load(int pid)
        {
            return _db.Load<BO.o17DocMenu>(GetSQL1(" WHERE a.o17ID=@pid"), new { pid = pid });
        }

        public IEnumerable<BO.o17DocMenu> GetList(BO.myQuery mq)
        {
            if (mq.explicit_orderby == null) mq.explicit_orderby = "a.o17Ordinary";
            DL.FinalSqlCommand fq = DL.basQuery.GetFinalSql(GetSQL1(), mq, _mother.CurrentUser);
            return _db.GetList<BO.o17DocMenu>(fq.FinalSql, fq.Parameters);
        }



        public int Save(BO.o17DocMenu rec, List<BO.x69EntityRole_Assign> lisX69)
        {
            if (!ValidateBeforeSave(rec))
            {
                return 0;
            }
            var p = new DL.Params4Dapper();
            p.AddInt("pid", rec.pid);
            p.AddInt("x01ID", rec.x01ID == 0 ? _mother.CurrentUser.x01ID : rec.x01ID, true);
            p.AddString("o17Name", rec.o17Name);
            
            p.AddInt("o17Ordinary", rec.o17Ordinary);

            int intO17ID= _db.SaveRecord("o17DocMenu", p, rec);
            if (intO17ID > 0)
            {
                if (lisX69 != null && !DL.BAS.SaveX69(_db, "o17", intO17ID, lisX69))
                {
                    this.AddMessageTranslated("Error: DL.BAS.SaveX69");
                    return 0;
                }
                if (_mother.App.HostingMode == Singleton.HostingModeEnum.SharedApp) //u sdílené aplikace je třeba aktualizovat obsah v o17DocMenu [a7_cloudheader]
                {
                    new DL.HostingTasks(_db).UpdateCloudHeader_O17(_mother.Lic.x01LoginDomain);
                }
                
                _mother.App.RefreshO17List(); //obnovit singleton seznam o17
                _mother.j02UserBL.ClearAllUsersCache(); //vyčistit uživatelům j02Cache_TimeStamp
            }
            
            return intO17ID;

        }
        private bool ValidateBeforeSave(BO.o17DocMenu rec)
        {
            if (string.IsNullOrEmpty(rec.o17Name))
            {
                this.AddMessage("Chybí vyplnit [Název]."); return false;
            }


            return true;
        }

    }
}
