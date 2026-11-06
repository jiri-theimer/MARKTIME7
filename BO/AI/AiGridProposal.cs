using System.Text.Json.Serialization;

namespace BO.AI
{
    /// <summary>Návrh sloupců tabulky (j72TheGridTemplate.j72Columns) od AI agenta.</summary>
    public class AiGridProposal
    {
        /// <summary>Název pro novou pojmenovanou tabulku.</summary>
        [JsonPropertyName("name")] public string Name { get; set; }
        /// <summary>Klíče sloupců v pořadí zobrazení (relace__Entita__Field).</summary>
        [JsonPropertyName("columns")] public List<string> Columns { get; set; } = new();
    }

    /// <summary>Kompletní odpověď modelu.</summary>
    public class AiGridAnswer
    {
        [JsonPropertyName("reply")] public string Reply { get; set; }
        /// <summary>null, pokud agent zatím nemá dost informací.</summary>
        [JsonPropertyName("proposal")] public AiGridProposal Proposal { get; set; }
    }

    /// <summary>Vstup z prohlížeče pro akci Ask agenta tabulky.</summary>
    public class AiGridAskInput : AiAskInput
    {
        /// <summary>Aktuální stav formuláře (uživatel mohl návrh ručně upravit).</summary>
        [JsonPropertyName("current")] public AiGridProposal Current { get; set; }
        /// <summary>j72ID tabulky, ze které bylo okno otevřeno (určuje entitu).</summary>
        [JsonPropertyName("j72id")] public int J72ID { get; set; }
    }
}
