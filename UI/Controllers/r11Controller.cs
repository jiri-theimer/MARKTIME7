using Microsoft.AspNetCore.Mvc;
using UI.Models.r11;

namespace UI.Controllers
{
    //Kapacitní plánování: stropy hodin osob na projektech (r11) a jejich rozplánování do bloků (r12).
    //Vytížení = naplánované hodiny proti pracovnímu fondu osoby; rozpad na dny se počítá zde, neukládá se.
    public class r11Controller : BaseController
    {
        private const string FMT = "yyyy-MM-dd";

        private bool TryDate(string s, out DateTime d)
        {
            return DateTime.TryParseExact(s, FMT, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out d);
        }
        private bool TryNum(string s, out double n)     //čísla chodí s tečkou i čárkou, nezávisle na jazyku aplikace
        {
            n = 0;
            if (string.IsNullOrWhiteSpace(s)) return true;
            return double.TryParse(s.Trim().Replace(" ", "").Replace(",", "."), System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out n);
        }
        private string TakeNotifyMessage()   //chybová hláška z BL jde do odpovědi, ne do notifikace při příštím načtení stránky
        {
            string s = Factory.GetFirstNotifyMessage();
            Factory.CurrentUser.Messages4Notify = null;
            return string.IsNullOrEmpty(s) ? Factory.tra("Záznam se nepodařilo uložit.") : s;
        }
        private IActionResult Fail(string strMessage)
        {
            return Json(new { ok = false, message = strMessage });
        }

        public IActionResult Index(int p41id)
        {
            if (!Factory.CurrentUser.j04IsModule_r01)
            {
                return this.StopPage(false, "Nemáte oprávnění pro tento Modul.");
            }
            if (!Factory.r11CapacityBL.IsAvailable())
            {
                return this.StopPage(false, "V databázi chybí tabulky kapacitního plánování (r11CapacityLimit, r12CapacityPlan).");
            }
            var v = new r11ViewModel() { p41id = p41id };
            if (v.p41id > 0)
            {
                v.RecP41 = Factory.p41ProjectBL.Load(v.p41id);
                if (v.RecP41 == null || !Factory.p41ProjectBL.InhaleRecDisposition(v.p41id, v.RecP41).ReadAccess)
                {
                    return this.StopPage(false, "Projekt nebyl nalezen nebo k němu nemáte přístup.");
                }
                v.CanEditContext = Factory.r11CapacityBL.CanEdit(v.p41id);
                v.NewP41ID = v.p41id;
                v.NewP41Name = v.RecP41.p41Name;
            }
            v.Zoom = Factory.CBL.LoadUserParam("r11-zoom", "week");
            v.Months = Factory.CBL.LoadUserParamInt("r11-months", 4);
            if (v.Months < 1 || v.Months > 12) v.Months = 4;

            return View(v);
        }

