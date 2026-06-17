namespace UI.Models.Record
{
    public class p91PageBoxViewModel: DynamicPageViewModel
    {
        public BO.a58RecPageBox RecA58 { get; set; }
        public BO.p91Invoice Rec { get; set; }

        public IEnumerable<BO.p91_CenovyRozpis> lisCenovyRozpis { get; set; }

        public IEnumerable<BO.p94Invoice_Payment> lisP94 { get; set; }
        public IEnumerable<BO.p99Invoice_Proforma> lisP99 { get; set; }
    }
}
