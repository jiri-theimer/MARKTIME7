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
    /// Registrovat jako singleton – HttpClient je sdílený.
    /// </summary>
    public class ClaudeClient : ILlmClient
    {
        private readonly HttpClient _http;
        public AiOptions Options { get; }

        public ClaudeClient(AiOptions options)
        {
            Options = options;
            _http = new HttpClient { Timeout = TimeSpan.FromSeconds(options.TimeoutSeconds) };
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

            using var msg = new HttpRequestMessage(HttpMethod.Post, Options.Endpoint);
            msg.Headers.Add("x-api-key", Options.ApiKey);
            msg.Headers.Add("anthropic-version", Options.ApiVersion);
            msg.Content = new StringContent(body.ToJsonString(), Encoding.UTF8);
            msg.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            var sw = Stopwatch.StartNew();
            HttpResponseMessage resp;
            string text;
            try
            {
                resp = await _http.SendAsync(msg, ct);
                text = await resp.Content.ReadAsStringAsync(ct);
            }
            catch (TaskCanceledException)
            {
                res.Error = "Vypršel časový limit volání AI.";
                return res;
            }
            catch (Exception ex)
            {
                res.Error = "Chyba spojení s AI: " + ex.Message;
                return res;
            }
            res.ElapsedMs = sw.ElapsedMilliseconds;

            JsonNode root;
            try { root = JsonNode.Parse(text); }
            catch { res.Error = $"Neplatná odpověď AI (HTTP {(int)resp.StatusCode})."; return res; }

            if (!resp.IsSuccessStatusCode)
            {
                res.Error = $"AI API HTTP {(int)resp.StatusCode}: {root?["error"]?["message"]?.GetValue<string>() ?? text}";
                return res;
            }

            res.StopReason = root["stop_reason"]?.GetValue<string>();
            var u = root["usage"];
            if (u != null)
            {
                res.Usage.InputTokens = u["input_tokens"]?.GetValue<int>() ?? 0;
                res.Usage.OutputTokens = u["output_tokens"]?.GetValue<int>() ?? 0;
                res.Usage.CacheReadTokens = u["cache_read_input_tokens"]?.GetValue<int>() ?? 0;
                res.Usage.CacheWriteTokens = u["cache_creation_input_tokens"]?.GetValue<int>() ?? 0;
            }

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
