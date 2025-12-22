using Microsoft.AspNetCore.Mvc;
using UI.Models.Record;
using UI.Models;
using UI.Models.wrk;
using BL.Singleton;


namespace UI.Controllers.wrk
{
    public class workflow_dialogController : BaseController
    {
        public IActionResult Index(int record_pid, string record_prefix, int run_b06id, string caller, bool povinnynazev, int run_b02id, int b06id)
        {
            record_prefix = BO.Code.Entity.GetPrefixDb(record_prefix);
            if (record_pid == 0 && record_prefix == "p31")
            {
                record_pid = Factory.CurrentUser.pid;
                record_prefix = "j02";  //pro ještě neuložený úkon uložit b05 záznam dočasně k uživateli
            }
            var v = new WorkflowDialogViewModel() { RecordPid = record_pid, RecordEntity = record_prefix, Caller = caller, NameIsRequired = povinnynazev };
            if (caller == "portal")
            {
                v.NameIsRequired = true; v.IsPortalAccess = true;
            }

            v.Notepad = new Models.Notepad.EditorViewModel() { Prefix = "b05", SelectedX04ID = Factory.Lic.x04ID_Default };
            if (v.RecordEntity != "o22" && v.RecordEntity != "p31" && v.RecordEntity != "j02" && v.RecordEntity != "p90" && v.RecordEntity != "p84")
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
            if (v.RecB01 != null && v.RecB01.b01PrincipleFlag == BO.b01PrincipleFlagEnum.StatusOnly)
            {
                return RedirectToAction("Index", "workflow_status", new { record_pid = record_pid, record_prefix = record_prefix, caller = caller, povinnynazev = povinnynazev, run_b02id = run_b02id });
            }

            if (b06id > 0 && v.lisB06.Where(p => p.pid == b06id).Count() > 0)
            {
                v.SelectedB06ID = b06id;
                v.RecB06 = null;

                RefreshState(v);
            }
            if (run_b06id > 0 && v.lisB06.Where(p => p.pid == run_b06id).Count() > 0)
            {
                v.SelectedB06ID = run_b06id;
                v.RecB06 = null;

                RefreshState(v);

                if (v.RecB06.p60ID > 0 && v.RecB06.b06AutoTaskFlag==BO.b06AutoTaskFlagEnum.Manual)
                {
                    //přejít na ruční založení úkolu
                    switch (record_prefix)
                    {
                        case "p41":
                        case "le5":
                            return RedirectToAction("Record", "p56", new { p60id = v.RecB06.p60ID, p41id = record_pid });
                        case "p91":
                            var recP91 = Factory.p91InvoiceBL.Load(record_pid);
                            return RedirectToAction("Record", "p56", new { p60id = v.RecB06.p60ID, p41id = recP91.p41ID_First });
                        default:
                            return RedirectToAction("Record", "p56", new { p60id = v.RecB06.p60ID });

                    }
                                     
                }

                if (Handle_RunStep(v) > 0)
                {
                    v.SetJavascript_CallOnLoad(v.RecordPid);
                }
            }

            if (record_prefix == "p91")
            {
                Handle_Invoice_Preview(record_pid);
            }

            return View(v);
        }


        private void RefreshState(WorkflowDialogViewModel v)
        {
            if (v.b02ID > 0)
            {
                v.RecB02 = Factory.b02WorkflowStatusBL.Load(v.b02ID);
                v.lisB06 = Factory.b06WorkflowStepBL.GetList(new BO.myQuery("b06")).Where(p => p.b02ID == v.b02ID && p.b06AutoRunFlag == BO.b06AutoRunFlagEnum.UzivatelskyKrok).ToList();
                foreach (var c in v.lisB06)
                {
                    if (Factory.WorkflowBL.IsStepAvailable4Me(c, v.RecordPid, v.RecordEntity))
                    {
                        c.UserInsert = "ok1";
                    }
                }
                v.lisB06 = v.lisB06.Where(p => p.UserInsert == "ok1").ToList();
            }
            else
            {
                v.lisB06 = new List<BO.b06WorkflowStep>();
            }

            v.RecB01 = Factory.b01WorkflowTemplateBL.Load(v.b01ID);


            v.lisB06.Add(new BO.b06WorkflowStep() { pid = 0, b06Name = "Doplnit poznámku", b06NotepadFlag = BO.b06NotepadFlagEnum.NotepadIsRequired });

            if (v.SelectedB06ID > 0 && v.RecB06 == null)
            {
                v.RecB06 = Factory.b06WorkflowStepBL.Load(v.SelectedB06ID);
            }
            if (v.RecB06 == null)
            {
                v.RecB06 = v.lisB06.First(p => p.pid == 0);
            }

            if (v.RecB06.b06NotepadFlag == BO.b06NotepadFlagEnum.WithoutNotepad)
            {
                v.Notepad = null;
            }
            if (v.RecB06.b06NotepadFlag != BO.b06NotepadFlagEnum.WithoutNotepad && v.Notepad == null)
            {
                v.Notepad = new Models.Notepad.EditorViewModel() { Prefix = "b05", SelectedX04ID = Factory.Lic.x04ID_Default };
            }

            if (v.RecB06.b06NomineeFlag == BO.b06NomineeFlagENum.RucniPovinna || v.RecB06.b06NomineeFlag == BO.b06NomineeFlagENum.RucniNepovinna)
            {

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
                    int intB05ID = Handle_RunStep(v);
                    if (intB05ID > 0)
                    {
                        return RedirectToAction("SendMail", "Mail", new { record_entity = v.RecordEntity, record_pid = v.RecordPid, b05id = intB05ID });
                    }
                }
                if (v.PostbackOper == "restart")    //vyčistit workflow záznamu
                {
                    Factory.WorkflowBL.InitWorkflowStatus(v.RecordPid, v.RecordEntity);
                    Factory.CBL.Restore(v.RecordEntity, v.RecordPid);
                    v.SetJavascript_CallOnLoad(v.RecordPid);
                }
                return View(v);
            }

