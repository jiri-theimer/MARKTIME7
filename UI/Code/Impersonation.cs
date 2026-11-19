using System.Security.Claims;

namespace UI.Code
{
    /// <summary>
    /// Přepnutí administrátora na identitu jiného uživatele (Admin/LogAsUser). V přihlašovací cookie zůstává login toho,
    /// kdo se přepnul – podle něj aplikace ukazuje pruh „Pracujete jako…“ a umí vrátit původní identitu.
    /// </summary>
    public static class Impersonation
    {
        public const string ClaimType = "mt_impersonator";      //login administrátora, který se přepnul
        public const string ParamFails = "logasuser-fails";     //počet po sobě jdoucích neúspěšných pokusů (uživatelský parametr)
        public const int MaxFails = 5;
        public const int BlockMinutes = 15;
        public const int SessionHours = 1;      //jak dlouho přepnutí platí
        public const int ReturnHours = 8;       //platnost přihlášení po návratu k vlastní identitě

        /// <summary>Login administrátora, pod kterým se právě pracuje za někoho jiného; null = běžné přihlášení.</summary>
        public static string Impersonator(ClaimsPrincipal user)
        {
            string s = user?.FindFirst(ClaimType)?.Value;
            return string.IsNullOrEmpty(s) ? null : s;
        }

        public static ClaimsPrincipal BuildPrincipal(string strLogin, string strEmail, string strImpersonator)
        {
            if (string.IsNullOrEmpty(strEmail)) strEmail = "info@marktime.cz";
            var claims = new List<Claim>()
            {
                new Claim(ClaimTypes.Name, strLogin),
                new Claim("access_token", "inspis_core_token"),     //stejné údaje jako běžné přihlášení (LoginController.SetClaim)
                new Claim(ClaimTypes.Email, strEmail)
            };
            if (!string.IsNullOrEmpty(strImpersonator)) claims.Add(new Claim(ClaimType, strImpersonator));
            return new ClaimsPrincipal(new[] { new ClaimsIdentity(claims, "User Identity") });
        }

        /// <summary>Porovnání tajného klíče bez úniku informace přes dobu porovnání.</summary>
        public static bool SecretEquals(string a, string b)
        {
            if (string.IsNullOrEmpty(a) || string.IsNullOrEmpty(b)) return false;
            return System.Security.Cryptography.CryptographicOperations.FixedTimeEquals(System.Text.Encoding.UTF8.GetBytes(a), System.Text.Encoding.UTF8.GetBytes(b));
        }
    }
}
