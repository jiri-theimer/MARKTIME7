using System.Text.Json.Nodes;
using BL.AI.Core;
using BO.AI;

namespace BL.AI.Tools
{
    /// <summary>Společné vlastnosti vstupu nástrojů agregace a hledej (filtry, období, stav, formát, archiv, řazení, limit): schéma a čtení.</summary>
    internal static class AiToolSpec
    {
        /// <summary>Doplní do vlastností schématu společné vstupy. sortHint = popis řazení pro daný nástroj.</summary>
        public static void AddCommonSchema(JsonObject props, BL.Factory f, string sortHint, string limitHint)
        {
            var filtr = AiSchema.Obj(new JsonObject
            {
                ["sloupec"] = AiSchema.Str(),
                ["op"] = AiSchema.StrEnum(BL.AI.Grid.AiGridFilterCodec.AllOps),
                ["hodnoty"] = AiSchema.Array(AiSchema.Str()),
                ["od"] = AiSchema.Nullable(AiSchema.Str()),
                ["do"] = AiSchema.Nullable(AiSchema.Str())
            });
            var obdobi = AiSchema.Obj(new JsonObject
            {
                ["pole"] = AiSchema.Str(),
                ["od"] = AiSchema.Nullable(AiSchema.Date()),
                ["do"] = AiSchema.Nullable(AiSchema.Date())
            });
            var razeni = AiSchema.Obj(new JsonObject
            {
                ["podle"] = AiSchema.Str(),
                ["sestupne"] = new JsonObject { ["type"] = "boolean" }
            });
            props["filtry"] = AiSchema.Array(filtr);
            props["obdobi"] = AiSchema.Nullable(obdobi);
            props["stav"] = AiSchema.Nullable(new JsonObject { ["type"] = "integer" });
            props["format"] = AiSchema.Nullable(AiSchema.Str());
            props["archiv"] = AiSchema.Nullable(new JsonObject { ["type"] = "integer" });
            props["razeni"] = AiSchema.Nullable(razeni);
            props["limit"] = AiSchema.Nullable(new JsonObject { ["type"] = "integer" });

            props["filtry"]["description"] = "Sloupcové filtry (A ZÁROVEŇ). hodnoty = hledané texty (více = NEBO); interval: od/do (číslo nebo yyyy-MM-dd, otevřená mez null).";
            props["obdobi"]["description"] = "Časové období podle pole z katalogu (OBDOBÍ). Otevřenou mez nech null.";
            props["stav"]["description"] = "Stav úkonů podle katalogu (jen entity, které jej mají); null = všechny.";
            props["format"]["description"] = "Formát úkonů podle katalogu; null = všechny.";
            props["archiv"]["description"] = "1 = jen otevřené (výchozí), 2 = jen archiv, 0 = obojí.";
            props["razeni"]["description"] = sortHint;
            props["limit"]["description"] = limitHint;
        }

        /// <summary>
        /// Přečte společné vstupy do zadání. key(id, co) převede id sloupce z katalogu na klíč (chybu zapíše do errors).
        /// </summary>
        public static void ReadCommon(JsonNode input, AiQuerySpec s, Func<string, string, string> key, int defaultLimit)
        {
            s.Title = AiToolJson.Str(input, "nadpis");
            if (s.Title?.Length > 120) s.Title = s.Title.Substring(0, 120);
            s.State = AiToolJson.Int(input, "stav");
            if (s.State == 0) s.State = null;
            s.Tab = AiToolJson.Str(input, "format");
            s.Archive = AiToolJson.Int(input, "archiv") ?? 1;
            s.Limit = AiToolJson.Int(input, "limit") ?? defaultLimit;

            if (input?["filtry"] is JsonArray fa)
            {
                foreach (var f in fa)
                {
                    s.Filter.Add(new AiGridFilter
                    {
                        Column = key(AiToolJson.Str(f, "sloupec"), "Filtr"),
                        Op = AiToolJson.Str(f, "op"),
                        Values = AiToolJson.StrList(f, "hodnoty"),
                        From = AiToolJson.Str(f, "od"),
                        To = AiToolJson.Str(f, "do")
                    });
                }
            }

            if (input?["obdobi"] is JsonObject ob)
            {
                s.Period = new AiAggPeriod
                {
                    Field = AiToolJson.Str(ob, "pole"),
                    D1 = AiToolJson.Str(ob, "od") ?? "2000-01-01",       // otevřená mez
                    D2 = AiToolJson.Str(ob, "do") ?? "2099-12-31"
                };
            }

            if (input?["razeni"] is JsonObject rz)
            {
                string by = AiToolJson.Str(rz, "podle");
                if (by != null) s.SortBy = key(by, "Řazení");
                s.SortDesc = AiToolJson.Bool(rz, "sestupne") ?? true;
            }
        }

        /// <summary>Převodník id sloupce z katalogu na klíč; "pocet" zůstává.</summary>
        public static Func<string, string, string> KeyResolver(AiColumnCatalog cat, string prefix, List<string> errors) => (id, what) =>
        {
            if (id == AiAggregate.Count) return id;
            var e = cat.ById(id) ?? cat.ByKey(id);
            if (e == null) { errors.Add($"{what}: neznámé id sloupce '{id}' (použij id z katalogu entity {prefix})."); return id; }
            return e.Key;
        };

        /// <summary>Texty do výsledku pro model zkrátit (dlouhé popisy úkonů zbytečně spotřebují tokeny).</summary>
        public static List<object[]> CutForModel(List<object[]> rows, int maxText = 200) =>
            rows.Select(r => r.Select(v => v is string s && s.Length > maxText ? s.Substring(0, maxText) + "…" : v).ToArray()).ToList();
    }
}
