using BL;
using ceTe.DynamicPDF.Merger;
using Microsoft.AspNetCore.Mvc;
using UI.Models.p91oper;

namespace UI.Controllers
{
    public class p91sendmailController : BaseController
    {
        public p91sendmailController()
        {
            ceTe.DynamicPDF.Document.AddLicense("DPSPROU4223720241231Xap8Eso/OLqTQoAdWV83/EhF3keLURxFeh6eWVIsKRuL5QcYIwkKfrnldyUxzLX17t/Zdk0VJQDF/Ka6byCKNrfL/A");
        }
        public IActionResult Index(string p91ids, int alltogether)
        {
            var v = new p91sendmailViewModel() { p91ids = p91ids, Rec = new BO.x40MailQueue(), AllTogether = alltogether };
            if (BO.Code.Bas.ConvertString2ListInt(v.p91ids).Count() == 0)
            {
                return this.StopPage(true, "Na vstupu chybí faktury.");
            }
            v.IsIncludeReportAttachment = Factory.CBL.LoadUserParamBool("p91sendmail-includereportattachment", true);
            v.IsIncludeIsdoc = Factory.CBL.LoadUserParamBool("p91sendmail-IsIncludeIsdoc", false);
            v.Notepad = new Models.Notepad.EditorViewModel() { Prefix = "x40", SelectedX04ID = Factory.Lic.x04ID_Default };

            v.Rec.x40IsUserSignature = Factory.CBL.LoadUserParamBool("sendmail-isusersignature", true);
            v.SubjectFlag = Factory.CBL.LoadUserParamInt("p91sendmail-subjectflag", 1);
            v.Rec.x40SkeletonFile = Factory.CBL.LoadUserParam("p91sendmail-skeleton");

            RefreshState(v);
            if (v.lisP91.Count() == 0)
            {
                return this.StopPage(true, "Na vstupu chybí faktury.");
            }
            v.RowItems = new List<p91RowItem>();
            foreach (var c in v.lisP91)
            {
                var receivers = new List<BO.x43MailQueue_Recipient>();
                var rec = new p91RowItem() { p91ID = c.pid, RecP91 = c };
                //rec.Subject = $"Faktura č. {c.p91Code}";
                var recP28 = Factory.p28ContactBL.Load(c.p28ID);
                if (recP28.j61ID_Invoice > 0)   //klient má svojí výchozí poštovní šablonu
                {
                    rec.j61ID = recP28.j61ID_Invoice;
                }
                BO.p92InvoiceType recP92 = null;
                if (recP28.p92ID > 0)
                {
                    recP92 = Factory.p92InvoiceTypeBL.Load(recP28.p92ID);
                }
                if (c.p41ID_First > 0)
                {
                    var recP41 = Factory.p41ProjectBL.Load(c.p41ID_First);
                    if (recP41.p92ID > 0)
                    {
                        recP92 = Factory.p92InvoiceTypeBL.Load(recP41.p92ID);
                    }
                }
                if (recP92 != null)
                {
                    rec.x31ID_Invoice = recP92.x31ID_Invoice;
                    rec.x31ID_Attachment = recP92.x31ID_Attachment;
                }


                var lisO32 = Factory.p28ContactBL.GetList_o32(recP28.pid, 0, 0);
                foreach (var cc in lisO32)
                {
                    switch (cc.o33ID)
                    {
                        case BO.o33FlagEnum.Email:
                            receivers.Add(new BO.x43MailQueue_Recipient() { x43Email = cc.o32Value, x43RecipientFlag = BO.x43RecipientIdEnum.recTO }); break;
                        case BO.o33FlagEnum.EmailCC:
                            receivers.Add(new BO.x43MailQueue_Recipient() { x43Email = cc.o32Value, x43RecipientFlag = BO.x43RecipientIdEnum.recCC }); break;
                        case BO.o33FlagEnum.EmailBCC:
                            receivers.Add(new BO.x43MailQueue_Recipient() { x43Email = cc.o32Value, x43RecipientFlag = BO.x43RecipientIdEnum.recBCC }); break;
                    }
                }
                var lisP30 = Factory.p28ContactBL.GetList_p30(recP28.pid).Where(p => p.PersonInvoiceEmail != null);
                foreach (var cc in lisP30)
                {
                    receivers.Add(new BO.x43MailQueue_Recipient() { x43Email = cc.PersonInvoiceEmail, x43RecipientFlag = BO.x43RecipientIdEnum.recTO });
                }

                rec.TO = string.Join(";", receivers.Where(p => p.x43RecipientFlag == BO.x43RecipientIdEnum.recTO).Select(p => p.x43Email));
                rec.CC = string.Join(";", receivers.Where(p => p.x43RecipientFlag == BO.x43RecipientIdEnum.recCC).Select(p => p.x43Email));
                rec.BCC = string.Join(";", receivers.Where(p => p.x43RecipientFlag == BO.x43RecipientIdEnum.recBCC).Select(p => p.x43Email));

                v.RowItems.Add(rec);
            }

            return View(v);
        }

