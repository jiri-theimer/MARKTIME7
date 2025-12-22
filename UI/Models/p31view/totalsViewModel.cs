using UI.Views.Shared.Components.myGrid;
using UI.Views.Shared.Components.myPeriod;

namespace UI.Models.p31view
{
    public class totalsViewModel:BaseViewModel
    {
        public myPeriodViewModel periodinput { get; set; }  //filtrování podle časového období
        
        public string TableClientID { get; set; }
        public IEnumerable<BO.j79TotalsTemplate> lisJ79 { get; set; }
        public int SelectedJ79ID { get; set; }
        public bool IsAllowEditTemplate { get; set; }
        public bool IsShared { get; set; }
        public BO.j79TotalsTemplate SelectedTemplate { get; set; }
        public p31StateQueryViewModel p31statequery { get; set; }   //filtrování podle stavu aktivit v horním pruhu
        public p31TabQueryViewModel p31tabquery { get; set; }   //filtrování podle formátu aktivit v horním pruhu

        public TheGridQueryViewModel TheGridQueryButton { get; set; }
        
        public string HtmlVystup { get; set; }

        public string record_prefix { get; set; }
        public int record_pid { get; set; }
        
        public string selected_entity { get; set; } //prefix z menu vybrané záznamy
        public string selected_pids { get; set; } //pids z menu vybrané záznamy

        public string GridColumns { get; set; }
        public List<BO.TheGridColumn> lisGridColumns { get; set; }

        public string GroupField1 { get; set; }
        public string GroupField2 { get; set; }
        public string GroupField3 { get; set; }
        public string PivotField { get; set; }
        public string PivotValue { get; set; }

        public string j02IDs { get; set; }
        public string SelectedPersons { get; set; }
        public string j07IDs { get; set; }
        public string SelectedPositions { get; set; }
        public string j11IDs { get; set; }
        public string SelectedTeams { get; set; }

        public string FindAddQueryValue(string strField)
        {

            if (this.SelectedTemplate == null || string.IsNullOrEmpty(strField))
            {
                return null;
            }
            var lis = BO.Code.Bas.ConvertString2List(this.SelectedTemplate.j79AddQuery, "|");
            foreach(string s in lis)
            {
                var arr = s.Split("###");
                if (arr[0] == strField)
                {
                    return arr[1];
                }
            }

            return null;
        }
        
    }
}
