using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BO
{
  
   
    public enum r02WorksheetFlagEnum
    {
        None=1,
        Free=2,
        Penetration=3
    }
    
    public class r02CapacityVersion : BaseBO
    {
        public string r02Name { get; set; }
        public int r02Ordinary { get; set; }
        public int x01ID { get; set; }
        
        
        public r02WorksheetFlagEnum r02WorksheetFlag { get; set; }
        
    }
}
