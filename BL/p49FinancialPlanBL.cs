

namespace BL
{
    public interface Ip49FinancialPlanBL
    {
        public BO.p49FinancialPlan Load(int pid);
        public IEnumerable<BO.p49FinancialPlan> GetList(BO.myQueryP49 mq);
        public int Save(BO.p49FinancialPlan rec);
        public bool CanEditBudget(int p41id);
        public IEnumerable<BO.p49BudgetActual> GetBudgetActuals(int p41id);
        public IEnumerable<BO.p49BudgetActivity> GetBudgetActivities();
        public IEnumerable<BO.p49BudgetProject> GetBudgetOverview();

    }
    class p49FinancialPlanBL : BaseBL, Ip49FinancialPlanBL
    {
        public p49FinancialPlanBL(BL.Factory mother) : base(mother)
        {

        }


        private string GetSQL1(string strAppend = null)
        {
            sb("SELECT a.*,");
            sb("p32x.p32Name,p34x.p34Name,j02x.j02Name,j27x.j27Code,isnull(p41x.p41NameShort,p41x.p41Name) as Project,p28x.p28Name as Client,p34x.p33ID,p56x.p56Name,");
            sb("case when p34x.p34IncomeStatementFlag=2 then 'P' else 'V' end as strana,");
            sb(_db.GetSQL1_Ocas("p49"));            
            sb(" FROM p49FinancialPlan a INNER JOIN p34ActivityGroup p34x ON a.p34ID=p34x.p34ID INNER JOIN p41Project p41x ON a.p41ID=p41x.p41ID");
            sb(" INNER JOIN p32Activity p32x ON a.p32ID=p32x.p32ID INNER JOIN j02User j02x ON a.j02ID=j02x.j02ID");
            sb(" INNER JOIN j27Currency j27x ON a.j27ID=j27x.j27ID");
            sb(" LEFT OUTER JOIN p56Task p56x ON a.p56ID=p56x.p56ID LEFT OUTER JOIN p28Contact p28x ON p41x.p28ID_Client=p28x.p28ID");
            sb(strAppend);
            return sbret();
        }
        public BO.p49FinancialPlan Load(int pid)
        {
            return _db.Load<BO.p49FinancialPlan>(GetSQL1(" WHERE a.p49ID=@pid"), new { pid = pid });
        }

        public IEnumerable<BO.p49FinancialPlan> GetList(BO.myQueryP49 mq)
        {
            DL.FinalSqlCommand fq = DL.basQuery.GetFinalSql(GetSQL1(), mq, _mother.CurrentUser);
            return _db.GetList<BO.p49FinancialPlan>(fq.FinalSql, fq.Parameters);
        }



        public int Save(BO.p49FinancialPlan rec)
        {
            if (!ValidateBeforeSave(rec))
            {
                return 0;
            }
            var p = new DL.Params4Dapper();
            p.AddInt("pid", rec.pid);
            p.AddInt("p41ID", rec.p41ID, true);
            p.AddInt("p34ID", rec.p34ID, true);
            p.AddInt("p32ID", rec.p32ID, true);
            p.AddInt("j02ID", rec.j02ID, true);
            p.AddInt("j27ID", rec.j27ID, true);
            p.AddInt("p56ID", rec.p56ID, true);
            p.AddInt("p28ID_Supplier", rec.p28ID_Supplier, true);

            p.AddDateTime("p49Date", rec.p49Date);
            p.AddString("p49Text", rec.p49Text);
            p.AddDouble("p49Amount", rec.p49Amount);
            p.AddDouble("p49PieceAmount", rec.p49PieceAmount);
            p.AddDouble("p49Pieces", rec.p49Pieces);
            p.AddDouble("p49MarginHidden", rec.p49MarginHidden);
            p.AddDouble("p49MarginTransparent", rec.p49MarginTransparent);

            p.AddInt("p49StatusFlag", rec.p49StatusFlag);

            int intPID = _db.SaveRecord("p49FinancialPlan", p, rec);
            if (intPID > 0)
            {
                var pars = new Dapper.DynamicParameters();
                {
                    pars.Add("p49id", intPID, System.Data.DbType.Int32);
                    pars.Add("j02id_sys", _mother.CurrentUser.pid, System.Data.DbType.Int32);
                }
                _db.RunSp("p49_aftersave", ref pars, false);
            }
            return intPID;
        }
        //rozpočet smí měnit ten, kdo má modul finančních plánů a vlastnické oprávnění k projektu
        public bool CanEditBudget(int p41id)
        {
            if (!_mother.CurrentUser.j04IsModule_p49 || p41id == 0) return false;
            var disp = _mother.p41ProjectBL.InhaleRecDisposition(p41id);
            return disp != null && disp.OwnerAccess;
        }

