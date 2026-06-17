using System.ComponentModel.DataAnnotations;

namespace BO
{
    public class x36UserParam:BaseBO
    {
        [Key]
        public int x36ID { get; set; }
        public string x36Key { get; set; }
        public string x36Value { get; set; }
        public int j02ID { get; set; }

    }
}
