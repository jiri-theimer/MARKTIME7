using BO;

namespace UI.Models.capacity
{
    public class ProjectPlanViewModel:BaseViewModel
    {
       
        public int p41ID { get; set; }

        public CapacityTimelineJ02ViewModel timeline { get; set; }
        public bool HasOwnerPermissions { get; set; }


        public BO.p41Project RecP41 { get; set; }

        
        public DateTime p41PlanFrom { get; set; }
        public DateTime p41PlanUntil { get; set; }
    }

    
}
