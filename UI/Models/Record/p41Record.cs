

namespace UI.Models.Record
{

  
    public class p56Clone : BO.p56Task
    {
        public bool Importovat { get; set; }
        public string Project { get; set; }
    }
    public class p40Clone : BO.p40WorkSheet_Recurrence
    {
        public bool Importovat { get; set; }
    }

    public class p41Record:BaseRecordViewModel
    {
        public BO.p41Project Rec { get; set; }
        public BO.p42ProjectType RecP42 { get; set; }
        public BO.p07ProjectLevel RecP07 { get; set; }
        public BO.p41Project RecParent { get; set; }
        public string TempGuid { get; set; }
        public string UploadGuid { get; set; }
        
        //public int p51Flag { get; set; }    //1 - nemá ceník, 2 - přiřazený ceník, 3 - ceník na míru
        public RoleAssignViewModel roles { get; set; }
        public ReminderViewModel reminder { get; set; }
        public HlidacViewModel hlidac { get; set; }
        public DispoziceViewModel disp { get; set; }
        public FreeFieldsViewModel ff1 { get; set; }
        public IEnumerable<BO.p07ProjectLevel> lisParentLevels { get; set; }
        public int SelectedParentLevelIndex { get; set; }
        public int SelectedP07ID { get; set; }
        public bool CanEditRecordCode { get; set; }
        
        public string SelectedComboParent { get; set; }
        public string SelectedComboJ18Name { get; set; }
        public string SelectedComboP42Name { get; set; }
        public string SelectedComboP92Name { get; set; }
        public string SelectedComboP87Name { get; set; }
        public string SelectedComboP61Name { get; set; }
        public string SelectedComboP15Name { get; set; }
        public string SelectedComboOwner { get; set; }
        public string SelectedComboP51Name { get; set; }
        public string SelectedComboP51Name_Internal { get; set; }
        public int SelectedP51ID_Flag3 { get; set; }
        public int SelectedP51ID_Flag2 { get; set; }

        public string SelectedComboClient { get; set; }
        public string SelectedComboOdberatel { get; set; }

        public List<p26Repeater> lisP26 { get; set; }
        public BarcodesViewModel barcodes { get; set; }

        public List<p56Clone> lisP56Clone { get; set; }
        public List<p40Clone> lisP40Clone { get; set; }

        public Notepad.EditorViewModel Notepad { get; set; }
        public PellEditorViewModel BillingMemo { get; set; }
    }

    public class p26Repeater : BO.p26ProjectContact
    {
        
        public bool IsTempDeleted { get; set; }
        public string TempGuid { get; set; }
        public string ComboP28 { get; set; }
        public string CssTempDisplay
        {
            get
            {
                if (this.IsTempDeleted)
                {
                    return "display:none;";
                }
                else
                {
                    return "display:table-row;";
                }
            }
        }
    }
}
