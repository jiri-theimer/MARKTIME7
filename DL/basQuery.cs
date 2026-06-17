using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DL
{
    public class basQuery
    {

        
        public static DL.FinalSqlCommand GetFinalSql(string strPrimarySql,BO.baseQuery mq, BO.RunningUser ru, bool bolPrepareParam4DT = false)
        {           
            if (mq.CurrentUser == null)
            {
                mq.CurrentUser = ru;
            }
            var ret = new DL.FinalSqlCommand();
            mq.ClearQRows();

            

            var lis = mq.GetRows();
            if (lis !=null && lis.Count > 0)
            {
                ret.Parameters = new Dapper.DynamicParameters();
                if (bolPrepareParam4DT) ret.Parameters4DT = new List<DL.Param4DT>();
                foreach (var c in lis.Where(p => String.IsNullOrEmpty(p.ParName) == false))
                {
                    ret.Parameters.Add(c.ParName, c.ParValue);
                    if (bolPrepareParam4DT) ret.Parameters4DT.Add(new DL.Param4DT() { ParName = c.ParName, ParValue = c.ParValue });
                }
                foreach (var c in lis.Where(p => String.IsNullOrEmpty(p.Par2Name) == false))
                {
                    ret.Parameters.Add(c.Par2Name, c.Par2Value);
                    if (bolPrepareParam4DT) ret.Parameters4DT.Add(new DL.Param4DT() { ParName = c.Par2Name, ParValue = c.Par2Value });
                }

                ret.SqlWhere = String.Join(" ", lis.Select(p => $"{p.AndOrZleva} {p.BracketLeft}{p.StringWhere}{p.BracketRight}")).Trim();    //složit závěrčnou podmínku
                
                
            }
           

            if (!string.IsNullOrEmpty(ret.SqlWhere))
            {
                int intLastWHERE = strPrimarySql.ToUpper().LastIndexOf(" WHERE ");
                if (intLastWHERE == -1)
                {
                    strPrimarySql = $"{strPrimarySql} WHERE {ret.SqlWhere}";
                }
                else
                {
                    int intLastFROM = strPrimarySql.ToUpper().LastIndexOf(" FROM ");    //from klauzule je v dotazu vždy
                    int intLastJOIN = strPrimarySql.ToUpper().LastIndexOf(" JOIN ");
                    if (intLastJOIN > intLastFROM) intLastFROM = intLastJOIN;   //sql, kde vůbec nejsou join klauzule, ale FROM tam musí být vždy


                    if (intLastFROM > intLastWHERE)
                    {
                        strPrimarySql = $"{strPrimarySql} WHERE {ret.SqlWhere}";
                    }
                    else
                    {
                        strPrimarySql = $"{strPrimarySql} AND {ret.SqlWhere}";
                    }
                }
                
                
            }

            if (!string.IsNullOrEmpty(mq.explicit_sqlgroupby))
            {
                strPrimarySql = $"{strPrimarySql} GROUP BY {mq.explicit_sqlgroupby}";
                
            }
            
            if (!string.IsNullOrEmpty(mq.explicit_orderby))
            {
                strPrimarySql = $"{strPrimarySql} ORDER BY {mq.explicit_orderby}";
            }

            
            if (strPrimarySql.Contains("@p31date1"))
            {   //view s napevno navrženou podmínkou časového filtru podle datumu úkonu p31Date                            
                
                if (mq.global_d1 == null)
                {
                    mq.global_d1 = new DateTime(2000, 1, 1);
                }
                if (mq.global_d2 == null)
                {
                    mq.global_d2 = new DateTime(3000, 1, 1);
                }
                if (ret.Parameters4DT == null)
                {
                    strPrimarySql= strPrimarySql.Replace("@p31date1",BO.Code.Bas.GD(mq.global_d1));
                    strPrimarySql = strPrimarySql.Replace("@p31date2", BO.Code.Bas.GD(mq.global_d2));
                }
                else
                {
                    ret.Parameters4DT.Add(new DL.Param4DT() { ParName = "p31date1", ParValue = mq.global_d1, ParamType = "datetime" });
                    ret.Parameters4DT.Add(new DL.Param4DT() { ParName = "p31date2", ParValue = mq.global_d2, ParamType = "datetime" });
                }

                                    

            }
            //if (strPrimarySql.Contains("@d1") && ret.Parameters4DT.Where(p=>p.ParName=="d1").Count()==0)
            //{   //view s napevno navrženou podmínkou časového filtru podle datumu úkonu p31Date                            
                
            //    ret.Parameters4DT.Add(new DL.Param4DT() { ParName = "d1", ParValue = mq.global_d1, ParamType = "datetime" });
                

            //}
            //if (strPrimarySql.Contains("@d2") && ret.Parameters4DT.Where(p => p.ParName == "d2").Count() == 0)
            //{   //view s napevno navrženou podmínkou časového filtru podle datumu úkonu p31Date                            

            //    ret.Parameters4DT.Add(new DL.Param4DT() { ParName = "d2", ParValue = mq.global_d2, ParamType = "datetime" });


            //}


            //System.IO.File.WriteAllText("c:\\temp\\hovado"+mq.Prefix+".txt", strPrimarySql);
            ret.FinalSql = strPrimarySql;
            return ret;
        }
    }
}
