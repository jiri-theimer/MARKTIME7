using UI.Code.Integrace;

namespace UI.Models.Integrace
{
    //okno "Integrace" nad vybranými záznamy přehledu (/Integrace/Index)
    public class IntegraceViewModel : BaseViewModel
    {
        public string prefix { get; set; }
        public string pids { get; set; }
        public int SelectedX07ID { get; set; }

        //Ecomail
        public string SelectedListID { get; set; }
        public bool IncludeContactPersons { get; set; }

        public int CountSelected { get; set; }
        public IEnumerable<BO.x07Integration> lisX07 { get; set; }
        public BO.x07Integration RecX07 { get; set; }
        public List<BO.StringPair> lisEcomailLists { get; set; }
        public string SettingError { get; set; }        //proč integraci teď nelze spustit
        public List<string> Preview { get; set; } = new List<string>();     //co se odešle (před spuštěním)
        public IntegraceResult Result { get; set; }
    }
}
