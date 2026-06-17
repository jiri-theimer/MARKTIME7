namespace UI.Models.Record
{
    public class p49Record:BaseRecordViewModel
    {
        public BO.p49FinancialPlan Rec { get; set; }
        public BO.p34ActivityGroup RecP34 { get; set; }
        public string ComboJ02 { get; set; }
        public string ComboP56 { get; set; }
        public string ComboP34 { get; set; }
        public string ComboP32 { get; set; }
        public string ComboJ27Code { get; set; }
        public string ComboSupplier { get; set; }

        public ProjectComboViewModel ProjectCombo { get; set; }
        public IEnumerable<BO.p56Task> lisP56 { get; set; }
    }
}
