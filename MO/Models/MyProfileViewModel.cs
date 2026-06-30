using UAParser;

namespace MO.Models
{
    public class MyProfileViewModel : BaseViewModel
    {
        public BO.j02User RecJ02 { get; set; }

        public string userAgent { get; set; }
        public ClientInfo client_info { get; set; }
    }
}
