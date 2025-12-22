
using System.Data;

using Microsoft.AspNetCore.Mvc;
using UI.Models;

namespace UI.Controllers
{
    public class CommonController : BaseController
    {
        private readonly BL.TheColumnsProvider _colsProvider;
       
        public CommonController(BL.TheColumnsProvider cp)
        {
            _colsProvider = cp;
        }
        public IActionResult Index()
        {
            return View();
        }
      

        public string DeleteRecord(string entity, int pid)  //Univerzální mazání záznamů
        {
            
            return this.Factory.CBL.DeleteRecord(entity, pid);
        }

        public bool UpdateRowColor(string prefix,string pids,int colorindex)
        {
            return Factory.CBL.UpdateRowColor(prefix, BO.Code.Bas.ConvertString2ListInt(pids), colorindex);
        }
        public bool ClearRowColor(string prefix, string pids)
        {
            return Factory.CBL.ClearRowColor(prefix, BO.Code.Bas.ConvertString2ListInt(pids));
        }

        public string SetPids2Temp(List<int> arr)
        {
            string guid = BO.Code.Bas.GetGuid();
            foreach(int pid in arr)
            {
                var c = new BO.p85Tempbox() { p85GUID = guid, p85DataPID = pid };
                Factory.p85TempboxBL.Save(c);
            }
            return guid;
        }
        
        public BO.Result SetUserParam(string key, string value)
        {
            if (Factory.CBL.SetUserParam(key, value))
            {
                return new BO.Result(false);
            }
            else
            {
                return new BO.Result(true);
            }
                       
        }
        public BO.Result SetUserParams(List<String> keys, List<string> values)
        {
            for(int i = 0; i < keys.Count; i++)
            {
                if (String.IsNullOrEmpty(keys[i]) == false)
                {
                    Factory.CBL.SetUserParam(keys[i], values[i]);
                }
            }
            return new BO.Result(false);

        }
        public string LoadUserParam(string key,string def=null)
        {
            return Factory.CBL.LoadUserParam(key,def);
        }


       
        public string GetWorkTable(string entity, string tableid, string master_entity,int master_pid, string pids,string delete_function,string edit_function, bool selectable,string rez)
        {
            var mq = new BO.InitMyQuery(Factory.CurrentUser).Load(entity, master_entity, master_pid);
            mq.SetPids(pids);

            var grid = Factory.j72TheGridTemplateBL.LoadState(entity, Factory.CurrentUser.pid,master_entity,rez);
            
            if (grid == null)
            {
                mq.explicit_columns = _colsProvider.getDefaultPallete(false, mq,Factory);
            }
            else
            {
                mq.explicit_columns = _colsProvider.ParseTheGridColumns(mq.Prefix, grid.j72Columns, Factory);

                
                
                mq.explicit_orderby = grid.j75SortDataField;
                if (grid.j75SortDataField !=null && grid.j75SortOrder != null)
                {
                    mq.explicit_orderby += " " + grid.j75SortOrder;
                }

            }
           
            
            var dt = Factory.gridBL.GetGridTable(mq);
            var intRows = dt.Rows.Count;

            var sb = new System.Text.StringBuilder();
            
            sb.Append(string.Format("<table id='{0}' class='table table-sm table-hover'>", tableid));
            sb.Append("<thead><tr class='bg-light'>");
            if (edit_function != null)
            {
                sb.Append(("<th></th>"));
            }
            foreach (var c in mq.explicit_columns)
            {
                switch (Factory.CurrentUser.j02LangIndex)
                {
                    case 1:
                        c.Header = c.TranslateLang1;break;
                    case 2:
                        c.Header = c.TranslateLang2;break;
                }
                if (c.NormalizedTypeName == "num")
                {
                    sb.Append(string.Format("<th style='text-align:right;'>{0}</th>", c.Header));
                }
                else
                {
                    sb.Append(string.Format("<th>{0}</th>", c.Header));
                }
                    
            }
            if (delete_function != null)
            {
                sb.Append(("<th></th>"));
            }
            sb.Append("</tr></thead>");
            sb.Append("<tbody>");
            string strClass = "";string strUpravit = Factory.tra("Upravit");
            for (int i = 0; i < intRows; i++)
            {
                strClass = null;
                if (selectable)
                {
                    strClass = "can-select";
                }                               
                if ((bool)dt.Rows[i]["isclosed"] == true)
                {
                    strClass += " isclosed";
                }
                if (strClass == null)
                {
                    sb.Append(string.Format("<tr data-v='{0}'>", dt.Rows[i]["pid"]));
                }
                else
                {
                    sb.Append(string.Format("<tr class='{0}' data-v='{1}'>", strClass.Trim(), dt.Rows[i]["pid"]));
                }
                

                if (edit_function != null)
                {
                    sb.Append(string.Format("<td><button type='button' class='btn btn-sm btn-light' onclick='{0}({1})'>"+strUpravit+"</button></td>", edit_function, dt.Rows[i]["pid"]));
                }
                foreach (var col in mq.explicit_columns)
                {
                    if (col.NormalizedTypeName == "num")
                    {
                        sb.Append(string.Format("<td style='text-align: right;'>{0}</td>", BO.Code.Bas.ParseCellValueFromDb(dt.Rows[i], col)));
                    }
                    else
                    {
                        sb.Append(string.Format("<td>{0}</td>", BO.Code.Bas.ParseCellValueFromDb(dt.Rows[i], col)));
                    }
                    
                }
                if (delete_function != null)
                {
                    sb.Append(string.Format("<td><button type='button' class='btn btn-sm btn-danger' title='Odstranit řádek' onclick='{0}({1})'>&times;</button></td>", delete_function, dt.Rows[i]["pid"]));
                }
                sb.Append("</tr>");
            }
            sb.Append("</tbody>");
            sb.Append("</table>");

            return sb.ToString();
        }


