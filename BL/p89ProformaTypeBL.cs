using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BL
{
    public interface Ip89ProformaTypeBL
    {
        public BO.p89ProformaType Load(int pid);
        public IEnumerable<BO.p89ProformaType> GetList(BO.myQuery mq);
        public IEnumerable<BO.p89ProformaType> GetList_ProformaCreate();
        public int Save(BO.p89ProformaType rec, List<BO.j08CreatePermission> lisJ08);

    }
    class p89ProformaTypeBL : BaseBL, Ip89ProformaTypeBL
    {
        public p89ProformaTypeBL(BL.Factory mother) : base(mother)
        {

        }


        private string GetSQL1(string strAppend = null)
        {
            sb("SELECT a.*,j27.j27Code,p93.p93Name,");
            sb(_db.GetSQL1_Ocas("p89"));
            sb(" FROM p89ProformaType a LEFT OUTER JOIN j27Currency j27 ON a.j27ID=j27.j27ID");
            sb(" LEFT OUTER JOIN p93InvoiceHeader p93 ON a.p93ID=p93.p93ID");
            sb(strAppend);
            return sbret();
        }
        public BO.p89ProformaType Load(int pid)
        {
            return _db.Load<BO.p89ProformaType>(GetSQL1(" WHERE a.p89ID=@pid"), new { pid = pid });
        }

        public IEnumerable<BO.p89ProformaType> GetList(BO.myQuery mq)
        {
            DL.FinalSqlCommand fq = DL.basQuery.GetFinalSql(GetSQL1(), mq, _mother.CurrentUser);
            return _db.GetList<BO.p89ProformaType>(fq.FinalSql, fq.Parameters);
        }



        public int Save(BO.p89ProformaType rec, List<BO.j08CreatePermission> lisJ08)
        {
            if (!ValidateBeforeSave(rec))
            {
                return 0;
            }
            var p = new DL.Params4Dapper();
            p.AddInt("pid", rec.pid);
            p.AddInt("x01ID", rec.x01ID == 0 ? _mother.CurrentUser.x01ID : rec.x01ID);
            p.AddString("p89Name", rec.p89Name);
            p.AddString("p89Code", rec.p89Code);
 
            p.AddInt("x31ID", rec.x31ID, true);
            p.AddInt("j61ID", rec.j61ID, true);
            p.AddInt("x31ID_Payment", rec.x31ID_Payment, true);
            p.AddInt("x38ID", rec.x38ID, true);
            p.AddInt("x38ID_Payment", rec.x38ID_Payment, true);
            
            p.AddInt("p93ID", rec.p93ID, true);

            
            p.AddByte("p89FilesTab", rec.p89FilesTab);
            p.AddByte("p89RolesTab", rec.p89RolesTab);

            p.AddString("p89DefaultText1", rec.p89DefaultText1);
            p.AddString("p89DefaultText2", rec.p89DefaultText2);
           

            int intPID = _db.SaveRecord("p89ProformaType", p, rec);

            if (intPID > 0)
            {
                if (lisJ08 != null)
                {
                    DL.BAS.SaveJ08(_db, "p89", intPID, lisJ08);
                }

            }

            return intPID;


        }
        private bool ValidateBeforeSave(BO.p89ProformaType rec)
        {
            if (string.IsNullOrEmpty(rec.p89Name))
            {
                this.AddMessage("Chybí vyplnit [Název]."); return false;
            }
            if (rec.x38ID == 0)
            {
                this.AddMessage("Chybí vyplnit [Číselná řada]."); return false;
            }
           

            return true;
        }

        public IEnumerable<BO.p89ProformaType> GetList_ProformaCreate()
        {
            if (_mother.CurrentUser.TestPermission(BO.PermValEnum.GR_o23_Creator))
            {
                return GetList(new BO.myQuery("p89"));  //všechny typy
            }
            string s = GetSQL1(" WHERE a.p89ID IN (select j08RecordPid FROM j08CreatePermission WHERE j08RecordEntity='p89' AND (j08IsAllUsers=1 OR j02ID=@j02id OR j04ID=@j04id");
            if (_mother.CurrentUser.j11IDs != null)
            {
                s += " OR j11ID IN (" + _mother.CurrentUser.j11IDs + ")";
            }
            s += "))";

            return _db.GetList<BO.p89ProformaType>(s, new { j02id = _mother.CurrentUser.pid, j04id = _mother.CurrentUser.j04ID });
        }

    }
}
