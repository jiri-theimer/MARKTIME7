

namespace BO
{    
    public enum o18TemplateENUM
    {
        None=0,
        Uctenka=1
    }
    public enum o18EntryCodeENUM
    {
        Manual = 1,  // 1-vyplňovat ručně kód
        NotUsed = 2, // 2-nepoužívat kód
        Autoo18 = 3, // 3-automaticky generovat v rámci dokumentu
        AutoP41 = 4, // 4-automaticky generovat v rámci projektu
        X38ID = 5     // podle číselné řady
    }


    public enum o18EntryNameENUM
    {
        Manual = 1,    // 1-vyplňovat ručně název    
        NotUsed = 2     // 2-nevyplňovat název
    }


    public enum o18EntryOrdinaryENUM
    {
        Manual = 1,      // 1-pořadí zadávat ručně   
        NotUsed = 2     // 2-nepracovat s pořadím
    }


    public enum o18GeoFlagEnum
    {
        _None = 0,
        LoadFromP15 = 1,
        LoadFromCurrentUser = 2
    }
    public enum o18BarcodeFlagEnum
    {
        _None=0,
        BarcodeSupport=1
    }



    public class o18DocType:BaseBO
    {
        public o18TemplateENUM o18TemplateFlag { get; set; }
        public int x01ID { get; set; }
        public int b01ID { get; set; }        
        public int x38ID { get; set; }
        public int o17ID { get; set; }
        public string o18Name { get; set; }
        public string o18Code { get; set; }
        public int o18Ordinary { get; set; }       
        public bool o18IsColors { get; set; }
        public bool o18IsAllowTree { get; set; }
        
        public string o18ReportCodes { get; set; }        
        public o18EntryNameENUM o18EntryNameFlag { get; set; } = o18EntryNameENUM.Manual;
        public o18EntryCodeENUM o18EntryCodeFlag { get; set; } = o18EntryCodeENUM.Manual;
        public o18EntryOrdinaryENUM o18EntryOrdinaryFlag { get; set; } = o18EntryOrdinaryENUM.Manual;
        
        public o18GeoFlagEnum o18GeoFlag { get; set; }
        public o18BarcodeFlagEnum o18BarcodeFlag { get; set; }


        public int o18MaxOneFileSize { get; set; }
        public string o18AllowedFileExtensions { get; set; }
        public bool o18IsAllowEncryption { get; set; }

        public byte o18NotepadTab { get; set; }
        public byte o18FilesTab { get; set; }
        public byte o18RolesTab { get; set; }
        public byte o18TagsTab { get; set; }
        public bool o18IsSeparatedNotepadTab { get; set; }

        public int p34ID_Uctenka { get; set; }
        public int p32ID_Uctenka { get; set; }

        public bool Is_p41 { get; }        
       
        public bool Is_p31 { get; }      
        public bool Is_j02 { get; }       
        public bool Is_o23 { get; }        
       
       
    }
}
