using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BO
{
    

    public enum p31RecordState
    {
        _NotExists = 0,
        Editing = 1,
        Locked = 2,
        Approved = 5,
        Disapproved = 6,
        Invoiced = 7,
        ExcludedFromBilling=8
    }

    public class p31RecDisposition:BaseRecDisposition
    {
        
        public BO.p31RecordState RecordState { get; set; }
        public string LockedReasonMessage { get; set; }
        
        public bool CanApprove { get; set; }
        public bool CanApproveAndEdit { get; set; }

    }
}
