
namespace BO
{
    public enum o21TypeFlagEnum
    {
        Udalost = 1,
        Lhuta = 2,
        Milnik = 3
    }
    public class o21MilestoneType:BaseBO
    {
        public string o21Name { get; set; }
        public o21TypeFlagEnum o21TypeFlag { get; set; }
        public int o21Ordinary { get; set; }
        public int x01ID { get; set; }
        public string o21Color { get; set; }
        public bool o21IsP41Compulsory { get; set; }
    }
}
