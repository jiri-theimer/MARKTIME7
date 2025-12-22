using Microsoft.AspNetCore.Mvc;
using UI.Models;
using UI.Models.Record;

namespace UI.Controllers.ical
{
    public class icalgenController : BaseController
    {
        public IActionResult Index()
        {
            var v = new iCalGenViewModel();
            
            RefreshState(v);
            v.PersonNameFormat = Factory.CBL.LoadUserParam("ical-PersonNameFormat", "inicialy");
            v.IsMe = Factory.CBL.LoadUserParamBool("ical-me", true);
            v.j02ID = Factory.CBL.LoadUserParamInt("ical-j02id",Factory.CurrentUser.pid);
            v.j11ID = Factory.CBL.LoadUserParamInt("ical-j11id");
            if (v.j02ID > 0)
            {
                v.ComboJ02ID = Factory.j02UserBL.Load(v.j02ID).FullNameAsc;
                
            }
            if (v.j11ID > 0)
            {
                v.ComboJ11ID = Factory.j11TeamBL.Load(v.j11ID).j11Name;
                
            }
            v.IsP31 = Factory.CBL.LoadUserParamBool("ical-p31", true);

            v.IsP56 = Factory.CBL.LoadUserParamBool("ical-p56", true);
            if (v.lisX67_p56.Count() > 0)
            {
                v.x67ID_p56 = Factory.CBL.LoadUserParamInt("ical-x67id_p56", v.lisX67_p56.First().pid);
            }
            v.IsO22 = Factory.CBL.LoadUserParamBool("ical-o22", true);
            if (v.lisX67_o22.Count() > 0)
            {
                v.x67ID_o22 = Factory.CBL.LoadUserParamInt("ical-x67id_o22", v.lisX67_o22.First().pid);                
            }
            v.p61ID = Factory.CBL.LoadUserParamInt("ical-p61id");
            v.p41ID = Factory.CBL.LoadUserParamInt("ical-p41id");
            if (v.p41ID > 0)
            {
                v.Project = Factory.p41ProjectBL.Load(v.p41ID).FullName;
            }
            v.p32IDs= Factory.CBL.LoadUserParam("ical-p32ids");
            if (!string.IsNullOrEmpty(v.p32IDs))
            {
                var lis = Factory.p32ActivityBL.GetList(new BO.myQueryP32() { pids = BO.Code.Bas.ConvertString2ListInt(v.p32IDs) });
                v.p32Names = string.Join(",", lis.Select(p => p.p32Name));
            }
            v.d1 = Factory.CBL.LoadUserParamDate("ical-d1");
            v.d2 = Factory.CBL.LoadUserParamDate("ical-d2");

            handle_complete_url(v);
            return View(v);
        }

        private void RefreshState(iCalGenViewModel v)
        {
            v.lisP61 = Factory.p61ActivityClusterBL.GetList(new BO.myQuery("p61"));
            v.lisX67_o22 = Factory.x67EntityRoleBL.GetList(new BO.myQuery("x67")).Where(p => p.x67Entity == "o22");
            v.lisX67_p56 = Factory.x67EntityRoleBL.GetList(new BO.myQuery("x67")).Where(p => p.x67Entity == "p56");

        }

        [HttpPost]        
        public IActionResult Index(iCalGenViewModel v)
        {
            RefreshState(v);
            if (v.j02ID > 0 && v.j11ID>0)
            {
                this.AddMessage("Není možné vyplnit uživatele i tým dohromady.");
            }
            if (v.IsP31 && v.p32IDs==null && v.p61ID == 0)
            {
                this.AddMessage("Pro časové úkony je třeba vybrat minimálně jednu aktivitu nebo klast aktivit.");
            }
         

            if (ModelState.IsValid)
            {
                handle_complete_url(v);



            }


            
            return View(v);
        }

        private void handle_complete_url(iCalGenViewModel v)
        {
            if (!v.IsP31 && !v.IsO22 && !v.IsP56)
            {
                v.FinalUrl = null;
                return;
            }
            //if (!v.IsMe && v.j02ID==0 && v.j11ID==0)
            //{
            //    v.FinalUrl = null;
            //    return;
            //}
            string strKey = new BO.Code.Cls.Crypto().EasyEncrypt(Factory.CurrentUser.j02Login);
            v.FinalUrl = $"{Factory.Lic.x01AppHost}/ical?key={strKey}";

            Factory.CBL.SetUserParam("ical-PersonNameFormat", v.PersonNameFormat);
            Factory.CBL.SetUserParam("ical-me", v.IsMe.ToString());

            Factory.CBL.SetUserParam("ical-p31", v.IsP31.ToString());
            if (v.IsP31)
            {
                Factory.CBL.SetUserParam("ical-p61id", v.p61ID.ToString());
                Factory.CBL.SetUserParam("ical-p32ids", v.p32IDs);
            }
            if (!v.IsMe)
            {
                Factory.CBL.SetUserParam("ical-j11id", v.j11ID.ToString());
                Factory.CBL.SetUserParam("ical-j02id", v.j02ID.ToString());
            }
            
            Factory.CBL.SetUserParam("ical-p41id", v.p41ID.ToString());

            Factory.CBL.SetUserParam("ical-p56", v.IsP56.ToString());
            if (v.IsP56)
            {
                Factory.CBL.SetUserParam("ical-x67id_p56", v.x67ID_p56.ToString());
            }

            Factory.CBL.SetUserParam("ical-o22", v.IsO22.ToString());
            if (v.IsO22)
            {
                Factory.CBL.SetUserParam("ical-x67id_o22", v.x67ID_o22.ToString());
            }
            
            
            
            if (v.d1 != null)
            {
                Factory.CBL.SetUserParam("ical-d1", v.d1.Value.ToString("dd.MM.yyyy"));
            }
            if (v.d2 != null)
            {
                Factory.CBL.SetUserParam("ical-d2", v.d2.Value.ToString("dd.MM.yyyy"));
            }

            if (v.IsMe)
            {
                v.FinalUrl += $"&j02id={Factory.CurrentUser.pid}";
            }
            else
            {
                if (v.j11ID > 0)
                {
                    v.FinalUrl += $"&j11id={v.j11ID}";
                }
                else
                {
                    if (v.j02ID > 0)
                    {
                        v.FinalUrl += $"&j02id={v.j02ID}";
                    }
                }
            }
            
            if (v.p41ID > 0)
            {
                v.FinalUrl += $"&p41id={v.p41ID}";
            }
            if (!string.IsNullOrEmpty(v.p32IDs))
            {
                v.FinalUrl += $"&p32ids={v.p32IDs}";
            }
            if (v.p61ID > 0)
            {
                v.FinalUrl += $"&p61id={v.p61ID}";
            }
            if (v.x67ID_p56>0)
            {
                v.FinalUrl += $"&x67id_p56={v.x67ID_p56}";
            }
            if (v.x67ID_o22>0)
            {
                v.FinalUrl += $"&x67id_o22={v.x67ID_o22}";
            }
            if (v.d1 != null && v.d2 != null)
            {
                v.FinalUrl += "&d1=" + BO.Code.Bas.ObjectDate2String(v.d1, "dd.MM.yyyy") + "&d2=" + BO.Code.Bas.ObjectDate2String(v.d2, "dd.MM.yyyy");
            }
            v.FinalUrl += "&person_name_format=" + v.PersonNameFormat;
            v.FinalUrl += "&n=marktime_icalendar.ics";
        }
    }
}
