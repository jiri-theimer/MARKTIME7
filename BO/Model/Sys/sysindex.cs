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
        public string index_keys { get; set; }      //klíčové sloupce ve formátu sp_helpindex: "a, b(-)", (-) = sestupně
        public string index_includes { get; set; }  //INCLUDE sloupce: "a, b" (starší Indexes.json je nemá)
        public string index_filter { get; set; }    //podmínka filtrovaného indexu
        public bool is_unique { get; set; }
        public bool is_primary_key { get; set; }
        public bool is_clustered { get; set; }
    }
}
