using System.Text.Json;
using System.Text.Json.Nodes;

namespace BL.AI.Tools
{
    /// <summary>Čtení vstupu nástrojů (JsonNode od modelu) a zápis kompaktních výsledků.</summary>
    internal static class AiToolJson
    {
        public static readonly JsonSerializerOptions Out = new()
        {
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.Never
        };

        public static string Str(JsonNode n, string name)
        {
            var v = n?[name];
            if (v is JsonValue jv && jv.TryGetValue<string>(out var s)) return string.IsNullOrWhiteSpace(s) ? null : s.Trim();
            return null;
        }

        public static int? Int(JsonNode n, string name)
        {
            var v = n?[name];
            if (v is not JsonValue jv) return null;
            if (jv.TryGetValue<int>(out var i)) return i;
            if (jv.TryGetValue<double>(out var d)) return (int)d;
            if (jv.TryGetValue<string>(out var s) && int.TryParse(s, out i)) return i;
            return null;
        }

        public static bool? Bool(JsonNode n, string name)
        {
            var v = n?[name];
            if (v is JsonValue jv && jv.TryGetValue<bool>(out var b)) return b;
            return null;
        }

        public static List<string> StrList(JsonNode n, string name)
        {
            if (n?[name] is not JsonArray a) return new();
            return a.Select(x => x is JsonValue v && v.TryGetValue<string>(out var s) ? s?.Trim() : x?.ToJsonString())
                .Where(s => !string.IsNullOrEmpty(s)).ToList();
        }

        public static string Serialize(object o) => JsonSerializer.Serialize(o, Out);
    }
}