            if (ModelState.IsValid)
            {
                if (Handle_RunStep(v) == 0)
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


        private int Handle_RunStep(WorkflowDialogViewModel v)
        {
            if (v.NameIsRequired && string.IsNullOrEmpty(v.b05Name))
            {
                this.AddMessage("Chybí vyplnit název."); return 0;
            }
            List<BO.x69EntityRole_Assign> lisNominee = null;


            if (v.RecB06.b06NomineeFlag == BO.b06NomineeFlagENum.RucniPovinna || v.RecB06.b06NomineeFlag == BO.b06NomineeFlagENum.RucniNepovinna)
            {
                lisNominee = new List<BO.x69EntityRole_Assign>();
                foreach (int j02id in BO.Code.Bas.ConvertString2ListInt(v.Nominee_j02IDs))
                {
                    lisNominee.Add(new BO.x69EntityRole_Assign() { j02ID = j02id, x67ID = v.RecB06.x67ID_Nominee });
                }
                foreach (int j11id in BO.Code.Bas.ConvertString2ListInt(v.Nominee_j11IDs))
                {
                    lisNominee.Add(new BO.x69EntityRole_Assign() { j11ID = j11id, x67ID = v.RecB06.x67ID_Nominee });
                }
                if (lisNominee.Count() == 0 && v.RecB06.b06NomineeFlag == BO.b06NomineeFlagENum.RucniPovinna)
                {
                    this.AddMessage("Nominace je povinná."); return 0;
                }
            }

            if (v.reminder.lisReminder != null && v.reminder.lisReminder.Where(p => p.IsTempDeleted == false && p.o24StaticDate == null).Count() > 0)
            {
                this.AddMessage("V upozornění chybí vyplnit datum+čas."); return 0;
            }

            if (v.RecB06.b06GeoFlag == BO.b06GeoFlagEnum.LoadFromCurrentUser && (BO.Code.Bas.InDouble(v.current_user_lon) == 0 || BO.Code.Bas.InDouble(v.current_cuser_lat) == 0))
            {
                this.AddMessage("Chybí souřadnice aktuální polohy uživatele."); return 0;
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

            if (v.RecB06.b06JobFlag == BO.b06JobFlagEnum.PdfFaktura)    //před spuštěním workflow vygenerovat PDF dokument faktury - lze řešit pouze na UI straně
            {
                var recP91 = Factory.p91InvoiceBL.Load(v.RecordPid);
                var recP92 = Factory.p92InvoiceTypeBL.Load(recP91.p92ID);
                if (recP92.x31ID_Invoice > 0)
                {
                    var recX31 = Factory.x31ReportBL.Load(recP92.x31ID_Invoice);
                    var strGUID = BO.Code.Bas.GetGuid();
                    var cc = new TheReportSupport();

                    var strTempPdfFileName = cc.GeneratePdfReport(Factory, null, recX31, strGUID, v.RecordPid, true);
                    if (System.IO.File.Exists(strTempPdfFileName))
                    {
                        //zkopírovat pdf do cílové složky
                        var strFileName = BO.Code.File.GetFileInfo(strTempPdfFileName).Name;
                        var strDir = Factory.CBL.GetGlobalParamValue("p92FolderPdf", recP91.p92ID);
                        
                        System.IO.File.Copy(strTempPdfFileName, $"{strDir}\\{strFileName}",true);

                        Factory.FBL.RunSql("exec dbo.zzz_schaffer_helios_pdf @p91id,@pdf_temp_path", new { p91id = recP91.pid, pdf_temp_path= $"{strDir}\\{strFileName}" });
                        

                    }
                }

            }


            int intB05ID = Factory.WorkflowBL.RunWorkflowStep(true, v.RecordPid, v.RecordEntity, strNotepad, intX04ID, v.RecB06, lisNominee, BO.Code.Bas.InDouble(v.current_user_lon), BO.Code.Bas.InDouble(v.current_cuser_lat), v.b05Date, v.b05Name, intPortalFlag, intTab1Flag);


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


        private void Handle_Invoice_Preview(int p91id)
        {
            var recP91 = Factory.p91InvoiceBL.Load(p91id);
            var recP92 = Factory.p92InvoiceTypeBL.Load(recP91.p92ID);
        }
    }
}
