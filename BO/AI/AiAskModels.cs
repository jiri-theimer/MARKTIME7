using System.Text.Json.Serialization;

namespace BO.AI
{
    /// <summary>Vstup z chatu pro agenta s nástroji: AiAskInput (poslední dotaz = poslední zpráva user) + konverzace z minulých kol.</summary>
    public class AiToolAskInput : AiAskInput
    {
        /// <summary>Konverzace ve formátu Messages API (JSON řetězec, jak jej vrátil server).</summary>
        [JsonPropertyName("thread")] public string Thread { get; set; }
    }

    /// <summary>
    /// Společné zadání dotazu nástrojů (agregace, hledej) s klíči sloupců relace__Entita__Field:
    /// entita, sloupcový filtr, období, stav, formát, archiv, řazení, limit.
    /// Ukládá se do p85TempBox – podle něj se znovu načtou data pro Pivot-Explorer a zdrojové záznamy.
    /// </summary>
    public class AiQuerySpec
    {
        /// <summary>Prefix entity (p31, p41, p91…).</summary>
        [JsonPropertyName("entity")] public string Entity { get; set; }
        [JsonPropertyName("filter")] public List<AiGridFilter> Filter { get; set; } = new();
        [JsonPropertyName("period")] public AiAggPeriod Period { get; set; }
        /// <summary>Stav úkonů 0–21 (null = filtr se nepoužije).</summary>
        [JsonPropertyName("state")] public int? State { get; set; }
        /// <summary>Formát úkonů: "" | time | kusovnik | expense | fee.</summary>
        [JsonPropertyName("tab")] public string Tab { get; set; }
        /// <summary>0 = otevřené i archiv, 1 = otevřené, 2 = archiv.</summary>
        [JsonPropertyName("archive")] public int Archive { get; set; } = 1;
        /// <summary>Klíč sloupce výsledku nebo "pocet" (jen agregace); null = výchozí řazení.</summary>
        [JsonPropertyName("sortBy")] public string SortBy { get; set; }
        [JsonPropertyName("sortDesc")] public bool SortDesc { get; set; } = true;
        [JsonPropertyName("limit")] public int Limit { get; set; } = 100;
        [JsonPropertyName("title")] public string Title { get; set; }
    }

    /// <summary>Zadání agregace (nástroj „agregace“).</summary>
    public class AiAggSpec : AiQuerySpec
    {
        [JsonPropertyName("dims")] public List<string> Dims { get; set; } = new();
        /// <summary>Klíče sčítaných sloupců; "pocet" = počet záznamů.</summary>
        [JsonPropertyName("measures")] public List<string> Measures { get; set; } = new();
    }

    /// <summary>Zadání výpisu záznamů (nástroj „hledej“).</summary>
    public class AiListSpec : AiQuerySpec
    {
        /// <summary>Klíče zobrazených sloupců v pořadí.</summary>
        [JsonPropertyName("columns")] public List<string> Columns { get; set; } = new();
    }

    public class AiAggPeriod
    {
        /// <summary>Datumové pole období (p31Date, p91DateSupply…).</summary>
        [JsonPropertyName("field")] public string Field { get; set; }
        /// <summary>yyyy-MM-dd</summary>
        [JsonPropertyName("d1")] public string D1 { get; set; }
        [JsonPropertyName("d2")] public string D2 { get; set; }
    }

    /// <summary>Tabulka výsledku pro chat – data z BL, model je nepřepisuje.</summary>
    public class AiAskTable
    {
        [JsonPropertyName("id")] public string Id { get; set; }
        /// <summary>agg = agregace (skupiny), list = výpis záznamů.</summary>
        [JsonPropertyName("kind")] public string Kind { get; set; } = "agg";
        [JsonPropertyName("title")] public string Title { get; set; }
        /// <summary>Použité období a filtry lidsky.</summary>
        [JsonPropertyName("description")] public string Description { get; set; }
        [JsonPropertyName("entity")] public string Entity { get; set; }
        [JsonPropertyName("columns")] public List<AiAskColumn> Columns { get; set; } = new();
        /// <summary>Řádky: dimenze jako text (datum yyyy-MM-dd, ano/ne 1/0), měřítka jako čísla.</summary>
        [JsonPropertyName("rows")] public List<object[]> Rows { get; set; } = new();
        /// <summary>Součet měřítek přes všechny skupiny (i ty, které se nevešly do limitu); u dimenzí null.</summary>
        [JsonPropertyName("total")] public object[] Total { get; set; }
        /// <summary>Počet všech skupin (agregace) nebo záznamů (výpis) výsledku.</summary>
        [JsonPropertyName("groups")] public int Groups { get; set; }
        /// <summary>Výpis: ID záznamu pro každý řádek (otevření karty); agregace: null.</summary>
        [JsonPropertyName("ids")] public List<int> Ids { get; set; }
        [JsonPropertyName("truncated")] public bool Truncated { get; set; }
        /// <summary>p85GUID uloženého zadání – Pivot-Explorer a zdrojové záznamy.</summary>
        [JsonPropertyName("guid")] public string Guid { get; set; }
        /// <summary>Lze zobrazit zdrojové úkony (jen entita p31).</summary>
        [JsonPropertyName("canDrill")] public bool CanDrill { get; set; }
    }

    public class AiAskColumn
    {
        /// <summary>Lidský název – zároveň název pole v Pivot-Exploreru (jedinečný v tabulce).</summary>
        [JsonPropertyName("name")] public string Name { get; set; }
        /// <summary>string | number | date | bool</summary>
        [JsonPropertyName("type")] public string Type { get; set; }
        [JsonPropertyName("measure")] public bool Measure { get; set; }
        [JsonPropertyName("decimals")] public int Decimals { get; set; }
    }
}
