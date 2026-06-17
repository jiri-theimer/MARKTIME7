

namespace UI.Models.Mail
{
    public class x40RecMessage:BaseViewModel
    {
        public BO.x40MailQueue Rec { get; set; }

        public Rebex.Mail.MailMessage Mail { get; set; }

        public bool IsEmlFileExists { get; set; }   //true: eml soubor zprávy na serveru existuje
        
        public IEnumerable<BO.TheGridColumn> lisGridColumns { get; set; }

    }
}
