using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UI.Models.Admin
{
    public class AdminCompanyLogo:BaseViewModel
    {
        public string LogoFile { get; set; }        
        public string UploadGuidLogo { get; set; }

        public bool IsMakeResize { get; set; }
    }
}
