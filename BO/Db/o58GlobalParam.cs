

namespace BO
{
    public class o58GlobalParam:BaseBO
    {        
        public int x01ID { get; set; }
        public BO.x24IdENUM x24ID { get; set; }
        public string o58Entity { get; set; }
        public string o58Key { get; set; }
        public string o58Name { get; set; }
        public int o58Ordinary { get; set; }
        public string o58DefaultValue { get; set; }
        public Boolean o58IsPerUser { get; set; }
        public bool o58IsEditable { get; set; }
    }
}
