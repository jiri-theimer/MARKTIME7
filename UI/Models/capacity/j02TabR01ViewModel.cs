namespace UI.Models.capacity
{
    public class j02TabR01ViewModel:BaseViewModel
    {
        public int j02id { get; set; }
        public IEnumerable<BO.r04CapacityResource> lisR04 { get; set; }

        public CapacityTimelineP41ViewModel timeline { get; set; }
        public bool HasOwnerPermissions { get; set; }

        public BO.j02User RecJ02 { get; set; }
    }
}
