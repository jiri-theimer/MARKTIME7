using Microsoft.AspNetCore.Mvc;
using UI.Models.p31oper;

namespace UI.Controllers.p31oper
{
    /// <summary>
    /// Stopky (p68StopWatch). Okno /p68/Index (modal z horní lišty, nebo samostatná záložka).
    /// Všechny operace jdou přes AJAX (Oper) a vrací znovu vykreslený seznam karet (_Rows) – bez postbacku celé stránky.
    /// Běží vždy nejvýše jeden časovač uživatele; start jiného ten běžící zastaví.
    /// /p68/Running – stav pro ikonu stopek v horní liště (běžící čas).
    /// </summary>
    public class p68Controller : BaseController
    {
        /// <summary>Max. naměřený čas jednoho časovače (ruční zadání i úpravy ±).</summary>
        private const int MaxSeconds = 24 * 60 * 60;

        public IActionResult Index()
        {
            return View(Load());
        }

        /// <summary>Seznam karet (po zavření okna úkonu apod.).</summary>
        public IActionResult Rows()
        {
            return PartialView("_Rows", Load());
        }

        /// <summary>
        /// Operace nad stopkami; vrací znovu vykreslený seznam karet.
        /// oper: start | stop | new_start (nový časovač a rovnou spustit) | add | clone | delete | clear | p41id | adjust (secs = ± sekundy)
        /// </summary>
        [HttpPost]
        public IActionResult Oper(string oper, int p68id, int p41id, int secs)
        {
            var now = DateTime.Now;
            var j02id = Factory.CurrentUser.pid;
            var rec = p68id > 0 ? Factory.p68StopWatchBL.Load(p68id) : null;
            if (rec != null && rec.j02ID != j02id) rec = null;      //jen vlastní stopky
            int jump = p68id;

            switch (oper)
            {
                case "start":
                    if (rec == null) break;
                    StopAll(now);
                    rec = Factory.p68StopWatchBL.Load(rec.pid);
                    rec.p68IsRunning = true;
                    rec.p68LastStart = now;
                    Factory.p68StopWatchBL.Save(rec);
                    break;

                case "stop":
                    StopAll(now);
                    break;

                case "new_start":
                case "add":
                    if (oper == "new_start") StopAll(now);
                    var c = new BO.p68StopWatch { j02ID = j02id, p68Ordinary = NextOrdinary() };
                    if (oper == "new_start") { c.p68IsRunning = true; c.p68LastStart = now; }
                    jump = Factory.p68StopWatchBL.Save(c);
                    break;

                case "clone":
                    if (rec == null) break;
                    jump = Factory.p68StopWatchBL.Save(new BO.p68StopWatch { j02ID = j02id, p41ID = rec.p41ID, p56ID = rec.p56ID, p32ID = rec.p32ID, p68Text = rec.p68Text, p68Ordinary = NextOrdinary() });
                    break;

                case "delete":
                    if (rec != null) Factory.CBL.DeleteRecord("p68", rec.pid);
                    jump = 0;
                    break;

                case "clear":
                    Factory.p68StopWatchBL.Clear(j02id);
                    jump = 0;
                    break;

                case "p41id":
                    if (rec == null) break;
                    if (rec.p41ID != p41id) rec.p32ID = 0;      //aktivita se nabízí podle projektu
                    rec.p41ID = p41id;
                    Factory.p68StopWatchBL.Save(rec);
                    break;

                case "adjust":
                    if (rec == null || secs == 0) break;
                    if (rec.p68IsRunning && rec.p68LastStart != null)
                    {
                        //běžící časovač: posunout začátek (přidat čas = start dřív), výsledek 0 až MaxSeconds
                        int total = Math.Clamp(Elapsed(rec, now) + secs, 0, MaxSeconds);
                        if (total < rec.p68Duration) rec.p68Duration = total;     //ubráno víc, než naběhlo od startu
                        rec.p68LastStart = now.AddSeconds(-(total - rec.p68Duration));
                    }
                    else
                    {
                        rec.p68Duration = Math.Clamp(rec.p68Duration + secs, 0, MaxSeconds);
                    }
                    Factory.p68StopWatchBL.Save(rec);
                    break;
            }
            Factory.j02UserBL.RecoveryUserCache_StopWatch(j02id);     //ikona stopek v liště (StopWatchFlag)

            var v = Load();
            v.Jump2Pid = jump;
            return PartialView("_Rows", v);
        }

