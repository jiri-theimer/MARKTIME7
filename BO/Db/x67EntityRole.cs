

namespace BO
{
    public class x67EntityRole:BaseBO
    {
        public int x01ID { get; set; }
        public string x67Entity { get; set; }
        public int a55ID { get; set; }
        public string x67Name { get; set; }
        public string x67RoleValue { get; set; }
        public int x67Ordinary { get; set; }
        public bool x67IsRequired { get; set; }
        public string NameWithEntity { get
            {
                return this.x67Name + " (" + BO.Code.Entity.GetAlias(this.x67Entity) + ")";
            }
        }
        
    }
}
