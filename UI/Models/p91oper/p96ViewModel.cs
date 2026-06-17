namespace UI.Models.p91oper
{
    public class p96ViewModel:BaseViewModel
    {
        public int p91ID { get; set; }
        
        public BO.p91Invoice RecP91 { get; set; }

        public IEnumerable<BO.p96Imprint> lisP96 { get; set; }
    }
}
