
namespace BO
{   
    public enum b02AutoRunFlagEnum
    {
        Standard = 0,
        Startovaci=1,
        Technicky=2
    }
    public enum b02RecordFlagEnum
    {
        _None = 0,
        ZaznamOtevreny = 1,
        ZaznamVArchivu = 2
    }

    public class b02WorkflowStatus:BaseBO
    {       
        public int b01ID { get; set; }
        public string b02Name { get; set; }
       
        public string b02Color { get; set; }
        public int b02Ordinary { get; set; }
        
        public b02AutoRunFlagEnum b02AutoRunFlag { get; set; }
        public b02RecordFlagEnum b02RecordFlag { get; set; }
        public bool b02IsRecordReadOnly4Owner { get; set; }
       
       

        public string b01Name { get; }
        public string b01Entity { get; }


        public string NameWithStatus{ get
            {
                return this.b01Name + " (" + this.b02Name + ")";
            }
        }

        
    }
}
