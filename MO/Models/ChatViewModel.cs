namespace MO.Models
{
    public class ChatViewModel : BaseViewModel
    {
        public bool IsAvailable { get; set; }
        public bool IsDenied { get; set; }
        public int MyID { get; set; }

        // jen stránka konverzace
        public int ChannelID { get; set; }
        public string ChannelName { get; set; }
        public int ChannelType { get; set; }    // 1 = přímá, 2 = skupina, 3 = k záznamu
        public int MembersCount { get; set; }
    }
}
