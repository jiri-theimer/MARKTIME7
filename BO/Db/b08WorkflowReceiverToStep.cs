using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BO
{
    public class b08WorkflowReceiverToStep:BaseBO
    {
        
        public int b06ID { get; set; }
        public int j11ID { get; set; }
        public int j04ID { get; set; }
        public int x67ID { get; set; }
        public bool b08IsRecordOwner { get; set; }
        public bool b08IsRecordCreator { get; set; }

        public string x67Name { get; }
        public string j04Name { get; }
        public string j11Name { get; }
    }
}
