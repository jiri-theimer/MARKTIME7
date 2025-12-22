using System.ComponentModel.DataAnnotations;
namespace BO
{
    public class o52TagBinding
    {
        [Key]
        public int o52ID { get; set; }
        public int o51ID { get; set; }
        public int o52RecordPid { get; set; }
        public string o52RecordEntity { get; set; }

        public string o51Name { get; }

    }
}
