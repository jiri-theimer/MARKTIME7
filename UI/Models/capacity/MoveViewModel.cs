namespace UI.Models.capacity
{
    public class MoveViewModel:BaseViewModel
    {
        public int p41ID { get; set; }
        public int r02ID { get; set; }
        public bool IsUseFaNefa { get; set; }
        public BO.p41Project RecP41 { get; set; }
        public DateTime? d1 { get; set; }
        public DateTime? d2 { get; set; }

        public DateTime? d1_orig { get; set; }
        public DateTime? d2_orig { get; set; }

        public List<PlanItem> lisItems { get; set; }

        public bool HasOwnerPermissions { get; set; }

        public IEnumerable<BO.r04CapacityResource> lisR04 { get; set; }

        public IEnumerable<BO.r01Capacity> lisR01 { get; set; }
    }
}
