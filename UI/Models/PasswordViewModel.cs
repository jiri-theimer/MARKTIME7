namespace UI.Models
{
    public class PasswordViewModel: BaseViewModel
    {
        public string Login { get; set; }

        public int LangIndex { get; set; }
        public bool IsChangedLangIndex { get; set; }
        public string ErrorMessage { get; set; }
        public string SuccessMessage { get; set; }
    }
}
