using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UI.Models.Record
{
    public class p29Record:BaseRecordViewModel
    {
        public BO.p29ContactType Rec { get; set; }
        
        public string ComboB01Name { get; set; }
        public string ComboX38Name { get; set; }

        public RoleAssignViewModel roles { get; set; }
        public CreateAssignViewModel creates { get; set; }
        
        

    }
}
