

namespace BL
{
    public interface Ip60TaskTemplateBL
    {
        public BO.p60TaskTemplate Load(int pid);
        public IEnumerable<BO.p60TaskTemplate> GetList(BO.myQuery mq);
        public int Save(BO.p60TaskTemplate rec, List<BO.x69EntityRole_Assign> lisX69);

    }
    class p60TaskTemplateBL : BaseBL, Ip60TaskTemplateBL
    {
        public p60TaskTemplateBL(BL.Factory mother) : base(mother)
        {

        }


        private string GetSQL1(string strAppend = null)
        {
            sb("SELECT a.*");
            sb(",j02owner.j02LastName+' '+j02owner.j02FirstName as Owner");
            sb("," + _db.GetSQL1_Ocas("p60"));
            sb(" FROM p60TaskTemplate a");
            sb(" INNER JOIN p57TaskType p57x ON a.p57ID=p57x.p57ID LEFT OUTER JOIN p41Project p41 ON a.p41ID=p41.p41ID");
            sb(" LEFT OUTER JOIN j02User j02owner ON a.j02ID_Owner=j02owner.j02ID");
            sb(strAppend);
            
            
            
            return sbret();
        }
        public BO.p60TaskTemplate Load(int pid)
        {
            return _db.Load<BO.p60TaskTemplate>(GetSQL1(" WHERE a.p60ID=@pid"), new { pid = pid });
        }

        public IEnumerable<BO.p60TaskTemplate> GetList(BO.myQuery mq)
        {
            DL.FinalSqlCommand fq = DL.basQuery.GetFinalSql(GetSQL1(), mq, _mother.CurrentUser);
            return _db.GetList<BO.p60TaskTemplate>(fq.FinalSql, fq.Parameters);
        }



        public int Save(BO.p60TaskTemplate rec, List<BO.x69EntityRole_Assign> lisX69)
        {
            if (!ValidateBeforeSave(rec))
            {
                return 0;
            }
            var p = new DL.Params4Dapper();
            p.AddInt("pid", rec.pid);
            p.AddInt("p57ID", rec.p57ID, true);
            p.AddInt("p41ID", rec.p41ID, true);
            p.AddInt("p55ID", rec.p55ID, true);
            p.AddInt("p15ID", rec.p15ID, true);
            if (rec.j02ID_Owner == 0) rec.j02ID_Owner = _mother.CurrentUser.pid;
            p.AddInt("j02ID_Owner", rec.j02ID_Owner, true);
            p.AddBool("p60IsPublic", rec.p60IsPublic);

            p.AddString("p60Name", rec.p60Name);

            p.AddString("p60Notepad", rec.p60Notepad);
            p.AddInt("x04ID", rec.x04ID, true);
            p.AddInt("p60Ordinary", rec.p60Ordinary);

            p.AddInt("p60PlanFrom_UC", rec.p60PlanFrom_UC);
            p.AddString("p60PlanFrom_Unit", rec.p60PlanFrom_Unit);
            p.AddInt("p60PlanUntil_UC", rec.p60PlanUntil_UC);
            p.AddString("p60PlanUntil_Unit", rec.p60PlanUntil_Unit);
            p.AddDouble("p60Plan_Hours", rec.p60Plan_Hours);
            p.AddDouble("p60Plan_Expenses", rec.p60Plan_Expenses);
            p.AddDouble("p60Plan_Revenue", rec.p60Plan_Revenue);
            p.AddDouble("p60Plan_Internal_Fee", rec.p60Plan_Internal_Fee);
            p.AddEnumInt("p60PlanFlag", rec.p60PlanFlag, true);

            p.AddBool("p60IsStopNotify", rec.p60IsStopNotify);


            int intPID = _db.SaveRecord("p60TaskTemplate", p, rec);
            if (intPID > 0)
            {
                if (lisX69 != null && !DL.BAS.SaveX69(_db, "p60", intPID, lisX69))
                {
                    this.AddMessageTranslated("Error: DL.BAS.SaveX69");
                    return 0;
                }
            }

            return intPID;
        }
        private bool ValidateBeforeSave(BO.p60TaskTemplate rec)
        {
            if (string.IsNullOrEmpty(rec.p60Name))
            {
                this.AddMessage("Chybí vyplnit [Název]."); return false;
            }
            if (rec.p57ID == 0)
            {
                this.AddMessage("Chybí [Typ úkolu]."); return false;
            }

            return true;
        }

    }
}
