using System.Collections.Concurrent;
using System.Reflection;

namespace BL.AI.Core
{
    /// <summary>
    /// Načítání promptů a datových souborů z embedded resources projektu BL (složky AI/Prompts a AI/*/Data).
    /// V BL.csproj: &lt;EmbeddedResource Include="AI\**\*.md;AI\**\*.json" /&gt;
    /// </summary>
    public static class AiPromptStore
    {
        private static readonly ConcurrentDictionary<string, string> _cache = new();

        /// <param name="fileName">např. "totals.system.md" nebo "p31-hints.json"</param>
        public static string Get(string fileName)
        {
            return _cache.GetOrAdd(fileName, fn =>
            {
                var asm = Assembly.GetExecutingAssembly();
                var name = asm.GetManifestResourceNames().FirstOrDefault(p => p.EndsWith("." + fn, StringComparison.OrdinalIgnoreCase));
                if (name == null) throw new InvalidOperationException($"AI resource '{fn}' nebyl nalezen (embedded resource v BL.csproj).");
                using var s = asm.GetManifestResourceStream(name);
                using var r = new StreamReader(s);
                return r.ReadToEnd();
            });
        }
    }
}
