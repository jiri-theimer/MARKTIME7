using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using System.IO.Packaging;
using UI.Models;
using UI.Views.Shared.Components.myPeriod;
using ceTe.DynamicPDF.Merger;
using BL;
using ceTe.DynamicPDF.Forms;
using System.IO.Compression;


namespace UI.Controllers
{
    public class ReportsClientController : BaseController
    {
        private readonly BL.Singleton.ThePeriodProvider _pp;
        private TheReportSupport _basRepSup;

        public ReportsClientController(BL.Singleton.ThePeriodProvider pp)
        {
            _pp = pp;
            _basRepSup = new TheReportSupport();
            ceTe.DynamicPDF.Document.AddLicense("DPSPROU4223720241231Xap8Eso/OLqTQoAdWV83/EhF3keLURxFeh6eWVIsKRuL5QcYIwkKfrnldyUxzLX17t/Zdk0VJQDF/Ka6byCKNrfL/A");
        }


        public IActionResult ReportNoContextFramework(int x31id, string code, string caller_prefix, string caller_pids, string guid_pids)
        {
            if (x31id == 0 && !string.IsNullOrEmpty(code))
            {
                x31id = Factory.x31ReportBL.LoadByCode(code, 0).pid;
            }
            var v = new ReportNoContextFrameworkViewModel() { caller_prefix = caller_prefix, caller_pids = caller_pids };
            if (!string.IsNullOrEmpty(guid_pids))
            {
                v.caller_pids = Factory.p85TempboxBL.LoadByGuid(guid_pids).p85Message; //vstupní pids předány přes p85Tempbox
            }

            if (!Factory.CurrentUser.j04IsModule_x31)
            {
                return this.StopPage(false, "Nemáte oprávnění pro tuto stránku.");
            }
            if (x31id == 0)
            {
                x31id = Factory.CBL.LoadUserParamInt("x31/last-reportnocontext-x31id");
            }
            if (x31id > 0)
            {
                v.SelectedReport = Factory.x31ReportBL.Load(x31id);
            }
            v.lisX31 = Factory.x31ReportBL.GetList(new BO.myQueryX31()).Where(p => p.x31Entity == null);
            switch (v.caller_prefix)
            {
                case "p41":
                case "le5":
                    v.lisX31 = v.lisX31.Where(p => p.x31QueryFlag == BO.x31QueryFlagENUM.p41 || p.x31QueryFlag == BO.x31QueryFlagENUM.p31 || p.x31QueryFlag == BO.x31QueryFlagENUM.p91); break;
                case "p28":
                    v.lisX31 = v.lisX31.Where(p => p.x31QueryFlag == BO.x31QueryFlagENUM.p28 || p.x31QueryFlag == BO.x31QueryFlagENUM.p41 || p.x31QueryFlag == BO.x31QueryFlagENUM.p31 || p.x31QueryFlag == BO.x31QueryFlagENUM.p91); break;
                case "p56":
                    v.lisX31 = v.lisX31.Where(p => p.x31QueryFlag == BO.x31QueryFlagENUM.p56 || p.x31QueryFlag == BO.x31QueryFlagENUM.p41 || p.x31QueryFlag == BO.x31QueryFlagENUM.p31 || p.x31QueryFlag == BO.x31QueryFlagENUM.p91); break;
                case "p91":
                    v.lisX31 = v.lisX31.Where(p => p.x31QueryFlag == BO.x31QueryFlagENUM.p91 || p.x31QueryFlag == BO.x31QueryFlagENUM.p31); break;
                case "j02":
                    v.lisX31 = v.lisX31.Where(p => p.x31QueryFlag == BO.x31QueryFlagENUM.j02 || p.x31QueryFlag == BO.x31QueryFlagENUM.p31 || p.x31QueryFlag == BO.x31QueryFlagENUM.p91); break;
                case "p31":
                    v.lisX31 = v.lisX31.Where(p => p.x31QueryFlag == BO.x31QueryFlagENUM.p31); break;
            }

            var qry = v.lisX31.Select(p => new { p.j25ID, p.j25Name, p.j25Ordinary }).Distinct();
            v.lisJ25 = new List<BO.j25ReportCategory>();
            foreach (var c in qry)
            {
                var cc = new BO.j25ReportCategory() { pid = c.j25ID, j25Name = c.j25Name, j25Ordinary = c.j25Ordinary, j25Code = "accordion-button collapsed py-2" };
                if (cc.j25Name == null)
                {
                    cc.j25Ordinary = -999999;
                    cc.j25Name = Factory.tra("Bez kategorie");
                }
                if (v.SelectedReport != null && c.j25ID == v.SelectedReport.j25ID)
                {
                    cc.j25Code = "accordion-button py-2";
                }
                cc.j25Name += " (" + v.lisX31.Where(p => p.j25ID == cc.pid).Count().ToString() + ")";
                v.lisJ25.Add(cc);
            }
            return View(v);

        }

