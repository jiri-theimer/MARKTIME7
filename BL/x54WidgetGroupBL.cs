
namespace BL
{
    public interface Ix54WidgetGroupBL
    {
        public BO.x54WidgetGroup Load(int pid);
        public BO.x54WidgetGroup LoadByCode(string code);
        public IEnumerable<BO.x54WidgetGroup> GetList(BO.myQuery mq);
        public int Save(BO.x54WidgetGroup rec,List<BO.x57WidgetToGroup> lisX57);
        public IEnumerable<BO.x57WidgetToGroup> GetList_x57(int x54id);

    }
    class x54WidgetGroupBL : BaseBL, Ix54WidgetGroupBL
    {
        public x54WidgetGroupBL(BL.Factory mother) : base(mother)
        {

        }


        private string GetSQL1(string strAppend = null)
        {
            sb("SELECT a.*,");
            sb(_db.GetSQL1_Ocas("x54"));
            sb(" FROM x54WidgetGroup a");
            sb(strAppend);
            return sbret();
        }
        public BO.x54WidgetGroup Load(int pid)
        {
            return _db.Load<BO.x54WidgetGroup>(GetSQL1(" WHERE a.x54ID=@pid"), new { pid = pid });
        }
        public BO.x54WidgetGroup LoadByCode(string code)
        {
            return _db.Load<BO.x54WidgetGroup>(GetSQL1(" WHERE a.x54Code LIKE @code AND a.x01ID=@x01id"), new {code=code, x01id = _mother.CurrentUser.x01ID });
        }

        public IEnumerable<BO.x54WidgetGroup> GetList(BO.myQuery mq)
        {
            if (mq.explicit_orderby == null) mq.explicit_orderby = "a.x54Ordinary,a.x54Name";
            DL.FinalSqlCommand fq = DL.basQuery.GetFinalSql(GetSQL1(), mq, _mother.CurrentUser);
            return _db.GetList<BO.x54WidgetGroup>(fq.FinalSql, fq.Parameters);
        }

        public IEnumerable<BO.x57WidgetToGroup> GetList_x57(int x54id)
        {
            sb("select a.*,x55.x55Name,x55.x55Name+isnull(' ('+x55.x55Category+')','') as NamePlusCategory,x55.x55Code");
            sb(" FROM x57WidgetToGroup a INNER JOIN x55Widget x55 ON a.x55ID=x55.x55ID");
            sb(" WHERE a.x54ID=@x54id");
            sb(" ORDER BY a.x57Ordinary,x55.x55Ordinal");
            return _db.GetList<BO.x57WidgetToGroup>(sbret(), new { x54id = x54id });
        }

        public int Save(BO.x54WidgetGroup rec, List<BO.x57WidgetToGroup> lisX57)
        {
            if (!ValidateBeforeSave(rec))
            {
                return 0;
            }
            var p = new DL.Params4Dapper();
            p.AddInt("pid", rec.pid);
            p.AddInt("x01ID", rec.x01ID == 0 ? _mother.CurrentUser.x01ID : rec.x01ID, true);
            p.AddString("x54Name", rec.x54Name);
            p.AddString("x54Code", rec.x54Code);
            p.AddInt("x54Ordinary", rec.x54Ordinary);
            p.AddBool("x54IsParamP28ID", rec.x54IsParamP28ID);
            p.AddBool("x54IsParamToday", rec.x54IsParamToday);
            p.AddBool("x54IsAllowAutoRefresh", rec.x54IsAllowAutoRefresh);
            p.AddBool("x54IsAllowSkins", rec.x54IsAllowSkins);

            int intPID = _db.SaveRecord("x54WidgetGroup", p, rec);
            if (intPID > 0 && lisX57 !=null)
            {
                if (rec.pid > 0)
                {
                    _db.RunSql("DELETE FROM x57WidgetToGroup WHERE x54ID=@pid", new { pid = intPID });
                }
                foreach (var c in lisX57.Where(p=>p.x55ID>0))
                {
                    _db.RunSql("INSERT INTO x57WidgetToGroup(x54ID,x55ID,x57IsDefault,x57Ordinary) VALUES(@pid,@x55id,@b,@poradi)", new { pid = intPID, x55id = c.x55ID, b = c.x57IsDefault,poradi= c.x57Ordinary });
                }
            }
            return intPID;
        }
        private bool ValidateBeforeSave(BO.x54WidgetGroup rec)
        {
            if (string.IsNullOrEmpty(rec.x54Name))
            {
                this.AddMessage("Chybí vyplnit [Název]."); return false;
            }
            
            return true;
        }

    }
}
