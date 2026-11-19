using System.Globalization;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json.Nodes;

namespace UI.Code
{
    /// <summary>
    /// Vykreslení AI widgetu (x55Widget.x55AiTask) na dashboardu: zadání → AiAggregate (stejná cesta jako Pivot-Explorer) → HTML.
    /// Bez volání AI. Katalog sloupců se staví pro přihlášeného uživatele → oprávnění podle toho, kdo widget vidí.
    /// HTML má inline styly u grafu, aby fungovalo i „Odeslat e-mailem“ (_html2mail posílá outerHTML).
    /// </summary>
    public class AiWidgetRenderer
    {
        public const int MaxAggRows = 20;
        public const int MaxListRows = 15;
        public const int MaxChartBars = 12;
        /// <summary>Grafy přes Chart.js (wwwroot/lib/chartjs, lokálně). Data jdou do &lt;script type="application/json"&gt;, kreslí widgets.js.</summary>
        public const bool ChartsEnabled = true;
        private const int MaxChartCategories = 12;
        private const int MaxChartSeries = 3;
        private const int MaxGroups = 5000;
        /// <summary>Souhrn s více dimenzemi: tolik řádků nejnižší úrovně se načte, aby mezisoučty skupin byly úplné.</summary>
        private const int MaxGroupedRows = 2000;
        /// <summary>Nejvýše tolik řádků pod jednou skupinou první úrovně (zbytek jako „… a další N“).</summary>
        private const int MaxGroupChildren = 50;
        /// <summary>Do tolika řádků je tabulka se skupinami rozbalená, nad to začíná sbalená na první úroveň.</summary>
        private const int MaxExpandedRows = 30;

        private readonly BL.Factory _f;
        private readonly BL.TheColumnsProvider _cp;
        private readonly BL.Singleton.ThePeriodProvider _pp;
        private readonly HtmlEncoder _enc;
        private static readonly CultureInfo _cz = new CultureInfo("cs-CZ");

        public AiWidgetRenderer(BL.Factory f, BL.TheColumnsProvider cp, BL.Singleton.ThePeriodProvider pp, HtmlEncoder enc)
        {
            _f = f; _cp = cp; _pp = pp; _enc = enc;
        }

        private string E(object o) => o == null ? "" : _enc.Encode(o.ToString());

        /// <summary>Zadání s dosazeným pojmenovaným obdobím (tento měsíc…) k dnešnímu dni.</summary>
        public void ApplyPeriod(BO.AI.AiWidgetTask task)
        {
            var spec = task.Spec;
            if (task.PeriodPid <= 0 || spec?.Period == null) return;
            var per = _pp.getPallete().FirstOrDefault(p => p.pid == task.PeriodPid);
            if (per?.d1 == null || per.d2 == null) return;
            spec.Period.D1 = per.d1.Value.ToString("yyyy-MM-dd");
            spec.Period.D2 = per.d2.Value.ToString("yyyy-MM-dd");
        }

        /// <summary>Pojmenované období, které právě odpovídá datům zadání (pro volbu „vždy aktuální“ při připíchnutí); null = žádné.</summary>
        public BO.ThePeriod MatchNamedPeriod(BO.AI.AiQuerySpec spec)
        {
            if (spec?.Period == null) return null;
            if (!BL.AI.Grid.AiGridFilterCodec.TryDate(spec.Period.D1, out var d1) || !BL.AI.Grid.AiGridFilterCodec.TryDate(spec.Period.D2, out var d2)) return null;
            return _pp.getPallete().FirstOrDefault(p => p.pid > 1 && p.pid <= 60 && p.d1 == d1.Date && p.d2 == d2.Date);
        }

