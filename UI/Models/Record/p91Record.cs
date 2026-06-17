

namespace UI.Models.Record
{
    public class p91Record: BaseRecordViewModel
    {
        public BO.p91Invoice Rec { get; set; }
        public RoleAssignViewModel roles { get; set; }
        public BO.p92InvoiceType RecP92 { get; set; }
        public string ComboP28Name { get; set; }
        public string ComboOwner { get; set; }
        public string ComboP80Name { get; set; }
        public string ComboP63Name { get; set; }
        public string ComboJ19Name { get; set; }
        public string ComboP98Name { get; set; }
        public string ComboJ17Name { get; set; }
        public string ComboP92Name { get; set; }
        

        public FreeFieldsViewModel ff1 { get; set; }

        public bool CanEditRecordCode { get; set; }

        public bool Isp91LockFlag2 { get; set; }
        public bool Isp91LockFlag4 { get; set; }
        public bool Isp91LockFlag8 { get; set; }

        public DispoziceViewModel disp { get; set; }

        public Notepad.EditorViewModel Notepad { get; set; }
        public string UploadGuid { get; set; }
    }
}
