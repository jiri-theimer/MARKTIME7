using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UI.Views.Shared.Components.myGrid;

namespace UI.Models
{
    public class GridMultiSelect:BaseViewModel
    {
        public string SelectedPids { get; set; }
        public string entity { get; set; }
        public string entityTitle { get; set; }
        public string prefix { get; set; }
        public int go2pid { get; set; }
        public int contextmenuflag { get; set; }

        public string master_entity { get; set; }
        public int master_pid { get; set; }
        
       
        public string go2pid_url_in_iframe { get; set; }

        public string dblclick { get; set; } = "tg_dblclick";

        public myGridInput gridinput { get; set; }
    }
}
