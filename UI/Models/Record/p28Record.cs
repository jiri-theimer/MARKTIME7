

namespace UI.Models.Record
{
 
    public class p28Record : BaseRecordViewModel
    {
        public int IsCompany { get; set; }  //0,1
        //public int p51Flag { get; set; }    //1 - nemá ceník, 2 - přiřazený ceník, 3 - ceník na míru
        public BO.p28Contact Rec { get; set; }
        public BO.p29ContactType RecP29 { get; set; }

        
        public string SelectedComboP29Name { get; set; }
        public string SelectedComboP92Name { get; set; }

        public string SelectedComboJ61Name { get; set; }
        public string SelectedComboP63Name { get; set; }
        public string SelectedComboOwner { get; set; }
        public string SelectedComboP51Name { get; set; }
        public int SelectedP51ID_Flag3 { get; set; }
        public int SelectedP51ID_Flag2 { get; set; }
        public string SelectedComboParentP28Name { get; set; }

        public PellEditorViewModel BillingMemo { get; set; }
        public FreeFieldsViewModel ff1 { get; set; }

        public string TempGuid { get; set; }
        public string UploadGuid { get; set; }

        public IEnumerable<BO.o15AutoComplete> lisAutocomplete { get; set; }

        public List<o32Repeater> lisO32 { get; set; }
        public List<p30Repeater> lisP30 { get; set; }

        public bool CanEditRecordCode { get; set; }

        public RoleAssignViewModel roles { get; set; }
        public ReminderViewModel reminder { get; set; }
        
        public DispoziceViewModel disp { get; set; }

        public Notepad.EditorViewModel Notepad { get; set; }
        public HlidacViewModel hlidac { get; set; }
    }




    public class o32Repeater : BO.o32Contact_Medium
    {
        public bool IsTempDeleted { get; set; }
        public string TempGuid { get; set; }
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

    public class p30Repeater : BO.p30ContactPerson
    {
        public bool IsTempDeleted { get; set; }
        public string TempGuid { get; set; }
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

        public string ComboPerson { get; set; }
    }

}
