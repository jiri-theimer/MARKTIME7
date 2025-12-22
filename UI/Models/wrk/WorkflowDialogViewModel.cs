namespace UI.Models.wrk
{
    public class WorkflowDialogViewModel:BaseViewModel
    {       
        public string Caller { get; set; }
        public bool NameIsRequired { get; set; }
        public int b01ID { get; set; }
        public int b02ID { get; set; }
        public int RecordPid { get; set; }
        public string RecordEntity { get; set; }
        public DateTime? b05Date { get; set; }
        public bool IsPortalAccess { get; set; }
        public bool IsTab1 { get; set; }
        public bool IsBillingMemo { get; set; }
        public string b05Name { get; set; }
        public int SelectedB06ID { get; set; }
        public BO.b06WorkflowStep RecB06 { get; set; }
        public List<BO.b06WorkflowStep> lisB06 { get; set; }    //pro krokový mechanismus
        public List<BO.b02WorkflowStatus> lisB02 { get; set; }  //bez krokového mechanismu

        public int SelectedB02ID { get; set; }
        public BO.b01WorkflowTemplate RecB01 { get; set; }
        public BO.b02WorkflowStatus RecB02 { get; set; }

        public Notepad.EditorViewModel Notepad { get; set; }

        public string Nominee_j02IDs { get; set; }
        public string Nominee_Persons { get; set; }
        public string Nominee_j11IDs { get; set; }
        public string Nominee_j11Names { get; set; }

        public string current_user_lon { get; set; }    //aktuáln souřadnice uživatele
        public string current_cuser_lat { get; set; }   //aktuální souřadnice uživatele

        public ReminderViewModel reminder { get; set; }
    }
}
