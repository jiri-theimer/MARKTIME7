using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UI.Models.Record
{
    public class x67Record:BaseRecordViewModel
    {
        public BO.x67EntityRole Rec { get; set; }

        
        public List<int> SelectedPermNumbers { get; set; }
        public IEnumerable<BO.Permission> lisAllPermissions { get; set; }

        public string Entity { get; set; }
        
        public List<BO.o28ProjectRole_Workload> lisO28 { get; set; }

       
    }
}
