using UI.Views.Shared.Components.myGrid;

namespace UI.Models.p31oper
{
    public class p31MoveToProjectViewModel:BaseViewModel
    {
        public string pids { get; set; }
        public string pids_valid { get; set; }
        public IEnumerable<BO.p31Worksheet> lisP31 { get; set; }
       
        public BO.p41Project RecP41 { get; set; }
        


        public myGridInput gridinput { get; set; }
        public ProjectComboViewModel ProjectCombo { get; set; }

        public bool IsChangeP32ID { get; set; }
        public int DestP32ID { get; set; }
        public string DestP32Name { get; set; }

    }
}
