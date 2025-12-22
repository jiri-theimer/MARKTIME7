namespace UI.Models.capacity
{
    public class EditViewModel:BaseViewModel
    {
        public int r02ID { get; set; }
        public bool IsUseFaNefa { get; set; }
        public string ItemsPrefix { get; set; }
        public string Oper { get; set; }
        public List<PlanItem> lisItems { get; set; }
        
        public IEnumerable<BO.r04CapacityResource> lisR04 { get; set; }
        public IEnumerable<BO.r01Capacity> lisR01 { get; set; }
    }

    public class PlanItem
    {
        public int r01id { get; set; }
        public int p41id { get; set; }
        public int j02id { get; set; }
        public string Person { get; set; }
        public string Project { get; set; }
        public string Client { get; set; }
        public DateTime d1_orig { get; set; }
        public DateTime d2_orig { get; set; }
        public DateTime d1 { get; set; }
        public DateTime d2 { get; set; }
        public string Memo { get; set; }
        public double HoursFa { get; set; }
        public double HoursNeFa { get; set; }
        public double HoursTotal { get; set; }
        public bool IsTempDeleted { get; set; }
        public string TempGuid { get; set; }
        public bool IsIncludeWeekend { get; set; }
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