        public IActionResult ReportNoContextXls(int x31id, string code)
        {
            var v = new ReportNoContextViewModel();
            if (x31id == 0 && !string.IsNullOrEmpty(code))
            {
                x31id = Factory.x31ReportBL.LoadByCode(code, 0).pid;
            }
            v.SelectedX31ID = x31id;
            RefreshStateReportNoContext(v);

            v.IsConfirmNeeded = v.RecX31.x31IsPeriodRequired;


            return View(v);
        }
        [HttpPost]
        public IActionResult ReportNoContextXls(ReportNoContextViewModel v, string oper, string format)
        {
            RefreshStateReportNoContext(v);
            if (oper == "confirm")
            {
                v.IsConfirmed = true;
            }

            if (ModelState.IsValid)
            {
                if (string.IsNullOrEmpty(format))
                {
                    format = "xlsx";
                }
                string strSQL = v.RecX31.x31DocSqlSource;
                if (strSQL.Contains("@datfrom"))
                {
                    strSQL = strSQL.Replace("@datfrom", BO.Code.Bas.GD(v.PeriodFilter.d1, new DateTime(2000, 1, 1)));
                    strSQL = strSQL.Replace("@datuntil", BO.Code.Bas.GD(v.PeriodFilter.d2, new DateTime(3000, 1, 1)));
                }
                if (v.SelectedJ72ID > 0 && strSQL.Contains("391=391"))
                {
                    var mq = new BO.myQueryP91();
                    mq.lisJ73 = Factory.j72TheGridTemplateBL.GetList_j73(v.SelectedJ72ID, "p91", 0);
                    var qq = DL.basQuery.GetFinalSql("select", mq, Factory.CurrentUser);
                    strSQL = strSQL.Replace("391=391", qq.SqlWhere);
                }
                if (v.SelectedJ72ID > 0 && strSQL.Contains("331=331"))
                {
                    var mq = new BO.myQueryP91();
                    mq.lisJ73 = Factory.j72TheGridTemplateBL.GetList_j73(v.SelectedJ72ID, "p31", 0);
                    var qq = DL.basQuery.GetFinalSql("select", mq, Factory.CurrentUser);
                    strSQL = strSQL.Replace("331=331", qq.SqlWhere);
                }
                var dt = Factory.FBL.GetDataTable(strSQL);
                string strTempFileName = $"{Factory.CurrentUser.Inicialy}-MT-EXPORT-{DateTime.Now.ToString("dd-MM-yyyy-HH-mm-ss")}";
                string filepath_dest = $"{Factory.TempFolder}\\{strTempFileName}.{format}";
                string filepath_source = $"{Factory.UploadFolder}\\X31\\{v.RecX31.ReportFileName}";
                if (!System.IO.File.Exists(filepath_source))
                {
                    filepath_source = $"{Factory.App.RootUploadFolder}\\_distribution\\trdx\\{v.RecX31.ReportFileName}";
                }

                var cExport = new UI.Code.dataExport();

                if (format == "csv")
                {
                    if (cExport.ToCSV(dt, filepath_dest, filepath_source, v.CsvDelimiter, v.CsvUvozovky))
                    {
                        v.GeneratedOutputFileName = $"{strTempFileName}.{format}";

                    }
                }
                if (format == "xlsx")
                {
                    if (cExport.ToXLSX(dt, filepath_dest, filepath_source))
                    {
                        v.GeneratedOutputFileName = $"{strTempFileName}.{format}";

                    }
                }

                Factory.CBL.SetUserParam("x31/last-reportnocontext-x31id", v.RecX31.pid.ToString());

            }

            return View(v);
        }

        public IActionResult ReportNoContext(int x31id, string code, string caller_prefix, string caller_pids,bool noframework)
        {
            var v = new ReportNoContextViewModel() { caller_prefix = caller_prefix, caller_pids = caller_pids,NoFramework=noframework };
            v.LangIndex = Factory.CurrentUser.j02LangIndex;
            
            if (x31id == 0 && !string.IsNullOrEmpty(code))
            {
                x31id = Factory.x31ReportBL.LoadByCode(code, 0).pid;
            }
            v.SelectedX31ID = x31id;
            RefreshStateReportNoContext(v);
            v.IsConfirmNeeded = v.RecX31.x31IsPeriodRequired;


            if (v.NoFramework)
            {
                if (v.IsConfirmNeeded && v.PeriodFilter.PeriodValue>0)
                {
                    v.IsConfirmed = true;
                }
                
            }


            if (x31id > 0)
            {
                int intLastSaved = Factory.CBL.LoadUserParamInt("x31/last-reportnocontext-x31id");
                if (intLastSaved != x31id)
                {
                    Factory.CBL.SetUserParam("x31/last-reportnocontext-x31id", x31id.ToString());
                }
            }

            return View(v);

        }

        [HttpPost]
        public IActionResult ReportNoContext(ReportNoContextViewModel v, string oper)
        {

            RefreshStateReportNoContext(v);
            if (oper == "confirm")
            {
                v.IsConfirmed = true;
            }

            return View(v);
        }

