
using Microsoft.AspNetCore.Mvc;
using UI.Models;
using UI.Models.Record;
//using UI.Models.Tab1;

namespace UI.Controllers
{
    public class x55Controller : BaseController
    {
        //public IActionResult Help(int pid, string x55code)
        //{
        //    var v = new x55Tab1() { pid = pid };
        //    if (!string.IsNullOrEmpty(x55code))
        //    {
        //        v.Rec = Factory.x55WidgetBL.LoadByCode(x55code,0);
        //    }
        //    else
        //    {
        //        v.Rec = Factory.x55WidgetBL.Load(v.pid);
        //    }
            
        //    return View(v);
        //}
        public IActionResult Record(int pid, bool isclone)
        {
            var v = new x55Record() { rec_pid = pid, rec_entity = "x55" };
            v.Rec = new BO.x55Widget();
            if (v.rec_pid > 0)
            {
                v.Rec = Factory.x55WidgetBL.Load(v.rec_pid);
                if (v.Rec == null)
                {
                    return RecNotFound(v);
                }
                v.HtmlHelp = v.Rec.x55Help;
                //var mq = new BO.myQueryJ04() { x55id = v.rec_pid, explicit_orderby = "j04Name" };
                //v.j04IDs = string.Join(",", Factory.j04UserRoleBL.GetList(mq).Select(p => p.pid));
                //v.j04Names = string.Join(",", Factory.j04UserRoleBL.GetList(mq).Select(p => p.j04Name));
            }

            v.Notepad = new Models.Notepad.EditorViewModel() { HtmlContent = v.Rec.x55Content,SelectedX04ID=v.Rec.x04ID };

            v.Toolbar = new MyToolbarViewModel(v.Rec);
            if (isclone)
            {
                v.MakeClone();
            }
            return ViewTup(v,BO.PermValEnum.GR_Admin);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Record(Models.Record.x55Record v)
        {
            if (v.IsPostback)
            {
                return View(v);
            }
            if (ModelState.IsValid)
            {
                BO.x55Widget c = new BO.x55Widget();
                if (v.rec_pid > 0) c = Factory.x55WidgetBL.Load(v.rec_pid);
                c.x55Name = v.Rec.x55Name;
                c.x55Code = v.Rec.x55Code;
                
                c.x55TableSql = v.Rec.x55TableSql;
                c.x55TableColHeaders = v.Rec.x55TableColHeaders;
                c.x55TableColTypes = v.Rec.x55TableColTypes;
                c.x55Content = v.Notepad.HtmlContent;
                c.x04ID = v.Notepad.SelectedX04ID;
                c.x55Ordinal = v.Rec.x55Ordinal;
                c.x55Image = v.Rec.x55Image;
                c.x55Description = v.Rec.x55Description;
                c.x55BoxBackColor = v.Rec.x55BoxBackColor;
                c.x55HeaderForeColor = v.Rec.x55HeaderForeColor;
                c.x55HeaderBackColor = v.Rec.x55HeaderBackColor;
                c.x55BoxMaxHeight = v.Rec.x55BoxMaxHeight;
                c.x55DataTablesLimit = v.Rec.x55DataTablesLimit;
                c.x55DataTablesButtons = v.Rec.x55DataTablesButtons;
                c.x55Help = v.HtmlHelp;
                
                c.x55ChartHeaders = v.Rec.x55ChartHeaders;
                c.x55ChartSql = v.Rec.x55ChartSql;
                c.x55ChartType = v.Rec.x55ChartType;
                c.x55ChartColors = v.Rec.x55ChartColors;                
                c.x55ChartHeight = v.Rec.x55ChartHeight;
                c.x55Category = v.Rec.x55Category;
                c.x55TableColTotals = v.Rec.x55TableColTotals;
                c.x55ReportCodes = v.Rec.x55ReportCodes;

                c.ValidUntil = v.Toolbar.GetValidUntil(c);
                c.ValidFrom = v.Toolbar.GetValidFrom(c);

                //List<int> j04ids = BO.Code.Bas.ConvertString2ListInt(v.j04IDs);

                c.pid = Factory.x55WidgetBL.Save(c);
                if (c.pid > 0)
                {

                    v.SetJavascript_CallOnLoad(c.pid);
                    return View(v);
                }

            }


            this.Notify_RecNotSaved();
            return View(v);
        }
    }
}