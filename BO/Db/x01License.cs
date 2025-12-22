

namespace BO
{
    public enum x01LockFlagEnum
    {
        None=0,
        PouzeAdmin=2,        
        VypnutyRobot=4,
        PouzeAdminPlusVypnutyRobot = 6
    }
    public class x01License:BaseBO
    {
        public Guid x01Guid { get; }    //raději pouze pro čtení, hodnota se generuje v SP
        
        public string x01Name { get; set; }
        public string x01AppName { get; set; }
        public string x01AppHost { get; set; }
        public string x01LoginDomain { get; set; }
        public string x01CountryCode { get; set; }
        public string x01ContactEmail { get; set; }
        public string x01ContactName { get; set; }
        public int x01LimitUsers { get; set; }
        public int j27ID { get; set; }
        public int x04ID_Default { get; set; }
        public x15IdEnum x15ID { get; set; }
        public bool x01IsAllowPasswordRecovery { get; set; }
        public string x01PasswordRecovery_Question { get; set; }
        public string x01PasswordRecovery_Answer { get; set; }
        public string x01LogoFileName { get; set; }
        public int x01Round2Minutes { get; set; }
        public string x01ApiKey { get; set; }        
        public string x01CustomCssFile { get; set; }
        public int x01LangIndex { get; set; }
       
        public int x01InvoiceMaturityDays { get; set; }
        
        public string x01BillingLang1 { get; set; }
        public string x01BillingLang2 { get; set; }
        public string x01BillingLang3 { get; set; }
        public string x01BillingLang4 { get; set; }
        public string x01BillingFlag1 { get; set; }
        public string x01BillingFlag2 { get; set; }
        public string x01BillingFlag3 { get; set; }
        public string x01BillingFlag4 { get; set; }
        public string x01ImportCnb_j27Codes { get; set; }
        public bool x01IsAllowDuplicity_RegID { get; set; }
        public bool x01IsAllowDuplicity_VatID { get; set; }
        public bool x01IsAllowDuplicity_p86 { get; set; }
        public bool x01IsCapacityFaNefa { get; set; }
        public int b02ID { get; set; }
        public string x01RobotLogin { get; set; }
        public int x01RecordsCountP31 { get; set; }
        public int x01LockFlag { get; set; }    //2: přístup pouze pro admina, 4: vypnutý robot

        
       

        public string GetBillingLang(int intLangIndex)
        {
            switch (intLangIndex)
            {
                case 1:
                    return this.x01BillingLang1;
                case 2:
                    return this.x01BillingLang2;
                case 3:                
                    return this.x01BillingLang3;
                case 4:                
                    return this.x01BillingLang4;
                default:
                    return null;

            }
        }

        public string CloudDatabaseName
        {
            get
            {
                return "a7"+this.x01LoginDomain.Split(".")[0];
            }
        }

    }
}
