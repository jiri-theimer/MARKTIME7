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
        public BO.AI.AiAskTable ComputeTable(BO.AI.AiWidgetTask task, List<string> errors)
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
                var t = ComputeTable(task, err);
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

        private string Html(BO.x55Widget rec, BO.AI.AiWidgetTask task, BO.AI.AiAskTable t)
        {
            var sb = new StringBuilder();
            sb.Append("<div class='aiw'>");
            if (!string.IsNullOrEmpty(t.Description))
                sb.Append($"<div class='aiw-desc'>{E(t.Description)}</div>");

            if (t.Rows.Count == 0)
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
                if (table) sb.Append(Table(task, t, rec.x55Code));
                if (t.Truncated)
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
