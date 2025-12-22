


using System.Reflection.Metadata;

namespace BO
{
    public class RunningUser
    {
        public int pid { get; }
        public string j02Login { get; set; }
        public int x01ID { get; }
        public bool IsHostingModeTotalCloud { get; set; }
        
        public int j07ID { get; }
        public bool j04IsModule_p41 { get; set; }
        public bool j04IsModule_p28 { get; set; }
        public bool j04IsModule_j02 { get; set; }
        public bool j04IsModule_p31 { get; set; }
        public bool j04IsModule_p91 { get; set; }
        public bool j04IsModule_p90 { get; set; }
        public bool j04IsModule_o23 { get; set; }
        public bool j04IsModule_p56 { get; set; }
        public bool j04IsModule_Widgets { get; set; }
        public bool j04IsModule_x31 { get; set; }
        public bool j04IsModule_p11 { get; set; }
        public bool j04IsModule_o43 { get; set; }
        public bool j04IsModule_r01 { get; set; }
        public bool j04IsModule_p49 { get; set; }

        public string PersonalPageUrl { get; }

        public string RoleValue { get; }
     
        public string j02MyMenuLinks { get; set; }
        public string j02SmsVerifyCode { get; set; }    //vyplněno: čeká se na zadání sms kódu 2-fakturového ověření
        public DateTime? j02SmsVerifyCreate { get; set; }   //čas vygenerování ověřovacího SMS kódu
        public int j02HesBitStream { get; set; }
        public string j02DefaultHoursFormat { get; set; }   // N nebo T

        public int j02CalendarTotalFlagValue { get; set; }
        public string j11IDs { get; }          // seznam týmů osoby
        public string MasterSlave_j02IDs { get; }
        public string MasterSlave_Approve_j02IDs { get; }
        public string o17IDs { get; }          //seznam nabízených menu agend
        public int x69ID_p41_j18 { get; }   //první projektová role nastavená ve středisku
        public int x69ID_p41_p28 { get; }   //první projektová role nastavená v klientovi
        public bool IsMasterPerson { get; }  //má pod sebou podřízené osoby
        public bool IsApprovingPerson { get; } //zda potenciálně může schvalovat úkony
        public bool IsBillingPerson { get; } //zda potenciálně může vyúčtovat úkony
        public int StopWatchFlag { get; }   //1: běží stopky, 2: stopky mají záznamy, 0 - nic
        public int BellsCount { get; }  //počet zvonečků k upozornění
        public int j02ID { get; }
        public string j02PasswordHash { get; }
        public bool j02IsDebugLog { get; }
        public int j02LangIndex { get; }
        public bool j02IsMustChangePassword { get; }
        public int j04ID { get; }
        public DateTime? j02LiveChatTimestamp { get; }
        
        public string j02HomePageUrl { get; }
        public string j02SkinBackColor { get; set; }    //set, protože v můj-profil se aktualizuje kvůli simulaci
        public string j02SkinForeColor { get; set; }    //set, protože v můj-profil se aktualizuje kvůli simulaci
        public string j02SkinSelColor { get; set; }     //set, protože v můj-profil se aktualizuje kvůli simulaci
        public int j02FontSizeFlag { get; }
        public j02ModalWindowsFlagENUM j02ModalWindowsFlag { get; }
        public DeviceTypeFlag j02Ping_DeviceTypeFlag { get; }
        public int j02Ping_InnerWidth { get; }  //je zde šířka obrazovky avail
        public int j02Ping_InnerHeight { get; } //je zde výška obrazovky AVAIL
        public DateTime? j02PingTimeStamp { get; }
        public string j04Name { get; }
        public int a55ID_o23 { get; }   //globálně preferovaná individuální stránka záznamu
        public int a55ID_p28 { get; }   //globálně preferovaná individuální stránka záznamu
        public int a55ID_le5 { get; }   //globálně preferovaná individuální stránka záznamu
        public int a55ID_le4 { get; }   //globálně preferovaná individuální stránka záznamu
        public int a55ID_j02 { get; }   //globálně preferovaná individuální stránka záznamu
        public int a55ID_p91 { get; }   //globálně preferovaná individuální stránka záznamu
        
        public string j02LastName { get; }
        public string j02FirstName { get; }
        public string j02TitleBeforeName { get; }
        public string j02Email { get; }
        public DateTime validfrom { get; }
        public DateTime validuntil { get; }
        
        public bool j02IsLoginManualLocked { get; }
        public bool j02IsLoginAutoLocked { get; }

        public string x01LoginDomain { get; }
        public string x01CustomCssFile { get; }
        public string x01AppName { get; }
        public string x01CountryCode { get; }
        public int j02GridCssBitStream { get; set; } //css pro grid 2: resize, 4: fixed, 8: auto, 16: licha/suda, 32: mřížka, 64: hover,  128: výška 1, 256: výška 1.5, 512: výška 2
        public int j02UIBitStream { get; }
        public j02TwoFactorVerifyFlagENUM j02TwoFactorVerifyFlag { get; }
        public byte j02PerformanceFlag { get; } //1: ohled na výkon - nenačítají se sumrow záznamů pro záložky
        public int j02WorksheetOperFlag { get; set; }
        public byte j02AutocompleteFlag { get; }
        public string j04GridColumnsExclude { get; }
        public DateTime? j02LastReadNews_Timestamp { get; }

