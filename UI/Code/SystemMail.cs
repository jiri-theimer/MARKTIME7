namespace UI.Code
{
    /// <summary>
    /// Systémové e-maily, které neodcházejí přes poštovní účet zákazníka, ale přes SMTP server aplikace (appsettings.json, sekce Smtp):
    /// ověřovací kód pro přihlášení (2FA) a obnova hesla.
    /// Odesílatel je jedna pevná adresa na doméně, kterou tento SMTP server obsluhuje (SPF/DKIM) – ne kontaktní e-mail z licence,
    /// se kterým zprávy končily ve spamu.
    /// Jiná adresa (instalace s vlastním SMTP serverem): appsettings.json "App": { "SystemSender": "noreply@firma.cz" }
    /// </summary>
    public static class SystemMail
    {
        public const string DefaultSender = "noreply@marktime.cz";

        public static string Sender(BL.Factory f)
        {
            string s = f.App.Configuration.GetSection("App")["SystemSender"];
            return string.IsNullOrEmpty(s) ? DefaultSender : s;
        }
    }
}
