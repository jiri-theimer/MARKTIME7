using System.Text.Json.Nodes;

namespace BL.AI.Core
{
    public interface ILlmClient
    {
        AiOptions Options { get; }

        /// <summary>Jedno volání modelu s vynuceným JSON výstupem podle schématu.</summary>
        Task<LlmResult> CompleteJsonAsync(LlmRequest request, CancellationToken ct = default);
    }

    public class LlmSystemBlock
    {
        public string Text { get; set; }
        /// <summary>true = za tento blok se umístí cache breakpoint (prompt caching).</summary>
        public bool Cache { get; set; }
    }

    public class LlmMessage
    {
        public string Role { get; set; }      // "user" | "assistant"
        public string Content { get; set; }
    }

    public class LlmRequest
    {
        public List<LlmSystemBlock> System { get; set; } = new();
        public List<LlmMessage> Messages { get; set; } = new();
        public JsonObject Schema { get; set; }
        public int? MaxTokens { get; set; }
    }

    public class LlmUsage
    {
        public int InputTokens { get; set; }
        public int OutputTokens { get; set; }
        public int CacheReadTokens { get; set; }
        public int CacheWriteTokens { get; set; }
        public override string ToString() => $"in={InputTokens} out={OutputTokens} cache_read={CacheReadTokens} cache_write={CacheWriteTokens}";
    }

    public class LlmResult
    {
        public bool Ok { get; set; }
        public string Json { get; set; }
        public string Error { get; set; }
        public string StopReason { get; set; }
        public string Model { get; set; }
        public LlmUsage Usage { get; set; } = new();
        public long ElapsedMs { get; set; }
    }
}