        //skutečnost rozpočtu: úkony projektu v peněžních sešitech (p33ID 2 a 5; hodiny a kusovník do rozpočtu nepatří), ať vznikly z plánu nebo ručně
        public IEnumerable<BO.p49BudgetActual> GetBudgetActuals(int p41id)
        {
            sb("SELECT a.p32ID,p32x.p32Name,p34x.p34ID,p34x.p34Name,p34x.p34Ordinary");
            sb(",case when p34x.p34IncomeStatementFlag=2 then 2 else 1 end as Strana");
            sb(",isnull(a.j27ID_Billing_Orig,0) as j27ID,j27x.j27Code,year(a.p31Date) as Rok,month(a.p31Date) as Mesic,sum(isnull(a.p31Amount_WithoutVat_Orig,0)) as Amount");
            sb(" FROM p31Worksheet a INNER JOIN p32Activity p32x ON a.p32ID=p32x.p32ID INNER JOIN p34ActivityGroup p34x ON p32x.p34ID=p34x.p34ID");
            sb(" LEFT OUTER JOIN j27Currency j27x ON a.j27ID_Billing_Orig=j27x.j27ID");
            sb(" WHERE a.p41ID=@p41id AND p34x.p33ID IN (2,5) AND a.p31Date IS NOT NULL AND GETDATE() BETWEEN a.p31ValidFrom AND a.p31ValidUntil");
            sb(" GROUP BY a.p32ID,p32x.p32Name,p34x.p34ID,p34x.p34Name,p34x.p34Ordinary,p34x.p34IncomeStatementFlag,a.j27ID_Billing_Orig,j27x.j27Code,year(a.p31Date),month(a.p31Date)");
            return _db.GetList<BO.p49BudgetActual>(sbret(), new { p41id = p41id });
        }

        public IEnumerable<BO.p49BudgetActivity> GetBudgetActivities()
        {
            sb("SELECT a.p32ID,a.p32Name,a.p32Ordinary,p34x.p34ID,p34x.p34Name,p34x.p34Ordinary,case when p34x.p34IncomeStatementFlag=2 then 2 else 1 end as Strana");
            sb(" FROM p32Activity a INNER JOIN p34ActivityGroup p34x ON a.p34ID=p34x.p34ID");
            sb(" WHERE p34x.x01ID=@x01id AND p34x.p33ID IN (2,5) AND GETDATE() BETWEEN a.p32ValidFrom AND a.p32ValidUntil AND GETDATE() BETWEEN p34x.p34ValidFrom AND p34x.p34ValidUntil");
            sb(" ORDER BY p34x.p34Ordinary,p34x.p34Name,a.p32Ordinary,a.p32Name");
            return _db.GetList<BO.p49BudgetActivity>(sbret(), new { x01id = _mother.CurrentUser.x01ID });
        }

