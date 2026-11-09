using System.Text.Json;
using System.Text.Json.Serialization;

namespace BO.AI
{
    /// <summary>
    /// Zadání AI widgetu na dashboardu (x55Widget.x55AiTask). Vzniká jednou z výsledku „Zeptej se MARKTIME“ (1× tokeny),
    /// při každém zobrazení ho MARKTIME převede na SQL stejnou cestou jako Pivot-Explorer – bez volání AI.
    /// Oprávnění (MyRecordsDisponible, citlivé sloupce) se uplatní vždy podle toho, kdo widget právě vidí.
    /// </summary>
    public class AiWidgetTask
    {
        [JsonPropertyName("v")] public int Version { get; set; } = 1;
        /// <summary>agg = souhrn (agregace), list = výpis záznamů.</summary>
        [JsonPropertyName("kind")] public string Kind { get; set; } = "agg";
        /// <summary>table | chart | both (graf jen u souhrnu). Zatím jen table – graf je vypnutý (AiWidgetRenderer.ChartsEnabled).</summary>
        [JsonPropertyName("view")] public string View { get; set; } = "table";
        /// <summary>Pojmenované období (BO.ThePeriod.pid z ThePeriodProvider) – vyhodnotí se při každém zobrazení („tento měsíc“). 0 = pevné datumy ze zadání.</summary>
        [JsonPropertyName("periodPid")] public int PeriodPid { get; set; }
        [JsonPropertyName("agg")] public AiAggSpec Agg { get; set; }
        [JsonPropertyName("list")] public AiListSpec List { get; set; }
        /// <summary>Lidský popis zadání v době vytvoření (jen pro informaci).</summary>
        [JsonPropertyName("desc")] public string Description { get; set; }

        [JsonIgnore] public AiQuerySpec Spec => Kind == "list" ? List : Agg;

        public string ToJson() => JsonSerializer.Serialize(this);

        public static AiWidgetTask Parse(string json)
        {
            if (string.IsNullOrWhiteSpace(json)) return null;
            try
            {
                var t = JsonSerializer.Deserialize<AiWidgetTask>(json);
                return t?.Spec == null ? null : t;
            }
            catch { return null; }
        }
    }
}
