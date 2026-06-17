using System;

namespace BO
{
    public enum x43RecipientIdEnum
    {
        recTO = 1,
        recCC = 2,
        recBCC = 3
    }
    public class x43MailQueue_Recipient:BaseBO
    {
        public int x40ID { get; set; }
        public string x43DisplayName { get; set; }
        public string x43Email { get; set; }
        public x43RecipientIdEnum x43RecipientFlag { get; set; }
    }
}
