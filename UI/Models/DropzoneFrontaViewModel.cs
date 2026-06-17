namespace UI.Models
{
    public class DropzoneFrontaViewModel:BaseViewModel
    {
        public IEnumerable<BO.o27Attachment> lisO27 { get; set; }
        public string prefix { get; set; }
        public int pid { get; set; }
    }
}
