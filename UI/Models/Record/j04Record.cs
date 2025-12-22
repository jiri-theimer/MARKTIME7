using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UI.Models.Record
{
    public class j04Record:BaseRecordViewModel
    {
        public BO.j04UserRole Rec { get; set; }
        public List<int> SelectedPermNumbers { get; set; }
        public IEnumerable<BO.Permission> lisAllPerms { get; set; }

        public List<int> SelectedX54IDs { get; set; }
        public IEnumerable<BO.x54WidgetGroup> lisX54 { get; set; }
    }
}
