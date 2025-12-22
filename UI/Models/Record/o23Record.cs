

namespace UI.Models.Record
{
    public class o23Record : BaseRecordViewModel
    {
        public BO.o23Doc Rec { get; set; }
        public string UploadGuid { get; set; }
        public int o18ID { get; set; }
        public bool CanEditRecordCode { get; set; }
        public DispoziceViewModel disp { get; set; }
        public Notepad.EditorViewModel Notepad { get; set; }
        public ReminderViewModel reminder { get; set; }
        public BarcodesViewModel barcodes { get; set; }
        public BO.o18DocType RecO18 { get; set; }
        public List<o19Repeator> lisO19 { get; set; }
        public IEnumerable<BO.o20DocTypeEntity> lisO20 { get; set; }
        public int SelectedO20ID { get; set; }
        public string SelectedBindName { get; set; }
        public IEnumerable<BO.o16DocType_FieldSetting> lisO16 { get; set; }

        public List<DocFieldInput> lisFields { get; set; }

        public int SelectedBindPid { get; set; }
        public string SelectedBindText { get; set; }
        public string SelectedBindEntity { get; set; }
        public string SelectedBindMyQueryInline { get; set; }

        public bool IsAutoCollapseO20 { get; set; }

        public int b07ID { get; set; }
        //public List<o27Repeator> lisO27 { get; set; }

       public int o43id_source { get; set; }
        public string SelectedComboOwner { get; set; }

        public RoleAssignViewModel roles { get; set; }

        public bool IsPasswordPassed { get; set; }
        public string PasswordNew { get; set; }
        public string PasswordVerify { get; set; }

        public string current_user_lon { get; set; }    //aktuáln souřadnice uživatele
        public string current_cuser_lat { get; set; }   //aktuální souřadnice uživatele
    }


    public class DocFieldInput : BO.o16DocType_FieldSetting
    {
        public string StringInput { get; set; }
        public double NumInput { get; set; }
        public DateTime? DateInput { get; set; }
        public bool CheckInput { get; set; }

        public bool IsVisible { get; set; } = true;
        public string CssDisplay
        {
            get
            {
                if (this.IsVisible)
                {
                    return "inline-flex;";
                }
                else
                {
                    return "display:none;";
                }
            }
        }
    }

    public class o19Repeator : BO.o19DocTypeEntity_Binding
    {
        public string SelectedBindText { get; set; }
        public string SelectedO20Name { get; set; }
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


}
