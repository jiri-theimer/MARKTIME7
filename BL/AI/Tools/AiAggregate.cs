using System.Data;
using System.Globalization;
using BL.AI.Core;
using BL.AI.Grid;
using BO.AI;

namespace BL.AI.Tools
{
    /// <summary>
    /// Provedení dotazů nástrojů nad entitou – stejná cesta jako grid a Pivot-Explorer:
    /// myQuery entity (oprávnění MyRecordsDisponible, období, stav, formát, archiv, sloupcový filtr) +
    /// gridBL.GetGroupByList (agregace) nebo gridBL.GetGridTable (výpis záznamů).
    /// Používají ji nástroje „agregace“ a „hledej“ i AiAskController (Pivot-Explorer, zdrojové úkony).
    /// </summary>
    public class AiAggregate
    {
        public const string Count = "pocet";
        public const string CountHeader = "Počet záznamů";
        public const int MaxDims = 4;
        public const int MaxLimit = 300;
        public const int MaxListColumns = 15;
        public const int MaxListLimit = 200;

        private readonly BL.Factory _f;
        private readonly BL.TheColumnsProvider _cp;

        public AiAggregate(BL.Factory f, BL.TheColumnsProvider cp) { _f = f; _cp = cp; }

        public AiColumnCatalog Catalog(string prefix) => AiColumnCatalog.Build(_f, _cp, prefix, AiColumnCatalog.CatalogMode.Grid);

        /// <summary>Sestavený dotaz agregace: sloupce výsledku v pořadí dimenze → měřítka.</summary>
        public class Plan
        {
            public BO.baseQuery Query { get; set; }
            public List<BO.TheGridColumn> Dims { get; } = new();
            public List<BO.TheGridColumn> Measures { get; } = new();
            /// <summary>Dimenze + měřítka (bez počtu záznamů) – sloupce pro Pivot-Explorer a drill.</summary>
            public List<BO.TheGridColumn> Display => Dims.Concat(Measures).ToList();
            public bool HasCount { get; set; }
            public string GroupBy { get; set; }
        }

        /// <summary>Sestavený dotaz výpisu záznamů.</summary>
        public class ListPlan
        {
            public BO.baseQuery Query { get; set; }
            public List<BO.TheGridColumn> Columns { get; } = new();
        }

        // ------------------------------------------------------------------ validace

        /// <summary>Společné části zadání: entita, filtr, období, stav, formát, archiv, limit.</summary>
        private void ValidateCommon(AiQuerySpec s, AiColumnCatalog cat, List<string> err, int maxLimit)
        {
            var u = _f.CurrentUser;
            AiGridFilterCodec.Validate(s.Filter, cat, err);
            if (s.Period != null)
            {
                var fields = AiToolEntities.PeriodFields(s.Entity);
                if (!fields.Any(p => p.Id == s.Period.Field)) err.Add($"Období: pole '{s.Period.Field}' není povolené (povoleno: {string.Join(", ", fields.Select(p => p.Id))}).");
                if (!AiGridFilterCodec.TryDate(s.Period.D1, out var d1) || !AiGridFilterCodec.TryDate(s.Period.D2, out var d2)) err.Add("Období: od a do musí být datum yyyy-MM-dd.");
                else if (d1 > d2) err.Add("Období: od je větší než do.");
            }
            if (s.State != null)
            {
                if (!AiToolEntities.HasState(s.Entity, u)) err.Add("Tato entita nemá filtr stavu úkonů.");
                else if (!BL.AI.Totals.AiTotalsService.States.ContainsKey(s.State.Value)) err.Add($"Neplatný stav úkonů: {s.State}");
            }
            if (!string.IsNullOrEmpty(s.Tab))
            {
                if (!AiToolEntities.HasTab(s.Entity, u)) err.Add("Tato entita nemá filtr formátu úkonů.");
                else if (!BL.AI.Totals.AiTotalsValidator.Tabs.Contains(s.Tab)) err.Add($"Neplatný formát úkonů: {s.Tab}");
            }
            if (!AiToolEntities.ArchiveValues(s.Entity).Contains(s.Archive)) err.Add($"Neplatná hodnota archivu: {s.Archive}");
            if (s.Limit < 1 || s.Limit > maxLimit) err.Add($"Limit musí být 1–{maxLimit}.");
        }

