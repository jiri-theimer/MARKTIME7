using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BO
{
    public class WorkflowEntityAttribute
    {
        public int b01ID { get; set; }
        public int b02ID { get; set; }
        public int j02ID_Owner { get; set; }
        public string UserInsert { get; set; }
        public bool IsStopNotify { get; set; }
    }
}
