using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UI.Models
{
    public class RecordCode: BaseViewModel
    {
        public System.Data.DataTable dtLast10 { get; set; }
        public string CodeValue { get; set; }
        public string prefix { get; set; }
        public int pid { get; set; }
    }
}
