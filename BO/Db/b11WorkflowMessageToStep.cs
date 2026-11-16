namespace BO
{
    public enum NotifyFormatMessageFlag
    {
        EmailOnly = 0               //poslat email zprávu (hodnoty 1 a 2 patřily SMS, verze 8 SMS neposílá)
    }
    public class b11WorkflowMessageToStep
    {        
        public int b06ID { get; set; }
        public int j61ID { get; set; }
        public int x67ID { get; set; }
        public int j04ID { get; set; }
        public int j11ID { get; set; }
       
        public bool b11IsRecordOwner { get; set; }
        public bool b11IsRecordCreator { get; set; }
        public bool b11IsRecordCreatorByEmail { get; set; }

        public string b11Subject { get; set; }

        public string j61Name { get; }
        public string j04Name { get; }
        public string j11Name { get; }
        public string x67Name { get; }


    }
}

