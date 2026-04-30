

namespace BO
{
    public class FieldBagParam
    {
        public int o58ID { get; set; }
       
        public string o58Code { get; set; }
        public string o58Name { get; set; }
        public int x24ID { get; set; }
        
        public int o59RecPid { get; set; }
        public string o59RecPrefix { get; set; }
        public string o59ValueString { get; set; }
        public double o59ValueNum { get; set; }
        public bool? o59ValueBoolean { get; set; }
        public DateTime? o59ValueDate { get; set; }

        public object DataValue { get
            {
                switch (this.x24ID)
                {

                    case 3:
                    case 1:
                        return this.o59ValueNum;
                    case 7:
                        return this.o59ValueBoolean;
                    case 4:
                    case 5:
                        return this.o59ValueDate;
                    
                    default:
                        return this.o59ValueString;
                }
            }
        }
        
        public string DataTypeName { get
            {
                
                switch (this.x24ID)
                {
                   
                    case 3:
                    case 1:
                        return "num";
                    case 7:
                        return "bool";
                    case 4:
                        return "date";
                    case 5:
                        return "datetime";
                    default:
                        return "string";
                }
            }
        }
    }
}
