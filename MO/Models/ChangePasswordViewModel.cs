namespace MO.Models
{
    public class ChangePasswordViewModel : BaseViewModel
    {
        public string CurrentPassword { get; set; }
        public string NewPassword { get; set; }
        public string VerifyPassword { get; set; }
    }
}
