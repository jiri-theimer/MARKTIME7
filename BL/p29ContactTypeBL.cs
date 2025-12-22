using System;
using System.Collections.Generic;
using System.Text;
namespace BL
{
    public interface Ip29ContactTypeBL
    {
        public BO.p29ContactType Load(int pid);
        public IEnumerable<BO.p29ContactType> GetList(BO.myQuery mq);
        public IEnumerable<BO.p29ContactType> GetList_ContactCreate();   //seznam typů pro které může uživatel zakládat nové kontakty
        public int Save(BO.p29ContactType rec, List<BO.x69EntityRole_Assign> lisX69, List<BO.j08CreatePermission> lisJ08);

    }
    class p29ContactTypeBL : BaseBL, Ip29ContactTypeBL
    {
        public p29ContactTypeBL(BL.Factory mother) : base(mother)
        {

        }


        private string GetSQL1(string strAppend = null)
        {
            sb("SELECT a.*,b01.b01Name,");
            sb(_db.GetSQL1_Ocas("p29"));
            sb(" FROM p29ContactType a LEFT OUTER JOIN b01WorkflowTemplate b01 ON a.b01ID=b01.b01ID");
            sb(strAppend);
            return sbret();
        }
        public BO.p29ContactType Load(int pid)
        {
            return _db.Load<BO.p29ContactType>(GetSQL1(" WHERE a.p29ID=@pid"), new { pid = pid });
        }

        public IEnumerable<BO.p29ContactType> GetList(BO.myQuery mq)
        {
            if (mq.explicit_orderby == null) mq.explicit_orderby = "a.p29Ordinary,a.p29Name";
            DL.FinalSqlCommand fq = DL.basQuery.GetFinalSql(GetSQL1(), mq, _mother.CurrentUser);
            return _db.GetList<BO.p29ContactType>(fq.FinalSql, fq.Parameters);
        }

        public IEnumerable<BO.p29ContactType> GetList_ContactCreate()
        {
            if (_mother.CurrentUser.TestPermission(BO.PermValEnum.GR_p28_Creator))
            {
                return GetList(new BO.myQuery("p29"));  //všechny typy
            }
            string s = GetSQL1(" WHERE a.p29ID IN (select j08RecordPid FROM j08CreatePermission WHERE j08RecordEntity='p29' AND (j08IsAllUsers=1 OR j02ID=@j02id OR j04ID=@j04id");
            if (_mother.CurrentUser.j11IDs != null)
            {
                s += " OR j11ID IN (" + _mother.CurrentUser.j11IDs+")";
            }
            s += "))";
            if (_mother.CurrentUser.IsHostingModeTotalCloud)
            {
                s += $" AND a.x01ID={_mother.CurrentUser.x01ID}";
            }
            
            return _db.GetList<BO.p29ContactType>(s, new {j02id= _mother.CurrentUser.pid,j04id=_mother.CurrentUser.j04ID });
        }


        public int Save(BO.p29ContactType rec, List<BO.x69EntityRole_Assign> lisX69,List<BO.j08CreatePermission> lisJ08)
        {
            if (!ValidateBeforeSave(rec))
            {
                return 0;
            }
            var p = new DL.Params4Dapper();            
            p.AddInt("pid", rec.pid);
            p.AddEnumInt("p29ScopeFlag", rec.p29ScopeFlag);
            p.AddInt("x01ID",rec.x01ID==0 ? _mother.CurrentUser.x01ID : rec.x01ID);
            
            p.AddInt("b01ID", rec.b01ID, true);
            p.AddInt("x38ID", rec.x38ID, true);
            p.AddString("p29Name", rec.p29Name);
            p.AddInt("p29Ordinary", rec.p29Ordinary);
            
            p.AddByte("p29FilesTab", rec.p29FilesTab);
            p.AddByte("p29RolesTab", rec.p29RolesTab);
            p.AddByte("p29BillingTab", rec.p29BillingTab);
            p.AddByte("p29ContactPersonsTab", rec.p29ContactPersonsTab);
            p.AddByte("p29ContactMediaTab", rec.p29ContactMediaTab);

            int intPID = _db.SaveRecord("p29ContactType", p, rec);
            if (intPID > 0)
            {
                if (lisX69 != null)
                {
                    DL.BAS.SaveX69(_db, "p29", intPID, lisX69);
                }
                if (lisJ08 != null)
                {
                    DL.BAS.SaveJ08(_db, "p29", intPID, lisJ08);
                }

            }

            return intPID;
        }
        private bool ValidateBeforeSave(BO.p29ContactType rec)
        {
            if (string.IsNullOrEmpty(rec.p29Name))
            {
                this.AddMessage("Chybí vyplnit [Název]."); return false;
            }

            return true;
        }

    }
}
