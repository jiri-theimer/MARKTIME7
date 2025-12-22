using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UI.Models;
using UI.Models.p31invoice;
using UI.Models.p31oper;

namespace UI.Controllers.p31oper
{
    public class p31clonebatchController : BaseController
    {


        public IActionResult Index(string pids,string guid_pids)
        {
            if (!string.IsNullOrEmpty(guid_pids))
            {
                pids = Factory.p85TempboxBL.LoadByGuid(guid_pids).p85Message; //vstupní pids předány přes p85Tempbox
            }
            var v = new p31CloneBatchViewModel() { pids = pids };
            v.ProjectCombo = new ProjectComboViewModel();

            RefreshState(v);
            if (v.lisSourceP31.Count() == 0)
            {
                return StopPage(true, "Na vstupu musí být minimálně jeden časový nebo kusovníkový úkon.");
            }
            v.lisDest = new List<DestRecord>();
            foreach (var rec in v.lisSourceP31)
            {
                v.lisDest.Add(new DestRecord() { p31Date = rec.p31Date, p31ID = rec.pid, DestText = rec.p31Text, DestHours = rec.p31Hours_Orig.ToString(), Project = rec.Project,p41ID=rec.p41ID, Person = rec.Person, p32Name = rec.p32Name });

            }
            if (v.lisSourceP31.Count() > v.lisDest.Count())
            {
                AddMessage("Některé úkony jsem vyřadil, protože nebyli časové nebo kusovníkové.", "info");
            }


            return View(v);
        }

        private void RefreshState(p31CloneBatchViewModel v)
        {
            var mq = new BO.myQueryP31() { tabquery = "time_or_kusovnik", pids = BO.Code.Bas.ConvertString2ListInt(v.pids) };
            v.lisSourceP31 = Factory.p31WorksheetBL.GetList(mq);

            if (v.ProjectCombo.SelectedP41ID > 0)
            {
                v.RecP41 = Factory.p41ProjectBL.Load(v.ProjectCombo.SelectedP41ID);
            }

        }

        [HttpPost]
        public IActionResult Index(p31CloneBatchViewModel v)
        {
            RefreshState(v);

            if (v.IsPostback)
            {
                if (v.PostbackOper == "p41id" && v.ProjectCombo.SelectedP41ID>0)
                {
                    foreach (var c in v.lisDest)
                    {
                        c.p41ID = v.ProjectCombo.SelectedP41ID; c.Project = v.RecP41.p41Name;
                    }
                }



                return View(v);
            }

            if (ModelState.IsValid)
            {
                int intErrs = 0; int intOks = 0;
                foreach (var c in v.lisDest)
                {
                    var recSource = v.lisSourceP31.First(p => p.pid == c.p31ID);
                    var rec = new BO.p31WorksheetEntryInput() { p41ID = c.p41ID, p32ID = recSource.p32ID, j02ID = recSource.j02ID, p31Text = c.DestText };
                    rec.Value_Orig = c.DestHours; rec.Value_Orig_Entried = c.DestHours; rec.p34ID = recSource.p34ID;
                    rec.Addp31Date(c.p31Date);
                    rec.p31HoursEntryflag = recSource.p31HoursEntryFlag;

                    if (!Factory.CurrentUser.TestPermission(BO.PermValEnum.GR_P31_Creator_Hours))
                    {
                        //nemá oprávnění vykazovat za celou firmu
                        rec.j02ID = Factory.CurrentUser.pid;
                    }
                    c.p31ID_Saved = Factory.p31WorksheetBL.SaveOrigRecord(rec, recSource.p33ID, null);
                    if (c.p31ID_Saved == 0)
                    {
                        intErrs += 1;
                    }
                    else
                    {
                        intOks += 1;
                    }

                }
                if (intErrs > 0)
                {

                    this.AddMessageTranslated($"Počet uložených úkonů: {intOks}, počet chyb: {intErrs}.", "info");
                }
                else
                {
                    v.SetJavascript_CallOnLoad(0);
                    return View(v);
                }
            }

            return View(v);
        }



    }



}