        public List<string> Validate(AiAggSpec s, AiColumnCatalog cat)
        {
            var err = new List<string>();
            if (AiToolEntities.Get(s.Entity, _f.CurrentUser) == null) { err.Add($"Entita '{s.Entity}' není dostupná."); return err; }

            if (s.Dims.Count > MaxDims) err.Add($"Nejvýše {MaxDims} dimenze.");
            foreach (var k in s.Dims)
            {
                var e = cat.ByKey(k);
                if (e == null) err.Add($"Neznámá dimenze: {k}");
                else if (e.Sum) err.Add($"'{e.Header}' je měřítko (M), ne dimenze.");
                else if (e.NotForTotals) err.Add($"Podle sloupce '{e.Header}' nelze seskupovat.");
            }
            if (s.Dims.Distinct(StringComparer.OrdinalIgnoreCase).Count() != s.Dims.Count) err.Add("Dimenze se opakuje.");
            if (s.Measures.Count == 0) err.Add("Chybí měřítko (alespoň \"pocet\").");
            foreach (var k in s.Measures.Where(p => p != Count))
            {
                var e = cat.ByKey(k);
                if (e == null) err.Add($"Neznámé měřítko: {k}");
                else if (!e.Sum) err.Add($"'{e.Header}' není sčítatelné měřítko (role M).");
            }
            if (s.Measures.Distinct(StringComparer.OrdinalIgnoreCase).Count() != s.Measures.Count) err.Add("Měřítko se opakuje.");
            if (s.SortBy != null && s.SortBy != Count && !s.Dims.Contains(s.SortBy) && !s.Measures.Contains(s.SortBy)) err.Add("Řazení musí být podle dimenze nebo měřítka výsledku.");
            ValidateCommon(s, cat, err, MaxLimit);
            return err;
        }

        public List<string> Validate(AiListSpec s, AiColumnCatalog cat)
        {
            var err = new List<string>();
            if (AiToolEntities.Get(s.Entity, _f.CurrentUser) == null) { err.Add($"Entita '{s.Entity}' není dostupná."); return err; }
            if (s.Columns.Count == 0) err.Add("Chybí sloupce výpisu.");
            if (s.Columns.Count > MaxListColumns) err.Add($"Nejvýše {MaxListColumns} sloupců.");
            foreach (var k in s.Columns) if (cat.ByKey(k) == null) err.Add($"Neznámý sloupec: {k}");
            if (s.Columns.Distinct(StringComparer.OrdinalIgnoreCase).Count() != s.Columns.Count) err.Add("Sloupec se opakuje.");
            if (s.SortBy != null)
            {
                var e = cat.ByKey(s.SortBy);
                if (e == null) err.Add($"Neznámý sloupec řazení: {s.SortBy}");
                else if (!s.Columns.Contains(s.SortBy, StringComparer.OrdinalIgnoreCase)) err.Add("Řadit lze jen podle sloupce výpisu.");
            }
            ValidateCommon(s, cat, err, MaxListLimit);
            return err;
        }

        // ------------------------------------------------------------------ dotaz

        /// <summary>Sloupce podle klíčů (ParseTheGridColumns vrací nové instance – úpravy neovlivní singleton) s názvy z katalogu.</summary>
        private Dictionary<string, BO.TheGridColumn> Columns(string prefix, List<string> keys, AiColumnCatalog cat, out List<BO.TheGridColumn> cols)
        {
            cols = keys.Count == 0 ? new List<BO.TheGridColumn>() : _cp.ParseTheGridColumns(prefix, string.Join(",", keys), _f);
            foreach (var c in cols) c.Header = cat.ByKey(c.UniqueName)?.Header ?? c.Header;     // názvy jako v katalogu (s entitou relace)
            return cols.GroupBy(p => p.UniqueName, StringComparer.OrdinalIgnoreCase).ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase);
        }