        private BO.o27Attachment TryImportReportFromDistribution(BO.x31Report rec)
        {
            string strFullPath = $"{Factory.App.RootUploadFolder}\\_distribution\\trdx\\{rec.x31Code}.trdx";   //zkusit vzít report z distribution složky
            if (System.IO.File.Exists(strFullPath))
            {
                var recO27 = new BO.o27Attachment() { o27Entity = "x31", o27RecordPid = rec.pid, o27ContentType = "application/trdx" };
                int intO27ID = Factory.o27AttachmentBL.UploadAndSaveOneFile(recO27, $"{rec.x31Code}.trdx", strFullPath, $"{rec.x31Code}.trdx");

                //System.IO.File.Copy(strFullPath, Factory.UploadFolder + "\\x31\\" + $"{rec.x31Code}.trdx", true);

                return Factory.o27AttachmentBL.Load(intO27ID);
            }
            return null;
        }
        private void RefreshStateReportNoContext(ReportNoContextViewModel v)
        {
            if (v.SelectedX31ID > 0)
            {
                v.RecX31 = Factory.x31ReportBL.Load(v.SelectedX31ID);
                v.SelectedReport = v.RecX31.x31Name;
                v.IsPeriodFilter = v.RecX31.x31IsPeriodRequired;

                if (!string.IsNullOrEmpty(v.ReportExportName))
                {

                    v.ReportExportName = _basRepSup.GetReportExportName(Factory, 0, v.RecX31);
                }


                var recO27 = Factory.x31ReportBL.LoadReportDoc(v.SelectedX31ID);
                if (recO27 == null)
                {
                    recO27 = TryImportReportFromDistribution(v.RecX31);
                }
                if (recO27 != null)
                {
                    v.ReportFileName = recO27.o27ArchiveFileName;
                    string strFullPath = $"{Factory.ReportFolder}\\{v.ReportFileName}";

                    if (!System.IO.File.Exists(strFullPath))
                    {
                        v.ReportFileName = recO27.o27OriginalFileName;
                        strFullPath = $"{Factory.ReportFolder}\\{v.ReportFileName}";
                    }
                    if (!System.IO.File.Exists(strFullPath))
                    {
                        strFullPath = $"{Factory.App.RootUploadFolder}\\_distribution\\trdx\\{v.ReportFileName}";   //zkusit vzít report z distribution složky

                    }

                    if (v.RecX31.x31FormatFlag == BO.x31FormatFlagENUM.XLSX)
                    {
                        if (v.RecX31.x31DocSqlSource != null && v.RecX31.x31DocSqlSource.Contains("@datfrom"))
                        {
                            v.IsPeriodFilter = true;
                        }
                        else
                        {
                            v.IsPeriodFilter = v.RecX31.x31IsPeriodRequired;
                        }
                        if (v.RecX31.x31DocSqlSource != null)
                        {
                            if (v.RecX31.x31DocSqlSource.Contains("331=331"))
                            {
                                v.lisJ72 = Factory.j72TheGridTemplateBL.GetList("p31Worksheet", Factory.CurrentUser.pid, null).Where(p => p.j72SystemFlag == BO.j72SystemFlagEnum.QueryOnly);

                            }
                            if (v.RecX31.x31DocSqlSource.Contains("391=391"))
                            {
                                v.lisJ72 = Factory.j72TheGridTemplateBL.GetList("p91Invoice", Factory.CurrentUser.pid, null).Where(p => p.j72SystemFlag == BO.j72SystemFlagEnum.QueryOnly);

                            }
                        }
                    }


                    if (v.RecX31.x31FormatFlag == BO.x31FormatFlagENUM.Telerik && System.IO.File.Exists(strFullPath))
                    {
                        var strXmlContent = System.IO.File.ReadAllText(strFullPath);

                        if (v.RecX31.x31IsPeriodRequired || (strXmlContent.Contains("datFrom", StringComparison.OrdinalIgnoreCase) && strXmlContent.Contains("datUntil", StringComparison.OrdinalIgnoreCase)))
                        {
                            v.IsPeriodFilter = true;

                        }
                        else
                        {
                            v.IsPeriodFilter = false;
                        }
                        if (strXmlContent.Contains("331=331"))
                        {
                            v.lisJ72 = Factory.j72TheGridTemplateBL.GetList("p31Worksheet", Factory.CurrentUser.pid, null).Where(p => p.j72SystemFlag == BO.j72SystemFlagEnum.QueryOnly);

                        }
                    }


                }
                else
                {
                    if (v.RecX31.x31FileName != null && System.IO.File.Exists($"{Factory.ReportFolder}\\{v.RecX31.x31FileName}"))
                    {
                        v.ReportFileName = v.RecX31.x31FileName; //chybí záznam v tabulce o27Attachment - je možné, že fyzicky ale existuje na disku
                    }
                    else
                    {
                        this.AddMessage("Na serveru nelze dohledat soubor šablony zvolené tiskové sestavy.");
                    }




                }

                if (v.IsPeriodFilter)
                {
                    v.PeriodFilter = new myPeriodViewModel() { UserParamKey = "report-period" };
                    v.PeriodFilter.LoadUserSetting(_pp, Factory);
                    v.PeriodFilter.IsShowButtonRefresh = true;
                }


            }


        }


