using ceTe.DynamicPDF.Merger;
using ceTe.DynamicPDF.PageElements;
using Microsoft.AspNetCore.Mvc;

using UI.Models;
using UI.Models.Record;
using UI.Models.Tab1;

namespace UI.Controllers
{
    public class p91Controller : BaseController
    {
        public IActionResult PdfPreview(int pid)
        {
            var recP91 = Factory.p91InvoiceBL.Load(pid);
            var recP92 = Factory.p92InvoiceTypeBL.Load(recP91.p92ID);

            var crep = new TheReportSupport();
            
           
            var files = new List<string>();

            if (recP92.x31ID_Invoice > 0)
            {
                var recX31 = Factory.x31ReportBL.Load(recP92.x31ID_Invoice);
                var strPath = crep.GeneratePdfReport(Factory, null, recX31, BO.Code.Bas.GetGuid(), pid, true);
                BO.Code.File.LogInfo("pdf1: " + strPath);
                if (strPath != null)
                {
                    files.Add(strPath);
                }             
            }
            if (recP92.x31ID_Attachment > 0)
            {
                var recX31 = Factory.x31ReportBL.Load(recP92.x31ID_Attachment);
                var strPath = crep.GeneratePdfReport(Factory, null, recX31, BO.Code.Bas.GetGuid(), pid, true);
                BO.Code.File.LogInfo("pdf2: " + strPath);
                if (strPath != null)
                {
                    files.Add(strPath);
                }
            }

            if (files.Count() == 0)
            {
                return this.StopPageSubform("U faktury se bohužel nepodařilo vygenerovat PDF dokumenty doklad a přílohy.");
            }
            
            if (files.Count()==1)
            {
                var pdfBytes = BO.Code.File.LoadFilefAsBytes(files[0]);
                return File(pdfBytes, "application/pdf");
            }
            

            ceTe.DynamicPDF.Document.AddLicense("DPSPROU4223720241231Xap8Eso/OLqTQoAdWV83/EhF3keLURxFeh6eWVIsKRuL5QcYIwkKfrnldyUxzLX17t/Zdk0VJQDF/Ka6byCKNrfL/A");

            MergeDocument doc = new MergeDocument(files[0]);
            doc.Append(files[1]);

            BO.Code.File.LogInfo(doc.EmbeddedFiles.ToString());

            return File(doc.Draw(), "application/pdf");

            //doc.Draw($"{Factory.TempFolder}\\{strUploadGuid}_{strFinalRepFileName}");

        }
        public IActionResult Info(int pid)
        {
            return Tab1(pid,"info");
        }
        public IActionResult Tab1(int pid,string caller)
        {
            var v = new p91Tab1() { Factory = this.Factory, prefix = "p91", pid = pid,caller=caller };
            
            v.Rec = Factory.p91InvoiceBL.Load(v.pid);
            if (v.Rec != null)
            {
                //v.RecSum = Factory.p28ContactBL.LoadSumRow(v.Rec.pid);
                
                
                v.SetTagging();
               
                v.SetFreeFields(0);

                //if (v.Rec.p92TypeFlag == BO.p92TypeFlagENUM.CreditNote)
                //{
                //    v.RecOpravovanyDoklad = Factory.p91InvoiceBL.Load(v.Rec.p91ID_CreditNoteBind);
                //}

                

            }
            return View(v);
        }

        public IActionResult Dashboard(int pid)
        {
            var v = new p91Tab1() { Factory = this.Factory, prefix = "p91", pid = pid, caller = "Dashboard" };
            v.Rec = Factory.p91InvoiceBL.Load(v.pid);
            if (v.Rec != null)
            {
                v.SetTagging();

                v.SetFreeFields(0);
            }
            return View(v);
        }


