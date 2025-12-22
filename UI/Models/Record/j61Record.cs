

namespace UI.Models.Record
{
    public class j61Record:BaseRecordViewModel
    {
        public BO.j61TextTemplate Rec { get; set; }

        public string ComboOwner { get; set; }

        public Notepad.EditorViewModel Notepad { get; set; }


        public List<BO.TheGridColumn> lisGridColumns { get; set; }
    }
}
