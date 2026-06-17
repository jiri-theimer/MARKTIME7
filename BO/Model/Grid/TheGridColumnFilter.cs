using System;
using System.Collections.Generic;
using System.Text;

namespace BO
{
    public class TheGridColumnFilter    //odesílá se z UI gridu a ukládá jako velký string do [j72Filter]
    {
        public string field { get; set; }
        public string oper { get; set; }
        public string value { get; set; }


        public BO.TheGridColumn BoundColumn {get;set;}
        public string value_alias { get; set; }
        public string hidden_input { get; set; }
        
        public string c1value { get; set; }
        public string c2value { get; set; }

        
    }
}
