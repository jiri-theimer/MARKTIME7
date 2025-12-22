using System;
using System.Collections.Generic;
using System.Text;

namespace BO.Sys
{
    public class SysDbObject
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public string Content { get; set; }

        public string xType { get; set; }
        public DateTime crDate { get; set; }
    }
}
