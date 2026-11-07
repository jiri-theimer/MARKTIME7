using System.Text.Json;
using System.Text.Json.Nodes;
using BL.AI.Core;
using BL.AI.Tools;
using BO.AI;

namespace BL.AI.Ask
{
    /// <summary>
    /// Obecný AI agent „Zeptej se MARKTIME“: odpovídá na otázky nad daty pomocí nástrojů (katalog, agregace, hledej).
    /// Odpověď je strukturovaná: text modelu + tabulky z výsledků nástrojů (data z BL, model je nepřepisuje).
    /// Zadání zobrazených tabulek se ukládá do p85TempBox (guid) – Pivot-Explorer a zdrojové úkony je načtou znovu.
    /// </summary>
    public class AiAskService : AiToolAgentBase<AiAskService.AskResult>
    {
        public const string FeatureCode = "ai-ask";
        /// <summary>p85Prefix uloženého zadání tabulky.</summary>
        public const string TempPrefix = "aiask";

        private readonly AiAggregate _agg;
        private AgregaceTool _agregace;
        private HledejTool _hledej;

        public AiAskService(BL.Factory f, BL.TheColumnsProvider cp, ILlmClient llm) : base(f, llm)
        {
            _agg = new AiAggregate(f, cp);
        }

        public AiAggregate Aggregate => _agg;

        public class AskResult : AiToolAskResult
        {
            public List<AiAskTable> Tables { get; set; } = new();
        }

        protected override string Feature => FeatureCode;
        protected override string FinalToolName => "odpoved";

        protected override string FinalToolDescription =>
            "Koncová odpověď uživateli: text a id výsledků (tabulky), které se mají zobrazit. Zavolej vždy jako poslední krok, i když se jen doptáváš.";

        private static readonly string[] _days = { "neděle", "pondělí", "úterý", "středa", "čtvrtek", "pátek", "sobota" };

        protected override List<LlmSystemBlock> BuildSystem() => new()
        {
            new LlmSystemBlock { Text = AiPromptStore.Get("ask.system.md") },
            new LlmSystemBlock { Text = AiToolEntities.ToPromptText(F), Cache = true },
            new LlmSystemBlock { Text = $"Dnešní datum: {DateTime.Today:yyyy-MM-dd} ({_days[(int)DateTime.Today.DayOfWeek]}).\nPřihlášený uživatel: {F.CurrentUser.j02FirstName} {F.CurrentUser.j02LastName}." }
        };

        protected override List<IAiTool> BuildTools()
        {
            _agregace = new AgregaceTool(F, _agg);
            _hledej = new HledejTool(F, _agg);
            return new List<IAiTool> { new KatalogTool(F, _agg), _agregace, _hledej };
        }

        protected override JsonObject BuildFinalSchema()
        {
            var s = AiSchema.Obj(new JsonObject
            {
                ["text"] = AiSchema.Str(),
                ["tabulky"] = AiSchema.Array(AiSchema.Obj(new JsonObject
                {
                    ["vysledek"] = AiSchema.Str(),
                    ["nadpis"] = AiSchema.Nullable(AiSchema.Str())
                }))
            });
            var p = (JsonObject)s["properties"];
            p["text"]["description"] = "Odpověď uživateli – prostý text, 1–4 věty.";
            p["tabulky"]["description"] = "Výsledky nástrojů agregace a hledej k zobrazení (id vysledek, např. r1), v pořadí zobrazení. Prázdné, když se doptáváš.";
            return s;
        }

        protected override void ProcessFinal(JsonNode final, string text, AiToolContext ctx, AskResult res)
        {
            res.Reply = (final?["text"] is JsonValue v && v.TryGetValue<string>(out var t) && !string.IsNullOrWhiteSpace(t)) ? t.Trim() : text;
            if (final?["tabulky"] is not JsonArray tabs) return;

            var used = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var tab in tabs)
            {
                string id = tab?["vysledek"] is JsonValue iv && iv.TryGetValue<string>(out var s) ? s.Trim() : null;
                if (string.IsNullOrEmpty(id) || !used.Add(id)) continue;

                var outcome = ctx.Results.TryGetValue(id, out var o) ? o as AiAggOutcome : null;
                if (outcome == null && ctx.ThreadResults.TryGetValue(id, out var prior))
                {
                    // výsledek z dřívějšího dotazu konverzace – data se načtou znovu
                    AiToolOutput r = prior.Tool == _agregace.Name ? _agregace.Run(prior.Input, ctx, id)
                                   : prior.Tool == _hledej.Name ? _hledej.Run(prior.Input, ctx, id) : null;
                    if (r != null && !r.IsError) outcome = r.Data as AiAggOutcome;
                }
                if (outcome == null) { res.Warnings.Add($"Výsledek {id} nebyl nalezen."); continue; }

                var table = outcome.Table;
                if (tab["nadpis"] is JsonValue nv && nv.TryGetValue<string>(out var title) && !string.IsNullOrWhiteSpace(title)) table.Title = title.Trim();
                table.Guid = SaveSpec(outcome.Spec);
                res.Tables.Add(table);
            }
        }

        // ------------------------------------------------------------------ uložené zadání tabulky (p85TempBox)

        private string SaveSpec(AiQuerySpec spec)
        {
            string guid = BO.Code.Bas.GetGuid();
            F.p85TempboxBL.Save(new BO.p85Tempbox
            {
                p85GUID = guid,
                p85Prefix = TempPrefix,
                p85OtherKey1 = F.CurrentUser.pid,
                p85FreeDate01 = DateTime.Now,
                p85FreeText01 = spec is AiListSpec ? "list" : "agg",
                p85Message = spec is AiListSpec l ? JsonSerializer.Serialize(l) : JsonSerializer.Serialize((AiAggSpec)spec)
            });
            return guid;
        }

        /// <summary>Zadání tabulky podle guid (AiAggSpec nebo AiListSpec) – jen vlastní záznamy přihlášeného uživatele; null = nenalezeno.</summary>
        public static AiQuerySpec LoadSpec(BL.Factory f, string guid)
        {
            if (string.IsNullOrEmpty(guid) || guid.Length > 50) return null;
            var rec = f.p85TempboxBL.LoadByGuid(guid);
            if (rec == null || rec.p85Prefix != TempPrefix || rec.p85OtherKey1 != f.CurrentUser.pid || string.IsNullOrEmpty(rec.p85Message)) return null;
            try
            {
                return rec.p85FreeText01 == "list"
                    ? JsonSerializer.Deserialize<AiListSpec>(rec.p85Message)
                    : JsonSerializer.Deserialize<AiAggSpec>(rec.p85Message);
            }
            catch { return null; }
        }
    }
}
