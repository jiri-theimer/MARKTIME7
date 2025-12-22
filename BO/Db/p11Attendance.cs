using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BO
{
    public class p11Attendance:BaseBO
    {
        public int j02ID { get; set; }
        public DateTime? p11Date { get; set; }
        public DateTime? p11TodayStart { get; set; }
        public DateTime? p11TodayEnd { get; set; }
        public string Prichod { get; }
        public string Odchod { get; }

        public string Prestavky_Inline { get; }
        public float Hodiny_Prestavka { get; }
        public float Hodiny_VPraci { get; }
        public float Hodiny_VPraciVcPrestavek { get; }
        public float Hodiny_Nepritomnost { get; set; }
        public float Hodiny_Vykazane { get; set; }
    }
}
