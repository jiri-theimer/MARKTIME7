using System;
using System.ComponentModel.DataAnnotations;

namespace BO
{
    public class j73TheGridQuery:BaseBO
    {
        [Key]
        public int j73ID { get; set; }
        public int j72ID { get; set; }
        public bool j72IsQueryNegation { get; }
        public string j73Op { get; set; }
        public string j73BracketLeft { get; set; }
        public string j73BracketRight { get; set; }

        public string j73Column { get; set; }        
        public string j73Operator { get; set; }
        public string j73Value { get; set; }
        public string j73ValueAlias { get; set; }

        public int j73ComboValue { get; set; }
        public int j73DatePeriodFlag { get; set; }
        public DateTime? j73Date1 { get; set; }
        public DateTime? j73Date2 { get; set; }

        public double j73Num1 { get; set; }
        public double j73Num2 { get; set; }
        public int j73Ordinal { get; set; }

        public string FieldType { get; set; }
        public string FieldEntity { get; set; }
        public string FieldSqlSyntax { get; set; }
        public string MasterPrefix { get; set; }
        public int MasterPid { get; set; }
        public string MyQueryInline { get; set; }
        public string SqlWrapper { get; set; }
        public bool j73IsOfferPersonsAdd { get; set; }
        public string j02AddValue { get; set; }
        public string j11AddValue { get; set; }
        public string j02AddValueAlias { get; set; }
        public string j11AddValueAlias { get; set; }
        public bool IsTempDeleted { get; set; }
        
        public string TempGuid { get; set; }
        public string CssTempDisplay
        {
            get
            {
                if (this.IsTempDeleted == true)
                {
                    return "display:none;";
                }
                else
                {
                    return "display:table-row;";
                }
            }
        }

        public string WrapFilter(string strWhere)
        {
            if (this.SqlWrapper == null)
            {
                return strWhere;
            }
            else
            {
                
                return this.SqlWrapper.Replace("#filter#", strWhere);
                
                
            }
        }
        public string WrapFilter(string strWhere,string strj02IDs,string strJ11IDs)
        {
            if (this.SqlWrapper == null)
            {
                return strWhere;
            }
            else
            {
                if (!string.IsNullOrEmpty(strj02IDs) && !string.IsNullOrEmpty(strJ11IDs))
                {
                    strWhere += $" AND (xa.j02ID IN ({strj02IDs}) OR xa.j11ID IN ({strJ11IDs}))";
                }
                else
                {
                    if (!string.IsNullOrEmpty(strj02IDs))
                    {
                        strWhere += $" AND xa.j02ID IN ({strj02IDs})";
                    }
                    if (!string.IsNullOrEmpty(strJ11IDs))
                    {
                        strWhere += $" AND xa.j11ID IN ({strJ11IDs})";
                    }
                }
                
                return this.SqlWrapper.Replace("#filter#", strWhere);


            }
        }

    }
}
