using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UI.Models
{
    public class ReportNoContextFrameworkViewModel:BaseViewModel
    {
        public IEnumerable<BO.x31Report> lisX31 { get; set; }
        public List<BO.j25ReportCategory> lisJ25 { get; set; }

        
        public BO.x31Report SelectedReport { get; set; }

        public string caller_prefix { get; set; }
        public string caller_pids { get; set; }
    }
}
