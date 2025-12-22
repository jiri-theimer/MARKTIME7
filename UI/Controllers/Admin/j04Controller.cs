using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UI.Models;
using UI.Models.Record;
using UI.Models.Tab1;

namespace UI.Controllers
{
    public class j04Controller : BaseController
    {
        public IActionResult Info(int pid)
        {
            var v = new BaseTab1ViewModel() { prefix = "j04", pid = pid };
            return View(v);
        }
        public IActionResult Record(int pid, bool isclone)
        {
            var v = new j04Record() { rec_pid = pid, rec_entity = "j04" };
            v.Rec = new BO.j04UserRole();

            if (v.rec_pid > 0)
            {
                v.Rec = Factory.j04UserRoleBL.Load(v.rec_pid);
                if (v.Rec == null)
                {
                    return RecNotFound(v);
                }
                v.SelectedPermNumbers = Factory.x67EntityRoleBL.GetList_BoundPerms(v.Rec.x67ID).Select(p => p.ValueInt).ToList();


                if (v.SelectedPermNumbers.Count() == 0 && v.Rec.RoleValue.Contains("1"))
                {
                    //rekonstrukce tabulky x68EntityRole_Permission
                    Factory.x67EntityRoleBL.Recovery_x68(Factory.x67EntityRoleBL.Load(v.Rec.x67ID));
                    v.SelectedPermNumbers = Factory.x67EntityRoleBL.GetList_BoundPerms(v.Rec.x67ID).Select(p => p.ValueInt).ToList();
                }

                var lisX54 = Factory.j04UserRoleBL.GetList_x54(v.rec_pid);
                v.SelectedX54IDs = lisX54.Select(p => p.pid).ToList();
            }

            RefreshState(v);


            v.Toolbar = new MyToolbarViewModel(v.Rec);
            if (isclone)
            {
                v.MakeClone();
            }

            return ViewTup(v, BO.PermValEnum.GR_Admin);
        }

       
        private void RefreshState(j04Record v)
        {
            v.lisAllPerms = BO.Code.Entity.GetAllPermissions("j04");
            v.lisX54 = Factory.x54WidgetGroupBL.GetList(new BO.myQuery("x54"));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Record(j04Record v)
        {
            RefreshState(v);
            if (v.IsPostback)
            {

                if (v.PostbackOper == "checkall")
                {
                    v.SelectedPermNumbers = v.lisAllPerms.Select(p => p.ValueInt).ToList();
                    
                }
                if (v.PostbackOper == "uncheckall")
                {
                    v.SelectedPermNumbers = new List<int>();
                    
                }

                return View(v);
            }
            
            if (ModelState.IsValid)
            {
                BO.j04UserRole c = new BO.j04UserRole();
                if (v.rec_pid > 0) c = Factory.j04UserRoleBL.Load(v.rec_pid);
                
                c.j04Name = v.Rec.j04Name;
                c.j04IsAllowLoginByGoogle = v.Rec.j04IsAllowLoginByGoogle;
                c.j04IsModule_p31 = v.Rec.j04IsModule_p31;
                c.j04IsModule_p41 = v.Rec.j04IsModule_p41;
                c.j04IsModule_p28 = v.Rec.j04IsModule_p28;
                c.j04IsModule_j02 = v.Rec.j04IsModule_j02;
                c.j04IsModule_p56 = v.Rec.j04IsModule_p56;

                c.j04IsModule_p91 = v.Rec.j04IsModule_p91;
                c.j04IsModule_p90 = v.Rec.j04IsModule_p90;
                c.j04IsModule_o23 = v.Rec.j04IsModule_o23;
                c.j04IsModule_x31 = v.Rec.j04IsModule_x31;
                c.j04IsModule_p11 = v.Rec.j04IsModule_p11;
                c.j04IsModule_o43 = v.Rec.j04IsModule_o43;
                c.j04IsModule_r01 = v.Rec.j04IsModule_r01;
                c.j04IsModule_p49 = v.Rec.j04IsModule_p49;

                c.j04IsModule_Widgets = v.Rec.j04IsModule_Widgets;
                

                c.a55ID_o23 = v.Rec.a55ID_o23;
                c.a55ID_p28 = v.Rec.a55ID_p28;
                c.a55ID_le5 = v.Rec.a55ID_le5;
                c.a55ID_le4 = v.Rec.a55ID_le4;
                c.a55ID_p91 = v.Rec.a55ID_p91;
                c.a55ID_j02 = v.Rec.a55ID_j02;

                
                c.j04FilesTab = v.Rec.j04FilesTab;
                c.j04GridColumnsExclude = v.Rec.j04GridColumnsExclude;

                c.ValidUntil = v.Toolbar.GetValidUntil(c);
                c.ValidFrom = v.Toolbar.GetValidFrom(c);

                c.pid = Factory.j04UserRoleBL.Save(c, v.SelectedPermNumbers,v.SelectedX54IDs);
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

