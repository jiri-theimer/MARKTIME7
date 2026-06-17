using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UI.Models
{
    public class FileUploadViewModel:BaseViewModel
    {
        public string Guid { get; set; }
        public string Entity { get; set; }

        public int RecPid { get; set; }

        public IEnumerable<BO.o27Attachment> lisO27 { get; set; }   //již uložené přílohy
        
        

        public List<BO.o27Attachment> lisTempFiles { get; set; } 

        public string o27Name { get; set; }    //popis
       
    }
}
