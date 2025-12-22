using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UI.Views.Shared.Components.myGrid;

namespace UI.Models.Tags
{
    public class TagsBatch:BaseViewModel
    {
        public int j72ID { get; set; }
        public string Record_Entity { get; set; }
        public string Record_Pids { get; set; }

        public int SelectedO53ID { get; set; }

        public BO.o53TagGroup RecO53 { get; set; }
        public IEnumerable<BO.o53TagGroup> lisO53 { get; set; }

        public IEnumerable<int> SelectedO51IDs { get; set; }
        public int SelectedRadioO51ID { get; set; }
        public IEnumerable<BO.o51Tag> ApplicableTags { get; set; }

        public myGridInput gridinput { get; set; }
    }
}
