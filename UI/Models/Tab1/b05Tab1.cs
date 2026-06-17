namespace UI.Models.Tab1
{
    public class b05Tab1: BaseTab1ViewModel
    {
        public BO.b05Workflow_History Rec { get; set; }

        public BO.p41Project RecP41 { get; set; }
        public BO.p28Contact RecP28 { get; set; }
        public BO.o23Doc RecO23 { get; set; }

        public IEnumerable<BO.o27Attachment> lisO27 { get; set; }
    }
}
