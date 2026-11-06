using System.Text.Json.Serialization;

namespace BO.AI
{
    /// <summary>Návrh sloupců tabulky (j72TheGridTemplate.j72Columns) a sloupcového filtru (stav gridu j75Filter) od AI agenta.</summary>
    public class AiGridProposal
    {
        /// <summary>Název pro novou pojmenovanou tabulku.</summary>
        [JsonPropertyName("name")] public string Name { get; set; }
        /// <summary>Klíče sloupců v pořadí zobrazení (relace__Entita__Field).</summary>
        [JsonPropertyName("columns")] public List<string> Columns { get; set; } = new();
        /// <summary>Sloupcový filtr gridu (ukládá se do stavu gridu j75Filter).</summary>
        [JsonPropertyName("filter")] public List<AiGridFilter> Filter { get; set; } = new();
        /// <summary>true = stačí splnit jednu z podmínek (j75OperatorFlag = 1, NEBO), false = všechny (A ZÁROVEŇ).</summary>
        [JsonPropertyName("matchAny")] public bool MatchAny { get; set; }
    }

    /// <summary>Jedna podmínka sloupcového filtru.</summary>
    public class AiGridFilter
    {
        /// <summary>Klíč sloupce (u modelu krátké id).</summary>
        [JsonPropertyName("column")] public string Column { get; set; }
        /// <summary>contains | notcontains | starts | equal | notequal | empty | notempty | interval | gtzero | zeroorempty | yes | no</summary>
        [JsonPropertyName("op")] public string Op { get; set; }
        /// <summary>Hledané texty (více hodnot = NEBO).</summary>
        [JsonPropertyName("values")] public List<string> Values { get; set; } = new();
        /// <summary>Interval od: číslo nebo datum yyyy-MM-dd; null = neomezeno.</summary>
        [JsonPropertyName("from")] public string From { get; set; }
        /// <summary>Interval do: číslo nebo datum yyyy-MM-dd; null = neomezeno.</summary>
        [JsonPropertyName("to")] public string To { get; set; }
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