        private static BO.TheGridColumn Col(Dictionary<string, BO.TheGridColumn> byKey, string k) =>
            byKey.TryGetValue(k, out var c) ? c : throw new InvalidOperationException($"Sloupec {k} není dostupný.");

        /// <summary>myQuery entity se společnými filtry zadání.</summary>
        private BO.baseQuery Query(AiQuerySpec s, AiColumnCatalog cat, List<BO.TheGridColumn> cols)
        {
            var mq = new BO.InitMyQuery(_f.CurrentUser).Load(s.Entity);
            mq.explicit_columns = cols;          // i sloupce jen pro filtr – kvůli JOIN relací
            mq.MyRecordsDisponible = true;
            mq.IsRecordValid = s.Archive switch { 2 => false, 0 => null, _ => true };

            string j75 = AiGridFilterCodec.ToJ75(s.Filter, cat);
            if (!string.IsNullOrEmpty(j75))
            {
                mq.TheGridFilter = _cp.ParseAdhocFilterFromString(j75, cols);
                mq.TheGridOperatorFlag = 0;
            }
            if (s.Period != null && AiGridFilterCodec.TryDate(s.Period.D1, out var d1) && AiGridFilterCodec.TryDate(s.Period.D2, out var d2))
            {
                mq.period_field = s.Period.Field;
                mq.global_d1 = d1;
                mq.global_d2 = d2;
            }
            if (s.State != null) mq.p31statequery = s.State.Value;
            if (!string.IsNullOrEmpty(s.Tab)) mq.p31tabquery = s.Tab;
            return mq;
        }

        public Plan Build(AiAggSpec s, AiColumnCatalog cat)
        {
            var measureKeys = s.Measures.Where(p => p != Count).ToList();
            var filterKeys = (s.Filter ?? new()).Select(p => p.Column).Where(p => !s.Dims.Contains(p) && !measureKeys.Contains(p)).Distinct().ToList();
            var byKey = Columns(s.Entity, s.Dims.Concat(measureKeys).Concat(filterKeys).ToList(), cat, out var cols);
            foreach (var c in cols) c.IsShowTotals = measureKeys.Contains(c.UniqueName, StringComparer.OrdinalIgnoreCase);

            var plan = new Plan { HasCount = s.Measures.Contains(Count) };
            foreach (var k in s.Dims) plan.Dims.Add(Col(byKey, k));
            foreach (var k in measureKeys) plan.Measures.Add(Col(byKey, k));

            var mq = Query(s, cat, cols);
            // bez dimenzí = jeden řádek celkem; GROUP BY musí obsahovat sloupec (konstanta nejde)
            plan.GroupBy = plan.Dims.Count == 0 ? $"a.{mq.PrefixDb}ID*0" : string.Join(",", plan.Dims.Select(p => p.getFinalSqlSyntax_GROUPBY()).Distinct());
            mq.explicit_sqlgroupby = plan.GroupBy;
            plan.Query = mq;
            return plan;
        }

        public ListPlan Build(AiListSpec s, AiColumnCatalog cat)
        {
            var filterKeys = (s.Filter ?? new()).Select(p => p.Column).Where(p => !s.Columns.Contains(p)).Distinct().ToList();
            var byKey = Columns(s.Entity, s.Columns.Concat(filterKeys).ToList(), cat, out var cols);
            foreach (var c in cols) c.IsShowTotals = cat.ByKey(c.UniqueName)?.Sum == true && s.Columns.Contains(c.UniqueName, StringComparer.OrdinalIgnoreCase);

            var plan = new ListPlan();
            foreach (var k in s.Columns) plan.Columns.Add(Col(byKey, k));
            var mq = Query(s, cat, cols);
            if (s.SortBy != null) mq.explicit_orderby = Col(byKey, s.SortBy).getFinalSqlSyntax_ORDERBY() + (s.SortDesc ? " DESC" : "");
            plan.Query = mq;
            return plan;
        }

