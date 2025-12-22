
using Dapper;

namespace DL
{
    public class FinalSqlCommand
    {
        public string FinalSql { get; set; }
        public string FinalTotalsRowSql { get; set; }
        public string SqlWhere { get; set; }

        
        public DynamicParameters Parameters { get; set; }
        public List<DL.Param4DT> Parameters4DT { get; set; }
    }
}
