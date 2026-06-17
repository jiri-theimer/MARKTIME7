using Microsoft.AspNetCore.Mvc;
using UI.Models.Record;
using UI.Models;
using UI.Models.Admin;

namespace UI.Controllers.Admin
{
    public class SpaceUsedController : BaseController
    {
        public IActionResult Index()
        {
            var v = new SpaceUsedViewModel() { IsTruncateTemps = true, MonthBeforeP31Log=3,MonthBeforeJ92=0,MonthBeforeOutbox=6,MonthBeforeUpdateLogs=3 };

            
            RefreshState(v);

            return ViewTup(v, BO.PermValEnum.GR_Admin);
        }

        private void RefreshState(SpaceUsedViewModel v)
        {
            v.lisTabsUsedSpace = Factory.FBL.GetTabsUsedSpace();
            v.DbUsedSpace = Factory.FBL.GetDbUsedSpace();

            v.rows_j92 = FindRows("j92PingLog", v) + FindRows("j90LoginAccessLog", v)+FindRows("j91RobotLog",v);
            if (v.rows_j92 > 0)
            {
                v.size_j92 = FindSize("j92PingLog", v) + FindSize("j90LoginAccessLog", v)+FindSize("j91RobotLog",v);
            }
            
            

            v.rows_temp = FindRows("p31Worksheet_Temp", v) + FindRows("p85TempBox", v);
            if (v.rows_temp > 0)
            {
                v.size_temp = FindSize("p31Worksheet_Temp", v) + FindSize("p85TempBox", v);
            }
            
            

            v.size_p31 = FindSize("p31Worksheet", v);
            v.rows_p31 = FindRows("p31Worksheet", v);
            v.rows_p31log = FindRows("p31Worksheet_Log", v);
            if (v.rows_p31log > 0)
            {
                v.size_p31log = FindSize("p31Worksheet_Log", v);
            }

            v.rows_p31del = FindRows("p31Worksheet_Del", v);
            if (v.rows_p31del > 0)
            {
                v.size_p31del = FindSize("p31Worksheet_Del", v);
            }


            v.rows_updatelog = FindRows("p91Invoice_Log", v) + FindRows("p28Contact_Log", v) + FindRows("p41Project_Log", v) + FindRows("p56Task_Log", v) + FindRows("o23Doc_Log", v) + FindRows("o43Inbox_Log", v);
            if (v.rows_updatelog > 0)
            {
                v.size_updatelog = FindSize("p91Invoice_Log", v) + FindSize("p28Contact_Log", v) + FindSize("p41Project_Log", v) + FindSize("p56Task_Log", v) + FindSize("o23Doc_Log", v) + FindSize("o43Inbox_Log", v);
            }
            
            

            v.rows_outbox = FindRows("x40MailQueue", v);
            if (v.rows_outbox > 0)
            {
                v.size_outbox = FindSize("x40MailQueue", v) + FindSize("x43MailQueue_Recipient", v);
            }
            
            

            v.size_inbox = FindSize("o43Inbox", v);
            v.rows_inbox = FindRows("o43Inbox", v);

        }

        private int FindSize(string tabname, SpaceUsedViewModel v)
        {
            if (v.lisTabsUsedSpace.Where(p => p.tabname == tabname).Count() > 0)
            {
                return v.lisTabsUsedSpace.Where(p => p.tabname == tabname).First().realsize * 1024;
            }
            return 0;
        }
        private int FindRows(string tabname, SpaceUsedViewModel v)
        {
            if (v.lisTabsUsedSpace.Where(p => p.tabname == tabname).Count() > 0)
            {
                return v.lisTabsUsedSpace.Where(p => p.tabname == tabname).First().tabrows;
            }
            return 0;
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Index(SpaceUsedViewModel v)
        {
            RefreshState(v);

            if (v.IsPostback)
            {
                
                return View(v);
            }
            
            if (ModelState.IsValid)
            {
                if (v.IsTruncateTemps)
                {
                    Factory.FBL.ClearSpaceUsed("temp",0);
                }
                if (v.IsTruncateEmls)
                {
                    Factory.FBL.ClearSpaceUsed("eml", 0);
                }
                if (v.IsTruncateP96)
                {
                    Factory.FBL.ClearSpaceUsed("p96", 0);
                }

                if (v.IsTruncateJ92)
                {
                    Factory.FBL.ClearSpaceUsed("j92",v.MonthBeforeJ92);
                }
                if (v.IsTruncateOutbox)
                {
                    Factory.FBL.ClearSpaceUsed("outbox",v.MonthBeforeOutbox);
                }
                if (v.IsTruncateUpdateLogs)
                {
                    Factory.FBL.ClearSpaceUsed("updatelog",v.MonthBeforeUpdateLogs);
                }
                if (v.IsTruncateP31log)
                {
                    Factory.FBL.ClearSpaceUsed("p31log",v.MonthBeforeP31Log);
                }

                if (1==1)
                {

                    v.SetJavascript_CallOnLoad(0);
                    return View(v);
                }

            }


            this.Notify_RecNotSaved();
            return View(v);
        }
    }
}
