using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UI.Models;
using UI.Views.Shared.Components.myGrid;
using UI.Views.Shared.Components.myPeriod;

namespace UI.Models
{
    public class TheGridViewModel:BaseViewModel
    {
        public string entity { get; set; }
        
        public myGridInput gridinput { get; set; }
        
        public string entityTitle { get; set; }
        public string prefix { get; set; }
      public string rez { get; set; }
        public int master_pid { get; set; }      
        
        public string myqueryinline { get; set; } //explicitní myquery ve tvaru název@typ@hodnota, lze předávat více parametrů najednou

        public List<NavTab> NavTabs { get; set; }
        public List<NavTab> OverGridTabs { get; set; }

        public string go2pid_url_in_iframe { get; set; }

        public myPeriodViewModel periodinput { get; set; } //fixní filtr v horním pruhu
        public p31StateQueryViewModel p31statequery { get; set; }   //filtrování podle stavu aktivit v horním pruhu
        public p31TabQueryViewModel p31tabquery { get; set; }   //filtrování podle formátu aktivit v horním pruhu
        public RecordBinQueryViewModel recordbinquery { get; set; } //filtrování podle archivu
        public ProjectTreeQueryViewModel p41treequery { get; set; } //filtrování podle stromové úrovně projektu
        public bool IsCanbeMasterView { get; set; }
        public string dblClickSetting { get; set; }
        public int o43mavazbu { get; set; } //0: bez ohledu na vazbu, 1: zprávy bez vazby, 2: zprávy s vazbou
        
        public TheGridQueryViewModel TheGridQueryButton { get; set; }
        public int j27id_query { get; set; }

        public bool show_podrizene { get; set; }
        public string caller { get; set; }
    }



}
