using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace BL.AI.Core
{
    /// <summary>Výsledek dotazu na agenta s nástroji.</summary>
    public class AiToolAskResult : AiAskResult
    {
        /// <summary>Celá konverzace ve formátu Messages API (JSON) – prohlížeč ji pošle s dalším dotazem.</summary>
        public string Thread { get; set; }
        public int ToolCalls { get; set; }
    }

    /// <summary>
    /// Základ AI agenta s nástroji (tool use): cyklus model → nástroje → model, dokud model nezavolá koncový nástroj
    /// (FinalToolName) nebo neodpoví textem. Limit volání nástrojů na jeden dotaz, po jeho vyčerpání se odpověď vynutí.
    /// Konverzace (včetně volání nástrojů a jejich výsledků) se drží v prohlížeči a posílá se zpět – server je bezstavový.
    /// Jeden dotaz uživatele = jeden záznam j98OpenAILog; volání nástrojů jsou v j98ExtraJson.
    /// </summary>
    public abstract class AiToolAgentBase<TResult> where TResult : AiToolAskResult, new()
    {
        protected readonly BL.Factory F;
        protected readonly ILlmClient Llm;

        protected AiToolAgentBase(BL.Factory f, ILlmClient llm) { F = f; Llm = llm; }

        /// <summary>Kód agenta do j98Feature.</summary>
        protected abstract string Feature { get; }

        /// <summary>Koncový nástroj – jeho vstup je strukturovaná odpověď uživateli.</summary>
        protected abstract string FinalToolName { get; }

        /// <summary>Max. počet volání nástrojů (mimo koncový) na jeden dotaz uživatele.</summary>
        protected virtual int MaxToolCalls => 8;

        /// <summary>Kolik posledních dotazů uživatele (včetně jejich volání nástrojů) se posílá modelu.</summary>
        protected virtual int MaxTurns => 6;

        /// <summary>Max. velikost konverzace z prohlížeče (znaky JSON) – ochrana proti nafouknutí požadavku.</summary>
        protected virtual int MaxThreadChars => 1_500_000;

        public virtual bool IsEnabled => Llm.Options.Enabled;

        protected abstract List<LlmSystemBlock> BuildSystem();

        /// <summary>Nástroje agenta bez koncového.</summary>
        protected abstract List<IAiTool> BuildTools();

        protected abstract JsonObject BuildFinalSchema();
        protected abstract string FinalToolDescription { get; }

        /// <summary>Zpracuje koncovou odpověď (vstup koncového nástroje, nebo null při odpovědi prostým textem) a naplní výsledek.</summary>
        protected abstract void ProcessFinal(JsonNode final, string text, AiToolContext ctx, TResult res);

        private const string FinalAck = "Odpověď byla zobrazena uživateli.";

        public async Task<TResult> AskAsync(BO.AI.AiToolAskInput input, CancellationToken ct = default)
        {
            var res = new TResult();
            if (!IsEnabled) { res.Error = "AI není zapnuté."; return res; }
            string question = input?.Messages?.LastOrDefault(p => p.Role == "user")?.Content?.Trim();
            if (string.IsNullOrEmpty(question)) { res.Error = "Chybí dotaz."; return res; }
            if ((input.Thread?.Length ?? 0) > MaxThreadChars) { res.Error = "Konverzace je příliš dlouhá – začněte novou."; return res; }

            JsonArray thread;
            try { thread = string.IsNullOrWhiteSpace(input.Thread) ? new JsonArray() : (JsonArray)JsonNode.Parse(input.Thread); }
            catch { thread = new JsonArray(); }
            thread = Trim(thread);

            var ctx = new AiToolContext { F = F };
            ScanThread(thread, ctx);

            // nový dotaz: navázat na koncovou zprávu s výsledky nástrojů (po koncovém nástroji končí konverzace zprávou user)
            var q = new JsonObject { ["type"] = "text", ["text"] = question };
            if (thread.Count > 0 && thread[thread.Count - 1]?["role"]?.GetValue<string>() == "user" && thread[thread.Count - 1]["content"] is JsonArray lastUser)
                lastUser.Add(q);
            else
                thread.Add(new JsonObject { ["role"] = "user", ["content"] = new JsonArray { q } });

            var tools = BuildTools();
            var req = new LlmToolRequest
            {
                System = BuildSystem(),
                Tools = tools.Select(t => new LlmTool { Name = t.Name, Description = t.Description, InputSchema = t.InputSchema }).ToList(),
                Messages = thread
            };
            req.Tools.Add(new LlmTool { Name = FinalToolName, Description = FinalToolDescription, InputSchema = BuildFinalSchema() });

            var log = new AiLogEntry
            {
                Feature = Feature,
                ContextPage = input.ContextPage,
                ConversationGuid = input.ConversationGuid,
                TurnIndex = input.Messages.Count(p => p.Role == "user"),
                QueryText = question
            };
            var calls = new JsonArray();

            try
            {
                await RunAsync(req, tools, ctx, res, log, calls, ct);
            }
            catch (Exception ex)
            {
                res.Ok = false;
                res.Error = "Chyba AI agenta: " + ex.Message;
            }

            res.Thread = thread.ToJsonString();
            log.AnswerText = res.Reply;
            log.ExtraJson = new JsonObject { ["tools"] = calls }.ToJsonString(_logJson);
            if (!res.Ok) { log.Status = 'E'; log.ErrorText = res.Error; }
            else if (res.Warnings.Count > 0) { log.Status = 'V'; log.ErrorText = string.Join("; ", res.Warnings); }
            res.LogId = AiLog.Insert(F, log);
            return res;
        }

        private async Task RunAsync(LlmToolRequest req, List<IAiTool> tools, AiToolContext ctx, TResult res, AiLogEntry log, JsonArray calls, CancellationToken ct)
        {
            var thread = req.Messages;
            while (true)
            {
                req.ForceTool = res.ToolCalls >= MaxToolCalls ? FinalToolName : null;
                var r = await Llm.CompleteToolsAsync(req, ct);
                log.Add(r);
                if (!r.Ok) { res.Error = r.Error; return; }

                thread.Add(new JsonObject { ["role"] = "assistant", ["content"] = r.Content });
                var uses = r.Content.OfType<JsonObject>().Where(b => b["type"]?.GetValue<string>() == "tool_use").ToList();
                string text = string.Join("\n", r.Content.OfType<JsonObject>()
                    .Where(b => b["type"]?.GetValue<string>() == "text").Select(b => b["text"]?.GetValue<string>()).Where(s => !string.IsNullOrWhiteSpace(s)));

                if (uses.Count == 0)
                {
                    // odpověď prostým textem (bez koncového nástroje)
                    log.AnswerJson = r.Content.ToJsonString();
                    ProcessFinal(null, text, ctx, res);
                    res.Ok = true;
                    return;
                }

                var results = new JsonArray();
                JsonNode final = null;
                foreach (var u in uses)
                {
                    string id = u["id"]?.GetValue<string>();
                    string name = u["name"]?.GetValue<string>();
                    var input = u["input"];

                    if (name == FinalToolName)
                    {
                        final = input;
                        results.Add(ToolResult(id, FinalAck, false));
                        continue;
                    }

                    var call = new JsonObject { ["n"] = name, ["in"] = input?.DeepClone() };
                    AiToolOutput o;
                    var sw = Stopwatch.StartNew();
                    var tool = tools.FirstOrDefault(t => t.Name == name);
                    if (tool == null) o = AiToolOutput.Error($"Neznámý nástroj: {name}");
                    else if (res.ToolCalls >= MaxToolCalls) o = AiToolOutput.Error($"Překročen limit {MaxToolCalls} volání nástrojů na jeden dotaz. Odpověz uživateli z toho, co už máš.");
                    else
                    {
                        res.ToolCalls++;
                        try { o = tool.Execute(input, ctx) ?? AiToolOutput.Error("Nástroj nevrátil výsledek."); }
                        catch (Exception ex) { o = AiToolOutput.Error("Chyba při provádění nástroje: " + ex.Message); }
                    }
                    if (!o.IsError && o.ResultId != null) ctx.Results[o.ResultId] = o.Data;

                    call["ok"] = !o.IsError;
                    call["ms"] = sw.ElapsedMilliseconds;
                    if (o.ResultId != null) call["id"] = o.ResultId;
                    if (o.Rows > 0) call["rows"] = o.Rows;
                    if (o.IsError) call["err"] = Cut(o.Content, 500);
                    calls.Add(call);

                    results.Add(ToolResult(id, o.Content, o.IsError));
                }
                thread.Add(new JsonObject { ["role"] = "user", ["content"] = results });

                if (final != null)
                {
                    log.AnswerJson = final.ToJsonString();
                    ProcessFinal(final, text, ctx, res);
                    res.Ok = true;
                    return;
                }
            }
        }

        private static JsonObject ToolResult(string toolUseId, string content, bool isError)
        {
            var o = new JsonObject { ["type"] = "tool_result", ["tool_use_id"] = toolUseId, ["content"] = content ?? "" };
            if (isError) o["is_error"] = true;
            return o;
        }

        /// <summary>
        /// Ponechá posledních MaxTurns dotazů uživatele. Dotaz začíná zprávou user s textovým blokem;
        /// řez nesmí oddělit tool_use od jeho tool_result – proto se začátek hledá jen na zprávách user bez tool_result
        /// nebo na koncové zprávě s potvrzením odpovědi, za kterou následuje text dalšího dotazu.
        /// </summary>
        private JsonArray Trim(JsonArray thread)
        {
            var starts = new List<int>();
            for (int i = 0; i < thread.Count; i++)
            {
                if (thread[i]?["role"]?.GetValue<string>() != "user" || thread[i]["content"] is not JsonArray c) continue;
                if (c.OfType<JsonObject>().Any(b => b["type"]?.GetValue<string>() == "text")) starts.Add(i);
            }
            if (starts.Count <= MaxTurns) return thread;

            int from = starts[starts.Count - MaxTurns];
            var cut = new JsonArray();
            for (int i = from; i < thread.Count; i++)
            {
                var m = thread[i].DeepClone();
                if (i == from && m["content"] is JsonArray c)
                {
                    // tool_result patří k tool_use v oříznuté části → ponechat jen text
                    var keep = c.OfType<JsonObject>().Where(b => b["type"]?.GetValue<string>() == "text").Select(b => b.DeepClone()).ToArray();
                    m["content"] = new JsonArray(keep);
                }
                cut.Add(m);
            }
            return cut;
        }

        /// <summary>Výsledky dřívějších volání v konverzaci: ID výsledku (z obsahu tool_result) → nástroj a vstup.</summary>
        private static void ScanThread(JsonArray thread, AiToolContext ctx)
        {
            var uses = new Dictionary<string, (string, JsonNode)>();
            int max = 0;
            foreach (var m in thread)
            {
                if (m?["content"] is not JsonArray content) continue;
                foreach (var b in content.OfType<JsonObject>())
                {
                    string type = b["type"]?.GetValue<string>();
                    if (type == "tool_use") uses[b["id"]?.GetValue<string>() ?? ""] = (b["name"]?.GetValue<string>(), b["input"]);
                    else if (type == "tool_result" && b["content"] is JsonValue v && v.TryGetValue<string>(out var s) && s.StartsWith("{"))
                    {
                        string rid = null;
                        try { rid = JsonNode.Parse(s)?["vysledek"]?.GetValue<string>(); } catch { }
                        if (rid == null || !uses.TryGetValue(b["tool_use_id"]?.GetValue<string>() ?? "", out var u)) continue;
                        ctx.ThreadResults[rid] = (u.Item1, u.Item2?.DeepClone());
                        if (rid.Length > 1 && int.TryParse(rid.Substring(1), out int n)) max = Math.Max(max, n);
                    }
                }
            }
            ctx.SeedResultIndex(max);
        }

        private static string Cut(string s, int len) => s == null ? null : (s.Length > len ? s.Substring(0, len) : s);

        private static readonly JsonSerializerOptions _logJson = new() { Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping };
    }
}
