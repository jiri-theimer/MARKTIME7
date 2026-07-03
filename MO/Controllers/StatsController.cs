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

            var v = BuildStats(date1, date2, label);
            v.PageTitle = Factory.tra("Statistika");

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