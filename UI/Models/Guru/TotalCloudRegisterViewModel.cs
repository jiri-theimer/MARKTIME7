namespace UI.Models.Guru
{
    /// <summary>Model stránek samoobslužné registrace v režimu TotalCloud (/CreateLicenseFree/Index a /Complete).</summary>
    public class TotalCloudRegisterViewModel : BaseViewModel
    {
        public string Message { get; set; }
        public bool IsClosed { get; set; }          //registrace není v této instalaci dostupná
        public string CountryCode { get; set; } = "CZ";

        //Index
        public string RecaptchaSiteKey { get; set; }
        public bool AllowGoogle { get; set; }

        //Complete
        public bool IsLinkInvalid { get; set; }     //odkaz vypršel nebo není platný
        public bool IsAccountExists { get; set; }   //registrace už byla dokončena
        public string Token { get; set; }
        public string Email { get; set; }
        public bool IsGoogle { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string PasswordHint { get; set; }
        public List<UI.Code.DbCreate.TotalCloudProfile> Profiles { get; set; }
    }
}