        /// <summary>Výsledek zadání jako tabulka (stejný formát jako v AiAsk). Chyby zadání → errors, tabulka null.</summary>
        /// <param name="aggRows">Souhrn: kolik řádků vrátit místo limitu zadání (0 = podle zadání). Pro tabulku s mezisoučty jsou potřeba všechny řádky.</param>
        public BO.AI.AiAskTable ComputeTable(BO.AI.AiWidgetTask task, List<string> errors, int aggRows = 0)
        {
            var agg = new BL.AI.Tools.AiAggregate(_f, _cp);
            var cat = agg.Catalog(task.Spec.Entity);
            if (task.Kind == "list")
            {
                var s = task.List;
                s.Limit = Math.Clamp(s.Limit <= 0 ? MaxListRows : s.Limit, 1, MaxListRows);
                var err = agg.Validate(s, cat);
                if (err.Count > 0) { errors.AddRange(err); return null; }
                var plan = agg.Build(s, cat);
                var (rows, totals) = agg.RunList(plan, s.Limit);
                return agg.ToTable(s, plan, rows, totals);
            }
            else
            {
                var s = task.Agg;
                s.Limit = Math.Clamp(s.Limit <= 0 ? MaxAggRows : s.Limit, 1, MaxAggRows);
                var err = agg.Validate(s, cat);
                if (err.Count > 0) { errors.AddRange(err); return null; }
                var plan = agg.Build(s, cat);
                var dt = agg.Run(plan, MaxGroups);
                if (aggRows > 0)
                {
                    int intLimit = s.Limit;
                    s.Limit = aggRows;      //jen pro převod na tabulku; limit zadání zůstává
                    var tAll = agg.ToTable(s, plan, dt);
                    s.Limit = intLimit;
                    return tAll;
                }
                return agg.ToTable(s, plan, dt);
            }
        }

        public string Render(BO.x55Widget rec)
        {
            var task = BO.AI.AiWidgetTask.Parse(rec.x55AiTask);
            if (task == null) return Msg(_f.tra("Zadání AI widgetu je poškozené."));
            ApplyPeriod(task);
            try
            {
                var err = new List<string>();
                var t = ComputeTable(task, err, IsMultiDim(task) ? MaxGroupedRows : 0);
                if (t == null) return Unavailable(err);
                return Html(rec, task, t);
            }
            catch (Exception ex)
            {
                return Msg(_f.tra("Widget se nepodařilo spočítat.") + " " + ex.Message);
            }
        }

        /// <summary>
        /// Zadání ve formátu vstupu nástroje agregace / hledej (id sloupců z katalogu) – pro úpravu widgetu v „Zeptej se MARKTIME“:
        /// model dostane současné zadání a upraví jen to, o co uživatel požádá.
        /// </summary>
        public JsonObject ToToolInput(BO.AI.AiWidgetTask task)
        {
            var s = task.Spec;
            var cat = new BL.AI.Tools.AiAggregate(_f, _cp).Catalog(s.Entity);
            string Id(string key) => key == BL.AI.Tools.AiAggregate.Count ? key : (cat.ByKey(key)?.Id ?? key);
            JsonArray Arr(IEnumerable<string> keys) => new JsonArray(keys.Select(k => (JsonNode)JsonValue.Create(Id(k))).ToArray());

            var o = new JsonObject { ["entita"] = s.Entity, ["nadpis"] = s.Title };
            if (task.Kind == "list") o["sloupce"] = Arr(task.List.Columns);
            else { o["dimenze"] = Arr(task.Agg.Dims); o["meritka"] = Arr(task.Agg.Measures); }

            var f = new JsonArray();
            foreach (var x in s.Filter ?? new())
                f.Add(new JsonObject
                {
                    ["sloupec"] = Id(x.Column), ["op"] = x.Op,
                    ["hodnoty"] = new JsonArray((x.Values ?? new()).Select(v => (JsonNode)JsonValue.Create(v)).ToArray()),
                    ["od"] = x.From, ["do"] = x.To
                });
            o["filtry"] = f;
            if (s.Period != null) o["obdobi"] = new JsonObject { ["pole"] = s.Period.Field, ["od"] = s.Period.D1, ["do"] = s.Period.D2 };
            if (s.State != null) o["stav"] = s.State.Value;
            if (!string.IsNullOrEmpty(s.Tab)) o["format"] = s.Tab;
            o["archiv"] = s.Archive;
            if (s.SortBy != null) o["razeni"] = new JsonObject { ["podle"] = Id(s.SortBy), ["sestupne"] = s.SortDesc };
            o["limit"] = s.Limit;
            return o;
        }

        // ------------------------------------------------------------------ HTML