        private void RefreshState(p91sendmailViewModel v)
        {

            v.lisP91 = Factory.p91InvoiceBL.GetList(new BO.myQueryP91() { pids = BO.Code.Bas.ConvertString2ListInt(v.p91ids) });
            v.lisX31 = Factory.x31ReportBL.GetList(new BO.myQueryX31() { entity = "p91" });
            v.lisJ61 = Factory.j61TextTemplateBL.GetList(new BO.myQueryJ61() { entity = "p91" });
            if (v.RowItems != null)
            {
                foreach (var c in v.lisP91)
                {
                    v.RowItems.First(p => p.p91ID == c.pid).RecP91 = c;
                }
            }
            v.Notepad.PlaceHolder = "Do všech zpráv navíc uvést tento text (pod text zprávy)...";
        }

        [HttpPost]
        public IActionResult Index(p91sendmailViewModel v)
        {
            RefreshState(v);
            if (v.IsPostback)
            {
                if (v.PostbackOper == "IsIncludeIsdoc")
                {
                    Factory.CBL.SetUserParam("p91sendmail-IsIncludeIsdoc", v.IsIncludeIsdoc.ToString());
                }
                return View(v);
            }

            if (ModelState.IsValid)
            {
                if (v.Rec.j40ID == 0)
                {
                    this.AddMessage("Chybí vybrat odesílatele zprávy.");
                    return View(v);
                }
                int x = 1;
                foreach (var c in v.RowItems)
                {
                    if (v.AllTogether == 0 && string.IsNullOrEmpty(c.TO))
                    {
                        this.AddMessageTranslated($"Řádek #{x}: Chybí vyplnit příjemce [TO].");
                        return View(v);
                    }
                    //if (string.IsNullOrEmpty(v.Rec.x40Subject) && string.IsNullOrEmpty(c.Subject))
                    //{
                    //    this.AddMessageTranslated($"Řádek #{x}: Chybí vyplnit [Předmět zprávy].");
                    //    return View(v);
                    //}
                    if (c.x31ID_Invoice == 0 && c.x31ID_Attachment == 0)
                    {
                        this.AddMessageTranslated($"Řádek #{x}: Chybí vyplnit [Sestava faktury] nebo [Sestava přílohy].");
                        return View(v);
                    }
                    x += 1;
                }


                Factory.CBL.SetUserParam("p91sendmail-j40id", v.Rec.j40ID.ToString());
                Factory.CBL.SetUserParam("sendmail-isusersignature", BO.Code.Bas.GB(v.Rec.x40IsUserSignature));
                Factory.CBL.SetUserParam("p91sendmail-skeleton", v.Rec.x40SkeletonFile);

                var ret = new BO.Result(false);
                if (v.AllTogether == 0)
                {
                    ret = handle_send_together0(v);
                }
                if (v.AllTogether == 1)
                {
                    ret = handle_send_together1(v);
                }

                if (ret.Flag == BO.ResultEnum.Success)
                {
                    v.SetJavascript_CallOnLoad(v.Rec.pid);
                    return View(v);
                }
                else
                {
                    this.AddMessageTranslated(ret.Message, "warning");
                }
            }




            return View(v);
        }

