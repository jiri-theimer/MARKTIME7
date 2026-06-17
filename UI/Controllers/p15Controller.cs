using Microsoft.AspNetCore.Mvc;
using UI.Models.Record;
using UI.Models;
using UI.Models.Tab1;
using System.Net.WebSockets;

namespace UI.Controllers
{
    public class p15Controller : BaseController
    {
        public BO.Result AppendWeatherLog(string record_prefix,int record_pid)
        {
            return Factory.p15LocationBL.AppendWeatherLog(record_prefix, record_pid);
            
        }
        public IActionResult Info(int pid)
        {
            return Tab1(pid, "info");
        }
        public IActionResult Tab1(int pid, string caller)
        {
            var v = new p15Tab1() { Factory = this.Factory, pid = pid, caller = caller };

            v.Rec = Factory.p15LocationBL.Load(v.pid);
            if (v.Rec != null)
            {
               

            }

            return View(v);
        }
        public IActionResult Record(int pid, bool isclone)
        {
            
            var v = new p15Record() { rec_pid = pid, rec_entity = "p15", ComboOwner = Factory.CurrentUser.FullnameDesc };
            v.Notepad = new Models.Notepad.EditorViewModel() { Prefix = "p15", SelectedX04ID = Factory.Lic.x04ID_Default };
            v.Rec = new BO.p15Location() { j02ID_Owner = Factory.CurrentUser.pid };
            if (v.rec_pid > 0)
            {
                v.Rec = Factory.p15LocationBL.Load(v.rec_pid);
                if (v.Rec == null)
                {
                    return RecNotFound(v);
                }
                v.Longitude = v.Rec.p15Longitude.ToString();
                v.Latitude = v.Rec.p15Latitude.ToString();
                v.ComboOwner = v.Rec.Owner;
                v.Notepad.HtmlContent = v.Rec.p15Notepad;
                v.Notepad.SelectedX04ID = v.Rec.x04ID;
                

            }
            RefreshState(v);
            v.Toolbar = new MyToolbarViewModel(v.Rec);
            if (isclone)
            {
                v.MakeClone();
            }

            return ViewTup(v, BO.PermValEnum.GR_Admin);
        }

        private void RefreshState(p15Record v)
        {
            v.lisAutocomplete = Factory.o15AutoCompleteBL.GetList(new BO.myQuery("o15"));

        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Record(p15Record v)
        {
            RefreshState(v);

            if (v.IsPostback)
            {
                if (v.PostbackOper == "geo")
                {
                    var c = new BL.Code.GeoSupport();
                    var georesult = c.LoadGeoPlace(v.Rec.p15City, v.Rec.p15Street).Result;
                    if (georesult.IsError)
                    {
                        this.AddMessageTranslated(georesult.ErrorMessage);
                    }
                    else
                    {
                        this.AddMessageTranslated("Nalezeno: "+ georesult.FormattedAddress+", "+ georesult.CountryRegion, "info");
                        v.Latitude = georesult.Latitude.ToString();
                        v.Longitude = georesult.Longitude.ToString();
                    }
                    
                }
                return View(v);
            }

            if (ModelState.IsValid)
            {
                BO.p15Location c = new BO.p15Location();
                if (v.rec_pid > 0) c = Factory.p15LocationBL.Load(v.rec_pid);
                c.p15Name = v.Rec.p15Name;
                c.p15Street = v.Rec.p15Street;
                c.p15City = v.Rec.p15City;
                c.p15PostCode = v.Rec.p15PostCode;
                c.p15Country = v.Rec.p15Country;
                c.p15Notepad = v.Notepad.HtmlContent;
                c.x04ID = v.Notepad.SelectedX04ID;
                c.p15Longitude = BO.Code.Bas.InDouble(v.Longitude);
                c.p15Latitude = BO.Code.Bas.InDouble(v.Latitude);
                c.j02ID_Owner = v.Rec.j02ID_Owner;
                c.ValidUntil = v.Toolbar.GetValidUntil(c);
                c.ValidFrom = v.Toolbar.GetValidFrom(c);

                c.pid = Factory.p15LocationBL.Save(c);
                if (c.pid > 0)
                {
                    Factory.o27AttachmentBL.CommitNotepdChanges(v.Notepad.TempGuid, "p15", c.pid);

                    v.SetJavascript_CallOnLoad(c.pid);
                    return View(v);
                }

            }


            this.Notify_RecNotSaved();
            return View(v);
        }
    }
}