        private string Html(BO.x55Widget rec, BO.AI.AiWidgetTask task, BO.AI.AiAskTable tAll)
        {
            //souhrn s více dimenzemi má načtené všechny řádky (kvůli mezisoučtům); graf a prostá tabulka pracují s limitem zadání
            bool multi = IsMultiDim(task);
            bool grouped = multi && !tAll.Truncated;    //neúplné řádky = mezisoučty by lhaly → prostá tabulka
            var t = multi ? Limited(tAll, Math.Clamp(task.Agg.Limit <= 0 ? MaxAggRows : task.Agg.Limit, 1, MaxAggRows)) : tAll;
            var sb = new StringBuilder();
            sb.Append("<div class='aiw'>");
            if (!string.IsNullOrEmpty(t.Description))
                sb.Append($"<div class='aiw-desc'>{E(t.Description)}</div>");

            if (task.Kind != "list" && task.Agg.Dims.Count == 0 && t.Rows.Count > 0)
            {
                sb.Append(Kpi(task, t));        //souhrn bez dimenzí = jedno číslo → KPI dlaždice
            }
            else if (t.Rows.Count == 0)
            {
                sb.Append($"<div class='aiw-empty'>{E(_f.tra("Nic nenalezeno."))}</div>");
            }
            else
            {
                bool chart = ChartsEnabled && task.Kind != "list" && task.View != "table" && t.Columns.Count >= 2 && !t.Columns[0].Measure && t.Columns.Any(c => c.Measure);
                if (chart && IsTimeSeries(t) && t.Truncated)
                {
                    //časová řada oříznutá na N „největších“ období by měla díry → přepočítat s řazením podle období (nejnovější)
                    var again = BO.AI.AiWidgetTask.Parse(task.ToJson());
                    ApplyPeriod(again);
                    again.Agg.SortBy = again.Agg.Dims[0];
                    again.Agg.SortDesc = true;
                    var t2 = ComputeTable(again, new List<string>());
                    if (t2 != null) t = t2;
                }
                bool table = !chart || task.View != "chart";
                if (chart) sb.Append(Chart(t));
                if (table && grouped)
                {
                    sb.Append(GroupedTable(task, tAll, rec.x55Code, out int intShown, out int intAll));
                    if (intAll > intShown)
                    {
                        sb.Append($"<div class='aiw-desc'>{E(string.Format(_f.tra("Zobrazeno {0} z {1} řádků, celkem je spočítáno ze všech."), intShown, intAll.ToString("N0", _cz)))}</div>");
                    }
                }
                else if (table) sb.Append(Table(task, t, rec.x55Code));
                if (t.Truncated && !(table && grouped))
                {
                    string m = task.Kind == "list" ? _f.tra("Zobrazeno {0} z {1} záznamů.") : _f.tra("Zobrazeno {0} z {1} řádků, celkem je spočítáno ze všech.");
                    sb.Append($"<div class='aiw-desc'>{E(string.Format(m, t.Rows.Count, t.Groups.ToString("N0", _cz)))}</div>");
                }
            }

            //patička: otevřít v Pivot-Exploreru / zdrojové záznamy
            sb.Append("<div class='aiw-foot'>");
            if (task.Kind != "list")
                sb.Append($"<a href=\"javascript:aiw_pivot({rec.pid})\"><span class='material-icons-outlined-btn'>functions</span>{E(_f.tra("Pivot-Explorer"))}</a>");
            sb.Append($"<span class='aiw-ai' title='{E(_f.tra("Widget vytvořila AI; data se počítají přímo z databáze, bez volání AI."))}'><span class='material-icons-outlined-btn'>auto_awesome</span>AI</span>");
            sb.Append("</div>");
            sb.Append("</div>");
            return sb.ToString();
        }

        // ------------------------------------------------------------------ KPI (souhrn bez dimenzí)