        public IActionResult Data(int p41id, string d1, int months)
        {
            if (!Factory.CurrentUser.j04IsModule_r01 || !Factory.r11CapacityBL.IsAvailable())
            {
                return Fail(Factory.tra("Nemáte oprávnění pro tento Modul."));
            }
            if (months < 1 || months > 12) months = 4;
            DateTime datStart;
            if (!TryDate(d1, out datStart)) datStart = DateTime.Today;
            datStart = new DateTime(datStart.Year, datStart.Month, 1);
            DateTime datEnd = datStart.AddMonths(months).AddDays(-1);
            int intDays = (int)(datEnd - datStart).TotalDays + 1;

            var lisAll = Factory.r11CapacityBL.GetList(0, 0).ToList();
            if (p41id > 0)
            {
                var recRoot = Factory.p41ProjectBL.Load(p41id);
                if (recRoot == null || !Factory.p41ProjectBL.InhaleRecDisposition(p41id, recRoot).ReadAccess)
                {
                    return Fail(Factory.tra("Projekt nebyl nalezen nebo k němu nemáte přístup."));
                }
                var j02ids = lisAll.Where(p => p.p41ID == p41id).Select(p => p.j02ID).ToHashSet();
                lisAll = lisAll.Where(p => j02ids.Contains(p.j02ID)).ToList();  //lidé projektu se všemi svými projekty: vytížení se počítá ze všech
            }

            //které projekty přihlášený vidí: neviditelné se započítají do vytížení, ale jejich řádky se neukážou
            HashSet<int> visible = null;
            bool bolOwnerAll = Factory.CurrentUser.IsAdmin || Factory.CurrentUser.TestPermission(BO.PermValEnum.GR_p41_Owner);
            if (!bolOwnerAll && !Factory.CurrentUser.TestPermission(BO.PermValEnum.GR_p41_Reader) && lisAll.Count > 0)
            {
                var mq = new BO.myQueryP41("p41") { pids = lisAll.Select(p => p.p41ID).Distinct().ToList(), IsRecordValid = null };
                visible = Factory.p41ProjectBL.GetList(mq).Select(p => p.pid).ToHashSet();
            }

            var blocks = Factory.r11CapacityBL.GetList_r12(0, 0).ToLookup(p => p.r11ID);
            var actuals = Factory.r11CapacityBL.GetList_Actuals(0, 0).ToDictionary(p => p.p41ID.ToString() + "-" + p.j02ID.ToString());
            DateTime datMin = datStart.AddYears(-3), datMax = datEnd.AddYears(3);      //mez pro rozpad dlouhých bloků
            var fondCache = new Dictionary<string, Dictionary<DateTime, double>>();
            var people = new List<r11Person>();

            foreach (var grp in lisAll.GroupBy(p => p.j02ID))
            {
                var first = grp.First();
                var person = new r11Person() { j02id = first.j02ID, name = first.Person, cap = new double[intDays], plan = new double[intDays], rows = new List<r11Row>() };

                //úseky k rozložení: bloky, nebo celý strop v období projektu
                var segs = new List<(DateTime s, DateTime e, double h)>();
                foreach (var c in grp)
                {
                    var lisB = blocks[c.pid].ToList();
                    bool bolSpread = false;
                    if (lisB.Count > 0)
                    {
                        foreach (var b in lisB) segs.Add((b.r12Start.Date, b.r12End.Date, b.r12Hours));
                    }
                    else if (c.p41PlanFrom != null && c.p41PlanUntil != null && Convert.ToDateTime(c.p41PlanUntil).Date >= Convert.ToDateTime(c.p41PlanFrom).Date)
                    {
                        bolSpread = true;
                        segs.Add((Convert.ToDateTime(c.p41PlanFrom).Date, Convert.ToDateTime(c.p41PlanUntil).Date, c.r11HoursTotal));
                    }

                    if (visible != null && !visible.Contains(c.p41ID))
                    {
                        person.hidden += 1;
                        continue;
                    }
                    var row = new r11Row()
                    {
                        r11id = c.pid, p41id = c.p41ID, project = c.Project, code = c.p41Code, client = c.Client, text = c.r11Text,
                        fa = c.r11HoursFa, nefa = c.r11HoursNeFa, total = c.r11HoursTotal,
                        planned = Math.Round(lisB.Sum(p => p.r12Hours), 2), spread = bolSpread, ctx = (p41id > 0 && c.p41ID == p41id),
                        edit = (bolOwnerAll || c.j02ID_ProjectOwner == Factory.CurrentUser.pid) ? 1 : 2,
                        blocks = lisB.Select(p => new r11Block() { id = p.pid, d1 = p.r12Start.ToString(FMT), d2 = p.r12End.ToString(FMT), hours = p.r12Hours, text = p.r12Text }).ToList()
                    };
                    if (c.p41PlanFrom != null) row.pf = Convert.ToDateTime(c.p41PlanFrom).ToString(FMT);
                    if (c.p41PlanUntil != null) row.pu = Convert.ToDateTime(c.p41PlanUntil).ToString(FMT);
                    if (actuals.TryGetValue(c.p41ID.ToString() + "-" + c.j02ID.ToString(), out BO.r11Actual act))
                    {
                        row.actfa = Math.Round(act.HoursFa, 2); row.actnefa = Math.Round(act.HoursNeFa, 2);
                    }
                    person.rows.Add(row);
                }

                //pracovní fond osoby pro okno i pro celé úseky, které do okna zasahují
                DateTime f1 = datStart, f2 = datEnd;
                foreach (var g in segs.Where(p => p.e >= datStart && p.s <= datEnd))
                {
                    if (g.s < f1) f1 = g.s;
                    if (g.e > f2) f2 = g.e;
                }
                if (f1 < datMin) f1 = datMin;
                if (f2 > datMax) f2 = datMax;
                string strKey = $"{first.c21ID}|{first.j02CountryCode}";
                if (!fondCache.TryGetValue(strKey, out Dictionary<DateTime, double> fond) || !fond.ContainsKey(f1) || !fond.ContainsKey(f2))
                {
                    fond = Factory.r11CapacityBL.GetFondPerDay(first.c21ID, first.j02CountryCode, f1, f2);
                    fondCache[strKey] = fond;
                }
                for (int i = 0; i < intDays; i++)
                {
                    person.cap[i] = Math.Round(fond[datStart.AddDays(i)], 2);
                }

                //hodiny úseku se rozloží do jeho dnů v poměru pracovního fondu; úsek bez fondu rovnoměrně
                foreach (var g in segs.Where(p => p.e >= datStart && p.s <= datEnd))
                {
                    DateTime s = g.s < f1 ? f1 : g.s, e = g.e > f2 ? f2 : g.e;
                    double dblSum = 0; int intCount = 0;
                    for (var d = s; d <= e; d = d.AddDays(1)) { dblSum += fond[d]; intCount += 1; }
                    if (intCount == 0) continue;
                    for (var d = (s < datStart ? datStart : s); d <= e && d <= datEnd; d = d.AddDays(1))
                    {
                        double dblPart = dblSum > 0 ? g.h * fond[d] / dblSum : g.h / intCount;
                        person.plan[(int)(d - datStart).TotalDays] += dblPart;
                    }
                }
                for (int i = 0; i < intDays; i++) person.plan[i] = Math.Round(person.plan[i], 2);

                person.rows = person.rows.OrderByDescending(p => p.ctx).ThenBy(p => p.project).ToList();
                if (person.rows.Count > 0 || person.hidden > 0) people.Add(person);
            }

            return Json(new
            {
                ok = true, start = datStart.ToString(FMT), days = intDays, today = DateTime.Today.ToString(FMT),
                people = people.OrderBy(p => p.name).ToList()
            });
        }

