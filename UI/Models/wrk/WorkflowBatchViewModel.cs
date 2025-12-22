using UI.Views.Shared.Components.myGrid;

namespace UI.Models.wrk
{
    public class WorkflowBatchViewModel:BaseViewModel
    {
       
        public string Record_Prefix { get; set; }
        public string Record_Pids { get; set; }

        public List<LocalRepeater> lisRecs { get; set; }
        public IEnumerable<BO.b06WorkflowStep> lisB06_All { get; set; }

        
    }

    public class LocalStep
    {
        public int b06ID { get; set; }
        public string b06Name { get; set; }
    }

    public class LocalRepeater
    {
        public int pid { get; set; }
        public string Alias { get; set; }
        public int b02ID { get; set; }
        public string b02Name { get; set; }
        public string b02Color { get; set; }
        public string Comment { get; set; }
       
        public int SelectedB06ID { get; set; }


        public List<LocalStep> LocalSteps { get; set; }

       
    }
}
