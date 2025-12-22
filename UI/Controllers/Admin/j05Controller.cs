using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UI.Models;
using UI.Models.Record;

namespace UI.Controllers.Admin
{
    public class j05Controller : BaseController
    {
        public IActionResult Record(int pid, bool isclone)
        {
            var v = new j05Record() { rec_pid = pid, rec_entity = "j05" };
            v.Rec = new BO.j05MasterSlave();
            if (v.rec_pid > 0)
            {
                v.Rec = Factory.j05MasterSlaveBL.Load(v.rec_pid);
                if (v.Rec == null)
                {
                    return RecNotFound(v);
                }
                if (v.Rec.j11ID_Slave > 0) v.SlaveIsTeam = true;
                v.ComboPersonMaster = v.Rec.PersonMaster;
                v.ComboPersonSlave = v.Rec.PersonSlave;
                v.ComboTeamSlave = v.Rec.TeamSlave;
            }
            RefreshState(v);
            v.Toolbar = new MyToolbarViewModel(v.Rec);
            if (isclone)
            {
                v.MakeClone();
            }

            return ViewTup(v, BO.PermValEnum.GR_Admin);
        }

        private void RefreshState(j05Record v)
        {


        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Record(j05Record v)
        {
            RefreshState(v);
            if (v.IsPostback)
            {
                return View(v);
            }
            if (ModelState.IsValid)
            {
                BO.j05MasterSlave c = new BO.j05MasterSlave();
                if (v.rec_pid > 0) c = Factory.j05MasterSlaveBL.Load(v.rec_pid);
                c.j02ID_Master = v.Rec.j02ID_Master;
                c.j02ID_Slave = v.Rec.j02ID_Slave;
                c.j11ID_Slave = v.Rec.j11ID_Slave;
                c.j05Disposition_p31 = v.Rec.j05Disposition_p31;
                c.j05IsCreate_p31 = v.Rec.j05IsCreate_p31;
                c.j05Disposition_j02 = v.Rec.j05Disposition_j02;

                c.ValidUntil = v.Toolbar.GetValidUntil(c);
                c.ValidFrom = v.Toolbar.GetValidFrom(c);

                c.pid = Factory.j05MasterSlaveBL.Save(c);
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
