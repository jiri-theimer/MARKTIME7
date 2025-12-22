using BL;
using UI.Models;

namespace UI.Views.Shared.Components.myGrid
{
    public class myGridViewModel
    {        
        public myGridInput GridInput { get; set; }
        public BO.TheGridState GridState { get; set; }

        public IEnumerable<BO.TheGridColumn> Columns { get; set; }

        public List<BO.TheGridColumnFilter> AdhocFilter { get; set; }


        public myGridOutput firstdata { get; set; }

        public string GridMessage { get; set; }     //zpráva dole za navigací pageru

        

       
        public int GetTrueColumnWidth(BO.TheGridColumn col,List<string> arrcolwidths,int j02GridCssClassFlag)
        {
            if (j02GridCssClassFlag==2 && arrcolwidths.Count() > 0)
            {
                var qry = arrcolwidths.Where(p => p.Contains(col.UniqueName));
                if (qry.Count() > 0)
                {
                    return Convert.ToInt32(qry.First().Substring(qry.First().Length - 3, 3));
                }
            }
            if (col.FixedWidth > 0) return col.FixedWidth;
            
            if (j02GridCssClassFlag > 0) return 200;
            
            return 0;   //auto
        }
    }
}
