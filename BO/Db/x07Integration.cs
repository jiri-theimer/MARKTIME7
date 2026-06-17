

namespace BO
{
    public enum x07FlagEnum
    {
        Ecomail=1,
        SmartEmailing=2,
        AbraFlexi=3

    }
    public class x07Integration:BaseBO
    {
        public x07FlagEnum x07Flag { get; set; }
        public int x01ID { get; set; }
        public string x07Name { get; set; }
        public string x07Login { get; set; }
        public string x07Password { get; set; }
        public int x07Ordinary { get; set; }
        public string x07Token { get; set; }
    }
}
