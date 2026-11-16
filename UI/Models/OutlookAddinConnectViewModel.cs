namespace UI.Models
{
    /// <summary>Stránky OutlookAddinConnectController (připojení doplňku pro Outlook, návod).</summary>
    public class OutlookAddinConnectViewModel : BaseViewModel
    {
        public string Token { get; set; }
        public string UserName { get; set; }
        public int ValidDays { get; set; }
        public string ManifestUrl { get; set; }
    }
}
