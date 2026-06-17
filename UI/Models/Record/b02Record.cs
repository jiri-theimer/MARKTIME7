using BO;

namespace UI.Models.Record
{
    public class b02Record:BaseRecordViewModel
    {
        public BO.b02WorkflowStatus Rec { get; set; }
        public BO.b01WorkflowTemplate RecB01 { get; set; }
        public string ComboB01 { get; set; }
    }
}
