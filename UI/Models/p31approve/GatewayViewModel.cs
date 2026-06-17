using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UI.Views.Shared.Components.myGrid;
using UI.Views.Shared.Components.myPeriod;

namespace UI.Models.p31approve
{
    public class GatewayViewModel:p31approveMother
    {
      public BO.myQueryP31 myQueryP31 { get; set; }
        public string pidsinline { get; set; }
        public List<int> lisInputPids { get;set; }
        public BO.p72IdENUM p72id { get; set; }
        
        public int approvinglevel { get; set; }        

       public string guid_pids { get; set; }
       public string guid_grid_p31 { get; set; }

        public myGridInput gridinput { get; set; }

        public myPeriodViewModel periodinput { get; set; } //fixní filtr v horním pruhu
        
        public int p31statequery { get; set; }  //fixní filtr

        public string p31tabquery { get; set; }   //fixní filtr v horním pruhu

        public bool IsSkipGateway { get; set; } //přeskočit úvodní stránku/bránu schvalování
    }


}
