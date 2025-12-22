using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UI.Models.Home
{
    public class ChangePasswordViewModel:BaseViewModel
    {
        public string NewPassword { get; set; }
        public string CurrentPassword { get; set; }
        public string VerifyPassword { get; set; }
    }
}
