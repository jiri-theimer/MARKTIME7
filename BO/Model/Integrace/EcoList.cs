

namespace BO.Integrace.Ecomail
{
    public class List
    {
        public int id { get; set; }
        public string name { get; set; }
        public string from_name { get; set; }
        public string from_email { get; set; }
        public string reply_to { get; set; }
        public object sub_success_page { get; set; }
        public object sub_confirmed_page { get; set; }
        public object unsub_page { get; set; }
        public object double_optin { get; set; }
        public object conf_subject { get; set; }
        public object conf_message { get; set; }
    }
}
