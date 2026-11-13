

namespace UI.Models.Record
{
    public class o51Record:BaseRecordViewModel
    {
        public BO.o51Tag Rec { get; set; }
        
       
        public string Combo_o53Name { get; set; }

        //založení více štítků najednou (jen nový záznam): názvy oddělené novým řádkem, středníkem nebo čárkou
        public bool IsBatch { get; set; }
        public string BatchNames { get; set; }
    }
}
