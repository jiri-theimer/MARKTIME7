using System.Globalization;
using BL.AI.Core;
using BO.AI;

namespace BL.AI.Grid
{
    /// <summary>
    /// Převod sloupcového filtru gridu mezi návrhem AI (AiGridFilter) a stavem gridu j75Filter.
    /// Formát j75Filter (myGridSupport.Event_HandleTheGridFilter): "pole###operátor###hodnota" oddělené "$$$".
    /// Operátory a hodnoty stejně jako thegrid.js (tg_filter_ok) a baseQuery.ParseSqlFromTheGridFilter:
    /// 1 prázdné, 2 neprázdné, 3 obsahuje, 33 neobsahuje, 4 interval "od|do", 5 začíná, 6 rovno, 7 nerovno,
    /// 8 ANO, 9 NE, 10 větší než nula, 11 nula nebo prázdné. Více textových hodnot odděluje středník (= NEBO).
    /// </summary>
    public static class AiGridFilterCodec
    {
        private static readonly Dictionary<string, string> _codes = new()
        {
            ["empty"] = "1", ["notempty"] = "2", ["contains"] = "3", ["notcontains"] = "33", ["interval"] = "4",
            ["starts"] = "5", ["equal"] = "6", ["notequal"] = "7", ["yes"] = "8", ["no"] = "9",
            ["gtzero"] = "10", ["zeroorempty"] = "11"
        };

        /// <summary>Povolené operátory podle typu filtru – stejná nabídka jako popup filtru v gridu.</summary>
        public static readonly Dictionary<string, string[]> OpsByType = new()
        {
            ["string"] = new[] { "contains", "notcontains", "starts", "equal", "notequal", "empty", "notempty" },
            ["number"] = new[] { "interval", "gtzero", "zeroorempty", "empty", "notempty" },
            ["date"] = new[] { "interval", "empty", "notempty" },
            ["bool"] = new[] { "yes", "no" }
        };

        public static IEnumerable<string> AllOps => _codes.Keys;

        private static readonly string[] _textOps = { "contains", "notcontains", "starts", "equal", "notequal" };

        // otevřený interval – grid vyžaduje obě meze (prázdná mez = podmínka s NULL)
        private const string NumMin = "-999999999", NumMax = "999999999";
        private static readonly DateTime DateMin = new(1900, 1, 1), DateMax = new(2099, 12, 31);

        /// <summary>Kontrola podmínek proti katalogu; chyby vrací modelu k opravě.</summary>
        public static void Validate(List<AiGridFilter> filter, AiColumnCatalog cat, List<string> errors)
        {
            foreach (var f in filter ?? new())
            {
                var e = cat.ByKey(f.Column);
                if (e == null) { errors.Add($"Filtr: neznámý sloupec {f.Column}"); continue; }
                if (e.FilterType == null) { errors.Add($"Filtr: podle sloupce '{e.Header}' nelze filtrovat."); continue; }
                if (!OpsByType[e.FilterType].Contains(f.Op))
                {
                    errors.Add($"Filtr: operátor '{f.Op}' nelze použít pro sloupec '{e.Header}' (typ {e.FilterType}; povoleno: {string.Join(", ", OpsByType[e.FilterType])}).");
                    continue;
                }
                if (_textOps.Contains(f.Op) && Clean(f.Values).Count == 0) errors.Add($"Filtr: u sloupce '{e.Header}' chybí hledaný text.");
                if (f.Op == "interval")
                {
                    bool isDate = e.FilterType == "date";
                    bool okFrom = string.IsNullOrWhiteSpace(f.From) || (isDate ? TryDate(f.From, out _) : TryNum(f.From, out _));
                    bool okTo = string.IsNullOrWhiteSpace(f.To) || (isDate ? TryDate(f.To, out _) : TryNum(f.To, out _));
                    if (!okFrom || !okTo) errors.Add($"Filtr: neplatná mez intervalu u sloupce '{e.Header}' ({(isDate ? "datum yyyy-MM-dd" : "číslo")}).");
                    else if (string.IsNullOrWhiteSpace(f.From) && string.IsNullOrWhiteSpace(f.To)) errors.Add($"Filtr: interval u sloupce '{e.Header}' nemá vyplněnou ani jednu mez.");
                    else if (isDate && TryDate(f.From, out var d1) && TryDate(f.To, out var d2) && d1 > d2) errors.Add($"Filtr: u sloupce '{e.Header}' je datum od větší než datum do.");
                    else if (!isDate && TryNum(f.From, out var n1) && TryNum(f.To, out var n2) && n1 > n2) errors.Add($"Filtr: u sloupce '{e.Header}' je hodnota od větší než do.");
                }
            }
        }

