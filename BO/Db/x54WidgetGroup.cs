
namespace BO
{
    public class x54WidgetGroup:BaseBO
    {
        public int x01ID { get; set; }
        public string x54Name { get; set; }
        public string x54Code { get; set; }
        public int x54Ordinary { get; set; }
        public bool x54IsParamP28ID { get; set; }
        public bool x54IsParamToday { get; set; }
        public bool x54IsAllowAutoRefresh { get; set; }
        public bool x54IsAllowSkins { get; set; }
    }

    public class x57WidgetToGroup
    {
        public int x57ID { get; set; }
        public int x55ID { get; set; }
        public int x54ID { get; set; }
        public bool x57IsDefault { get; set; }
        public int x57Ordinary { get; set; }
        public string x55Name { get; }
        public string x55Code { get; }
        public string NamePlusCategory { get; }
    }
}
