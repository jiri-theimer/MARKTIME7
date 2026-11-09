namespace BL.AI.Core
{
    /// <summary>
    /// Singleton s globální (serverovou) konfigurací AI z appsettings. Načte se jednou při startu.
    /// Efektivní konfigurace pro licenci: <see cref="ForLicense"/>; pro aktuálního uživatele se ILlmClient registruje jako scoped.
    /// </summary>
    public class AiConfig
    {
        public AiOptions Global { get; }

        public AiConfig(BL.Singleton.RunningApp app)
        {
            Global = AiOptions.Load(app);
        }

        public AiOptions ForLicense(BO.x01License lic) => AiOptions.ForLicense(Global, lic);

        /// <summary>
        /// Klient pro licenci přihlášeného uživatele. BL.Factory se ze scope vyžádá až při prvním čtení Options (v akci / view),
        /// NE při vytvoření klienta: controllery dostávají ILlmClient v konstruktoru, tedy před BaseController.OnActionExecuting,
        /// a předčasně vytvořená Factory by neznala login (uživatel by se nenačetl → přesměrování na přihlášení).
        /// Výsledek = licence (x01AiUsageFlag) A oprávnění role GR_AI – platí pro všechny vstupy do AI (views, agenti, ClaudeClient.Send).
        /// </summary>
        public ILlmClient CreateClient(IServiceProvider scope) => new ClaudeClient(() =>
        {
            var f = (BL.Factory)scope.GetService(typeof(BL.Factory));
            return AiOptions.ForUser(ForLicense(f.Lic), f.CurrentUser);
        });

        /// <summary>Klient pro konkrétní konfiguraci – např. test připojení v administraci před uložením.</summary>
        public ILlmClient CreateClient(AiOptions options) => new ClaudeClient(options);
    }
}
