
namespace UI.Models.Record
{
    public class p53Record:BaseRecordViewModel
    {
        public BO.p53VatRate Rec { get; set; }
        
        public string ComboJ27Code { get; set; }
        public DateTime? d1 { get; set; }
        public DateTime? d2 { get; set; }
    }
}
