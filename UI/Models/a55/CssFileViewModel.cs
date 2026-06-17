using System;
using System.Collections.Generic;
using System.Linq;


namespace UI.Models.a55
{
    public class CssFileViewModel:BaseViewModel
    {
        public string FileName { get; set; }
        public string Content { get; set; }

        public List<string> lisFileNames { get; set; }
    }
}