        /// <summary>Zadání pro zdrojové úkony: výpis i agregace → agregace bez dimenzí se stejnými filtry (drill doplní podmínky řádku).</summary>
        public static AiAggSpec ForDrill(AiQuerySpec s)
        {
            if (s is AiAggSpec a) return a;
            return new AiAggSpec
            {
                Entity = s.Entity, Filter = s.Filter, Period = s.Period, State = s.State, Tab = s.Tab, Archive = s.Archive,
                Limit = 1, Title = s.Title, Measures = new() { Count }
            };
        }

        /// <summary>Agregované řádky (TOP max+1 – překročení limitu pozná volající podle počtu řádků).</summary>
        public DataTable Run(Plan p, int maxGroups)
        {
            var sel = new List<string>();
            foreach (var c in p.Display) sel.Add(c.getFinalSqlSyntax_GROUPBY_SELECT());
            if (sel.Count == 0) sel.Add("NULL AS ai_none");
            return _f.gridBL.GetGroupByList(p.Query, $"TOP ({maxGroups + 1}) " + string.Join(",", sel));
        }

        /// <summary>Výpis: řádky (TOP limit+1) a součtový řádek přes všechny záznamy (RowsCount + součty měřítek).</summary>
        public (DataTable rows, DataRow totals) RunList(ListPlan p, int limit)
        {
            string orderby = p.Query.explicit_orderby;
            p.Query.TopRecordsOnly = limit + 1;
            var rows = _f.gridBL.GetGridTable(p.Query);
            // součtový dotaz bez TOP a bez ORDER BY (GetGridTable doplnil výchozí řazení entity – v agregaci by bylo chybou)
            p.Query.TopRecordsOnly = 0;
            p.Query.explicit_orderby = null;
            var tot = _f.gridBL.GetGridTable(p.Query, true);
            p.Query.explicit_orderby = orderby;
            return (rows, tot.Rows.Count > 0 ? tot.Rows[0] : null);
        }

        // ------------------------------------------------------------------ výsledek

        /// <summary>Tabulka agregace pro chat: seřazené řádky do limitu, součty přes všechny skupiny.</summary>
        public AiAskTable ToTable(AiAggSpec s, Plan p, DataTable dt)
        {
            var display = p.Display;
            var names = UniqueNames(display.Select(c => c.Header).Concat(p.HasCount ? new[] { CountHeader } : Array.Empty<string>()).ToList());
            var t = new AiAskTable { Kind = "agg", Title = s.Title, Entity = s.Entity, CanDrill = s.Entity == "p31", Description = Describe(s) };
            for (int i = 0; i < display.Count; i++)
                t.Columns.Add(new AiAskColumn { Name = names[i], Type = TypeOf(display[i].FieldType), Measure = display[i].IsShowTotals, Decimals = Decimals(display[i].FieldType) });
            if (p.HasCount) t.Columns.Add(new AiAskColumn { Name = names[^1], Type = "number", Measure = true, Decimals = 0 });

            var rows = new List<object[]>();
            int rc = dt.Columns.IndexOf("RowsCount");
            foreach (DataRow r in dt.Rows)
            {
                var v = new object[t.Columns.Count];
                for (int i = 0; i < display.Count; i++)
                {
                    object o = dt.Columns.Contains(display[i].UniqueName) ? r[display[i].UniqueName] : null;
                    v[i] = display[i].IsShowTotals ? Num(o, t.Columns[i].Decimals) : Text(o, t.Columns[i].Type);
                }
                if (p.HasCount) v[^1] = rc < 0 || r[rc] == DBNull.Value ? 0L : Convert.ToInt64(r[rc]);
                rows.Add(v);
            }

            // celkem přes všechny skupiny
            t.Total = new object[t.Columns.Count];
            for (int i = 0; i < t.Columns.Count; i++)
            {
                if (!t.Columns[i].Measure) continue;
                double sum = rows.Sum(x => x[i] == null ? 0d : Convert.ToDouble(x[i], CultureInfo.InvariantCulture));
                t.Total[i] = Math.Round(sum, t.Columns[i].Decimals);
            }

            // řazení: zadané, jinak první měřítko sestupně
            int si = -1; bool desc = s.SortDesc;
            if (s.SortBy == Count && p.HasCount) si = t.Columns.Count - 1;
            else if (s.SortBy != null)
            {
                int di = s.Dims.IndexOf(s.SortBy);
                int mi = s.Measures.Where(x => x != Count).ToList().IndexOf(s.SortBy);
                si = di >= 0 ? di : mi >= 0 ? s.Dims.Count + mi : -1;
            }
            if (si < 0) { si = t.Columns.FindIndex(c => c.Measure); desc = true; }
            if (si >= 0)
            {
                var cmp = Comparer<object>.Create((a, b) => CompareValues(a, b));
                rows = desc ? rows.OrderByDescending(x => x[si], cmp).ToList() : rows.OrderBy(x => x[si], cmp).ToList();
            }

            t.Groups = rows.Count;
            t.Truncated = rows.Count > s.Limit;
            t.Rows = rows.Take(s.Limit).ToList();
            return t;
        }

