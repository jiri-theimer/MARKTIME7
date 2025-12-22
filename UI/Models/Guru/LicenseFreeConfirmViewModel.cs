namespace UI.Models.Guru
{
    public class LicenseFreeConfirmViewModel: BaseViewModel
    {
        public string UserName { get; set; }
        public int x01ID { get; set; }
        public BO.j02User RecJ02 { get; set; }
        public BO.x01License RecX01 { get; set; }
    }
}
