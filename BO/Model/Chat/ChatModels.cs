namespace BO.Chat
{
    /// <summary>Konverzace z pohledu přihlášeného uživatele (seznam vlevo).</summary>
    public class ChannelItem
    {
        public int pid { get; set; }
        public int u01TypeFlag { get; set; }
        public string u01Name { get; set; }
        public string u01RecordEntity { get; set; }
        public int u01RecordPid { get; set; }
        public int j02ID_Owner { get; set; }
        public DateTime? u01DateLastMessage { get; set; }
        public DateTime? u01DateInsert { get; set; }
        public bool u03IsMuted { get; set; }
        public bool u03IsPinned { get; set; }
        public int u03RoleFlag { get; set; }
        public int u02ID_LastRead { get; set; }
        public int UnreadCount { get; set; }
        public int MembersCount { get; set; }
        public int PartnerID { get; set; }              //pouze 1:1: druhý účastník
        public string PartnerName { get; set; }
        public int PartnerLastReadID { get; set; }      //pouze 1:1: poslední zpráva přečtená druhou stranou
        public int LastSenderID { get; set; }
        public string LastSenderName { get; set; }
        public string LastText { get; set; }
        public bool LastIsDeleted { get; set; }
        public int LastAttachmentsCount { get; set; }

        public string DisplayName
        {
            get
            {
                if (this.u01TypeFlag == 1)
                {
                    return string.IsNullOrWhiteSpace(this.PartnerName) ? "?" : this.PartnerName.Trim();
                }
                return string.IsNullOrWhiteSpace(this.u01Name) ? "?" : this.u01Name;
            }
        }
    }

    public class MessageItem
    {
        public int pid { get; set; }
        public int u01ID { get; set; }
        public int j02ID_Sender { get; set; }
        public string SenderName { get; set; }
        public int u02TypeFlag { get; set; }
        public string u02Text { get; set; }
        public int u02ID_ReplyTo { get; set; }
        public string ReplyText { get; set; }
        public string ReplySenderName { get; set; }
        public int u02AttachmentsCount { get; set; }
        public bool u02IsEdited { get; set; }
        public bool u02IsDeleted { get; set; }
        public DateTime u02DateInsert { get; set; }

        public List<AttachmentItem> Attachments { get; set; } = new List<AttachmentItem>();
        public List<ReactionItem> Reactions { get; set; } = new List<ReactionItem>();
    }

    public class AttachmentItem
    {
        public int o27ID { get; set; }
        public int o27RecordPid { get; set; }
        public string o27OriginalFileName { get; set; }
        public int o27FileSize { get; set; }
        public string o27ContentType { get; set; }
    }

    public class ReactionItem
    {
        public int u02ID { get; set; }
        public int j02ID { get; set; }
        public string u04Emoji { get; set; }
        public string PersonName { get; set; }
    }

    /// <summary>Obsazená role v záznamu a uživatelé, kteří ji zastávají (nabídka při přidávání členů do konverzace k záznamu).</summary>
    public class RoleItem
    {
        public string Name { get; set; }
        public List<int> j02IDs { get; set; } = new List<int>();
    }

    public class MemberItem
    {
        public int j02ID { get; set; }
        public string j02Login { get; set; }
        public string PersonName { get; set; }
        public int u03RoleFlag { get; set; }
        public bool u03IsMuted { get; set; }
        public int u02ID_LastRead { get; set; }
    }
}
