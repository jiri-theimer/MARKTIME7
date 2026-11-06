using System.Text.Json.Serialization;

namespace BO.AI
{
    /// <summary>Společný vstup z chatu (aichat.js) pro všechny AI agenty. Agent si jej může rozšířit.</summary>
    public class AiAskInput
    {
        [JsonPropertyName("messages")] public List<AiChatMessage> Messages { get; set; } = new();
        /// <summary>Identifikátor konverzace (generuje prohlížeč) → j98Guid.</summary>
        [JsonPropertyName("conversation")] public string ConversationGuid { get; set; }
        /// <summary>Stránka, ze které byl agent otevřen → j98ContextPage.</summary>
        [JsonPropertyName("page")] public string ContextPage { get; set; }
    }

    public class AiChatMessage
    {
        /// <summary>user | assistant</summary>
        [JsonPropertyName("role")] public string Role { get; set; }
        [JsonPropertyName("content")] public string Content { get; set; }
    }
}
