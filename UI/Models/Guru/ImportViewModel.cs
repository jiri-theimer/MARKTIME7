namespace UI.Models.Guru
{
    public class ImportViewModel:BaseViewModel
    {
        public BO.x01License Rec_Source { get; set; }
        public BO.x01License Rec_Dest { get; set; }
        public int x01ID_Dest { get; set; }
        public int x01ID_Source { get; set; }
        public string Message { get; set; }
    }
}
