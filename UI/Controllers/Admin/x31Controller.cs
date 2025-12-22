using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;


using UI.Models;
using UI.Models.Record;
using System.Data;

namespace UI.Controllers
{
    public class x31Controller : BaseController
    {
              
        public IActionResult Record(int pid, bool isclone)
        {
            var v = new x31Record() { rec_pid = pid, rec_entity = "x31",UploadGuid=BO.Code.Bas.GetGuid() };
            v.Rec = new BO.x31Report();
            if (v.rec_pid > 0)
            {
                v.Rec = Factory.x31ReportBL.Load(v.rec_pid);
                if (v.Rec == null)
                {
                    return RecNotFound(v);
                }
                v.ComboJ25Name = v.Rec.j25Name;

               

            }
            v.Toolbar = new MyToolbarViewModel(v.Rec);
            if (isclone)
            {
                v.MakeClone();
            }
            RefreshStateRecord(v);

            

            return ViewTup(v, BO.PermValEnum.GR_Admin);
        }

       

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Record(x31Record v)
        {
            RefreshStateRecord(v);
            if (v.IsPostback)
            {
                if (v.PostbackOper== "Clear_x31LastScheduledRun")
                {
                    Factory.x31ReportBL.Clear_x31LastScheduledRun(v.rec_pid);
                    this.AddMessageTranslated("Paměť generování této sestavy vyčištěna.", "info");
                }
                return View(v);
            }
            if (ModelState.IsValid)
            {
                BO.x31Report c = new BO.x31Report();
                if (v.rec_pid > 0) c = Factory.x31ReportBL.Load(v.rec_pid);
                c.x31FormatFlag = v.Rec.x31FormatFlag;
                c.x31Entity = v.Rec.x31Entity;
                c.x31Name = v.Rec.x31Name;
                c.x31Code = v.Rec.x31Code;
                c.x31Description = v.Rec.x31Description;
                c.x31IsPeriodRequired = v.Rec.x31IsPeriodRequired;

                c.x31ExportFileNameMask = v.Rec.x31ExportFileNameMask;
                
                c.x31DocSqlSource = v.Rec.x31DocSqlSource;
                c.x31DocSqlSourceTabs = v.Rec.x31DocSqlSourceTabs;
                c.x31LangIndex = v.Rec.x31LangIndex;

                c.j25ID = v.Rec.j25ID;
                c.x31Ordinary = v.Rec.x31Ordinary;

                c.x31IsScheduling = v.Rec.x31IsScheduling;
                c.x31IsRunInDay1 = v.Rec.x31IsRunInDay1;
                c.x31IsRunInDay2 = v.Rec.x31IsRunInDay2;
                c.x31IsRunInDay3 = v.Rec.x31IsRunInDay3;
                c.x31IsRunInDay4 = v.Rec.x31IsRunInDay4;
                c.x31IsRunInDay5 = v.Rec.x31IsRunInDay5;
                c.x31IsRunInDay6 = v.Rec.x31IsRunInDay6;
                c.x31IsRunInDay7 = v.Rec.x31IsRunInDay7;
                c.x31RunInTime = v.Rec.x31RunInTime;
                c.x31SchedulingReceivers = v.Rec.x31SchedulingReceivers;
                c.x21ID_Scheduling = v.Rec.x21ID_Scheduling;
                c.x31IsAllowPfx = v.Rec.x31IsAllowPfx;
                c.x31QueryFlag = v.Rec.x31QueryFlag;

                c.ValidUntil = v.Toolbar.GetValidUntil(c);
                c.ValidFrom = v.Toolbar.GetValidFrom(c);
               
                c.pid = Factory.x31ReportBL.Save(c, v.roles.getList4Save(Factory));
                if (c.pid > 0)
                {
                    if (Factory.o27AttachmentBL.SaveSingleUpload(v.UploadGuid, "x31", c.pid))
                    {
                        v.SetJavascript_CallOnLoad(c.pid);
                        return View(v);
                    }

                    
                }
            }
            this.Notify_RecNotSaved();
            return View(v);

        }

        private void RefreshStateRecord(x31Record v)
        {
            if (v.rec_pid > 0)
            {
                v.RecO27 = Factory.x31ReportBL.LoadReportDoc(v.rec_pid);               
            }
            v.lisPeriodSource = new BL.Singleton.ThePeriodProvider().getPallete().Where(p => p.pid > 1).ToList();

            if (v.roles == null)
            {
                v.roles = new RoleAssignViewModel() { RecPid = v.rec_pid, RecPrefix = "x31", RolePrefix = "x31", Header = "." };


            }
        }


    }
}