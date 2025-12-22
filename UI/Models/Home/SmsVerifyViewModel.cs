namespace UI.Models.Home
{
    public class SmsVerifyViewModel:BaseViewModel
    {
        public string SmsCode { get; set; }
        public BO.j02User recJ02 { get; set; }
    }
}
