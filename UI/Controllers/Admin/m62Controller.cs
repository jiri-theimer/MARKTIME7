using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Net.Http;
using UI.Models;
using UI.Models.Record;
using UI.Models.Admin;

namespace UI.Controllers.Admin
{
    public class m62Controller : BaseController
    {
        private readonly IHttpClientFactory _httpclientfactory;
        public m62Controller(IHttpClientFactory hcf)
        {
            _httpclientfactory = hcf;
        }
        public IActionResult Settings()
        {
            var v = new m62SettingsViewModel() { SelectedDate=DateTime.Today };
            
            RefreshStateSettings(v);
            var recX01 = Factory.x01LicenseBL.Load(Factory.CurrentUser.x01ID);
            if (!string.IsNullOrEmpty(recX01.x01ImportCnb_j27Codes))
            {
                v.SelectedJ27IDs = new List<int>();
                foreach(var strJ27Code in recX01.x01ImportCnb_j27Codes.Split(","))
                {
                    v.SelectedJ27IDs.Add(Factory.FBL.LoadCurrencyByCode(strJ27Code).pid);
                }
                
            }
            return View(v);
        }

        private void RefreshStateSettings(m62SettingsViewModel v)
        {
            v.lisAllJ27 = Factory.FBL.GetListCurrency().Where(p => p.j27ID != 2);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Settings(m62SettingsViewModel v)
        {
            RefreshStateSettings(v);
            
            if (v.PostbackOper == "import")
            {
                var httpclient = _httpclientfactory.CreateClient();
                int intPID = Factory.m62ExchangeRateBL.ImportOneRate(httpclient, v.SelectedDate, v.SelectedJ27ID);
                if (intPID > 0)
                {
                    v.SetJavascript_CallOnLoad(intPID);
                    return View(v);
                }
                
            }
            if (v.PostbackOper == "settings")
            {
                var codes = new List<string>();
                foreach(var j27id in v.SelectedJ27IDs.Where(p=>p>0))
                {
                    codes.Add(Factory.FBL.LoadCurrencyByID(j27id).j27Code);
                }
                var recX01 = Factory.x01LicenseBL.Load(Factory.CurrentUser.x01ID);
                recX01.x01ImportCnb_j27Codes = string.Join(",", codes);
                Factory.x01LicenseBL.Save(recX01);
                
                v.SetJavascript_CallOnLoad(0);
                return View(v);
            }

            
            this.Notify_RecNotSaved();
            return View(v);
        }
        public IActionResult Record(int pid, bool isclone)
        {
            var v = new m62Record() { rec_pid = pid, rec_entity = "m62" };
            v.Rec = new BO.m62ExchangeRate() { m62Date = DateTime.Today };
            if (v.rec_pid > 0)
            {
                v.Rec = Factory.m62ExchangeRateBL.Load(v.rec_pid);
                if (v.Rec == null)
                {
                    return RecNotFound(v);
                }
                v.ComboJ27Master = v.Rec.j27Code_Master;
                v.ComboJ27Slave = v.Rec.j27Code_Slave;

            }
            RefreshStateRecord(v);
            v.Toolbar = new MyToolbarViewModel(v.Rec);
            if (isclone)
            {
                v.MakeClone();
            }

            return ViewTup(v, BO.PermValEnum.GR_Admin);
        }

        private void RefreshStateRecord(m62Record v)
        {


        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Record(m62Record v)
        {
            RefreshStateRecord(v);

            if (ModelState.IsValid)
            {
                BO.m62ExchangeRate c = new BO.m62ExchangeRate();
                if (v.rec_pid > 0) c = Factory.m62ExchangeRateBL.Load(v.rec_pid);
                c.m62Date = v.Rec.m62Date;
                c.m62RateType = v.Rec.m62RateType;
                c.m62Units = v.Rec.m62Units;
                c.m62Rate = v.Rec.m62Rate;
                c.j27ID_Master = v.Rec.j27ID_Master;
                c.j27ID_Slave = v.Rec.j27ID_Slave;
                
                c.ValidUntil = v.Toolbar.GetValidUntil(c);
                c.ValidFrom = v.Toolbar.GetValidFrom(c);

                c.pid = Factory.m62ExchangeRateBL.Save(c);
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
