using Microsoft.AspNetCore.Mvc;
using UI.Models.wrk;
using UI.Models;

namespace UI.Controllers.wrk
{
    public class workflow_statusController : BaseController
    {
        public IActionResult Index(int record_pid, string record_prefix, int run_b02id, string caller, bool povinnynazev)
        {            
            var v = new WorkflowDialogViewModel() { RecordPid = record_pid, RecordEntity = record_prefix, Caller = caller, NameIsRequired = povinnynazev };
            if (caller == "portal")
            {
                v.NameIsRequired = true; v.IsPortalAccess = true;
            }

            v.Notepad = new Models.Notepad.EditorViewModel() { Prefix = "b05", SelectedX04ID = Factory.Lic.x04ID_Default };
            if (v.RecordEntity != "o22" && v.RecordEntity != "p31" && v.RecordEntity != "j02" && v.RecordEntity != "p90")
            {
                var tuple = Factory.WorkflowBL.LoadEntityAttributes(v.RecordEntity, v.RecordPid);
                v.b01ID = tuple.b01ID; v.b02ID = tuple.b02ID;
            }

            if (v.b01ID > 0 && v.b02ID == 0)
            {
                v.b02ID = Factory.WorkflowBL.InitWorkflowStatus(v.RecordPid, v.RecordEntity);
                if (v.b02ID > 0)
                {
                    this.AddMessage("U záznam jsem nahodil výchozí workflow stav workflow šablony.", "info");
                }
                else
                {
                    this.AddMessage("U záznam jsem se pokoušel nahodit výchozí workflow stav, ale neúspěšně.");
                }
            }


            RefreshState(v);

            if (run_b02id > 0)  //změna worklow stavu bez krokového mechanismu
            {
                if (v.lisB02.Where(p => p.pid == run_b02id).Count() > 0)
                {
                    if (Factory.WorkflowBL.RunWorkflowStatus(v.RecordEntity, v.RecordPid, run_b02id,null,0) > 0)
                    {
                        v.SetJavascript_CallOnLoad(v.RecordPid);
                    }

                }
            }

            return View(v);
        }


        private void RefreshState(WorkflowDialogViewModel v)
        {
            if (v.b02ID > 0)
            {
                v.RecB02 = Factory.b02WorkflowStatusBL.Load(v.b02ID);               
            }
            

            v.RecB01 = Factory.b01WorkflowTemplateBL.Load(v.b01ID);
            v.lisB02 = Factory.b02WorkflowStatusBL.GetList(new BO.myQuery("b02")).Where(p => p.b01ID == v.b01ID && p.b02AutoRunFlag == BO.b02AutoRunFlagEnum.Standard).ToList();
            v.lisB02.Add(new BO.b02WorkflowStatus() { pid = 0, b02Name = "Doplnit poznámku" });

            
            if (v.SelectedB02ID > 0 && v.RecB02 == null)
            {
                v.RecB02 = Factory.b02WorkflowStatusBL.Load(v.SelectedB02ID);
            }
         

         
            if (v.Notepad == null)
            {
                v.Notepad = new Models.Notepad.EditorViewModel() { Prefix = "b05", SelectedX04ID = Factory.Lic.x04ID_Default };
            }

            
            if (v.reminder == null)
            {
                v.reminder = new ReminderViewModel() { is_static_date = true, record_pid = 0, record_prefix = "b05" };
            }


        }


        [HttpPost]
        public IActionResult Index(WorkflowDialogViewModel v)
        {
            RefreshState(v);

            if (v.IsPostback)
            {
                if (v.PostbackOper == "mail")
                {
                    int intB05ID = Handle_RunStatus(v);
                    if (intB05ID > 0)
                    {
                        return RedirectToAction("SendMail", "Mail", new { record_entity = v.RecordEntity, record_pid = v.RecordPid, b05id = intB05ID });
                    }
                }
                return View(v);
            }

            if (ModelState.IsValid)
            {
                if (Handle_RunStatus(v) == 0)
                {
                    return View(v);
                }
                else
                {
                    v.SetJavascript_CallOnLoad(v.RecordPid);
                }

            }

            return View(v);
        }


        private int Handle_RunStatus(WorkflowDialogViewModel v)
        {
            if (v.NameIsRequired && string.IsNullOrEmpty(v.b05Name))
            {
                this.AddMessage("Chybí vyplnit název."); return 0;
            }
            
           

            if (v.reminder.lisReminder != null && v.reminder.lisReminder.Where(p => p.IsTempDeleted == false && p.o24StaticDate == null).Count() > 0)
            {
                this.AddMessage("V upozornění chybí vyplnit datum+čas."); return 0;
            }
            

            string strNotepad = null; int intX04ID = 0;
            if (v.Notepad != null)
            {
                strNotepad = v.Notepad.HtmlContent;
                intX04ID = v.Notepad.SelectedX04ID;
            }
            int intPortalFlag = (v.IsPortalAccess ? 1 : 0);
            int intTab1Flag = 0;
            if (v.IsTab1) intTab1Flag += 2;
            if (v.IsBillingMemo) intTab1Flag += 4;
            int intB05ID = Factory.WorkflowBL.RunWorkflowStatus(v.RecordEntity, v.RecordPid,v.SelectedB02ID, strNotepad, intX04ID,v.b05Date, v.b05Name,intPortalFlag,intTab1Flag);


            if (intB05ID > 0)
            {
                if (v.Notepad != null)
                {
                    Factory.o27AttachmentBL.CommitNotepdChanges(v.Notepad.TempGuid, "b05", intB05ID);
                }

                if (v.reminder != null)
                {
                    v.reminder.SaveChanges(Factory, intB05ID);
                }

                return intB05ID;
            }

            return 0;
        }
    }
}
