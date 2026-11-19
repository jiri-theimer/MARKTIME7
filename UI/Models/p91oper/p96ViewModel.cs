namespace UI.Models.p91oper
{
    public class p96ViewModel:BaseViewModel
    {
        public int p91ID { get; set; }
        
        public BO.p91Invoice RecP91 { get; set; }

        public IEnumerable<BO.p96Imprint> lisP96 { get; set; }
        public bool CanDelete { get; set; }     //uživatel smí otisky z archivu odstraňovat
        public bool HasAttachmentReport { get; set; }   //typ faktury má tiskovou sestavu přílohy
        public bool IsIncludeAttachments { get; set; }
    }

    public class p96CreateViewModel : BaseViewModel
    {
        public string p91ids { get; set; }
        public int CountInput { get; set; }     //počet vybraných faktur
        public bool IsIncludeAttachments { get; set; }  //generovat i přílohy faktur (sestava přílohy z typu faktury)
        public int CountOK { get; set; }
        public List<BO.StringPair> Results { get; set; }    //Key=kód faktury, Value=výsledek
        public List<bool> ResultsOK { get; set; }
        public string ZipFileName { get; set; }     //ZIP s vygenerovanými PDF v temp složce
    }

    public class p96DownloadViewModel : BaseViewModel
    {
        public string p91ids { get; set; }
        public bool IsAllVersions { get; set; }     //false = jen poslední verze každé sestavy faktury
        public int CountInput { get; set; }         //počet vybraných faktur
        public int CountWithArchive { get; set; }   //z toho s nějakým PDF v archivu
        public int CountFilesLast { get; set; }
        public int CountFilesAll { get; set; }
        public long SizeLast { get; set; }
        public long SizeAll { get; set; }
        public List<string> CodesWithoutArchive { get; set; }
        public string ZipFileName { get; set; }     //hotový ZIP v temp složce
        public int ZipCount { get; set; }
        public int ZipMissing { get; set; }         //záznam v archivu je, soubor na disku chybí
    }
}
