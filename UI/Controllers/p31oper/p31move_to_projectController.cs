using Microsoft.AspNetCore.Mvc;
using UI.Models;
using UI.Models.p31oper;

namespace UI.Controllers.p31oper
{
    public class p31move_to_projectController : BaseController
    {
        public IActionResult Index(string pids,string guid_pids)
        {
            if (!string.IsNullOrEmpty(guid_pids))
            {
                pids = Factory.p85TempboxBL.LoadByGuid(guid_pids).p85Message; //vstupní pids předány přes p85Tempbox
            }
            var v = new p31MoveToProjectViewModel() { pids = pids, pids_valid = pids };
            v.ProjectCombo = new ProjectComboViewModel();
            RefreshState(v);
            if (v.lisP31.Count() == 0)
            {
                return StopPage(true, "Na vstupu chybí rozpracované úkony.");
            }
            v.pids_valid = string.Join(",", v.lisP31.Select(p => p.pid));
            RefreshState(v);

            if (v.lisP31.Count() == 0)
            {
                return StopPage(true, "Na vstupu chybí rozpracované úkony.");
            }
            
            if (v.lisP31.Count() < BO.Code.Bas.ConvertString2ListInt(v.pids).Count())
            {
                AddMessage("Schválené nebo vyúčtované úkony jsem vyřadil z výběru.", "info");
            }


            return View(v);
        }

        private void RefreshState(p31MoveToProjectViewModel v)
        {
            var mq = new BO.myQueryP31() {p31statequery=20, pids = BO.Code.Bas.ConvertString2ListInt(v.pids_valid) };
            v.lisP31 = Factory.p31WorksheetBL.GetList(mq);
            

            
            
            v.gridinput = new Views.Shared.Components.myGrid.myGridInput() {is_enable_selecting=false, entity = "p31Worksheet", master_entity = "inform", myqueryinline = $"pids|list_int|{v.pids_valid}", oncmclick = "", ondblclick = "" };
            v.gridinput.query = new BO.InitMyQuery(Factory.CurrentUser).Load("p31", null, 0, $"pids|list_int|{v.pids_valid}");

        }


        [HttpPost]
        public IActionResult Index(p31MoveToProjectViewModel v)
        {
            RefreshState(v);

            if (v.IsPostback)
            {
                
                return View(v);
            }

            if (ModelState.IsValid)
            {
                if (v.ProjectCombo.SelectedP41ID == 0)
                {
                    this.AddMessageTranslated("Musíte vybrat cílový projekt.");
                    return View(v);
                }
                if (v.IsChangeP32ID && v.DestP32ID == 0)
                {
                    this.AddMessageTranslated("Musíte vybrat cílovou aktivitu.");
                    return View(v);
                }
                int intOks = Factory.p31WorksheetBL.Move2Project(v.ProjectCombo.SelectedP41ID, v.lisP31.Select(p => p.pid).ToList(),v.DestP32ID);
                
                if (intOks == 0)
                {
                    this.AddMessageTranslated("Nedošlo k přesunutí úkonů na cílový projekt.");
                    return View(v);
                }

                v.SetJavascript_CallOnLoad(0);
                return View(v);
            }

            this.Notify_RecNotSaved();
            return View(v);
        }
    }
}
