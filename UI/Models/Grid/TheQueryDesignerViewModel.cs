namespace UI.Models
{
    public class TheQueryDesignerViewModel: BaseViewModel
    {
        public BO.j72TheGridTemplate Rec { get; set; }
        public List<BO.j73TheGridQuery> lisJ73 { get; set; }

        public List<BO.TheQueryField> lisQueryFields { get; set; }

        public List<BO.ThePeriod> lisPeriods { get; set; }

        public bool HasOwnerPermissions { get; set; }

        public string j04IDs { get; set; }
        public string j04Names { get; set; }
        public string CallerPathName { get; set; }
        public bool ForceSaveAsAtStart { get; set; }    //otevřít návrhář v režimu "Kopírovat"
    }
}
