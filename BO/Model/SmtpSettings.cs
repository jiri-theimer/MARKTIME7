
namespace BO
{
    public class SmtpSettings
    {
        public string Host { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public int Port { get; set; } = 25;
        public bool EnableSsl { get; set; }     //STARTTLS (typicky port 587)
    }
}
