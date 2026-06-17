using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UI.Models.Record
{
    public class a59Record: BaseRecordViewModel
    {
        public BO.a59RecPageLayer Rec { get; set; }

        public BO.a55RecPage RecA55 { get; set; }

        public string HtmlContentHelp { get; set; }
        public string EditorLanguageKey { get; set; }

        public List<int> SelectedB02IDs { get; set; }
        public IEnumerable<BO.b02WorkflowStatus> lisB02 { get; set; }
    }
}
