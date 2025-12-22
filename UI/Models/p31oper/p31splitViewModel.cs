namespace UI.Models.p31oper
{
    public class p31splitViewModel:BaseViewModel
    {
        public int pid { get; set; }
        public string approve_guid { get; set; }
        public BO.p31Worksheet Rec { get; set; }
        public string Hours1 { get; set; }
        public string Text1 { get; set; }
        public string Text1Internal { get; set; }
        public string Hours2 { get; set; }
        public string Text2 { get; set; }
        public string Text2Internal { get; set; }
        public int ActiveTabIndex { get; set; } = 1;
    }
}
