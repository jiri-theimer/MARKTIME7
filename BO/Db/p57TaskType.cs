

namespace BO
{
    
    public enum p57PlanScopeEnum
    {
        _Default=0,
        NenabizetPlan=1,
        PlanJePovinny=2,
        PlanHodinJePovinny=3,
        PlanVydajuJePovinny=4,
        NenabizetPlanAniTermin=5
    }
    public enum p57HelpdeskFlagEnum
    {
        _Default=0,
        Helpdesk=1
    }
    public enum p57ProjectFlagEnum
    {
        _Default=0,
        ProjectCompulsory=1,
        ProjectHidden=2
    }
    public class p57TaskType:BaseBO
    {
        public int b01ID { get; set; }
        public string p57Name { get; set; }
        public p57PlanScopeEnum p57PlanScope { get; set; }
        public int p57Ordinary { get; set; }
        public int x01ID { get; set; }        
        public int x38ID { get; set; }
        public string b01Name { get; }
        public p57ProjectFlagEnum p57ProjectFlag { get; set; }
        public p57HelpdeskFlagEnum p57HelpdeskFlag { get; set; }
    }
}
