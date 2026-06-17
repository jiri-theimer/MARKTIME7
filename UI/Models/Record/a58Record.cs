using System;
using System.Collections.Generic;
using System.Linq;

namespace UI.Models.Record
{
    public class a58Record: BaseRecordViewModel
    {
        public BO.a58RecPageBox Rec { get; set; }
        public BO.a59RecPageLayer RecA59 { get; set; }
        public BO.a55RecPage RecA55 { get; set; }
        

        public IEnumerable<BO.b06WorkflowStep> lisB06 { get; set; }
        
        public IEnumerable<BO.x31Report> lisX31 { get; set; }

        public int b01ID { get; set; }
       
        public string HtmlContentText { get; set; }

        public Notepad.EditorViewModel Notepad { get; set; }

    }
}
