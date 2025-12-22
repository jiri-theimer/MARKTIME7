
using BL;
using BL.Singleton;
using ceTe.DynamicPDF;
using ceTe.DynamicPDF.Merger;
using SQLitePCL;
using System.ComponentModel;
using UI.Views.Shared.Components.myPeriod;

namespace UI
{
    public class TheReportSupport
    {
        public string PfxPath { get; set; }
        public string PfxPassword { get; set; }
        public string PfxLabel { get; set; } = "Digitálně podepsáno";
        private string _strTranslate { get; set; }

        public string GeneratePdfGridReport(BL.Factory f, string strTrdxFileName, string strUploadGuid, bool bolReturnFullPath = true)
        {
            string strPath = $"{f.ReportFolder}\\{strTrdxFileName}";
            if (!System.IO.File.Exists(strPath))
            {
                strPath = $"{f.App.RootUploadFolder}\\_distribution\\trdx\\{strTrdxFileName}";
            }

            if (f.App.HostingMode == BL.Singleton.HostingModeEnum.SharedApp)    //sdílená aplikace pro N databází: měníme connect-string přímo v trdx šabloně
            {
                string s = BO.Code.File.GetFileContent(strPath);
                if (s.Contains("ApplicationPrimary"))
                {
                    s = s.Replace("wwwroot/_users", $"{f.App.RootUploadFolder}//_users");
                    s = s.Replace("ApplicationPrimary", f.App.ParseConnectStringFromLogin(f.CurrentUser.j02Login));
                    BO.Code.File.WriteText2File($"{f.ReportFolder}\\{strTrdxFileName}", s);
                    strPath = $"{f.ReportFolder}\\{strTrdxFileName}";
                }                
            }

            var uriReportSource = new Telerik.Reporting.UriReportSource();
            uriReportSource.Uri = strPath;

            

            Telerik.Reporting.Processing.ReportProcessor processor = new Telerik.Reporting.Processing.ReportProcessor(f.App.Configuration);

            var result = processor.RenderReport("PDF", uriReportSource, null);

            System.IO.MemoryStream ms = new System.IO.MemoryStream();
            ms.Write(result.DocumentBytes, 0, result.DocumentBytes.Length);
            ms.Seek(0, System.IO.SeekOrigin.Begin);

            var strGUID = BO.Code.Bas.GetGuid();
            var strPdfFileName = strUploadGuid + "_" + strGUID + ".pdf";

            BO.Code.File.SaveStream2File(f.TempFolder + "\\" + strPdfFileName, ms);
            f.o27AttachmentBL.CreateTempInfoxFile(strUploadGuid, "x31", strPdfFileName, strGUID + ".pdf", "application/pdf");
            if (bolReturnFullPath)
            {
                return f.TempFolder + "\\" + strPdfFileName;
            }
            else
            {
                return strPdfFileName;
            }

        }
        public string GeneratePdfReport(BL.Factory f, BL.Singleton.ThePeriodProvider pp, BO.x31Report recX31, string strUploadGuid, int recpid, bool bolReturnFullPath = true, int intX21ID_Explicit = 0,string p31guid=null,string translate=null,bool include_isdoc=false,int j72id=0)
        {
            var uriReportSource = new Telerik.Reporting.UriReportSource();

           
            string strTempFile = $"{f.TempFolder}\\{recX31.pid}-{BO.Code.Bas.ObjectDate2String(recX31.DateUpdate, "ddMMyyyyHHmmss")}.trdx";
            string reportXml = null;
            if (!System.IO.File.Exists(strTempFile) || j72id>0)
            {
                var recO27 = f.x31ReportBL.LoadReportDoc(recX31.pid);
                string strArchiveFileName = null;
                if (recO27 != null)
                {
                    strArchiveFileName= recO27.o27ArchiveFileName;
                }
                else
                {
                    strArchiveFileName = recX31.x31FileName;
                }
                if (System.IO.File.Exists($"{f.ReportFolder}\\{strArchiveFileName}"))
                {
                    System.IO.File.Copy($"{f.ReportFolder}\\{strArchiveFileName}", strTempFile, true);
                }
                else
                {
                    System.IO.File.Copy($"{f.App.RootUploadFolder}\\_distribution\\trdx\\{strArchiveFileName}", strTempFile, true);
                }
                if (j72id > 0)
                {
                    reportXml = File.ReadAllText(strTempFile);
                    var recJ72 = f.j72TheGridTemplateBL.Load(j72id);
                    var mq = new BO.InitMyQuery(f.CurrentUser).Load(recJ72.j72Entity);
                    mq.lisJ73 = f.j72TheGridTemplateBL.GetList_j73(j72id, recJ72.j72Entity.Substring(0, 3), 0);
                    
                    DL.FinalSqlCommand fq = DL.basQuery.GetFinalSql("", mq, f.CurrentUser);

                    string strFilterAlias = f.j72TheGridTemplateBL.getFiltrAlias(recJ72.j72Entity.Substring(0, 3), mq);
                    if (reportXml.Contains("331=331"))
                    {
                        reportXml = reportXml.Replace("331=331", fq.SqlWhere).Replace("#query_alias#", strFilterAlias);
                    }
                    if (reportXml.Contains("1=1"))
                    {
                        reportXml = reportXml.Replace("1=1", fq.SqlWhere).Replace("#query_alias#", strFilterAlias);
                    }
                    BO.Code.File.WriteText2File(strTempFile, reportXml);
                }
                
                
                if (f.App.HostingMode == BL.Singleton.HostingModeEnum.SharedApp)    //sdílená aplikace pro N databází: měníme connect-string přímo v trdx šabloně
                {
                    string s = BO.Code.File.GetFileContent(strTempFile);                    
                    if (s.Contains("ApplicationPrimary") || s.Contains("wwwroot/_users/"))
                    {
                        
                        s = s.Replace("wwwroot/_users", $"{f.App.RootUploadFolder}//_users");
                        s = s.Replace("ApplicationPrimary", f.App.ParseConnectStringFromLogin(f.CurrentUser.j02Login));
                        BO.Code.File.WriteText2File(strTempFile, s);
                    }

                }
                else
                {

                }

                
            }

            if (!string.IsNullOrEmpty(translate))
            {
                string s = BO.Code.File.GetFileContent(strTempFile);
                s = TranslateFromCzech(s, f, translate);
                strTempFile= $"{f.TempFolder}\\{BO.Code.Bas.GetGuid()}.trdx";
                BO.Code.File.WriteText2File(strTempFile, s);
            }

            if (f.App.HostingMode == BL.Singleton.HostingModeEnum.None)
            {
                if (reportXml == null)
                {
                    reportXml = File.ReadAllText(strTempFile);
                }
                if (reportXml.Contains("wwwroot/_users/"))  //možný problém na inhouse licencích s přístupem do PLUGINS přes relativní cestu!
                {
                    var destinationFolderFiles = Directory.GetFiles($"{f.WwwUsersFolder}\\PLUGINS", "*.*", SearchOption.TopDirectoryOnly);
                    string strTempDir = $"{f.TempFolder}\\wwwroot\\_users\\{f.Lic.x01Guid}\\PLUGINS";
                    if (!System.IO.Directory.Exists(strTempDir))
                    {
                        System.IO.Directory.CreateDirectory(strTempDir);
                    }
                    foreach (string strFilePath in destinationFolderFiles)
                    {
                        var fileinfo = BO.Code.File.GetFileInfo(strFilePath);
                        if (!System.IO.File.Exists($"{strTempDir}\\{fileinfo.Name}"))
                        {
                            System.IO.File.Copy(strFilePath, $"{strTempDir}\\{fileinfo.Name}");
                        }
                        else
                        {
                            if (fileinfo.Length != BO.Code.File.GetFileInfo($"{strTempDir}\\{fileinfo.Name}").Length)
                            {
                                System.IO.File.Copy(strFilePath, $"{strTempDir}\\{fileinfo.Name}", true);
                            }
                        }

                    }
                }
            }
            uriReportSource.Uri = strTempFile;  //report načítat z TEMPu            



            if (recpid > 0)
            {
                uriReportSource.Parameters.Add("pid", recpid);
            }
            


            if (pp != null)
            {
                var per = new myPeriodViewModel() { UserParamKey = "report-period" };
                per.LoadUserSetting(pp, f);
                if (per.d1 == null)
                {
                    per.d1 = new DateTime(2000, 1, 1);
                }
                if (per.d2 == null)
                {
                    per.d2 = new DateTime(3000, 1, 1);
                }

                if (intX21ID_Explicit > 0)
                {
                    //časové období přímo v nastavení sestavy           
                    per.d1 = pp.ByPid(intX21ID_Explicit).d1;
                    per.d2 = pp.ByPid(intX21ID_Explicit).d2;

                }
                uriReportSource.Parameters.Add("datfrom", per.d1);
                uriReportSource.Parameters.Add("datuntil", per.d2);
            }
            

            
            if (!string.IsNullOrEmpty(p31guid))
            {
                uriReportSource.Parameters.Add("p31guid", p31guid);
            }
            
            
            uriReportSource.Parameters.Add("l5", f.getP07Level(5, true));

            uriReportSource.Parameters.Add("x01id", f.CurrentUser.x01ID);
           
            uriReportSource.Parameters.Add("translate", string.IsNullOrEmpty(translate) ? "cz" : translate);

            uriReportSource.Parameters.Add("permission_role_value", f.CurrentUser.RoleValue);


            //if (f.App.HostingMode == BL.Singleton.HostingModeEnum.SharedApp) //sdílená aplikace pro N databází
            //{
            //    appConfig.GetSection("ConnectionStrings")["ApplicationPrimary"] = f.App.ParseConnectStringFromLogin(f.CurrentUser.j02Login);
            //}

            Telerik.Reporting.Processing.ReportProcessor processor = new Telerik.Reporting.Processing.ReportProcessor(f.App.Configuration);

            var result = processor.RenderReport("PDF", uriReportSource, null);
            
            System.IO.MemoryStream ms = new System.IO.MemoryStream();
            ms.Write(result.DocumentBytes, 0, result.DocumentBytes.Length);
            ms.Seek(0, System.IO.SeekOrigin.Begin);


            string strReportFileName = GetReportExportName(f, recpid, recX31) + ".pdf";            
            BO.Code.File.SaveStream2File($"{f.TempFolder}\\{strUploadGuid}_{strReportFileName}", ms);

            if (include_isdoc)
            {
                //přiložit isdoc soubor do pdf dokumentu
                ceTe.DynamicPDF.Document.AddLicense("DPSPROU4223720241231Xap8Eso/OLqTQoAdWV83/EhF3keLURxFeh6eWVIsKRuL5QcYIwkKfrnldyUxzLX17t/Zdk0VJQDF/Ka6byCKNrfL/A");
                var rec = f.p91InvoiceBL.CreateIntegraceRecord(f.p91InvoiceBL.Load(recpid));
                string strIsdocPath = BL.Code.p91Support.GenerateIsdoc(rec, new HttpClient(), $"{f.TempFolder}");
                var docy = new ceTe.DynamicPDF.Merger.MergeDocument();
                docy.Append($"{f.TempFolder}\\{strUploadGuid}_{strReportFileName}");
                docy.EmbeddedFiles.Add(new EmbeddedFile(strIsdocPath));

                SetPdfFileFields(docy, f.Lic.x01Name, $"MARKTIME {f.App.AppBuild}", rec.p91Client, "ISDOC");   //vlastnosti pdf dokumentu


                docy.Draw($"{f.TempFolder}\\{strUploadGuid}1.pdf");
                System.IO.File.Copy($"{f.TempFolder}\\{strUploadGuid}1.pdf", $"{f.TempFolder}\\{strUploadGuid}_{strReportFileName}", true);

            }

            if (this.PfxPath != null)
            {
                var podepsat = new PdfSupport() { PfxPath = this.PfxPath, PfxPassword = this.PfxPassword,Label=this.PfxLabel,PdfFieldSubject=strReportFileName };
                podepsat.PodepsatDokument($"{f.TempFolder}\\{strUploadGuid}_{strReportFileName}", $"{f.TempFolder}\\{strUploadGuid}_Signed_{strReportFileName}");
                strReportFileName = $"Signed_{strReportFileName}";
            }
            

            f.o27AttachmentBL.CreateTempInfoxFile(strUploadGuid, "x31", $"{strUploadGuid}_{strReportFileName}", strReportFileName, "application/pdf");
            if (recX31.x31Entity == "p91")
            {
                //ošetřit otisk faktury
                f.p91InvoiceBL.SaveImprint($"{f.TempFolder}\\{strUploadGuid}_{strReportFileName}", recpid,recX31.x31Code);
            }
            if (bolReturnFullPath)
            {                
                return $"{f.TempFolder}\\{strUploadGuid}_{strReportFileName}";
            }
            else
            {
                return $"{strUploadGuid}_{strReportFileName}";
            }
            
        }

