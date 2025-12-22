using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UI.Views.Shared.Components.myGrid;

namespace UI.Models.p91oper
{
    public class p31operbatchViewModel:BaseViewModel
    {
        public string p31ids { get; set; }
        public IEnumerable<BO.p31Worksheet> lisP31 { get; set; }
        public string BaseOper { get; set; }
        public int SelectedOper { get; set; }
        public BO.p91Invoice RecP91 { get; set; }
        public int p91ID { get; set; }
        public myGridInput gridinput { get; set; }
    }
}
