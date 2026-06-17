using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BO.Sys
{
    public class sysindex
    {
        public string tabname { get; set; }
        public int tabid { get; set; }
        public string index_name { get; set; }
        public string index_description { get; set; }
        public string index_keys { get; set; }
    }
}
