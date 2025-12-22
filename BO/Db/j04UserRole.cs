

namespace BO
{    
    public class j04UserRole : BaseBO
    {
        public int x01ID { get; set; }
        
        public string j04Name { get; set; }

        public int x67ID { get; set; }
        public bool j04IsModule_o23 { get; set; }
        public bool j04IsModule_p28 { get; set; }
        public bool j04IsModule_p41 { get; set; }
        public bool j04IsModule_j02 { get; set; }
        public bool j04IsModule_p31 { get; set; }
        public bool j04IsModule_p90 { get; set; }
        public bool j04IsModule_p91 { get; set; }
        public bool j04IsModule_p56 { get; set; }
        public bool j04IsModule_Widgets { get; set; }
        public bool j04IsModule_x31 { get; set; }
        public bool j04IsModule_p11 { get; set; }
        public bool j04IsModule_o43 { get; set; }
        public bool j04IsModule_r01 { get; set; }
        public bool j04IsModule_p49 { get; set; }
        public bool j04IsAllowLoginByGoogle { get; set; }
     
        public string j04HomePageUrl { get; set; }
        public string j04HomePageUrl_Mobile { get; set; }
       

        

        public int j04PasswordExpirationDays { get; set; }
        public int j04MobileStreamEnum { get; set; }  // výčet povolených dat pro mobilní aplikaci
        
        public byte j04FilesTab { get; set; }

        public int a55ID_o23 { get; set; }
        public int a55ID_p28 { get; set; }
        public int a55ID_le5 { get; set; }
        public int a55ID_le4 { get; set; }
        public int a55ID_p91 { get; set; }
        public int a55ID_j02 { get; set; }
        public string j04GridColumnsExclude { get; set; }

        public string x67RoleValue { get; }
        public string RoleValue
        {
            get
            {
                return x67RoleValue;
            }
        }

        
    }
}