        //přehled: projekty licence, které mají rozpočet; plán i skutečnost za celou dobu projektu, po měnách
        public IEnumerable<BO.p49BudgetProject> GetBudgetOverview()
        {
            sb("SELECT q.p41ID,isnull(p41x.p41NameShort,p41x.p41Name) as Project,p41x.p41Code,p28x.p28Name as Client,j27x.j27Code");
            sb(",sum(q.PlanVydaje) as PlanVydaje,sum(q.PlanPrijmy) as PlanPrijmy,sum(q.SkutVydaje) as SkutVydaje,sum(q.SkutPrijmy) as SkutPrijmy");
            sb(" FROM (");
            sb(" SELECT a.p41ID,a.j27ID");
            sb(" ,sum(case when p34x.p34IncomeStatementFlag=2 then 0 else a.p49Amount end) as PlanVydaje,sum(case when p34x.p34IncomeStatementFlag=2 then a.p49Amount else 0 end) as PlanPrijmy,0 as SkutVydaje,0 as SkutPrijmy");
            sb(" FROM p49FinancialPlan a INNER JOIN p34ActivityGroup p34x ON a.p34ID=p34x.p34ID");
            sb(" WHERE GETDATE() BETWEEN a.p49ValidFrom AND a.p49ValidUntil");
            sb(" GROUP BY a.p41ID,a.j27ID");
            sb(" UNION ALL");
            sb(" SELECT w.p41ID,w.j27ID_Billing_Orig,0,0");
            sb(" ,sum(case when p34x.p34IncomeStatementFlag=2 then 0 else isnull(w.p31Amount_WithoutVat_Orig,0) end),sum(case when p34x.p34IncomeStatementFlag=2 then isnull(w.p31Amount_WithoutVat_Orig,0) else 0 end)");
            sb(" FROM p31Worksheet w INNER JOIN p32Activity p32x ON w.p32ID=p32x.p32ID INNER JOIN p34ActivityGroup p34x ON p32x.p34ID=p34x.p34ID");
            sb(" WHERE p34x.p33ID IN (2,5) AND GETDATE() BETWEEN w.p31ValidFrom AND w.p31ValidUntil");
            sb(" AND w.p41ID IN (select p41ID FROM p49FinancialPlan WHERE GETDATE() BETWEEN p49ValidFrom AND p49ValidUntil)");
            sb(" GROUP BY w.p41ID,w.j27ID_Billing_Orig");
            sb(" ) q INNER JOIN p41Project p41x ON q.p41ID=p41x.p41ID INNER JOIN p07ProjectLevel p07x ON p41x.p07ID=p07x.p07ID");
            sb(" LEFT OUTER JOIN p28Contact p28x ON p41x.p28ID_Client=p28x.p28ID LEFT OUTER JOIN j27Currency j27x ON q.j27ID=j27x.j27ID");
            sb(" WHERE p07x.x01ID=@x01id AND GETDATE() BETWEEN p41x.p41ValidFrom AND p41x.p41ValidUntil");
            sb(" GROUP BY q.p41ID,isnull(p41x.p41NameShort,p41x.p41Name),p41x.p41Code,p28x.p28Name,j27x.j27Code");
            sb(" ORDER BY isnull(p41x.p41NameShort,p41x.p41Name),j27x.j27Code");
            return _db.GetList<BO.p49BudgetProject>(sbret(), new { x01id = _mother.CurrentUser.x01ID });
        }

        private bool ValidateBeforeSave(BO.p49FinancialPlan rec)
        {
            if (rec.p49Date == null)
            {
                this.AddMessage("Chybí vyplnit [Datum]."); return false;
            }
            if (rec.p49Amount == 0)
            {
                this.AddMessage("Chybí vyplnit [Částka]."); return false;
            }
            if (rec.p41ID==0)
            {
                this.AddMessage("Chybí vyplnit [Projekt]."); return false;
            }
            if (rec.p34ID==0)
            {
                this.AddMessage("Chybí vyplnit [Sešit]."); return false;
            }
            if (rec.p32ID == 0)
            {
                this.AddMessage("Chybí vyplnit [Aktivita]."); return false;
            }
            if (rec.j27ID == 0)
            {
                this.AddMessage("Chybí vyplnit [Měna]."); return false;
            }
            return true;
        }

    }
}