        /// <summary>Návrh → j75Filter.</summary>
        public static string ToJ75(List<AiGridFilter> filter, AiColumnCatalog cat)
        {
            var lis = new List<string>();
            foreach (var f in filter ?? new())
            {
                var e = cat.ByKey(f.Column);
                if (e == null || !_codes.TryGetValue(f.Op ?? "", out var code)) continue;
                string value = "";
                if (_textOps.Contains(f.Op)) value = string.Join(";", Clean(f.Values));
                else if (f.Op == "yes" || f.Op == "no") value = code;          // bool: hodnota = operátor (thegrid.js)
                else if (f.Op == "interval")
                {
                    if (e.FilterType == "date")
                    {
                        var d1 = TryDate(f.From, out var a) ? a : DateMin;
                        var d2 = TryDate(f.To, out var b) ? b : DateMax;
                        string s2 = d2.ToString("dd.MM.yyyy", CultureInfo.InvariantCulture);
                        if (e.Type == "datetime" || e.Type == "datetimesec") s2 += " 23:59:59";   // jinak by poslední den vypadl
                        value = d1.ToString("dd.MM.yyyy", CultureInfo.InvariantCulture) + "|" + s2;
                    }
                    else
                    {
                        value = (TryNum(f.From, out var a) ? Num(a) : NumMin) + "|" + (TryNum(f.To, out var b) ? Num(b) : NumMax);
                    }
                }
                lis.Add(e.Key + "###" + code + "###" + value);
            }
            return string.Join("$$$", lis);
        }

        /// <summary>j75Filter → návrh (předvyplnění formuláře z aktuálního stavu gridu).</summary>
        public static List<AiGridFilter> FromJ75(string j75Filter, AiColumnCatalog cat, List<string> skipped)
        {
            var ret = new List<AiGridFilter>();
            if (string.IsNullOrEmpty(j75Filter)) return ret;
            foreach (var s in j75Filter.Split("$$$"))
            {
                var a = s.Split("###");
                if (a.Length < 2) continue;
                var e = cat.ByKey(a[0]);
                string op = _codes.FirstOrDefault(p => p.Value == a[1]).Key;
                if (e == null || e.FilterType == null || op == null) { skipped.Add(e?.Header ?? a[0]); continue; }
                string v = a.Length > 2 ? a[2] : "";
                var f = new AiGridFilter { Column = e.Key, Op = op };
                if (_textOps.Contains(op)) f.Values = v.Split(";").Select(x => x.Trim()).Where(x => x != "").ToList();
                if (op == "interval")
                {
                    var p = v.Split("|");
                    string c1 = p[0], c2 = p.Length > 1 ? p[1] : "";
                    if (e.FilterType == "date")
                    {
                        f.From = ParseCz(c1, out var d1) && d1 != DateMin ? d1.ToString("yyyy-MM-dd") : null;
                        f.To = ParseCz(c2, out var d2) && d2.Date != DateMax ? d2.ToString("yyyy-MM-dd") : null;
                    }
                    else
                    {
                        f.From = c1 == NumMin || c1 == "" ? null : c1;
                        f.To = c2 == NumMax || c2 == "" ? null : c2;
                    }
                }
                ret.Add(f);
            }
            return ret;
        }

        private static List<string> Clean(List<string> v) =>
            (v ?? new()).Select(x => (x ?? "").Replace("###", "").Replace("$$$", "").Replace(";", " ").Trim()).Where(x => x != "").ToList();

        public static bool TryDate(string s, out DateTime d) =>
            DateTime.TryParseExact(s ?? "", "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out d);

        private static bool TryNum(string s, out double d) =>
            double.TryParse((s ?? "").Trim().Replace(",", "."), NumberStyles.Float, CultureInfo.InvariantCulture, out d);

        private static string Num(double d) => d.ToString("0.############", CultureInfo.InvariantCulture);

        private static bool ParseCz(string s, out DateTime d) =>
            DateTime.TryParseExact((s ?? "").Trim(), new[] { "dd.MM.yyyy", "d.M.yyyy", "dd.MM.yyyy HH:mm:ss", "dd.MM.yyyy HH:mm" }, CultureInfo.InvariantCulture, DateTimeStyles.None, out d);
    }
}
