using ClosedXML.Excel;
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
            v.ViewMode = Factory.CBL.LoadUserParam("r11-view", "people") == "projects" ? "projects" : "people";
            v.Heat = Factory.CBL.LoadUserParam("r11-heat", "plan") == "actual" ? "actual" : "plan";
            v.lisJ11 = Factory.j11TeamBL.GetList(new BO.myQueryJ11()).OrderBy(p => p.j11Name).ToList();
            v.lisJ18 = Factory.j18CostUnitBL.GetList(new BO.myQuery("j18")).OrderBy(p => p.j18Name).ToList();
            v.j11ID = Factory.CBL.LoadUserParamInt("r11-j11id", 0);
            v.j18ID = Factory.CBL.LoadUserParamInt("r11-j18id", 0);
            if (!v.lisJ11.Any(p => p.pid == v.j11ID)) v.j11ID = 0;
            if (!v.lisJ18.Any(p => p.pid == v.j18ID)) v.j18ID = 0;

            return View(v);
        }

        //Sestaví vytížení: lidé se stropem, jejich projekty a rozpad naplánovaných hodin na dny zobrazeného období.
        //Vrací null a strError, když přihlášený nemá k datům přístup.
        private List<r11Person> BuildTimeline(int p41id, string d1, ref int months, int j11id, int j18id, out DateTime datStart, out int intDays, out string strError)
        {
            strError = null; datStart = DateTime.Today; intDays = 0;
            if (!Factory.CurrentUser.j04IsModule_r01 || !Factory.r11CapacityBL.IsAvailable())
            {
                strError = Factory.tra("Nemáte oprávnění pro tento Modul."); return null;
            }
            if (months < 1 || months > 12) months = 4;
            if (!TryDate(d1, out datStart)) datStart = DateTime.Today;
            datStart = new DateTime(datStart.Year, datStart.Month, 1);
            DateTime datEnd = datStart.AddMonths(months).AddDays(-1);
            intDays = (int)(datEnd - datStart).TotalDays + 1;
            DateTime datFrom = datStart;    //kopie pro lambda výrazy (out parametr v nich použít nejde)

            var lisAll = Factory.r11CapacityBL.GetList(0, 0).ToList();
            if (p41id > 0)
            {
                var recRoot = Factory.p41ProjectBL.Load(p41id);
                if (recRoot == null || !Factory.p41ProjectBL.InhaleRecDisposition(p41id, recRoot).ReadAccess)
                {
                    strError = Factory.tra("Projekt nebyl nalezen nebo k němu nemáte přístup."); return null;
                }
                var j02ids = lisAll.Where(p => p.p41ID == p41id).Select(p => p.j02ID).ToHashSet();
                lisAll = lisAll.Where(p => j02ids.Contains(p.j02ID)).ToList();  //lidé projektu se všemi svými projekty: vytížení se počítá ze všech
            }
            if (j18id > 0) lisAll = lisAll.Where(p => p.j18ID == j18id).ToList();      //filtr: středisko osoby
            if (j11id > 0)                                                              //filtr: tým
            {
                var team = Factory.r11CapacityBL.GetTeamPersonIds(j11id).ToHashSet();
                lisAll = lisAll.Where(p => team.Contains(p.j02ID)).ToList();
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

            //1. průchod: úseky k rozložení pro každý strop (bloky, nebo celý strop v období projektu) a rozsah dnů, pro který je potřeba kapacita
            var work = new List<(IGrouping<int, BO.r11CapacityLimit> grp, Dictionary<int, List<(DateTime s, DateTime e, double h)>> segs, HashSet<int> spread, DateTime f1, DateTime f2)>();
            DateTime datAbs1 = datStart, datAbs2 = datEnd;
            foreach (var grp in lisAll.GroupBy(p => p.j02ID))
            {
                var segs = new Dictionary<int, List<(DateTime s, DateTime e, double h)>>();
                var spread = new HashSet<int>();
                DateTime f1 = datStart, f2 = datEnd;
                foreach (var c in grp)
                {
                    var lis = new List<(DateTime s, DateTime e, double h)>();
                    var lisB = blocks[c.pid].ToList();
                    if (lisB.Count > 0)
                    {
                        foreach (var b in lisB) lis.Add((b.r12Start.Date, b.r12End.Date, b.r12Hours));
                    }
                    else if (c.p41PlanFrom != null && c.p41PlanUntil != null && Convert.ToDateTime(c.p41PlanUntil).Date >= Convert.ToDateTime(c.p41PlanFrom).Date)
                    {
                        spread.Add(c.pid);
                        lis.Add((Convert.ToDateTime(c.p41PlanFrom).Date, Convert.ToDateTime(c.p41PlanUntil).Date, c.r11HoursTotal));
                    }
                    lis = lis.Where(p => p.e >= datFrom && p.s <= datEnd).ToList();     //jen úseky, které zasahují do zobrazeného období
                    foreach (var g in lis)
                    {
                        if (g.s < f1) f1 = g.s;
                        if (g.e > f2) f2 = g.e;
                    }
                    segs[c.pid] = lis;
                }
                if (f1 < datMin) f1 = datMin;
                if (f2 > datMax) f2 = datMax;
                if (f1 < datAbs1) datAbs1 = f1;
                if (f2 > datAbs2) datAbs2 = f2;
                work.Add((grp, segs, spread, f1, f2));
            }

            //absence všech lidí jedním dotazem: snižují kapacitu dne a na takový den se plán nerozkládá
            var absences = new Dictionary<int, Dictionary<DateTime, double>>();
            if (work.Count > 0)
            {
                foreach (var a in Factory.r11CapacityBL.GetList_Absences(datAbs1, datAbs2))
                {
                    if (!absences.TryGetValue(a.j02ID, out Dictionary<DateTime, double> dic))
                    {
                        dic = new Dictionary<DateTime, double>();
                        absences[a.j02ID] = dic;
                    }
                    dic[a.Datum.Date] = a.Hours;
                }
            }

            //skutečnost po dnech zobrazeného období: klíč "p41-j02"
            var actdays = new Dictionary<string, double[]>();
            if (work.Count > 0)
            {
                foreach (var a in Factory.r11CapacityBL.GetList_ActualsPerDay(datStart, datEnd))
                {
                    int i = (int)(a.Datum.Date - datFrom).TotalDays;
                    if (i < 0 || i >= intDays) continue;
                    string strK = a.p41ID.ToString() + "-" + a.j02ID.ToString();
                    if (!actdays.TryGetValue(strK, out double[] arr))
                    {
                        arr = new double[intDays];
                        actdays[strK] = arr;
                    }
                    arr[i] += a.Hours;
                }
            }
            //2. průchod: kapacita a rozpad plánu na dny
            foreach (var w in work)
            {
                var grp = w.grp;
                var first = grp.First();
                DateTime f1 = w.f1, f2 = w.f2;
                var person = new r11Person() { j02id = first.j02ID, name = first.Person, cap = new double[intDays], abs = new double[intDays], plan = new double[intDays], act = new double[intDays], rows = new List<r11Row>() };

                string strKey = $"{first.c21ID}|{first.j02CountryCode}";
                if (!fondCache.TryGetValue(strKey, out Dictionary<DateTime, double> fond) || !fond.ContainsKey(f1) || !fond.ContainsKey(f2))
                {
                    fond = Factory.r11CapacityBL.GetFondPerDay(first.c21ID, first.j02CountryCode, f1, f2);
                    fondCache[strKey] = fond;
                }
                absences.TryGetValue(first.j02ID, out Dictionary<DateTime, double> abs);
                double Net(DateTime d)      //kapacita dne = pracovní fond minus absence
                {
                    double dbl = fond[d];
                    if (abs != null && abs.TryGetValue(d, out double dblAbs)) dbl -= dblAbs;
                    return dbl < 0 ? 0 : dbl;
                }
                for (int i = 0; i < intDays; i++)
                {
                    DateTime d = datFrom.AddDays(i);
                    person.cap[i] = Math.Round(Net(d), 2);
                    if (abs != null && abs.TryGetValue(d, out double dblA)) person.abs[i] = Math.Round(dblA, 2);
                }

                //hodiny úseku se rozloží do jeho dnů v poměru kapacity; úsek bez kapacity rovnoměrně
                foreach (var c in grp)
                {
                    var rowplan = new double[intDays];
                    foreach (var g in w.segs[c.pid])
                    {
                        DateTime s = g.s < f1 ? f1 : g.s, e = g.e > f2 ? f2 : g.e;
                        double dblSum = 0; int intCount = 0;
                        for (var d = s; d <= e; d = d.AddDays(1)) { dblSum += Net(d); intCount += 1; }
                        if (intCount == 0) continue;
                        for (var d = (s < datFrom ? datFrom : s); d <= e && d <= datEnd; d = d.AddDays(1))
                        {
                            rowplan[(int)(d - datFrom).TotalDays] += dblSum > 0 ? g.h * Net(d) / dblSum : g.h / intCount;
                        }
                    }
                    for (int i = 0; i < intDays; i++) person.plan[i] += rowplan[i];
                    string strPair = c.p41ID.ToString() + "-" + c.j02ID.ToString();
                    actdays.TryGetValue(strPair, out double[] rowact);
                    if (rowact != null)
                    {
                        for (int i = 0; i < intDays; i++) { rowact[i] = Math.Round(rowact[i], 2); person.act[i] += rowact[i]; }
                    }

                    if (visible != null && !visible.Contains(c.p41ID))
                    {
                        person.hidden += 1;
                        continue;
                    }
                    for (int i = 0; i < intDays; i++) rowplan[i] = Math.Round(rowplan[i], 2);
                    var lisB = blocks[c.pid].ToList();
                    var row = new r11Row()
                    {
                        r11id = c.pid, p41id = c.p41ID, project = c.Project, code = c.p41Code, client = c.Client, text = c.r11Text,
                        fa = c.r11HoursFa, nefa = c.r11HoursNeFa, total = c.r11HoursTotal,
                        planned = Math.Round(lisB.Sum(p => p.r12Hours), 2), spread = w.spread.Contains(c.pid), ctx = (p41id > 0 && c.p41ID == p41id),
                        edit = (bolOwnerAll || c.j02ID_ProjectOwner == Factory.CurrentUser.pid) ? 1 : 2,
                        blocks = lisB.Select(p => new r11Block() { id = p.pid, d1 = p.r12Start.ToString(FMT), d2 = p.r12End.ToString(FMT), hours = p.r12Hours, text = p.r12Text }).ToList(),
                        dayplan = rowplan, dayact = rowact ?? new double[intDays]
                    };
                    if (c.p41PlanFrom != null) row.pf = Convert.ToDateTime(c.p41PlanFrom).ToString(FMT);
                    if (c.p41PlanUntil != null) row.pu = Convert.ToDateTime(c.p41PlanUntil).ToString(FMT);
                    if (actuals.TryGetValue(strPair, out BO.r11Actual act))
                    {
                        row.actfa = Math.Round(act.HoursFa, 2); row.actnefa = Math.Round(act.HoursNeFa, 2);
                    }
                    person.rows.Add(row);
                }
                for (int i = 0; i < intDays; i++) { person.plan[i] = Math.Round(person.plan[i], 2); person.act[i] = Math.Round(person.act[i], 2); }

                person.rows = person.rows.OrderByDescending(p => p.ctx).ThenBy(p => p.project).ToList();
                if (person.rows.Count > 0 || person.hidden > 0) people.Add(person);
            }

            return people.OrderBy(p => p.name).ToList();
        }

        public IActionResult Data(int p41id, string d1, int months, int j11id, int j18id)
        {
            var people = BuildTimeline(p41id, d1, ref months, j11id, j18id, out DateTime datStart, out int intDays, out string strError);
            if (people == null) return Fail(strError);

            return Json(new { ok = true, start = datStart.ToString(FMT), days = intDays, today = DateTime.Today.ToString(FMT), people = people });
        }

        //Export do XLSX: list "Vytížení" (kapacita, plán, volno, absence a vykázané hodiny osob + plán po projektech ve sloupcích podle měřítka) a list "Bloky".
        public IActionResult Export(int p41id, string d1, int months, string zoom, int j11id, int j18id)
        {
            var people = BuildTimeline(p41id, d1, ref months, j11id, j18id, out DateTime datStart, out int intDays, out string strError);
            if (people == null) return this.StopPage(false, strError);
            if (zoom != "day" && zoom != "month") zoom = "week";
            DateTime datEnd = datStart.AddDays(intDays - 1);

            //sloupce období: den / týden (od pondělí) / měsíc, oříznuté na zobrazené období
            var bks = new List<(int i1, int i2, string label)>();
            int x = 0;
            while (x < intDays)
            {
                DateTime d = datStart.AddDays(x);
                int to;
                if (zoom == "day") to = x;
                else if (zoom == "week") to = x + 6 - (((int)d.DayOfWeek + 6) % 7);
                else to = x + DateTime.DaysInMonth(d.Year, d.Month) - d.Day;
                if (to > intDays - 1) to = intDays - 1;
                DateTime de = datStart.AddDays(to);
                string strLabel = zoom == "day" ? d.ToString("dd.MM.yyyy") : (zoom == "week" ? d.ToString("dd.MM.") + "–" + de.ToString("dd.MM.yy") : d.ToString("MM/yyyy"));
                bks.Add((x, to, strLabel));
                x = to + 1;
            }
            double Sum(double[] arr, int i1, int i2)
            {
                double dbl = 0;
                for (int i = i1; i <= i2; i++) dbl += arr[i];
                return Math.Round(dbl, 2);
            }

            using var wb = new XLWorkbook();
            var ws = wb.Worksheets.Add("Vytížení");
            ws.Cell(1, 1).Value = Factory.tra("Kapacitní plán") + " " + datStart.ToString("dd.MM.yyyy") + " – " + datEnd.ToString("dd.MM.yyyy");
            ws.Cell(1, 1).Style.Font.Bold = true;
            ws.Cell(1, 1).Style.Font.FontSize = 13;

            string[] heads = { "Osoba", "Projekt", "Kód", "Klient", "Řádek", "Strop Fa", "Strop Nefa", "Strop", "Rozplánováno", "Vykázáno Fa", "Vykázáno Nefa", "Zbývá" };
            int intFix = heads.Length, r = 3;
            for (int c = 0; c < intFix; c++) ws.Cell(r, c + 1).Value = Factory.tra(heads[c]);
            for (int c = 0; c < bks.Count; c++) ws.Cell(r, intFix + 1 + c).Value = bks[c].label;
            var hrng = ws.Range(r, 1, r, intFix + bks.Count);
            hrng.Style.Font.Bold = true;
            hrng.Style.Fill.BackgroundColor = XLColor.FromHtml("#EDEDED");
            r += 1;

            foreach (var p in people)
            {
                //souhrnné řádky osoby (kapacita = pracovní fond minus absence)
                string[] kinds = { "Kapacita", "Plán", "Volno", "Absence", "Vykázáno" };
                for (int k = 0; k < 5; k++)
                {
                    ws.Cell(r, 1).Value = p.name ?? "";
                    ws.Cell(r, 5).Value = Factory.tra(kinds[k]);
                    if (k == 1)
                    {
                        ws.Cell(r, 6).Value = Math.Round(p.rows.Sum(q => q.fa), 2);
                        ws.Cell(r, 7).Value = Math.Round(p.rows.Sum(q => q.nefa), 2);
                        ws.Cell(r, 8).Value = Math.Round(p.rows.Sum(q => q.total), 2);
                        ws.Cell(r, 10).Value = Math.Round(p.rows.Sum(q => q.actfa), 2);
                        ws.Cell(r, 11).Value = Math.Round(p.rows.Sum(q => q.actnefa), 2);
                    }
                    for (int c = 0; c < bks.Count; c++)
                    {
                        double dblCap = Sum(p.cap, bks[c].i1, bks[c].i2), dblPlan = Sum(p.plan, bks[c].i1, bks[c].i2);
                        var cell = ws.Cell(r, intFix + 1 + c);
                        if (k == 0) cell.Value = dblCap;
                        if (k == 2) cell.Value = Math.Round(dblCap - dblPlan, 2);
                        if (k == 3)
                        {
                            double dblAbs = Sum(p.abs, bks[c].i1, bks[c].i2);
                            if (dblAbs > 0.004) cell.Value = dblAbs;
                        }
                        if (k == 4)
                        {
                            double dblAct = Sum(p.act, bks[c].i1, bks[c].i2);
                            if (dblAct > 0.004) cell.Value = dblAct;
                        }
                        if (k == 1)
                        {
                            cell.Value = dblPlan;
                            if (dblPlan > 0.049)    //stejné barvy jako na obrazovce: volno / plno od 80 % / přetíženo
                            {
                                string strColor = dblPlan > dblCap + 0.049 ? "#FFC9C9" : (dblCap > 0 && dblPlan >= dblCap * 0.8 ? "#FFE9A8" : "#D9F2E3");
                                cell.Style.Fill.BackgroundColor = XLColor.FromHtml(strColor);
                            }
                        }
                        if (k == 2 && dblCap - dblPlan < -0.049) cell.Style.Font.FontColor = XLColor.FromHtml("#B42318");
                    }
                    ws.Range(r, 1, r, intFix + bks.Count).Style.Font.Bold = true;
                    r += 1;
                }
                //projekty osoby
                foreach (var row in p.rows)
                {
                    ws.Cell(r, 1).Value = p.name ?? "";
                    ws.Cell(r, 2).Value = row.project ?? "";
                    ws.Cell(r, 3).Value = row.code ?? "";
                    ws.Cell(r, 4).Value = row.client ?? "";
                    ws.Cell(r, 5).Value = Factory.tra("Projekt");
                    ws.Cell(r, 6).Value = row.fa;
                    ws.Cell(r, 7).Value = row.nefa;
                    ws.Cell(r, 8).Value = row.total;
                    if (row.blocks.Count > 0) ws.Cell(r, 9).Value = row.planned;
                    ws.Cell(r, 10).Value = row.actfa;
                    ws.Cell(r, 11).Value = row.actnefa;
                    double dblRest = Math.Round(row.total - row.actfa - row.actnefa, 2);     //zbývá vykázat do stropu; záporné = strop překročen
                    ws.Cell(r, 12).Value = dblRest;
                    if (dblRest < -0.049) ws.Cell(r, 12).Style.Font.FontColor = XLColor.FromHtml("#B42318");
                    if (row.total > 0 && row.actfa + row.actnefa > row.total + 0.049)
                    {
                        ws.Range(r, 10, r, 11).Style.Fill.BackgroundColor = XLColor.FromHtml("#FFC9C9");    //vykázáno nad strop
                    }
                    for (int c = 0; c < bks.Count; c++)
                    {
                        double dbl = Sum(row.dayplan, bks[c].i1, bks[c].i2);
                        if (dbl > 0.004) ws.Cell(r, intFix + 1 + c).Value = dbl;
                    }
                    r += 1;
                }
            }
            if (r > 4)
            {
                ws.Range(4, 6, r - 1, intFix + bks.Count).Style.NumberFormat.Format = "0.##";
            }
            ws.Columns(1, intFix).AdjustToContents();
            ws.Columns(intFix + 1, intFix + bks.Count).Width = zoom == "day" ? 11 : (zoom == "week" ? 15 : 9);
            ws.SheetView.FreezeRows(3);
            ws.SheetView.FreezeColumns(5);

            //list s bloky rozplánování
            var ws2 = wb.Worksheets.Add("Bloky");
            string[] heads2 = { "Osoba", "Projekt", "Kód", "Klient", "Od", "Do", "Hodiny", "Poznámka" };
            for (int c = 0; c < heads2.Length; c++) ws2.Cell(1, c + 1).Value = Factory.tra(heads2[c]);
            ws2.Range(1, 1, 1, heads2.Length).Style.Font.Bold = true;
            ws2.Range(1, 1, 1, heads2.Length).Style.Fill.BackgroundColor = XLColor.FromHtml("#EDEDED");
            int r2 = 2;
            foreach (var p in people)
            {
                foreach (var row in p.rows)
                {
                    foreach (var b in row.blocks)
                    {
                        ws2.Cell(r2, 1).Value = p.name ?? "";
                        ws2.Cell(r2, 2).Value = row.project ?? "";
                        ws2.Cell(r2, 3).Value = row.code ?? "";
                        ws2.Cell(r2, 4).Value = row.client ?? "";
                        if (TryDate(b.d1, out DateTime b1)) ws2.Cell(r2, 5).Value = b1;
                        if (TryDate(b.d2, out DateTime b2)) ws2.Cell(r2, 6).Value = b2;
                        ws2.Cell(r2, 7).Value = b.hours;
                        ws2.Cell(r2, 8).Value = b.text ?? "";
                        r2 += 1;
                    }
                }
            }
            if (r2 > 2)
            {
                ws2.Range(2, 5, r2 - 1, 6).Style.DateFormat.Format = "dd.MM.yyyy";
                ws2.Range(2, 7, r2 - 1, 7).Style.NumberFormat.Format = "0.##";
            }
            ws2.Columns(1, heads2.Length).AdjustToContents();
            ws2.SheetView.FreezeRows(1);

            using var ms = new MemoryStream();
            wb.SaveAs(ms);
            string strFileName = "Kapacitni_plan_" + datStart.ToString("yyyyMM") + "_" + DateTime.Now.ToString("yyyyMMdd_HHmm") + ".xlsx";
            return File(ms.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", strFileName);
        }

        //název projektu pro combo v panelu nového stropu (combo samo ukazuje první sloupec své nabídky)
        public IActionResult ProjectName(int p41id)
        {
            var rec = p41id > 0 ? Factory.p41ProjectBL.Load(p41id) : null;
            if (rec == null || !Factory.p41ProjectBL.InhaleRecDisposition(p41id, rec).ReadAccess) return Fail(null);
            string s = rec.p41Name + (string.IsNullOrEmpty(rec.p41Code) ? "" : " (" + rec.p41Code + ")");
            if (!string.IsNullOrEmpty(rec.Client)) s = rec.Client + " - " + s;
            return Json(new { ok = true, name = s });
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
