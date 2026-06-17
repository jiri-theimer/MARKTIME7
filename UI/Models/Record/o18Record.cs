
namespace UI.Models.Record
{
    public class o18Record: BaseRecordViewModel
    {
        public BO.o18DocType Rec { get; set; }
        public string SelectedEntity { get; set; }

        public RoleAssignViewModel roles { get; set; }
        public CreateAssignViewModel creates { get; set; }
        public List<o16Repeater> lisO16 { get; set; }
        public List<o20Repeater> lisO20 { get; set; }
        public string ComboB01 { get; set; }
        public string ComboO17 { get; set; }
        public string ComboX38 { get; set; }
        public string ComboP34Name { get; set; }
        public string ComboP32Name { get; set; }

    }



    public class o16Repeater : BO.o16DocType_FieldSetting
    {        
       
        public bool IsTempDeleted { get; set; }
        public string TempGuid { get; set; }
        public string CssTempDisplay
        {
            get
            {
                if (this.IsTempDeleted == true)
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

    public class o20Repeater : BO.o20DocTypeEntity
    {
        public string ComboEntity { get; set; }
        public string ComboSelectedText { get; set; }
        public bool IsTempDeleted { get; set; }
        public string TempGuid { get; set; }
        public string CssTempDisplay
        {
            get
            {
                if (this.IsTempDeleted == true)
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
