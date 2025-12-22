using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace BO.Code
{
    public static class MergeContent
    {
        public static List<string> GetAllMergeFieldsInContent(string strContent)
        {

            //vrátí seznam slučovacích polí, které se vyskytují v strContent
            var lis = new List<string>();
            if (string.IsNullOrEmpty(strContent)) return lis;

            System.Text.RegularExpressions.MatchCollection matches = System.Text.RegularExpressions.Regex.Matches(strContent, @"\[%.*?\%]");
            foreach (System.Text.RegularExpressions.Match m in matches)
            {
                string strField = m.Value.Replace("[%", "").Replace("%]", "");
                lis.Add(strField.ToLower());
            }

            return lis;

        }
        public static string GetMergedContent(string strContent,DataTable dt)
        {
            var fields = GetAllMergeFieldsInContent(strContent);

            foreach (DataRow dr in dt.Rows)
            {
                string strVal = "";
                foreach (DataColumn col in dt.Columns)
                {
                    if (fields.Contains(col.ColumnName.ToLower()))
                    {
                        if (dr[col] == null)
                        {
                            strVal = "";
                        }
                        else
                        {
                            switch (col.DataType.Name.ToString())
                            {
                                case "DateTime":
                                    strVal = BO.Code.Bas.ObjectDate2String(dr[col],"dd.MM.yyyy");
                                    break;
                                case "Decimal":
                                case "Double":
                                    strVal = BO.Code.Bas.Number2String(Convert.ToDouble(dr[col]));
                                    break;
                                default:
                                    strVal = dr[col].ToString();
                                    break;
                            }

                        }
                        strContent = strContent.Replace("[%" + col.ColumnName + "%]", strVal, StringComparison.OrdinalIgnoreCase);

                    }

                }

            }
            return strContent;
        }


        public static List<BO.MergeExternalSql> GetAllMergeExternalSqlsInContent(string strContent, List<BO.StringPair> vars)
        {
            //vrátí seznam sql výrazů, které se mají volat vůči externímu datovému zdroji uvnitř závorek {}
            //ve vars jsou proměnné uvedené v SQL, @a03redizo, @j03login, @a01id,@a03id + jejich hodnoty
            
            var lis = new List<BO.MergeExternalSql>();
            if (string.IsNullOrEmpty(strContent)) return lis;

            System.Text.RegularExpressions.MatchCollection matches = System.Text.RegularExpressions.Regex.Matches(strContent, @"\{.*?\}");
            foreach (System.Text.RegularExpressions.Match m in matches)
            {
                var c = new BO.MergeExternalSql() { OrigExpr = m.Value };  //do Key se uloží hledaný výraz vč. {}
                string strExpr = m.Value.Replace("{", "").Replace("}", "");
                
                c.OrigSql = strExpr;
                c.MergedSql = strExpr;


                if (vars != null && vars.Count > 0)
                {
                    foreach (BO.StringPair v in vars)
                    {
                        if (c.MergedSql.Contains(v.Key, StringComparison.OrdinalIgnoreCase))
                        {
                            c.MergedSql = c.MergedSql.Replace(v.Key, v.Value, StringComparison.OrdinalIgnoreCase);
                        }
                    }
                }



                lis.Add(c);
            }

            return lis;




        }
    }
}