        /// <summary>
        /// Velká čísla (max. 3 měřítka) a porovnání s předchozím obdobím stejné délky – stejné zadání, jen posunuté období
        /// (jeden SQL dotaz navíc, bez AI). Bez období v zadání se porovnání nezobrazí.
        /// </summary>
        private string Kpi(BO.AI.AiWidgetTask task, BO.AI.AiAskTable t)
        {
            var row = t.Rows[0];
            var measures = t.Columns.Select((c, i) => (c, i)).Where(x => x.c.Measure).Take(MaxChartSeries).ToList();

            object[] prevRow = null;
            string prevLabel = null;
            var prev = PreviousPeriod(task.Agg.Period);
            if (prev != null)
            {
                try
                {
                    var pt = BO.AI.AiWidgetTask.Parse(task.ToJson());
                    pt.PeriodPid = 0;
                    pt.Agg.Period.D1 = prev.Value.d1.ToString("yyyy-MM-dd");
                    pt.Agg.Period.D2 = prev.Value.d2.ToString("yyyy-MM-dd");
                    var tp = ComputeTable(pt, new List<string>());
                    if (tp != null && tp.Rows.Count > 0) prevRow = tp.Rows[0];
                    prevLabel = $"{prev.Value.d1:d.M.} – {prev.Value.d2:d.M.yyyy}";
                }
                catch { prevRow = null; }
            }

            var sb = new StringBuilder();
            sb.Append($"<div class='aiw-kpis aiw-kpis-{measures.Count}'>");
            foreach (var m in measures)
            {
                double v = D(row[m.i]);
                sb.Append("<div class='aiw-kpi'>");
                sb.Append($"<div class='aiw-kpi-name'>{E(m.c.Name)}</div>");
                sb.Append($"<div class='aiw-kpi-value'>{E(KpiFmt(row[m.i], m.c))}</div>");
                if (prevRow != null)
                {
                    double p = D(prevRow[m.i]);
                    string cmp;
                    if (Math.Abs(p) < 0.000001) cmp = $"<span class='aiw-kpi-flat'>{E(_f.tra("předchozí období"))}: 0</span>";
                    else
                    {
                        double pct = (v - p) / Math.Abs(p) * 100;
                        string arrow = pct > 0.05 ? "▲" : (pct < -0.05 ? "▼" : "•");
                        string cls = pct > 0.05 ? "aiw-kpi-up" : (pct < -0.05 ? "aiw-kpi-down" : "aiw-kpi-flat");
                        cmp = $"<span class='{cls}'>{arrow} {E(Math.Abs(pct).ToString("N0", _cz))} %</span> <span class='aiw-kpi-prev'>{E(_f.tra("proti"))} {E(KpiFmt(prevRow[m.i], m.c))}</span>";
                    }
                    sb.Append($"<div class='aiw-kpi-cmp' title='{E(_f.tra("Předchozí období"))}: {E(prevLabel)}'>{cmp}</div>");
                }
                sb.Append("</div>");
            }
            sb.Append("</div>");
            return sb.ToString();
        }

        /// <summary>KPI číslo: od 1 000 bez desetinných míst (velké částky se jinak nevejdou).</summary>
        private string KpiFmt(object v, BO.AI.AiAskColumn c)
        {
            double d = D(v);
            return d.ToString("N" + (Math.Abs(d) >= 1000 ? 0 : c.Decimals), _cz);
        }

        /// <summary>
        /// Předchozí období stejné délky: celé měsíce od 1. dne (i rozpracovaný měsíc / rok k dnešku) → posun o stejný počet měsíců
        /// (listopad 1.–9. → říjen 1.–9.; Q4 → Q3; rok → rok); jinak posun o počet dní. null = zadání bez období / otevřené období.
        /// </summary>
        public static (DateTime d1, DateTime d2)? PreviousPeriod(BO.AI.AiAggPeriod per)
        {
            if (per == null || !BL.AI.Grid.AiGridFilterCodec.TryDate(per.D1, out var d1) || !BL.AI.Grid.AiGridFilterCodec.TryDate(per.D2, out var d2)) return null;
            if (d1.Year < 1990 || d2.Year > 2090 || d2 < d1) return null;     //otevřené meze (2000-01-01 / 2099-12-31) nemají „předchozí“
            if (d1.Day == 1)
            {
                int months = (d2.Year * 12 + d2.Month) - (d1.Year * 12 + d1.Month) + 1;
                bool fullLastMonth = d2.Day == DateTime.DaysInMonth(d2.Year, d2.Month);
                var p1 = d1.AddMonths(-months);
                var p2 = fullLastMonth ? p1.AddMonths(months).AddDays(-1) : d2.AddMonths(-months);
                return (p1, p2);
            }
            int days = (d2 - d1).Days + 1;
            return (d1.AddDays(-days), d1.AddDays(-1));
        }

