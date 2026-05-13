
using Microsoft.AspNetCore.Mvc;

using UI.Models;
using UI.Models.Record;

namespace UI.Controllers
{
    public class c21Controller : BaseController
    {
        public IActionResult Record(int pid, bool isclone)
        {
            var v = new c21Record() { rec_pid = pid, rec_entity = "c21" };
            v.Rec = new BO.c21FondCalendar();
            if (v.rec_pid > 0)
            {
                v.Rec = Factory.c21FondCalendarBL.Load(v.rec_pid);
                if (v.Rec == null)
                {
                    return RecNotFound(v);
                }

                var lis = Factory.c21FondCalendarBL.GetList_c28(v.rec_pid).ToList();
                v.lisC28 = new List<c28Repeater>();
                foreach (var c in lis)
                {

                    v.lisC28.Add(new c28Repeater()
                    {
                        TempGuid = BO.Code.Bas.GetGuid(),
                        c21ID = c.c21ID_Log,
                        c28ValidFrom = c.ValidFrom,
                        c28ValidUntil = c.ValidUntil,
                        ComboC21 = Factory.c21FondCalendarBL.Load(c.c21ID_Log).c21Name

                    });
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

        private void RefreshState(c21Record v)
        {
            if (v.lisC28 == null)
            {
                v.lisC28 = new List<c28Repeater>();
            }
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Record(c21Record v, string guid)
        {
            RefreshState(v);

            if (v.IsPostback)
            {
                if (v.PostbackOper == "add_row")
                {
                    var c = new c28Repeater() { TempGuid = BO.Code.Bas.GetGuid() };
                    v.lisC28.Add(c);

                }

                if (v.PostbackOper == "delete_row")
                {
                    v.lisC28.First(p => p.TempGuid == guid).IsTempDeleted = true;

                }

                return View(v);
            }
            

            if (ModelState.IsValid)
            {
                BO.c21FondCalendar c = new BO.c21FondCalendar();
                if (v.rec_pid > 0) c = Factory.c21FondCalendarBL.Load(v.rec_pid);
                c.c21Name = v.Rec.c21Name;
                c.c21ScopeFlag = v.Rec.c21ScopeFlag;
                c.c21Ordinary = v.Rec.c21Ordinary;
                c.c21Day1_Hours = v.Rec.c21Day1_Hours;
                c.c21Day2_Hours = v.Rec.c21Day2_Hours;
                c.c21Day3_Hours = v.Rec.c21Day3_Hours;
                c.c21Day4_Hours = v.Rec.c21Day4_Hours;
                c.c21Day5_Hours = v.Rec.c21Day5_Hours;
                c.c21Day6_Hours = v.Rec.c21Day6_Hours;
                c.c21Day7_Hours = v.Rec.c21Day7_Hours;

                c.ValidUntil = v.Toolbar.GetValidUntil(c);
                c.ValidFrom = v.Toolbar.GetValidFrom(c);

               
                var lis = new List<BO.c28FondCalendar_Log>();
                foreach (var row in v.lisC28.Where(p => p.IsTempDeleted == false))
                {
                    var cc = new BO.c28FondCalendar_Log() { c21ID_Log = row.c21ID,ValidFrom = row.c28ValidFrom, ValidUntil = row.c28ValidUntil };
                    lis.Add(cc);
                }


                c.pid = Factory.c21FondCalendarBL.Save(c, lis);
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