        public IActionResult Record(int pid)
        {            
            var v = new p91Record() { rec_pid = pid, rec_entity = "p91", ComboOwner = Factory.CurrentUser.FullnameDesc, UploadGuid = BO.Code.Bas.GetGuid() };
            if (v.rec_pid == 0)
            {
                return this.StopPage(true, "Na vstupu chybí ID vyúčtování.");
            }
            v.disp = new DispoziceViewModel();
            v.disp.InitItems("p91", Factory);

            
            v.Rec = Factory.p91InvoiceBL.Load(v.rec_pid);
            if (v.Rec == null)
            {
                return RecNotFound(v);
            }
            if (BO.Code.Bas.bit_compare_or(v.Rec.p91LockFlag, 2)) v.Isp91LockFlag2 = true;
            if (BO.Code.Bas.bit_compare_or(v.Rec.p91LockFlag, 4)) v.Isp91LockFlag4 = true;
            if (BO.Code.Bas.bit_compare_or(v.Rec.p91LockFlag, 8)) v.Isp91LockFlag8 = true;


            v.SetTagging(Factory.o51TagBL.GetTagging("p91", v.rec_pid));
            
            v.ComboP28Name = v.Rec.p28Name;
            v.ComboOwner = v.Rec.Owner;
            v.RecP92 = Factory.p92InvoiceTypeBL.Load(v.Rec.p92ID);
            v.ComboP92Name = v.Rec.p92Name;

            if (v.Rec.p98ID > 0)
            {
                v.ComboP98Name = Factory.p98Invoice_Round_Setting_TemplateBL.Load(v.Rec.p98ID).p98Name;
            }
            if (v.Rec.p63ID > 0)
            {
                v.ComboP63Name = Factory.p63OverheadBL.Load(v.Rec.p63ID).p63Name;
            }
            if (v.Rec.p80ID > 0)
            {
                v.ComboP80Name = Factory.p80InvoiceAmountStructureBL.Load(v.Rec.p80ID).p80Name;
            }
            if (v.Rec.j19ID > 0)
            {
                v.ComboJ19Name = Factory.FBL.LoadJ19(v.Rec.j19ID).j19Name;
            }

            InhaleDisp(v, v.Rec.p91BitStream);
           
            

            RefreshStateRecord(v);
            if (!InhalePermissions(v))
            {
                return this.StopPage(true, "Nemáte oprávnění k editaci karty záznamu.");
            }
            else
            {
                //ještě ověřit, zda nejsou nějaké úkony vyúčtování v archivu.
                if (Factory.p91InvoiceBL.HasDeletedP31Records(v.rec_pid))
                {
                    return this.StopPage(true, "Toto vyúčtování nelze upravovat, protože miminálně jeden jeho úkon byl přesunutý do archivu.");
                }
                
            }

            v.Toolbar = new MyToolbarViewModel(v.Rec) { AllowClone = false };
            
            return View(v);
        }