        /// <summary>
        /// Data grafu pro Chart.js: časová dimenze → sloupce (≤ 12 bodů) / čára, kategorie → vodorovné pruhy (top 12 + „Ostatní“).
        /// Až 3 měřítka jako řady. JSON v &lt;script type="application/json"&gt; (jQuery .html() ho nespouští), výchozí encoder escapuje &lt;.
        /// </summary>
        private string Chart(BO.AI.AiAskTable t)
        {
            var measures = t.Columns.Select((c, i) => (c, i)).Where(x => x.c.Measure).Take(MaxChartSeries).ToList();
            var dims = t.Columns.Select((c, i) => (c, i)).Where(x => !x.c.Measure).ToList();
            if (measures.Count == 0 || dims.Count == 0 || t.Rows.Count == 0) return "";

            string Label(object[] r) => string.Join(" / ", dims.Select(d => Fmt(r[d.i], d.c) ?? "–"));
            bool time = IsTimeSeries(t);
            var rows = t.Rows.ToList();
            var labels = new List<string>();
            var series = measures.Select(m => new List<double>()).ToList();
            bool other = false;

            if (time)
            {
                rows = rows.OrderBy(r => TimeKey(r[dims[0].i])).ToList();
                foreach (var r in rows) { labels.Add(Label(r)); for (int k = 0; k < measures.Count; k++) series[k].Add(D(r[measures[k].i])); }
            }
            else
            {
                var top = rows.Take(MaxChartCategories).ToList();
                foreach (var r in top) { labels.Add(Label(r)); for (int k = 0; k < measures.Count; k++) series[k].Add(D(r[measures[k].i])); }
                //zbytek do „Ostatní“: celkem (ze všech skupin) − zobrazené
                if ((t.Groups > top.Count) && t.Total != null)
                {
                    var rest = measures.Select((m, k) => t.Total[m.i] == null ? 0 : D(t.Total[m.i]) - series[k].Sum()).ToList();
                    if (rest.Any(v => Math.Abs(v) > 0.000001))
                    {
                        labels.Add(_f.tra("Ostatní"));
                        for (int k = 0; k < measures.Count; k++) series[k].Add(Math.Round(rest[k], measures[k].c.Decimals));
                        other = true;
                    }
                }
            }

            string type = time ? (labels.Count > MaxChartCategories ? "line" : "bar") : "bar";
            bool horizontal = !time;
            int height = horizontal ? Math.Clamp(labels.Count * 24 + 50, 120, 380) : 230;

            var cfg = new
            {
                type,
                horizontal,
                other,
                labels,
                series = measures.Select((m, k) => new { name = m.c.Name, decimals = m.c.Decimals, data = series[k] })
            };
            string json = System.Text.Json.JsonSerializer.Serialize(cfg);     //výchozí encoder: < > & jako \u003C… (nelze ukončit <script>)
            return $"<div class='aiw-chartjs' style='position:relative;height:{height}px;margin:2px 0 10px;'><canvas></canvas><script type='application/json'>{json}</script></div>";
        }

        /// <summary>První dimenze je čas: typ date nebo hodnoty ve tvaru rok-měsíc / měsíc.rok.</summary>
        private static bool IsTimeSeries(BO.AI.AiAskTable t)
        {
            int di = t.Columns.FindIndex(c => !c.Measure);
            if (di < 0 || t.Rows.Count == 0) return false;
            if (t.Columns[di].Type == "date") return true;
            var vals = t.Rows.Select(r => r[di]?.ToString()).Where(v => !string.IsNullOrEmpty(v)).ToList();
            return vals.Count > 0 && vals.All(v => TimeKey(v) != null);
        }

        /// <summary>Řaditelný klíč období: 2026-11(-05), 11.2026, 11/2026, 2026/11, 2026 → "2026-11-05"; null = není období.</summary>
        private static string TimeKey(object o)
        {
            string v = o?.ToString()?.Trim();
            if (string.IsNullOrEmpty(v)) return null;
            var m = System.Text.RegularExpressions.Regex.Match(v, @"^(\d{4})[-/](\d{1,2})(?:[-/](\d{1,2}))?");
            if (m.Success) return $"{m.Groups[1].Value}-{int.Parse(m.Groups[2].Value):00}-{(m.Groups[3].Success ? int.Parse(m.Groups[3].Value) : 1):00}";
            m = System.Text.RegularExpressions.Regex.Match(v, @"^(\d{1,2})[./](\d{4})$");
            if (m.Success) return $"{m.Groups[2].Value}-{int.Parse(m.Groups[1].Value):00}-01";
            m = System.Text.RegularExpressions.Regex.Match(v, @"^(\d{1,2})\.(\d{1,2})\.(\d{4})$");
            if (m.Success) return $"{m.Groups[3].Value}-{int.Parse(m.Groups[2].Value):00}-{int.Parse(m.Groups[1].Value):00}";
            if (System.Text.RegularExpressions.Regex.IsMatch(v, @"^(19|20)\d{2}$")) return v + "-01-01";
            return null;
        }

