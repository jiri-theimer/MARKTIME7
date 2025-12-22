using System;
using System.Collections.Generic;
using System.Text;

namespace BL
{
    public interface Ib01WorkflowTemplateBL
    {
        public BO.b01WorkflowTemplate Load(int pid);        
        public IEnumerable<BO.b01WorkflowTemplate> GetList(BO.myQuery mq);
        public int Save(BO.b01WorkflowTemplate rec);
        public IEnumerable<BO.b11WorkflowMessageToStep> GetListB11(int b01id);

    }
    class b01WorkflowTemplateBL : BaseBL, Ib01WorkflowTemplateBL
    {
        public b01WorkflowTemplateBL(BL.Factory mother) : base(mother)
        {

        }


        private string GetSQL1(string strAppend = null)
        {
            sb("SELECT a.*,");
            sb(_db.GetSQL1_Ocas("b01"));
            sb(" FROM b01WorkflowTemplate a");
            sb(strAppend);
            return sbret();
        }
        public BO.b01WorkflowTemplate Load(int pid)
        {
            return _db.Load<BO.b01WorkflowTemplate>(GetSQL1(" WHERE a.b01ID=@pid"), new { pid = pid });
        }
       

        public IEnumerable<BO.b01WorkflowTemplate> GetList(BO.myQuery mq)
        {
            if (mq.explicit_orderby == null) { mq.explicit_orderby = "a.b01Name"; };
            DL.FinalSqlCommand fq = DL.basQuery.GetFinalSql(GetSQL1(), mq, _mother.CurrentUser);
            return _db.GetList<BO.b01WorkflowTemplate>(fq.FinalSql, fq.Parameters);
        }



        public int Save(BO.b01WorkflowTemplate rec)
        {
            if (ValidateBeforeSave(rec) == false)
            {
                return 0;
            }
            var p = new DL.Params4Dapper();
            p.AddInt("pid", rec.pid);            
            p.AddString("b01Name", rec.b01Name);
            p.AddString("b01Entity", rec.b01Entity);
            p.AddEnumInt("b01PrincipleFlag", rec.b01PrincipleFlag);
            p.AddInt("x01ID", rec.x01ID == 0 ? _mother.CurrentUser.x01ID : rec.x01ID);

            int intPID = _db.SaveRecord("b01WorkflowTemplate", p, rec);
            if (intPID > 0 && rec.pid==0)
            {
                //založit úvodní stavy šablony
                
                _mother.b02WorkflowStatusBL.Save(new BO.b02WorkflowStatus() { b01ID = intPID, b02AutoRunFlag = BO.b02AutoRunFlagEnum.Technicky, b02Name = "Technický stav šablony", b02Ordinary = 999 });

                if (rec.b01Entity != "x01")
                {
                    _mother.b02WorkflowStatusBL.Save(new BO.b02WorkflowStatus() { b01ID = intPID, b02AutoRunFlag = BO.b02AutoRunFlagEnum.Startovaci, b02Name = "Záznam založen", b02Ordinary = -1 });
                }
                                                
            }
            return intPID;
        }

        public bool ValidateBeforeSave(BO.b01WorkflowTemplate rec)
        {
            if (string.IsNullOrEmpty(rec.b01Name))
            {
                this.AddMessage("Chybí vyplnit [Název]."); return false;
            }


            if (string.IsNullOrEmpty(rec.b01Entity))
            {
                this.AddMessage("Chybí vyplnit [Entita]."); return false;
            }

            if (rec.b01Entity=="x01" && GetList(new BO.myQuery("b01") { IsRecordValid=null}).Where(p=>p.b01Entity=="x01" && p.pid !=rec.pid).Count() > 0)
            {
                this.AddMessage("Může být založena pouze jedna workflow šablona pro entitu [Systém]."); return false;
            }


            return true;
        }


        public IEnumerable<BO.b11WorkflowMessageToStep> GetListB11(int b01id)
        {
            sb("SELECT a.*,j61.j61Name,x67.x67Name,j04.j04Name,j11.j11Name,");
            sb(_db.GetSQL1_Ocas("b11", false, false, false));
            sb(" FROM b11WorkflowMessageToStep a INNER JOIN j61TextTemplate j61 ON a.j61ID=j61.j61ID INNER JOIN b06WorkflowStep b06 ON a.b06ID=b06.b06ID INNER JOIN b02WorkflowStatus b02 ON b06.b02ID=b02.b02ID");
            sb(" LEFT OUTER JOIN x67EntityRole x67 ON a.x67ID=x67.x67ID LEFT OUTER JOIN j04UserRole j04 ON a.j04ID=j04.j04ID LEFT OUTER JOIN j11Team j11 ON a.j11ID=j11.j11ID");
            sb(" WHERE b02.b01ID=@b01id");

            return _db.GetList<BO.b11WorkflowMessageToStep>(sbret(), new { b01id = b01id });
        }

    }
}
