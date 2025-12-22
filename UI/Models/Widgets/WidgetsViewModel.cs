

namespace UI.Models.Widgets
{
    public class WidgetsViewModel: BaseViewModel
    {
        public IEnumerable<BO.x54WidgetGroup> lisX54 { get; set; }        
        public int SelectedX54ID { get; set; }
        public BO.x54WidgetGroup RecX54 { get; set; }
        public DateTime d0 { get; set; }
        public int j02id_me { get; set; }
        public IEnumerable<BO.j02User> lisJ02 { get; set; }
        public BO.p28Contact Rec { get; set; }
        public string Skin { get; set; }
        public bool IsSubform { get; set; }     //true: jedná se o podformulář
        public string BoxColCss { get; set; } = "col-lg-6";

        public WidgetsEnvironment DockStructure { get; set; }

        public BO.x56WidgetBinding recX56 { get; set; }

        public IEnumerable<BO.x55Widget> lisAllWidgets { get; set; }
        public List<BO.x55Widget> lisUserWidgets { get; set; }
        public int ColumnsPerPage { get; set; }
        public int PageAutoRefreshPerSeconds { get; set; }

        public string DataTables_Localisation { get; set; }

        public bool IsDataTables { get; set; }
        public bool IsPdfButtons { get; set; }
        public bool IsExportButtons { get; set; }
        public bool IsPrintButton { get; set; }
        public bool IsCharts { get; set; }

        public bool IsAllScopeAllClients { get; set; }
        public int SelectedP28ID { get; set; }
        public string SelectedP28Name { get; set; }
        public IEnumerable<BO.p28Contact> lisP28 { get; set; }

    }
}