        private string Table(BO.AI.AiWidgetTask task, BO.AI.AiAskTable t, string tableId)
        {
            var sb = new StringBuilder();
            sb.Append($"<table id='{E(tableId)}' class='table table-sm aiw-table'><thead><tr>");
            foreach (var c in t.Columns)
                sb.Append($"<th{(c.Type == "number" ? " class='text-end'" : "")}>{E(c.Name)}</th>");
            sb.Append("</tr></thead><tbody>");

            for (int i = 0; i < t.Rows.Count; i++)
            {
                var r = t.Rows[i];
                sb.Append("<tr>");
                for (int k = 0; k < t.Columns.Count; k++)
                {
                    var c = t.Columns[k];
                    string txt = E(Fmt(r[k], c));
                    //odkaz na kartu: výpis = 1. sloupec záznamu, souhrn = sloupec s odkazem (projekt, klient…)
                    if (task.Kind == "list" && k == 0 && t.Ids != null && i < t.Ids.Count && t.Ids[i] > 0)
                        txt = Link(t.Entity, t.Ids[i], txt);
                    else if (c.Link != null && t.LinkIds != null && i < t.LinkIds.Count && t.LinkIds[i] != null && t.LinkIds[i][k] > 0)
                        txt = Link(c.Link, t.LinkIds[i][k], txt);
                    sb.Append($"<td{(c.Type == "number" ? " class='text-end'" : "")}>{txt}</td>");
                }
                sb.Append("</tr>");
            }
            sb.Append("</tbody>");

            if (t.Total != null && t.Total.Any(x => x != null))
            {
                sb.Append("<tfoot><tr>");
                for (int k = 0; k < t.Columns.Count; k++)
                {
                    string txt = k == 0 && t.Total[0] == null ? E(_f.tra("Celkem")) : E(Fmt(t.Total[k], t.Columns[k]));
                    sb.Append($"<td{(t.Columns[k].Type == "number" ? " class='text-end'" : "")}>{txt}</td>");
                }
                sb.Append("</tr></tfoot>");
            }
            sb.Append("</table>");
            return sb.ToString();
        }

        // ------------------------------------------------------------------ tabulka s mezisoučty (souhrn s více dimenzemi)

        private static bool IsMultiDim(BO.AI.AiWidgetTask task) => task.Kind != "list" && task.Agg != null && task.Agg.Dims.Count >= 2;

        /// <summary>Stejná tabulka omezená na prvních n řádků (graf, prostá tabulka).</summary>
        private static BO.AI.AiAskTable Limited(BO.AI.AiAskTable t, int n)
        {
            return new BO.AI.AiAskTable
            {
                Id = t.Id, Kind = t.Kind, Title = t.Title, Description = t.Description, Entity = t.Entity, Columns = t.Columns,
                Rows = t.Rows.Take(n).ToList(), Total = t.Total, Groups = t.Groups, Ids = t.Ids,
                LinkIds = t.LinkIds?.Take(n).ToList(), Truncated = t.Groups > n, Guid = t.Guid, CanDrill = t.CanDrill
            };
        }

        private class GRow
        {
            public object[] V;      //hodnoty řádku (u skupiny: popisek dimenze + mezisoučty)
            public int[] L;         //ID záznamů pro odkazy
            public int Level;       //úroveň = index dimenze, od které řádek ukazuje hodnoty
            public bool IsGroup;    //řádek mezisoučtu
            public int Top;         //pořadí skupiny první úrovně
            public int More;        //>0: řádek „… a další N“
        }