        /// <summary>Tabulka výpisu záznamů: řádky do limitu s ID záznamů, součty měřítek a počet ze všech záznamů.</summary>
        public AiAskTable ToTable(AiListSpec s, ListPlan p, DataTable dt, DataRow totals)
        {
            var names = UniqueNames(p.Columns.Select(c => c.Header).ToList());
            var t = new AiAskTable { Kind = "list", Title = s.Title, Entity = s.Entity, CanDrill = s.Entity == "p31", Description = Describe(s), Ids = new() };
            for (int i = 0; i < p.Columns.Count; i++)
                t.Columns.Add(new AiAskColumn { Name = names[i], Type = TypeOf(p.Columns[i].FieldType), Measure = p.Columns[i].IsShowTotals, Decimals = Decimals(p.Columns[i].FieldType) });

            int pidCol = dt.Columns.IndexOf("pid");
            foreach (DataRow r in dt.Rows.Cast<DataRow>().Take(s.Limit))
            {
                var v = new object[t.Columns.Count];
                for (int i = 0; i < p.Columns.Count; i++)
                {
                    object o = dt.Columns.Contains(p.Columns[i].UniqueName) ? r[p.Columns[i].UniqueName] : null;
                    v[i] = t.Columns[i].Type == "number" ? Num(o, t.Columns[i].Decimals) : Text(o, t.Columns[i].Type);
                }
                t.Rows.Add(v);
                t.Ids.Add(pidCol < 0 || r[pidCol] == DBNull.Value ? 0 : Convert.ToInt32(r[pidCol]));
            }

            int count = totals != null && totals.Table.Columns.Contains("RowsCount") && totals["RowsCount"] != DBNull.Value ? Convert.ToInt32(totals["RowsCount"]) : dt.Rows.Count;
            t.Groups = count;
            t.Truncated = dt.Rows.Count > s.Limit;
            if (p.Columns.Any(c => c.IsShowTotals))
            {
                t.Total = new object[t.Columns.Count];
                for (int i = 0; i < p.Columns.Count; i++)
                    if (p.Columns[i].IsShowTotals && totals != null && totals.Table.Columns.Contains(p.Columns[i].UniqueName))
                        t.Total[i] = Num(totals[p.Columns[i].UniqueName], t.Columns[i].Decimals);
            }
            return t;
        }

        private static int CompareValues(object a, object b)
        {
            if (a == null && b == null) return 0;
            if (a == null) return -1;
            if (b == null) return 1;
            if (a is string sa && b is string sb) return string.Compare(sa, sb, new CultureInfo("cs-CZ"), CompareOptions.IgnoreCase);
            return Convert.ToDouble(a, CultureInfo.InvariantCulture).CompareTo(Convert.ToDouble(b, CultureInfo.InvariantCulture));
        }

