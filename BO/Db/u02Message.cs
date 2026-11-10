namespace BO
{
    /// <summary>Zpráva interního chatu.</summary>
    public class u02Message : BaseBO
    {
        public int u01ID { get; set; }
        public int j02ID_Sender { get; set; }
        public int u02TypeFlag { get; set; } = 1;       //1: zpráva uživatele, 2: systémová
        public string u02Text { get; set; }
        public string u02Text200 { get; set; }
        public int u02ID_ReplyTo { get; set; }
        public int u02AttachmentsCount { get; set; }
        public bool u02IsEdited { get; set; }
        public bool u02IsDeleted { get; set; }
    }
}
