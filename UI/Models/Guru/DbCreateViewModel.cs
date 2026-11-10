namespace UI.Models.Guru
{
    /// <summary>Model stránky /CreateLicense/Index - průvodce založením nové zákaznické databáze.</summary>
    public class DbCreateViewModel : BaseViewModel
    {
        public bool IsAccessDenied { get; set; }
        public string DeniedMessage { get; set; }

        public bool IsGuru { get; set; }            //přihlášený guru vidí všechna nastavení, anonym pouze základ
        public bool IsSharedApp { get; set; }       //režim SharedApp: název databáze se odvozuje z domény (a7 + doména)
        public string ServerName { get; set; }
        public string DefaultDbName { get; set; }   //mimo SharedApp: databáze z AppConnection
        public string DefaultAppHost { get; set; }
        public string GeneratedPassword { get; set; }
        public string RecaptchaSiteKey { get; set; }

        //pravidla hesla z appsettings.json (PasswordChecker)
        public int PwdMinLength { get; set; }
        public int PwdMaxLength { get; set; }
        public bool PwdRequireDigit { get; set; }
        public bool PwdRequireLowercase { get; set; }
        public bool PwdRequireUppercase { get; set; }
        public bool PwdRequireNonAlphanumeric { get; set; }
    }
}
