using UI.Views.Shared.Components.myGrid;

namespace UI.Models.batch
{
    public class BatchUpdateViewModel:BaseViewModel
    {
        public string prefix { get; set; }
        public int j72id { get; set; }
        public string pids { get; set; }
        public string pids_valid { get; set; }
        public myGridInput gridinput { get; set; }
        public string oper { get; set; }    //typ změny: p41PlanFrom/p41InvoiceDefaultText1/p41InvoiceDefaultText2/p92ID/p42ID        
        public bool IsSetValue { get; set; }
        public string DestComboText { get; set; }
        public int DestComboValue { get; set; }
        public string DestTextValue { get; set; }
        public double DestNumValue { get; set; }
        public DateTime? DestDateValue { get; set; }
        public bool DestBoolValue { get; set; }
        public BatchUpdateElement CurElement { get; set; }

        public List<BatchUpdateElement> lisElements { get; set; }

        public int SelectedX67ID { get; set; }
        public IEnumerable<BO.x67EntityRole> lisX67 { get; set; }
        public string SelectedJ02IDs { get; set; }
        public string SelectedJ11IDs { get; set; }
        public string SelectedUsers { get; set; }
        public string SelectedTeams { get; set; }
        public bool x69IsAllUsers { get; set; }
        public bool IsClearRoleAssign { get; set; }

        public bool Isp91LockFlag2 { get; set; }
        public bool Isp91LockFlag4 { get; set; }
        public bool Isp91LockFlag8 { get; set; }

        public string NewPassword { get; set; }
        public string NewPasswordVerify { get; set; }
        


        public int ErrsCount { get; set; }
        public int OksCount { get; set; }
    }

    public class BatchUpdateElement
    {
        public string Field { get; set; }
        public string Name { get; set; }
        public string DataType { get; set; }    //int, string, date
        public string Entity { get; set; }
        public bool IsNoClearValue { get; set; }
        public bool IsBreak { get; set; }
        public bool IsRequiredValue { get; set; }
    }
}
