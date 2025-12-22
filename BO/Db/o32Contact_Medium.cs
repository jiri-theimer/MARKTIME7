using System;


namespace BO
{
    public enum o33FlagEnum
    {
        _NotSpecified = 0,
        Tel = 1,
        Email = 2,
        URL = 3,
        SKYPE = 4,
        ICQ = 5,
        FAX = 6,
        Other = 7,
        EmailCC = 21,
        EmailBCC = 22
    }

    public class o32Contact_Medium:BaseBO
    {
        public int p28ID { get; set; }
        public o33FlagEnum o33ID { get; set; } = o33FlagEnum._NotSpecified;
        public string o32Value { get; set; }
        public string o32Description { get; set; }
        public bool o32IsDefaultInInvoice { get; set; }

        public string o32Person { get; set; }
        public string o33Name { get; }       
        public string p28Name { get; }

        public bool IsSetAsDeleted { get; set; }
    }
}
