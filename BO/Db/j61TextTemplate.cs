using System;


namespace BO
{
    public enum j61UserFlagEnum
    {
        UserSettings=0,
        Yes=1,
        No=2
    }
    public class j61TextTemplate:BaseBO
    {
        public int x01ID { get; set; }
        
        public int j02ID_Owner { get; set; }
        public bool j61IsPublic { get; set; }
        public string j61Name { get; set; }
        public string j61Entity { get; set; }
        public string j61MailBody { get; set; }
        public int x04ID { get; set; }
        public string j61MailSubject { get; set; }
        public int j61Ordinary { get; set; }

        public string j61MailTO { get; set; }
        public string j61MailCC { get; set; }
        public string j61MailBCC { get; set; }
        public string j61MessageFields { get; set; }
        public string j61HtmlTemplateFile { get; set; }

        public string j61GridColumns { get; set; }
        public j61UserFlagEnum j61RecordLinkFlag { get; set; }
        public j61UserFlagEnum j61GridColumnsFlag { get; set; }
        public j61UserFlagEnum j61UserSignatureFlag { get; set; }

        public string j61SqlSource { get; set; }

        public string Owner { get; }
     
    }
}
