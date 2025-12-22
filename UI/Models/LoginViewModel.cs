namespace UI.Models
{
    public class LoginViewModel : BaseViewModel
    {
        public string Login { get; set; }
        public string Password { get; set; }
        public int CookieExpiresInHours { get; set; } = 168;
        public int LangIndex { get; set; }
        public bool IsChangedLangIndex { get; set; }

        public string Message { get; set; }

        public string GoogleName { get; set; }
        public string GoogleEmail { get; set; }
        public string GoogleToken { get; set; }
        public string GoogleId { get; set; }

        public string Browser_UserAgent { get; set; }
        public int Browser_AvailWidth { get; set; }
        public int Browser_AvailHeight { get; set; }
        public int Browser_InnerWidth { get; set; }
        public int Browser_InnerHeight { get; set; }
        public string Browser_DeviceType { get; set; }
        public string Browser_Host { get; set; }
    }
}
