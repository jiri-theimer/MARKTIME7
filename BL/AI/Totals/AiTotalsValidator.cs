namespace BL.AI.Totals
{
    /// <summary>Kontrola návrhu (s klíči sloupců) proti katalogu uživatele a pravidlům Součtů.</summary>
    public static class AiTotalsValidator
    {
        public static readonly string[] Tabs = { "", "time", "kusovnik", "expense", "fee" };
        public static readonly string[] PeriodFields = { "p31Date", "p91Date", "p91DateSupply" };
        public static readonly string[] Aggregations = { "sum", "average", "count", "min", "max" };

        public class Result
        {
            public List<string> Errors { get; } = new();
            public List<string> Warnings { get; } = new();
            public bool IsValid => Errors.Count == 0;
        }

        public static Result Validate(BO.AI.AiTotalsProposal p, AiTotalsCatalog cat, IEnumerable<BO.ThePeriod> periods, IEnumerable<int> filterIds = null)
        {
            var r = new Result();
            if (p == null) { r.Errors.Add("Návrh je prázdný."); return r; }

            if (string.IsNullOrWhiteSpace(p.Name)) r.Errors.Add("Chybí název šablony.");
            if (p.Measures == null || p.Measures.Count == 0) r.Errors.Add("Návrh musí obsahovat alespoň jedno měřítko.");

            var used = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var k in (p.Rows ?? new()).Concat(p.Columns ?? new()))
            {
                var e = cat.ByKey(k);
                if (e == null) r.Errors.Add($"Neznámá dimenze: {k}");
                else if (e.Role != "dimension") r.Errors.Add($"'{e.Header}' není dimenze.");
                if (!used.Add(k)) r.Errors.Add($"Sloupec '{e?.Header ?? k}' je použit vícekrát.");
            }
            foreach (var mm in p.Measures ?? new())
            {
                var e = cat.ByKey(mm.Key);
                if (e == null) r.Errors.Add($"Neznámé měřítko: {mm.Key}");
                else if (e.Role != "measure") r.Errors.Add($"'{e.Header}' není měřítko.");
                if (!used.Add(mm.Key ?? "")) r.Errors.Add($"Sloupec '{e?.Header ?? mm.Key}' je použit vícekrát.");
                if (!Aggregations.Contains(mm.Aggregation ?? "sum", StringComparer.OrdinalIgnoreCase)) r.Errors.Add($"Neplatná agregace: {mm.Aggregation}");
            }

            // stejné hlavičky = kolize polí ve WebDataRocks (CSV hlavička je název sloupce)
            var headers = used.Select(k => cat.ByKey(k)?.Header).Where(h => h != null).GroupBy(h => h).Where(g => g.Count() > 1).Select(g => g.Key);
            foreach (var h in headers) r.Errors.Add($"Dva vybrané sloupce mají stejný název '{h}' – WebDataRocks je nerozliší.");

            if (p.State < 0 || p.State > 21) r.Errors.Add($"Neplatný stav úkonů: {p.State}");
            if (!Tabs.Contains(p.Tab ?? "")) r.Errors.Add($"Neplatný formát úkonů: {p.Tab}");
            if (p.Chart != 0 && p.Chart != 1) r.Errors.Add($"Neplatný typ grafu: {p.Chart}");
            if (p.Filter != 0 && filterIds != null && !filterIds.Contains(p.Filter)) r.Errors.Add($"Neznámý uložený filtr: {p.Filter}");

            var per = p.Period ?? new BO.AI.AiTotalsPeriod();
            if (!PeriodFields.Contains(per.Field ?? "p31Date")) r.Errors.Add($"Neplatné pole období: {per.Field}");
            if (per.Value == 1)
            {
                if (!TryDate(per.D1, out var d1) || !TryDate(per.D2, out var d2)) r.Errors.Add("Vlastní období musí mít platná data od–do (yyyy-MM-dd).");
                else if (d1 > d2) r.Errors.Add("Datum od je větší než datum do.");
            }
            else if (per.Value != 0 && !periods.Any(x => x.pid == per.Value)) r.Errors.Add($"Neznámé pojmenované období: {per.Value}");

            // varování – velikost datového zdroje (limit 1 MB)
            bool daily = used.Any(k => k.EndsWith("__p31Date", StringComparison.OrdinalIgnoreCase));
            if (daily && per.Value == 0) r.Warnings.Add("Denní granularita bez omezení období může překročit limit datového zdroje 1 MB.");
            if ((p.Rows?.Count ?? 0) + (p.Columns?.Count ?? 0) > 4) r.Warnings.Add("Mnoho dimenzí zvětšuje datový zdroj – zvažte méně.");

            return r;
        }

        public static bool TryDate(string s, out DateTime d) =>
            DateTime.TryParseExact(s ?? "", "yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out d);
    }
}
