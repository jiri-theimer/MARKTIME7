using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BO
{
    public enum p97AmountFlagEnum
    {
        VAT = 1,
        WithoutVAT = 2,
        WithVAT = 3
    }
    public class p97Invoice_Round_Setting:BaseBO
    {
        public int j27ID { get; set; }
        public int p98ID { get; set; }
        public p97AmountFlagEnum p97AmountFlag { get; set; }
        public int p97Scale { get; set; }

        private string j27Code { get; }
      
    }
}
