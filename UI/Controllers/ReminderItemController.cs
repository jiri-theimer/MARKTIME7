using Microsoft.AspNetCore.Mvc;
using UI.Models;
using UI.Models.p91oper;

namespace UI.Controllers
{
    public class ReminderItemController : BaseController
    {
        public IActionResult Index(string record_prefix,int record_pid,bool is_static_date, int o24count,string o24unit,int o24mediumflag,string bindprefix, int j02id, int j11id,int x67id,int p28id,int p24id,string o24memo,string o24staticdate)
        {
            var v = new ReminderItemViewModel() { record_prefix = record_prefix,o24Count=o24count, BindPrefix=bindprefix,j02ID=j02id,j11ID=j11id,p28ID=p28id,p24ID=p24id,x67ID=x67id,o24Memo=o24memo,o24Unit= o24unit };
            if (string.IsNullOrEmpty(v.BindPrefix))
            {
                v.BindPrefix = "j02";
            }
            if (v.j02ID == 0 && v.j11ID == 0 && v.p28ID == 0 && v.p24ID == 0 && v.x67ID == 0)
            {
                v.j02ID = Factory.CurrentUser.pid;
            }
            if (v.j02ID > 0)
            {
                v.ComboJ02ID = Factory.j02UserBL.Load(v.j02ID).FullnameDesc;
            }
            if (v.j11ID > 0)
            {
                v.ComboJ11ID = Factory.j11TeamBL.Load(v.j11ID).j11Name;
            }
            if (v.p24ID > 0)
            {
                v.ComboP24Name = Factory.p24ContactGroupBL.Load(v.p24ID).p24Name;
            }
            if (v.x67ID > 0)
            {
                v.ComboX67ID = Factory.x67EntityRoleBL.Load(v.x67ID).x67Name;
            }
            if (v.p28ID > 0)
            {
                v.ComboP28Name = Factory.p28ContactBL.Load(v.p28ID).p28Name;
            }
            if (o24mediumflag > 0)
            {
                v.o24MediumFlag = (BO.o24MediumFlagEnum)o24mediumflag;
            }
            
            v.is_static_date = is_static_date;
            if (v.is_static_date)
            {

            }
            if (!string.IsNullOrEmpty(o24staticdate))
            {
                v.o24StaticDate = BO.Code.Bas.String2Date(o24staticdate);
            }
            else
            {
                if (v.is_static_date)
                {
                    v.o24StaticDate = DateTime.Today.AddDays(1);
                }
            }
            
            

            RefreshState(v);

            return View(v);
        }


        private void RefreshState(ReminderItemViewModel v)
        {

        }
        [HttpPost]
        public IActionResult Index(ReminderItemViewModel v)
        {
            RefreshState(v);
            if (v.IsPostback)
            {
                return View(v);
            }

            if (ModelState.IsValid)
            {
                if (v.is_static_date && v.o24StaticDate == null)
                {
                    v.message = "Na vstupu chybí datum.";
                    return View(v);
                }
                if (v.BindPrefix=="j02" && v.j02ID == 0)
                {
                    v.message = "Na vstupu chybí zadat uživatele.";
                    return View(v);
                }
                if (v.BindPrefix == "j11" && v.j11ID == 0)
                {
                    v.message = "Na vstupu chybí zadat tým.";
                    return View(v);
                }
                if (v.BindPrefix == "p28" && v.p28ID == 0)
                {
                    v.message = "Na vstupu chybí zadat kontakt.";
                    return View(v);
                }
                if (v.BindPrefix == "p24" && v.p24ID == 0)
                {
                    v.message = "Na vstupu chybí zadat skupina kontaktů.";
                    return View(v);
                }
                v.issubmit = true;
                
                return View(v);

            }

            
            return View(v);
        }
    }
}
