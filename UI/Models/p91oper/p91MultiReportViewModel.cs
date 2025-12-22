namespace UI.Models.p91oper
{
    public class p91MultiReportViewModel:BaseViewModel
    {
        public int p93ID { get; set; }
        public int DraftQuery { get; set; }
        public int x31ID { get; set; }
        public string ComboX31ID { get; set; }
        public string ComboP93ID { get; set; }

        public DateTime? d1 { get; set; }
        public DateTime? d2 { get; set; }

        public string OutputFileName { get; set; }
        public string p91Supplier { get; set; }
        public string p91Supplier_RegID { get; set; }
        public string p91Supplier_VatID { get; set; }
        public string p91Supplier_Street { get; set; }
        public string p91Supplier_City { get; set; }
        public string p91Supplier_ZIP { get; set; }
        public string p91Supplier_Country { get; set; }
        public string p91Supplier_Registration { get; set; }
        public string p91Supplier_ICDPH_SK { get; set; }
        public string LogoFile { get; set; }
        public string UploadGuidLogo { get; set; }
        public bool IsChangeLogo { get; set; }

        public string p93Contact { get; set; }
        public string p93Email { get; set; }
        public string p93Referent { get; set; }

        public string p93Signature { get; set; }
    }
}
