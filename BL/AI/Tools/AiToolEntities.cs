using System.Text;

namespace BL.AI.Tools
{
    /// <summary>
    /// Entity, nad kterými smí obecný agent pracovat (katalog, agregace), a pevné filtry, které entita nabízí.
    /// Pravidla pevných filtrů odpovídají gridu (AiGridFixedFilters / TheGridController).
    /// </summary>
    public static class AiToolEntities
    {
        public class Def
        {
            public string Prefix { get; init; }
            /// <summary>Popis pro model – co entita obsahuje a k jakým otázkám se hodí.</summary>
            public string About { get; init; }
            public Func<BO.RunningUser, bool> Module { get; init; }
        }

        public static readonly List<Def> All = new()
        {
            new() { Prefix = "p31", Module = u => u.j04IsModule_p31, About = "Úkony (výkazy práce a výdajů): hodiny, částky, fakturační ceny, náklady; dimenze osoba, projekt, klient, aktivita, datum, měsíc, rok, stav schválení a vyúčtování. Nejčastější zdroj pro otázky na hodiny, výnosy, nevyfakturováno." },
            new() { Prefix = "p41", Module = u => u.j04IsModule_p41, About = "Projekty (zakázky): klient, typ, stav, plán, rozpočty a souhrnné hodnoty projektu." },
            new() { Prefix = "p28", Module = u => u.j04IsModule_p28, About = "Kontakty (klienti, dodavatelé, firmy i osoby)." },
            new() { Prefix = "p56", Module = u => u.j04IsModule_p56, About = "Úkoly: řešitel, termín, stav, projekt." },
            new() { Prefix = "p91", Module = u => u.j04IsModule_p91, About = "Vyúčtování (vydané faktury): částky bez DPH / s DPH, dluh, splatnost, úhrada, klient, měna." },
            new() { Prefix = "p90", Module = u => u.j04IsModule_p90, About = "Zálohy (zálohové faktury): částky, úhrady, splatnost, klient." },
            new() { Prefix = "j02", Module = u => u.j04IsModule_j02, About = "Uživatelé (osoby, zaměstnanci): středisko, pozice, fond hodin." },
            new() { Prefix = "o23", Module = u => u.j04IsModule_o23, About = "Dokumenty: typ, stav, vazby na projekt a klienta." }
        };

        public static Def Get(string prefix, BO.RunningUser u) => All.FirstOrDefault(p => p.Prefix == prefix && p.Module(u));
        public static IEnumerable<Def> Allowed(BO.RunningUser u) => All.Where(p => p.Module(u));

        public static bool HasState(string prefix, BO.RunningUser u) => u.j04IsModule_p31 && new[] { "p31", "p41", "p28", "o23", "p56", "j02" }.Contains(prefix);
        public static bool HasTab(string prefix, BO.RunningUser u) => u.j04IsModule_p31 && (prefix == "p91" || HasState(prefix, u));
        public static IEnumerable<int> ArchiveValues(string prefix) => prefix == "p31" ? new[] { 1, 2 } : new[] { 0, 1, 2 };
        public static List<(string Id, string Name)> PeriodFields(string prefix) => BL.AI.Grid.AiGridFixedFilters.FieldsOf(prefix);

        public static readonly Dictionary<int, string> ArchiveNames = BL.AI.Grid.AiGridFixedFilters.ArchiveNames;

        /// <summary>Seznam entit pro system prompt.</summary>
        public static string ToPromptText(BL.Factory f)
        {
            var sb = new StringBuilder();
            sb.AppendLine("# ENTITY (entita = prefix)");
            foreach (var d in Allowed(f.CurrentUser))
            {
                var e = f.EProvider.ByPrefix(d.Prefix);
                sb.AppendLine($"{d.Prefix} | {e?.AliasPlural ?? e?.AliasSingular} | {d.About}");
            }
            return sb.ToString();
        }
    }
}
