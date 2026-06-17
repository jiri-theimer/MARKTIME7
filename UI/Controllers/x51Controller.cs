using Microsoft.AspNetCore.Mvc;
using UI.Models.Record;
using UI.Models;
using DocumentFormat.OpenXml.Drawing;
using Microsoft.AspNetCore.Components.Web;

namespace UI.Controllers
{
    public class x51Controller : BaseController
    {
        public IActionResult Dashboard(int pid)
        {
            var v = new x51HelpPageViewModel();
            v.Rec = Factory.x51HelpCoreBL.Load(pid);
            return View(v);
        }
        public IActionResult Index(string viewurl,string pagetitle,int pid,int lastpid,string fullurl)
        {
            var v = new x51HelpPageViewModel() { viewurl = viewurl, pagetitle = pagetitle,fullurl=fullurl };
            if (pid > 0)
            {
                v.Rec = Factory.x51HelpCoreBL.Load(pid);
            }
            if (v.Rec==null && !string.IsNullOrEmpty(v.fullurl))
            {
                v.Rec = Factory.x51HelpCoreBL.LoadByViewUrl(v.fullurl);
                if (v.Rec == null)
                {
                    v.Rec = Factory.x51HelpCoreBL.LoadByNearUrl(v.fullurl);
                }
            }
            if (v.Rec==null && !string.IsNullOrEmpty(v.viewurl))
            {
                v.Rec = Factory.x51HelpCoreBL.LoadByViewUrl(v.viewurl);                
            }
            


            v.treeNodes = new List<myTreeNode>();
            v.lisX51 = Factory.x51HelpCoreBL.GetList(new BO.myQuery("x51"));
            foreach (var rec in v.lisX51)
            {
                var c = new myTreeNode()
                {
                    TreeIndex = rec.x51TreeIndex,
                    TreeLevel = rec.x51TreeLevel + 1,
                    Text = rec.x51Name,
                    TreeIndexFrom = rec.x51TreePrev,
                    TreeIndexTo = rec.x51TreeNext,
                    Pid = rec.pid,
                    ParentPid = rec.x51ParentID,
                    Prefix = "x51",                    
                    Expanded = false

                };
                if (c.TreeIndexTo > c.TreeIndexFrom)
                {
                    c.Text = $"{c.Text} ({v.lisX51.Where(p=>p.x51TreeIndex>c.TreeIndexFrom && p.x51TreeIndex<=c.TreeIndexTo).Count()})";
                }
                v.treeNodes.Add(c);

            }

            int intActivePid = lastpid;
            if (intActivePid == 0 && v.Rec != null && v.treeNodes.Any(p => p.Pid == v.Rec.pid))
            {
                intActivePid = v.Rec.pid;
            }

            if (intActivePid>0)
            {
                v.treeNodes.First(p => p.Pid == intActivePid).CssClass = "active";
                v.treeNodes.First(p => p.Pid == intActivePid).Expanded = true;
                int x = Factory.x51HelpCoreBL.Load(intActivePid).x51ParentID;
                while (x > 0)
                {
                    v.treeNodes.First(p => p.Pid == x).Expanded = true;
                    if (v.treeNodes.Any(p => p.ParentPid == x))
                    {
                        x = v.treeNodes.First(p => p.ParentPid == x).Pid;
                    }
                    else
                    {
                        x = 0;
                    }                    
                }
                
            }

            return View(v);
        }
        public IActionResult Record(int pid, bool isclone,string viewurl,int parentid)
        {
            var v = new x51Record() { rec_pid = pid, rec_entity = "x51" };
            v.Rec = new BO.x51HelpCore() { x51ViewUrl = viewurl };
            if (v.rec_pid > 0)
            {
                v.Rec = Factory.x51HelpCoreBL.Load(v.rec_pid);
                if (v.Rec == null)
                {
                    return RecNotFound(v);
                }
                if (v.Rec.x51ParentID > 0)
                {
                    v.ComboParent = Factory.x51HelpCoreBL.Load(v.Rec.x51ParentID).x51Name;
                }

            }
            if (parentid > 0)
            {
                var c = Factory.x51HelpCoreBL.Load(parentid);
                v.ComboParent = c.x51Name;
                v.Rec.x51ParentID = c.pid;
            }
            
            RefreshState(v);
            v.Notepad = new Models.Notepad.EditorViewModel() { Prefix = "x51", SelectedX04ID = Factory.Lic.x04ID_Default, HtmlContent = v.Rec.x51Html };

            v.Toolbar = new MyToolbarViewModel(v.Rec);
            if (isclone)
            {
                v.MakeClone();
            }

            return ViewTup(v, BO.PermValEnum.GR_Admin);
        }

        private void RefreshState(x51Record v)
        {
            

        }
        [HttpPost]        
        public IActionResult Record(x51Record v)
        {
            RefreshState(v);

            if (v.IsPostback)
            {
                return View(v);
            }

            if (ModelState.IsValid)
            {
                BO.x51HelpCore c = new BO.x51HelpCore();
                if (v.rec_pid > 0) c = Factory.x51HelpCoreBL.Load(v.rec_pid);
                c.x51Name = v.Rec.x51Name;
                c.x51ViewUrl = v.Rec.x51ViewUrl;
                c.x51NearUrls = v.Rec.x51NearUrls;

                c.x51Html = v.Notepad.HtmlContent;
                c.x51Ordinary = v.Rec.x51Ordinary;
                c.x51ParentID = v.Rec.x51ParentID;
                c.ValidUntil = v.Toolbar.GetValidUntil(c);
                c.ValidFrom = v.Toolbar.GetValidFrom(c);

                c.pid = Factory.x51HelpCoreBL.Save(c);
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
