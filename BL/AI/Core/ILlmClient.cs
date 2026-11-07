using System.Text.Json.Nodes;

namespace BL.AI.Core
{
    public interface ILlmClient
    {
        AiOptions Options { get; }

        /// <summary>Jedno volání modelu s vynuceným JSON výstupem podle schématu.</summary>
        Task<LlmResult> CompleteJsonAsync(LlmRequest request, CancellationToken ct = default);

        /// <summary>Jedno kolo tool use: model vrátí text a/nebo požadavky na volání nástrojů (tool_use). Cyklus řídí AiToolAgentBase.</summary>
        Task<LlmToolResult> CompleteToolsAsync(LlmToolRequest request, CancellationToken ct = default);
    }

    /// <summary>Definice nástroje pro model (Messages API: tools[]).</summary>
    public class LlmTool
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public JsonObject InputSchema { get; set; }
    }

    public class LlmToolRequest
    {
        public List<LlmSystemBlock> System { get; set; } = new();
        public List<LlmTool> Tools { get; set; } = new();
        /// <summary>Zprávy ve formátu Messages API (role + content jako pole bloků text / tool_use / tool_result). Klient je neupravuje.</summary>
        public JsonArray Messages { get; set; } = new();
        /// <summary>Vynutit volání tohoto nástroje (tool_choice = tool), null = model rozhodne sám.</summary>
        public string ForceTool { get; set; }
        public int? MaxTokens { get; set; }
    }

    public class LlmToolResult
    {
        public bool Ok { get; set; }
        public string Error { get; set; }
        public string StopReason { get; set; }
        public string Model { get; set; }
        public LlmUsage Usage { get; set; } = new();
        public long ElapsedMs { get; set; }
        /// <summary>Obsah odpovědi asistenta (bloky text / tool_use) – vkládá se beze změny zpět do konverzace.</summary>
        public JsonArray Content { get; set; } = new();
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
