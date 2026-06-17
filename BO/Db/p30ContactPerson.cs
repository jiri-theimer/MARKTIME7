using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BO
{
    public class p30ContactPerson:BaseBO
    {
        public int p28ID { get; set; }
        public int p28ID_Person { get; set; }        
        public string p30Name { get; set; }
        
        public bool IsSetAsDeleted { get; set; }
                
        public string Person { get; }
        public string PersonWithInvoiceEmail { get; }
        public string Mother { get; }
        public string PersonInvoiceEmail { get; }
    }
}
