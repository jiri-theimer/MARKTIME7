using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UI.Models.Admin
{
    public class p07ProjectLevelsViewModel:BaseViewModel
    {
        public BO.p07ProjectLevel RecL1 { get; set; }
        public BO.p07ProjectLevel RecL2 { get; set; }
        public BO.p07ProjectLevel RecL3 { get; set; }
        public BO.p07ProjectLevel RecL4 { get; set; }
        public BO.p07ProjectLevel RecL5 { get; set; }

        public bool UseL1 { get; set; }
        public bool UseL2 { get; set; }
        public bool UseL3 { get; set; }
        public bool UseL4 { get; set; }
        public bool UseL5 { get; set; }
    }
}
