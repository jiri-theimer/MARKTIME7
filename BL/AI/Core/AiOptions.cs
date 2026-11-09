using Microsoft.Extensions.Configuration;

namespace BL.AI.Core
{
    /// <summary>
    /// Konfigurace AI vrstvy.
    /// Globální (serverová) konfigurace: sekce "AI" v appsettings.json, API klíč přednostně z c:\temp\appsecrets.json (sekce "AI").
    /// <code>
    /// "AI": {
    ///   "Enabled": true,
    ///   "Model": "claude-sonnet-5-5",
    ///   "MaxTokens": 4096,
    ///   "TimeoutSeconds": 90
    /// }
    /// </code>
    /// Která konfigurace platí pro licenci, rozhoduje x01License.x01AiUsageFlag (viz <see cref="ForLicense"/>):
    /// 0 = AI vypnuté, 1 = globální konfigurace z appsettings, 2 = vlastní klíč/model/limity z x01License.
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

        /// <summary>Odkud konfigurace pochází (pro administraci).</summary>
        public BO.x01AiUsageFlagEnum Source { get; set; } = BO.x01AiUsageFlagEnum.GlobalApiKey;

        public const string SecretsPath = "c:\\temp\\appsecrets.json";

        /// <summary>Globální konfigurace ze serveru (appsettings + appsecrets). Načítá se jednou při startu.</summary>
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
                o.ApiVersion = sec["ApiVersion"] ?? o.ApiVersion;
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

            const string restart = " Po úpravě konfigurace serveru je nutné restartovat aplikaci.";
            if (!hasSection)
                o.DisabledReason = $"V {appsettings} chybí sekce \"AI\"." + restart;
            else if (!o.Enabled)
                o.DisabledReason = $"V {appsettings} je \"AI\": {{ \"Enabled\": false }}." + restart;
            else if (string.IsNullOrEmpty(o.ApiKey))
                o.DisabledReason = (secretsError ?? $"Chybí API klíč: v {SecretsPath} očekávám {{ \"AI\": {{ \"ApiKey\": \"sk-ant-...\" }} }}.") + restart;

            if (o.DisabledReason != null) o.Enabled = false;
            return o;
        }

        /// <summary>
        /// Efektivní konfigurace pro licenci podle x01AiUsageFlag. Globální konfiguraci nemění (vrací kopii).
        /// U vlastního připojení (2) se z globální konfigurace přebírá jen Endpoint a ApiVersion; prázdný model / nulové limity
        /// = hodnota z appsettings. Vlastní připojení nezávisí na "AI": { "Enabled" } v appsettings.
        /// </summary>
        public static AiOptions ForLicense(AiOptions global, BO.x01License lic)
        {
            var flag = lic?.x01AiUsageFlag ?? BO.x01AiUsageFlagEnum.GlobalApiKey;
            var o = (AiOptions)global.MemberwiseClone();
            o.Source = flag;

            switch (flag)
            {
                case BO.x01AiUsageFlagEnum.None:
                    o.Enabled = false;
                    o.ApiKey = null;
                    o.DisabledReason = "AI je vypnuté v administraci (Globální parametry databáze → AI).";
                    return o;

                case BO.x01AiUsageFlagEnum.PrivateApiKey:
                    o.ApiKey = lic.x01AiApikey?.Trim();
                    if (!string.IsNullOrWhiteSpace(lic.x01AiModel)) o.Model = lic.x01AiModel.Trim();
                    if (lic.x01AiMaxTokens > 0) o.MaxTokens = lic.x01AiMaxTokens;
                    if (lic.x01AiTimeoutSeconds > 0) o.TimeoutSeconds = lic.x01AiTimeoutSeconds;
                    o.Enabled = !string.IsNullOrEmpty(o.ApiKey);
                    o.DisabledReason = o.Enabled ? null : "V administraci (Globální parametry databáze → AI) chybí API klíč pro vlastní připojení.";
                    return o;

                default:    // GlobalApiKey: appsettings beze změny
                    return o;
            }
        }
    }
}
