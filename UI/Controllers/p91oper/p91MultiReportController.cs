using ceTe.DynamicPDF.Merger;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel;
using UI.Models.p91oper;

namespace UI.Controllers.p91oper
{
    public class p91MultiReportController : BaseController
    {
        private readonly BL.Singleton.ThePeriodProvider _pp;

        public p91MultiReportController(BL.Singleton.ThePeriodProvider pp)
        {
            _pp = pp;            
            ceTe.DynamicPDF.Document.AddLicense("DPSPROU4223720241231Xap8Eso/OLqTQoAdWV83/EhF3keLURxFeh6eWVIsKRuL5QcYIwkKfrnldyUxzLX17t/Zdk0VJQDF/Ka6byCKNrfL/A");
        }
        public IActionResult Index()
        {
            var v = new p91MultiReportViewModel() { UploadGuidLogo = BO.Code.Bas.GetGuid(),DraftQuery=1 };

            return ViewTup(v, BO.PermValEnum.GR_P91_Owner);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Index(p91MultiReportViewModel v)
        {

            if (ModelState.IsValid)
            {
                if (v.IsPostback)
                {
                    if (v.PostbackOper == "p93id")
                    {
                        var rec = Factory.p93InvoiceHeaderBL.Load(v.p93ID);
                        v.p91Supplier = rec.p93Company;
                        v.p91Supplier_RegID = rec.p93RegID;
                        v.p91Supplier_VatID = rec.p93VatID;
                        v.p91Supplier_ICDPH_SK = rec.p93ICDPH_SK;
                        v.p91Supplier_City = rec.p93City;
                        v.p91Supplier_Street = rec.p93Street;
                        v.p91Supplier_ZIP = rec.p93Zip;
                        v.p91Supplier_Country = rec.p93Country;

                        v.p93Contact = rec.p93Contact;
                        v.p93Email = rec.p93Email;
                        v.p93Referent = rec.p93Referent;
                        v.p93Signature = rec.p93Signature;
                    }
                    return View(v);
                }

                if (v.p93ID == 0 || v.d1==null || v.d2==null || v.x31ID==0)
                {
                    this.AddMessageTranslated("Na vstupu chybí povinné atributy.");
                    return View(v);
                }
                var mq = new BO.myQueryP91() {p93id=v.p93ID, global_d1 = v.d1, global_d2 = v.d2, period_field = "p91DateSupply" };
                
                var lisP91 = Factory.p91InvoiceBL.GetList(mq);
                if (v.DraftQuery == 1)
                {
                    lisP91 = lisP91.Where(p => p.p91IsDraft == false);
                }
                if (v.DraftQuery == 2)
                {
                    lisP91 = lisP91.Where(p => p.p91IsDraft == true);
                }
                if (lisP91.Count() == 0)
                {
                    this.AddMessageTranslated("Vstupním podmínkám nevyhovuje ani jedno vyúčtování.");
                    return View(v);
                }

                var recP93 = Factory.p93InvoiceHeaderBL.Load(v.p93ID);
                string strLogoUschova = recP93.p93LogoFile;
                string strUschovaGuid = BO.Code.Bas.GetGuid();
                
                if (Factory.o27AttachmentBL.GetTempFiles(v.UploadGuidLogo).Count() > 0)
                {
                    v.IsChangeLogo = true;
                    if (strLogoUschova != null)
                    {
                        System.IO.File.Copy(Factory.PluginsFolder + "\\" + strLogoUschova, Factory.TempFolder + "\\" + strUschovaGuid + "-"+ strLogoUschova, true);
                    }
                    Handle_Change_Logo(v);
                }

                

                var recX31 = Factory.x31ReportBL.Load(v.x31ID);
                var crs = new TheReportSupport();
                MergeDocument doc = new MergeDocument();
                string strGUID = BO.Code.Bas.GetGuid();
                int x = 0;
                foreach (var c in lisP91)
                {
                    x += 1;

                    Factory.p91InvoiceBL.Update_Temp_MultiReport(c.pid, v.p91Supplier, v.p91Supplier_RegID, v.p91Supplier_VatID, v.p91Supplier_Street, v.p91Supplier_City, v.p91Supplier_ZIP, v.p91Supplier_Country, v.p91Supplier_Registration, v.p91Supplier_ICDPH_SK,v.p93ID,v.p93Contact,v.p93Email,v.p93Referent,v.p93Signature);

                    string strPdfFullPath = crs.GeneratePdfReport(Factory, _pp, recX31, x.ToString() + "-" + strGUID, c.pid, true);
                    doc.Append(strPdfFullPath);

                    Factory.p91InvoiceBL.Update_Temp_MultiReport(c.pid, c.p91Supplier, c.p91Supplier_RegID, c.p91Supplier_VatID, c.p91Supplier_Street, c.p91Supplier_City, c.p91Supplier_ZIP, c.p91Supplier_Country, c.p91Supplier_Registration, c.p91Supplier_ICDPH_SK,v.p93ID,recP93.p93Contact,recP93.p93Email,recP93.p93Referent,recP93.p93Signature);

                }

                doc.Draw($"{Factory.TempFolder}\\MultiReport_{strGUID}.pdf");
                v.OutputFileName = $"MultiReport_{strGUID}.pdf";

                if (v.IsChangeLogo)
                {
                    if (strLogoUschova != null)
                    {
                        System.IO.File.Copy(Factory.TempFolder + "\\" + strUschovaGuid + "-"+ strLogoUschova, Factory.PluginsFolder + "\\" + strLogoUschova, true);
                    }                    
                    recP93.p93LogoFile = strLogoUschova;
                    Factory.p93InvoiceHeaderBL.Save(recP93,null);
                }

                this.AddMessageTranslated($"Počet vyúčtování: {lisP91.Count()}", "info");
                this.AddMessageTranslated("Generování dokončeno.", "info");

                return View(v);

            }


            
            return View(v);
        }


        private void Handle_Change_Logo(p91MultiReportViewModel v)
        {                        
            var recP93 = Factory.p93InvoiceHeaderBL.Load(v.p93ID);

            var tempfile = Factory.o27AttachmentBL.GetTempFiles(v.UploadGuidLogo).First();
            var strOrigFileName = "p93_logo_" + recP93.pid.ToString() + "_original" + tempfile.o27FileExtension;

            System.IO.File.Copy(tempfile.FullPath, Factory.PluginsFolder + "\\" + strOrigFileName, true);

            var strDestFileName = "p93_logo_" + recP93.pid.ToString() + tempfile.o27FileExtension;
            if (Factory.App.HostingMode != BL.Singleton.HostingModeEnum.None)
            {
                strDestFileName = BO.Code.Bas.ParseDbNameFromCloudLogin(Factory.CurrentUser.j02Login) + "_" + strDestFileName;
            }
            Code.basUI.ResizeImage(tempfile.FullPath, Factory.PluginsFolder + "\\" + strDestFileName, 250, 100);


            recP93.p93LogoFile = strDestFileName;
            Factory.p93InvoiceHeaderBL.Save(recP93, null);
        }
    }
}