        /// <summary>Uložení textu, aktivity a ručně zadaného času (bez překreslení karet).</summary>
        [HttpPost]
        public StopWatchRow SaveClientChanges(int pid, string p68text, string durhhmm, int p32id)
        {
            var c = Factory.p68StopWatchBL.Load(pid);
            if (c == null || c.j02ID != Factory.CurrentUser.pid) return null;
            c.p68Text = p68text;
            c.p32ID = p32id;

            if (!string.IsNullOrEmpty(durhhmm) && !c.p68IsRunning)     //ruční změna času (jen u zastaveného časovače)
            {
                c.p68Duration = Math.Clamp(BO.Code.Time.ConvertTimeToSeconds(durhhmm), 0, MaxSeconds);
                c.p68LastEnd = DateTime.Now;
            }
            Factory.p68StopWatchBL.Save(c);
            return ConvertRec2Row(Factory.p68StopWatchBL.Load(pid), DateTime.Now);
        }

        /// <summary>Stav pro horní lištu: flag 0 = nic, 1 = běží, 2 = jsou záznamy; elapsed = sekundy běžícího časovače.</summary>
        public IActionResult Running()
        {
            var now = DateTime.Now;
            var lis = Factory.p68StopWatchBL.GetList(Factory.CurrentUser.pid).ToList();
            var run = lis.FirstOrDefault(p => p.p68IsRunning);
            return Json(new
            {
                flag = run != null ? 1 : lis.Count > 0 ? 2 : 0,
                elapsed = run == null ? 0 : Elapsed(run, now),
                project = run == null ? null : (run.p28Name != null ? $"{run.p28Name} - {run.p41Name}" : run.p41Name),
                count = lis.Count
            });
        }

        // ------------------------------------------------------------------

        private p68ViewModel Load()
        {
            var now = DateTime.Now;
            var v = new p68ViewModel { lisRows = new List<StopWatchRow>() };
            //nejnovější nahoře
            foreach (var rec in Factory.p68StopWatchBL.GetList(Factory.CurrentUser.pid).OrderByDescending(p => p.p68Ordinary).ThenByDescending(p => p.pid))
            {
                v.lisRows.Add(ConvertRec2Row(rec, now));
            }
            v.TotalSeconds = v.lisRows.Sum(p => p.ElapsedSeconds);
            return v;
        }

        /// <summary>Zastaví běžící časovače uživatele a připočte naměřený čas.</summary>
        private void StopAll(DateTime now)
        {
            foreach (var r in Factory.p68StopWatchBL.GetList(Factory.CurrentUser.pid).Where(p => p.p68IsRunning))
            {
                r.p68Duration = Math.Min(Elapsed(r, now), MaxSeconds);
                r.p68IsRunning = false;
                r.p68LastEnd = now;
                Factory.p68StopWatchBL.Save(r);
            }
        }

        /// <summary>Celkový čas časovače včetně právě běžícího úseku (TotalSeconds – i přes půlnoc).</summary>
        private static int Elapsed(BO.p68StopWatch r, DateTime now)
        {
            int secs = r.p68Duration;
            if (r.p68IsRunning && r.p68LastStart != null) secs += (int)Math.Max(0, (now - r.p68LastStart.Value).TotalSeconds);
            return secs;
        }

        private int NextOrdinary()
        {
            var lis = Factory.p68StopWatchBL.GetList(Factory.CurrentUser.pid).ToList();
            return lis.Count == 0 ? 1 : Math.Max(lis.Count, lis.Max(p => p.p68Ordinary)) + 1;
        }

        private StopWatchRow ConvertRec2Row(BO.p68StopWatch rec, DateTime now)
        {
            var c = new StopWatchRow() { pid = rec.pid, p41ID = rec.p41ID, p32ID = rec.p32ID, p56ID = rec.p56ID, j02ID = rec.j02ID, p68Text = rec.p68Text, p68Ordinary = rec.p68Ordinary, p68IsRunning = rec.p68IsRunning, p68LastStart = rec.p68LastStart, p68LastEnd = rec.p68LastEnd, p68Duration = rec.p68Duration };
            c.Project = rec.p41Name;
            c.Activity = rec.p32Name;
            if (rec.p28Name != null)
            {
                c.Project = $"{rec.p28Name} - {rec.p41Name}";
            }
            c.ElapsedSeconds = Elapsed(rec, now);
            c.duration_hhmm = BO.Code.Time.GetTimeFromSeconds((double)c.p68Duration);
            c.DateInsert = rec.DateInsert;

            if (c.p41ID > 0)
            {
                var recP41 = Factory.p41ProjectBL.Load(c.p41ID);
                c.p61ID = (recP41.p61ID > 0 ? recP41.p61ID : recP41.p61ID_Byp42ID);
            }

            return c;
        }
    }
}
