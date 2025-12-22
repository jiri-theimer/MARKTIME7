using System;
using System.Collections.Generic;
using System.Text;

namespace BO.Code.Cls
{
    public class ThePeriodProviderSupport
    {
        private List<BO.ThePeriod> _lis;
        public List<BO.ThePeriod> GetPallete()
        {
            _lis = new List<BO.ThePeriod>();
            SetupPallete();


            return _lis;
        }

        private void SetupPallete()
        {
            _lis.Add(new BO.ThePeriod() { pid = 0, PeriodName = "--Nefiltrovat období--", PeriodInterval = "" });
            _lis.Add(new BO.ThePeriod() { pid = 1, PeriodName = "--Časové období ručně--", PeriodInterval = "" });
            AF(11); //včera
            AF(10); //dnes
            AF(12); //zítra

            AF(21); //minulý týden
            AF(20); //tento týden
            AF(22); //příští týden
            AF(23); //příští 2 týdny
            AF(24); //příští 3 týdny

            AF(31); //minulý měsíc
            AF(30); //tento měsíc
            AF(32); //příští měsíc
            AF(33); AF(34); AF(35); AF(36);AF(37);

            AF(41); AF(40); AF(42);

            AF(51); AF(50); AF(52);
        }

       
        

        private void AF(int pid)
        {
            DateTime d1 = DateTime.Now; DateTime d2 = DateTime.Now; string strName = ""; string strInterval = null;

            switch (pid)
            {
                case 10:
                    strName = "Dnes"; d1 = DateTime.Today; d2 = DateTime.Today;
                    break;
                case 11:
                    strName = "Včera"; d1 = DateTime.Today.AddDays(-1); d2 = DateTime.Today.AddDays(-1);
                    break;
                case 12:
                    strName = "Zítra"; d1 = DateTime.Today.AddDays(1); d2 = DateTime.Today.AddDays(1);
                    break;
                case 20:
                    strName = "Tento týden";
                    d1 = FirstDateInWeek(DateTime.Today);
                    d2 = d1.AddDays(6);
                    break;
                case 21:
                    strName = "Minulý týden";
                    d1 = FirstDateInWeek(DateTime.Today.AddDays(-7));
                    d2 = d1.AddDays(6);
                    break;
                case 22:
                    strName = "Příští týden";
                    d1 = FirstDateInWeek(DateTime.Today.AddDays(7));
                    d2 = d1.AddDays(6);
                    break;
                case 23:
                    strName = "Příští 2 týdny";
                    d1 = FirstDateInWeek(DateTime.Today.AddDays(7));
                    d2 = d1.AddDays(13);
                    break;
                case 24:
                    strName = "Příští 3 týdny";
                    d1 = FirstDateInWeek(DateTime.Today.AddDays(7));
                    d2 = d1.AddDays(20);
                    break;
                case 30:
                    strName = "Tento měsíc";
                    d1 = new DateTime(d1.Year, d1.Month, 1);
                    d2 = d1.AddMonths(1).AddDays(-1);
                    strInterval = $"{d1.Month}/{d1.Year}";
                    break;
                case 31:
                    strName = "Minulý měsíc";
                    d1 = new DateTime(d1.Year, d1.Month, 1).AddMonths(-1);
                    d2 = d1.AddMonths(1).AddDays(-1);
                    strInterval = $"{d1.Month}/{d1.Year}";
                    break;
                case 32:
                    strName = "Příští měsíc";
                    d1 = new DateTime(d1.Year, d1.Month, 1).AddMonths(1);
                    d2 = d1.AddMonths(1).AddDays(-1);

                    strInterval = $"{d1.Month}/{d1.Year}";
                    break;
                case 33:
                    strName = "Předminulý měsíc";
                    d1 = new DateTime(d1.Year, d1.Month, 1).AddMonths(-2);
                    d2 = d1.AddMonths(1).AddDays(-1);

                    strInterval = $"{d1.Month}/{d1.Year}";
                    break;
                case 34:
                    d2 = new DateTime(d1.Year, d1.Month, 1).AddMonths(-2).AddDays(-1);
                    d1 = new DateTime(1990, 1, 1);
                    
                    strName = $"Do konce {d2.Month}/{d2.Year}";
                    strInterval = "";
                    break;
                case 35:
                    d2 = new DateTime(d1.Year, d1.Month, 1).AddMonths(-1).AddDays(-1);
                    d1 = new DateTime(1990, 1, 1);

                    strName = $"Do konce {d2.Month}/{d2.Year}";
                    strInterval = "";
                    break;
                case 36:
                    d2 = new DateTime(d1.Year, d1.Month, 1).AddDays(-1);
                    d1 = new DateTime(1990, 1, 1);
                    strName = $"Do konce {d2.Month}/{d2.Year}";
                    strInterval = "";
                    break;
                case 37:
                    d2 = DateTime.Today;
                    d1 = new DateTime(1990, 1, 1);
                    strName = $"Do dnes vč.";
                    strInterval = $"{DateTime.Today.ToString("dd.MM.")}";
                    break;
                case 40:
                    strName = "Tento kvartál";
                    int x = (int)System.Math.Ceiling((decimal)d1.Month / 3);
                    d1 = new DateTime(d1.Year, (3 * x) - 2, 1);
                    d2 = d1.AddMonths(3).AddDays(-1);
                    break;
                case 41:
                    strName = "Minulý kvartál";
                    int xx = (int)System.Math.Ceiling((decimal)d1.Month / 3);
                    d1 = new DateTime(d1.Year, (3 * xx) - 2, 1);
                    d1 = d1.AddMonths(-3);
                    d2 = d1.AddMonths(3).AddDays(-1);
                    break;
                case 42:
                    strName = "Příští kvartál";
                    int xxx = (int)System.Math.Ceiling((decimal)d1.Month / 3);
                    d1 = new DateTime(d1.Year, (3 * xxx) - 2, 1);
                    d1 = d1.AddMonths(3);
                    d2 = d1.AddMonths(3).AddDays(-1);
                    break;
                case 50:
                    strName = "Tento rok";
                    d1 = new DateTime(d1.Year, 1, 1);
                    d2 = new DateTime(d1.Year, 12, 31);
                    strInterval = d1.Year.ToString();
                    break;
                case 51:
                    strName = "Minulý rok";
                    d1 = new DateTime(d1.Year - 1, 1, 1);
                    d2 = d1.AddYears(1).AddDays(-1);
                    strInterval = d1.Year.ToString();
                    break;
                case 52:
                    strName = "Příští rok";
                    d1 = new DateTime(d1.Year + 1, 1, 1);
                    d2 = new DateTime(d1.Year + 1, 12, 31);
                    strInterval = d1.Year.ToString();
                    break;
            }

            if (strInterval == null)
            {
                if (d1 == d2)
                {
                    strInterval = Bas.ObjectDate2String(d1, "dd.MM.");
                }
                else
                {
                    if (d1.Month == d2.Month && d1.Year == d2.Year)
                    {
                        strInterval = $"{BO.Code.Bas.ObjectDate2String(d1, "dd.MM.")} - {Bas.ObjectDate2String(d2, "dd.MM.")}";
                    }
                    else
                    {
                        if (d1.Year == d2.Year)
                        {
                            strInterval = $"{BO.Code.Bas.ObjectDate2String(d1, "dd.MM.")} - {Bas.ObjectDate2String(d2, "dd.MM.")}";
                        }
                        else
                        {
                            strInterval = $"{BO.Code.Bas.ObjectDate2String(d1, "dd.MM.yyyy")} - {Bas.ObjectDate2String(d2, "dd.MM.yyyy")}";
                        }
                    }

                }
            }


            _lis.Add(new BO.ThePeriod() { pid = pid, PeriodName = strName, PeriodInterval = strInterval, d1 = d1, d2 = d2 });


        }


        private DateTime FirstDateInWeek(DateTime dt)
        {
            while (dt.DayOfWeek != System.Threading.Thread.CurrentThread.CurrentCulture.DateTimeFormat.FirstDayOfWeek)
                dt = dt.AddDays(-1);
            return dt;
           
        }


        

    }


}
