using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UI.Models.Admin
{
    public class AdminHome: BaseViewModel
    {
        public string IISBinding { get; set; }
        public IEnumerable<BO.p07ProjectLevel> lisP07 { get; set; }

        public BO.x01License RecX01 { get; set; }
        
    }
}
