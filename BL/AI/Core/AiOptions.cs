using Microsoft.Extensions.Configuration;

namespace BL.AI.Core
{
    /// <summary>
    /// Konfigurace AI vrstvy. Sekce "AI" v appsettings.json, API klíč přednostně z c:\temp\appsecrets.json (sekce "AI").
    /// <code>
    /// "AI": {
    ///   "Enabled": true,
    ///   "Model": "claude-sonnet-5-5",
    ///   "MaxTokens": 4096,
    ///   "TimeoutSeconds": 90
    /// }
    /// </code>
    /// </summary>
    public class AiOptions
    {
        public bool Enabled { get; set; }
        public string ApiKey { get; set; }
        public string Model { get; set; } = "claude-sonnet-5-5";
        public int MaxTokens { get; set; } = 4096;
        public int TimeoutSeconds { get; set; } = 90;
        public string Endpoint { get; set; } = "https://api.anthropic.com/v1/messages";
        public string ApiVersion { get; set; } = "2023-06-01";

        /// <summary>Proč je AI vypnuté (zobrazí se administrátorovi místo obecné hlášky).</summary>
        public string DisabledReason { get; set; }

        public const string SecretsPath = "c:\\temp\\appsecrets.json";

        public static AiOptions Load(BL.Singleton.RunningApp app)
        {
            var o = new AiOptions();
            string appsettings = System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(), "appsettings.json");   // stejný soubor jako RunningApp

            var sec = app.Configuration?.GetSection("AI");
            bool hasSection = sec != null && sec.Exists();
            if (hasSection)
            {
                o.Enabled = sec.GetValue<bool>("Enabled", false);
                o.Model = sec["Model"] ?? o.Model;
                o.MaxTokens = sec.GetValue<int>("MaxTokens", o.MaxTokens);
                o.TimeoutSeconds = sec.GetValue<int>("TimeoutSeconds", o.TimeoutSeconds);
                o.Endpoint = sec["Endpoint"] ?? o.Endpoint;
                o.ApiKey = sec["ApiKey"];
            }

            // tajné údaje stejně jako SMTP v RunningApp
            string secretsError = null;
            try
            {
                var secrets = new ConfigurationBuilder().AddJsonFile(SecretsPath, true).Build();
                var key = secrets.GetSection("AI")["ApiKey"];
                if (!string.IsNullOrEmpty(key)) o.ApiKey = key;
            }
            catch (Exception ex)
            {
                secretsError = $"Soubor {SecretsPath} nelze načíst (neplatný JSON?): {ex.Message}";
            }
            if (string.IsNullOrEmpty(o.ApiKey)) o.ApiKey = System.Environment.GetEnvironmentVariable("ANTHROPIC_API_KEY");

            if (!hasSection)
                o.DisabledReason = $"V {appsettings} chybí sekce \"AI\".";
            else if (!o.Enabled)
                o.DisabledReason = $"V {appsettings} je \"AI\": {{ \"Enabled\": false }}.";
            else if (string.IsNullOrEmpty(o.ApiKey))
                o.DisabledReason = secretsError ?? $"Chybí API klíč: v {SecretsPath} očekávám {{ \"AI\": {{ \"ApiKey\": \"sk-ant-...\" }} }}.";

            if (o.DisabledReason != null) o.Enabled = false;
            return o;
        }
    }
}
