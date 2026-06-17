using Microsoft.AspNetCore.Mvc;
using ClosedXML.Excel;
using UI.Models.Reporting;
using BL;
using DocumentFormat.OpenXml.Drawing.Charts;
using UI.Models.p91oper;
using UI.Views.Shared.Components.myPeriod;
using DocumentFormat.OpenXml.Spreadsheet;

namespace UI.Controllers.reporting
{
    public class PierstoneController : BaseController
    {
        public PierstoneController(BL.Singleton.ThePeriodProvider pp)
        {
            _pp = pp;

        }
        private readonly BL.Singleton.ThePeriodProvider _pp;
        public IActionResult Index(string prefix, string pids, string guid_pids)
        {
            if (prefix == "le5") prefix = "p41";

            var v = new PierstoneViewModel() { prefix = prefix, pids = pids};
            if (!string.IsNullOrEmpty(guid_pids))
            {
                v.pids = Factory.p85TempboxBL.LoadByGuid(guid_pids).p85Message; //vstupní pids předány přes p85Tempbox
            }
            v.ReportLanguaage = Factory.CBL.LoadUserParam("pierstone-language", "cs");
            v.PageOrientation = Factory.CBL.LoadUserParam("pierstone-pageorientation","landscape");
            v.DataScope = Factory.CBL.LoadUserParam("pierstone-datascope", "time");


            if (string.IsNullOrEmpty(v.prefix) || BO.Code.Bas.ConvertString2ListInt(v.pids).Count()==0)
            {
                return this.StopPage(true, "pids or prefix missing");
            }

            RefreshState(v);

            return View(v);
        }

        private void RefreshState(PierstoneViewModel v)
        {
            v.lisPIDs = BO.Code.Bas.ConvertString2ListInt(v.pids);
            v.PeriodFilter = new myPeriodViewModel() { UserParamKey = "report-period" };
            v.PeriodFilter.LoadUserSetting(_pp, Factory);
            v.PeriodFilter.IsShowButtonRefresh = false;

            v.p31statequery = new UI.Models.p31StateQueryViewModel();
            v.p31statequery.UserParamKey = "pierstone-statequery";
            v.p31statequery.Value = Factory.CBL.LoadUserParamInt(v.p31statequery.UserParamKey, 0);
            
            var mq = new BO.myQueryP31() {p31statequery=v.p31statequery.Value, explicit_orderby = "p28Client.p28Name,p41x.p41Name,a.p31Date,j02.j02LastName",MyRecordsDisponible=true };
            switch (v.DataScope)
            {
                case "time":
                    mq.tabquery = "time";
                    break;
                default:
                    break;
            }
            if (v.PeriodFilter.d1 !=null && v.PeriodFilter.d2 != null)
            {
                mq.global_d1 = v.PeriodFilter.d1;
                mq.global_d2 = v.PeriodFilter.d2;
            }
            switch (v.prefix)
            {
                case "p28":
                    mq.p28ids = v.lisPIDs;break;
                case "p41":
                case "le5":
                    mq.p41ids = v.lisPIDs; break;
                case "j02":
                    mq.j02ids = v.lisPIDs; break;
            }

            
           
            v.lisP31 = Factory.p31WorksheetBL.GetList(mq);

            
        }

        [HttpPost]
        public IActionResult Index(PierstoneViewModel v)
        {
            RefreshState(v);
            if (v.lisP31.Count() == 0)
            {
                this.AddMessageTranslated("Žádné časové úkony ve filtrovaném období.");
                return View(v);
            }
            if (v.IsPostback)
            {

                return View(v);
            }

            if (ModelState.IsValid)
            {
                Handle_Export(v);

                Factory.CBL.SetUserParam("pierstone-language", v.ReportLanguaage);
                Factory.CBL.SetUserParam("pierstone-pageorientation", v.PageOrientation);
                Factory.CBL.SetUserParam("pierstone-datascope", v.DataScope);
            }




            return View(v);
        }

