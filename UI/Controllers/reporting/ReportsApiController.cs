using BL;
using Microsoft.AspNetCore.Mvc;
using SQLitePCL;
using System;
using System.Data.SqlTypes;
using System.Drawing.Printing;
using Telerik.Reporting.Services;
using Telerik.Reporting.Services.AspNetCore;

namespace UI.Controllers
{
    [Route("apix/reports")]
    public class ReportsApiController : ReportsControllerBase
    {
        

        public ReportsApiController(IReportServiceConfiguration reportServiceConfiguration, BL.Singleton.RunningApp app) : base(reportServiceConfiguration)
        {


            var resolver = new CustomReportSourceResolver(app);

            if (!System.IO.Directory.Exists(app.RootUploadFolder + "\\repservice"))
            {
                System.IO.Directory.CreateDirectory(app.RootUploadFolder + "\\repservice");
            }
            reportServiceConfiguration.Storage = new Telerik.Reporting.Cache.File.FileStorage(app.RootUploadFolder+"\\repservice");
            reportServiceConfiguration.HostAppId = $"ReportViewer{app.AppName}";    //HostAppId musí být unikátní v rámci všech websites na serveru
            

            reportServiceConfiguration.ReportSourceResolver = resolver;

        }

    }


    public class CustomReportSourceResolver : IReportSourceResolver
    {
        private BL.Singleton.RunningApp _app;
        

        public CustomReportSourceResolver(BL.Singleton.RunningApp app)
        {
            _app = app;
            
        }
        public Telerik.Reporting.ReportSource Resolve(string reportId, OperationOrigin operationOrigin, IDictionary<string, object> currentParameterValues)
        {
            //soubor sestavy###login uživatele###j72id###exportname###translate###caller_prefix###caller_pids###x31id
            List<string> lis = BO.Code.Bas.ConvertString2List(reportId, "###"); string strRepExportName = "Report";string strTranslate = null;string strCaller_Prefix = null; string strCaller_Pids = null;
            reportId = lis[0];
            string strLogin = lis[1];
            int intJ72ID_Query = 0;
            int intX31ID = 0;
            

            if (lis.Count > 2)
            {
                intJ72ID_Query = BO.Code.Bas.InInt(lis[2]);
            }
            if (lis.Count > 3 && !string.IsNullOrEmpty(lis[3]))
            {
                strRepExportName = lis[3];
            }
            if (lis.Count > 4)
            {
                strTranslate = lis[4];
            }
            if (lis.Count > 5)
            {
                strCaller_Prefix = BO.Code.Entity.GetPrefixDb(lis[5]);
            }
            if (lis.Count > 6)
            {
                strCaller_Pids = lis[6];
            }
            if (lis.Count > 7)
            {
                intX31ID = Convert.ToInt32(lis[7]);
            }


            var cu = new BO.RunningUser() { j02Login = strLogin };
            BL.Factory f = new BL.Factory(cu, _app, null, null);

            string strFullPath = $"{f.ReportFolder}\\{reportId}";
            if (!File.Exists(strFullPath))
            {
                strFullPath = $"{_app.RootUploadFolder}\\_distribution\\trdx\\{reportId}";
            }

            

            string reportXml = File.ReadAllText(strFullPath);

            if (_app.HostingMode == BL.Singleton.HostingModeEnum.SharedApp) //sdílená aplikace pro N databází
            {                
                reportXml = reportXml.Replace("ApplicationPrimary", _app.ParseConnectStringFromLogin(strLogin));
            }

            reportXml = reportXml.Replace("Name=\"report1\"", $"Name=\"{strRepExportName}\"");  //ovlivňuje název exportovního report PDF souboru

            if (reportXml.Contains("wwwroot/_users")) //grafické soubory jsou v _users složce
            {
                reportXml = reportXml.Replace("wwwroot/_users", $"{_app.RootUploadFolder}//_users");

                //BO.Code.File.LogInfo($"{_app.RootUploadFolder}//_users");
                
            }
            

            if (!string.IsNullOrEmpty(strCaller_Prefix) && !string.IsNullOrEmpty(strCaller_Pids))
            {
                if (reportXml.Contains("331=331"))
                {
                    switch (strCaller_Prefix)
                    {
                        case "p28":
                            reportXml = reportXml.Replace("331=331", $"p41x.p28ID_Client IN ({strCaller_Pids})");
                            break;
                        default:    //p41, j02, p91, p56, o23
                            reportXml = reportXml.Replace("331=331", $"a.{strCaller_Prefix}ID IN ({strCaller_Pids})");
                            break;
                    }
                }
                if (reportXml.Contains("141=141"))
                {
                    switch (strCaller_Prefix)
                    {
                        case "p28":
                            reportXml = reportXml.Replace("141=141", $"a.p28ID_Client IN ({strCaller_Pids})");
                            break;
                        case "p41":
                            reportXml = reportXml.Replace("141=141", $"a.{strCaller_Prefix}ID IN ({strCaller_Pids})");
                            break;
                    }
                }
                if (strCaller_Prefix=="j02" && reportXml.Contains("102=102"))
                {
                    reportXml = reportXml.Replace("102=102", $"a.{strCaller_Prefix}ID IN ({strCaller_Pids})");
                }
                if (reportXml.Contains("391=391"))
                {
                    switch (strCaller_Prefix)
                    {
                        case "p41":
                        case "j02":
                        case "o23":
                        case "p56":
                            reportXml = reportXml.Replace("391=391", $"a.p91ID IN (SELECT p91ID FROM p31Worksheet WHERE {strCaller_Prefix}ID IN ({strCaller_Pids}))");
                            break;                       
                        case "p28":
                        default:
                            reportXml = reportXml.Replace("391=391", $"a.{strCaller_Prefix}ID IN ({strCaller_Pids})");
                            break;
                    }
                }



            }
            if (intJ72ID_Query > 0 && (reportXml.Contains("331=331") || reportXml.Contains("1=1")))
            {
                var recJ72 = f.j72TheGridTemplateBL.Load(intJ72ID_Query);
                var mq = new BO.InitMyQuery(f.CurrentUser).Load(recJ72.j72Entity);
                mq.lisJ73 = f.j72TheGridTemplateBL.GetList_j73(intJ72ID_Query, recJ72.j72Entity.Substring(0, 3), 0);

                DL.FinalSqlCommand fq = DL.basQuery.GetFinalSql("", mq, cu);

                string strFilterAlias = f.j72TheGridTemplateBL.getFiltrAlias(recJ72.j72Entity.Substring(0, 3), mq);
                if (reportXml.Contains("331=331") && fq.SqlWhere !=null)
                {
                    
                    reportXml = reportXml.Replace("331=331", fq.SqlWhere).Replace("#query_alias#", strFilterAlias);
                }
                if (reportXml.Contains("1=1") && fq.SqlWhere !=null)
                {
                    reportXml = reportXml.Replace("1=1", fq.SqlWhere).Replace("#query_alias#", strFilterAlias);
                }
                
            }
            if (!string.IsNullOrEmpty(strTranslate))
            {
               
                reportXml = new UI.TheReportSupport().TranslateFromCzech(reportXml, f, strTranslate);

                
            }


            

            return new Telerik.Reporting.XmlReportSource { Xml = reportXml };
        }


        
    }


}