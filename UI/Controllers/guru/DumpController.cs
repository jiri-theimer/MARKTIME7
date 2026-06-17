using Microsoft.AspNetCore.Components.RenderTree;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using UI.Models;
using UI.Models.Guru;

namespace UI.Controllers.guru
{
    public class DumpController : Controller
    {
        private BL.Factory _f;
        private System.Text.StringBuilder _sb;

        public DumpController(BL.Factory f)
        {
            _f = f;

        }

        public IActionResult Index()
        {

            var v = new CreateLicenseViewModel();
            v.DestX01ID = 1;

            return View(v);

        }


        private IActionResult ViewOnlyForVerified(object v)
        {
            return View(v); //pro testování

        }

        [HttpPost]
        public ActionResult Index(CreateLicenseViewModel v, string oper,bool identity)
        {
            
            _f.InhaleUserByLogin(_f.App.GuruLogin);

            if (oper == "postback")
            {
                return View(v);
            }
            

            if (ModelState.IsValid)
            {
                
                if (string.IsNullOrEmpty(v.p93Registration))
                {
                    v.Message = "Chybí definice tabulek!";
                    return View(v);
                }

                _sb = new System.Text.StringBuilder();
                var tabs = BO.Code.Bas.ConvertString2List(v.p93Registration, System.Environment.NewLine);
                if (oper == "delete")
                {                    
                    var lis = new List<string>();
                    tabs = tabs.Where(p => !p.StartsWith("---")).ToList();
                    for (int i = tabs.Count()-1; i >=0; i--)
                    {                        
                        lis.Add(tabs[i]);
                    }
                    tabs = lis;                    
                }
                for(int i = 0; i < tabs.Count(); i++)
                {
                    string strTab = tabs[i];
                    if (!string.IsNullOrEmpty(strTab))
                    {
                        Handle_One_Table(strTab, v, oper, identity);

                        _sb.AppendLine("");
                    }
                }
               

                v.p93Company = _sb.ToString();

               
                return View(v);




            }


            v.Message += "<br>Došlo k chybě.";

            return View(v);
        }


        private void Handle_One_Table(string strTable, CreateLicenseViewModel v,string oper, bool bolIdentity)
        {
            string strWhere = null;
            strTable = strTable.Trim().Replace(" where", " WHERE");
            var arr = BO.Code.Bas.ConvertString2List(strTable, " WHERE");
            if (arr.Count() > 1)
            {
                strTable = arr[0].Trim();
                strWhere = arr[1].Trim();
            }
            string strDest = null;
            if (!string.IsNullOrEmpty(v.DestDbName))
            {
                strDest = $"{v.DestDbName}.dbo.";
            }
            if (oper == "delete")
            {
                _sb.AppendLine($"DELETE FROM {strDest}{strTable};");
                return;
            }
            string strSQL = $"SELECT * FROM {strTable}";
            if (strWhere != null)
            {
                strSQL += " WHERE " + strWhere;
            }

            DataTable dt = _f.FBL.GetDataTable(strSQL);
           

            if (dt.Rows.Count == 0) return;
            
            

            _sb.AppendLine($"----tabulka {strTable}--------");

            var cols = Code.basGuru.GetColumns(dt);
            if (!bolIdentity)
            {
                var strKeyField = strTable.Substring(0, 3) + "ID";
                if (cols.Any(p=>p.Key== strKeyField))
                {
                    cols.Remove(cols.First(p => p.Key == strKeyField));
                }
            }
            if (bolIdentity)
            {
                _sb.AppendLine("");
                _sb.AppendLine($"SET IDENTITY_INSERT {strDest}{strTable} ON;");
            }

            foreach (DataRow dbRow in dt.Rows)
            {
                _sb.AppendLine("");
                _sb.AppendLine($"INSERT INTO {strDest}{strTable} (");
                _sb.AppendLine(string.Join(",", cols.Select(p => p.Key)));
                _sb.AppendLine(")");
                _sb.AppendLine("VALUES (");
                
                for (int i = 0; i < cols.Count(); i++)
                {                                        
                    string strField = cols[i].Key;
                    if (i > 0) _sb.Append(",");

                    if (!bolIdentity && strField == $"{strTable.Substring(0,3)}ID")
                    {
                        continue;
                    }
                    string strValue = Code.basGuru.GVN(dbRow[strField], strField, v.DestX01ID);

                    if (strField == "x01ID")
                    {
                        strValue = v.DestX01ID.ToString();
                    }
                    if (strField == "b01ID")
                    {
                        if (dbRow[strField] == System.DBNull.Value)
                        {
                            strValue = "NULL";
                        }
                        else
                        {
                            strValue = dbRow[strField].ToString();
                        }                        
                    }
                    if (strField == strTable.Substring(0, 3) + "UserInsert" || strField == strTable.Substring(0, 3) + "UserUpdate")
                    {
                        strValue = "'dump'";
                    }

                    _sb.Append(strValue);
                }
                _sb.AppendLine(");");

                

            }

            if (bolIdentity)
            {
                _sb.AppendLine("");
                _sb.AppendLine($"SET IDENTITY_INSERT {strDest}{strTable} OFF");
            }

        }
    }
}
