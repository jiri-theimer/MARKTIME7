

namespace UI.Models
{
    public class RobotViewModel:BaseViewModel
    {
        public string ErrorMessage { get; set; }
        public string InfoMessage { get; set; }
        public string Guid { get; set; }
       
        public DateTime D0 { get; set; }
        public DateTime Today { get; set; }


        public BO.j40MailAccount smtp_account { get; set; }

        public string ManualOper { get; set; }      //ping, recovery
    }
}