        /// <summary>
        /// Souhrn s více dimenzemi jako tabulka se skupinami: řádky se seskupí podle první dimenze (pak druhé…). Každá hodnota první
        /// dimenze má řádek souhrnu (i prázdná hodnota a i skupina s jediným řádkem); na dalších úrovních dostane mezisoučet jen
        /// skupina s více řádky, skupina s jediným řádkem zůstane jedním řádkem. Měřítka jsou součty,
        /// takže mezisoučet = součet řádků skupiny. Skupiny první úrovně jdou kliknutím sbalit (widgets.js).
        /// </summary>
        private string GroupedTable(BO.AI.AiWidgetTask task, BO.AI.AiAskTable t, string tableId, out int shownGroups, out int allGroups)
        {
            var s = task.Agg;
            int nd = t.Columns.TakeWhile(c => !c.Measure).Count();      //dimenze jsou první sloupce
            int nc = t.Columns.Count;

            //řazení jako v zadání: podle měřítka (výchozí první měřítko sestupně) nebo podle dimenze
            int si = -1; bool desc = s.SortDesc;
            if (s.SortBy == BL.AI.Tools.AiAggregate.Count) si = nc - 1;
            else if (s.SortBy != null)
            {
                int di = s.Dims.IndexOf(s.SortBy);
                int mi = s.Measures.Where(x => x != BL.AI.Tools.AiAggregate.Count).ToList().IndexOf(s.SortBy);
                si = di >= 0 ? di : (mi >= 0 ? nd + mi : -1);
            }
            int firstMeasure = t.Columns.FindIndex(c => c.Measure);
            if (si < 0 || si >= nc) { si = firstMeasure; desc = true; }

            var src = t.Rows.Select((r, i) => new GRow { V = r, L = (t.LinkIds != null && i < t.LinkIds.Count) ? t.LinkIds[i] : null }).ToList();
            var output = new List<GRow>();
            int top = 0; int all = 0; int limit = Math.Clamp(s.Limit <= 0 ? MaxAggRows : s.Limit, 1, MaxAggRows);

            void Emit(List<GRow> rows, int level)
            {
                var groups = rows.GroupBy(r => r.V[level]?.ToString() ?? "")     //podle zobrazeného popisku, stejně jako Pivot-Explorer
                    .Select(g =>
                    {
                        var lis = g.ToList();
                        var sum = new object[nc];
                        sum[level] = lis[0].V[level];
                        for (int k = 0; k < nc; k++)
                            if (t.Columns[k].Measure) sum[k] = Math.Round(lis.Sum(x => D(x.V[k])), t.Columns[k].Decimals);
                        return (rows: lis, sum);
                    }).ToList();

                //pořadí skupin: podle mezisoučtu měřítka, podle popisku vlastní dimenze, jinak podle prvního měřítka sestupně
                if (si >= nd) groups = (desc ? groups.OrderByDescending(g => D(g.sum[si])) : groups.OrderBy(g => D(g.sum[si]))).ToList();
                else if (si == level) groups = (desc ? groups.OrderByDescending(g => g.sum[level]?.ToString(), StringComparer.Create(_cz, true)) : groups.OrderBy(g => g.sum[level]?.ToString(), StringComparer.Create(_cz, true))).ToList();
                else if (firstMeasure >= 0) groups = groups.OrderByDescending(g => D(g.sum[firstMeasure])).ToList();

                if (level == 0)
                {
                    all = groups.Count;
                    groups = groups.Take(limit).ToList();
                }
                foreach (var g in groups)
                {
                    if (level == 0) top++;
                    if ((g.rows.Count == 1 && level > 0) || level >= nd - 1)     //první úroveň má řádek souhrnu vždy, i s jediným řádkem – jinak by vypadal jako součást předchozí skupiny
                    {
                        foreach (var r in g.rows) output.Add(new GRow { V = r.V, L = r.L, Level = level, Top = top });
                    }
                    else
                    {
                        var links = new int[nc];
                        var ids = g.rows.Where(x => x.L != null && x.L[level] > 0).Select(x => x.L[level]).Distinct().ToList();
                        if (ids.Count == 1) links[level] = ids[0];     //odkaz u skupiny jen když všechny její řádky míří na stejný záznam
                        output.Add(new GRow { V = g.sum, L = links, Level = level, IsGroup = true, Top = top });
                        Emit(g.rows, level + 1);
                    }
                }
            }
            Emit(src, 0);
            shownGroups = top; allGroups = all;

            //nejvýše MaxGroupChildren řádků pod jednou skupinou první úrovně
            var final = new List<GRow>();
            foreach (var grp in output.GroupBy(r => r.Top))
            {
                var lis = grp.ToList();
                if (lis.Count > MaxGroupChildren + 1)
                {
                    final.AddRange(lis.Take(MaxGroupChildren + 1));
                    final.Add(new GRow { Level = 1, Top = grp.Key, More = lis.Count - MaxGroupChildren - 1 });
                }
                else final.AddRange(lis);
            }
            bool closed = final.Count > MaxExpandedRows && final.Any(r => r.IsGroup && r.Level == 0);

            var sb = new StringBuilder();
            sb.Append($"<table id='{E(tableId)}' class='table table-sm aiw-table aiw-grouped'><thead><tr>");
            foreach (var c in t.Columns)
                sb.Append($"<th{(c.Type == "number" ? " class='text-end'" : "")}>{E(c.Name)}</th>");
            sb.Append("</tr></thead><tbody>");

            foreach (var r in final)
            {
                bool topGroup = r.IsGroup && r.Level == 0;
                bool child = r.Level > 0;
                string cls = topGroup ? "aiw-g" + (closed ? " aiw-closed" : "") : (child ? "aiw-c" + (r.IsGroup ? " aiw-s" : "") : "aiw-one");
                string bold = r.IsGroup ? " style='font-weight:600;'" : "";     //inline kvůli odeslání e-mailem
                sb.Append($"<tr class='{cls}' data-g='{r.Top}'{bold}{(child && closed ? " hidden" : "")}>");
                if (r.More > 0)
                {
                    sb.Append($"<td></td><td colspan='{nc - 1}' class='aiw-more'>{E(_f.tra("… a další"))} {r.More}</td></tr>");
                    continue;
                }
                for (int k = 0; k < nc; k++)
                {
                    var c = t.Columns[k];
                    string txt = "";
                    bool show = c.Measure || (r.IsGroup ? k == r.Level : k >= r.Level);
                    if (show)
                    {
                        txt = E(Fmt(r.V[k], c));
                        if (!c.Measure && c.Link != null && r.L != null && r.L[k] > 0) txt = Link(c.Link, r.L[k], txt);
                        if (!c.Measure && txt == "") txt = r.IsGroup ? E(_f.tra("(prázdné)")) : "–";    //skupina bez hodnoty má vlastní pojmenovaný souhrn
                    }
                    string tdcls = c.Type == "number" ? "text-end" : "";
                    if (k == 0 && r.Level == 0) tdcls = (tdcls + " aiw-top").Trim();
                    sb.Append($"<td{(tdcls != "" ? $" class='{tdcls}'" : "")}>{txt}</td>");
                }
                sb.Append("</tr>");
            }
            sb.Append("</tbody>");

            if (t.Total != null && t.Total.Any(x => x != null))
            {
                sb.Append("<tfoot><tr>");
                for (int k = 0; k < nc; k++)
                {
                    string txt = k == 0 && t.Total[0] == null ? E(_f.tra("Celkem")) : E(Fmt(t.Total[k], t.Columns[k]));
                    sb.Append($"<td{(t.Columns[k].Type == "number" ? " class='text-end'" : "")}>{txt}</td>");
                }
                sb.Append("</tr></tfoot>");
            }
            sb.Append("</table>");
            return sb.ToString();
        }