        private void RefreshStateRecord(p91Record v)
        {
            if (v.RecP92 == null)
            {
                v.RecP92 = Factory.p92InvoiceTypeBL.Load(v.Rec.p92ID);
            }
            if (v.ff1 == null)
            {
                v.ff1 = new FreeFieldsViewModel();
                v.ff1.InhaleFreeFieldsView(Factory, v.rec_pid, "p91");
            }
            v.ff1.RefreshInputsVisibility(Factory, v.rec_pid, "p91", v.Rec.p92ID);

            if (!v.disp.IsInhaled)
            {
                InhaleDisp(v, v.Rec.p91BitStream);
            }
            

            InhaleRoles(v);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Record(p91Record v)
        {            
            RefreshStateRecord(v);
            if (v.IsPostback)
            {
                
                return View(v);
            }
            
            
            
            if (ModelState.IsValid)
            {
                BO.p91Invoice c = Factory.p91InvoiceBL.Load(v.rec_pid);                               
                c.p91Date = v.Rec.p91Date;
                c.p91DateMaturity = v.Rec.p91DateMaturity;
                c.p91DateSupply = v.Rec.p91DateSupply;
                c.p91Datep31_From = v.Rec.p91Datep31_From;
                c.p91Datep31_Until = v.Rec.p91Datep31_Until;

                c.p92ID = v.Rec.p92ID;
                c.j19ID = v.Rec.j19ID;
                c.p80ID = v.Rec.p80ID;
                c.p63ID = v.Rec.p63ID;
                c.p98ID = v.Rec.p98ID;
                
                c.p91Text1 = v.Rec.p91Text1;               
                c.p91Text2 = v.Rec.p91Text2;

                c.p28ID = v.Rec.p28ID;
               

                c.ValidUntil = v.Toolbar.GetValidUntil(c);
                c.ValidFrom = v.Toolbar.GetValidFrom(c);
                c.j02ID_Owner = v.Rec.j02ID_Owner;

                c.p91LockFlag = 0;
                if (v.Isp91LockFlag2) c.p91LockFlag += 2;
                if (v.Isp91LockFlag4) c.p91LockFlag += 4;
                if (v.Isp91LockFlag8) c.p91LockFlag += 8;

                c.p91Client = v.Rec.p91Client;
                c.p91Client_RegID = v.Rec.p91Client_RegID;
                c.p91Client_VatID = v.Rec.p91Client_VatID;
                c.p91Client_ICDPH_SK = v.Rec.p91Client_ICDPH_SK;
                c.p91ClientAddress1_City = v.Rec.p91ClientAddress1_City;
                c.p91ClientAddress1_Street = v.Rec.p91ClientAddress1_Street;
                c.p91ClientAddress1_ZIP = v.Rec.p91ClientAddress1_ZIP;
                c.p91ClientAddress1_Country = v.Rec.p91ClientAddress1_Country;
                c.p91ClientAddress1_Before = v.Rec.p91ClientAddress1_Before;

                c.p91Supplier = v.Rec.p91Supplier;
                c.p91Supplier_RegID = v.Rec.p91Supplier_RegID;
                c.p91Supplier_VatID = v.Rec.p91Supplier_VatID;
                c.p91Supplier_ICDPH_SK = v.Rec.p91Supplier_ICDPH_SK;
                c.p91Supplier_City = v.Rec.p91Supplier_City;
                c.p91Supplier_Street = v.Rec.p91Supplier_Street;
                c.p91Supplier_ZIP = v.Rec.p91Supplier_ZIP;
                c.p91Supplier_Country = v.Rec.p91Supplier_Country;
                c.p91Supplier_Registration = v.Rec.p91Supplier_Registration;

              
                c.p91BitStream = v.disp.GetBitStream();
                c.p91PortalFlag = v.Rec.p91PortalFlag;
                c.p91VatCodePohoda = v.Rec.p91VatCodePohoda;



                c.pid = Factory.p91InvoiceBL.Update(c, v.ff1.inputs, (v.roles !=null? v.roles.getList4Save(Factory):null));
                if (c.pid > 0)
                {
                    Factory.o51TagBL.SaveTagging("p91", c.pid, v.TagPids);
                                       
                    
                    if (v.disp.IsFiles)
                    {
                        Factory.o27AttachmentBL.SaveChangesAndUpload(v.UploadGuid, "p91", c.pid);
                    }

                    

                    v.SetJavascript_CallOnLoad(c.pid);
                    return View(v);
                }

            }


            this.Notify_RecNotSaved();
            return View(v);
        }




        private bool InhalePermissions(p91Record v)
        {
            var mydisp = Factory.p91InvoiceBL.InhaleRecDisposition(v.Rec.pid,v.Rec);
            
            if (!mydisp.OwnerAccess)
            {
                return false;
            }
            if (!v.Rec.p91IsDraft)
            {
                if (v.RecP92.x38ID > 0)
                {
                    v.CanEditRecordCode = Factory.x38CodeLogicBL.CanEditRecordCode(v.RecP92.x38ID, mydisp);
                }
                else
                {
                    v.CanEditRecordCode = mydisp.OwnerAccess;
                }
            }
            
            return true;
        }

        
        public BO.p28Contact LoadClientProfile(int p28id)
        {
            return Factory.p28ContactBL.Load(p28id);
            
            
        }

        public BO.p93InvoiceHeader LoadSupplierProfile(int p93id)
        {
            return Factory.p93InvoiceHeaderBL.Load(p93id);
        }

        private void InhaleDisp(p91Record v, int bitstream)
        {

            if (v.RecP92== null) return;
            //int intCache = (v.rec_pid == 0 ? Factory.j02UserBL.LoadBitstreamFromUserCache("p91", v.RecP92.pid) : 0);    //pro nový záznam načíst uložená rozšíření z cache
            int intCache = 0;   //cache nepoužívat

            
            v.disp.SetVal(PosEnum.Files, v.RecP92.p92FilesTab, bitstream, v.rec_pid, intCache);            
            v.disp.SetVal(PosEnum.Roles, v.RecP92.p92RolesTab, bitstream, v.rec_pid, intCache);
          

        }
        private void InhaleRoles(p91Record v)
        {
            if (v.disp.IsRoles && v.roles == null)
            {
                v.roles = new RoleAssignViewModel() { RecPid = v.rec_pid, RecPrefix = "p91", RolePrefix = "p91", Header = "Obsazení rolí v záznamu Vyúčtovní" };
            }

        }

        public string GetText1(int p91id)
        {
            string s = this.Factory.p91InvoiceBL.Load(p91id).p91Text1;
            if (string.IsNullOrEmpty(s))
            {
                s = "";
            }
            return s;
        }
        public string GetText2(int p91id)
        {
            string s= this.Factory.p91InvoiceBL.Load(p91id).p91Text2;
            if (string.IsNullOrEmpty(s))
            {
                s = "";
            }
            return s;
        }
    }
}
