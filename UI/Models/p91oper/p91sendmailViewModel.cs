namespace UI.Models.p91oper
{
    public class p91sendmailViewModel:BaseViewModel
    {
        public string p91ids { get; set; }
        public BO.x40MailQueue Rec { get; set; }
        public string SelectedJ40Name { get; set; }
        public string Recipient { get; set; }
        public IEnumerable<BO.p91Invoice> lisP91 { get; set; }
        public List<p91RowItem> RowItems { get; set; }

        public int SelectedJ61ID { get; set; }
        public string SelectedJ61Name { get; set; }

        public Notepad.EditorViewModel Notepad { get; set; }

        public bool IsIncludeReportAttachment { get; set; }
        public bool IsIncludeIsdoc { get; set; }
        public int SubjectFlag { get; set; }    //1:spojovat výchozí předmět + předmět zprávy, 2: pouze výchozí předmět, 3: pouze předmět zprávy
        public int AllTogether { get; set; }

        public IEnumerable<BO.x31Report> lisX31 { get; set; }
        public IEnumerable<BO.j61TextTemplate> lisJ61 { get; set; }
        
    }

    public class p91RowItem
    {
        public int p91ID { get; set; }
        public BO.p91Invoice RecP91 { get; set; }
        public string TO { get; set; }
        public string CC { get; set; }
        public string BCC { get; set; }
        public int x31ID_Invoice { get; set; }
        public int x31ID_Attachment { get; set; }
        public string Subject { get; set; }
        public int j61ID { get; set; }
        public string ErrorMessage { get; set; }
    }
}
