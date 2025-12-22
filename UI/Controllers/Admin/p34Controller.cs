using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UI.Models;
using UI.Models.Record;

namespace UI.Controllers
{
    public class p34Controller : BaseController
    {
        public IActionResult Record(int pid, bool isclone)
        {
            var v = new p34Record() { rec_pid = pid, rec_entity = "p34" };
            v.Rec = new BO.p34ActivityGroup();
            if (v.rec_pid > 0)
            {
                v.Rec = Factory.p34ActivityGroupBL.Load(v.rec_pid);
                if (v.Rec == null)
                {
                    return RecNotFound(v);
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

        private void RefreshState(p34Record v)
        {
            
            if (v.Rec.p34ActivityEntryFlag == BO.p34ActivityEntryFlagENUM.AktivitaSeNezadava)
            {
                var lis = Factory.p32ActivityBL.GetList(new BO.myQueryP32() { p34id = v.rec_pid,explicit_orderby="p32Ordinary" });
                if (lis.Count() > 0)
                {
                    v.RecP32FirstByOrdinary = lis.First();
                }
                
            }

        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Record(p34Record v)
        {
            RefreshState(v);
            if (v.IsPostback)
            {
                return View(v);
            }
            if (ModelState.IsValid)
            {
                BO.p34ActivityGroup c = new BO.p34ActivityGroup();
                if (v.rec_pid > 0) c = Factory.p34ActivityGroupBL.Load(v.rec_pid);
                c.p33ID = v.Rec.p33ID;
                c.p34ActivityEntryFlag = v.Rec.p34ActivityEntryFlag;
                c.p34IncomeStatementFlag = v.Rec.p34IncomeStatementFlag;                
                c.p34Name = v.Rec.p34Name;
              
                c.p34Code = v.Rec.p34Code;
                c.p34Ordinary = v.Rec.p34Ordinary;
                c.p34TextInternalFlag = v.Rec.p34TextInternalFlag;


                c.p34FilesFlag = v.Rec.p34FilesFlag;
                c.p34TagsFlag = v.Rec.p34TagsFlag;
                c.p34InboxFlag = v.Rec.p34InboxFlag;
                c.p34TrimmingFlag = v.Rec.p34TrimmingFlag;

                c.ValidUntil = v.Toolbar.GetValidUntil(c);
                c.ValidFrom = v.Toolbar.GetValidFrom(c);

                c.pid = Factory.p34ActivityGroupBL.Save(c);
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
