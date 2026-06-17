
namespace UI.Models.Record
{
    public class j40Record:BaseRecordViewModel
    {
        public BO.j40MailAccount Rec { get; set; }
        public string ComboPerson { get; set; }
        
        public bool IsUseSSL { get; set; }
    }
}
