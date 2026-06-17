using Microsoft.AspNetCore.Mvc;
using UI.Models.Record;
using UI.Models;
using System;

namespace UI.Controllers.Admin
{
    public class x54Controller : BaseController
    {
        public IActionResult Record(int pid, bool isclone)
        {
            var v = new x54Record() { rec_pid = pid, rec_entity = "x54" };
            v.Rec = new BO.x54WidgetGroup() { x54IsParamToday = true, x54IsAllowSkins=true };
            if (v.rec_pid > 0)
            {
                v.Rec = Factory.x54WidgetGroupBL.Load(v.rec_pid);
                if (v.Rec == null)
                {
                    return RecNotFound(v);
                }

                
                v.lisX57 = new List<x57Repeater>();
                foreach (var c in Factory.x54WidgetGroupBL.GetList_x57(v.rec_pid))
                {
                    v.lisX57.Add(new x57Repeater()
                    {
                        TempGuid = BO.Code.Bas.GetGuid(),
                        x55ID = c.x55ID,
                        ComboX55 = c.NamePlusCategory,
                        x57IsDefault=c.x57IsDefault,
                        x57Ordinary=c.x57Ordinary
                    });
                }

            }
            v.Toolbar = new MyToolbarViewModel(v.Rec);
            if (isclone)
            {
                v.MakeClone();
            }

            RefreshState(v);
            return ViewTup(v, BO.PermValEnum.GR_Admin);
        }

        private void RefreshState(x54Record v)
        {
            if (v.lisX57 == null)
            {
                v.lisX57 = new List<x57Repeater>();
            }
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Record(Models.Record.x54Record v,string guid)
        {
            RefreshState(v);

            if (v.IsPostback)
            {                
                if (v.PostbackOper == "add_row")
                {
                    var c = new x57Repeater() { TempGuid = BO.Code.Bas.GetGuid() };
                    v.lisX57.Add(c);

                }
                if (v.PostbackOper == "delete_row")
                {
                    v.lisX57.First(p => p.TempGuid == guid).IsTempDeleted = true;

                }
                return View(v);
            }
            if (ModelState.IsValid)
            {
                BO.x54WidgetGroup c = new BO.x54WidgetGroup();
                if (v.rec_pid > 0) c = Factory.x54WidgetGroupBL.Load(v.rec_pid);
                c.x54Name = v.Rec.x54Name;
                c.x54Ordinary = v.Rec.x54Ordinary;
                c.x54IsParamP28ID = v.Rec.x54IsParamP28ID;
                c.x54IsParamToday = v.Rec.x54IsParamToday;
                c.x54IsAllowAutoRefresh = v.Rec.x54IsAllowAutoRefresh;
                c.x54IsAllowSkins = v.Rec.x54IsAllowSkins;
                c.ValidUntil = v.Toolbar.GetValidUntil(c);
                c.ValidFrom = v.Toolbar.GetValidFrom(c);
                c.x54Code = v.Rec.x54Code;
                var lis = new List<BO.x57WidgetToGroup>();
                foreach(var cc in v.lisX57.Where(p => !p.IsTempDeleted))
                {
                    lis.Add(new BO.x57WidgetToGroup() { x55ID = cc.x55ID, x57IsDefault = cc.x57IsDefault,x57Ordinary=cc.x57Ordinary });
                }
                   
                c.pid = Factory.x54WidgetGroupBL.Save(c, lis);
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
