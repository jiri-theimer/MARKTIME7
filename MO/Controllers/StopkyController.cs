using Microsoft.AspNetCore.Mvc;
using MO.Models;

namespace MO.Controllers
{
    public class StopkyController : BaseController
    {
        public IActionResult Index()
        {
            var v = new StopkyViewModel { PageTitle = Factory.tra("Stopky") };
            RefreshState(v);
            return View(v);
        }

        [HttpPost]
        public IActionResult Index(StopkyViewModel v, string oper, int p68id)
        {
            // Nejprve uložit rozpracované změny (text/aktivita/projekt/ruční čas) ze všech řádků,
            // teprve pak provést strukturální akci (start/stop/přidat/smazat...).
            SaveAllRowEdits(v);

            switch (oper)
            {
                case "start":
                    HandleStart(p68id);
                    break;

                case "stop":
                    HandleStop(p68id);
                    break;

                case "add_row":
                    {
                        var c = new BO.p68StopWatch { j02ID = Factory.CurrentUser.pid, p68Ordinary = NewRowOrdinary(v) };
                        p68id = Factory.p68StopWatchBL.Save(c);
                        break;
                    }

                case "clone_row":
                    {
                        var src = Factory.p68StopWatchBL.Load(p68id);
                        if (src != null)
                        {
                            var clone = new BO.p68StopWatch
                            {
                                j02ID = Factory.CurrentUser.pid,
                                p41ID = src.p41ID,
                                p32ID = src.p32ID,
                                p68Text = src.p68Text,
                                p68Ordinary = NewRowOrdinary(v)
                            };
                            p68id = Factory.p68StopWatchBL.Save(clone);
                        }
                        break;
                    }

                case "delete_row":
                    Factory.CBL.DeleteRecord("p68", p68id);
                    p68id = 0;
                    break;

                case "clear":
                    Factory.p68StopWatchBL.Clear(Factory.CurrentUser.pid);
                    p68id = 0;
                    break;
            }

            var v2 = new StopkyViewModel { PageTitle = Factory.tra("Stopky"), JumpToPid = p68id };
            RefreshState(v2);
            return View(v2);
        }


        // ===== Pomocné metody =====

        private void RefreshState(StopkyViewModel v)
        {
            var lisP68 = Factory.p68StopWatchBL.GetList(Factory.CurrentUser.pid).ToList();

            v.ProjectComboItems = Factory.p41ProjectBL
                .GetList(new BO.myQueryP41("p41") { j02id_query = Factory.CurrentUser.pid, p33id_for_p31_entry = 1 })
                .Select(p => new ComboItem { Id = p.pid, Code = p.p41Code, Text = p.PrefferedName, Meta = p.Client })
                .ToList();

            foreach (var rec in lisP68)
            {
                var row = new StopwatchRowViewModel
                {
                    pid = rec.pid,
                    p68Ordinary = rec.p68Ordinary,
                    p68IsRunning = rec.p68IsRunning,
                    p68LastStart = rec.p68LastStart,
                    p68Duration = rec.p68Duration,
                    DurationHHMM = BO.Code.Time.GetTimeFromSeconds(rec.p68Duration),
                    p41ID = rec.p41ID,
                    SelectedProjectText = rec.p41Name,
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

                if (rec.p68IsRunning)
                {
                    v.RunningStartUtc = rec.p68LastStart.Value.ToUniversalTime().ToString("o");
                    v.RunningBaseDuration = rec.p68Duration;
                }
            }

            v.Rows = v.Rows.OrderBy(r => r.p68Ordinary).ToList();
        }

        private void SaveAllRowEdits(StopkyViewModel v)
        {
            if (v.Rows == null) return;

            foreach (var row in v.Rows)
            {
                if (row.pid <= 0) continue;

                var rec = Factory.p68StopWatchBL.Load(row.pid);
                if (rec == null) continue;

                rec.p41ID = row.p41ID;
                rec.p32ID = row.p32ID;
                rec.p68Text = row.p68Text;
                rec.p68Ordinary = row.p68Ordinary;

                // Ruční úprava času - jen pokud stopky zrovna neběží
                if (!rec.p68IsRunning && !string.IsNullOrWhiteSpace(row.DurationHHMM))
                {
                    int secs = BO.Code.Time.ConvertTimeToSeconds(row.DurationHHMM);
                    if (secs < 0) secs = 0;
                    if (secs > 24 * 60 * 60) secs = 24 * 60 * 60;
                    rec.p68Duration = secs;
                }

                Factory.p68StopWatchBL.Save(rec);
            }
        }

        // Zastaví aktuálně běžící stopky (pokud nějaké jsou - smí běžet jen jedna) a spustí zvolenou.
        // Na rozdíl od desktopové verze NEsahá na řádky, které už neběží - vylučuje tak riziko
        // chybného přičtení "mrtvé" doby od starého LastStart (desktopový bug).
        private void HandleStart(int p68id)
        {
            var now = DateTime.Now;

            var runningRows = Factory.p68StopWatchBL.GetList(Factory.CurrentUser.pid).Where(p => p.p68IsRunning);
            foreach (var rec in runningRows)
            {
                AccumulateAndStop(rec, now);
            }

            var target = Factory.p68StopWatchBL.Load(p68id);
            if (target == null) return;

            target.p68LastStart = now;
            target.p68LastEnd = null;
            target.p68IsRunning = true;
            Factory.p68StopWatchBL.Save(target);
        }

        private void HandleStop(int p68id)
        {
            var rec = Factory.p68StopWatchBL.Load(p68id);
            if (rec == null || !rec.p68IsRunning) return;

            AccumulateAndStop(rec, DateTime.Now);
        }

        // Jediné místo, které přičítá uběhlý úsek do Duration - volá se výhradně na řádku,
        // který v tu chvíli skutečně běží (p68IsRunning == true), takže LastStart je vždy čerstvý.
        private void AccumulateAndStop(BO.p68StopWatch rec, DateTime now)
        {
            var start = rec.p68LastStart ?? now;
            var elapsedSeconds = (int)Math.Max(0, (now - start).TotalSeconds);

            rec.p68Duration += elapsedSeconds;
            rec.p68LastEnd = now;
            rec.p68IsRunning = false;
            Factory.p68StopWatchBL.Save(rec);
        }

        private int NewRowOrdinary(StopkyViewModel v)
        {
            int x = v.Rows.Count + 1;
            if (v.Rows.Count > 0)
            {
                var max = v.Rows.Max(r => r.p68Ordinary);
                if (max >= x) x = max + 1;
            }
            return x;
        }
    }
}
