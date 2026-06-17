

namespace UI.Models
{
    public class TheGridDesignerViewModel:BaseViewModel
    {
        public bool IsFieldsDesignerOnly { get; set; }  //true: návrhář polí pro notifikační šablonu
        public BO.j72TheGridTemplate Rec { get; set; }

        public List<BO.TheGridColumn> SelectedColumns;
        public string ParentLayoutName { get; set; }

        public List<BO.EntityRelation> Relations;
        public List<BO.TheGridColumn> AllColumns;

        public bool ForceSaveAsAtStart { get; set; }    //otevřít návrhář v režimu "Kopírovat"

        public List<UI.Models.kendoTreeItem> treeNodes { get; set; }

        public List<BO.j73TheGridQuery> lisJ73 { get; set; }

        public List<BO.TheQueryField> lisQueryFields { get; set; }

        public List<BO.ThePeriod> lisPeriods { get; set; }

        public bool HasOwnerPermissions { get; set; }
        

        public string j04IDs { get; set; }
        public string j04Names { get; set; }

        public string j11IDs { get; set; }
        public string j11Names { get; set; }
        public string CallerPathName { get; set; }
        public string UniqueNamesCatalog { get; set; }
        
    }
}
