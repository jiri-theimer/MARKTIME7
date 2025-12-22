using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UI.Models
{
    public class SelectDocTypeViewModel: BaseViewModel
    {
        public IEnumerable<BO.o18DocType> lisO18 { get; set; }

        public int SelectedPid { get; set; }

        public string prefix { get; set; }
        public int recpid { get; set; }
        public string RecordAlias { get; set; }
        public int o17id { get; set; }
        public int o43id_source { get; set; }
    }
}
