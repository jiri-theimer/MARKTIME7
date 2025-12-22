using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BO.Sys
{
    public class sysconstraint:sysobject
    {
        public string tablename { get; set; }
        public int tableid { get; set; }
    }
}