        public bool TestPermission(BO.PermValEnum oneperm)
        {
            int index = (int)oneperm - 1;
            return index >= 0 && index < this.RoleValue.Length && this.RoleValue[index] == '1';

            //if (this.RoleValue.Substring((int)oneperm - 1, 1) == "1")
            //    return true;
            //else
            //    return false;


        }
        public bool TestPermission(BO.PermValEnum oneperm, BO.PermValEnum orperm)
        {

            int index = (int)oneperm - 1;
            if (index >= 0 && index < this.RoleValue.Length && this.RoleValue[index] == '1')
            {
                return true;
            }
            else
            {
                index = (int)orperm - 1;
                return index >= 0 && index < this.RoleValue.Length && this.RoleValue[index] == '1';
            }

            //if (this.RoleValue.Substring((int)oneperm - 1, 1) == "1")
            //{
            //    return true;
            //}
            //else
            //{
            //    if (this.RoleValue.Substring((int)orperm - 1, 1) == "1")
            //    {
            //        return true;
            //    }
            //}

            //return false;
                
        }

        

        private bool? _IsAdmin;
        public bool IsAdmin
        {
            get
            {
                if (_IsAdmin != null)
                {
                    return Convert.ToBoolean(_IsAdmin);
                }
                _IsAdmin = TestPermission(PermValEnum.GR_Admin);
               

                return Convert.ToBoolean(_IsAdmin);
            }
        }

        private bool? _IsVysledovkyAccess;
        public bool IsVysledovkyAccess
        {
            get
            {
                if (_IsVysledovkyAccess != null)
                {
                    return Convert.ToBoolean(_IsVysledovkyAccess);
                }
                _IsVysledovkyAccess = TestPermission(PermValEnum.GR_P31_Vysledovky);


                return Convert.ToBoolean(_IsVysledovkyAccess);
            }
        }


        private bool? _IsRatesAccess;
        public bool IsRatesAccess
        {
            get
            {
                if (_IsRatesAccess != null)
                {
                    return Convert.ToBoolean(_IsRatesAccess);
                }
                _IsRatesAccess = TestPermission(PermValEnum.GR_AllowRates);


                return Convert.ToBoolean(_IsRatesAccess);
            }
        }

        
        public string SiteMenuPersonalName
        {
            get
            {

                if (this.j02FirstName.Length + this.j02LastName.Length > 15)
                    return this.j02FirstName.ToUpper();
                else
                    return $"{this.j02FirstName.ToUpper()}_{this.j02LastName.ToUpper()}";
            }
        }

        public string LangName
        {
            get
            {
                switch (this.j02LangIndex)
                {
                    case 1:
                        return "EN";
                    case 2:
                        return "DE";
                    case 4:
                        return "SK";
                    default:
                        return "CS";
                }
            }
        }

        public string getFontSizeCss()
        {
            
            switch (this.j02FontSizeFlag)
            {
                case 1:
                    return "fontsize1.css";     //menší písmo                                       
                case 3:
                    return "fontsize3.css";        //větší písmo                                                        
                default:
                    return "fontsize2.css"; //výchozí písmo 
            }
           
        }

        public bool IsShowMenuNew() //zda zobrazovat menu [Nový]
        {
            if (this.IsAdmin || TestPermission(PermValEnum.GR_p41_Creator) || TestPermission(PermValEnum.GR_p28_Creator))
            {
                return true;
            }
            

            return false;
        }

        public List<BO.StringPair> Messages4Notify { get; set; }
        public void AddMessage(string strMessage, string strTemplate = "error")
        {
            if (Messages4Notify == null) { Messages4Notify = new List<BO.StringPair>(); };
            strMessage = strMessage.Replace("\"", "").Replace("\r\n","<hr>");
            Messages4Notify.Add(new BO.StringPair() { Key = strTemplate, Value = strMessage }); ;
        }

        public string GetLastMessageNotify()
        {
            if (Messages4Notify == null)
            {
                return null;
            }
            else
            {
                return Messages4Notify.Last().Value;
            }
            
        }

        

        public bool isclosed
        {
            get
            {                
                if (this.j02IsLoginAutoLocked || this.j02IsLoginManualLocked)
                {
                    return true;
                }
                if (DateTime.Now < this.validfrom || DateTime.Now > this.validuntil)
                {
                    return true;
                }
                return false;
            }
        }

       
        public string Inicialy
        {
            get
            {
                return $"{this.j02FirstName.Substring(0, 1)}{this.j02LastName.Substring(0, 1)}".ToUpper();
            }
        }
        public string FullNameAsc
        {
            get
            {
                return $"{this.j02TitleBeforeName} {this.j02FirstName} {this.j02LastName}".Trim();
            }
        }
        public string FullnameDesc
        {
            get
            {
                return $"{this.j02LastName} {this.j02FirstName} {this.j02TitleBeforeName}".Trim();                
            }
        }


        public bool IsHes(int hesOneValue)
        {
            return BO.Code.Bas.bit_compare_or(this.j02HesBitStream, hesOneValue);
        }
        
        public bool IsMobileDisplay()
        {
            //if (j02Login.Contains("admin@") || j02Login.Contains("mobile"))
            //{
            //    return true;    //testování - vyhodit!!
            //}
            if (j02ModalWindowsFlag == BO.j02ModalWindowsFlagENUM.KeepMobileUI) return true;
            if (j02ModalWindowsFlag == BO.j02ModalWindowsFlagENUM.KeepDesktopUI) return false;  //držet desktop rozhraní

            if (j02Ping_InnerWidth >0 && (j02Ping_InnerHeight>j02Ping_InnerWidth || j02Ping_InnerHeight<500 || j02Ping_InnerWidth < 800))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public string GetLayoutPerDisplay()
        {
           
            if (IsMobileDisplay())
            {
                return "~/Views/Shared/_LayoutMobile.cshtml";
            }
            else
            {
                return "~/Views/Shared/_Layout.cshtml";
            }
        }

        public bool IsPortalUserOnly()
        {
            if (this.j04IsModule_Widgets && !this.j04IsModule_p31 && this.RoleValue== "00000000000000000000000000000000000000000000000000")
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
