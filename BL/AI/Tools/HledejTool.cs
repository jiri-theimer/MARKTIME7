using System.Text.Json.Nodes;
using BL.AI.Core;
using BO.AI;

namespace BL.AI.Tools
{
    /// <summary>
    /// Nástroj „hledej“: výpis jednotlivých záznamů entity s vybranými sloupci a filtry (stejný dotaz jako grid – GetGridTable).
    /// Vrací řádky do limitu, počet všech nalezených záznamů a součty sčítatelných sloupců ze všech záznamů.
    /// Řádky v UI nesou ID záznamu – „Otevřít kartu“.
    /// </summary>
    public class HledejTool : IAiTool
    {
        public const int DefaultLimit = 30;

        private readonly BL.Factory _f;
        private readonly AiAggregate _agg;

        public HledejTool(BL.Factory f, AiAggregate agg) { _f = f; _agg = agg; }

        public string Name => "hledej";

        public string Description =>
            "Vypíše jednotlivé záznamy entity (úkony, projekty, faktury…) s vybranými sloupci a filtry. Vrátí id výsledku (vysledek), řádky do limitu, " +
            "počet všech nalezených záznamů a součty sčítatelných sloupců (M) ze všech záznamů. Id sloupců ber z katalogu téže entity. " +
            "Použij, když uživatel chce vidět konkrétní záznamy (seznam, výpis, které…, ukaž mi…). Na součty a přehledy podle skupin použij agregace.";

        public JsonObject InputSchema
        {
            get
            {
                var s = AiSchema.Obj(new JsonObject
                {
                    ["entita"] = AiSchema.StrEnum(AiToolEntities.Allowed(_f.CurrentUser).Select(p => p.Prefix)),
                    ["nadpis"] = AiSchema.Str(),
                    ["sloupce"] = AiSchema.Array(AiSchema.Str())
                });
                var p = (JsonObject)s["properties"];
                AiToolSpec.AddCommonSchema(p, _f,
                    "Řazení podle id sloupce výpisu; null = výchozí řazení entity.",
                    $"Max. počet vrácených záznamů (výchozí {DefaultLimit}, max. {AiAggregate.MaxListLimit}). Počet a součty se počítají vždy ze všech nalezených.");
                s["required"] = AiSchema.Arr(p.Select(x => x.Key));
                p["nadpis"]["description"] = "Krátký nadpis tabulky pro uživatele (max. 80 znaků).";
                p["sloupce"]["description"] = $"Id sloupců výpisu v pořadí zleva (1–{AiAggregate.MaxListColumns}): nejdřív identifikace (datum, název, kód), pak atributy, vpravo čísla.";
                return s;
            }
        }

        public AiToolOutput Execute(JsonNode input, AiToolContext ctx) => Run(input, ctx, null);

        public AiToolOutput Run(JsonNode input, AiToolContext ctx, string resultId)
        {
            string prefix = AiToolJson.Str(input, "entita");
            if (AiToolEntities.Get(prefix, _f.CurrentUser) == null) return AiToolOutput.Error($"Entita '{prefix}' není dostupná.");
            var cat = ctx.Cached("cat-" + prefix, () => _agg.Catalog(prefix));

            var errors = new List<string>();
            var key = AiToolSpec.KeyResolver(cat, prefix, errors);
            var spec = new AiListSpec
            {
                Entity = prefix,
                Columns = AiToolJson.StrList(input, "sloupce").Select(x => key(x, "Sloupec")).ToList()
            };
            AiToolSpec.ReadCommon(input, spec, key, DefaultLimit);
            if (errors.Count == 0) errors.AddRange(_agg.Validate(spec, cat));
            if (errors.Count > 0) return AiToolOutput.Error(errors);

            AiAskTable table;
            try
            {
                var plan = _agg.Build(spec, cat);
                var (rows, totals) = _agg.RunList(plan, spec.Limit);
                table = _agg.ToTable(spec, plan, rows, totals);
            }
            catch (Exception ex)
            {
                return AiToolOutput.Error($"Dotaz nad databází selhal ({ex.Message}). Zkus jiné sloupce nebo filtry.");
            }
            table.Id = resultId ?? ctx.NewResultId();

            var forModel = new
            {
                vysledek = table.Id,
                nadpis = table.Title,
                filtry = table.Description,
                sloupce = table.Columns.Select(c => c.Name),
                radky = AiToolSpec.CutForModel(table.Rows),
                celkem = table.Total,
                pocet_zaznamu = table.Groups,
                zobrazeno = table.Rows.Count,
                zkraceno = table.Truncated
            };
            return new AiToolOutput
            {
                Content = AiToolJson.Serialize(forModel),
                ResultId = table.Id,
                Data = new AiAggOutcome { Spec = spec, Table = table },
                Rows = table.Groups
            };
        }
    }
}