        private BO.Result handle_send_together1(p91sendmailViewModel v)     //všechna vyúčtování poslat v jediné zprávě
        {
            var recJ40 = Factory.j40MailAccountBL.Load(v.Rec.j40ID);
            var rec = new BO.x40MailQueue() { j40ID = v.Rec.j40ID, x40SenderName = recJ40.Name, x40SenderAddress = recJ40.j40SmtpEmail, x40IsHtmlBody = true };
            rec.x40Recipient = v.Rec.x40Recipient; rec.x40CC = v.Rec.x40CC; rec.x40BCC = v.Rec.x40BCC;
            rec.x40SkeletonFile = v.Rec.x40SkeletonFile;
            rec.x40Subject = v.Rec.x40Subject;
            rec.x40Body = v.Notepad.HtmlContent;
            rec.x40IsUserSignature = v.Rec.x40IsUserSignature;

            int intP91ID_First = 0;
            if (v.RowItems.Count() > 0)
            {
                intP91ID_First = v.RowItems.First().p91ID; rec.x40RecordEntity = "p91";   //zpráva má pid první z vyúčtování, je třeba vytvořit další fake záznamy v tabulce x40
                rec.x40RecordPid = intP91ID_First;
            }


            Factory.MailBL.ClearAttachments();


            foreach (var ri in v.RowItems)
            {
                var files = new List<string>();
                string strFinalPath = $"{Factory.TempFolder}\\{ri.RecP91.p91Code}.pdf";

                if (ri.x31ID_Invoice > 0)
                {
                    var recX31 = Factory.x31ReportBL.Load(ri.x31ID_Invoice);
                    var cc = new TheReportSupport();

                    strFinalPath = cc.GeneratePdfReport(Factory, null, recX31, BO.Code.Bas.GetGuid(), ri.p91ID, true, 0, null, null, v.IsIncludeIsdoc);
                    if (strFinalPath != null)
                    {
                        files.Add(strFinalPath);
                    }
                }
                if (ri.x31ID_Attachment > 0)
                {
                    var recX31 = Factory.x31ReportBL.Load(ri.x31ID_Attachment);
                    var cc = new TheReportSupport();
                    strFinalPath = cc.GeneratePdfReport(Factory, null, recX31, BO.Code.Bas.GetGuid(), ri.p91ID, true);
                    if (strFinalPath != null)
                    {
                        files.Add(strFinalPath);

                    }

                }

                if (files.Count() > 1)
                {
                    strFinalPath = $"{Factory.TempFolder}\\{ri.RecP91.p91Code}.pdf";
                    MergeDocument doc = new MergeDocument(files[0]);
                    doc.Append(files[1]);
                    doc.Draw(strFinalPath);
                }

                if (files.Count() > 0)
                {
                    Factory.MailBL.AddAttachment(strFinalPath, $"{ri.RecP91.p91Code}.pdf");
                }




            }

            var ret = Factory.MailBL.SendMessage(rec, false, null);
            if (ret.pid > 0 && intP91ID_First > 0)
            {
                var recMaster = Factory.MailBL.LoadMessageByPid(ret.pid);
                foreach (var ri in v.RowItems.Where(p => p.p91ID != intP91ID_First))  //pro ostatní vyúčtování je třeba vytvořit fake záznam o odeslání zprávy
                {
                    var recfake = rec;
                    rec.x40RecordPid = ri.p91ID;
                    recfake.x40RecordPid = ri.p91ID;
                    recfake.x40Attachments = ri.RecP91.p91Code + " - stopa o odeslání";
                    recfake.x40ErrorMessage = recMaster.x40ErrorMessage;
                    recfake.x40Status = recMaster.x40Status;
                    recfake.x40SenderAddress = recMaster.x40SenderAddress;
                    recfake.x40SenderName = recMaster.x40SenderName;
                    recfake.x40EmlFolder = recMaster.x40EmlFolder;
                    recfake.x40EmlFileSize = recMaster.x40EmlFileSize;

                    Factory.MailBL.SaveX40(null, recfake);
                }
            }


            return ret;

        }

