using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BL
{
    public interface Ir01CapacityBL
    {
        public BO.r01Capacity Load(int pid);
        public IEnumerable<BO.r01Capacity> GetList(BO.myQueryR01 mq);
        public int Save(BO.r01Capacity rec);
        public IEnumerable<BO.r05CapacityUnit> GetList_r05(int r01id);
        public IEnumerable<BO.CapacityResourceGroupBy> GetList_GroupByJ02(BO.myQueryR01 mq, BO.CapacityGroupByEnum groupby);
        public IEnumerable<BO.CapacityProjectGroupBy> GetList_GroupByP41(BO.myQueryR01 mq, BO.CapacityGroupByEnum groupby);
        public bool ValidateBeforeSave(BO.r01Capacity rec);

    }
    class r01CapacityBL : BaseBL, Ir01CapacityBL
    {
        public r01CapacityBL(BL.Factory mother) : base(mother)
        {

        }


        private string GetSQL1(string strAppend = null)
        {
            sb("SELECT a.*,r02.r02Name,j02.j02FirstName+' '+j02.j02LastName as PersonAsc,j02.j02LastName+' '+j02.j02FirstName as Person,LEFT(j02.j02FirstName,1)+LEFT(j02.j02LastName,1) as Person_Inicialy,isnull(p41.p41NameShort,p41.p41Name) as Project,p41.p41PlanFrom,p41.p41PlanUntil,p28.p28Name as Client,");
            sb(_db.GetSQL1_Ocas("r01"));
            sb(" FROM r01Capacity a INNER JOIN r02CapacityVersion r02 ON a.r02ID=r02.r02ID INNER JOIN j02User j02 ON a.j02ID=j02.j02ID INNER JOIN p41Project p41 ON a.p41ID=p41.p41ID LEFT OUTER JOIN p28Contact p28 ON p41.p28ID_Client=p28.p28ID");
            sb(strAppend);
            return sbret();
        }
        public BO.r01Capacity Load(int pid)
        {
            return _db.Load<BO.r01Capacity>(GetSQL1(" WHERE a.r01ID=@pid"), new { pid = pid });
        }

        public IEnumerable<BO.r01Capacity> GetList(BO.myQueryR01 mq)
        {
            DL.FinalSqlCommand fq = DL.basQuery.GetFinalSql(GetSQL1(), mq, _mother.CurrentUser);
            return _db.GetList<BO.r01Capacity>(fq.FinalSql, fq.Parameters);
        }

        public IEnumerable<BO.r05CapacityUnit> GetList_r05(int r01id)
        {
            string s = "select a.* FROM r05CapacityUnit a WHERE a.r01ID=@r01id";

            return _db.GetList<BO.r05CapacityUnit>(s, new { r01id = r01id });
        }

        public IEnumerable<BO.CapacityResourceGroupBy> GetList_GroupByJ02(BO.myQueryR01 mq, BO.CapacityGroupByEnum groupby)
        {
            string s = "r05.r05Date";
            switch (groupby)
            {
                case BO.CapacityGroupByEnum.Week:
                    s = "r05.r05Week";
                    break;
                case BO.CapacityGroupByEnum.Month:
                    s = "r05.r05Month";
                    break;
                case BO.CapacityGroupByEnum.Year:
                    s = "r05.r05Year";
                    break;
            }
            sb($"SELECT min(a.j02ID) as j02ID,sum(r05.r05HoursFa) as HoursFa,sum(r05.r05HoursNeFa) as HoursNeFa,sum(r05.r05HoursTotal) as HoursTotal,{s} as D1,min(j02.j02LastName)+' '+min(j02.j02FirstName) as Person,COUNT(DISTINCT a.r01ID) as Krat");
            sb(" FROM r01Capacity a INNER JOIN r02CapacityVersion r02 ON a.r02ID=r02.r02ID INNER JOIN r05CapacityUnit r05 ON a.r01ID=r05.r01ID INNER JOIN j02User j02 ON a.j02ID=j02.j02ID");

           
            mq.explicit_sqlgroupby = $"a.j02ID, {s}";
            mq.explicit_orderby = "min(j02.j02LastName)";

            DL.FinalSqlCommand fq = DL.basQuery.GetFinalSql(sbret(), mq, _mother.CurrentUser);

            return _db.GetList<BO.CapacityResourceGroupBy>(fq.FinalSql, fq.Parameters);
        }
        public IEnumerable<BO.CapacityProjectGroupBy> GetList_GroupByP41(BO.myQueryR01 mq, BO.CapacityGroupByEnum groupby)
        {
            string s = "r05.r05Date";
            switch (groupby)
            {
                case BO.CapacityGroupByEnum.Week:
                    s = "r05.r05Week";
                    break;
                case BO.CapacityGroupByEnum.Month:
                    s = "r05.r05Month";
                    break;
                case BO.CapacityGroupByEnum.Year:
                    s = "r05.r05Year";
                    break;
            }
            sb($"SELECT min(a.p41ID) as p41ID,sum(r05.r05HoursFa) as HoursFa,sum(r05.r05HoursNeFa) as HoursNeFa,sum(r05.r05HoursTotal) as HoursTotal,{s} as D1,min(isnull(p28.p28Name+' - ','')+p41.p41Name) as Project,COUNT(DISTINCT a.r01ID) as Krat");
            sb(" FROM r01Capacity a INNER JOIN r02CapacityVersion r02 ON a.r02ID=r02.r02ID INNER JOIN r05CapacityUnit r05 ON a.r01ID=r05.r01ID INNER JOIN p41Project p41 ON a.p41ID=p41.p41ID LEFT OUTER JOIN p28Contact p28 ON p41.p28ID_Client=p28.p28ID");

            

            mq.explicit_sqlgroupby = $"a.p41ID, {s}";
            mq.explicit_orderby = "min(p28.p28Name),min(p41.p41Name)";

            DL.FinalSqlCommand fq = DL.basQuery.GetFinalSql(sbret(), mq, _mother.CurrentUser);

            return _db.GetList<BO.CapacityProjectGroupBy>(fq.FinalSql, fq.Parameters);
        }
        public int Save(BO.r01Capacity rec)
        {

            if (!ValidateBeforeSave(rec))
            {
                return 0;
            }
            var p = new DL.Params4Dapper();
            TimeSpan span = rec.r01End - rec.r01Start;


            p.AddInt("pid", rec.pid);
            p.AddInt("r02ID", rec.r02ID, true);
            p.AddInt("p41ID", rec.p41ID, true);
            p.AddInt("j02ID", rec.j02ID, true);
            p.AddString("r01Text", rec.r01Text);
            p.AddDateTime("r01Start", rec.r01Start);
            p.AddDateTime("r01End", rec.r01End);

            p.AddDouble("r01HoursFa", rec.r01HoursFa);
            p.AddDouble("r01HoursNeFa", rec.r01HoursNeFa);
            p.AddDouble("r01HoursTotal", rec.r01HoursFa + rec.r01HoursNeFa);

            p.AddBool("r01IsIncludeWeekend", rec.r01IsIncludeWeekend);
            p.AddString("r01Color", rec.r01Color);

            int intDaysAll = 0; int intDaysPlan = 0;

            for (DateTime d = rec.r01Start; d <= rec.r01End; d = d.AddDays(1))
            {
                intDaysAll += 1;
                if (rec.r01IsIncludeWeekend || (!rec.r01IsIncludeWeekend && d.DayOfWeek != DayOfWeek.Saturday && d.DayOfWeek != DayOfWeek.Sunday))
                {
                    intDaysPlan += 1;
                }
            }

            if (intDaysPlan <= 0)
            {
                return 0;
            }

            rec.r01UnitFa = rec.r01HoursFa / Convert.ToDouble(intDaysPlan);
            rec.r01UnitNeFa = rec.r01HoursNeFa / Convert.ToDouble(intDaysPlan);
            
            p.AddDouble("r01UnitFa", rec.r01UnitFa);
            p.AddDouble("r01UnitNeFa", rec.r01UnitNeFa);
            p.AddDouble("r01UnitTotal", rec.r01UnitFa+ rec.r01UnitNeFa);

            p.AddInt("r01DaysAll", intDaysAll);
            p.AddInt("r01DaysPlan", intDaysPlan);

            int intR01ID = _db.SaveRecord("r01Capacity", p, rec);

            var lisR05 = GetList_r05(intR01ID);
            var strGUID = BO.Code.Bas.GetGuid();
            double fa = rec.r01UnitFa;
            double nefa = rec.r01UnitNeFa;


            for (DateTime d = rec.r01Start; d <= rec.r01End; d = d.AddDays(1))
            {
                DateTime y = new DateTime(d.Year, 1, 1);
                DateTime m = new DateTime(d.Year, d.Month, 1);
                DateTime w = BO.Code.Bas.get_first_prev_monday(d);

                if (rec.r01IsIncludeWeekend || (!rec.r01IsIncludeWeekend && d.DayOfWeek != DayOfWeek.Sunday && d.DayOfWeek != DayOfWeek.Saturday))
                {
                    if (rec.pid > 0 && lisR05.Any(p => p.r05Date == d))
                    {
                        int intR05ID = lisR05.First(p => p.r05Date == d).r05ID;
                        _db.RunSql("UPDATE r05CapacityUnit SET r05Date=@d, r05HoursFa=@fa,r05HoursNeFa=@nefa,r05HoursTotal=@total,r05TempGuid=@guid,r05Year=@y,r05Month=@m,r05Week=@w WHERE r05ID=@pid", new { d = d, fa = fa, nefa = nefa, total = fa+nefa, pid = intR05ID, guid = strGUID, y = y, m = m, w = w });
                    }
                    else
                    {
                        _db.RunSql("INSERT INTO r05CapacityUnit(r05Date,r01ID,r05HoursFa,r05HoursNeFa,r05HoursTotal,r05TempGuid,r05Year,r05Month,r05Week) VALUES(@d,@pid,@fa,@nefa,@total,@guid,@y,@m,@w)", new { d = d, pid = intR01ID, fa = fa, nefa = nefa, total = fa+nefa, guid = strGUID, y = y, m = m, w = w });
                    }
                }

            }
            if (rec.pid > 0)
            {
                _db.RunSql("if exists(select r05ID FROM r05CapacityUnit WHERE r01ID=@pid AND ISNULL(r05TempGuid,'')<>@guid) DELETE FROM r05CapacityUnit WHERE r01ID=@pid AND ISNULL(r05TempGuid,'')<>@guid", new { pid = intR01ID, guid = strGUID });
            }
            _db.RunSql("update r05CapacityUnit set r05TempGuid=null WHERE r01ID=@pid", new { pid = intR01ID });

            _db.RunSql("exec dbo.p41_recalc_capacity @p41id", new { @p41id = rec.p41ID });

            return intR01ID;

        }
        public bool ValidateBeforeSave(BO.r01Capacity rec)
        {
            rec.r01HoursTotal = rec.r01HoursFa + rec.r01HoursNeFa;
            //rec.r01End = BO.Code.Bas.ConvertDateTo235959(rec.r01End);
            if (rec.p41ID == 0 || rec.j02ID == 0 || rec.r02ID == 0)
            {
                this.AddMessageTranslated("p41id or j02id or r02id missing."); return false;
            }
            if (rec.r01Start >= rec.r01End)
            {
                this.AddMessage("Začátek musí být menší než Konec."); return false;
            }
            if (rec.r01HoursFa<=0 && rec.r01HoursNeFa<=0)
            {
                this.AddMessage("Hodiny plánu musí být větší než nula."); return false;
            }
            int intDaysPlan = 0;
            for (DateTime d = rec.r01Start; d <= rec.r01End; d = d.AddDays(1))
            {               
                if (rec.r01IsIncludeWeekend || (!rec.r01IsIncludeWeekend && d.DayOfWeek != DayOfWeek.Saturday && d.DayOfWeek != DayOfWeek.Sunday))
                {
                    intDaysPlan += 1;
                }
            }
            if (intDaysPlan == 0)
            {
                this.AddMessageTranslated("V zadaném období není pracovní den.");
                return false;
            }

            var recP41 = _mother.p41ProjectBL.Load(rec.p41ID);
            if (recP41.p41PlanFrom !=null && recP41.p41PlanUntil != null && (rec.r01End > recP41.p41PlanUntil || rec.r01Start<recP41.p41PlanFrom))
            {
                this.AddMessageTranslated("Datum od-do v záznamu plánu je mimo plán období projektu.");
                return false;
            }

            var lisR01 = GetList(new BO.myQueryR01() { p41id = rec.p41ID, r02id = rec.r02ID, j02id = rec.j02ID }).Where(p => p.pid != rec.pid);
            var lisR04 = _mother.p41ProjectBL.GetList_r04(rec.p41ID,0).Where(p => p.j02ID == rec.j02ID);
            if (lisR04.Count() == 0)
            {
                this.AddMessageTranslated($"[{_mother.CBL.GetObjectAlias("j02", rec.j02ID)}] chybí v personálních zdrojích pro plánování tohoto projektu.");
                return false;
            }
            if (rec.r01HoursFa>0 && recP41.IsPlan_FaZastropovano())    //plán FA hodin v projektu je zastropován
            {                
                if (_mother.Lic.x01IsCapacityFaNefa)
                {
                    if (Math.Round(lisR01.Sum(p => p.r01HoursFa) + rec.r01HoursFa,0) > Math.Round(lisR04.Sum(p => p.r04HoursFa),0))
                    {
                        this.AddMessageTranslated($"Fa kapacita [{rec.r01HoursFa}] pro [{_mother.CBL.GetObjectAlias("j02", rec.j02ID)}] by přesáhla zastropovaný plán Fa hodin v projektu!");
                        return false;
                    }
                }
                else
                {
                    if (lisR04.Sum(p => p.r04HoursTotal) == 0)
                    {
                        this.AddMessageTranslated($"[{_mother.CBL.GetObjectAlias("j02", rec.j02ID)}] chybí v personálních zdrojích pro plánování tohoto projektu.");
                        return false;
                    }
                    if (Math.Round( lisR01.Sum(p => p.r01HoursTotal)+ rec.r01HoursFa,0) > Math.Round(lisR04.Sum(p => p.r04HoursTotal),0))    //plán celkových hodin v projektu je zastropován
                    {
                        
                        this.AddMessageTranslated($"Kapacita [{rec.r01HoursFa}] pro [{_mother.CBL.GetObjectAlias("j02", rec.j02ID)}] by přesáhla zastropovaný plán hodin: [{lisR04.Sum(p => p.r04HoursTotal)}]!");
                        return false;
                    }
                }
                
            }

            if (rec.r01HoursNeFa>0 && recP41.IsPlan_NefaZastropovano())
            {
                //plán NEFA hodin v projektu je zastropován
                
                if (Math.Round(lisR01.Sum(p => p.r01HoursNeFa) + rec.r01HoursNeFa,0) > Math.Round(lisR04.Sum(p => p.r04HoursNeFa),0))
                {
                    
                    this.AddMessageTranslated($"NeFa kapacita [{rec.r01HoursNeFa}] pro [{_mother.CBL.GetObjectAlias("j02", rec.j02ID)}] by přesáhla zastropovaný plán Nefa hodin v projektu!");
                    return false;
                }
            }
           



            return true;
        }

    }
}
