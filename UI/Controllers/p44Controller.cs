using Microsoft.AspNetCore.Mvc;
using UI.Models.Record;
using UI.Models;

namespace UI.Controllers
{
    public class p44Controller : BaseController
    {
        public IActionResult Record(int pid, bool isclone)
        {
            var v = new p44Record() { rec_pid = pid, rec_entity = "p44", ProjectCombo = new ProjectComboViewModel() };
            v.Rec = new BO.p44ProjectTemplate() {p44IsBilling = true,p44IsRoles=true, p44IsP40 = true, p44IsP56 = true,p44FixingFlag=BO.p44FixingFlagEnum.FixAll };

            
            if (v.rec_pid > 0)
            {
                v.Rec = Factory.p44ProjectTemplateBL.Load(v.rec_pid);
                if (v.Rec == null)
                {
                    return RecNotFound(v);
                }
                v.ProjectCombo.SelectedP41ID = v.Rec.p41ID_Pattern;
                v.ProjectCombo.SelectedProject = Factory.p41ProjectBL.Load(v.Rec.p41ID_Pattern).FullName;
            }
            v.Toolbar = new MyToolbarViewModel(v.Rec);
            if (isclone)
            {
                v.MakeClone();
            }
            return ViewTup(v, BO.PermValEnum.GR_p41_Creator);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Record(Models.Record.p44Record v)
        {
            if (v.IsPostback)
            {
                return View(v);
            }
            if (ModelState.IsValid)
            {
                BO.p44ProjectTemplate c = new BO.p44ProjectTemplate();
                if (v.rec_pid > 0) c = Factory.p44ProjectTemplateBL.Load(v.rec_pid);
                c.p44Name = v.Rec.p44Name;
                c.p44Ordinary = v.Rec.p44Ordinary;
                c.p41ID_Pattern = v.ProjectCombo.SelectedP41ID;
                c.p44FixingFlag = v.Rec.p44FixingFlag;
                c.p44IsB20 = v.Rec.p44IsB20;
                c.p44IsJ18ID = v.Rec.p44IsJ18ID;
                c.p44IsTags = v.Rec.p44IsTags;
                c.p44IsO24 = v.Rec.p44IsO24;
                c.p44IsP56 = v.Rec.p44IsP56;
                c.p44IsP40 = v.Rec.p44IsP40;
                c.p44IsBilling = v.Rec.p44IsBilling;
                c.p44IsClient = v.Rec.p44IsClient;
                c.p44IsO22 = v.Rec.p44IsO22;
                c.p44IsRoles = v.Rec.p44IsRoles;

                c.ValidUntil = v.Toolbar.GetValidUntil(c);
                c.ValidFrom = v.Toolbar.GetValidFrom(c);

                c.pid = Factory.p44ProjectTemplateBL.Save(c);
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
