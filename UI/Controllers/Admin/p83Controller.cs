using Microsoft.AspNetCore.Mvc;
using UI.Models.Record;
using UI.Models;

namespace UI.Controllers.Admin
{
    public class p83Controller : BaseController
    {
        public IActionResult Record(int pid, bool isclone)
        {
            var v = new p83Record() { rec_pid = pid, rec_entity = "p83" };
            v.Rec = new BO.p83UpominkaType();
            if (v.rec_pid > 0)
            {
                v.Rec = Factory.p83UpominkaTypeBL.Load(v.rec_pid);
                if (v.Rec == null)
                {
                    return RecNotFound(v);
                }
                try
                {
                    if (v.Rec.x31ID_Index1 > 0)
                    {
                        v.ComboX31_Index1 = Factory.x31ReportBL.Load(v.Rec.x31ID_Index1).x31Name;
                    }
                    if (v.Rec.x31ID_Index2 > 0)
                    {
                        v.ComboX31_Index2 = Factory.x31ReportBL.Load(v.Rec.x31ID_Index2).x31Name;
                    }
                    if (v.Rec.x31ID_Index3 > 0)
                    {
                        v.ComboX31_Index3 = Factory.x31ReportBL.Load(v.Rec.x31ID_Index3).x31Name;
                    }
                    if (v.Rec.j61ID_Index1 > 0)
                    {
                        v.ComboJ61_Index1 = Factory.j61TextTemplateBL.Load(v.Rec.j61ID_Index1).j61Name;
                    }
                    if (v.Rec.j61ID_Index2 > 0)
                    {
                        v.ComboJ61_Index2 = Factory.j61TextTemplateBL.Load(v.Rec.j61ID_Index2).j61Name;
                    }
                    if (v.Rec.j61ID_Index3 > 0)
                    {
                        v.ComboJ61_Index3 = Factory.j61TextTemplateBL.Load(v.Rec.j61ID_Index3).j61Name;
                    }
                }catch(Exception e)
                {
                    this.AddMessageTranslated(e.Message);
                    //nic
                }
                
            }
            RefreshState(v);
            v.Toolbar = new MyToolbarViewModel(v.Rec);
            if (isclone)
            {
                v.MakeClone();
            }

            return ViewTup(v, BO.PermValEnum.GR_Admin);
        }

        private void RefreshState(p83Record v)
        {


        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Record(p83Record v)
        {
            RefreshState(v);

            if (ModelState.IsValid)
            {
                BO.p83UpominkaType c = new BO.p83UpominkaType();
                if (v.rec_pid > 0) c = Factory.p83UpominkaTypeBL.Load(v.rec_pid);
                c.p83Name = v.Rec.p83Name;
                c.x31ID_Index1 = v.Rec.x31ID_Index1;
                c.x31ID_Index2 = v.Rec.x31ID_Index2;
                c.x31ID_Index3 = v.Rec.x31ID_Index3;
                c.p83Days_Index1 = v.Rec.p83Days_Index1;
                c.p83Days_Index2 = v.Rec.p83Days_Index2;
                c.p83Days_Index3 = v.Rec.p83Days_Index3;
                c.p83TextA_Index1 = v.Rec.p83TextA_Index1;
                c.p83TextA_Index2 = v.Rec.p83TextA_Index2;
                c.p83TextA_Index3 = v.Rec.p83TextA_Index3;
                c.p83TextB_Index1 = v.Rec.p83TextB_Index1;
                c.p83TextB_Index2 = v.Rec.p83TextB_Index2;
                c.p83TextB_Index3 = v.Rec.p83TextB_Index3;
                c.p83Days_Index1 = v.Rec.p83Days_Index1;
                c.p83Days_Index2 = v.Rec.p83Days_Index2;
                c.p83Days_Index3 = v.Rec.p83Days_Index3;
                c.p83Name_Index1 = v.Rec.p83Name_Index1;
                c.p83Name_Index2 = v.Rec.p83Name_Index2;
                c.p83Name_Index3 = v.Rec.p83Name_Index3;
                c.p83Ordinary = v.Rec.p83Ordinary;
                c.j61ID_Index1 = v.Rec.j61ID_Index1;
                c.j61ID_Index2 = v.Rec.j61ID_Index2;
                c.j61ID_Index3 = v.Rec.j61ID_Index3;

                c.ValidUntil = v.Toolbar.GetValidUntil(c);
                c.ValidFrom = v.Toolbar.GetValidFrom(c);

                c.pid = Factory.p83UpominkaTypeBL.Save(c);
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
