using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UI.Models.Record
{
    public class j05Record:BaseRecordViewModel
    {
        public BO.j05MasterSlave Rec { get; set; }
        
        public bool SlaveIsTeam { get; set; }
       
        public string ComboPersonMaster { get; set; }
        public string ComboPersonSlave { get; set; }
        public string ComboTeamSlave { get; set; }
    }
}
