using UI.Models.Record;

namespace UI.Models.capacity
{
    public class ProjectResourcesViewModel:BaseViewModel
    {
        public bool IsUseFaNefa { get; set; }
        public int p41ID { get; set; }
        public List<r04Repeater> lisR04 { get; set; }
        public IEnumerable<BO.r01Capacity> lisR01 { get; set; }
        public IEnumerable<BO.x67EntityRole> lisProjectRoles { get; set; }
        public IEnumerable<BO.j11Team> lisJ11 { get; set; }
        public int SelectedX67ID { get; set; }        
        public string SelectedX67Name { get; set; }
        public int SelectedJ11ID { get; set; }
        public int SelectedJ11Name { get; set; }
        public BO.p41Project RecP41 { get; set; }
        
        public int CapacityStream { get; set; }
        public bool FaZastropovan { get; set; }
        public bool NeFaZastropovan { get; set; }
    }

    public class r04Repeater : BO.r04CapacityResource
    {
        public bool IsTempDeleted { get; set; }
        public string TempGuid { get; set; }
        public string FullName { get; set; }
        public string CssTempDisplay
        {
            get
            {
                if (this.IsTempDeleted)
                {
                    return "display:none;";
                }
                else
                {
                    return "display:table-row;";
                }
            }
        }
    }
}
