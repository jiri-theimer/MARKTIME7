namespace UI.Models
{
    public class p41TabP40ViewModel: BaseViewModel
    {
        public int p41id { get; set; }
        public IEnumerable<BO.p40WorkSheet_Recurrence> lisP40 { get; set; }
        public bool HasOwnerPermissions { get; set; }
    }
}
