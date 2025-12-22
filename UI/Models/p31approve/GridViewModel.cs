using UI.Views.Shared.Components.myGrid;

namespace UI.Models.p31approve
{
    public class GridViewModel: p31approveMother
    {
        
        public string pidsinline { get; set; }
        public BO.p72IdENUM p72id { get; set; }

        public int approvinglevel { get; set; }

        
        public myGridInput gridinput { get; set; }
        public int j72ID { get; set; }
        public string p72Query { get; set; }


        public string SelectedTab { get; set; }
        
        public p31StateQueryViewModel p31statequery { get; set; }
       

        public string batchpids { get; set; }
        public double BatchInvoiceRate { get; set; }
        public int BatchApproveLevel { get; set; }

        
        public GridRecord Rec { get; set; }

    }
}
