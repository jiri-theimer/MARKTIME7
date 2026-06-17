using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BL
{
    public interface Ip11AttendanceBL
    {
        public BO.p11Attendance Load(int pid);
        public IEnumerable<BO.p11Attendance> GetList(BO.myQuery mq);
        public int Save(BO.p11Attendance rec);
        public IEnumerable<BO.p11Attendance> GetList(DateTime d1, DateTime d2, int j02id, bool bolIncludeReportCols);

    }
    class p11AttendanceBL : BaseBL, Ip11AttendanceBL
    {
        public p11AttendanceBL(BL.Factory mother) : base(mother)
        {

        }


        private string GetSQL1(string strAppendWhere = null,bool bolIncludeReportCols=false)
        {
            sb("SELECT a.*,convert(char(5),a.p11TodayStart,114) as Prichod,convert(char(5),a.p11TodayEnd,114) as Odchod,");            
            sb(_db.GetSQL1_Ocas("p11"));
            if (bolIncludeReportCols)
            {
                sb(",dbo.p31_get_dochazka_prestavky_inline(a.j02ID,a.p11Date) as Prestavky_Inline");
                sb(",p31.Hodiny_Prestavka");
                sb(",convert(float,DATEDIFF(MINUTE,p11TodayStart,p11TodayEnd))/60.00 as Hodiny_VPraciVcPrestavek");
                sb(",(-1*isnull(p31.Hodiny_Prestavka,0))+(convert(float,DATEDIFF(MINUTE,a.p11TodayStart,a.p11TodayEnd))/60.00) as Hodiny_VPraci");
            }
            sb(" FROM p11Attendance a");
            if (_mother.CurrentUser.IsHostingModeTotalCloud)
            {
                sb(" INNER JOIN j02User j02x ON a.j02ID=j02x.j02ID INNER JOIN j04UserRole j04x ON j02x.j04ID=j04x.j04ID INNER JOIN x67EntityRole x67x ON j04x.x67ID=x67x.x67ID");
            }
            if (bolIncludeReportCols)
            {
                if (_mother.CurrentUser.IsHostingModeTotalCloud)
                {
                    sb($" LEFT OUTER JOIN (select j02ID,p31Date,sum(xa.p31Hours_Orig) as Hodiny_Vykazane,sum(case when xb.p32AbsenceBreakFlag>0 then xa.p31Hours_Orig end) as Hodiny_Prestavka,sum(case when isnull(xb.p32AbsenceFlag,0)>0 and isnull(xb.p32AbsenceBreakFlag,0)=0 then xa.p31Hours_Orig end) as Hodiny_Nepritomnost FROM p31Worksheet xa INNER JOIN p32Activity xb ON xa.p32ID=xb.p32ID INNER JOIN p34ActivityGroup xc ON xb.p34ID=xc.p34ID WHERE xc.x01ID={_mother.CurrentUser.x01ID} GROUP BY xa.j02ID,xa.p31Date) p31 ON a.j02ID=p31.j02ID AND a.p11Date=p31.p31Date");
                }
                else
                {
                    sb(" LEFT OUTER JOIN (select j02ID,p31Date,sum(xa.p31Hours_Orig) as Hodiny_Vykazane,sum(case when xb.p32AbsenceBreakFlag>0 then xa.p31Hours_Orig end) as Hodiny_Prestavka,sum(case when isnull(xb.p32AbsenceFlag,0)>0 and isnull(xb.p32AbsenceBreakFlag,0)=0 then xa.p31Hours_Orig end) as Hodiny_Nepritomnost FROM p31Worksheet xa INNER JOIN p32Activity xb ON xa.p32ID=xb.p32ID GROUP BY xa.j02ID,xa.p31Date) p31 ON a.j02ID=p31.j02ID AND a.p11Date=p31.p31Date");
                }
                
                
            }
            sb(strAppendWhere);
            return sbret();
        }
        public BO.p11Attendance Load(int pid)
        {
            return _db.Load<BO.p11Attendance>(GetSQL1(" WHERE a.p11ID=@pid"), new { pid = pid });
        }

        public IEnumerable<BO.p11Attendance> GetList(BO.myQuery mq)
        {
            DL.FinalSqlCommand fq = DL.basQuery.GetFinalSql(GetSQL1(), mq, _mother.CurrentUser);
            return _db.GetList<BO.p11Attendance>(fq.FinalSql, fq.Parameters);
        }
        public IEnumerable<BO.p11Attendance> GetList(DateTime d1,DateTime d2,int j02id,bool bolIncludeReportCols)
        {
            var mq = new BO.myQuery("p11");
            string s = $" WHERE a.j02ID={j02id} AND a.p11Date BETWEEN {BO.Code.Bas.GD(d1)} AND {BO.Code.Bas.GD(d2)}";
            if (d1 == d2)
            {
                s = $" WHERE a.j02ID={j02id} AND a.p11Date = {BO.Code.Bas.GD(d1)}";
            }
            
            DL.FinalSqlCommand fq = DL.basQuery.GetFinalSql(GetSQL1(s, bolIncludeReportCols), mq, _mother.CurrentUser);
            return _db.GetList<BO.p11Attendance>(fq.FinalSql, fq.Parameters);
        }


        public int Save(BO.p11Attendance rec)
        {
            if (!ValidateBeforeSave(rec))
            {
                return 0;
            }
            if (rec.pid == 0)
            {
                var lis = GetList(Convert.ToDateTime(rec.p11Date), Convert.ToDateTime(rec.p11Date), rec.j02ID,false);
                if (lis.Count() > 0) rec.pid = lis.First().pid;
            }
            var p = new DL.Params4Dapper();
            p.AddInt("pid", rec.pid);
            p.AddInt("j02ID", rec.j02ID, true);
            p.AddDateTime("p11Date", rec.p11Date);
            p.AddDateTime("p11TodayStart", rec.p11TodayStart);
            p.AddDateTime("p11TodayEnd", rec.p11TodayEnd);
            

            return _db.SaveRecord("p11Attendance", p, rec);
        }
        private bool ValidateBeforeSave(BO.p11Attendance rec)
        {
            if (rec.j02ID==0)
            {
                this.AddMessage("Chybí vyplnit [Uživatel]."); return false;
            }
            if (rec.p11Date==null)
            {
                this.AddMessageTranslated("p11Date missing."); return false;
            }
            if (rec.p11TodayEnd !=null && rec.p11TodayStart == null)
            {
                this.AddMessage("Chybí vyplnit [Příchod]."); return false;
            }
            if (rec.p11TodayEnd != null && rec.p11TodayStart != null && Convert.ToDateTime(rec.p11TodayStart)>=Convert.ToDateTime(rec.p11TodayEnd))
            {
                this.AddMessage("Čas příchodu musí být menší než čas odchodu."); return false;
            }
            return true;
        }

    }
}
