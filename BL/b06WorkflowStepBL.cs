using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BL
{
    public interface Ib06WorkflowStepBL
    {
        public BO.b06WorkflowStep Load(int pid);
        public IEnumerable<BO.b06WorkflowStep> GetList(BO.myQuery mq);
        public int Save(BO.b06WorkflowStep rec, List<BO.b08WorkflowReceiverToStep> lisB08, List<BO.b11WorkflowMessageToStep> lisB11);
        public IEnumerable<BO.b08WorkflowReceiverToStep> GetListB08(int b06id);
        public IEnumerable<BO.b11WorkflowMessageToStep> GetListB11(int b06id);

    }
    class b06WorkflowStepBL : BaseBL, Ib06WorkflowStepBL
    {
        public b06WorkflowStepBL(BL.Factory mother) : base(mother)
        {

        }


        private string GetSQL1(string strAppend = null)
        {
            sb("SELECT a.*,b02x.b02Name,b02x.b02Color,b01x.b01Name,b02x.b01ID,b01x.b01Entity,");
            sb("isnull(a.b06Name,'')+isnull(' -> '+b02target.b02Name,'') as NameWithTargetStatus,");
            sb(_db.GetSQL1_Ocas("b06"));
            sb(" FROM b06WorkflowStep a INNER JOIN b02WorkflowStatus b02x ON a.b02ID=b02x.b02ID INNER JOIN b01WorkflowTemplate b01x ON b02x.b01ID=b01x.b01ID");
            sb(" LEFT OUTER JOIN b02WorkflowStatus b02target ON a.b02ID_Target=b02target.b02ID");
            sb(strAppend);
            return sbret();
        }
        public BO.b06WorkflowStep Load(int pid)
        {
            return _db.Load<BO.b06WorkflowStep>(GetSQL1(" WHERE a.b06ID=@pid"), new { pid = pid });
        }


        public IEnumerable<BO.b06WorkflowStep> GetList(BO.myQuery mq)
        {
            if (mq.explicit_orderby == null) { mq.explicit_orderby = "a.b02ID,a.b06Ordinary"; };
            DL.FinalSqlCommand fq = DL.basQuery.GetFinalSql(GetSQL1(), mq, _mother.CurrentUser);
            return _db.GetList<BO.b06WorkflowStep>(fq.FinalSql, fq.Parameters);
        }



        public int Save(BO.b06WorkflowStep rec, List<BO.b08WorkflowReceiverToStep> lisB08, List<BO.b11WorkflowMessageToStep> lisB11)
        {
            if (!ValidateBeforeSave(rec, lisB08,lisB11))
            {
                return 0;
            }
            var p = new DL.Params4Dapper();
            p.AddInt("pid", rec.pid);
            p.AddInt("b02ID", rec.b02ID, true);
            p.AddInt("b02ID_Target", rec.b02ID_Target, true);
            p.AddString("b06Name", rec.b06Name);
            p.AddEnumInt("b06AutoRunFlag", rec.b06AutoRunFlag);

            p.AddInt("b06Ordinary", rec.b06Ordinary);

            p.AddEnumInt("b06NotepadFlag", rec.b06NotepadFlag);
            p.AddEnumInt("b06GeoFlag", rec.b06GeoFlag);

            p.AddEnumInt("b06NomineeFlag", rec.b06NomineeFlag,true);            
            p.AddInt("x67ID_Nominee", rec.x67ID_Nominee, true);
            p.AddInt("x67ID_Direct", rec.x67ID_Direct,true);
            p.AddInt("j11ID_Direct", rec.j11ID_Direct, true);
            p.AddInt("b02ID_LastReceiver_ReturnTo", rec.b02ID_LastReceiver_ReturnTo, true);
            //p.AddInt("x67ID_Direct_Source", rec.x67ID_Direct_Source, true);

            p.AddString("b06RunSQL", rec.b06RunSQL);
            p.AddString("b06ValidateAutoMoveSQL", rec.b06ValidateAutoMoveSQL);
            p.AddString("b06ValidateBeforeRunSQL", rec.b06ValidateBeforeRunSQL);
            p.AddString("b06ValidateBeforeErrorMessage", rec.b06ValidateBeforeErrorMessage);

            p.AddString("b06FrameworkSql", rec.b06FrameworkSql);
            p.AddInt("b06NextRunAfterHours", rec.b06NextRunAfterHours);
            p.AddString("b06CacheValueSql", rec.b06CacheValueSql);

            p.AddInt("p60ID", rec.p60ID, true);
            p.AddEnumInt("b06AutoTaskFlag", rec.b06AutoTaskFlag);
            p.AddInt("p83ID", rec.p83ID, true);

            p.AddEnumInt("b06JobFlag", rec.b06JobFlag);
            //p.AddBool("b06IsNotifyMerge", rec.b06IsNotifyMerge);
            //p.AddString("b06NotifyMergeTime",rec.b06NotifyMergeTime);

            int intPID = _db.SaveRecord("b06WorkflowStep", p, rec);

            if (intPID > 0)
            {
                if (lisB08 != null)
                {
                    if (rec.pid > 0)
                    {
                        _db.RunSql("DELETE FROM b08WorkflowReceiverToStep WHERE b06ID=@pid", new { pid = intPID });
                    }
                    foreach (var c in lisB08)
                    {
                        _db.RunSql("INSERT INTO b08WorkflowReceiverToStep(b06ID,x67ID,j11ID,j04ID,b08IsRecordOwner,b08IsRecordCreator) VALUES (@pid,@x67id,@j11id,@j04id,@b08IsRecordOwner,@b08IsRecordCreator)", new { pid = intPID, x67id = BO.Code.Bas.TestIntAsDbKey(c.x67ID), j11id = BO.Code.Bas.TestIntAsDbKey(c.j11ID), j04id = BO.Code.Bas.TestIntAsDbKey(c.j04ID), b08IsRecordOwner=c.b08IsRecordOwner, b08IsRecordCreator=c.b08IsRecordCreator });
                    }
                }
                if (lisB11 != null)
                {
                    if (rec.pid > 0)
                    {
                        _db.RunSql("DELETE FROM b11WorkflowMessageToStep WHERE b06ID=@pid", new { pid = intPID });
                    }
                    foreach (var c in lisB11)
                    {
                        _db.RunSql("INSERT INTO b11WorkflowMessageToStep(b06ID,j61ID,x67ID,j04ID,j11ID,b11IsRecordOwner,b11IsRecordCreator,b11IsRecordCreatorByEmail,b11Subject) VALUES (@pid,@j61id,@x67id,@j04id,@j11id,@b11IsRecordOwner,@b11IsRecordCreator,@b11IsRecordCreatorByEmail,@b11Subject)", new { pid = intPID, j61id = BO.Code.Bas.TestIntAsDbKey(c.j61ID), x67id = BO.Code.Bas.TestIntAsDbKey(c.x67ID), j04id = BO.Code.Bas.TestIntAsDbKey(c.j04ID), j11id = BO.Code.Bas.TestIntAsDbKey(c.j11ID), b11IsRecordOwner=c.b11IsRecordOwner, b11IsRecordCreator=c.b11IsRecordCreator, b11IsRecordCreatorByEmail=c.b11IsRecordCreatorByEmail, b11Subject=c.b11Subject });
                    }
                }

            }

            return intPID;
        }

        public bool ValidateBeforeSave(BO.b06WorkflowStep rec, List<BO.b08WorkflowReceiverToStep> lisB08, List<BO.b11WorkflowMessageToStep> lisB11)
        {
            if (string.IsNullOrEmpty(rec.b06Name) && rec.b02ID_Target==0)
            {
                this.AddMessage("Chybí vyplnit [Název]."); return false;
            }
            var recB02 = _mother.b02WorkflowStatusBL.Load(rec.b02ID);
            if (recB02==null)
            {
                this.AddMessage("Chybí vazba na workflow stav."); return false;
            }
            

            if (rec.b06AutoRunFlag==BO.b06AutoRunFlagEnum.UzivatelskyKrok)
            {
                if (lisB08.Count() == 0)
                {
                    this.AddMessage("V nastavení kroku chybí určení, kdo provádí krok."); return false;
                }
                
            }

            if (rec.b06GeoFlag==BO.b06GeoFlagEnum.LoadFromCurrentUser && rec.b06AutoRunFlag != BO.b06AutoRunFlagEnum.UzivatelskyKrok)
            {
                this.AddMessage("Načítat polohu uživatele je možné pouze u uživatelského kroku."); return false;
            }

            if (lisB11 != null)
            {
                if (lisB11.Any(p => p.j04ID == 0 && p.j11ID == 0 && p.x67ID == 0 && !p.b11IsRecordCreator && !p.b11IsRecordOwner && !p.b11IsRecordCreatorByEmail))
                {
                    this.AddMessage("V rozpisu notifikace chybí vybrat příjemce zprávy."); return false;
                }
                if (lisB11.Any(p => p.j61ID == 0))
                {
                    this.AddMessage("V rozpisu notifikace chybí vybrat šablonu zprávy."); return false;
                }
            }

            if (rec.b06NomineeFlag == BO.b06NomineeFlagENum.Pevna)
            {
                if (rec.j11ID_Direct==0 && rec.b02ID_LastReceiver_ReturnTo == 0)
                {
                    this.AddMessage("Pro automatickou změnu obsazení role v záznamu chybí tým nebo poslední stav."); return false;
                }
            }

            return true;
        }


        public IEnumerable<BO.b08WorkflowReceiverToStep> GetListB08(int b06id)
        {
            sb("SELECT a.*,x67.x67Name,j11.j11Name,j04.j04Name,x67.x67Name,");
            sb(_db.GetSQL1_Ocas("b08", false, false, false));
            sb(" FROM b08WorkflowReceiverToStep a");
            sb(" LEFT OUTER JOIN x67EntityRole x67 ON a.x67ID=x67.x67ID LEFT OUTER JOIN j11Team j11 ON a.j11ID=j11.j11ID LEFT OUTER JOIN j04UserRole j04 ON a.j04ID=j04.j04ID");
            sb(" WHERE a.b06ID=@b06id");

            return _db.GetList<BO.b08WorkflowReceiverToStep>(sbret(), new { b06id = b06id });
        }
        public IEnumerable<BO.b11WorkflowMessageToStep> GetListB11(int b06id)
        {
            sb("SELECT a.*,j61.j61Name,x67.x67Name,j04.j04Name,j11.j11Name,");
            sb(_db.GetSQL1_Ocas("b11", false, false, false));
            sb(" FROM b11WorkflowMessageToStep a INNER JOIN j61TextTemplate j61 ON a.j61ID=j61.j61ID");
            sb(" LEFT OUTER JOIN x67EntityRole x67 ON a.x67ID=x67.x67ID LEFT OUTER JOIN j04UserRole j04 ON a.j04ID=j04.j04ID LEFT OUTER JOIN j11Team j11 ON a.j11ID=j11.j11ID");
            sb(" WHERE a.b06ID=@b06id");

            return _db.GetList<BO.b11WorkflowMessageToStep>(sbret(), new { b06id = b06id });
        }
    }
}
