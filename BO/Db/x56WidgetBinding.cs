using System.ComponentModel.DataAnnotations;

namespace BO
{
    public class x56WidgetBinding:BaseBO
    {
        [Key]
        public int x56ID { get; set; }
        public int j02ID { get; set; }
        public int x54ID { get; set; }
        public string x56Skin { get; set; }
        public string x56Boxes { get; set; }
        public string x56DockState { get; set; }
    }
}
