

namespace BL
{
    public interface Ic24DayColorBL
    {
        public BO.c24DayColor Load(int pid);
        public IEnumerable<BO.c24DayColor> GetList(BO.myQuery mq);
        public int Save(BO.c24DayColor rec);
        public IEnumerable<BO.c23PersonalDayColor> GetList_c23(DateTime d1,DateTime d2,int j02id);
        public int SaveC23Record(DateTime d, int c24id, int j02id);

    }
    class c24DayColorBL : BaseBL, Ic24DayColorBL
    {
        public c24DayColorBL(BL.Factory mother) : base(mother)
        {

        }


        private string GetSQL1(string strAppend = null)
        {
            sb("SELECT a.*,");
            sb(_db.GetSQL1_Ocas("c24"));
            sb(" FROM c24DayColor a");
            sb(strAppend);
            return sbret();
        }
        public BO.c24DayColor Load(int pid)
        {
            return _db.Load<BO.c24DayColor>(GetSQL1(" WHERE a.c24ID=@pid"), new { pid = pid });
        }

        public IEnumerable<BO.c24DayColor> GetList(BO.myQuery mq)
        {
            DL.FinalSqlCommand fq = DL.basQuery.GetFinalSql(GetSQL1(), mq, _mother.CurrentUser);
            return _db.GetList<BO.c24DayColor>(fq.FinalSql, fq.Parameters);
        }



        public int Save(BO.c24DayColor rec)
        {
            if (!ValidateBeforeSave(rec))
            {
                return 0;
            }
            var p = new DL.Params4Dapper();
            p.AddInt("pid", rec.pid);
            p.AddString("c24Name", rec.c24Name);
            p.AddString("c24Color", rec.c24Color);

            p.AddInt("x01ID", rec.x01ID == 0 ? _mother.CurrentUser.x01ID : rec.x01ID, true);
           
            int intPID = _db.SaveRecord("c24DayColor", p, rec);
            
            return intPID;
        }
        private bool ValidateBeforeSave(BO.c24DayColor rec)
        {
            if (string.IsNullOrEmpty(rec.c24Name))
            {
                this.AddMessage("Chybí vyplnit [Název]."); return false;
            }
            if (string.IsNullOrEmpty(rec.c24Color))
            {
                this.AddMessage("Chybí vyplnit [Barva]."); return false;
            }

            return true;
        }

        public IEnumerable<BO.c23PersonalDayColor> GetList_c23(DateTime d1, DateTime d2, int j02id)
        {
            string s = "select a.*," + _db.GetSQL1_Ocas("c23", false, false) + ", c24.c24Name,c24.c24Color,j02.j02LastName+' '+j02.j02FirstName as Person FROM c23PersonalDayColor a INNER JOIN c24DayColor c24 ON a.c24ID=c24.c24ID INNER JOIN j02User j02 ON a.j02ID=j02.j02ID";
            s += " WHERE a.c23Date BETWEEN @d1 AND @d2";
            if (j02id > 0)
            {
                s += " AND a.j02ID = " + j02id.ToString();
            }
            if (_mother.CurrentUser.IsHostingModeTotalCloud)
            {
                sb($" AND c24.x01ID={_mother.CurrentUser.x01ID}");
            }

            return _db.GetList<BO.c23PersonalDayColor>(s, new { d1 = d1,d2=d2 });
        }

        public int SaveC23Record(DateTime d,int c24id, int j02id)
        {
            if (c24id == 0 && j02id == 0) return 0;
            if (c24id==0 && j02id > 0)
            {
                //vyčistit barvu dne
                _db.RunSql("DELETE FROM c23PersonalDayColor WHERE j02ID=@j02id AND c23Date=@d", new { j02id = j02id, d = d });
                return 0;
            }

            var lis = GetList_c23(d, d, j02id);
            if (lis.Any(p => p.c24ID == c24id))
            {
                return lis.First(p => p.c24ID == c24id).pid;
            }
            if (lis.Count() > 0)
            {
                _db.RunSql("DELETE FROM c23PersonalDayColor WHERE j02ID=@j02id AND c23Date=@d", new { j02id = j02id, d = d });
            }
            var rec = new BO.c23PersonalDayColor() { c23Date = d, c24ID = c24id, j02ID = j02id };
            var p = new DL.Params4Dapper();            
            p.AddDateTime("c23Date", d);
            p.AddInt("c24ID", c24id,true);
            p.AddInt("j02ID", j02id, true);


            int intPID = _db.SaveRecord("c23PersonalDayColor", p, rec);

            return intPID;
        }

    }
}
