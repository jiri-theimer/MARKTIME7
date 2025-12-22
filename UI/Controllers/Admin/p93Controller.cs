using DocumentFormat.OpenXml.Office2016.Drawing.Charts;
using Microsoft.AspNetCore.Mvc;
using UI.Models;
using UI.Models.Record;
using UI.Models.Tab1;

namespace UI.Controllers
{
    public class p93Controller : BaseController
    {
        public IActionResult Info(int pid)
        {
            var v = new BaseTab1ViewModel() { prefix = "p93", pid = pid };
            return View(v);
        }
        public IActionResult Record(int pid, bool isclone)
        {
            var v = new p93Record() { rec_pid = pid, rec_entity = "p93", UploadGuidLogo=BO.Code.Bas.GetGuid(),UploadGuidSignature=BO.Code.Bas.GetGuid(),UploadGuidPfx=BO.Code.Bas.GetGuid() };
            v.Rec = new BO.p93InvoiceHeader() { p93CountryCode=Factory.Lic.x01CountryCode};
            if (v.rec_pid > 0)
            {
                v.Rec = Factory.p93InvoiceHeaderBL.Load(v.rec_pid);
                if (v.Rec == null)
                {
                    return RecNotFound(v);
                }

                if (v.Rec.p93LogoFile != null)
                {                    
                    v.LogoFile = $"/_users/{Factory.Lic.x01Guid}/PLUGINS/{v.Rec.p93LogoFile}";
                }
                if (v.Rec.p93SignatureFile != null)
                {                    
                    v.SignatureFile = $"/_users/{Factory.Lic.x01Guid}/PLUGINS/{v.Rec.p93SignatureFile}";
                }
                if (v.Rec.p93PfxCertificate != null)
                {
                    v.PfxFile = $"/_users/{Factory.Lic.x01Guid}/PLUGINS/{v.Rec.p93PfxCertificate}";
                }
                var lis = Factory.p93InvoiceHeaderBL.GetList_p88(v.rec_pid).ToList();
                v.lisP88 = new List<p88Repeater>();
                foreach(var c in lis)
                {
                    var recP86 = Factory.p86BankAccountBL.Load(c.p86ID);
                    v.lisP88.Add(new p88Repeater() {
                        TempGuid=BO.Code.Bas.GetGuid(),j27ID=c.j27ID,p86ID=c.p86ID
                        ,ComboJ27=Factory.FBL.LoadCurrencyByID(c.j27ID).j27Code
                        ,ComboP86=recP86.p86Account+"/"+recP86.p86Code+" ("+recP86.p86Name+")"
                    });
                }
            }
            RefreshState(v);
            v.Toolbar = new MyToolbarViewModel(v.Rec);
            if (isclone)
            {
                v.MakeClone();
                v.PfxFile = null;
            }

            return ViewTup(v, BO.PermValEnum.GR_Admin);
        }

