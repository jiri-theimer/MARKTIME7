using System;
using System.Text;

namespace BO
{

    public enum DeviceTypeFlag
    {
        Desktop = 1,
        Phone = 2
    }

    public enum j02ModalWindowsFlagENUM
    {
        Default = 1,
        FullScreen = 2,
        KeepMobileUI=3,
        KeepDesktopUI=4
    }
    public enum j02TwoFactorVerifyFlagENUM
    {
        None = 0,
        AlwaysAfterLogin = 1,
        IfChangedUserAgend = 2
    }
    public enum j02WorksheetOperFlagENUM
    {
        None=0,
        ZakazPrekrocitFond=1,
        ZakazVidetVyuctovaneUkony=2
    }


    public class j02User : BaseBO
    {
        public string j02MyMenuLinks { get; set; }
        public string j02PasswordHash { get; } //nový soupec verze 7
        public bool j02IsDebugLog { get; set; }     //nový sloupec verze 7
        public string j02HomePageUrl { get; set; }  //nový sloupec verze 7
        public string j02Code { get; set; }
        public int j02LangIndex { get; set; }
        public string j02Login { get; set; }
        public bool j02IsLoginManualLocked { get; set; }
        public bool j02IsLoginAutoLocked { get; set; }
        public DateTime? j02AutoLockedWhen { get; set; }
        public j02TwoFactorVerifyFlagENUM j02TwoFactorVerifyFlag { get; set; }
        public int j04ID { get; set; }
        public int j07ID { get; set; }
        public int j18ID { get; set; }
        public int c21ID { get; set; }
        
        public j02WorksheetOperFlagENUM j02WorksheetOperFlag { get; set; }
        public int j02VirtualParentID { get; set; }
        public j02ModalWindowsFlagENUM j02ModalWindowsFlag { get; set; } = BO.j02ModalWindowsFlagENUM.FullScreen;
        public int j02GridCssBitStream { get; set; }    //css pro grid 2: resize, 4: fixed, 8: auto, 16: licha/suda, 32: mřížka, 64: hover,  128: výška 1, 256: výška 1.5, 512: výška 2
        public int j02UIBitStream { get; set; }     //nastavení rozložení panelů v modulech
        
        public int j02NotifySubscriberFlag { get; set; }

        public DateTime? j02LiveChatTimestamp { get; set; }


        public DateTime? j02HiddenSessionTimestamp { get; set; }

        public int j02AccessFailedCount { get; set; }
        public bool j02IsMustChangePassword { get; set; }
        public DateTime? j02PasswordExpiration { get; set; }

        public double Rezerva { get; set; } //pro výpočet časového fondu apod.

        public DeviceTypeFlag j02Ping_DeviceTypeFlag { get; set; } = DeviceTypeFlag.Desktop;   // nový sloupec ve verzi 6
        public int j02Ping_InnerWidth { get; set; }   // nový sloupec ve verzi 6
        public int j02Ping_InnerHeight { get; set; }  // nový sloupec ve verzi 6

        public DateTime? j02Ping_TimeStamp { get; set; }

        public int j02HesBitStream { get; set; }
        public int j02BitStream { get; set; }
        public string j02DefaultHoursFormat { get; set; }   // N nebo T

        public string j02LastName { get; set; }
        public string j02FirstName { get; set; }
        public string j02TitleBeforeName { get; set; }
        public string j02TitleAfterName { get; set; }
        public string j02Email { get; set; }
        public string j02Mobile { get; set; }
        public string j02EmailSignature { get; set; }
        public string j02InvoiceSignatureText { get; set; }
        public string j02InvoiceSignatureFile { get; set; }
        
        public string j02CountryCode { get; set; }
        public string j02SkinBackColor { get;set; }
        public string j02SkinForeColor { get; set; }
        public string j02SkinSelColor { get; set; }

        
        public int j02FontSizeFlag { get; set; }     // zvolená velikost písma
        public int j02PerformanceFlag { get; set; } //1:vypnout stránkování + veličiny v záložkách
        public int j02TimesheetEntryDaysBackLimit { get; set; } //kolik dní dozadu smí vykazovat
        public string j02TimesheetEntryDaysBackLimit_p34IDs { get; set; }
        public int p72ID_NonBillable { get; set; }
        //readonly:
        public string j02Cache_j11IDs { get; }
        
        public int j02Cache_p31Count { get; }  //celkový počet vykázaných úkonů
        public int j02Cache_BellsCount { get; } //počet aktivních zvonečků
        public string j04Name { get; }
        public string j07Name { get; }
        public string c21Name { get; }
        public BO.c21ScopeFlagENUM c21ScopeFlag { get; }
        
        public string j18Name { get; }

        public int j02AutocompleteFlag { get; set; }    //1:autocomplete on
        public int j02MySearchBitStream { get; set; }   //nastavení fulltext hledání
        public double j02Plan_Internal_Rate { get; set; }   //simulační nákladová sazba
        public DateTime? j02ReadNews_Timestamp { get; set; }

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


    }


}
