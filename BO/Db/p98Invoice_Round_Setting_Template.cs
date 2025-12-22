using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BO
{
    public class p98Invoice_Round_Setting_Template:BaseBO
    {
        public int x01ID { get; set; }
        public string p98Name { get; set; }
        public bool p98IsDefault { get; set; }
        public bool p98IsIncludeInVat { get; set; }
    }
}
