

namespace UI.Models.Mail
{   
    public class SendMailViewModel:BaseViewModel
    {
        public BO.x40MailQueue Rec { get; set; }
        public int b05ID { get; set; }
        public string Recipient { get; set; }
        public string SelectedJ40Name { get; set; }
        public int SelectedJ61ID { get; set; }
        public string SelectedJ61Name { get; set; }
        
        public string UploadGuid { get; set; }
        public string j61GridColumns { get; set; }
        public string j61MailBody { get; set; }
        public bool IsShowGridColumns { get; set; }
        public IEnumerable<BO.TheGridColumn> lisGridColumns { get; set; }
        public int ActiveTabIndex { get; set; } = 1;

        public Notepad.EditorViewModel Notepad { get; set; }
        public bool IsTest { get; set; }

        public List<string> Last10Receivers { get; set; }

        public int SelectedJ07ID { get; set; }
        public string SelectedJ07Name { get; set; }
        public int SelectedJ11ID { get; set; }
        public string SelectedJ11Name { get; set; }
    }
}
