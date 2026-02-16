using UI.Views.Shared.Components.myPeriod;

namespace UI.Models.p31view
{
    public class p31TotalsViewModel : BaseViewModel
    {
        public myPeriodViewModel periodinput { get; set; }  //filtrování podle časového období

       
        public p31StateQueryViewModel p31statequery { get; set; }   //filtrování podle stavu aktivit v horním pruhu
        public p31TabQueryViewModel p31tabquery { get; set; }   //filtrování podle formátu aktivit v horním pruhu

        public TheGridQueryViewModel TheGridQueryButton { get; set; }

        public int SelectedJ79ID { get; set; }
        public BO.j79TotalsTemplate SelectedTemplate { get; set; }
        public IEnumerable<BO.j79TotalsTemplate> lisJ79 { get; set; }

        public string record_prefix { get; set; }
        public int record_pid { get; set; }

        public string selected_entity { get; set; } //prefix z menu vybrané záznamy
        public string selected_pids { get; set; } //pids z menu vybrané záznamy

        public string GridColumns { get; set; }
        public List<BO.TheGridColumn> lisGridColumns { get; set; }

        


        

    }
}
