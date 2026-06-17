using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UI.Models
{
    public class RecordValidity:BaseViewModel
    {
        public DateTime d1 { get; set; }
        public DateTime d2 { get; set; }
        public bool IsAutoClose { get; set; }
    }
}
