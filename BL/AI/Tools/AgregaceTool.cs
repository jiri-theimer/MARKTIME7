using System.Text.Json.Nodes;
using BL.AI.Core;
using BO.AI;

namespace BL.AI.Tools
{
    /// <summary>Výsledek nástroje agregace nebo hledej: zadání (s klíči sloupců) + tabulka pro chat.</summary>
    public class AiAggOutcome
    {
        public AiQuerySpec Spec { get; set; }
        public AiAskTable Table { get; set; }
    }

    /// <summary>
    /// Nástroj „agregace“: součty měřítek seskupené podle dimenzí s filtry – stejná data jako Součty / Pivot-Explorer.
    /// Model posílá id sloupců z katalogu entity; nástroj je převede na klíče, zvaliduje a provede s oprávněními uživatele.
    /// </summary>
    public class AgregaceTool : IAiTool
    {
        /// <summary>Max. počet skupin načtených pro řazení a součty (do chatu jde jen limit řádků).</summary>
        public const int MaxGroups = 20000;
        public const int DefaultLimit = 50;

        private readonly BL.Factory _f;
        private readonly AiAggregate _agg;

        public AgregaceTool(BL.Factory f, AiAggregate agg) { _f = f; _agg = agg; }

        public string Name => "agregace";

        public string Description =>
            "Spočítá součty měřítek (M) seskupené podle dimenzí (D) entity s filtry. Vrátí id výsledku (vysledek), použité filtry, sloupce, řádky a celkové součty. " +
            "Id sloupců ber z katalogu téže entity. Bez dimenzí vrátí jeden řádek s celkovými součty. " +
            "Pro výpis jednotlivých záznamů použij nástroj hledej. Výsledek uživateli ukážeš tak, že jeho id uvedeš v odpovědi (tabulky) – čísla do textu nepřepisuj.";

        public JsonObject InputSchema
        {
            get
            {
                var s = AiSchema.Obj(new JsonObject
                {
                    ["entita"] = AiSchema.StrEnum(AiToolEntities.Allowed(_f.CurrentUser).Select(p => p.Prefix)),
                    ["nadpis"] = AiSchema.Str(),
                    ["dimenze"] = AiSchema.Array(AiSchema.Str()),
                    ["meritka"] = AiSchema.Array(AiSchema.Str())
                });
                var p = (JsonObject)s["properties"];
                AiToolSpec.AddCommonSchema(p, _f,
                    $"Řazení podle id dimenze/měřítka výsledku nebo \"{AiAggregate.Count}\"; null = podle prvního měřítka sestupně.",
                    $"Max. počet vrácených řádků (výchozí {DefaultLimit}, max. {AiAggregate.MaxLimit}). Celkem se počítá vždy ze všech.");
                s["required"] = AiSchema.Arr(p.Select(x => x.Key));
                p["nadpis"]["description"] = "Krátký nadpis tabulky pro uživatele (max. 80 znaků).";
                p["dimenze"]["description"] = $"Id sloupců s rolí D, podle kterých se seskupuje (0–{AiAggregate.MaxDims}).";
                p["meritka"]["description"] = $"Id sloupců s rolí M, které se sčítají; \"{AiAggregate.Count}\" = počet záznamů. Alespoň jedno.";
                return s;
            }
        }

        public AiToolOutput Execute(JsonNode input, AiToolContext ctx) => Run(input, ctx, null);

        /// <summary>Provede agregaci; resultId = null → nové id výsledku (opakované spuštění dřívějšího výsledku předá jeho id).</summary>
        public AiToolOutput Run(JsonNode input, AiToolContext ctx, string resultId)
        {
            string prefix = AiToolJson.Str(input, "entita");
            if (AiToolEntities.Get(prefix, _f.CurrentUser) == null) return AiToolOutput.Error($"Entita '{prefix}' není dostupná.");
            var cat = ctx.Cached("cat-" + prefix, () => _agg.Catalog(prefix));

            var errors = new List<string>();
            var key = AiToolSpec.KeyResolver(cat, prefix, errors);
            var spec = new AiAggSpec
            {
                Entity = prefix,
                Dims = AiToolJson.StrList(input, "dimenze").Select(x => key(x, "Dimenze")).ToList(),
                Measures = AiToolJson.StrList(input, "meritka").Select(x => key(x, "Měřítko")).ToList()
            };
            if (spec.Measures.Count == 0) spec.Measures.Add(AiAggregate.Count);
            AiToolSpec.ReadCommon(input, spec, key, DefaultLimit);
            if (errors.Count == 0) errors.AddRange(_agg.Validate(spec, cat));
            if (errors.Count > 0) return AiToolOutput.Error(errors);

            AiAggregate.Plan plan;
            System.Data.DataTable dt;
            try
            {
                plan = _agg.Build(spec, cat);
                dt = _agg.Run(plan, MaxGroups);
            }
            catch (Exception ex)
            {
                return AiToolOutput.Error($"Dotaz nad databází selhal ({ex.Message}). Některé sloupce nejde seskupovat ani sčítat – zkus jiné dimenze nebo měřítka.");
            }
            if (dt.Rows.Count > MaxGroups)
                return AiToolOutput.Error($"Výsledek má více než {MaxGroups:N0} skupin. Zužte období, přidejte filtr nebo použijte hrubší dimenzi (např. měsíc místo dne).");

            var table = _agg.ToTable(spec, plan, dt);
            table.Id = resultId ?? ctx.NewResultId();

            var forModel = new
            {
                vysledek = table.Id,
                nadpis = table.Title,
                filtry = table.Description,
                sloupce = table.Columns.Select(c => c.Name),
                radky = AiToolSpec.CutForModel(table.Rows),
                celkem = table.Total,
                pocet_skupin = table.Groups,
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
