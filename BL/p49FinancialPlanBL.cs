

namespace BL
{
    public interface Ip49FinancialPlanBL
    {
        public BO.p49FinancialPlan Load(int pid);
        public IEnumerable<BO.p49FinancialPlan> GetList(BO.myQueryP49 mq);
        public int Save(BO.p49FinancialPlan rec);

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
