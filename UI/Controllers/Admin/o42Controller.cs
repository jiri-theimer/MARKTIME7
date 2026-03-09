using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using UI.Models;
using UI.Models.Record;

namespace UI.Controllers
{
    public class o42Controller : BaseController
    {
        public IActionResult Record(int pid, bool isclone)
        {
            var v = new o42Record() { rec_pid = pid, rec_entity = "o42" };
            v.Rec = new BO.o42ImapRule();
            if (v.rec_pid > 0)
            {
                v.Rec = Factory.o42ImapRuleBL.Load(v.rec_pid);
                if (v.Rec == null)
                {
                    return RecNotFound(v);
                }
               
                if (v.Rec.j02ID_Default > 0)
                {
                    v.ComboJ02 = Factory.j02UserBL.Load(v.Rec.j02ID_Default).FullNameAsc;
                }
                if (v.Rec.p41ID_Default > 0)
                {
                    v.ComboP41 = Factory.p41ProjectBL.Load(v.Rec.p41ID_Default).FullName;
                }
                if (v.Rec.p28ID_Default > 0)
                {
                    v.ComboP28 = Factory.p28ContactBL.Load(v.Rec.p28ID_Default).p28Name;
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

        private void RefreshState(o42Record v)
        {
            v.lisJ40 = Factory.j40MailAccountBL.GetList(new BO.myQueryJ40() { isimap=true });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Record(o42Record v)
        {
            RefreshState(v);
            if (ModelState.IsValid)
            {
                BO.o42ImapRule c = new BO.o42ImapRule();
                if (v.rec_pid > 0) c = Factory.o42ImapRuleBL.Load(v.rec_pid);
                c.j40ID = v.Rec.j40ID;
                c.o42Name = v.Rec.o42Name;
                c.o42WhatToDoFlag = v.Rec.o42WhatToDoFlag;
                c.j02ID_Default = v.Rec.j02ID_Default;
                c.p41ID_Default = v.Rec.p41ID_Default;
                c.p28ID_Default = v.Rec.p28ID_Default;
                c.o42Description = v.Rec.o42Description;
                c.o42Condition_Sender = v.Rec.o42Condition_Sender;
                c.o42Condition_Subject = v.Rec.o42Condition_Subject;
                c.o42Condition_Cc = v.Rec.o42Condition_Cc;
                c.o42Condition_Bcc = v.Rec.o42Condition_Bcc;                
                
                
                c.ValidUntil = v.Toolbar.GetValidUntil(c);
                c.ValidFrom = v.Toolbar.GetValidFrom(c);

                c.pid= Factory.o42ImapRuleBL.Save(c);
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