using System.ComponentModel.DataAnnotations;

namespace BO
{
    public class x97Translate:BaseBO
    {
        [Key]
        public int x97ID { get; set; }
        public string x97Code { get; set; }
        public string x97Orig { get; set; }
        public string x97Lang1 { get; set; }
        public string x97Lang2 { get; set; }
        public string x97Lang3 { get; set; }
        public string x97Lang4 { get; set; }
        public string x97OrigSource { get; set; }
    }
}
