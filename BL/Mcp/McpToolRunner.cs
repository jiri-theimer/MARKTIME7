using System.Diagnostics;
using System.Text;
using System.Text.Json.Nodes;
using BL.AI.Core;
using BL.AI.Tools;

namespace BL.Mcp
{
    /// <summary>Nástroj tak, jak ho vidí MCP klient.</summary>
    public class McpToolInfo
    {
        public string Name { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public JsonObject InputSchema { get; set; }
        /// <summary>true = nástroj data jen čte.</summary>
        public bool ReadOnly { get; set; } = true;
    }

    /// <summary>
    /// Nástroje MARKTIME pro MCP klienty (Claude, ChatGPT…). Stejné nástroje jako agent „Zeptej se MARKTIME“ (katalog, agregace, hledej),
    /// běží s oprávněními uživatele, který připojení povolil. Model tu není náš – popisy nástrojů proto nesou i to,
    /// co agent dostává v system promptu (seznam entit, jméno uživatele).
    /// Třída nezávisí na knihovně MCP: převod na její typy je v UI (UI/Code/Mcp/McpSdk.cs).
    /// </summary>
    public class McpToolRunner
    {
        public const string FeatureCode = "mcp";
        public const string ServerName = "MARKTIME";

        private readonly BL.Factory _f;
        private readonly AiAggregate _agg;
        private List<IAiTool> _tools;

        public McpToolRunner(BL.Factory f, BL.TheColumnsProvider cp)
        {
            _f = f;
            _agg = new AiAggregate(f, cp);
        }

        /// <summary>Pokyny serveru pro model (stejné pro všechny uživatele).</summary>
        public static string Instructions => AiPromptStore.Get("mcp.instructions.md");

        /// <summary>Přístup k MCP má uživatel s oprávněním aplikační role „Přístup k AI agentům“ (GR_AI).</summary>
        public bool IsAllowed => _f.CurrentUser != null && _f.CurrentUser.TestPermission(BO.PermValEnum.GR_AI);

        public const string DeniedMessage = "Vaše aplikační role v MARKTIME nemá oprávnění „Přístup k AI agentům“. O přístup požádejte administrátora MARKTIME.";

        private List<IAiTool> Tools => _tools ??= new List<IAiTool> { new KatalogTool(_f, _agg), new AgregaceTool(_f, _agg), new HledejTool(_f, _agg) };

        public List<McpToolInfo> List()
        {
            var lis = new List<McpToolInfo>();
            if (!IsAllowed) return lis;     // bez oprávnění klient žádné nástroje nevidí; volání vrátí DeniedMessage

            foreach (var t in Tools)
            {
                lis.Add(new McpToolInfo
                {
                    Name = t.Name,
                    Title = Title(t.Name),
                    Description = Describe(t.Name),
                    InputSchema = (JsonObject)t.InputSchema.DeepClone()
                });
            }
            return lis;
        }

        /// <summary>Provede nástroj. Nevyhazuje výjimky – chybu vrací jako IsError s textem pro model.</summary>
        /// <param name="clientName">název připojeného klienta – do logu j98OpenAILog</param>
        public AiToolOutput Call(string name, JsonNode input, string clientName = null)
        {
            if (!IsAllowed) return AiToolOutput.Error(DeniedMessage);
            var tool = Tools.FirstOrDefault(t => t.Name == name);
            if (tool == null) return AiToolOutput.Error($"Neznámý nástroj: {name}. Dostupné: {string.Join(", ", Tools.Select(t => t.Name))}.");

            var sw = Stopwatch.StartNew();
            AiToolOutput o;
            try { o = tool.Execute(input ?? new JsonObject(), new AiToolContext { F = _f }) ?? AiToolOutput.Error("Nástroj nevrátil výsledek."); }
            catch (Exception ex) { o = AiToolOutput.Error("Chyba při provádění nástroje: " + ex.Message); }
            sw.Stop();

            Log(name, input, o, sw.ElapsedMilliseconds, clientName);
            return o;
        }

        private static string Title(string name) => name switch
        {
            "katalog" => "Katalog sloupců entity",
            "agregace" => "Součty podle skupin",
            "hledej" => "Výpis záznamů",
            _ => name
        };

        private string Describe(string name)
        {
            var u = _f.CurrentUser;
            switch (name)
            {
                case "katalog":
                    var sb = new StringBuilder();
                    sb.AppendLine("MARKTIME (evidence práce, projektů, klientů a fakturace): vrátí katalog sloupců entity – id sloupce, název, skupinu, roli (M = měřítko, D = dimenze, F = jen filtr a výpis) a typ filtru, a pevné filtry entity (pole období, stavy, formáty, archiv).");
                    sb.AppendLine("Zavolej jej pro každou entitu dřív, než ji použiješ v nástroji agregace nebo hledej – id sloupců platí jen pro danou entitu. Katalog respektuje oprávnění uživatele.");
                    sb.AppendLine($"Data patří uživateli: {u.j02FirstName} {u.j02LastName} („já“, „moje“ = tento uživatel).");
                    sb.Append(AiToolEntities.ToPromptText(_f));
                    return sb.ToString().TrimEnd();
                case "agregace":
                    return "MARKTIME: spočítá součty měřítek (M) seskupené podle dimenzí (D) entity s filtry – hodiny, částky, počty podle klientů, projektů, osob, měsíců… " +
                           "Vrátí použité filtry, sloupce, řádky do limitu a celkové součty ze všech skupin. Id sloupců ber z nástroje katalog pro tutéž entitu. " +
                           "Bez dimenzí vrátí jeden řádek s celkovými součty. Pro výpis jednotlivých záznamů použij nástroj hledej.";
                case "hledej":
                    return "MARKTIME: vypíše jednotlivé záznamy entity (úkony, projekty, kontakty, faktury, úkoly…) s vybranými sloupci a filtry. " +
                           "Vrátí řádky do limitu, počet všech nalezených záznamů a součty sčítatelných sloupců (M) ze všech záznamů. Id sloupců ber z nástroje katalog pro tutéž entitu. " +
                           "Na součty a přehledy podle skupin použij nástroj agregace.";
            }
            return Tools.First(t => t.Name == name).Description;
        }

        /// <summary>Jedno volání nástroje = jeden záznam j98OpenAILog (j98Feature = mcp). Tokeny modelu tu neznáme – model běží u klienta.</summary>
        private void Log(string name, JsonNode input, AiToolOutput o, long ms, string clientName)
        {
            string json = input?.ToJsonString(_logJson);
            var call = new JsonObject { ["n"] = name, ["in"] = input?.DeepClone(), ["ok"] = !o.IsError, ["ms"] = ms };
            if (o.Rows > 0) call["rows"] = o.Rows;
            AiLog.Insert(_f, new AiLogEntry
            {
                Feature = FeatureCode,
                ContextPage = clientName,
                QueryText = name + " " + json,
                Status = o.IsError ? 'E' : 'O',
                ErrorText = o.IsError ? o.Content : null,
                DurationMs = ms,
                ExtraJson = new JsonObject { ["tools"] = new JsonArray { call } }.ToJsonString(_logJson)
            });
        }

        private static readonly System.Text.Json.JsonSerializerOptions _logJson = new()
        {
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            TypeInfoResolver = new System.Text.Json.Serialization.Metadata.DefaultJsonTypeInfoResolver()     // bez něj JsonNode.ToJsonString(options) v .NET 8 vyhodí výjimku
        };
    }
}
