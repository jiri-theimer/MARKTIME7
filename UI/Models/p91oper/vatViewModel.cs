using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UI.Views.Shared.Components.myGrid;

namespace UI.Models.p91oper
{
    public class vatViewModel:BaseViewModel
    {
        public int p91ID { get; set; }

        public BO.x15IdEnum SelectedX15ID { get; set; }
        public double VatRate { get; set; }

        public BO.p91Invoice RecP91 { get; set; }

        public myGridInput gridinput { get; set; }

    }
}
