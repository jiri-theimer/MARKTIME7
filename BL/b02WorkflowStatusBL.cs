using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BL
{
    public interface Ib02WorkflowStatusBL
    {
        public BO.b02WorkflowStatus Load(int pid);
        public IEnumerable<BO.b02WorkflowStatus> GetList(BO.myQuery mq);
        public int Save(BO.b02WorkflowStatus rec);
       

    }
    class b02WorkflowStatusBL : BaseBL, Ib02WorkflowStatusBL
    {
        public b02WorkflowStatusBL(BL.Factory mother) : base(mother)
        {

        }


        private string GetSQL1(string strAppend = null)
        {
            sb("SELECT a.*,b01x.b01Name,b01x.b01Entity,");
            
            sb(_db.GetSQL1_Ocas("b02"));
            sb(" FROM b02WorkflowStatus a INNER JOIN b01WorkflowTemplate b01x ON a.b01ID=b01x.b01ID");
            sb(strAppend);
            return sbret();
        }
        public BO.b02WorkflowStatus Load(int pid)
        {
            return _db.Load<BO.b02WorkflowStatus>(GetSQL1(" WHERE a.b02ID=@pid"), new { pid = pid });
        }


        public IEnumerable<BO.b02WorkflowStatus> GetList(BO.myQuery mq)
        {
            if (mq.explicit_orderby == null) { mq.explicit_orderby = "a.b01ID,a.b02Ordinary"; };
            DL.FinalSqlCommand fq = DL.basQuery.GetFinalSql(GetSQL1(), mq, _mother.CurrentUser);
            return _db.GetList<BO.b02WorkflowStatus>(fq.FinalSql, fq.Parameters);
        }



        public int Save(BO.b02WorkflowStatus rec)
        {
            if (!ValidateBeforeSave(rec))
            {
                return 0;
            }
            var p = new DL.Params4Dapper();
            p.AddInt("pid", rec.pid);
            p.AddInt("b01ID", rec.b01ID,true);
            p.AddString("b02Name", rec.b02Name);            
            p.AddNonBlackColorString("b02Color", rec.b02Color);
           
            p.AddInt("b02Ordinary", rec.b02Ordinary);
         
            p.AddBool("b02IsRecordReadOnly4Owner", rec.b02IsRecordReadOnly4Owner);
            p.AddEnumInt("b02AutoRunFlag", rec.b02AutoRunFlag);
            p.AddEnumInt("b02RecordFlag", rec.b02RecordFlag);
           

            int intPID = _db.SaveRecord("b02WorkflowStatus", p, rec);
           

            return intPID;
        }

        public bool ValidateBeforeSave(BO.b02WorkflowStatus rec)
        {
            if (string.IsNullOrEmpty(rec.b02Name))
            {
                this.AddMessage("Chybí vyplnit [Název]."); return false;
            }
            if (rec.b01ID == 0)
            {
                this.AddMessage("Chybí vazba na workflow šablonu."); return false;
            }

            if (rec.b02AutoRunFlag == BO.b02AutoRunFlagEnum.Startovaci)
            {
                var lis = _mother.b02WorkflowStatusBL.GetList(new BO.myQuery("b02")).Where(p => p.b01ID == rec.b01ID && p.b02AutoRunFlag == BO.b02AutoRunFlagEnum.Startovaci && p.pid != rec.pid);
                if (lis.Count() > 0)
                {
                    this.AddMessage("Ve workflow šabloně může být pouze jeden Startovací stav."); return false;
                }
            }


            return true;
        }


        
    }
}
