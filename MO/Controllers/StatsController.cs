using Microsoft.AspNetCore.Mvc;
using MO.Models;

namespace MO.Controllers
{
    public class StatsController : BaseController
    {
        // ===== Statistika za období =====
        public IActionResult Index(string d1, string d2, string label)
        {
            var date1 = ParseDate(d1) ?? new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
            var date2 = ParseDate(d2) ?? date1.AddMonths(1).AddDays(-1);

            var v = new StatsViewModel
            {
                PageTitle = Factory.tra("Statistika"),
                d1 = date1,
                d2 = date2,
                PeriodLabel = !string.IsNullOrEmpty(label) ? label : $"{date1:d.M.} – {date2:d.M.yyyy}"
            };

            var qryP31 = Factory.p31WorksheetBL.GetList(new BO.myQueryP31
            {
                j02id = Factory.CurrentUser.pid,
                global_d1 = date1,
                global_d2 = date2
            }).ToList();

            // Vykázané hodiny celkem / fakturovatelné
            v.Hodiny = qryP31.Sum(p => p.p31Hours_Orig);
            v.HodinyFa = qryP31.Where(p => p.p32IsBillable).Sum(p => p.p31Hours_Orig);

            // Rozpracované (stav "Nic", ještě nevyúčtováno)
            v.HodinyWip = qryP31.Where(p => p.p71ID == BO.p71IdENUM.Nic && p.p91ID == 0).Sum(p => p.p31Hours_Orig);
            v.HodinyFaWip = qryP31.Where(p => p.p32IsBillable && p.p71ID == BO.p71IdENUM.Nic && p.p91ID == 0).Sum(p => p.p31Hours_Orig);
            v.HodinyNeFaWip = qryP31.Where(p => !p.p32IsBillable && p.p71ID == BO.p71IdENUM.Nic && p.p91ID == 0).Sum(p => p.p31Hours_Orig);

            // Schváleno, čeká na vyúčtování
            v.Hodiny4Approve = qryP31.Where(p => p.p71ID == BO.p71IdENUM.Schvaleno && p.p91ID == 0).Sum(p => p.p31Hours_Orig);

            // Vyúčtováno (jen s oprávněním na sazby)
            v.HasRatesAccess = Factory.CurrentUser.IsRatesAccess;
            if (v.HasRatesAccess)
            {
                v.HodinyVyfa4 = qryP31.Where(p => p.p91ID > 0 && p.p70ID == BO.p70IdENUM.Vyfakturovano).Sum(p => p.p31Hours_Orig);
                v.HodinyVyfa6 = qryP31.Where(p => p.p91ID > 0 && p.p70ID == BO.p70IdENUM.ZahrnutoDoPausalu).Sum(p => p.p31Hours_Orig);
                v.HodinyVyfa23 = qryP31.Where(p => p.p91ID > 0
                    && (p.p70ID == BO.p70IdENUM.SkrytyOdpis || p.p70ID == BO.p70IdENUM.ViditelnyOdpis))
                    .Sum(p => p.p31Hours_Orig);
            }

            // Fond pracovní doby
            var recJ02 = Factory.j02UserBL.Load(Factory.CurrentUser.pid);
            if (recJ02 != null)
            {
                if (recJ02.c21ScopeFlag == BO.c21ScopeFlagENUM.PerTimesheet)
                {
                    v.Fond = v.Hodiny;
                }
                else
                {
                    v.Fond = Factory.c21FondCalendarBL.GetSumHours(recJ02.c21ID, recJ02.j02CountryCode, date1, date2);
                }
            }

            if (v.Fond > 0)
            {
                v.UtilTotal = 100.00 * v.Hodiny / v.Fond;
                v.UtilFa = 100.00 * v.HodinyFa / v.Fond;
            }

            // Top 15 naposledy vykazovaných projektů
            v.Top15Projects = Factory.p41ProjectBL.GetList_MyTop10(Factory.CurrentUser.pid, 15);

            return View(v);
        }


        private DateTime? ParseDate(string s)
        {
            if (string.IsNullOrWhiteSpace(s)) return null;
            if (DateTime.TryParseExact(s, "yyyy-MM-dd",
                System.Globalization.CultureInfo.InvariantCulture,
                System.Globalization.DateTimeStyles.None, out var dt))
                return dt;
            try { return BO.Code.Bas.String2Date(s); }
            catch { return null; }
        }
    }
}