        public IActionResult ReportContext(int pid, string prefix, int x31id, string x31code, string pids, string p31guid, int x31id_direct_merge1, string guid_pids)
        {
            if (pids != null && pid == 0)
            {
                pid = BO.Code.Bas.ConvertString2ListInt(pids)[0];
            }
            if (!string.IsNullOrEmpty(guid_pids))
            {
                pids = Factory.p85TempboxBL.LoadByGuid(guid_pids).p85Message; //vstupní pids předány přes p85Tempbox
            }
            var v = new ReportContextViewModel() { rec_pid = pid, rec_prefix = prefix, rec_pids = pids, p31guid = p31guid };

            v.LangIndex = Factory.CurrentUser.j02LangIndex;
            if (string.IsNullOrEmpty(v.rec_prefix) || (v.rec_pid == 0 && string.IsNullOrEmpty(pids) && string.IsNullOrEmpty(p31guid)))
            {
                return StopPage(true, "pid/pids/p31guid or prefix missing");
            }

            if (x31id == 0 && string.IsNullOrEmpty(x31code))
            {
                v.UserParamKey = "ReportContext-" + prefix + "-x31id";
                x31id = Factory.CBL.LoadUserParamInt(v.UserParamKey);

            }
            if (x31id == 0 && !string.IsNullOrEmpty(x31code))
            {
                x31id = Factory.x31ReportBL.LoadByCode(x31code, 0).pid;
            }
            if (x31id == 0 && prefix == "p91")
            {
                var recP91 = Factory.p91InvoiceBL.Load(pid);
                x31id = Factory.p92InvoiceTypeBL.Load(recP91.p92ID).x31ID_Invoice;
            }
            if (Factory.CurrentUser.j04IsModule_p91)
            {
                v.IsPfxSignature = Factory.CBL.LoadUserParamBool("ReportContext-IsPfxSignature", false);
                v.IsIncludeISDOC = Factory.CBL.LoadUserParamBool("ReportContext-IsIncludeISDOC", false);
            }


            v.SelectedX31ID = x31id;
            try
            {
                v.MergedX31ID_1 = Factory.CBL.LoadUserParamInt("ReportContext-mergedx31id-1-" + v.rec_prefix);
                if (v.MergedX31ID_1 > 0) v.MergedX31Name_1 = Factory.x31ReportBL.Load(v.MergedX31ID_1).x31Name;
                v.MergedX31ID_2 = Factory.CBL.LoadUserParamInt("ReportContext-mergedx31id-2-" + v.rec_prefix);
                if (v.MergedX31ID_2 > 0) v.MergedX31Name_2 = Factory.x31ReportBL.Load(v.MergedX31ID_2).x31Name;
                v.MergedX31ID_3 = Factory.CBL.LoadUserParamInt("ReportContext-mergedx31id-3-" + v.rec_prefix);
                if (v.MergedX31ID_3 > 0) v.MergedX31Name_3 = Factory.x31ReportBL.Load(v.MergedX31ID_3).x31Name;
            }
            catch (Exception ex)
            {
                this.AddMessageTranslated(ex.Message);
            }
            if (x31id_direct_merge1 > 0)
            {
                v.MergedX31ID_1 = x31id_direct_merge1;  //přímo volaný merge report #1
                v.MergedX31ID_2 = 0;
                v.MergedX31ID_3 = 0;
            }


            RefreshStateReportContext(v);
            if (v.RecX31 != null && v.RecX31.x31FormatFlag == BO.x31FormatFlagENUM.DOCX)
            {
                ReportContext_GenerateDOCX(v); //automaticky vygenerovat DOCX
            }

            if (x31id_direct_merge1 > 0)
            {
                v.FinalMergedPdfFileName = MergeReports(v, BO.Code.Bas.GetGuid());  //hned spustit pdf merge
            }


            return View(v);
        }

