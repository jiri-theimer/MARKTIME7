namespace MO.Models
{
    public class UkolViewModel : BaseViewModel
    {
        public int pid { get; set; }

        // ===== Jen pro čtení (kontext úkolu) =====
        public string TaskType { get; set; }
        public string ProjectDisplay { get; set; }
        public int p41ID { get; set; }
        public string Owner { get; set; }
        public string WorkflowStatusName { get; set; }
        public string WorkflowStatusColor { get; set; }
        public string WorkflowStatusForeColor { get; set; }

        // ===== Editovatelné =====
        public string Name { get; set; }

        /// <summary>Cílový workflow stav (b02ID). Ukládá se přes Factory.WorkflowBL.RunWorkflowStatus, ne přes p56TaskBL.Save.</summary>
        public int TargetB02ID { get; set; }
        public IEnumerable<ComboItem> StatusOptions { get; set; } = new List<ComboItem>();

        public string PlanFrom { get; set; }     // yyyy-MM-dd, prázdné = bez data
        public string PlanUntil { get; set; }    // yyyy-MM-dd, prázdné = bez data
        public string Notepad { get; set; }

        public bool IsReadOnly { get; set; }

        // ===== Návrat zpět =====
        public string Ret { get; set; }
        public string RetD { get; set; }
    }
}