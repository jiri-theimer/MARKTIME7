using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UI.Models.Tags
{
    public class TagsMultiSelect:BaseViewModel
    {
        public string Entity { get; set; }
       
        public IEnumerable<int> CheckedO51IDs { get; set; }
        public IEnumerable<int> SelectedO51IDs { get; set; }
        public IEnumerable<BO.o51Tag> ApplicableTags_Multi { get; set; }
        public IEnumerable<BO.o51Tag> ApplicableTags_Single { get; set; }

        public List<SingleSelectCombo> SingleCombos { get; set; }
        
    }

    public class SingleSelectCombo
    {
        public int o51ID { get; set; }
        public string o51Name { get; set; }
        public int o53ID { get; set; }
        public string o53Name { get; set; }
    }
}
