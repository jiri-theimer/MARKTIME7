using System;
using System.ComponentModel.DataAnnotations;

namespace BO
{
    public enum a59StructureFlagENUM
    {
        Boxes=1,
        CustomHtml=2
    }

    public class a59RecPageLayer: BaseBO
    {       
        public int a55ID { get; set; }
        public a59StructureFlagENUM a59StructureFlag { get; set; }
        public string a59Name { get; set; }
        
        public int a59ColumnsPerPage { get; set; }
        public string a59Boxes { get; set; }
        public string a59DockState { get; set; }
        public string a59CustomHtmlStructure { get; set; }
        public string a59CssClassContainer { get; set; }

    }
}
