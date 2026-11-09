using System;
using System.Collections.Generic;
using System.Linq;

namespace UI.Models.Widgets
{
    /// <summary>
    /// Rozložení dashboardu (Widgets/Index) jako mřížka: jedno pořadí widgetů + příznak „široký“ (přes 2 sloupce).
    /// Počet sloupců (1/2/3) zůstává uživatelským nastavením – mřížka se mu přizpůsobí.
    /// Uložený stav (x56DockState, u „Moje AI widgety“ uživatelský parametr):
    ///   nový formát:  "grid:12,15w,7"   (w = široký)
    ///   starý formát: "sort=12&amp;sort=15|sort=7"  (sloupce oddělené |) – načte se po řádcích (1. z každého sloupce, pak 2. …),
    ///   takže plocha vypadá skoro stejně; v novém formátu se uloží až při prvním přesunu / změně šířky.
    /// WidgetsEnvironment (sloupce) dál používají AdminOneWebpage a dynamické stránky Record.
    /// </summary>
    public class WidgetsGrid
    {
        public const string Prefix = "grid:";

        public List<BO.x55Widget> Items { get; } = new List<BO.x55Widget>();
        public HashSet<int> Wide { get; } = new HashSet<int>();
        public bool IsWide(int x55id) => Wide.Contains(x55id);

        public struct Entry { public int Pid; public bool Wide; }

        public static List<Entry> Parse(string state)
        {
            var ret = new List<Entry>();
            if (string.IsNullOrWhiteSpace(state)) return ret;

            if (state.StartsWith(Prefix, StringComparison.Ordinal))
            {
                foreach (var part in state.Substring(Prefix.Length).Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
                {
                    bool wide = part.EndsWith("w", StringComparison.OrdinalIgnoreCase);
                    if (int.TryParse(wide ? part.Substring(0, part.Length - 1) : part, out int pid) && pid > 0 && !ret.Any(p => p.Pid == pid))
                    {
                        ret.Add(new Entry() { Pid = pid, Wide = wide });
                    }
                }
                return ret;
            }

            //starý formát po sloupcích
            var cols = state.Replace("sort=", "").Split('|')
                .Select(c => c.Split('&', StringSplitOptions.RemoveEmptyEntries).Select(x => int.TryParse(x.Trim(), out int n) ? n : 0).Where(n => n > 0).ToList())
                .ToList();
            int rows = cols.Count == 0 ? 0 : cols.Max(c => c.Count);
            for (int r = 0; r < rows; r++)
            {
                foreach (var col in cols)
                {
                    if (r < col.Count && !ret.Any(p => p.Pid == col[r])) ret.Add(new Entry() { Pid = col[r], Wide = false });
                }
            }
            return ret;
        }

        /// <summary>Seřadí widgety uživatele podle uloženého stavu; widgety, které ve stavu chybí (nově vložené), přijdou na konec.</summary>
        public static WidgetsGrid Build(string state, IEnumerable<BO.x55Widget> userWidgets)
        {
            var g = new WidgetsGrid();
            var list = userWidgets.ToList();
            foreach (var e in Parse(state))
            {
                var c = list.FirstOrDefault(p => p.pid == e.Pid);
                if (c != null && !g.Items.Contains(c))
                {
                    g.Items.Add(c);
                    if (e.Wide) g.Wide.Add(c.pid);
                }
            }
            foreach (var c in list)
            {
                if (!g.Items.Contains(c)) g.Items.Add(c);
            }
            return g;
        }
    }
}
