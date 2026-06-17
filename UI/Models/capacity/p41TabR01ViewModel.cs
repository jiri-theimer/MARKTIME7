namespace UI.Models.capacity
{
    public class p41TabR01ViewModel : BaseViewModel
    {
        public int p41id { get; set; }
        public IEnumerable<BO.r04CapacityResource> lisR04 { get; set; }

        public CapacityTimelineJ02ViewModel timeline { get; set; }
        public bool HasOwnerPermissions { get; set; }

        public BO.p41Project RecP41 { get; set; }




    }
}
