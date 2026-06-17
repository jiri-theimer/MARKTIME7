using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UI.Models;

namespace UI.Models.a55
{
    public class AdminOneWebpageViewModel: BaseViewModel
    {
        public int SelectedA55ID { get; set; }
        public BO.a55RecPage RecA55 { get; set; }

        public IEnumerable<BO.a55RecPage> lisA55 { get; set; }

        public int SelectedA59ID { get; set; }
        public BO.a59RecPageLayer RecA59 { get; set; }
        public IEnumerable<BO.a59RecPageLayer> lisA59 { get; set; }


        public string BoxColCss { get; set; } = "col-lg-6";
        public WebpageLayerEnvironment DockStructure { get; set; }
        public int ColumnsPerPage { get; set; }

        public List<BO.a58RecPageBox> lisUserWidgets { get; set; }
        public IEnumerable<BO.a58RecPageBox> lisAllWidgets { get; set; }

        public string SearchBox { get; set; }

        
        public int Simulation_RecPid { get; set; }
        public string Simulation_RecName { get; set; }
    }
}
