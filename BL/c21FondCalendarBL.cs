using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BL
{
    public interface Ic21FondCalendarBL
    {
        public BO.c21FondCalendar Load(int pid);
        public IEnumerable<BO.c21FondCalendar> GetList(BO.myQuery mq);
        public int Save(BO.c21FondCalendar rec, List<BO.c28FondCalendar_Log> lisC28);
        public double GetSumHours(int c21id,string countrycode,  DateTime d1, DateTime d2);
        public IEnumerable<BO.FondHours> GetSumHoursPerMonth(int c21id, string countrycode, DateTime d1, DateTime d2);
        public IEnumerable<BO.c22FondCalendar_Date> GetList_c22(int c21id, string countrycode, DateTime d1, DateTime d2);
        public IEnumerable<BO.c28FondCalendar_Log> GetList_c28(int c21id);

    }
    class c21FondCalendarBL : BaseBL, Ic21FondCalendarBL
    {
        public c21FondCalendarBL(BL.Factory mother) : base(mother)
        {

        }


        private string GetSQL1(string strAppend = null)
        {
            sb("SELECT a.*,");
            sb(_db.GetSQL1_Ocas("c21"));
            sb(" FROM c21FondCalendar a");
            sb(strAppend);
            return sbret();
        }
        public BO.c21FondCalendar Load(int pid)
        {
            return _db.Load<BO.c21FondCalendar>(GetSQL1(" WHERE a.c21ID=@pid"), new { pid = pid });
        }

        public IEnumerable<BO.c21FondCalendar> GetList(BO.myQuery mq)
        {
            DL.FinalSqlCommand fq = DL.basQuery.GetFinalSql(GetSQL1(), mq, _mother.CurrentUser);
            return _db.GetList<BO.c21FondCalendar>(fq.FinalSql, fq.Parameters);
        }


      

        public int Save(BO.c21FondCalendar rec,List<BO.c28FondCalendar_Log> lisC28)
        {
            if (!ValidateBeforeSave(rec,lisC28))
            {
                return 0;
            }
            var p = new DL.Params4Dapper();
            p.AddInt("pid", rec.pid);
            p.AddInt("x01ID", rec.x01ID == 0 ? _mother.CurrentUser.x01ID : rec.x01ID, true);
            p.AddString("c21Name", rec.c21Name);
            p.AddInt("c21Ordinary", rec.c21Ordinary);
            p.AddEnumInt("c21ScopeFlag", rec.c21ScopeFlag);
            p.AddDouble("c21Day1_Hours", rec.c21Day1_Hours);
            p.AddDouble("c21Day2_Hours", rec.c21Day2_Hours);
            p.AddDouble("c21Day3_Hours", rec.c21Day3_Hours);
            p.AddDouble("c21Day4_Hours", rec.c21Day4_Hours);
            p.AddDouble("c21Day5_Hours", rec.c21Day5_Hours);
            p.AddDouble("c21Day6_Hours", rec.c21Day6_Hours);
            p.AddDouble("c21Day7_Hours", rec.c21Day7_Hours);


            int intPID = _db.SaveRecord("c21FondCalendar", p, rec);
            if (intPID > 0)
            {
              if (lisC28 != null)
                {
                    _db.RunSql("DELETE FROM c28FondCalendar_Log WHERE c21ID=@pid", new { pid = intPID });
                    foreach(var c in lisC28)
                    {
                        _db.RunSql("INSERT INTO c28FondCalendar_Log(c21ID,c21ID_Log,c28ValidFrom,c28ValidUntil) VALUES(@pid,@c21id_log,@d1,@d2)", new { pid = intPID, c21id_log=c.c21ID_Log,d1=c.ValidFrom,d2=c.ValidUntil });
                    }
                    
                }
                _db.RunSql("exec dbo.c21_aftersave @c21id,@j02id_sys", new { c21id = intPID, j02id_sys = _mother.CurrentUser.pid });

            }
            return intPID;
        }
        private bool ValidateBeforeSave(BO.c21FondCalendar rec, List<BO.c28FondCalendar_Log> lisC28)
        {
            if (string.IsNullOrEmpty(rec.c21Name))
            {
                this.AddMessage("Chybí vyplnit [Název]."); return false;
            }

            if (lisC28 != null)
            {
                if (lisC28.Any(p => p.c21ID_Log == 0 || p.ValidFrom==null || p.ValidUntil==null))
                {
                    this.AddMessageTranslated("V časové historii nejsou vyplněné všechny údaje."); return false;
                }

                if (lisC28.Any(p => p.c21ID_Log==rec.pid))
                {
                    this.AddMessageTranslated("V časové historii je neplatný odkaz na fond."); return false;
                }
                if (lisC28.Any(p => p.ValidUntil > DateTime.Today || p.ValidFrom > DateTime.Today))
                {
                    this.AddMessageTranslated("V časové historii musí být [Platnost do] i [Platnost od] menší než aktuální datum.");return false;
                }
                if (lisC28.Any(p => p.ValidUntil < p.ValidFrom))
                {
                    this.AddMessageTranslated("V časové historii fondu nesmí být platnost od větší než platnost do."); return false;
                }
            }
            return true;
        }


        public double GetSumHours(int c21id,string countrycode,DateTime d1,DateTime d2)
        {
            if (string.IsNullOrEmpty(countrycode)) countrycode = _mother.CurrentUser.x01CountryCode;
            var ret = _db.Load<BO.GetDouble>("SELECT sum(c22Hours_Work) as Value FROM c22FondCalendar_Date WHERE c21ID=@c21id AND isnull(c22CountryCode,@defcountrycode)=@countrycode AND c22Date BETWEEN @d1 AND @d2", new { c21id = c21id,countrycode=countrycode, defcountrycode=_mother.CurrentUser.x01CountryCode, d1 = d1, d2 = d2 });
            if (ret != null)
            {
                return ret.Value;
            }
            else
            {
                return 0;
            }
        }


        public IEnumerable<BO.FondHours> GetSumHoursPerMonth(int c21id, string countrycode, DateTime d1, DateTime d2)
        {
            if (string.IsNullOrEmpty(countrycode)) countrycode = _mother.CurrentUser.x01CountryCode;
            string s = "SELECT sum(c22Hours_Work) as Hodiny,year(c22Date) as Rok,month(c22Date) as Mesic FROM c22FondCalendar_Date WHERE c21ID=@c21id AND isnull(c22CountryCode,@defcountrycode)=@countrycode AND c22Date BETWEEN @d1 AND @d2 GROUP BY year(c22Date),month(c22Date) ORDER BY year(c22Date),month(c22Date)";
            return _db.GetList<BO.FondHours>(s, new { c21id = c21id, countrycode = countrycode, defcountrycode = _mother.CurrentUser.x01CountryCode, d1 = d1, d2 = d2 });
        }

        public IEnumerable<BO.c22FondCalendar_Date> GetList_c22(int c21id, string countrycode, DateTime d1, DateTime d2)
        {
            if (string.IsNullOrEmpty(countrycode)) countrycode = _mother.CurrentUser.x01CountryCode;
            string s = "select * from c22FondCalendar_Date WHERE c21ID=@c21id AND isnull(c22CountryCode,@defcountrycode)=@countrycode AND c22Date BETWEEN @d1 AND @d2";
            return _db.GetList<BO.c22FondCalendar_Date>(s, new { c21id = c21id, countrycode = countrycode, defcountrycode = _mother.CurrentUser.x01CountryCode, d1 = d1, d2 = d2 });
        }



        public IEnumerable<BO.c28FondCalendar_Log> GetList_c28(int c21id)
        {
            sb("select a.*,a.c28ValidFrom as ValidFrom,a.c28ValidUntil as ValidUntil");
            sb(" FROM c28FondCalendar_Log a");
            sb(" WHERE a.c21ID=@c21id");
            return _db.GetList<BO.c28FondCalendar_Log>(sbret(), new { c21id = c21id });
        }

    }
}
