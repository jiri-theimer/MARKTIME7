namespace UI.Models
{
    public class p87ViewModel : BaseViewModel
    {
        public int StatusFlag { get; set; }     //-1 = všechny stavy
        public List<BO.p87BankTransaction> lisP87 { get; set; }
        public int CountUnpaired { get; set; }

        public string oper { get; set; }
        public int p87id { get; set; }
        public string code { get; set; }
    }
}