        private BO.Result handle_send_together0(p91sendmailViewModel v)     //každé vyúčtování odeslat samostatně
        {
            var recJ40 = Factory.j40MailAccountBL.Load(v.Rec.j40ID);
            var ret = new BO.Result(false);
            int intErrs = 0; int intOKs = 0;

            foreach (var ri in v.RowItems)
            {
                var rec = new BO.x40MailQueue() { j40ID = v.Rec.j40ID, x40SenderName = recJ40.Name, x40SenderAddress = recJ40.j40SmtpEmail, x40IsHtmlBody = true };
                rec.x40Recipient = ri.TO; rec.x40CC = ri.CC; rec.x40BCC = ri.BCC;
                rec.x40SkeletonFile = v.Rec.x40SkeletonFile;
                rec.x40RecordPid = ri.p91ID; rec.x40RecordEntity = "p91";
                rec.x40Subject = ri.Subject;
               
                if (v.SubjectFlag == 2 || v.SubjectFlag == 4)
                {
                    rec.x40Subject = v.Rec.x40Subject;  //jednotný předmět zprávy
                }

                rec.x40CC_j61 = v.Rec.x40CC_j61;
                rec.x40BCC_j61 = v.Rec.x40BCC_j61;
                rec.x40TO_j61 = v.Rec.x40TO_j61;

                int intJ61ID = ri.j61ID;
                if (intJ61ID == 0)
                {
                    intJ61ID = v.SelectedJ61ID;
                }
                if (v.SubjectFlag == 4 && v.SelectedJ61ID > 0)  //jednotná šablona zprávy
                {
                    intJ61ID = v.SelectedJ61ID;
                }

                if (intJ61ID > 0)
                {
                    var recJ61 = Factory.j61TextTemplateBL.Load(intJ61ID);
                    rec.x40Body = recJ61.j61MailBody;
                    if (v.SubjectFlag == 1)
                    {
                        rec.x40Subject = recJ61.j61MailSubject; //předmět zprávy načítat vždy z šablony zprávy
                    }
                    if (string.IsNullOrEmpty(rec.x40Subject))
                    {
                        rec.x40Subject = recJ61.j61MailSubject;
                    }
                    
                }
                if (string.IsNullOrEmpty(rec.x40Subject))
                {
                    rec.x40Subject = v.Rec.x40Subject;  //pokud i zde je předmět prázdný, načte se s výchozího předmětu zprávy
                }
                if (rec.x40Subject != null)
                {
                    rec.x40Subject = rec.x40Subject.Trim();
                }


                if (!string.IsNullOrEmpty(v.Notepad.HtmlContent))
                {
                    rec.x40Body += v.Notepad.HtmlContent;   //ještě uvést text pod zprávou
                }
                

                var dt = Factory.gridBL.GetList4MailMerge("p91", ri.p91ID);
                rec.x40Body = BO.Code.MergeContent.GetMergedContent(rec.x40Body, dt);
                rec.x40Subject = BO.Code.MergeContent.GetMergedContent(rec.x40Subject, dt);

                rec.x40IsUserSignature = v.Rec.x40IsUserSignature;

                //string strGUID = BO.Code.Bas.GetGuid();
                Factory.MailBL.ClearAttachments();
                var files = new List<string>();
                string strFinalPath = $"{Factory.TempFolder}\\{ri.RecP91.p91Code}.pdf";

                if (ri.x31ID_Invoice > 0)
                {
                    var recX31 = Factory.x31ReportBL.Load(ri.x31ID_Invoice);
                    var cc = new TheReportSupport();

                    strFinalPath = cc.GeneratePdfReport(Factory, null, recX31, BO.Code.Bas.GetGuid(), ri.p91ID, true, 0, null, null, v.IsIncludeIsdoc);
                    if (strFinalPath != null)
                    {
                        files.Add(strFinalPath);
                    }
                }
                if (ri.x31ID_Attachment > 0)
                {
                    var recX31 = Factory.x31ReportBL.Load(ri.x31ID_Attachment);
                    var cc = new TheReportSupport();
                    strFinalPath = cc.GeneratePdfReport(Factory, null, recX31, BO.Code.Bas.GetGuid(), ri.p91ID, true);
                    if (strFinalPath != null)
                    {
                        files.Add(strFinalPath);

                    }

                }

                if (files.Count() > 1)
                {
                    strFinalPath = $"{Factory.TempFolder}\\{ri.RecP91.p91Code}.pdf";
                    MergeDocument doc = new MergeDocument(files[0]);
                    doc.Append(files[1]);
                    doc.Draw(strFinalPath);
                }

                if (files.Count() > 0)
                {
                    Factory.MailBL.AddAttachment(strFinalPath, $"{ri.RecP91.p91Code}.pdf");
                }


                var xx = Factory.MailBL.SendMessage(rec, false, null);
                if (xx.Flag == BO.ResultEnum.Failed)
                {
                    ri.ErrorMessage = xx.Message;
                    intErrs += 1;
                }
                else
                {
                    intOKs += 1;
                }

            }

            Factory.CBL.SetUserParam("p91sendmail-subjectflag", v.SubjectFlag.ToString());

            if (intErrs > 0)
            {
                ret.Flag = BO.ResultEnum.Failed;
                ret.Message = $"Při odesílání zpráv došlo k chybám. Počet chyb: {intErrs}, počet odeslaných zpráv: {intOKs}";


            }

            return ret;
        }
    }
}
