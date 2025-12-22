using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UI.Models.Admin
{
    public class m62SettingsViewModel:BaseViewModel
    {
        public List<int> SelectedJ27IDs { get; set; }
        public int SelectedJ27ID { get; set; }
        public string SelectedJ27Code { get; set; }
        public DateTime SelectedDate { get; set; }

        public IEnumerable<BO.j27Currency> lisAllJ27 { get; set; }
    }
}
