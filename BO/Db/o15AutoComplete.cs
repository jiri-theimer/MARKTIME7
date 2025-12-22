using System.ComponentModel.DataAnnotations;

namespace BO
{
    public enum AutoCompleteFlag 
    {
        TitulPred = 1,      
        TitulZa = 2,
        Pozice=3,
        Stat=328,
        UrlAdresa=427
    }
    public class o15AutoComplete:BaseBO
    {
        [Key]
        public int o15ID { get; set; }
        public int x01ID { get; set; }
        public AutoCompleteFlag o15Flag { get; set; }

        public string o15Value { get; set; }
        public int o15Ordinary { get; set; }
    }
}
