

namespace BO
{
    public class x51HelpCore:BaseBO
    {               
        public string x51Name { get; set; }
        public string x51ViewUrl { get; set; }
        public string x51ExternalUrl { get; set; }
        public string x51Html { get; set; }        
        public int x51Ordinary { get; set; }
        public string x51NearUrls { get; set; }

        public int x51ParentID { get; set; }  // strom
        public string x51TreePath { get; set; }   // strom
        public int x51TreeLevel { get; set; } // strom
        public int x51TreeIndex { get; set; } // strom
        public int x51TreePrev { get; set; }  // strom
        public int x51TreeNext { get; set; }  // strom
    }
}
