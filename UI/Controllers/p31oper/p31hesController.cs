using Microsoft.AspNetCore.Mvc;
using UI.Models.p31oper;

namespace UI.Controllers.p31oper
{
    public class p31hesController : BaseController
    {
        //Uživatelovo nastavení formátu vykazování hodin
        public IActionResult Index(int tabindex)
        {
            var v = new hesViewModel() { HoursFormat = Factory.CurrentUser.j02DefaultHoursFormat, HesBitStream = Factory.CurrentUser.j02HesBitStream };
            if (tabindex > 0)
            {
                v.ActiveTabIndex = tabindex;
            }
            v.InhaleSetting();

            v.Approve_GridBox_Position = Factory.CBL.LoadUserParamInt("approve-gridbox-position", 1);
            v.Approve_Default_UI = Factory.CBL.LoadUserParam("approve-default-ui", "Inline");
            v.Approve_IsSkipGateway = Factory.CBL.LoadUserParamBool("approve-index-IsSkipGateway", false);
            v.TimeInputFrom= Factory.CBL.LoadUserParamInt("p31_TimeInputFrom", 8);
            v.TimeInputTo= Factory.CBL.LoadUserParamInt("p31_TimeInputTo", 18);
            v.TimeInputInterval = Factory.CBL.LoadUserParamInt("p31_TimeInputInterval", 30);

            return View(v);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Index(hesViewModel v)
        {
            v.HesBitStream = Factory.CurrentUser.j02HesBitStream;
            if (v.IsPostback)
            {
                return View(v);
            }


            if (ModelState.IsValid)
            {

                v.HesBitStream = 0;
                if (v.HoursInterval == 30) v.HesBitStream += 2;
                if (v.HoursInterval == 60) v.HesBitStream += 4;
                if (v.HoursInterval == 5) v.HesBitStream += 8;
                if (v.HoursInterval == 10) v.HesBitStream += 16;
                if (v.HoursInterval == 6) v.HesBitStream += 32;
                if (v.HoursInterval == 15) v.HesBitStream += 64;

                if (v.TimesheetEntryByMinutes) v.HesBitStream += 128;
                //if (v.OfferTrimming) v.HesBitStream += 256;
                if (v.OfferContactPerson) v.HesBitStream += 512;

                if (v.ActivityFlag == 99) v.HesBitStream += 1024;
                if (v.ActivityFlag == 0) v.HesBitStream += 2048;
                if (v.ActivityFlag == 1) v.HesBitStream += 4096;
                if (v.ActivityFlag == 2) v.HesBitStream += 8192;

                if (v.Approve_InterniHodiny) v.HesBitStream += 16384;
                if (v.Approve_HodinyVPausalu) v.HesBitStream += 32768;
                if (v.Approve_UrovenSchvalovani) v.HesBitStream += 65536;
                if (v.Approve_DoDefaultApproveState) v.HesBitStream += 131072;  //automaticky nahazovat fakturační status
                if (v.Approve_ShowRecZoom) v.HesBitStream += 262144;

                var c = Factory.j02UserBL.Load(Factory.CurrentUser.pid);
                c.j02HesBitStream = v.HesBitStream;
                c.j02DefaultHoursFormat = v.HoursFormat;

                Factory.CBL.SetUserParam("approve-index-IsSkipGateway", v.Approve_IsSkipGateway.ToString());

                if (Factory.j02UserBL.Save(c, null) == 0)
                {
                    
                    return View(v);
                }

                Factory.CBL.SetUserParam("approve-gridbox-position", v.Approve_GridBox_Position.ToString());
                Factory.CBL.SetUserParam("approve-default-ui", v.Approve_Default_UI);

                Factory.CBL.SetUserParam("p31_TimeInputFrom", v.TimeInputFrom.ToString());
                Factory.CBL.SetUserParam("p31_TimeInputTo", v.TimeInputTo.ToString());
                Factory.CBL.SetUserParam("p31_TimeInputInterval", v.TimeInputInterval.ToString());

                

                v.SetJavascript_CallOnLoad(0);
                return View(v);
            }

            this.Notify_RecNotSaved();
            return View(v);
        }



        
    }
}
