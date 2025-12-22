using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UI.Models.Tab1
{
    public class p28Tab1:BaseTab1ViewModel
    {
        public BO.p28Contact Rec { get; set; }
        public BO.p28ContactSum RecSum { get; set; }
        //public IEnumerable<BO.o32Contact_Medium> lisO32 { get; set; }
        //public IEnumerable<BO.p26ProjectContact> lisP26 { get; set; }
        //public IEnumerable<BO.p30ContactPerson> lisP30 { get; set; }
        //public IEnumerable<BO.p41Project> lisP41 { get; set; }
        public bool JeKontaktniOsoba { get; set; }
    }
}