        private void Handle_Export(PierstoneViewModel v)
        {
            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("MARKTIME Export");
                bool bolSazby = Factory.CurrentUser.TestPermission(BO.PermValEnum.GR_AllowRates);

                if (v.PageOrientation=="landscape")
                {
                    worksheet.PageSetup.PageOrientation = XLPageOrientation.Landscape;
                }
                

                string strPeriod = null;
                if (v.PeriodFilter.d1 != null)
                {
                    strPeriod=$"{v.PeriodFilter.d1.Value.ToString("dd.MM.yyyy")} - {v.PeriodFilter.d2.Value.ToString("dd.MM.yyyy")}";
                }
                //worksheet.Range("C1").Value = strPeriod;
                string strGrandTotal_Hours = "0";
                string strGrandTotal_Total = "0";


                int row = 1;
                string strLastClient = null; string strLastProject = null; int intLastStartProjetRow = 4;int intProjectsCount = 0;

                foreach (var rec in v.lisP31)
                {
                    if (rec.p41Name != strLastProject && row> intLastStartProjetRow+1)
                    {
                        Handle_TotalRow(worksheet, row, intLastStartProjetRow,bolSazby,v.ReportLanguaage);
                        strGrandTotal_Hours += $"+D{row}";
                        strGrandTotal_Total += $"+F{row}";
                        row += 2;
                    }
                    if (rec.ClientName != strLastClient)
                    {
                        if (strLastClient != null)
                        {
                            row += 1;
                        }
                        worksheet.Range($"A{row}:B{row}").Merge();
                        worksheet.Range($"A{row}:F{row}").Style.Font.Bold = true;
                        worksheet.Range($"A{row}").Value = rec.ClientName;
                        worksheet.Range($"C{row}").Value = strPeriod;
                        worksheet.Range($"A{row}:F{row}").Style.Font.Bold = true;
                        worksheet.Range($"A{row}:G{row}").Style.Fill.BackgroundColor = XLColor.LightGray;
                        row += 1;
                    }
                    if (rec.p41Name != strLastProject)
                    {
                        worksheet.Range($"A{row}:F{row}").Merge();
                        worksheet.Range($"A{row}:F{row}").Style.Font.Bold = true;
                        worksheet.Range($"A{row}").Value = rec.p41Name;
                        worksheet.Range($"A{row}:F{row}").Style.Font.FontColor = XLColor.Navy;
                        worksheet.Range($"A{row}:G{row}").Style.Fill.BackgroundColor = XLColor.LightGray;
                        row += 1;
                        if (v.ReportLanguaage == "eng")
                        {
                            worksheet.Range($"A{row}").Value = "Date";
                            worksheet.Range($"B{row}").Value = "Name";
                            worksheet.Range($"C{row}").Value = "Text";
                            worksheet.Range($"D{row}").Value = "Hours";
                        }
                        else
                        {
                            worksheet.Range($"A{row}").Value = "Datum";
                            worksheet.Range($"B{row}").Value = "Jméno";
                            worksheet.Range($"C{row}").Value = "Text";
                            worksheet.Range($"D{row}").Value = "Hodiny";
                        }
                        
                        worksheet.Range($"A{row}:G{row}").Style.Fill.BackgroundColor = XLColor.LightGray;
                        if (bolSazby)
                        {
                            worksheet.Range($"E{row}").Value = (v.ReportLanguaage=="eng" ? "Rate" : "Sazba");
                            worksheet.Range($"F{row}").Value = (v.ReportLanguaage == "eng" ? "Total" : "Celkem");
                        }
                        intLastStartProjetRow = row;
                        intProjectsCount += 1;
                        row += 1;
                    }
                    worksheet.Cell(row, 1).Value = rec.p31Date;
                    worksheet.Cell(row, 2).Value = rec.Person;
                    worksheet.Cell(row, 3).Value = rec.p31Text;
                    worksheet.Cell(row, 3).Style.Alignment.WrapText = true;


                    if (rec.p33ID==BO.p33IdENUM.Cas || rec.p33ID == BO.p33IdENUM.Kusovnik)
                    {
                        worksheet.Cell(row, 4).Value = rec.p31Hours_Orig;

                        if (bolSazby)
                        {
                            worksheet.Cell(row, 5).Value = rec.p31Rate_Billing_Orig;
                            worksheet.Cell(row, 6).FormulaA1 = $"D{row}*E{row}";
                            worksheet.Cell(row, 7).Value = rec.j27Code_Billing_Orig;
                        }
                    }
                    else
                    {
                        worksheet.Cell(row, 6).Value = rec.p31Amount_WithoutVat_Orig;
                        worksheet.Cell(row, 7).Value = rec.j27Code_Billing_Orig;
                        if (rec.p34IncomeStatementFlag == BO.p34IncomeStatementFlagENUM.Vydaj)
                        {
                            worksheet.Range($"A{row}:G{row}").Style.Font.FontColor = XLColor.Brown;
                        }
                        else
                        {
                            worksheet.Range($"A{row}:G{row}").Style.Font.FontColor = XLColor.ForestGreen;
                        }
                        
                    }
                    


                    worksheet.Range($"D{row}:F{row}").Style.NumberFormat.Format = "#,##0.00";


                    strLastClient = rec.ClientName;
                    strLastProject = rec.p41Name;

                    row += 1;

                }

                Handle_TotalRow(worksheet, row, intLastStartProjetRow, bolSazby,v.ReportLanguaage);
                strGrandTotal_Hours += $"+D{row}";
                strGrandTotal_Total += $"+F{row}";

                

                if (intProjectsCount>1)
                {
                    row += 2;
                    worksheet.Range($"A{row}").Value = (v.ReportLanguaage == "eng" ? "Grand Total" : "Suma");
                    worksheet.Cell(row, 4).FormulaA1 = strGrandTotal_Hours;
                    if (bolSazby)
                    {
                        worksheet.Cell(row, 6).FormulaA1 = strGrandTotal_Total;
                    }
                    worksheet.Range($"A{row}:G{row}").Style.Fill.BackgroundColor = XLColor.LightGray;
                    worksheet.Range($"A{row}:F{row}").Style.Font.Bold = true;
                    worksheet.Range($"D{row}:F{row}").Style.NumberFormat.Format = "#,##0.00";
                }
                

                worksheet.Columns(4, 6).AdjustToContents();
                worksheet.Columns(1, 1).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Left);                
                worksheet.Columns(1, 1).Width = 15;
                //worksheet.Columns(3, 3).Style.Alignment.WrapText = true;
                worksheet.Columns(2, 2).Width = 20;
                worksheet.Columns(3, 3).Width = 60;
                worksheet.Columns(7, 7).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
                

