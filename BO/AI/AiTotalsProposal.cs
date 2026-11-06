using System.Text.Json.Serialization;

namespace BO.AI
{
    /// <summary>Návrh šablony Součtů (j79) od AI agenta. Názvy vlastností odpovídají JSON schématu pro model.</summary>
    public class AiTotalsProposal
    {
        [JsonPropertyName("name")] public string Name { get; set; }
        /// <summary>Klíče dimenzí do řádků pivotu.</summary>
        [JsonPropertyName("rows")] public List<string> Rows { get; set; } = new();
        /// <summary>Klíče dimenzí do sloupců pivotu.</summary>
        [JsonPropertyName("columns")] public List<string> Columns { get; set; } = new();
        [JsonPropertyName("measures")] public List<AiTotalsMeasure> Measures { get; set; } = new();
        /// <summary>j79StateQuery, 0 = všechny stavy.</summary>
        [JsonPropertyName("state")] public int State { get; set; }
        /// <summary>j79TabQuery: "" | time | kusovnik | expense | fee.</summary>
        [JsonPropertyName("tab")] public string Tab { get; set; } = "";
        [JsonPropertyName("period")] public AiTotalsPeriod Period { get; set; } = new();
        /// <summary>j79Chart: 0 bez grafu, 1 sloupcový graf.</summary>
        [JsonPropertyName("chart")] public int Chart { get; set; }
    }

    public class AiTotalsMeasure
    {
        [JsonPropertyName("key")] public string Key { get; set; }
        /// <summary>sum | average | count | min | max (agregace WebDataRocks).</summary>
        [JsonPropertyName("aggregation")] public string Aggregation { get; set; } = "sum";
    }

    public class AiTotalsPeriod
    {
        /// <summary>pid z ThePeriodProvider (0 = bez omezení, 1 = vlastní rozsah d1–d2).</summary>
        [JsonPropertyName("value")] public int Value { get; set; }
        [JsonPropertyName("field")] public string Field { get; set; } = "p31Date";
        [JsonPropertyName("d1")] public string D1 { get; set; }   // yyyy-MM-dd
        [JsonPropertyName("d2")] public string D2 { get; set; }
    }

    /// <summary>Kompletní odpověď modelu.</summary>
    public class AiTotalsAnswer
    {
        /// <summary>Text pro uživatele (vysvětlení návrhu nebo doplňující otázka).</summary>
        [JsonPropertyName("reply")] public string Reply { get; set; }
        /// <summary>null, pokud agent zatím nemá dost informací.</summary>
        [JsonPropertyName("proposal")] public AiTotalsProposal Proposal { get; set; }
        /// <summary>Které filtry si má uživatel potvrdit: period | state | tab.</summary>
        [JsonPropertyName("confirm")] public List<string> Confirm { get; set; } = new();
    }

    /// <summary>Položka katalogu sloupců Součtů.</summary>
    public class AiCatalogItem
    {
        [JsonPropertyName("key")] public string Key { get; set; }
        [JsonPropertyName("header")] public string Header { get; set; }
        /// <summary>measure | dimension</summary>
        [JsonPropertyName("role")] public string Role { get; set; }
        [JsonPropertyName("group")] public string Group { get; set; }
        [JsonPropertyName("type")] public string Type { get; set; }
        [JsonPropertyName("hint")] public string Hint { get; set; }
    }

    /// <summary>Vstup z prohlížeče pro akci Ask.</summary>
    public class AiTotalsAskInput
    {
        [JsonPropertyName("messages")] public List<AiChatMessage> Messages { get; set; } = new();
        /// <summary>Aktuální stav formuláře (uživatel mohl návrh ručně upravit).</summary>
        [JsonPropertyName("current")] public AiTotalsProposal Current { get; set; }
        /// <summary>Identifikátor konverzace (generuje prohlížeč) → j98Guid.</summary>
        [JsonPropertyName("conversation")] public string ConversationGuid { get; set; }
        [JsonPropertyName("page")] public string ContextPage { get; set; }
    }

    public class AiChatMessage
    {
        [JsonPropertyName("role")] public string Role { get; set; }
        [JsonPropertyName("content")] public string Content { get; set; }
    }
}
