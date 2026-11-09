using System.Diagnostics;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace BL.AI.Core
{
    /// <summary>
    /// Klient Claude Messages API (https://api.anthropic.com/v1/messages).
    /// Používá structured outputs (output_config.format = json_schema) a prompt caching (cache_control).
    /// Registruje se jako scoped (konfigurace závisí na licenci přihlášeného uživatele, viz <see cref="AiOptions.ForLicense"/>);
    /// HttpClient je statický a sdílený, časový limit se uplatňuje na každý požadavek zvlášť.
    /// </summary>
    public class ClaudeClient : ILlmClient
    {
        private static readonly HttpClient _http = new HttpClient { Timeout = System.Threading.Timeout.InfiniteTimeSpan };
        private readonly Lazy<AiOptions> _options;
        public AiOptions Options => _options.Value;

        public ClaudeClient(AiOptions options)
        {
            _options = new Lazy<AiOptions>(options);
        }

        /// <summary>Konfigurace se vyhodnotí až při prvním použití (v době vytvoření scope ještě nemusí být známý přihlášený uživatel).</summary>
        public ClaudeClient(Func<AiOptions> options)
        {
            _options = new Lazy<AiOptions>(options);
        }

        public async Task<LlmResult> CompleteJsonAsync(LlmRequest request, CancellationToken ct = default)
        {
            var res = new LlmResult { Model = Options.Model };
            if (!Options.Enabled)
            {
                res.Error = "AI není v konfiguraci zapnuté nebo chybí API klíč.";
                return res;
            }

            var body = new JsonObject
            {
                ["model"] = Options.Model,
                ["max_tokens"] = request.MaxTokens ?? Options.MaxTokens,
                ["system"] = BuildSystem(request.System),
                ["messages"] = BuildMessages(request.Messages),
            };
            if (request.Schema != null)
            {
                body["output_config"] = new JsonObject
                {
                    ["format"] = new JsonObject
                    {
                        ["type"] = "json_schema",
                        ["schema"] = JsonNode.Parse(request.Schema.ToJsonString())
                    }
                };
            }

            var (root, error, ms) = await SendAsync(body, ct);
            res.ElapsedMs = ms;
            if (root == null) { res.Error = error; return res; }

            res.StopReason = root["stop_reason"]?.GetValue<string>();
            ReadUsage(root, res.Usage);

            var sb = new StringBuilder();
            foreach (var block in root["content"]?.AsArray() ?? new JsonArray())
            {
                if (block?["type"]?.GetValue<string>() == "text") sb.Append(block["text"]?.GetValue<string>());
            }
            res.Json = sb.ToString();

            switch (res.StopReason)
            {
                case "refusal":
                    res.Error = "AI odmítla odpovědět."; return res;
                case "max_tokens":
                    res.Error = "Odpověď AI byla zkrácena (max_tokens)."; return res;
            }

            try { using var _ = JsonDocument.Parse(res.Json); }
            catch { res.Error = "AI nevrátila platný JSON."; return res; }

            res.Ok = true;
            return res;
        }

        public async Task<LlmToolResult> CompleteToolsAsync(LlmToolRequest request, CancellationToken ct = default)
        {
            var res = new LlmToolResult { Model = Options.Model };
            if (!Options.Enabled)
            {
                res.Error = "AI není v konfiguraci zapnuté nebo chybí API klíč.";
                return res;
            }

            var tools = new JsonArray();
            foreach (var t in request.Tools)
            {
                tools.Add(new JsonObject
                {
                    ["name"] = t.Name,
                    ["description"] = t.Description,
                    ["input_schema"] = JsonNode.Parse(t.InputSchema.ToJsonString())
                });
            }

            // kopie zpráv: cache breakpoint na posledním bloku poslední zprávy (inkrementální cache konverzace)
            // se nesmí propsat do historie, jinak by breakpointů přibývalo (API jich dovolí max. 4)
            var messages = (JsonArray)JsonNode.Parse(request.Messages.ToJsonString());
            if (messages.Count > 0 && messages[messages.Count - 1]?["content"] is JsonArray lastContent && lastContent.Count > 0
                && lastContent[lastContent.Count - 1] is JsonObject lastBlock)
            {
                lastBlock["cache_control"] = new JsonObject { ["type"] = "ephemeral" };
            }

            var body = new JsonObject
            {
                ["model"] = Options.Model,
                ["max_tokens"] = request.MaxTokens ?? Options.MaxTokens,
                ["system"] = BuildSystem(request.System),     // cache breakpoint v system pokrývá i tools (pořadí cache: tools → system → messages)
                ["tools"] = tools,
                ["messages"] = messages,
            };
            if (!string.IsNullOrEmpty(request.ForceTool))
                body["tool_choice"] = new JsonObject { ["type"] = "tool", ["name"] = request.ForceTool };

            var (root, error, ms) = await SendAsync(body, ct);
            res.ElapsedMs = ms;
            if (root == null) { res.Error = error; return res; }

            res.StopReason = root["stop_reason"]?.GetValue<string>();
            ReadUsage(root, res.Usage);
            res.Content = root["content"] as JsonArray ?? new JsonArray();
            res.Content = (JsonArray)JsonNode.Parse(res.Content.ToJsonString());   // odpojit od root

            switch (res.StopReason)
            {
                case "refusal":
                    res.Error = "AI odmítla odpovědět."; return res;
                case "max_tokens":
                    res.Error = "Odpověď AI byla zkrácena (max_tokens)."; return res;
            }
            res.Ok = true;
            return res;
        }

        /// <summary>Odešle požadavek; vrací kořen JSON odpovědi, nebo null a text chyby. Společné pro CompleteJsonAsync a CompleteToolsAsync.</summary>
        private async Task<(JsonNode root, string error, long ms)> SendAsync(JsonObject body, CancellationToken ct)
        {
            using var msg = new HttpRequestMessage(HttpMethod.Post, Options.Endpoint);
            msg.Headers.Add("x-api-key", Options.ApiKey);
            msg.Headers.Add("anthropic-version", Options.ApiVersion);
            msg.Content = new StringContent(body.ToJsonString(), Encoding.UTF8);
            msg.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
            cts.CancelAfter(TimeSpan.FromSeconds(Options.TimeoutSeconds > 0 ? Options.TimeoutSeconds : 90));

            var sw = Stopwatch.StartNew();
            HttpResponseMessage resp;
            string text;
            try
            {
                resp = await _http.SendAsync(msg, cts.Token);
                text = await resp.Content.ReadAsStringAsync(cts.Token);
            }
            catch (OperationCanceledException) when (ct.IsCancellationRequested) { return (null, "Volání AI bylo zrušeno.", sw.ElapsedMilliseconds); }
            catch (OperationCanceledException) { return (null, $"Vypršel časový limit volání AI ({Options.TimeoutSeconds} s).", sw.ElapsedMilliseconds); }
            catch (Exception ex) { return (null, "Chyba spojení s AI: " + ex.Message, sw.ElapsedMilliseconds); }
            long ms = sw.ElapsedMilliseconds;

            JsonNode root;
            try { root = JsonNode.Parse(text); }
            catch { return (null, $"Neplatná odpověď AI (HTTP {(int)resp.StatusCode}).", ms); }

            if (!resp.IsSuccessStatusCode)
                return (null, $"AI API HTTP {(int)resp.StatusCode}: {root?["error"]?["message"]?.GetValue<string>() ?? text}", ms);
            return (root, null, ms);
        }

        private static void ReadUsage(JsonNode root, LlmUsage usage)
        {
            var u = root["usage"];
            if (u == null) return;
            usage.InputTokens = u["input_tokens"]?.GetValue<int>() ?? 0;
            usage.OutputTokens = u["output_tokens"]?.GetValue<int>() ?? 0;
            usage.CacheReadTokens = u["cache_read_input_tokens"]?.GetValue<int>() ?? 0;
            usage.CacheWriteTokens = u["cache_creation_input_tokens"]?.GetValue<int>() ?? 0;
        }

        private static JsonArray BuildSystem(List<LlmSystemBlock> blocks)
        {
            var arr = new JsonArray();
            foreach (var b in blocks.Where(p => !string.IsNullOrWhiteSpace(p.Text)))
            {
                var o = new JsonObject { ["type"] = "text", ["text"] = b.Text };
                if (b.Cache) o["cache_control"] = new JsonObject { ["type"] = "ephemeral" };
                arr.Add(o);
            }
            return arr;
        }

        private static JsonArray BuildMessages(List<LlmMessage> messages)
        {
            var arr = new JsonArray();
            foreach (var m in messages.Where(p => !string.IsNullOrWhiteSpace(p.Content)))
            {
                arr.Add(new JsonObject { ["role"] = m.Role == "assistant" ? "assistant" : "user", ["content"] = m.Content });
            }
            return arr;
        }
    }
}
