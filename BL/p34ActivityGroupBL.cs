
namespace BL
{
    public interface Ip34ActivityGroupBL
    {
        public BO.p34ActivityGroup Load(int pid);
        public IEnumerable<BO.p34ActivityGroup> GetList(BO.myQueryP34 mq);
        public int Save(BO.p34ActivityGroup rec);
        public IEnumerable<BO.p34ActivityGroup> GetList_WorksheetEntryIn_OneProject(BO.p41Project recP41, int j02id);
        public IEnumerable<BO.p34ActivityGroup> GetList_WorksheetEntry_InAllProjects(int j02id);


    }
    class p34ActivityGroupBL : BaseBL, Ip34ActivityGroupBL
    {

        public p34ActivityGroupBL(BL.Factory mother) : base(mother)
        {

        }

        private string GetSQL1(string strAppend = null)
        {
            sb("SELECT a.*,p33.p33Name,p33.p33Code," + _db.GetSQL1_Ocas("p34"));
            sb(" FROM p34ActivityGroup a INNER JOIN p33ActivityInputType p33 ON a.p33ID=p33.p33ID");

            sb(strAppend);
            return sbret();
        }

        public BO.p34ActivityGroup Load(int pid)
        {
            return _db.Load<BO.p34ActivityGroup>(GetSQL1(" WHERE a.p34ID=@pid"), new { pid = pid });
        }
        public IEnumerable<BO.p34ActivityGroup> GetList(BO.myQueryP34 mq)
        {            
            if (mq.explicit_orderby == null) mq.explicit_orderby = "a.p34Ordinary,a.p34Name";
            
            DL.FinalSqlCommand fq = DL.basQuery.GetFinalSql(GetSQL1(), mq, _mother.CurrentUser);
            return _db.GetList<BO.p34ActivityGroup>(fq.FinalSql, fq.Parameters);
        }

        public IEnumerable<BO.p34ActivityGroup> GetList_WorksheetEntryIn_OneProject(BO.p41Project recP41, int j02id)    //nabídka sešitů pro vykazování ve vybraném projektu
        {
            return GetList(new BO.myQueryP34() { p41id = recP41.pid, p61id = (recP41.p61ID>0 ? recP41.p61ID : recP41.p61ID_Byp42ID) });

        }


        public IEnumerable<BO.p34ActivityGroup> GetList_WorksheetEntry_InAllProjects(int j02id)
        {
           
            var s = " WHERE getdate() BETWEEN a.p34ValidFrom AND a.p34ValidUntil";
            if (_mother.CurrentUser.IsHostingModeTotalCloud)
            {
                s += $" AND a.x01ID={_mother.CurrentUser.x01ID}";
            }

            if (_mother.CurrentUser.TestPermission(BO.PermValEnum.GR_P31_Creator_Hours) && _mother.CurrentUser.TestPermission(BO.PermValEnum.GR_P31_Creator_Expenses) && _mother.CurrentUser.TestPermission(BO.PermValEnum.GR_P31_Creator_Fees))
            {
                //právo vykazovat do všech projektů  
                s += " ORDER BY a.p34Ordinary,a.p34Name";
                return _db.GetList<BO.p34ActivityGroup>(GetSQL1(s));
            }

            s += " AND (1=1";
            if (_mother.CurrentUser.TestPermission(BO.PermValEnum.GR_P31_Creator_Hours))
            {
                //právo vykazovat hodiny a kusovník do všech projektů    
                s += " OR a.p33ID IN (1,3)";
            }
            if (_mother.CurrentUser.TestPermission(BO.PermValEnum.GR_P31_Creator_Expenses))
            {
                //právo vykazovat výdaje do všech projektů    
                s += " OR (a.p33ID IN (2,5) AND a.p34IncomeStatementFlag=1)";
            }
            if (_mother.CurrentUser.TestPermission(BO.PermValEnum.GR_P31_Creator_Fees))
            {
                //právo vykazovat odměny do všech projektů    
                s += " OR (a.p33ID IN (2,5) AND a.p34IncomeStatementFlag=2)";
            }

            s += ")";

            var strEntryFlags = "1,2";
            if (j02id != _mother.CurrentUser.pid)
            {
                strEntryFlags = "1,2,4";
            }


            s += " OR (getdate() BETWEEN a.p34ValidFrom AND a.p34ValidUntil AND a.p34ID IN (";
            s += "SELECT a1.p34ID FROM o28ProjectRole_Workload a1 INNER JOIN x69EntityRole_Assign a2 ON a1.x67ID=a2.x67ID INNER JOIN x67EntityRole a3 ON a2.x67ID=a3.x67ID";
            s += " WHERE a3.x67Entity='p41' AND a1.o28EntryFlag IN (" + strEntryFlags + ") AND (a2.j02ID=@j02id OR a2.j11ID IN (SELECT j11ID FROM j12Team_Person WHERE j02ID=@j02id))";
            s += ")";

            s += ")";

            s += " ORDER BY a.p34Ordinary,a.p34Name";

            return _db.GetList<BO.p34ActivityGroup>(GetSQL1(s), new { j02id = j02id });

        }



        public int Save(BO.p34ActivityGroup rec)
        {
            if (!ValidateBeforeSave(rec))
            {
                return 0;
            }
            using (var sc = new System.Transactions.TransactionScope())     //ukládání v transakci
            {
                var p = new DL.Params4Dapper();

                p.AddInt("pid", rec.pid);
                p.AddInt("x01ID", rec.x01ID == 0 ? _mother.CurrentUser.x01ID : rec.x01ID);                
                p.AddInt("p34Ordinary", rec.p34Ordinary);
                p.AddString("p34Name", rec.p34Name);
                p.AddString("p34Code", rec.p34Code);
                p.AddEnumInt("p33ID", rec.p33ID);
                p.AddEnumInt("p34ActivityEntryFlag", rec.p34ActivityEntryFlag);
                p.AddEnumInt("p34IncomeStatementFlag", rec.p34IncomeStatementFlag);
                p.AddByte("p34TextInternalFlag", rec.p34TextInternalFlag);

                
                p.AddByte("p34FilesFlag", rec.p34FilesFlag);                
                p.AddByte("p34TagsFlag", rec.p34TagsFlag);
                p.AddByte("p34InboxFlag", rec.p34InboxFlag);
                p.AddByte("p34TrimmingFlag", rec.p34TrimmingFlag);

                int intPID = _db.SaveRecord("p34ActivityGroup", p, rec);

                if (intPID > 0)
                {
                   
                    if (rec.pid == 0)   //nový sešit
                    {                        
                        _db.RunSql("INSERT INTO p43ProjectType_Workload(p42ID,p34ID) SELECT p42ID,@p34id FROM p42ProjectType WHERE getdate() BETWEEN p42ValidFrom AND p42ValidUntil", new { p34id = intPID });
                    }
                    sc.Complete();
                }


                return intPID;
            }

        }


        private bool ValidateBeforeSave(BO.p34ActivityGroup rec)
        {

            if (string.IsNullOrEmpty(rec.p34Name))
            {
                this.AddMessage("Chybí vyplnit [Název]."); return false;
            }
           
           


            return true;
        }
    }
}
