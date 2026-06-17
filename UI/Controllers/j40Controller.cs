using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using UI.Models;
using UI.Models.Record;

namespace UI.Controllers
{
    public class j40Controller : BaseController
    {
        public IActionResult Record(int pid, bool isclone)
        {
            var v = new j40Record() { rec_pid = pid, rec_entity = "j40" };
            v.Rec = new BO.j40MailAccount() { j40UsageFlag = BO.MailUsageFlag.SmtpPrivate };
            if (v.rec_pid > 0)
            {
                v.Rec = Factory.j40MailAccountBL.Load(v.rec_pid);
                if (v.Rec == null)
                {
                    return RecNotFound(v);
                }
                v.Rec.j40SmtpPassword = null;
                v.Rec.j40ImapPassword = null;
                v.ComboPerson = v.Rec.Person;
                if (v.Rec.j40SmtpEnableSsl)
                {
                    v.IsUseSSL = true;
                }


            }
            else
            {
                v.Rec.j02ID = Factory.CurrentUser.pid;
            }
            if (v.Rec.j02ID > 0)
            {
                v.ComboPerson = Factory.j02UserBL.Load(v.Rec.j02ID).FullnameDesc;
            }
            v.Toolbar = new MyToolbarViewModel(v.Rec);
            if (isclone)
            {
                v.MakeClone();
            }
            return View(v);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Record(j40Record v)
        {
            if (v.IsPostback)
            {                
                return View(v);
            }
            if (ModelState.IsValid)
            {
                BO.j40MailAccount c = new BO.j40MailAccount();
                if (v.rec_pid > 0) c = Factory.j40MailAccountBL.Load(v.rec_pid);
                c.j02ID = v.Rec.j02ID;
                c.j40UsageFlag = v.Rec.j40UsageFlag;
                c.j40Name = v.Rec.j40Name;
                c.j40SmtpHost = v.Rec.j40SmtpHost;
                c.j40SmtpPort = v.Rec.j40SmtpPort;
                c.j40SmtpName = v.Rec.j40SmtpName;
                c.j40SmtpEmail = v.Rec.j40SmtpEmail;
                c.j40SmtpUsePersonalReply = v.Rec.j40SmtpUsePersonalReply;
                c.j40SmtpLogin = v.Rec.j40SmtpLogin;
                if (!String.IsNullOrEmpty(v.Rec.j40SmtpPassword))
                {
                    c.j40SmtpPassword = new BO.Code.Cls.Crypto().Encrypt(v.Rec.j40SmtpPassword);
                }
                c.j40SmtpUseDefaultCredentials = v.Rec.j40SmtpUseDefaultCredentials;
                c.j40SmtpEnableSsl = v.Rec.j40SmtpEnableSsl;
                c.j40SslModeFlag = v.Rec.j40SslModeFlag;

                c.j40ImapHost = v.Rec.j40ImapHost;
                c.j40ImapPort = v.Rec.j40ImapPort;
               
                c.j40ImapLogin = v.Rec.j40ImapLogin;                
                if (!String.IsNullOrEmpty(v.Rec.j40ImapPassword))
                {
                    c.j40ImapPassword =new BO.Code.Cls.Crypto().Encrypt(v.Rec.j40ImapPassword);
                }                
                c.j40ImapEnableSsl = v.Rec.j40ImapEnableSsl;


                c.ValidUntil = v.Toolbar.GetValidUntil(c);
                c.ValidFrom = v.Toolbar.GetValidFrom(c);
                c.j40Ordinary = v.Rec.j40Ordinary;

                c.pid= Factory.j40MailAccountBL.Save(c);
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