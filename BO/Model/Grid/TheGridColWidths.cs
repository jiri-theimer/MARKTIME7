namespace BO.Model.Grid
{
    /// <summary>
    /// Uživatelské šířky sloupců gridu (j75ColumnsGridWidth): čárkou oddělené položky "UniqueName sloupce:šířka v px".
    /// Starší zápis měl šířku zarovnanou na 3 znaky ("sloupec: 85") – čte se stejně.
    /// Sloupec se hledá přesnou shodou názvu (dříve podřetězcem: sloupec p31Text tak přebíral šířku p31TextInternal)
    /// a šířka se bere celá (dříve jen poslední 3 znaky: 1200 px se uložilo jako 200).
    /// </summary>
    public static class TheGridColWidths
    {
        public const int MinWidth = 30;
        public const int MaxWidth = 3000;

        private static bool Parse(string item, out string colid, out int width)
        {
            colid = null; width = 0;
            if (string.IsNullOrWhiteSpace(item)) return false;
            int x = item.LastIndexOf(':');
            if (x <= 0) return false;
            colid = item.Substring(0, x).Trim();
            return int.TryParse(item.Substring(x + 1).Trim(), out width) && width > 0;
        }

        /// <summary>Uložená šířka sloupce v px; 0 = uživatel ji nenastavil.</summary>
        public static int Find(IEnumerable<string> items, string colid)
        {
            if (items == null || string.IsNullOrEmpty(colid)) return 0;
            foreach (string item in items)
            {
                if (Parse(item, out string c, out int w) && c == colid) return w;
            }
            return 0;
        }

        /// <summary>Nový obsah j75ColumnsGridWidth po nastavení šířky sloupce; width = 0 šířku sloupce zruší (výchozí šířka).</summary>
        public static string Set(string stored, string colid, int width)
        {
            var lis = new List<string>();
            foreach (string item in (stored ?? "").Split(',', StringSplitOptions.RemoveEmptyEntries))
            {
                if (Parse(item, out string c, out int w) && c != colid) lis.Add($"{c}:{w}");
            }
            if (width > 0)
            {
                lis.Add($"{colid}:{Math.Min(Math.Max(width, MinWidth), MaxWidth)}");
            }
            return lis.Count == 0 ? null : string.Join(",", lis);
        }
    }
}
