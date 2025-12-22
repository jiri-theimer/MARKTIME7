using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UI.Models.Record
{
    public class j18Record:BaseRecordViewModel
    {
        public BO.j18CostUnit Rec { get; set; }
        

        public RoleAssignViewModel roles { get; set; }
    }
}
