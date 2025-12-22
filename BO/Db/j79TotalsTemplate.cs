

namespace BO
{
    public class j79TotalsTemplate:BaseBO
    {
        public string j79Entity { get; set; }
        public string j79Name { get; set; }
        public int j79Ordinary { get; set; }
        public int j02ID { get; set; }
        public bool j79IsSystem { get; set; }
        public bool j79IsPublic { get; set; }
        public string j79Columns { get; set; }
        public string j79GroupField1 { get; set; }
        public string j79GroupField2 { get; set; }
        public string j79GroupField3 { get; set; }
        public string j79PivotField { get; set; }
        public string j79PivotValue { get; set; }
        
        public int j79StateQuery { get; set; }
        public string j79TabQuery { get; set; }
        public string j79Query_j02IDs { get; set; }
        public string j79Query_j11IDs { get; set; }
        public string j79Query_j07IDs { get; set; }
        public string j79AddQuery { get; set; }
    }
}