        private void RefreshState(p93Record v)
        {
            if (v.lisP88 == null)
            {
                v.lisP88 = new List<p88Repeater>();
            }

        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Record(p93Record v,string guid)
        {
           
            RefreshState(v);
            if (v.IsPostback)
            {
                if (v.PostbackOper == "recalc")
                {
                    if (v.recalc_d1==null || v.recalc_d2 == null)
                    {
                        this.AddMessage("Na vstupu chybí časové období přepočtu.");
                        return View(v);
                    }
                    var lisP91 = Factory.p91InvoiceBL.GetList(new BO.myQueryP91() {p93id=v.rec_pid, period_field = "p91DateSupply", global_d1 = v.recalc_d1, global_d2 = v.recalc_d2 });
                    foreach(var c in lisP91)
                    {
                        c.p91Supplier = v.Rec.p93Company;
                        c.p91Supplier_RegID = v.Rec.p93RegID;
                        c.p91Supplier_VatID = v.Rec.p93VatID;
                        c.p91Supplier_ICDPH_SK = v.Rec.p93ICDPH_SK;
                        c.p91Supplier_City = v.Rec.p93City;
                        c.p91Supplier_Street = v.Rec.p93Street;
                        c.p91Supplier_ZIP = v.Rec.p93Zip;
                        c.p91Supplier_Country = v.Rec.p93Country;
                        c.p91Supplier_Registration = v.Rec.p93Registration;
                        

                        Factory.p91InvoiceBL.Update(c, null, null);
                    }
                    this.AddMessageTranslated($"Počet aktualizovaných dokladů: {lisP91.Count()}.", "info");
                    return View(v);
                }
                if (v.PostbackOper == "postback")
                {
                    return View(v);
                }
                if (v.PostbackOper == "add_row")
                {
                    var c = new p88Repeater() { TempGuid = BO.Code.Bas.GetGuid() };
                    v.lisP88.Add(c);
                    
                }
                if (v.PostbackOper == "delete_row")
                {
                    v.lisP88.First(p => p.TempGuid == guid).IsTempDeleted = true;
                    
                }


                if (v.PostbackOper == "delete_logo")
                {
                    v.UploadGuidLogo = BO.Code.Bas.GetGuid();
                    v.IsDeleteLogo = true;
                    this.AddMessage("Změny se potvrdí až po uložení záznamu.", "info");
                    
                }
                if (v.PostbackOper == "delete_signature")
                {
                    v.UploadGuidSignature = BO.Code.Bas.GetGuid();
                    v.IsDeleteSignature = true;
                    this.AddMessage("Změny se potvrdí až po uložení záznamu.", "info");
                    
                }
                if (v.PostbackOper== "delete_pfx")
                {
                    v.IsDeletePfx = true;
                    this.AddMessage("Změny se potvrdí až po uložení záznamu.", "info");
                }
                return View(v);
            }
            
            if (ModelState.IsValid)
            {
                
                BO.p93InvoiceHeader c = new BO.p93InvoiceHeader();
                if (v.rec_pid > 0) c = Factory.p93InvoiceHeaderBL.Load(v.rec_pid);
                c.p93Name = v.Rec.p93Name;
                c.p93Company = v.Rec.p93Company;
                c.p93City = v.Rec.p93City;
                c.p93Street = v.Rec.p93Street;
                c.p93Zip = v.Rec.p93Zip;
                c.p93RegID = v.Rec.p93RegID;
                c.p93VatID = v.Rec.p93VatID;
                c.p93Contact = v.Rec.p93Contact;
                c.p93Registration = v.Rec.p93Registration;
                c.p93Referent = v.Rec.p93Referent;
                c.p93Signature = v.Rec.p93Signature;
                c.p93FreeText01 = v.Rec.p93FreeText01;
                c.p93FreeText02 = v.Rec.p93FreeText02;
                c.p93FreeText03 = v.Rec.p93FreeText03;
                c.p93FreeText04 = v.Rec.p93FreeText04;
                
                c.p93Country = v.Rec.p93Country;
                c.p93CountryCode = v.Rec.p93CountryCode;
                c.p93Email = v.Rec.p93Email;
                c.p93ICDPH_SK = v.Rec.p93ICDPH_SK;
                c.j27ID_Domestic = v.Rec.j27ID_Domestic;

                if (!string.IsNullOrEmpty(v.PfxPassword))
                {
                    c.p93PfxPassword = v.PfxPassword;
                }
                
                c.ValidUntil = v.Toolbar.GetValidUntil(c);
                c.ValidFrom = v.Toolbar.GetValidFrom(c);
                if (v.IsDeleteLogo) c.p93LogoFile = null;
                if (v.IsDeleteSignature) c.p93SignatureFile = null;
                if (v.IsDeletePfx)
                {
                    c.p93PfxCertificate = null;                    
                }

                var lis = new List<BO.p88InvoiceHeader_BankAccount>();
                foreach (var row in v.lisP88.Where(p => p.IsTempDeleted == false))
                {
                    var cc = new BO.p88InvoiceHeader_BankAccount() { j27ID = row.j27ID,p86ID=row.p86ID };
                    lis.Add(cc);
                }

                c.pid = Factory.p93InvoiceHeaderBL.Save(c,lis);
                if (c.pid > 0)
                {
                    if (Factory.o27AttachmentBL.GetTempFiles(v.UploadGuidLogo).Count() > 0)
                    {
                        var tempfile = Factory.o27AttachmentBL.GetTempFiles(v.UploadGuidLogo).First();
                        var strOrigFileName = "p93_logo_" + c.pid.ToString() + "_original"+ tempfile.o27FileExtension;
                        System.IO.File.Copy(tempfile.FullPath,Factory.PluginsFolder+ "\\" + strOrigFileName, true);

                        var strDestFileName = "p93_logo_" + c.pid.ToString() + tempfile.o27FileExtension;
                        if (Factory.App.HostingMode !=BL.Singleton.HostingModeEnum.None)
                        {
                            strDestFileName = BO.Code.Bas.ParseDbNameFromCloudLogin(Factory.CurrentUser.j02Login) + "_" + strDestFileName;
                        }
                        Code.basUI.ResizeImage(tempfile.FullPath,Factory.PluginsFolder+ "\\" + strDestFileName, 250, 100);

                        c = Factory.p93InvoiceHeaderBL.Load(c.pid);
                        c.p93LogoFile = strDestFileName;
                        Factory.p93InvoiceHeaderBL.Save(c, null);
                    }
                    
                    if (Factory.o27AttachmentBL.GetTempFiles(v.UploadGuidSignature).Count() > 0)
                    {
                        var tempfile = Factory.o27AttachmentBL.GetTempFiles(v.UploadGuidSignature).First();
                        var strOrigFileName = "p93_signature_" + c.pid.ToString() + "_original"+tempfile.o27FileExtension;
                        System.IO.File.Copy(tempfile.FullPath,Factory.PluginsFolder+ "\\" + strOrigFileName, true);
                        
                        var strDestFileName = "p93_signature_" + c.pid.ToString() + tempfile.o27FileExtension;
                        if (Factory.App.HostingMode !=BL.Singleton.HostingModeEnum.None)
                        {
                            strDestFileName = BO.Code.Bas.ParseDbNameFromCloudLogin(Factory.CurrentUser.j02Login) + "_" + strDestFileName;
                        }
                        Code.basUI.ResizeImage(tempfile.FullPath,Factory.PluginsFolder+ "\\" + strDestFileName, 300, 130);

                        c = Factory.p93InvoiceHeaderBL.Load(c.pid);
                        c.p93SignatureFile = strDestFileName;
                        Factory.p93InvoiceHeaderBL.Save(c, null);
                    }
                    if (Factory.o27AttachmentBL.GetTempFiles(v.UploadGuidPfx).Count() > 0)
                    {
                        var tempfile = Factory.o27AttachmentBL.GetTempFiles(v.UploadGuidPfx).First();
                        var strDestFileName = $"p93_certificate_{c.pid}.pfx";
                        System.IO.File.Copy(tempfile.FullPath,$"{Factory.PluginsFolder}\\{strDestFileName}", true);
                        
                        c = Factory.p93InvoiceHeaderBL.Load(c.pid);
                        c.p93PfxCertificate = strDestFileName;
                        Factory.p93InvoiceHeaderBL.Save(c, null);
                    }

                    v.SetJavascript_CallOnLoad(c.pid);
                    return View(v);
                }


                    
                

            }


            this.Notify_RecNotSaved();
            return View(v);
        }
    }
}