        [HttpPost]
        public IActionResult ReportContext(ReportContextViewModel v, string oper, int j61id, bool isdoc, bool pfx)
        {
            RefreshStateReportContext(v);
            _basRepSup.PfxPath = null; _basRepSup.PfxPassword = null;


            if (oper == "change_x31id" && v.SelectedX31ID > 0)
            {

                v.ReportExportName = _basRepSup.GetReportExportName(Factory, v.rec_pid, v.RecX31);

                Factory.CBL.SetUserParam(v.UserParamKey, v.SelectedX31ID.ToString());
                v.GeneratedTempFileName = "";
            }
            if (oper == "generate_docx")
            {
                ReportContext_GenerateDOCX(v);
            }
            if (oper == "merge")    //pdf merge náhled
            {
                v.FinalMergedPdfFileName = MergeReports(v, BO.Code.Bas.GetGuid());
            }
            if (oper == "merge_and_download")    //pdf merge download
            {
                v.FinalMergedPdfFileName = MergeReports(v, BO.Code.Bas.GetGuid());
                v.FinalMergedPdfFileName_Download = v.FinalMergedPdfFileName;
            }
            if (oper == "merge_and_preview")    //pdf merge náhled
            {
                v.FinalMergedPdfFileName = MergeReports(v, BO.Code.Bas.GetGuid());
                v.FinalMergedPdfFileName_Preview = v.FinalMergedPdfFileName;
            }
            if (oper == "mail") //odeslat sestavu poštou
            {
                string strUploadGuid = BO.Code.Bas.GetGuid();
                if (v.rec_pids != null && v.Guid_MultipleReport != null)
                {
                    strUploadGuid = v.Guid_MultipleReport;    //vygenerovaný multireport pdf
                }
                else
                {
                    if (pfx)
                    {
                        if (!InhalePfxCertificate(v))
                        {
                            return View(v);
                        }
                    }
                    if (v.RecX31 == null && v.SelectedX31ID > 0)
                    {
                        v.RecX31 = Factory.x31ReportBL.Load(v.SelectedX31ID);
                    }

                    string strPdfPath = _basRepSup.GeneratePdfReport(Factory, _pp, v.RecX31, strUploadGuid, v.rec_pid, true, 0, v.p31guid, v.Translate, isdoc);

                }

                return RedirectToAction("SendMail", "Mail", new { uploadguid = strUploadGuid, record_pid = v.rec_pid, record_entity = v.rec_prefix, j61id = j61id });
            }
            if (oper == "merge_and_mail")
            {
                string strUploadGuid = BO.Code.Bas.GetGuid();
                if (MergeReports(v, strUploadGuid) != null)
                {
                    return RedirectToAction("SendMail", "Mail", new { uploadguid = strUploadGuid, record_pid = v.rec_pid, record_entity = v.rec_prefix });
                }
            }

            if (oper == "multireport")
            {
                var pids = BO.Code.Bas.ConvertString2ListInt(v.rec_pids);
                MergeDocument doc = new MergeDocument();

                v.Guid_MultipleReport = BO.Code.Bas.GetGuid();
                int x = 0;
                foreach (int pid in pids)
                {
                    x += 1;
                    string strPdfFullPath = _basRepSup.GeneratePdfReport(Factory, _pp, v.RecX31, x.ToString() + "-" + v.Guid_MultipleReport, pid, true, 0, v.p31guid, v.Translate);
                    doc.Append(strPdfFullPath);
                    if (v.MergedX31ID_1 > 0)    //merge
                    {
                        strPdfFullPath = _basRepSup.GeneratePdfReport(Factory, _pp, Factory.x31ReportBL.Load(v.MergedX31ID_1), BO.Code.Bas.GetGuid(), pid, true, 0, v.p31guid, v.Translate);
                        doc.Append(strPdfFullPath);
                    }
                }
                doc.Draw($"{Factory.TempFolder}\\MultiReport_{v.Guid_MultipleReport}.pdf");
                v.GeneratedTempFileName = $"MultiReport_{v.Guid_MultipleReport}.pdf";
                Factory.o27AttachmentBL.CreateTempInfoxFile(v.Guid_MultipleReport, v.rec_prefix, $"MultiReport_{v.Guid_MultipleReport}.pdf", $"MultiReport_{v.Guid_MultipleReport}.pdf", "application/pdf");

            }
            if (oper == "multizip")
            {
                var pids = BO.Code.Bas.ConvertString2ListInt(v.rec_pids);
                
                v.Guid_MultipleReport = BO.Code.Bas.GetGuid();
                int x = 0;
                var files = new List<string>();
                foreach (int pid in pids)
                {
                    x += 1;
                    MergeDocument doc = new MergeDocument();
                    string strPdfFullPath = _basRepSup.GeneratePdfReport(Factory, _pp, v.RecX31, x.ToString() + "-" + v.Guid_MultipleReport, pid, true, 0, v.p31guid, v.Translate);                                        
                    doc.Append(strPdfFullPath);
                    if (v.MergedX31ID_1 > 0)    //merge
                    {
                        var strMergedPath = _basRepSup.GeneratePdfReport(Factory, _pp, Factory.x31ReportBL.Load(v.MergedX31ID_1), BO.Code.Bas.GetGuid(), pid, true, 0, v.p31guid, v.Translate);
                        doc.Append(strMergedPath);
                    }
                    
                    var strTempFile = $"{Factory.TempFolder}\\{_basRepSup.GetReportExportName(Factory,pid,v.RecX31)}.pdf";
                    if (System.IO.File.Exists(strTempFile))
                    {
                        System.IO.File.Delete(strTempFile);
                    }

                    doc.Draw(strTempFile);
                    files.Add(strTempFile);                    
                }

                using (var memoryStream = new MemoryStream())
                {
                    using (var zipArchive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
                    {
                        foreach (var file in files)
                        {
                            zipArchive.CreateEntryFromFile(file, Path.GetFileName(file));
                        }
                    }
                    memoryStream.Position = 0;
                    var ret= File(memoryStream, "application/zip", "my.zip");
                    BO.Code.File.SaveStream2File($"{Factory.TempFolder}\\marktime_multireport_{Factory.CurrentUser.Inicialy}.zip", ret.FileStream);

                    v.GeneratedTempFileName = $"marktime_multireport_{Factory.CurrentUser.Inicialy}.zip";
                    

                }

            }

            return View(v);
        }

        private void ReportContext_GenerateDOCX(ReportContextViewModel v)
        {
            v.AllGeneratedTempFileNames = Handle_DocMailMerge(v);

            if (v.AllGeneratedTempFileNames != null)
            {
                v.GeneratedTempFileName = v.AllGeneratedTempFileNames[0];
                this.AddMessage("Dokument byl vygenerován.", "info");
            }
        }


        private void RefreshStateReportContext(ReportContextViewModel v)
        {
            if (v.rec_pids != null)
            {
                v.rec_pids_count = v.rec_pids.Split(",").Count();
            }
            if (v.SelectedX31ID > 0)
            {
                v.RecX31 = Factory.x31ReportBL.Load(v.SelectedX31ID);
                if (v.RecX31 == null)
                {
                    this.AddMessage("Nelze načíst záznam tiskové šablony.");
                    return;
                }
                v.SelectedReport = v.RecX31.x31Name;
                if (string.IsNullOrEmpty(v.ReportExportName))
                {

                    v.ReportExportName = _basRepSup.GetReportExportName(Factory, v.rec_pid, v.RecX31);
                }

                if (v.rec_prefix != null && v.rec_prefix != "j02" && v.rec_pid > 0 && (Factory.CurrentUser.j04IsModule_p91 || Factory.CurrentUser.j04IsModule_p28))
                {
                    if (v.rec_prefix == "p91")
                    {
                        var recP91 = Factory.p91InvoiceBL.Load(v.rec_pid);
                        var recP92 = Factory.p92InvoiceTypeBL.Load(recP91.p92ID);
                        var intJ61ID = Factory.p91InvoiceBL.NajdiVychoziJ61ID(recP92, recP91, Factory.p28ContactBL.Load(recP91.p28ID));
                        if (intJ61ID > 0)
                        {
                            v.lisJ61 = Factory.j61TextTemplateBL.GetList(new BO.myQueryJ61() { pids = new List<int>() { intJ61ID } });
                        }
                        
                    }
                    if (v.lisJ61 == null)
                    {
                        v.lisJ61 = Factory.j61TextTemplateBL.GetList(new BO.myQueryJ61() { entity = v.rec_prefix, MyRecordsDisponible = true });
                    }
                  
                }

                var recO27 = Factory.x31ReportBL.LoadReportDoc(v.SelectedX31ID);
                if (recO27 == null)
                {
                    recO27 = TryImportReportFromDistribution(v.RecX31);
                }
                if (recO27 != null)
                {
                    v.ReportFileName = recO27.o27ArchiveFileName;
                    string strPath = $"{Factory.ReportFolder}\\{v.ReportFileName}";
                    if (!System.IO.File.Exists(strPath))
                    {
                        v.ReportFileName = recO27.o27OriginalFileName;
                        strPath = $"{Factory.ReportFolder}\\{v.ReportFileName}";
                    }
                    if (!System.IO.File.Exists(strPath))
                    {
                        strPath = $"{Factory.App.RootUploadFolder}\\_distribution\\trdx\\{v.ReportFileName}";
                    }
                    if (System.IO.File.Exists(strPath))
                    {
                        //var xmlReportSource = new Telerik.Reporting.XmlReportSource();
                        var strXmlContent = System.IO.File.ReadAllText(strPath);
                        if (v.RecX31.x31IsPeriodRequired || (strXmlContent.Contains("datFrom", StringComparison.OrdinalIgnoreCase) && strXmlContent.Contains("datUntil", StringComparison.OrdinalIgnoreCase)))
                        {
                            v.IsPeriodFilter = true;
                            v.PeriodFilter = new myPeriodViewModel() { UserParamKey = "report-period" };
                            v.PeriodFilter.IsShowButtonRefresh = true;
                            v.PeriodFilter.LoadUserSetting(_pp, Factory);

                        }
                        else
                        {
                            v.IsPeriodFilter = false;
                        }
                    }

                }
                else
                {
                    if (v.RecX31.x31FileName != null && System.IO.File.Exists($"{Factory.ReportFolder}\\{v.RecX31.x31FileName}"))
                    {
                        v.ReportFileName = v.RecX31.x31FileName; //chybí záznam v tabulce o27Attachment - je možné, že fyzicky ale existuje na disku
                    }
                    else
                    {
                        this.AddMessage("Na serveru nelze dohledat soubor šablony zvolené tiskové sestavy.");
                    }

                }
            }

        }


        private void handle_merge_value(Text item, DataTable dt, DataRow dr)
        {
            string strVal = "";

            foreach (DataColumn col in dt.Columns)
            {

                if (item.Text.Contains($"«{col.ColumnName}»", StringComparison.OrdinalIgnoreCase) || item.Text.Contains($"<{col.ColumnName}>", StringComparison.OrdinalIgnoreCase))
                {
                    if (dr[col] == System.DBNull.Value || dr[col] == null)
                    {
                        strVal = "";
                    }
                    else
                    {
                        switch (col.DataType.Name.ToString())
                        {
                            case "DateTime":
                                strVal = BO.Code.Bas.ObjectDate2String(dr[col]);
                                break;
                            case "Decimal":
                            case "Double":
                                strVal = BO.Code.Bas.Number2String(Convert.ToDouble(dr[col]));
                                break;
                            default:
                                strVal = dr[col].ToString();
                                break;
                        }

                    }
                    item.Text = item.Text.Replace($"<{col.ColumnName}>", strVal, StringComparison.OrdinalIgnoreCase).Replace($"«{col.ColumnName}»", strVal, StringComparison.OrdinalIgnoreCase);

                }
            }
        }
        private List<string> Handle_DocMailMerge(ReportContextViewModel v)
        {
            var recO27 = Factory.x31ReportBL.LoadReportDoc(v.RecX31.pid);
            if (recO27 == null)
            {
                this.AddMessage("Na serveru nelze dohledat soubor šablony zvolené tiskové sestavy."); return null;
            }
            DataTable dt = null;
            if (string.IsNullOrEmpty(v.RecX31.x31DocSqlSource))
            {
                dt = Factory.gridBL.GetList4MailMerge(v.rec_prefix, v.rec_pid);
            }
            else
            {
                dt = Factory.gridBL.GetList4MailMerge(v.rec_pid, v.RecX31.x31DocSqlSource);  //sql zdroj na míru
            }

            if (dt.Rows.Count == 0)
            {
                this.AddMessage("Na vstupu chybí záznam."); return null;
            }

            var strFileName = BO.Code.Bas.GetGuid();


            var filenames = new List<string>();
            int x = 0;
            foreach (DataRow dr in dt.Rows)
            {
                x += 1;
                filenames.Add(strFileName + "_" + x.ToString() + ".docx");
                GenerateOneDoc(recO27, dt, dr, filenames.Last());
            }

            if (dt.Rows.Count > 1)  //join dokumentů, pokud má zdroj více záznamů
            {
                System.IO.File.Copy(Factory.TempFolder + "\\" + filenames[0], Factory.TempFolder + "\\" + strFileName + ".docx", true);   //výsledek pro záznam

                for (int xx = 1; xx < filenames.Count; xx++)  //join druhého a dalšího záznamu do dokumentu prvního záznamu
                {
                    using (WordprocessingDocument myDoc = WordprocessingDocument.Open(Factory.TempFolder + "\\" + strFileName + ".docx", true))
                    {
                        MainDocumentPart mainPart = myDoc.MainDocumentPart;

                        string altChunkId = "AltChunkId" + xx.ToString();
                        AlternativeFormatImportPart chunk = mainPart.AddAlternativeFormatImportPart(
                            AlternativeFormatImportPartType.WordprocessingML, altChunkId);
                        using (FileStream fileStream = System.IO.File.Open(Factory.TempFolder + "\\" + filenames[xx], FileMode.Open))
                        {
                            chunk.FeedData(fileStream);
                        }
                        Paragraph pb1 = new Paragraph(new Run((new Break() { Type = BreakValues.Page })));
                        mainPart.Document.Body.InsertAfter(pb1, mainPart.Document.Body.LastChild);

                        AltChunk altChunk = new AltChunk();
                        altChunk.Id = altChunkId;
                        mainPart.Document.Body.InsertAfter(altChunk, mainPart.Document.Body.Elements<Paragraph>().Last());

                        mainPart.Document.Save();
                        myDoc.Close();
                    }
                }
                filenames.Insert(0, strFileName + ".docx");
            }

            return filenames;
        }


        private void GenerateOneDoc(BO.o27Attachment recO27, DataTable dt, DataRow dr, string strTempFileName)
        {
            string strTempPath = Factory.TempFolder + "\\" + strTempFileName;
            System.IO.File.Copy(Factory.ReportFolder + "\\" + recO27.o27ArchiveFileName, strTempPath, true);
            Package wordPackage = Package.Open(strTempPath, FileMode.Open, FileAccess.ReadWrite);

            using (WordprocessingDocument wordDocument = WordprocessingDocument.Open(wordPackage))
            {
                var body = wordDocument.MainDocumentPart.Document.Body;
                var allParas = wordDocument.MainDocumentPart.Document.Descendants<DocumentFormat.OpenXml.Wordprocessing.Text>();

                foreach (var item in allParas)
                {
                    handle_merge_value(item, dt, dr);


                }
                foreach (HeaderPart headerPart in wordDocument.MainDocumentPart.HeaderParts)
                {
                    Header header = headerPart.Header;
                    var allHeaderParas = header.Descendants<Text>();
                    foreach (var item in allHeaderParas)
                    {
                        handle_merge_value(item, dt, dr);

                    }

                }

                foreach (FooterPart footerPart in wordDocument.MainDocumentPart.FooterParts)
                {
                    Footer footer = footerPart.Footer;
                    var allFooterParas = footer.Descendants<Text>();
                    foreach (var item in allFooterParas)
                    {
                        handle_merge_value(item, dt, dr);

                    }

                }


                wordDocument.MainDocumentPart.Document.Save();

            }
        }

        private string MergeReports(ReportContextViewModel v, string strUploadGuid)
        {
            if (v.SelectedX31ID == 0)
            {
                this.AddMessage("Chybí vybrat tiskovou sestavu."); return null;
            }
            if (v.MergedX31ID_1 == 0 && v.MergedX31ID_2 == 0 && v.MergedX31ID_3 == 0)
            {
                this.AddMessage("Musíte zvolit minimálně jednu slučovanou sestavu."); return null;
            }


            string s = null;
            var files = new List<string>();

            if (v.SelectedX31ID > 0)
            {
                InhalePfxCertificate(v);
                s = _basRepSup.GeneratePdfReport(Factory, _pp, Factory.x31ReportBL.Load(v.SelectedX31ID), BO.Code.Bas.GetGuid(), v.rec_pid, true, 0, v.p31guid, v.Translate);
                files.Add(s);
            }
            if (v.MergedX31ID_1 > 0)
            {
                InhalePfxCertificate(v);
                s = _basRepSup.GeneratePdfReport(Factory, _pp, Factory.x31ReportBL.Load(v.MergedX31ID_1), BO.Code.Bas.GetGuid(), v.rec_pid, true, 0, v.p31guid, v.Translate);
                files.Add(s);
            }
            Factory.CBL.SetUserParam("ReportContext-mergedx31id-1-" + v.rec_prefix, v.MergedX31ID_1.ToString());

            if (v.MergedX31ID_2 > 0)
            {
                InhalePfxCertificate(v);
                s = _basRepSup.GeneratePdfReport(Factory, _pp, Factory.x31ReportBL.Load(v.MergedX31ID_2), BO.Code.Bas.GetGuid(), v.rec_pid, true, 0, v.p31guid, v.Translate);
                files.Add(s);
            }
            Factory.CBL.SetUserParam("ReportContext-mergedx31id-2-" + v.rec_prefix, v.MergedX31ID_2.ToString());

            if (v.MergedX31ID_3 > 0)
            {
                InhalePfxCertificate(v);
                s = _basRepSup.GeneratePdfReport(Factory, _pp, Factory.x31ReportBL.Load(v.MergedX31ID_3), BO.Code.Bas.GetGuid(), v.rec_pid, true, 0, v.p31guid, v.Translate);
                files.Add(s);
            }
            Factory.CBL.SetUserParam("ReportContext-mergedx31id-3-" + v.rec_prefix, v.MergedX31ID_3.ToString());


            MergeDocument doc = new MergeDocument(files[0]);
            for (int i = 1; i < files.Count(); i++)
            {
                doc.Append(files[i]);

            }


            string strFinalRepFileName = _basRepSup.GetReportExportName(Factory, v.rec_pid, v.RecX31);
            _basRepSup.SetPdfFileFields(doc, Factory.Lic.x01Name, $"MARKTIME {Factory.App.AppBuild}", strFinalRepFileName,(v.RecX31 !=null ? v.RecX31.x31Name : null));   //vlastnosti pdf dokumentu
            
            strFinalRepFileName = $"{strFinalRepFileName}.pdf";
                       
            doc.Draw($"{Factory.TempFolder}\\{strUploadGuid}_{strFinalRepFileName}");
            
            Factory.o27AttachmentBL.CreateTempInfoxFile(strUploadGuid, v.rec_prefix, strUploadGuid + "_" + strFinalRepFileName, strFinalRepFileName, "application/pdf");

            return strUploadGuid + "_" + strFinalRepFileName;
        }

        private bool InhalePfxCertificate(ReportContextViewModel v)
        {
            _basRepSup.PfxPath = null; _basRepSup.PfxPassword = null;
            if (v.rec_prefix == "p91" && v.rec_pid > 0)
            {
                var recP91 = Factory.p91InvoiceBL.Load(v.rec_pid);
                var recP93 = Factory.p93InvoiceHeaderBL.Load(recP91.p93ID);
                if (recP93.p93PfxCertificate != null)
                {
                    _basRepSup.PfxPath = $"{Factory.WwwUsersFolder}\\PLUGINS\\{recP93.p93PfxCertificate}";  //kvalifikovaný certifikát
                    _basRepSup.PfxPassword = recP93.p93PfxPassword;
                    return true;
                }
                else
                {
                    this.AddMessageTranslated("V nastavení vystavovatele faktury chybí PFX soubor elektronického podpisu.");
                }
            }
            return false;
        }
    }
}