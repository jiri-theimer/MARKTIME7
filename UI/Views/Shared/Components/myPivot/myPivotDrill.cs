using System.Globalization;
using System.Text.Json;

namespace UI.Views.Shared.Components.myPivot
{
    /// <summary>
    /// Drill-down z buňky pivotu: podmínky (hodnoty dimenzí buňky a filtry pivotu) → SQL WHERE nad stejnými sloupci,
    /// ze kterých vznikla data pivotu (myPivotData). Hodnoty se nikdy nevkládají do SQL bez převodu na typ sloupce.
    /// </summary>
    public static class myPivotDrill
    {
        public class Cond
        {
            /// <summary>Název pole (hlavička, jako v myPivotData).</summary>
            public string f { get; set; }
            /// <summary>"in" = hodnota musí být mezi vybranými, "notin" = nesmí (filtr pivotu).</summary>
            public string mode { get; set; }
            /// <summary>Prázdná hodnota (NULL / prázdný text).</summary>
            public bool empty { get; set; }
            public List<JsonElement> values { get; set; }
            /// <summary>Rozsahy datumů [od, do) ve formátu yyyy-MM-dd – seskupení po měsících / čtvrtletích / rocích.</summary>
            public List<List<string>> ranges { get; set; }
        }

        public class Input
        {
            public List<Cond> conds { get; set; }
            public string label { get; set; }
        }

        /// <summary>SQL WHERE pro podmínky; null + chyba v error při neplatném vstupu.</summary>
        public static string BuildWhere(List<Cond> conds, List<BO.TheGridColumn> cols, out string error)
        {
            error = null;
            var names = myPivotData.FieldNames(cols);
            var parts = new List<string>();
            foreach (var c in conds ?? new())
            {
                int i = names.FindIndex(n => string.Equals(n, c.f, StringComparison.OrdinalIgnoreCase));
                if (i < 0) { error = $"Neznámé pole: {c.f}"; return null; }
                var col = cols[i];
                if (col.IsShowTotals) { error = $"Podle pole {c.f} nelze filtrovat."; return null; }
                string type = myPivotData.TypeOf(col.FieldType);
                string expr = ValueExpr(col);

                var vals = new List<string>();
                foreach (var v in c.values ?? new())
                {
                    string sv = SqlValue(v, type);
                    if (sv == null) { error = $"Neplatná hodnota pole {c.f}."; return null; }
                    vals.Add(sv);
                }
                var ors = new List<string>();
                if (vals.Count > 0) ors.Add($"{expr} IN ({string.Join(",", vals)})");
                foreach (var r in c.ranges ?? new())
                {
                    if (type != "date" || r.Count != 2 || !TryDate(r[0], out var d1) || !TryDate(r[1], out var d2)) { error = $"Neplatný rozsah pole {c.f}."; return null; }
                    ors.Add($"({expr}>={SqlDate(d1)} AND {expr}<{SqlDate(d2)})");
                }
                string isEmpty = type == "string" ? $"({expr} IS NULL OR {expr}='')" : $"{expr} IS NULL";
                string notEmpty = type == "string" ? $"({expr} IS NOT NULL AND {expr}<>'')" : $"{expr} IS NOT NULL";

                if (c.mode == "notin")
                {
                    // NOT IN s ošetřením NULL: prázdné hodnoty zůstávají, pokud nejsou mezi vyloučenými
                    if (ors.Count == 0 && !c.empty) continue;
                    if (ors.Count == 0) { parts.Add(notEmpty); continue; }
                    string not = $"NOT ({string.Join(" OR ", ors)})";
                    parts.Add(c.empty ? $"({notEmpty} AND {not})" : $"({expr} IS NULL OR {not})");
                }
                else
                {
                    if (c.empty) ors.Add(isEmpty);
                    if (ors.Count == 0) { error = $"Chybí hodnota pole {c.f}."; return null; }
                    parts.Add($"({string.Join(" OR ", ors)})");
                }
            }
            return parts.Count == 0 ? "" : string.Join(" AND ", parts);
        }

        /// <summary>Výraz hodnoty sloupce – stejný jako v getFinalSqlSyntax_GROUPBY_SELECT, jen bez agregační funkce.</summary>
        public static string ValueExpr(BO.TheGridColumn col)
        {
            string sel = col.getFinalSqlSyntax_GROUPBY_SELECT();      // "MIN(výraz) AS alias"
            int a = sel.IndexOf('(');
            int b = sel.LastIndexOf(") AS ", StringComparison.OrdinalIgnoreCase);
            return sel.Substring(a + 1, b - a - 1);
        }

        private static string SqlValue(JsonElement v, string type)
        {
            switch (type)
            {
                case "number":
                    double d;
                    if (v.ValueKind == JsonValueKind.Number) d = v.GetDouble();
                    else if (!double.TryParse(v.ToString(), NumberStyles.Float, CultureInfo.InvariantCulture, out d)) return null;
                    return double.IsFinite(d) ? d.ToString("0.##########", CultureInfo.InvariantCulture) : null;
                case "bool":
                    string b = v.ToString();
                    return b == "1" || b.Equals("true", StringComparison.OrdinalIgnoreCase) ? "1" : b == "0" || b.Equals("false", StringComparison.OrdinalIgnoreCase) ? "0" : null;
                case "date":
                    string s = v.ToString();
                    if (DateTime.TryParseExact(s, new[] { "yyyy-MM-dd", "yyyy-MM-ddTHH:mm", "yyyy-MM-ddTHH:mm:ss" }, CultureInfo.InvariantCulture, DateTimeStyles.None, out var dt)) return SqlDate(dt);
                    return null;
                default:
                    return BO.Code.Bas.GS(v.ToString());
            }
        }

        private static string SqlDate(DateTime d) => "convert(datetime,'" + d.ToString("yyyy-MM-ddTHH:mm:ss", CultureInfo.InvariantCulture) + "',126)";

        private static bool TryDate(string s, out DateTime d) =>
            DateTime.TryParseExact(s ?? "", "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out d);
    }
}
