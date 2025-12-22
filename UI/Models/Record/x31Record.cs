using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UI.Models.Record
{
    public class x31Record:BaseRecordViewModel
    {
        public BO.x31Report Rec { get; set; }

        public BO.o27Attachment RecO27 { get; set; }
        public string UploadGuid { get; set; }
        public string ComboJ25Name { get; set; }

        public List<BO.ThePeriod> lisPeriodSource { get; set; }

        public RoleAssignViewModel roles { get; set; }
    }
}
