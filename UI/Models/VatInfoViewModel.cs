using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UI.Models
{
    public class VatInfoViewModel:BaseViewModel
    {
        public string DIC { get; set; }

        public bool IsViesValid { get; set; }
        public string CompanyName { get; set; }
        public string CompanyAddress { get; set; }

        public string ADIS_Veta { get; set; }
    }
}