        [HttpPost]
        public IActionResult SaveLimit(int r11id, int p41id, int j02id, string fa, string nefa, string text)
        {
            if (!Factory.CurrentUser.j04IsModule_r01) return Fail(Factory.tra("Nemáte oprávnění pro tento Modul."));
            if (!TryNum(fa, out double dblFa) || !TryNum(nefa, out double dblNefa)) return Fail(Factory.tra("Hodiny nejsou číslo."));

            Factory.CurrentUser.Messages4Notify = null;
            var rec = new BO.r11CapacityLimit();
            if (r11id > 0)
            {
                rec = Factory.r11CapacityBL.Load(r11id);
                if (rec == null) return Fail(Factory.tra("Záznam nebyl nalezen."));
            }
            else
            {
                rec.p41ID = p41id; rec.j02ID = j02id;
            }
            rec.r11HoursFa = dblFa; rec.r11HoursNeFa = dblNefa;
            rec.r11Text = string.IsNullOrWhiteSpace(text) ? null : (text.Trim().Length > 300 ? text.Trim().Substring(0, 300) : text.Trim());
            int intPID = Factory.r11CapacityBL.SaveLimit(rec);
            if (intPID == 0) return Fail(TakeNotifyMessage());
            return Json(new { ok = true, pid = intPID });
        }

        [HttpPost]
        public IActionResult DeleteLimit(int r11id)
        {
            if (!Factory.CurrentUser.j04IsModule_r01) return Fail(Factory.tra("Nemáte oprávnění pro tento Modul."));
            Factory.CurrentUser.Messages4Notify = null;
            if (!Factory.r11CapacityBL.DeleteLimit(r11id)) return Fail(TakeNotifyMessage());
            return Json(new { ok = true });
        }

        [HttpPost]
        public IActionResult SaveBlock(int r12id, int r11id, string d1, string d2, string hours, string text)
        {
            if (!Factory.CurrentUser.j04IsModule_r01) return Fail(Factory.tra("Nemáte oprávnění pro tento Modul."));
            if (!TryDate(d1, out DateTime dat1) || !TryDate(d2, out DateTime dat2) || dat2 < dat1) return Fail(Factory.tra("Neplatné období."));
            if (!TryNum(hours, out double dblHours)) return Fail(Factory.tra("Hodiny nejsou číslo."));

            Factory.CurrentUser.Messages4Notify = null;
            var rec = new BO.r12CapacityPlan() { r11ID = r11id };
            if (r12id > 0)
            {
                rec = Factory.r11CapacityBL.Load_r12(r12id);
                if (rec == null) return Fail(Factory.tra("Záznam nebyl nalezen."));
                if (text == null) text = rec.r12Text;       //posun myší text neposílá
                if (string.IsNullOrWhiteSpace(hours)) dblHours = rec.r12Hours;  //posun myší hodiny nemění
            }
            rec.r12Start = dat1; rec.r12End = dat2; rec.r12Hours = dblHours;
            rec.r12Text = string.IsNullOrWhiteSpace(text) ? null : (text.Trim().Length > 300 ? text.Trim().Substring(0, 300) : text.Trim());
            int intPID = Factory.r11CapacityBL.SaveBlock(rec);
            if (intPID == 0) return Fail(TakeNotifyMessage());
            return Json(new { ok = true, pid = intPID });
        }

        [HttpPost]
        public IActionResult DeleteBlock(int r12id)
        {
            if (!Factory.CurrentUser.j04IsModule_r01) return Fail(Factory.tra("Nemáte oprávnění pro tento Modul."));
            Factory.CurrentUser.Messages4Notify = null;
            if (!Factory.r11CapacityBL.DeleteBlock(r12id)) return Fail(TakeNotifyMessage());
            return Json(new { ok = true });
        }
    }
}
