using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BO
{
    public class x90Migration
    {
        public int x90ID { get; set; }
        public string x90Table1 { get; set; }
        public string x90Table2 { get; set; }
        public string x90SqlFrom1 { get; set; }
        public string x90SqlCondition1 { get; set; }
        public int x90Ordinary { get; set; }
        public bool x90IsUse { get; set; }
        public string x90Mapping1 { get; set; }
        public string x90Mapping2 { get; set; }
        public string x90Fields1Over { get; set; }
        public string x90Fields1Missing { get; set; }
        public string x90Version { get; set; }
    }
}
