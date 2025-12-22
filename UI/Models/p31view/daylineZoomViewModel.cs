using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UI.Views.Shared.Components.myGrid;

namespace UI.Models.p31view
{
    public class daylineZoomViewModel:BaseViewModel
    {
        public DateTime SelectedDate1 { get; set; }
        public DateTime SelectedDate2 { get; set; }        
        public int j02ID { get; set; }
        public int p28ID { get; set; }
        public int p41ID { get; set; }
        public int p32ID { get; set; }
        public bool? p32IsBillable { get; set; }
        public int p70ID { get; set; }
        public bool? IsWip { get; set; }
        public bool? IsApproved_And_Wait4Invoice { get; set; }
        public BO.j02User RecJ02 { get; set; }
        public myGridInput gridinput { get; set; }
    }
}
