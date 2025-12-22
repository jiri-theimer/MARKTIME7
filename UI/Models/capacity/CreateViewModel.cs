namespace UI.Models.capacity
{
    public class CreateViewModel:BaseViewModel
    {
        
        public ProjectComboViewModel ProjectCombo { get; set; }
        public bool IsUseFaNefa { get; set; }
        public bool FaZastropovan { get; set; }
        public bool NeFaZastropovan { get; set; }
        public string SourceGroupBy { get; set; }
        public string Items { get; set; }
        public string ItemsPrefix { get; set; }
        public int p41ID { get; set; }
        public int r02ID { get; set; }
        public List<InputPlanItem> InputCells { get; set; }
        public List<InputPlanItem> InputIntervals { get; set; }
       
        public BO.p41Project RecP41 { get; set; }
        public string opgScale { get; set; } = "interval";
        public IEnumerable<BO.r04CapacityResource> lisR04 { get; set; }
        public IEnumerable<BO.r01Capacity> lisR01 { get; set; }
        
    }

    public class InputPlanItem
    {        
        public string d { get; set; }
        public string dalias { get; set; }
        public int j02id { get; set; }
        public int p41id { get; set; }
        
        public string Person { get; set; }
        public string Project { get; set; }
        public DateTime d1 { get; set; }
        public DateTime d2 { get; set; }
        public string Memo { get; set; }
        public double HoursFa { get; set; }
        public double HoursNeFa { get; set; }
        public double HoursTotal { get; set; }
        public bool IsTempDeleted { get; set; }
        public string TempGuid { get; set; }
        public string Color { get; set; }
        
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
