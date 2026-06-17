
using BO;

namespace UI.Models.p31invoice
{
    public class GatewayViewModel: BaseViewModel
    {
        public string tempguid { get; set; }

       
        public IEnumerable<BO.p31Worksheet> lisP31 { get; set; }
        public IEnumerable<BO.b05Workflow_History> lisFakturacniPoznamky { get; set; } //fakturační poznámky
        public List<p91CreateItem> lisP91_Scale1 { get; set; }
        public List<p91CreateItem> lisP91_Scale2 { get; set; }
        public List<p91CreateItem> lisP91_Scale3 { get; set; }

        public DateTime? p91DateSupply { get; set; }
        public DateTime? p91Date { get; set; }
        public DateTime? p91Datep31_From { get; set; }
        public DateTime? p91Datep31_Until { get; set; }

        public bool IsDraft { get; set; }
        public int BillingScale { get; set; } = 1;   //1: vše dohromady, 2: za každého klienta, 3: za každý projekt

        public int SelectedInvoiceP91ID { get; set; }
        public string SelectedInvoiceText { get; set; }

        public bool IsRememberDates { get; set; }
        public bool IsRememberDohromady { get; set; }
        public bool IsRememberMaturity { get; set; }
        public bool IsRememberInvoiceText { get; set; }
        public int p91ID_LastRemember { get; set; }
        public bool IsRememberP92ID { get; set; }
        public bool IsRememberIsDraft { get; set; }

        public BO.p91Invoice RecLastRemember { get; set; }

        //public bool IsShowFinishPage { get; set; }
        public string ShowAfterFinish { get; set; } //recpage/grid/closeandrefresh

    }
}
