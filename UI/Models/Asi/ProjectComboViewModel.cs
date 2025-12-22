namespace UI.Models
{
    public class ProjectComboViewModel
    {
        public int SelectedP41ID { get; set; }
        public string SelectedProject { get; set; }

        public int SelectedLevelIndex { get; set; } = 0;
        public List<BO.ListItemValue> lisLevelIndex { get; set; }
        public string ProjectEntity { get; set; } = "le5";
        public string elementidprefix { get; set; } = "ProjectCombo.";

        public string CssClassDiv { get; set; } = "col-sm-11 col-md-10";

        public string PostbackFlag { get; set; }

        public string MyQueryInline { get; set; }

        public bool IsHideLabel { get; set; }

        
    }
}
