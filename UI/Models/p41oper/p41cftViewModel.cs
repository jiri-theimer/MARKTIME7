using BO;

namespace UI.Models.p41oper
{
    public class p56cft : BO.p56Task
    {
        public bool IsChecked { get; set; } = true;
        public string Assign_j02IDs { get; set; }
        public string Assign_Persons { get; set; }
        public string Assign_j11IDs { get; set; }
        public string Assign_j11Names { get; set; }
    }
    public class p40cft : BO.p40WorkSheet_Recurrence
    {
        public bool IsChecked { get; set; } = true;
    }
    public class o22cft : BO.o22Milestone
    {
        public bool IsChecked { get; set; } = true;
    }
    public class p41cftViewModel:BaseViewModel
    {
        public int p44ID { get; set; }
        public int p41ID { get; set; }
        public BO.p41Project RecP41 { get; set; }
        public string TempGuid { get; set; }
        public BO.p44ProjectTemplate RecP44 { get; set; }
        public RoleAssignViewModel roles { get; set; }
        public ReminderViewModel reminder { get; set; }
        public FreeFieldsViewModel ff1 { get; set; }
        public HlidacViewModel hlidac { get; set; }
        public string p41Name { get; set; }
        public string p41NameShort { get; set; }
        public int p28ID_Client { get; set; }
        public string Klient { get; set; }
        public int j18ID { get; set; }
        public string Stredisko { get; set; }
        public string TagPids { get; set; }
        public string TagNames { get; set; }
        public string TagHtml { get; set; }


        public p41BillingFlagEnum p41BillingFlag { get; set; }
        public int SelectedP51ID_Flag3 { get; set; }
        public int SelectedP51ID_Flag2 { get; set; }
        public string p41InvoiceDefaultText2 { get; set; }
        public string p41InvoiceDefaultText1 { get; set; }
        public string SelectedComboP51Name { get; set; }
        
        public List<p56cft> lisP56 { get; set; }
        public List<p40cft> lisP40 { get; set; }
        public List<o22cft> lisO22 { get; set; }
    }
    
}
