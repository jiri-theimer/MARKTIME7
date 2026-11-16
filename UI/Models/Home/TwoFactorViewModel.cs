namespace UI.Models.Home
{
    // Home/TwoFactor: zadání ověřovacího kódu po přihlášení
    public class TwoFactorViewModel : BaseViewModel
    {
        public string Code { get; set; }
        public string Oper { get; set; }            //"email" = poslat kód e-mailem
        public bool IsAppPaired { get; set; }       //uživatel má spárovanou ověřovací aplikaci
        public bool IsEmailPending { get; set; }    //čeká se na kód z e-mailu
        public string EmailMasked { get; set; }
        public DateTime? EmailSent { get; set; }
    }

    // Home/TwoFactorSetup: spárování ověřovací aplikace
    public class TwoFactorSetupViewModel : BaseViewModel
    {
        public string Code { get; set; }
        public string Oper { get; set; }            //"confirm" | "new" | "unpair"
        public bool IsPaired { get; set; }
        public bool IsRequired { get; set; }        //administrátor uživateli ověření nařídil
        public bool IsAfterLogin { get; set; }      //stránka se otevřela hned po přihlášení kódem z e-mailu
        public string Secret { get; set; }          //navržený klíč po čtveřicích (zobrazení); do formuláře se nevrací
        public string OtpUri { get; set; }          //obsah QR kódu
        public string Login { get; set; }
    }
}
