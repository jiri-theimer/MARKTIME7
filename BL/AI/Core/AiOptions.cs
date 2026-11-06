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

        public static AiOptions Load(BL.Singleton.RunningApp app)
        {
            var o = new AiOptions();
            var sec = app.Configuration?.GetSection("AI");
            if (sec != null)
            {
                o.Enabled = sec.GetValue<bool>("Enabled", false);
                o.Model = sec["Model"] ?? o.Model;
                o.MaxTokens = sec.GetValue<int>("MaxTokens", o.MaxTokens);
                o.TimeoutSeconds = sec.GetValue<int>("TimeoutSeconds", o.TimeoutSeconds);
                o.Endpoint = sec["Endpoint"] ?? o.Endpoint;
                o.ApiKey = sec["ApiKey"];
            }

            // tajné údaje stejně jako SMTP v RunningApp
            var secrets = new ConfigurationBuilder().AddJsonFile("c:\\temp\\appsecrets.json", true).Build();
            var key = secrets.GetSection("AI")["ApiKey"];
            if (!string.IsNullOrEmpty(key)) o.ApiKey = key;
            if (string.IsNullOrEmpty(o.ApiKey)) o.ApiKey = System.Environment.GetEnvironmentVariable("ANTHROPIC_API_KEY");

            if (string.IsNullOrEmpty(o.ApiKey)) o.Enabled = false;
            return o;
        }
    }
}
