using DocumentFormat.OpenXml.Drawing;
using Microsoft.AspNetCore.Mvc;
using System.Data.SqlTypes;
using System.Net.Http;
using System.Text;
using UI.Models.p91oper;

namespace UI.Controllers
{
    public class p91exportController : BaseController
    {
        public IActionResult Index(string p91ids, string guid_pids)
        {
            if (!string.IsNullOrEmpty(guid_pids))
            {
                p91ids = Factory.p85TempboxBL.LoadByGuid(guid_pids).p85Message; //vstupní pids předány přes p85Tempbox
            }
            var lis = BO.Code.Bas.ConvertString2ListInt(p91ids);
            if (lis.Count() == 0)
            {
                return this.StopPage(true, "Na vstupu chybí faktury.");
            }
            var v = new exportViewModel() { p91ids = p91ids };
            v.destformat = Factory.CBL.LoadUserParam("p91export-destformat", "isdoc");
            v.iszip = Factory.CBL.LoadUserParamBool("p91export-iszip", false);
            v.isjointexts = Factory.CBL.LoadUserParamBool("p91export-isjointexts", false);
            RefreshState(v);
            foreach (var c in v.lisP91)
            {
                if (c.p91IsDraft)
                {
                    return this.StopPage(true, $"{c.p91Code} je DRAFT faktura.");
                }

                if (string.IsNullOrEmpty(c.p91Client_VatID) && string.IsNullOrEmpty(c.p91Client_ICDPH_SK))
                {
                    //return this.StopPage(true, $"U faktury {c.p91Code} chybí DIČ.");
                    if (c.p28CountryCode == "SK")
                    {
                        this.AddMessageTranslated($"U faktury {c.p91Code} IČ DPH.", "warning");
                    }
                    else
                    {
                        this.AddMessageTranslated($"U faktury {c.p91Code} chybí DIČ.", "warning");
                    }
                    
                }
            }

            if (v.destformat !=null && v.lisP91.Count() > 0)
            {
                handle_generate(v);
            }

            return View(v);
        }

        private void RefreshState(exportViewModel v)
        {
            var lis = BO.Code.Bas.ConvertString2ListInt(v.p91ids);
            v.lisP91 = Factory.p91InvoiceBL.GetList(new BO.myQueryP91() { pids = lis });
            if (v.tempsubfolder == null)
            {
                v.tempsubfolder = $"p91export-{Factory.CurrentUser.j02Login}-{BO.Code.Bas.ObjectDateTime2String(DateTime.Now, "dd-MM-yyyy-HH-mm-ss-fff")}";
            }
        }


        [HttpPost]
        public IActionResult Index(exportViewModel v)
        {
            RefreshState(v);
            if (v.IsPostback)
            {
                if (v.PostbackOper== "destformat")
                {
                    Factory.CBL.SetUserParam("p91export-destformat", v.destformat);
                    handle_generate(v);
                }
                if (v.PostbackOper == "iszip")
                {
                    Factory.CBL.SetUserParam("p91export-iszip", BO.Code.Bas.GB(v.iszip));
                    handle_generate(v);
                }
                if (v.PostbackOper == "isjointexts")
                {
                    Factory.CBL.SetUserParam("p91export-isjointexts", BO.Code.Bas.GB(v.isjointexts));
                    handle_generate(v);
                }
                if (v.PostbackOper == "schaffer-run")
                {
                    handle_generate_schaffer(v);
                }
                return View(v);
            }
            if (ModelState.IsValid)
            {
                handle_generate(v);
            }


                

            return View(v);
        }