                worksheet.Range($"A1:G{row}").Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                worksheet.Range($"A1:G{row}").Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                for(int i = 4; i <= row; i++)
                {
                    if (worksheet.Cell(i, 1).Value.IsBlank)
                    {
                        worksheet.Range($"A{i}:G{i}").Style.Border.InsideBorder = XLBorderStyleValues.None;
                        worksheet.Range($"A{i}:G{i}").Style.Border.OutsideBorder = XLBorderStyleValues.None;
                    }
                }


                worksheet.PageSetup.FitToPages(1, 0);

                v.TempFileName = $"marktime_export_{Factory.CurrentUser.pid}.xlsx";
                workbook.SaveAs($"{Factory.TempFolder}\\{v.TempFileName}");

            }
        }

        private void Handle_TotalRow(IXLWorksheet worksheet, int row, int intLastStartProjetRow,bool bolSazby,string strLang)
        {
            worksheet.Range($"A{row}").Value = (strLang == "eng" ? "Total" : "Celkem");
            worksheet.Range($"A{row}:F{row}").Style.Font.Bold = true;
            worksheet.Cell(row, 4).FormulaA1 = $"=SUM(D{intLastStartProjetRow + 1}:D{row - 1})";
            if (bolSazby)
            {
                worksheet.Cell(row, 6).FormulaA1 = $"=SUM(F{intLastStartProjetRow + 1}:F{row - 1})";
            }
            worksheet.Range($"D{row}:F{row}").Style.NumberFormat.Format = "#,##0.00";
            worksheet.Range($"A{row}:G{row}").Style.Fill.BackgroundColor = XLColor.LightGray;
        }
    }



}
