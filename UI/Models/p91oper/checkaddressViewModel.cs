namespace UI.Models.p91oper
{
    public class checkaddressViewModel:BaseViewModel
    {
        public string input_p91ids { get; set; }
        public string input_p28ids { get; set; }
        public IEnumerable<BO.p28Contact> lisP28 { get; set; }

    }
}
