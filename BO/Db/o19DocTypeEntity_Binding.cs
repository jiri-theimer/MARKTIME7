using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BO
{
    public class o19DocTypeEntity_Binding:BaseBO
    {
        public int o20ID { get; set; }
        public int o23ID { get; set; }
        public int o19RecordPid { get; set; }
        public string RecordAlias { get; }
        public string o20Entity { get; set; }
        public int o18ID { get; }
        public string o18Name { get; }
        public string o23Name { get; }
        public string o20Name { get; }
        public int o20Ordinary { get; }
        public bool o20IsMultiselect { get; }

        public bool IsSetAsDeleted { get; set; }
    }
}