        public void SetPdfFileFields(MergeDocument doc,string strAuthor,string strCreator,string strSubject,string strTitle)
        {
           
            try
            {
                //vlastnosti PDF souboru                
                doc.Author = strAuthor;
                doc.Creator = strCreator;
                doc.Producer = "PDFsharp";
                doc.Title = strTitle;                
                doc.Subject = strSubject; ;
            }
            catch
            {
                //nic
            }
        }
        public string GetReportExportName(BL.Factory f, int pid, BO.x31Report recX31) //vygeneruje název PDF souboru tiskové sestavy
        {
            string s = null;
            string prefix = recX31.x31Entity;
            if (recX31.x31ExportFileNameMask != null)
            {
                s = f.x31ReportBL.ParseExportFileNameMask(recX31.x31ExportFileNameMask, prefix, pid);
                if (s != null)
                {
                    return s;
                }
            }

            if (s == null && prefix != null && pid > 0 && prefix != "x31")
            {
                s = $"{f.CBL.GetObjectAlias(prefix, pid)}";
                if (prefix != "p91" && prefix != "p90" && prefix !="p82")
                {
                    s += "-report";
                }
            }

            if (s == null)
            {
                //s = BO.Code.File.PrepareFileName(recX31.x31Name, true);
                s = BO.Code.File.ConvertToSafeFileName(recX31.x31Name, 200);
            }
            else
            {
                s = BO.Code.File.ConvertToSafeFileName(s, 200);
                //s = BO.Code.File.PrepareFileName(s, true);
            }
            s = s.Replace(".", "");

            return s;



        }



