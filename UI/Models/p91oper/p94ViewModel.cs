
namespace UI.Models.p91oper
{
    public class p94ViewModel:BaseViewModel
    {
        public int p91ID { get; set; }
        public BO.p94Invoice_Payment Rec { get; set; }

        public BO.p91Invoice RecP91 { get; set; }

        public IEnumerable<BO.p94Invoice_Payment> lisP94 { get; set; }
    }
}
