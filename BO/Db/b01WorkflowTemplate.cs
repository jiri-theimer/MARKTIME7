

namespace BO
{
    public enum b01PrincipleFlagEnum
    {
        Step=1,
        StatusOnly=2
    }
    public class b01WorkflowTemplate:BaseBO
    {        
        public string b01Name { get; set; }
        public int x01ID { get; set; }
        public string b01Entity { get; set; }
        public b01PrincipleFlagEnum b01PrincipleFlag { get; set; }

    }
}
