

namespace BL
{
    public interface Ip42ProjectTypeBL
    {
        public BO.p42ProjectType Load(int pid);
        public IEnumerable<BO.p42ProjectType> GetList(BO.myQueryP42 mq);
        public IEnumerable<BO.p42ProjectType> GetList_ProjectCreate();   //seznam typů pro které může uživatel zakládat nové projekty
        public int Save(BO.p42ProjectType rec, List<BO.p43ProjectType_Workload> lisP43, List<BO.j08CreatePermission> lisJ08);
        public IEnumerable<BO.p43ProjectType_Workload> GetList_p43(int p42id,int p34id=0);

    }
    class p42ProjectTypeBL : BaseBL, Ip42ProjectTypeBL
    {

        public p42ProjectTypeBL(BL.Factory mother) : base(mother)
        {

        }

        private string GetSQL1(string strAppend = null)
        {
            sb("SELECT a.*,b01x.b01Name,x38x.x38Name," + _db.GetSQL1_Ocas("p42") + " FROM p42ProjectType a");            
            
            sb(" LEFT OUTER JOIN b01WorkflowTemplate b01x ON a.b01ID=b01x.b01ID");
            sb(" LEFT OUTER JOIN x38CodeLogic x38x ON a.x38ID=x38x.x38ID");
            sb(strAppend);
            return sbret();
        }

        public BO.p42ProjectType Load(int pid)
        {
            return _db.Load<BO.p42ProjectType>(GetSQL1(" WHERE a.p42ID=@pid"), new { pid = pid });
        }
        public IEnumerable<BO.p42ProjectType> GetList(BO.myQueryP42 mq)
        {
            if (mq.explicit_orderby == null) mq.explicit_orderby = "a.p42Ordinary";
            DL.FinalSqlCommand fq = DL.basQuery.GetFinalSql(GetSQL1(), mq, _mother.CurrentUser);
            return _db.GetList<BO.p42ProjectType>(fq.FinalSql, fq.Parameters);
        }
        public IEnumerable<BO.p42ProjectType> GetList_ProjectCreate()
        {
            if (_mother.CurrentUser.TestPermission(BO.PermValEnum.GR_p41_Creator))
            {
                return GetList(new BO.myQueryP42());  //všechny typy
            }
            string s = GetSQL1(" WHERE a.p42ID IN (select j08RecordPid FROM j08CreatePermission WHERE j08RecordEntity='p42' AND (j08IsAllUsers=1 OR j02ID=@j02id OR j04ID=@j04id");
            if (_mother.CurrentUser.j11IDs != null)
            {
                s += " OR j11ID IN (" + _mother.CurrentUser.j11IDs + ")";
            }
            s += "))";
            
            if (_mother.CurrentUser.IsHostingModeTotalCloud)
            {
                s += $" AND a.x01ID={_mother.CurrentUser.x01ID}";
            }

            return _db.GetList<BO.p42ProjectType>(s, new { j02id = _mother.CurrentUser.pid, j04id = _mother.CurrentUser.j04ID });
        }

        public int Save(BO.p42ProjectType rec, List<BO.p43ProjectType_Workload> lisP43, List<BO.j08CreatePermission> lisJ08)
        {
            if (!ValidateBeforeSave(rec))
            {
                return 0;
            }
            using (var sc = new System.Transactions.TransactionScope())
            {
                var p = new DL.Params4Dapper();

                p.AddInt("pid", rec.pid);                
                p.AddInt("b01ID", rec.b01ID, true);
                p.AddInt("x01ID", rec.x01ID == 0 ? _mother.CurrentUser.x01ID : rec.x01ID, true);
                p.AddInt("x38ID", rec.x38ID, true);
                p.AddInt("p61ID", rec.p61ID, true);

                p.AddEnumInt("p42ArchiveFlag", rec.p42ArchiveFlag);
                p.AddEnumInt("p42ArchiveFlagP31", rec.p42ArchiveFlagP31);
                p.AddString("p42Name", rec.p42Name);
                p.AddString("p42Code", rec.p42Code);
                p.AddInt("p42Ordinary", rec.p42Ordinary);
                
                p.AddByte("p42FilesTab", rec.p42FilesTab);
                p.AddByte("p42RolesTab", rec.p42RolesTab);
                p.AddByte("p42BillingTab", rec.p42BillingTab);
                p.AddByte("p42BudgetTab", rec.p42BudgetTab);
                p.AddByte("p42ClientTab", rec.p42ClientTab);
                p.AddByte("p42ContactsTab", rec.p42ContactsTab);
                p.AddBool("p42IsP54", rec.p42IsP54);
                p.AddByte("p42BillingFlag", rec.p42BillingFlag);

                int intPID = _db.SaveRecord("p42ProjectType", p, rec);
                if (intPID > 0 && lisP43 != null)
                {
                    if (rec.pid > 0)
                    {
                        _db.RunSql("DELETE FROM p43ProjectType_Workload WHERE p42ID=@pid", new { pid = intPID });
                    }
                    foreach(var c in lisP43)
                    {
                        _db.RunSql("INSERT INTO p43ProjectType_Workload(p42ID,p34ID) SELECT @pid,p34ID FROM p34ActivityGroup WHERE p34ID=@p34id", new { pid = intPID, p34id=c.p34ID });
                    }
                    
                    _db.RunSql("update p41Project set p41BillingFlag=@flag WHERE p42ID=@pid", new { pid = intPID,flag=rec.p42BillingFlag });
                    
                    if (lisJ08 != null)
                    {
                        DL.BAS.SaveJ08(_db, "p42", intPID, lisJ08);
                    }
                }
                sc.Complete();
                return intPID;
            }
                
        }


        private bool ValidateBeforeSave(BO.p42ProjectType rec)
        {

            if (string.IsNullOrEmpty(rec.p42Name))
            {
                this.AddMessage("Chybí vyplnit [Název]."); return false;
            }

            if (rec.x38ID==0)
            {
                this.AddMessage("Chybí vyplnit [Číselná řada]."); return false;
            }
            


            return true;
        }

        public IEnumerable<BO.p43ProjectType_Workload> GetList_p43(int p42id, int p34id = 0)
        {
            string s = "select *,p43ID as pid FROM p43ProjectType_Workload WHERE p42ID=@p42id";
            if (p34id > 0)
            {
                s += $" AND p34ID={p34id}";
            }
            return _db.GetList<BO.p43ProjectType_Workload>(s , new { p42id = p42id });
        }
    }
}
