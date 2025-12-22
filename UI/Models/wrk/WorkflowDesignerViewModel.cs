namespace UI.Models.wrk
{
    public class WorkflowDesignerViewModel:BaseViewModel
    {
       
        public IEnumerable<BO.b01WorkflowTemplate> lisB01 { get; set; }
        public int SelectedB01ID { get; set; }
        public BO.b01WorkflowTemplate RecB01 { get; set; }
        public IEnumerable<BO.j61TextTemplate> lisJ61 { get; set; }

        public IEnumerable<BO.b02WorkflowStatus> lisB02 { get; set; }
        public IEnumerable<BO.b06WorkflowStep> lisB06 { get; set; }

        public IEnumerable<BO.b11WorkflowMessageToStep> lisAllB11 { get; set; }
    }
}
