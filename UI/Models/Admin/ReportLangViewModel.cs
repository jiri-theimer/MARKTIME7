namespace UI.Models.Admin
{
    //Administrace: vlastní výrazy zákazníka pro překlad tiskových sestav (/ReportLang/Index)
    public class ReportLangViewModel : BaseViewModel
    {
        public string RowsJson { get; set; }    //řádky slovníku zákazníka jako JSON [{cs,en,de,sk}] (stejné jako v souboru) - tam i zpět
        public string SharedRowsJson { get; set; }  //společný slovník ve stejném tvaru - jen k nahlédnutí (druhá záložka)
        public int SharedCount { get; set; }    //počet výrazů ve společném slovníku
    }
}
