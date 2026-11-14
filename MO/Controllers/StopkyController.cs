using Microsoft.AspNetCore.Mvc;
using MO.Models;

namespace MO.Controllers
{
    /// <summary>
    /// Stopky (p68StopWatch) v mobilní aplikaci. Vzhled a chování odpovídá stopkám v UI (p68Controller):
    /// běží vždy nejvýše jeden časovač uživatele, start jiného ten běžící zastaví, nejnovější časovač je nahoře.
    /// Strukturální akce (start, stop, přidat, kopírovat, smazat, ±minuty, vykázat) jdou postbackem formuláře,
    /// který zároveň uloží rozpracované změny všech karet. Text a aktivita se ukládají i průběžně přes SaveRow (bez překreslení).
    /// Všechny operace pracují jen s časovači přihlášeného uživatele.
    /// </summary>
    public class StopkyController : BaseController
    {
        /// <summary>Max. naměřený čas jednoho časovače (ruční zadání i úpravy ±).</summary>
        private const int MaxSeconds = 24 * 60 * 60;

        public IActionResult Index()
        {
            var v = new StopkyViewModel { PageTitle = Factory.tra("Stopky"), HideHeaderTitle = true };
            RefreshState(v);
            return View(v);
        }

        /// <summary>
        /// oper: start | stop | new_start (nový časovač a rovnou spustit) | add | clone | delete | clear
        ///       | adjust (secs = ± sekundy) | report (zastavit a přejít na vykázání úkonu) | reload
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Index(StopkyViewModel v, string oper, int p68id, int secs)
        {
            // Nejprve uložit rozpracované změny (text/aktivita/projekt/ruční čas) ze všech karet,
            // teprve pak provést akci.
            SaveAllRowEdits(v);

            var now = DateTime.Now;
            var rec = LoadMine(p68id);
            int jump = p68id;

            switch (oper)
            {
                case "start":
                    if (rec == null) break;
                    StopAll(now);
                    rec = LoadMine(p68id);
                    rec.p68IsRunning = true;
                    rec.p68LastStart = now;
                    rec.p68LastEnd = null;
                    Factory.p68StopWatchBL.Save(rec);
                    break;

                case "stop":
                    StopAll(now);
                    break;

                case "new_start":
                case "add":
                    {
                        if (oper == "new_start") StopAll(now);
                        var c = new BO.p68StopWatch { j02ID = Factory.CurrentUser.pid, p68Ordinary = NextOrdinary() };
                        if (oper == "new_start") { c.p68IsRunning = true; c.p68LastStart = now; }
                        jump = Factory.p68StopWatchBL.Save(c);
                        break;
                    }

                case "clone":
                    if (rec == null) break;
                    jump = Factory.p68StopWatchBL.Save(new BO.p68StopWatch { j02ID = Factory.CurrentUser.pid, p41ID = rec.p41ID, p56ID = rec.p56ID, p32ID = rec.p32ID, p68Text = rec.p68Text, p68Ordinary = NextOrdinary() });
                    break;

                case "delete":
                    if (rec != null) Factory.CBL.DeleteRecord("p68", rec.pid);
                    jump = 0;
                    break;

                case "clear":
                    Factory.p68StopWatchBL.Clear(Factory.CurrentUser.pid);
                    jump = 0;
                    break;

                case "adjust":
                    if (rec == null || secs == 0) break;
                    if (rec.p68IsRunning && rec.p68LastStart != null)
                    {
                        // běžící časovač: posunout začátek (přidat čas = start dřív), výsledek 0 až MaxSeconds
                        int total = Math.Clamp(Elapsed(rec, now) + secs, 0, MaxSeconds);
                        if (total < rec.p68Duration) rec.p68Duration = total;     // ubráno víc, než naběhlo od startu
                        rec.p68LastStart = now.AddSeconds(-(total - rec.p68Duration));
                    }
                    else
                    {
                        rec.p68Duration = Math.Clamp(rec.p68Duration + secs, 0, MaxSeconds);
                    }
                    Factory.p68StopWatchBL.Save(rec);
                    break;

                case "report":
                    // vykázat jako časový úkon: běžící časovač nejdřív zastavit; po uložení úkonu se časovač odstraní (UkonController)
                    if (rec == null) break;
                    if (rec.p68IsRunning) StopAll(now);
                    return RedirectToAction("New", "Ukon", new { p68id = rec.pid });
            }

            // PRG: po akci přesměrovat, aby obnovení stránky akci neopakovalo
            return RedirectToAction("Index", new { jump = jump > 0 ? jump : (int?)null });
        }

        /// <summary>Průběžné uložení textu a aktivity jedné karty (bez překreslení stránky).</summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SaveRow(int pid, string text, int p32id)
        {
            var rec = LoadMine(pid);
            if (rec == null) return Json(new { ok = false });
            rec.p68Text = text;
            if (rec.p41ID > 0) rec.p32ID = p32id;
            Factory.p68StopWatchBL.Save(rec);
            return Json(new { ok = true });
        }

        // ===== Pomocné metody =====

