using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BO
{
    public class c23PersonalDayColor:BaseBO
    {
        public int j02ID { get; set; }
        public int c24ID { get; set; }
        public DateTime c23Date { get; set; }

        public string Person { get; }
        public string c24Name { get; }
        public string c24Color { get; set; }
    }
}
