

namespace UI.Models.p91oper
{
    public class p31move2invoiceViewModel:BaseViewModel
    {
        public int p31ID { get; set; }
        public BO.p31Worksheet Rec { get; set; }

        public BO.p91Invoice RecP91 { get; set; }

        public int SelectedP91ID { get; set; }
        public string SelectedP91Alias { get; set; }
    }
}
