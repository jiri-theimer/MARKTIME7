namespace UI.Models.Guru
{
    public class ServiceLoginViewModel : UI.Models.BaseViewModel
    {
        public string Secret { get; set; }              //nově vygenerovaný klíč (Base32) - nikam se neukládá
        public string SecretFormatted { get; set; }     //klíč po čtveřicích k opsání
        public string OtpUri { get; set; }              //obsah QR kódu
        public bool IsConfigured { get; set; }          //v appsecrets.json už nějaký klíč je
    }
}