        public int SaveMessage2Temp(string s)
        {
            var rec = new BO.p85Tempbox() { p85GUID = BO.Code.Bas.GetGuid(), p85Message = s };
            
            return Factory.p85TempboxBL.Save(rec);
            

        }

        public BO.p85Tempbox Save2Temp(int p85id,string guid,string prefix,string fieldname,string fieldvalue)
        {            
            if (string.IsNullOrEmpty(guid))
            {
                guid = BO.Code.Bas.GetGuid();
            }
            var rec = new BO.p85Tempbox() { p85GUID = guid, p85Prefix = prefix };

            if (p85id > 0)
            {
                rec = Factory.p85TempboxBL.Load(p85id);
            }
          
            
            if (fieldname.ToLower().Contains("freenumber")){
                if (fieldvalue == "0" || fieldvalue == "") fieldvalue = null;
            }

            BO.Code.Reflexe.SetPropertyValue(rec, fieldname, fieldvalue);

            p85id = Factory.p85TempboxBL.Save(rec);
         
            
            return Factory.p85TempboxBL.Load(p85id);

        }

        public List<int> GetPidsOfQuery(string entity, string master_entity,int master_pid)
        {
            
            var mq = new BO.InitMyQuery(Factory.CurrentUser).Load(entity,master_entity,master_pid);
            
            mq.IsRecordValid = true;
            //mq.InhaleMasterEntityQuery(master_prefix, master_pid, null);

            var ret = new List<int>();
            var dt=Factory.gridBL.GetGridTable(mq);
            foreach(DataRow dbRow in dt.Rows)
            {
                ret.Add(Convert.ToInt32(dbRow["pid"]));
            }

            return ret;
        }

        public BO.Result SaveResizedColumn(int j72id,int j02id,string colid,string colwidth)
        {            
            int w = BO.Code.Bas.InInt(colwidth);
            if (w <= 0) return new BO.Result(true);

            colwidth = $"    {w}";
            
            if (colid.StartsWith("th_"))
            {
                colid = colid.Substring(3, colid.Length - 3);
            }

            var gridstate = Factory.j72TheGridTemplateBL.LoadState(j72id, j02id);
            var arr = BO.Code.Bas.ConvertString2List(gridstate.j75ColumnsGridWidth).Where(p => !p.Contains(colid)).ToList();

            arr.Add($"{colid}:{BO.Code.Bas.RightString(colwidth, 3)}");

            gridstate.j75ColumnsGridWidth = string.Join(",", arr);
            Factory.j72TheGridTemplateBL.SaveState(gridstate, j02id);
            return new BO.Result(false);
        }

    }

   


    
    
}