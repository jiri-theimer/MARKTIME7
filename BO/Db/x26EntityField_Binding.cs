using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BO
{
    public class x26EntityField_Binding
    {
        public int x26ID { get; set; }
        public int x28ID { get; set; }
        public bool x26IsEntryRequired { get; set; }
        public int x26RecTypePid { get; set; }
        public string x26RecTypeEntity { get; set; }

        public string EntityTypeName { get; set; }      //pro formulář x28
        public bool IsChecked { get; set; } //pro formulář x28
    }
}
