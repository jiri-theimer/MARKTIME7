using System.Text.Json.Nodes;

namespace BL.AI.Core
{
    /// <summary>
    /// Nástroj pro tool use: tenká obálka nad BL, běží s oprávněními přihlášeného uživatele (F.CurrentUser).
    /// Model nikdy nedostane volné SQL – jen vstup podle InputSchema, který nástroj sám validuje.
    /// </summary>
    public interface IAiTool
    {
        /// <summary>Název nástroje pro model (a^[a-z_]+$), např. "agregace".</summary>
        string Name { get; }
        string Description { get; }
        JsonObject InputSchema { get; }

        /// <summary>Provede nástroj. Chybu vstupu vrací jako IsError = true s popisem – model ji opraví v dalším kole.</summary>
        AiToolOutput Execute(JsonNode input, AiToolContext ctx);
    }

    public class AiToolOutput
    {
        /// <summary>Text výsledku pro model (kompaktní JSON nebo řádkový text).</summary>
        public string Content { get; set; }
        public bool IsError { get; set; }
        /// <summary>ID výsledku (r1, r2…), pokud nástroj vrací data, na která se může odkázat odpověď.</summary>
        public string ResultId { get; set; }
        /// <summary>Data výsledku pro UI (vykreslí MARKTIME, model je nepřepisuje).</summary>
        public object Data { get; set; }
        /// <summary>Počet řádků výsledku – do logu.</summary>
        public int Rows { get; set; }

        public static AiToolOutput Error(string message) => new() { Content = message, IsError = true };
        public static AiToolOutput Error(IEnumerable<string> messages) => new() { Content = "Chyby vstupu:\n- " + string.Join("\n- ", messages), IsError = true };
    }

    /// <summary>Kontext jednoho dotazu uživatele – sdílený mezi voláními nástrojů.</summary>
    public class AiToolContext
    {
        public BL.Factory F { get; init; }
        /// <summary>Výsledky nástrojů v tomto dotazu (ResultId → Data).</summary>
        public Dictionary<string, object> Results { get; } = new(StringComparer.OrdinalIgnoreCase);
        /// <summary>Výsledky z dřívějších dotazů konverzace (ResultId → název nástroje a vstup) – lze je znovu spustit.</summary>
        public Dictionary<string, (string Tool, JsonNode Input)> ThreadResults { get; } = new(StringComparer.OrdinalIgnoreCase);
        /// <summary>Mezipaměť nástrojů v rámci dotazu (např. katalogy entit).</summary>
        public Dictionary<string, object> Cache { get; } = new();

        private int _lastResult;
        public void SeedResultIndex(int last) => _lastResult = Math.Max(_lastResult, last);
        public string NewResultId() => "r" + (++_lastResult);

        public T Cached<T>(string key, Func<T> factory) where T : class
        {
            if (Cache.TryGetValue(key, out var o) && o is T t) return t;
            t = factory();
            Cache[key] = t;
            return t;
        }
    }
}
