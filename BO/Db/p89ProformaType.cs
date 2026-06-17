using System;

namespace BO
{
    public class p89ProformaType:BaseBO
    {
        public int x01ID { get; set; }
        public int p93ID { get; set; }
        public int x31ID { get; set; }
        public int x31ID_Payment { get; set; }
        public int j27ID { get; set; }
       public int j61ID { get; set; }

        
        public byte p89FilesTab { get; set; }
        public byte p89RolesTab { get; set; }


        public string p89Name { get; set; }
        public string p89Code { get; set; }        
        public int x38ID { get; set; }        
        public int x38ID_Payment { get; set; }
        public string p89DefaultText1 { get; set; }
        public string p89DefaultText2 { get; set; }
        public string j27Code { get; }
    
        public string p93Name { get; }
       
    }
}