        private void handle_generate_schaffer(exportViewModel v)
        {
            foreach (var c in v.lisP91)
            {

                var recP92 = Factory.p92InvoiceTypeBL.Load(c.p92ID);
                if (recP92.x31ID_Invoice > 0)
                {
                    var recX31 = Factory.x31ReportBL.Load(recP92.x31ID_Invoice);
                    var strGUID = BO.Code.Bas.GetGuid();
                    var cc = new TheReportSupport();

                    var strTempPdfFileName = cc.GeneratePdfReport(Factory, null, recX31, strGUID, c.pid, true);
                    if (System.IO.File.Exists(strTempPdfFileName))
                    {
                        //zkopírovat pdf do cílové složky
                        var strFileName = BO.Code.File.GetFileInfo(strTempPdfFileName).Name;
                        var strDir = Factory.CBL.GetGlobalParamValue("p92FolderPdf", c.p92ID);
                        if (string.IsNullOrEmpty(strDir))
                        {
                            var recP93 = Factory.p93InvoiceHeaderBL.Load(recP92.p93ID);
                            strDir = $"\\\\SQL2\\MT_vs_SQL2\\{recP93.p93RegID}";
                        }

                        System.IO.File.Copy(strTempPdfFileName, $"{strDir}\\{strFileName}", true);

                        Factory.FBL.RunSql("exec dbo.zzz_schaffer_helios_pdf @p91id,@pdf_temp_path", new { p91id = c.pid, pdf_temp_path = $"{strDir}\\{strFileName}" });


                    }
                }
            }
            this.AddMessageTranslated($"Export dokončen ({v.lisP91.Count()}x)","info");
        }


        private void handle_generate(exportViewModel v)
        {
            v.FileNames = new List<string>();            
            //string url = null;

            if (v.tempsubfolder != null)
            {
                if (!System.IO.Directory.Exists($"{Factory.TempFolder}\\{v.tempsubfolder}"))
                {
                    System.IO.Directory.CreateDirectory($"{Factory.TempFolder}\\{v.tempsubfolder}");
                }
            }

            var httpclient = new HttpClient();
            var lisRecs = new List<BO.Integrace.InputInvoice>();
            
            foreach (var c in v.lisP91)
            {
                
                var rec = Factory.p91InvoiceBL.CreateIntegraceRecord(c);
                
                //rec.p91Code = BO.Code.File.PrepareFileName(rec.p91Code, true);
                if (v.isjointexts && !string.IsNullOrEmpty(rec.p91Text2))
                {
                    if (!string.IsNullOrEmpty(rec.p91Text1))
                    {
                        rec.p91Text1 = rec.p91Text1 + System.Environment.NewLine + rec.p91Text2;
                    }
                    else
                    {
                        rec.p91Text1 = rec.p91Text2;
                    }
                    
                }
                lisRecs.Add(rec);

         
                switch (v.destformat)
                {
                    case "isdoc":
                        var strFileName = $"{BO.Code.File.PrepareFileName(rec.p91Code, true)}.ISDOC";
                        var strISDOC = BL.Code.p91Support.GenerateIsdoc(rec, httpclient, $"{Factory.TempFolder}\\{v.tempsubfolder}", strFileName);
                        if (strISDOC != null)
                        {
                            
                            v.FileNames.Add(strFileName);
                        }
                        
                        break;
                    
                }
            }

            if (v.destformat == "pohoda")
            {
                //pro POHODu generovat celý Pack
                v.FileNames = new List<string>();

                var strPohodaPack = BL.Code.p91Support.GeneratePohodaXml(lisRecs, httpclient, $"{Factory.TempFolder}\\{v.tempsubfolder}");
                if (strPohodaPack != null)
                {
                    v.FileNames.Add($"POHODA.xml");
                }
               
            }

           

            if (v.iszip && v.lisP91.Count() > 0)
            {
                string strZipFileName = $"Export-{DateTime.Now.ToString("dd-MM-yyyy-HH-mm")}.ZIP";
                if (System.IO.File.Exists($"{Factory.TempFolder}\\{strZipFileName}"))
                {
                    System.IO.File.Delete($"{Factory.TempFolder}\\{strZipFileName}");
                }
                System.IO.Compression.ZipFile.CreateFromDirectory($"{Factory.TempFolder}\\{v.tempsubfolder}", $"{Factory.TempFolder}\\{strZipFileName}");
                v.FileNames.Clear();
                v.FileNames.Add(strZipFileName);
            }


        }
    }
}

