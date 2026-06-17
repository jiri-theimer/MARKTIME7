

namespace DL
{
    public class QueryRow
    {
        public string StringWhere { get; set; }
        public string ParName { get; set; }
        public object ParValue { get; set; }

        public string AndOrZleva { get; set; } = "AND";

        public string BracketLeft { get; set; }
        public string BracketRight { get; set; }

        public string Par2Name { get; set; }
        public object Par2Value { get; set; }
    }

   
}