        /// <summary>Časovač jen přihlášeného uživatele; cizí nebo neexistující = null.</summary>
        private BO.p68StopWatch LoadMine(int p68id)
        {
            if (p68id <= 0) return null;
            var rec = Factory.p68StopWatchBL.Load(p68id);
            return rec != null && rec.j02ID == Factory.CurrentUser.pid ? rec : null;
        }

        /// <summary>Celkový čas časovače včetně právě běžícího úseku (i přes půlnoc).</summary>
        private static int Elapsed(BO.p68StopWatch r, DateTime now)
        {
            int secs = r.p68Duration;
            if (r.p68IsRunning && r.p68LastStart != null) secs += (int)Math.Max(0, (now - r.p68LastStart.Value).TotalSeconds);
            return secs;
        }

        /// <summary>Zastaví běžící časovače uživatele a připočte naměřený čas.</summary>
        private void StopAll(DateTime now)
        {
            foreach (var r in Factory.p68StopWatchBL.GetList(Factory.CurrentUser.pid).Where(p => p.p68IsRunning).ToList())
            {
                r.p68Duration = Math.Min(Elapsed(r, now), MaxSeconds);
                r.p68IsRunning = false;
                r.p68LastEnd = now;
                Factory.p68StopWatchBL.Save(r);
            }
        }

        private int NextOrdinary()
        {
            var lis = Factory.p68StopWatchBL.GetList(Factory.CurrentUser.pid).ToList();
            return lis.Count == 0 ? 1 : Math.Max(lis.Count, lis.Max(p => p.p68Ordinary)) + 1;
        }

        private void RefreshState(StopkyViewModel v)
        {
            var now = DateTime.Now;
            var lisP68 = Factory.p68StopWatchBL.GetList(Factory.CurrentUser.pid).ToList();

            v.ProjectComboItems = Factory.p41ProjectBL
                .GetList(new BO.myQueryP41("p41") { j02id_query = Factory.CurrentUser.pid, p33id_for_p31_entry = 1 })
                .Select(p => new ComboItem { Id = p.pid, Code = p.p41Code, Text = p.PrefferedName, Meta = p.Client })
                .ToList();

            // nejnovější nahoře (stejně jako v UI)
            foreach (var rec in lisP68.OrderByDescending(p => p.p68Ordinary).ThenByDescending(p => p.pid))
            {
                var row = new StopwatchRowViewModel
                {
                    pid = rec.pid,
                    p68Ordinary = rec.p68Ordinary,
                    p68IsRunning = rec.p68IsRunning,
                    p68LastStart = rec.p68LastStart,
                    p68Duration = rec.p68Duration,
                    ElapsedSeconds = Elapsed(rec, now),
                    DateInsert = rec.DateInsert,
                    DurationHHMM = BO.Code.Time.GetTimeFromSeconds(rec.p68Duration),
                    p41ID = rec.p41ID,
                    SelectedProjectText = rec.p28Name != null ? $"{rec.p28Name} - {rec.p41Name}" : rec.p41Name,
                    p32ID = rec.p32ID,
                    SelectedActivityText = rec.p32Name,
                    p68Text = rec.p68Text
                };

                if (row.p41ID > 0)
                {
                    row.ActivityComboItems = Factory.p32ActivityBL
                        .GetList(new BO.myQueryP32 { p41id = row.p41ID, p33id = 1 })
                        .Select(a => new ComboItem { Id = a.pid, Text = a.p32Name, GroupBy = a.p38Name })
                        .ToList();
                }

                v.Rows.Add(row);
            }

            v.TotalSeconds = v.Rows.Sum(p => p.ElapsedSeconds);
            v.IsRunning = v.Rows.Any(p => p.p68IsRunning);
            if (int.TryParse(Request.Query["jump"], out int jump)) v.JumpToPid = jump;
        }

        private void SaveAllRowEdits(StopkyViewModel v)
        {
            if (v.Rows == null) return;

            foreach (var row in v.Rows)
            {
                var rec = LoadMine(row.pid);
                if (rec == null) continue;

                if (rec.p41ID != row.p41ID) row.p32ID = 0;      // aktivita se nabízí podle projektu
                rec.p41ID = row.p41ID;
                rec.p32ID = row.p32ID;
                rec.p68Text = row.p68Text;

                // Ruční úprava času - jen pokud časovač zrovna neběží a hodnota se opravdu změnila
                // (formulář nese čas zaokrouhlený na minuty; beze změny by se zahodily naměřené sekundy)
                if (!rec.p68IsRunning && !string.IsNullOrWhiteSpace(row.DurationHHMM)
                    && row.DurationHHMM.Trim() != BO.Code.Time.GetTimeFromSeconds(rec.p68Duration))
                {
                    rec.p68Duration = Math.Clamp(BO.Code.Time.ConvertTimeToSeconds(row.DurationHHMM), 0, MaxSeconds);
                    rec.p68LastEnd = DateTime.Now;
                }

                Factory.p68StopWatchBL.Save(rec);
            }
        }
    }
}
