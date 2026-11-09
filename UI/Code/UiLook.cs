namespace UI.Code
{
    /// <summary>
    /// Vzhled aplikace: "modern" (nová vrstva CSS wwwroot/css/modern.css) nebo "classic" (původní).
    /// Volba uživatele = uživatelský parametr "ui-look" (MyProfile), jinak výchozí vzhled instalace
    /// z appsettings.json "App": { "DefaultLook": "classic" } – bez nastavení "modern" (výchozí od verze 8).
    /// Klasickému vzhledu se modern.css vůbec nenačte, pravidla jsou navíc omezená na html.mt-modern.
    /// </summary>
    public static class UiLook
    {
        public const string ParamKey = "ui-look";

        public static bool IsModern(BL.Factory f)
        {
            if (f?.CurrentUser == null) return false;
            string v = f.CBL.LoadUserParam(ParamKey, null);
            if (string.IsNullOrEmpty(v)) v = f.App?.Configuration?.GetSection("App")["DefaultLook"];
            return v != "classic";
        }

        /// <summary>Třída pro element html.</summary>
        public static string CssClass(BL.Factory f) => IsModern(f) ? "mt-modern" : null;
    }
}
