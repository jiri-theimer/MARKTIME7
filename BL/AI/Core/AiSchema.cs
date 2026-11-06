using System.Text.Json.Nodes;

namespace BL.AI.Core
{
    /// <summary>
    /// Pomocné funkce pro JSON schéma strukturovaného výstupu (Claude structured outputs):
    /// každý objekt má všechny vlastnosti required a additionalProperties = false.
    /// </summary>
    public static class AiSchema
    {
        public static JsonObject Obj(JsonObject props) => new()
        {
            ["type"] = "object",
            ["properties"] = props,
            ["required"] = Arr(props.Select(p => p.Key)),
            ["additionalProperties"] = false
        };

        public static JsonObject Str() => new() { ["type"] = "string" };
        public static JsonObject StrEnum(IEnumerable<string> values) => new() { ["type"] = "string", ["enum"] = Arr(values) };
        public static JsonObject IntEnum(IEnumerable<int> values) => new() { ["type"] = "integer", ["enum"] = ArrI(values) };
        public static JsonObject Array(JsonObject items) => new() { ["type"] = "array", ["items"] = items };

        /// <summary>Hodnota nebo null.</summary>
        public static JsonObject Nullable(JsonObject schema) => new()
        {
            ["anyOf"] = new JsonArray { schema, new JsonObject { ["type"] = "null" } }
        };

        public static JsonObject Date() => new() { ["type"] = "string", ["format"] = "date" };

        public static JsonArray Arr(IEnumerable<string> v) { var a = new JsonArray(); foreach (var x in v) a.Add(x); return a; }
        public static JsonArray ArrI(IEnumerable<int> v) { var a = new JsonArray(); foreach (var x in v) a.Add(x); return a; }
    }
}
