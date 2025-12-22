using UI.Views.Shared.Components.myGrid;

namespace UI.Models.Record
{
    public class p24Record : BaseRecordViewModel
    {
        public BO.p24ContactGroup Rec { get; set; }
        public string p28IDs { get; set; }

        public int SelectedP28ID { get; set; }
        public string SelectedContact { get; set; }
        public int SelectedP29ID { get; set; }
        public string SelectedP29Name { get; set; }

        public myGridInput gridinput { get; set; }

        public int SelectedX07ID { get; set; }
        public string OperIntegrace { get; set; }
        public List<BO.StringPair> lisMailListIntegrace { get; set; }
        public string SelectedMailListID { get; set; }
        public bool IsMailList_IncludeP30 { get; set; }
        public BO.x07Integration RecX07 { get; set; }
        public IEnumerable<BO.x07Integration> lisX07 { get; set; }

        public int MailList_Count_OKs { get; set; }
        public int MailList_Count_Errs { get; set; }


    }
}
