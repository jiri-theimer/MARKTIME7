using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UI.Views.Shared.Components.myGrid;

namespace UI.Models.Admin
{
    public class AdminPage:BaseViewModel
    {
        public string area { get; set; }
        public string entity { get; set; }
        public string entityTitle { get; set; }
        public string entityTitleSingle { get; set; }
        public string prefix { get; set; }
        
        public int go2pid { get; set; }
        public int contextmenuflag { get; set; }

        public string master_entity { get; set; }
        public int master_pid { get; set; }
        
        public List<NavTab> NavTabs;

        public string go2pid_url_in_iframe { get; set; }

        public string dblclick { get; set; } = "tg_dblclick";

        public RecordBinQueryViewModel recordbinquery { get; set; } //filtrování podle archivu
        public myGridInput gridinput { get; set; }

        public List<TreeMenuNode> lisMenuNodes { get; set; }



        public TreeMenuNode AddChild(TreeMenuNode nParent, string strText, string strPrefix, string strUrl = null)
        {
            if (lisMenuNodes == null) lisMenuNodes = new List<TreeMenuNode>();
            var n = new TreeMenuNode() { ParentNode = nParent,Area=nParent.Area, Text = strText, Prefix = strPrefix, Url = strUrl };
            lisMenuNodes.Add(n);
            return n;
        }
        public TreeMenuNode AddMother(string strText, string strArea, string strIcon)
        {
            if (lisMenuNodes == null) lisMenuNodes = new List<TreeMenuNode>();
            var n = new TreeMenuNode() {Text = strText, Area = strArea, Icon = strIcon };
            lisMenuNodes.Add(n);
            return n;
        }
        public TreeMenuNode AddDivider(TreeMenuNode nParent,string strText=null)
        {
            if (lisMenuNodes == null) lisMenuNodes = new List<TreeMenuNode>();
            var n = new TreeMenuNode() { ParentNode = nParent, Text = strText,IsDivider=true};
            lisMenuNodes.Add(n);
            return n;
        }

    }

    public class TreeMenuNode
    {
        public TreeMenuNode ParentNode { get; set; }
        public string Area { get; set; }
        public string Text { get; set; }
        public string Prefix { get; set; }
        public string Url { get; set; }
        public string Icon { get; set; }
        public bool IsDivider { get; set; }
        public string active { get; set; }
    }

}
