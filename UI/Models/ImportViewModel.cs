namespace UI.Models
{
    public class ImportViewModel : BaseViewModel
    {
        public string Prefix { get; set; }  //p31/p41/p28/j02
        public string Clipboard { get; set; }
        public string Delimiter { get; set; }
        public string Guid { get; set; }
        public bool IsFirstColHeaders { get; set; }
        public List<string> ClipLines { get; set; }
        public List<string> FirstLine { get; set; }
        public int ClipColsCount { get; set; }
        public IEnumerable<BO.p85Tempbox> lisP85 { get; set; }
        public List<string> Cols { get; set; }

        public List<TempRowP31> lisP31 { get; set; }

        public int DefaultJ02ID { get; set; }
        public string DefaultComboJ02 { get; set; }
        public int DefaultP32ID { get; set; }
        public string DefaultComboP32 { get; set; }
        public int DefaultP41ID { get; set; }
        public string DefaultComboP41 { get; set; }
        public string p31ExternalCode { get; set; }
        public string CountryCode { get; set; }
        public List<BO.Rejstrik.DefaultZaznam> lisRejstrik { get; set; }
        public int StepIndex { get; set; }
        public int SelectedP29ID { get; set; }
    }
    
    public class TempRowP31
    {
        public bool IsCanImport { get; set; }
        public string CssStyle { get; set; }
        public int Index { get; set; }
        public DateTime? p31Date { get; set; }
        public string CasOd { get; set; }
        public string CasDo { get; set; }
        public BO.p41Project RecP41 { get; set; }
        public BO.p28Contact RecP28 { get; set; }
        
        public BO.j02User RecJ02 { get; set; }
        
        public double p31Value_Orig { get; set; }
        public BO.p32Activity RecP32 { get; set; }
        
        public string p31Text { get; set; }
        public string p31TextInternal { get; set; }

    }
    public class ImportField
    {
        public string Field { get; set; }
        public string p85Field { get; set; }
        public string Header { get; set; }
    }
}