        public string TranslateFromCzech(string reportXml, BL.Factory f, string lang)
        {
            if (!System.IO.File.Exists($"{f.App.RootUploadFolder}\\_distribution\\trdx\\reports.lang"))
            {
                return reportXml;
            }


            _strTranslate = reportXml;

            System.Text.RegularExpressions.MatchCollection matches = System.Text.RegularExpressions.Regex.Matches(reportXml, "Value=" + "\"" + "(.*?)" + "\"");
            bool b = false;
            var origs = new List<string>();
            foreach (System.Text.RegularExpressions.Match m in matches)
            {
                
                if (m.Length > 8)
                {
                    
                    b = true;
                    if (m.Value.Contains("Fields."))
                    {
                        b = false;
                        if (m.Value.Contains("{"))
                        {
                            b = true;
                        }
                    }
                    if (b)
                    {
                        origs.Add(m.Value);
                    }
                }
            }
            var trans = new List<BO.StringPair>();
            var lines = BO.Code.Bas.ConvertString2List(BO.Code.File.GetFileContent($"{f.App.RootUploadFolder}\\_distribution\\trdx\\reports.lang"), System.Environment.NewLine);
            foreach (string line in lines)
            {
                var lis = BO.Code.Bas.ConvertString2List(line, "|");
                if (lis.Count() > 1)
                {
                    var c = new BO.StringPair() { Key = lis[0], Value = lis[0] };
                    if (lang == "en") c.Value = lis[1];
                    if (lang == "de" && lis.Count() > 2)
                    {
                        c.Value = lis[2];
                    }
                    if (lang == "sk" && lis.Count() > 4)
                    {
                        c.Value = lis[4];
                    }
                    
                    trans.Add(c);
                }

            }

            foreach (string org in origs)
            {                
                string strSearch = org.Trim().Replace("Value=", "").Replace("\"", "");
               
                var qry = trans.Where(p => p.Key == strSearch);
                if (qry.Count() > 0)
                {
                    
                    tfe(strSearch, qry.First().Value);
                }
                else
                {
                    qry = trans.Where(p => $"{p.Key}:" == strSearch);
                    if (qry.Count() > 0)
                    {
                        tfe(strSearch, $"{qry.First().Value}:");
                    }
                    else
                    {
                        qry = trans.Where(p => p.Key.ToLower() == strSearch.ToLower());
                        if (qry.Count() > 0)
                        {

                            tfe(strSearch, qry.First().Value);
                        }

                        
                    }
                }
            }


            return _strTranslate;

        }

        private void tfe(string strOrig, string strTo)
        {
            //if (string.IsNullOrEmpty(strOrig) || string.IsNullOrEmpty(strTo))
            //{
            //    return;
            //}
            //BO.Code.File.LogInfo($"strOrig: {strOrig},strTo: {strTo}");
            _strTranslate = _strTranslate.Replace($"\"{strOrig}\"", $"\"{strTo}\"");
            //if (strOrig.Contains("{"))
            //{
            //    _strTranslate = _strTranslate.Replace(strOrig, strTo);
            //}
            //else
            //{
            //    _strTranslate = _strTranslate.Replace($"\"{strOrig}\"", $"\"{strTo}\"");
            //}



        }
    }
}