        private static string Link(string prefix, int pid, string html)
        {
            string href = prefix == "p31" ? $"javascript:_edit('p31',{pid})" : $"/Record/RecPage?prefix={prefix}&pid={pid}";
            string target = prefix == "p31" ? "" : " target='_top'";
            return $"<a href=\"{href}\"{target}>{html}</a>";
        }

        private string Fmt(object v, BO.AI.AiAskColumn c)
        {
            if (v == null) return null;
            switch (c.Type)
            {
                case "number":
                    return D(v).ToString("N" + c.Decimals, _cz);
                case "date":
                    return DateTime.TryParse(v.ToString(), CultureInfo.InvariantCulture, DateTimeStyles.None, out var d) ? d.ToString("d.M.yyyy") : v.ToString();
                case "bool":
                    string s = v.ToString();
                    return s == "1" || s.Equals("true", StringComparison.OrdinalIgnoreCase) ? _f.tra("ano") : _f.tra("ne");
                default:
                    return v.ToString();
            }
        }

        private static double D(object v)
        {
            if (v == null) return 0;
            try { return Convert.ToDouble(v, CultureInfo.InvariantCulture); } catch { return 0; }
        }

        private string Unavailable(List<string> err) =>
            Msg(_f.tra("Widget teď nelze zobrazit – zadání odkazuje na údaje, které nejsou dostupné (oprávnění nebo změna sloupců).") + " " + string.Join(" ", err.Take(2)));

        private string Msg(string text) => $"<div class='aiw-msg'>{E(text)}</div>";
    }
}
