using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BO
{
    public enum r04WorksheetFlagEnum
    {
        Free = 1,
        ForbiddenAll = 2,
        ForbiddenFa=3
    }
    public class r04CapacityResource:BaseBO
    {
        public int j02ID { get; set; }
        public int p41ID { get; set; }
        public string r04Text { get; set; }
        public double r04HoursFa { get; set; }
        public double r04HoursNeFa { get; set; }
        public double r04HoursTotal { get; set; }
        

        public r04WorksheetFlagEnum r04WorksheetFlag { get; set; }


        public string Person { get; }
        public string Project { get; }
        public string Client { get; }
        public int p41CapacityStream { get; }

        private bool? _FaZastropovan { get; set; }
        private bool? _NefaZastropovan { get; set; }
        public bool FaZastropovan
        {
            get
            {
                switch (_FaZastropovan)
                {
                    case null:
                        _FaZastropovan = BO.Code.Bas.bit_compare_or(this.p41CapacityStream, 2);
                        return Convert.ToBoolean(_FaZastropovan);
                    case true:
                        return true;
                    case false:
                        return false;
                }
                
            }
        }
        
        public bool NefaZastropovan
        {
            get
            {
                switch (_NefaZastropovan)
                {
                    case null:
                        _NefaZastropovan = BO.Code.Bas.bit_compare_or(this.p41CapacityStream, 4);
                        return Convert.ToBoolean(_NefaZastropovan);
                    case true:
                        return true;
                    case false:
                        return false;
                }
            }
        }
    }
}
