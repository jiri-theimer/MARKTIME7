using Microsoft.AspNetCore.Mvc;
using UI.Models.Record;
using UI.Models;
using System.Data;

namespace UI.Controllers.Admin
{
    public class b20Controller : BaseController
    {
        private void pokus()
        {
            //var d0 = DateTime.Now;
            //for (int i = 1; i <= 60; i++)
            //{

            //    var dr = Factory.db.GetDataReader($"select top {i} * from p31Worksheet");
            //    while (dr.Read())
            //    {

            //    }
            //    dr.Close();

            //}
            //BO.Code.File.LogInfo("datareader: " + (DateTime.Now - d0).TotalMilliseconds.ToString());

            //var d0 = DateTime.Now;
            //for(int i = 1; i <= 60; i++)
            //{
            //    var dt = Factory.db.GetDataTable($"select top {i} * from p31Worksheet");
            //    foreach (DataRow dbrow in dt.Rows)
            //    {

            //    }
            //}

            //BO.Code.File.LogInfo("datatable: " + (DateTime.Now - d0).TotalMilliseconds.ToString());

        }
        public IActionResult Record(int pid, bool isclone, int b01id)
        {
            pokus();
            var v = new b20Record() { rec_pid = pid, rec_entity = "b20" };
            v.Rec = new BO.b20Hlidac() { b20TypeFlag = BO.b20TypeFlagEnum.TestBySql, b20NextRunAfterMinutes = 60*5 };

            if (v.rec_pid > 0)
            {
                v.Rec = Factory.b20HlidacBL.Load(v.rec_pid);
                if (v.Rec == null)
                {
                    return RecNotFound(v);
                }
                try
                {
                    if (v.Rec.j11ID_Notify > 0)
                    {
                        v.ComboTeam = Factory.j11TeamBL.Load(v.Rec.j11ID_Notify).j11Name;
                    }
                    if (v.Rec.x67ID_Notify1 > 0)
                    {
                        v.ComboRole1 = Factory.x67EntityRoleBL.Load(v.Rec.x67ID_Notify1).x67Name;
                    }
                    if (v.Rec.x67ID_Notify2 > 0)
                    {
                        v.ComboRole2 = Factory.x67EntityRoleBL.Load(v.Rec.x67ID_Notify2).x67Name;
                    }
                }
                catch
                {
                    //nic divného
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

        private void RefreshState(b20Record v)
        {
           

        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Record(b20Record v)
        {
            RefreshState(v);
            if (v.IsPostback)
            {
                return View(v);
            }
            if (ModelState.IsValid)
            {
                BO.b20Hlidac c = new BO.b20Hlidac();
                if (v.rec_pid > 0) c = Factory.b20HlidacBL.Load(v.rec_pid);

                c.b20TypeFlag = v.Rec.b20TypeFlag;
                
                c.b20Entity = v.Rec.b20Entity;
                c.b20Name = v.Rec.b20Name;
                c.b20Ordinary = v.Rec.b20Ordinary;
                c.b20EntityAlias = v.Rec.b20EntityAlias;


                c.b20TestSql = v.Rec.b20TestSql;
                c.b20RunSql = v.Rec.b20RunSql;
                c.b20Par1Name = v.Rec.b20Par1Name;
                c.b20Par2Name = v.Rec.b20Par2Name;
                c.b20NextRunAfterMinutes = v.Rec.b20NextRunAfterMinutes;
                c.b20TestTimeFrom = v.Rec.b20TestTimeFrom;
                c.b20TestTimeUntil = v.Rec.b20TestTimeUntil;

                c.j61ID = v.Rec.j61ID;
                c.b20NotifyMessage = v.Rec.b20NotifyMessage;
                c.b20NotifyReceivers = v.Rec.b20NotifyReceivers;
                c.j11ID_Notify = v.Rec.j11ID_Notify;
                c.x67ID_Notify1 = v.Rec.x67ID_Notify1;
                c.x67ID_Notify2 = v.Rec.x67ID_Notify2;
                c.b20IsNotifyRecordOwner = v.Rec.b20IsNotifyRecordOwner;

                c.ValidUntil = v.Toolbar.GetValidUntil(c);
                c.ValidFrom = v.Toolbar.GetValidFrom(c);


                c.pid = Factory.b20HlidacBL.Save(c);
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