        /// <summary>Období a filtry lidsky – do popisu tabulky i pro model.</summary>
        public string Describe(AiQuerySpec s)
        {
            var parts = new List<string>();
            if (s.Period != null && AiGridFilterCodec.TryDate(s.Period.D1, out var d1) && AiGridFilterCodec.TryDate(s.Period.D2, out var d2))
            {
                string field = AiToolEntities.PeriodFields(s.Entity).FirstOrDefault(p => p.Id == s.Period.Field).Name ?? s.Period.Field;
                parts.Add($"{_f.tra(field)}: {d1:d.M.yyyy} – {d2:d.M.yyyy}");
            }
            if (s.State != null && s.State.Value != 0 && BL.AI.Totals.AiTotalsService.States.TryGetValue(s.State.Value, out var st)) parts.Add(_f.tra(st));
            if (!string.IsNullOrEmpty(s.Tab) && BL.AI.Totals.AiTotalsService.TabNames.TryGetValue(s.Tab, out var tab)) parts.Add(_f.tra(tab));
            if (s.Archive != 1 && AiToolEntities.ArchiveNames.TryGetValue(s.Archive, out var arch)) parts.Add(_f.tra(arch));
            var cat = s.Filter?.Count > 0 ? Catalog(s.Entity) : null;
            foreach (var x in s.Filter ?? new())
            {
                string h = cat?.ByKey(x.Column)?.Header ?? x.Column;
                string v = x.Op switch
                {
                    "interval" => $"{x.From ?? "…"} – {x.To ?? "…"}",
                    "yes" => _f.tra("ano"),
                    "no" => _f.tra("ne"),
                    "empty" => _f.tra("prázdné"),
                    "notempty" => _f.tra("vyplněné"),
                    "gtzero" => "> 0",
                    "zeroorempty" => "0 / " + _f.tra("prázdné"),
                    _ => $"{OpName(x.Op)} „{string.Join("“ / „", x.Values ?? new())}“"
                };
                parts.Add($"{h}: {v}");
            }
            return string.Join("; ", parts);
        }

        private string OpName(string op) => _f.tra(op switch
        {
            "contains" => "obsahuje", "notcontains" => "neobsahuje", "starts" => "začíná", "equal" => "je", "notequal" => "není", _ => op
        });

        // ------------------------------------------------------------------ hodnoty (stejně jako myPivotData)

        /// <summary>Jedinečné názvy polí – stejný algoritmus jako myPivotData.FieldNames (Pivot-Explorer a drill je identifikují názvem).</summary>
        public static List<string> UniqueNames(List<string> headers)
        {
            var names = new List<string>();
            foreach (var h in headers)
            {
                string name = string.IsNullOrWhiteSpace(h) ? "?" : h.Trim();
                string unique = name; int k = 2;
                while (names.Contains(unique, StringComparer.OrdinalIgnoreCase)) unique = $"{name} ({k++})";
                names.Add(unique);
            }
            return names;
        }

        public static string TypeOf(string fieldType) => fieldType switch
        {
            "num" or "num0" or "num3" or "num4" or "num5" or "int" => "number",
            "date" or "datetime" or "datetimesec" => "date",
            "bool" => "bool",
            _ => "string"
        };

        public static int Decimals(string fieldType) => fieldType switch
        {
            "num0" or "int" => 0,
            "num3" => 3,
            "num4" => 4,
            "num5" => 5,
            _ => 2
        };

        private static object Num(object o, int decimals)
        {
            if (o == null || o == DBNull.Value) return null;
            double d;
            try { d = Convert.ToDouble(o, CultureInfo.InvariantCulture); } catch { return null; }
            return double.IsFinite(d) ? Math.Round(d, decimals) : null;
        }

        private static string Text(object o, string type)
        {
            if (o == null || o == DBNull.Value) return null;
            switch (type)
            {
                case "date":
                    if (o is DateTime dtm) return dtm.TimeOfDay == TimeSpan.Zero ? dtm.ToString("yyyy-MM-dd") : dtm.ToString("yyyy-MM-ddTHH:mm");
                    return o.ToString();
                case "bool":
                    if (o is bool b) return b ? "1" : "0";
                    return Convert.ToInt32(o) != 0 ? "1" : "0";
                case "number":
                    return Convert.ToString(o, CultureInfo.InvariantCulture);
                default:
                    string s = o.ToString().Trim();
                    if (s.Contains('<') && s.Contains('>')) s = BO.Code.Bas.Html2Text(s);
                    return s == "" ? null : s;
            }
        }
    }
}
