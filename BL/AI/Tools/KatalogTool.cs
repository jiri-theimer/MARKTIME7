using System.Text;
using System.Text.Json.Nodes;
using BL.AI.Core;
using BL.AI.Grid;

namespace BL.AI.Tools
{
    /// <summary>
    /// Nástroj „katalog“: sloupce entity (AiColumnCatalog – stejná paleta a oprávnění jako návrhář sloupců)
    /// s rolí pro agregaci a pevné filtry entity (období, stav, formát, archiv).
    /// </summary>
    public class KatalogTool : IAiTool
    {
        private readonly AiAggregate _agg;
        private readonly BL.Factory _f;

        public KatalogTool(BL.Factory f, AiAggregate agg) { _f = f; _agg = agg; }

        public string Name => "katalog";

        public string Description =>
            "Vrátí katalog sloupců entity: id sloupce, název, skupinu (entita relace), roli a typ filtru, a pevné filtry entity (pole období, stavy, formáty, archiv). " +
            "Zavolej jej pro každou entitu, se kterou budeš pracovat, dřív než ji použiješ v jiném nástroji – id sloupců platí jen pro danou entitu. " +
            "Katalog už respektuje oprávnění uživatele (sazby, výsledovky).";

        public JsonObject InputSchema => AiSchema.Obj(new JsonObject
        {
            ["entita"] = AiSchema.StrEnum(AiToolEntities.Allowed(_f.CurrentUser).Select(p => p.Prefix))
        });

        public AiToolOutput Execute(JsonNode input, AiToolContext ctx)
        {
            string prefix = AiToolJson.Str(input, "entita");
            var def = AiToolEntities.Get(prefix, _f.CurrentUser);
            if (def == null) return AiToolOutput.Error($"Entita '{prefix}' není dostupná. Povolené: {string.Join(", ", AiToolEntities.Allowed(_f.CurrentUser).Select(p => p.Prefix))}.");

            var cat = ctx.Cached("cat-" + prefix, () => _agg.Catalog(prefix));
            var e = _f.EProvider.ByPrefix(prefix);
            var u = _f.CurrentUser;
            var sb = new StringBuilder();
            sb.AppendLine($"# ENTITA {prefix}: {e?.AliasPlural ?? e?.AliasSingular}");
            sb.AppendLine("# SLOUPCE (id | název | skupina | role | typ filtru | popis)");
            sb.AppendLine("role: M = měřítko (sčítá se), D = dimenze (seskupení), F = nelze seskupovat (jen filtr a výpis); typ filtru: string | number | date | bool | - (nelze filtrovat)");
            sb.AppendLine("Ve výpisu (hledej) lze použít sloupec s libovolnou rolí.");
            int n = 0;
            foreach (var c in cat.Items)
            {
                string role = c.Sum ? "M" : c.NotForTotals ? "F" : "D";
                sb.AppendLine($"{c.Id} | {c.Header} | {c.Group} | {role} | {c.FilterType ?? "-"} | {c.Hint}");
                n++;
            }
            sb.AppendLine($"{AiAggregate.Count} | {AiAggregate.CountHeader} | {e?.AliasSingular} | M | - | počet záznamů ve skupině (jen agregace)");
            sb.AppendLine();

            var fields = AiToolEntities.PeriodFields(prefix);
            sb.AppendLine("# OBDOBÍ (obdobi.pole)");
            if (fields.Count == 0) sb.AppendLine("Entita nemá filtr období – časové omezení řeš sloupcovým filtrem (date, interval).");
            foreach (var p in fields) sb.AppendLine($"{p.Id} | {p.Name}");
            if (prefix != "p31" && fields.Any(p => p.Id == "p31Date")) sb.AppendLine("p31Date u této entity = záznamy, které mají úkony v daném období.");
            sb.AppendLine();

            if (AiToolEntities.HasState(prefix, u))
            {
                sb.AppendLine("# STAV ÚKONŮ (stav)");
                if (prefix != "p31") sb.AppendLine("U této entity: záznamy, které mají úkony v daném stavu.");
                foreach (var s in BL.AI.Totals.AiTotalsService.States) sb.AppendLine($"{s.Key} | {s.Value}");
                sb.AppendLine();
            }
            if (AiToolEntities.HasTab(prefix, u))
            {
                sb.AppendLine("# FORMÁT ÚKONŮ (format)");
                foreach (var t in BL.AI.Totals.AiTotalsService.TabNames) sb.AppendLine($"\"{t.Key}\" | {t.Value}");
                sb.AppendLine();
            }
            sb.AppendLine("# ARCHIV (archiv)");
            foreach (var a in AiToolEntities.ArchiveValues(prefix)) sb.AppendLine($"{a} | {AiToolEntities.ArchiveNames[a]}{(a == 1 ? " (výchozí)" : "")}");
            sb.AppendLine();

            sb.AppendLine("# OPERÁTORY SLOUPCOVÉHO FILTRU (filtry.op podle typu filtru)");
            foreach (var o in AiGridFilterCodec.OpsByType) sb.AppendLine($"{o.Key}: {string.Join(", ", o.Value)}");

            return new AiToolOutput { Content = sb.ToString(), Rows = n };
        }
    }
}
