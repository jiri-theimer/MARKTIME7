namespace UI.Models
{
    /// <summary>Test systémového SMTP serveru (sekce SystemSmtp v appsecrets.json) – Administrace → Elektronická pošta.</summary>
    public class SystemSmtpViewModel : BaseViewModel
    {
        public string TestTo { get; set; }      //příjemce zkušební zprávy
        public string Result { get; set; }
        public bool IsOK { get; set; }
    }
}
