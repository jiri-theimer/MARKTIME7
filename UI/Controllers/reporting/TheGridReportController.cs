using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Text.Encodings.Web;
using UI.Models;
using UI.Views.Shared.Components.myPeriod;
using BL;
using DocumentFormat.OpenXml.Drawing.Charts;
using UI.Views.Shared.Components.myGrid;
using Microsoft.VisualBasic;

namespace UI.Controllers
{
    public class TheGridReportController : BaseController
    {
        private readonly BL.TheColumnsProvider _colsProvider;
        private readonly BL.Singleton.ThePeriodProvider _pp;
        private List<string> _lisColumnsReportWidths { get; set; }

        public TheGridReportController(BL.TheColumnsProvider cp, BL.Singleton.ThePeriodProvider pp)
        {
            _colsProvider = cp;
            _pp = pp;
        }

        public IActionResult ReportViewer(string guid)
        {
            var v = new TheGridReportViewModel() { guid = guid };
            v.TrdxRepDestFileName = v.guid + ".trdx";
            if (!System.IO.File.Exists(Factory.ReportFolder + "\\" + v.TrdxRepDestFileName))
            {
                return this.StopPage(true, "Nelze najít soubor: " + v.TrdxRepDestFileName);


            }

            return View(v);

        }

        public IActionResult Index(int j72id, string pids, string master_prefix, int master_pid, string guid, string p31guid,string guid_pids)
        {
            var v = new TheGridReportViewModel() { j72id = j72id, master_prefix = master_prefix, master_pid = master_pid, guid = guid, pids = pids, p31guid = p31guid };
            if (!string.IsNullOrEmpty(guid_pids))
            {
                v.pids = Factory.p85TempboxBL.LoadByGuid(guid_pids).p85Message; //vstupní pids předány přes p85Tempbox
            }
            if (string.IsNullOrEmpty(v.guid))
            {
                v.guid = Factory.CurrentUser.j02Login + "_temp_gridreport";
            }
            v.gridState = Factory.j72TheGridTemplateBL.LoadState(v.j72id, Factory.CurrentUser.pid);
            if (v.gridState.j75GroupDataField != null)
            {
                v.GroupByColumn = v.gridState.j75GroupDataField;
            }

            InhaleDefaults(v);

            RefreshState(v);


            return View(v);
        }

        [HttpPost]
        public IActionResult Index(Models.TheGridReportViewModel v)
        {
            RefreshState(v);

            if (v.IsPostback)
            {
                switch (v.PostbackOper)
                {
                    case "IsEnableResizer":
                        Factory.CBL.SetUserParam("thegridreport-isenableresizer", BO.Code.Bas.GB(v.IsEnableResizer));
                        break;
                    case "reset":
                        v.gridState.j75ColumnsReportWidth = null;
                        Factory.j72TheGridTemplateBL.SaveState(v.gridState, Factory.CurrentUser.j02ID);
                        RefreshState(v);
                        break;
                    case "resize":
                        double dblRatio = v.ZoomPercentage / 100.00;
                        double dblWidth = (1 - dblRatio) * BO.Code.Bas.InDouble(v.ResizedWidth);
                        dblWidth += BO.Code.Bas.InDouble(v.ResizedWidth);
                        SaveColWidth(v.GridColumns.First(p => p.UniqueName == v.ResizedField), dblWidth);
                        v.gridState.j75ColumnsReportWidth = String.Join("|", _lisColumnsReportWidths);
                        Factory.j72TheGridTemplateBL.SaveState(v.gridState, Factory.CurrentUser.j02ID);
                        RefreshState(v);
                        break;
                    case "orientation":
                        Factory.CBL.SetUserParam("thegridreport-orientation", v.PageOrientation.ToString());
                        break;
                    case "zoom":
                        Factory.CBL.SetUserParam("thegridreport-zoom", v.ZoomPercentage.ToString());
                        break;
                    case "toprecs":

                        Factory.CBL.SetUserParam("thegridreport-toprecs", v.MaxTopRecs.ToString());
                        break;
                }
                return View(v);
            }

            if (ModelState.IsValid)
            {


                v.SetJavascript_CallOnLoad(0);

            }

            return View(v);
        }

        private void RefreshState(Models.TheGridReportViewModel v)
        {
            if (v.gridState == null)
            {
                v.gridState = Factory.j72TheGridTemplateBL.LoadState(v.j72id, Factory.CurrentUser.pid);
            }


            _lisColumnsReportWidths = BO.Code.Bas.ConvertString2List(v.gridState.j75ColumnsReportWidth, "|");

            v.prefix = v.gridState.j72Entity.Substring(0, 3);

            v.TrdxRepDestFileName = v.guid + ".trdx";

            v.TrdxRepSourceFileName = FindRepFile(v);
            if (string.IsNullOrEmpty(v.TrdxRepSourceFileName))
            {
                this.AddMessage("V systému nelze najít template rep soubor.");
                return;
            }
            if (!System.IO.File.Exists(Factory.App.RootUploadFolder + "\\_distribution\\gridreport\\" + v.TrdxRepSourceFileName))
            {
                this.AddMessageTranslated(string.Format("V systému chybí template rep soubor {0}.", Factory.App.RootUploadFolder + "\\_distribution\\gridreport\\" + v.TrdxRepSourceFileName));
                return;
            }

            var mq = InhaleMyQuery(v, v.pids);
            


            v.GridColumns = mq.explicit_columns.ToList();
            if (v.PageBreakColumn != null)
            {
                v.PageBreakColumnInstance = v.GridColumns.First(p => p.UniqueName == v.PageBreakColumn);
            }
            if (v.GroupByColumn != null)
            {
                v.GroupByColumnInstance = v.GridColumns.First(p => p.UniqueName == v.GroupByColumn);

            }

            if (v.IsShowGroupsOnly && v.GroupByColumnInstance != null)
            {
                mq.explicit_columns = mq.explicit_columns.Where(p => p.UniqueName == v.GroupByColumnInstance.UniqueName || p.IsShowTotals);
                mq.explicit_columns.First(p => p.UniqueName == v.GroupByColumn).IsLastColInGrid = true;
                mq.explicit_columns = mq.explicit_columns.OrderByDescending(p => p.IsLastColInGrid);    //posunout groupby sloupec na první místo
                mq.explicit_sqlgroupby = v.GroupByColumnInstance.getFinalSqlSyntax_GROUPBY();
                mq.explicit_orderby = v.GroupByColumnInstance.getFinalSqlSyntax_GROUPBY();
                mq.TopRecordsOnly = 0;

            }
            else
            {
                v.IsShowGroupsOnly = false;
            }

            if (v.p31guid != null)
            {
                mq.master_prefix = "app";   //temp schvalování
            }
            if (mq.Prefix == "p31") //test, aby nedocházelo k nesmyslným součtům
            {
                foreach (var col in mq.explicit_columns.Where(p => p.Prefix != "p31"))
                {
                    col.IsShowTotals = false;
                }
            }

            var dt = Factory.gridBL.GetGridTable(mq, v.IsShowGroupsOnly);



            string strSQL = Factory.gridBL.GetLastFinalSql();       //vzít naposledy generovaný SQL v GRIDu
            if (strSQL.Contains("@p31guid"))
            {                
                strSQL = strSQL.Replace("@p31guid", $"'{v.p31guid}'");
            }
            var pars = Factory.gridBL.GetLastFinalGridParameters();
            if (pars != null)
            {
                foreach (var par in pars)
                {
                    if (strSQL.Contains("@" + par.ParName))
                    {
                        switch (par.ParValue.GetType().ToString())
                        {
                            case "System.String":
                                strSQL = strSQL.Replace("@", $"'{par.ParValue}'");
                                break;
                            case "System.DateTime":
                                strSQL = strSQL.Replace("@" + par.ParName, $"convert(datetime,'{BO.Code.Bas.ObjectDate2String(par.ParValue, "dd.MM.yyyy")}',104)");
                                break;
                            default:
                                strSQL = strSQL.Replace("@" + par.ParName, par.ParValue.ToString());
                                break;
                        }



                    }

                }
            }



            if (mq.global_d1 == null) mq.global_d1 = new DateTime(1900, 1, 1);
            if (mq.global_d2 == null) mq.global_d2 = new DateTime(3000, 1, 1);


            if (strSQL.Contains("@p31date1"))
            {
                strSQL = strSQL.Replace("@p31date1", $"convert(datetime,'{BO.Code.Bas.ObjectDate2String(mq.global_d1, "dd.MM.yyyy")}',104)");
                strSQL = strSQL.Replace("@p31date2", $"convert(datetime,'{BO.Code.Bas.ObjectDate2String(mq.global_d2, "dd.MM.yyyy")}',104)");
            }
            if (strSQL.Contains("o54InlineHtml"))
            {
                strSQL = strSQL.Replace("o54InlineHtml", "o54InlineText");
            }
            if (strSQL.Contains("<code>"))
            {                
                strSQL = BO.Code.Bas.Html2Text(strSQL);
            }
           



            double dblWidthComplete_CM = 0; int intColIndex = 0;
            double dblRatio = v.ZoomPercentage / 100.00;
            double dblDPI = 96;

            string strXmlContent = System.IO.File.ReadAllText(Factory.App.RootUploadFolder + "\\_distribution\\gridreport\\" + v.TrdxRepSourceFileName);

            var blocks = new List<BO.StringPair>();


            foreach (var col in mq.explicit_columns.Where(p => p.UniqueName != v.PageBreakColumn))
            {
                if ((col.Prefix == "p41" || col.Prefix.Substring(0, 1) == "l") && col.Header.Substring(0, 1) == "L" && col.Header.Length == 2)
                {
                    col.Header = Factory.getP07Level(Convert.ToInt32(col.Header.Substring(1, 1)), true);
                }

                if (!v.IsShowGroupsOnly && v.GroupByColumn != null && col.UniqueName == v.GroupByColumn)
                {
                    continue;   //vyhodit sloupec souhrnu z tabulky
                }

                double dblWidth_CM = LoadGridColWidth_CM(col, dblDPI, dblRatio);

                string strBlock = GenerateTrdx_getTableCell(col, intColIndex, dblWidth_CM, v);


                blocks.Add(new BO.StringPair() { Key = "TableCell", Value = strBlock });


                strBlock = $"<Column Width='{dblWidth_CM}cm' />";
                blocks.Add(new BO.StringPair() { Key = "Column", Value = strBlock });

                strBlock = GenerateTrdx_getColumnGroup(col, intColIndex, dblWidth_CM, v);


                blocks.Add(new BO.StringPair() { Key = "TableGroup", Value = strBlock });


                dblWidthComplete_CM += dblWidth_CM;
                intColIndex += 1;
            }



            string strFind = GenerateTrdx_FindBlock(ref strXmlContent, "<Cells>", "</Cells>");
            string strReplace = "<Cells>" + string.Join("", blocks.Where(p => p.Key == "TableCell").Select(p => p.Value)).Replace("'", "\"") + "</Cells>";
            GenerateTrdx_ParseResult(ref strXmlContent, strFind, strReplace);

            strFind = GenerateTrdx_FindBlock(ref strXmlContent, "<Columns>", "</Columns>");
            strReplace = "<Columns>" + string.Join("", blocks.Where(p => p.Key == "Column").Select(p => p.Value)).Replace("'", "\"") + "</Columns>";
            GenerateTrdx_ParseResult(ref strXmlContent, strFind, strReplace);

            strFind = GenerateTrdx_FindBlock(ref strXmlContent, "<ColumnGroups>", "</ColumnGroups>");
            strReplace = "<ColumnGroups>" + string.Join("", blocks.Where(p => p.Key == "TableGroup").Select(p => p.Value)).Replace("'", "\"") + "</ColumnGroups>";
            GenerateTrdx_ParseResult(ref strXmlContent, strFind, strReplace);

            strFind = ("<MarginsU Left='10mm' Right='5mm' Top='5mm' Bottom='5mm' />").Replace("'", "\"");
            strReplace = $"<MarginsU Left='{v.MarginLeft}mm' Right='{v.MarginRight}mm' Top='{v.MarginTop}mm' Bottom='{v.MarginBottom}mm' />";
            GenerateTrdx_ParseResult(ref strXmlContent, strFind, strReplace);




            if (v.PageBreakColumn != null)
            {

                GenerateTrdx_ParseResult(ref strXmlContent, "#sql_pageby#", HtmlEncoder.Default.Encode(CompletePageBreakBySql(strSQL, v.PageBreakColumnInstance)));
                strFind = "=Fields.page_record_name";
                strReplace = v.PageBreakColumnInstance.Header + ": {Fields.page_record_name}";
                GenerateTrdx_ParseResult(ref strXmlContent, strFind, strReplace);

                switch (v.PageBreakColumnInstance.NormalizedTypeName)
                {
                    case "string":
                        strSQL = strSQL.Replace($"SELECT TOP {v.MaxTopRecs}", $"SELECT TOP {v.MaxTopRecs} ISNULL({v.PageBreakColumnInstance.getFinalSqlSyntax_WHERE()},'0') as record_pid,");
                        break;
                    case "date":
                    case "num":
                        strSQL = strSQL.Replace($"SELECT TOP {v.MaxTopRecs}", $"SELECT TOP {v.MaxTopRecs} ISNULL({v.PageBreakColumnInstance.getFinalSqlSyntax_WHERE()},-9999) as record_pid,");
                        break;
                    default:
                        strSQL = strSQL.Replace($"SELECT TOP {v.MaxTopRecs}", $"SELECT TOP {v.MaxTopRecs} {v.PageBreakColumnInstance.getFinalSqlSyntax_WHERE()} as record_pid,");
                        break;

                }

            }
            //BO.Code.File.LogInfo(strSQL);
            GenerateTrdx_ParseResult(ref strXmlContent, "#sql#", HtmlEncoder.Default.Encode(strSQL)); //pro telerik report je třeba sql zahešovat

            if (!string.IsNullOrEmpty(v.Header))
            {
                v.Header = v.Header.Replace("&", "#");
            }

            GenerateTrdx_ParseResult(ref strXmlContent, "#header#", v.Header);

            if (v.GroupByColumn != null && !v.IsShowGroupsOnly)
            {

                strReplace = v.GroupByColumnInstance.UniqueName;

                strXmlContent = strXmlContent.Replace("Fields.groupby_field_select", "Fields." + strReplace);
                strXmlContent = strXmlContent.Replace("Fields.groupby_field_groupby", "Fields." + strReplace);
                strXmlContent = strXmlContent.Replace("Fields.groupby_field_orderby", "Fields." + strReplace);
                strXmlContent = strXmlContent.Replace("groupby_field_alias", v.GroupByColumnInstance.Header);


                double dblWidth = LoadGridColWidth_CM(v.GroupByColumnInstance, dblDPI, dblRatio);
                strXmlContent = strXmlContent.Replace("groupby_field_width", BO.Code.Bas.GN(dblWidth) + "cm");
            }



            System.IO.File.WriteAllText(Factory.ReportFolder + "\\" + v.TrdxRepDestFileName, strXmlContent);

            foreach (var c in v.GridColumns)
            {
                c.Rezerva = BO.Code.Bas.GN(Math.Round(LoadGridColWidth_Pixels(c) * dblRatio, 2));

            }
        }

        private double LoadGridColWidth_Pixels(BO.TheGridColumn col)
        {
            double w = LoadSavedColWidth(col); //vzít naposledy uloženou šířku sloupce
            if (w == 0)
            {
                w = Convert.ToDouble(col.FixedWidth);
            }
            if (w == 0)
            {
                w = 150.0;    //výchozí šířka sloupce v px
            }
            return w;
        }
        private double LoadGridColWidth_CM(BO.TheGridColumn col, double dblDPI, double dblRatio)
        {
            double w = LoadGridColWidth_Pixels(col);

            w = (w * 25.4) / dblDPI / 10;

            w = w * dblRatio;   //snížení šířky, protože font reportu je menší než zobrazení na displeji

            return Math.Round(w, 4);
        }

        private double LoadSavedColWidth(BO.TheGridColumn col)
        {
            foreach (string s in _lisColumnsReportWidths)
            {
                if (s.StartsWith(col.UniqueName))
                {
                    var arr = s.Split(";");
                    return BO.Code.Bas.InDouble(arr[1]);
                }
            }
            return 0;
        }
        private void SaveColWidth(BO.TheGridColumn col, double w)
        {
            if (w < 35)
            {
                w = 0;
            }
            for (int i = 0; i < _lisColumnsReportWidths.Count(); i++)
            {
                if (_lisColumnsReportWidths[i].StartsWith(col.UniqueName))
                {
                    var arr = _lisColumnsReportWidths[i].Split(";");
                    _lisColumnsReportWidths[i] = col.UniqueName + ";" + w.ToString();
                    return;
                }
            }
            _lisColumnsReportWidths.Add(col.UniqueName + ";" + w.ToString());
        }

        private string GenerateTrdx_FindBlock(ref string strContent, string strStartElement, string strEndElement)
        {
            int x = strContent.IndexOf(strStartElement);
            int y = strContent.IndexOf(strEndElement, x + 1);
            if (x == -1 || y == -1)
            {
                return "???##not-merged: " + strStartElement;
            }
            return strContent.Substring(x, strEndElement.Length + y - x);
        }
        private string GenerateTrdx_getColumnGroup(BO.TheGridColumn col, int intColIndex, double dblWidth_CM, TheGridReportViewModel v)
        {
            var s = new System.Text.StringBuilder();
            string strW = BO.Code.Bas.GN(dblWidth_CM) + "cm"; string strBgColor = "242, 242, 242";
            if (v.GroupByColumn != null)
            {
                strBgColor = "217, 217, 217";
            }
            s.AppendLine($"<TableGroup Name='{col.UniqueName + "_group" + intColIndex.ToString()}'>");
            s.AppendLine("<ReportItem>");
            s.AppendLine($"<TextBox Width='{strW}' Height='0.5cm' Left='0cm' Top='0cm' Value='{col.Header}' Name='textBox_header{intColIndex}' StyleName='Office.TableHeader'>");
            s.AppendLine($"<Style BackgroundColor='{strBgColor}' TextAlign='Center'>");
            s.AppendLine("<Font Size='8pt' Bold='True' />");
            s.AppendLine("</Style>");
            s.AppendLine("</TextBox>");
            s.AppendLine("</ReportItem>");
            s.AppendLine("</TableGroup>");

            return s.ToString();
        }

        private void GenerateTrdx_ParseResult(ref string strContent, string strFind, string strReplace)
        {
            //strContent = strContent.Replace(strFind, strReplace,StringComparison.OrdinalIgnoreCase);
            strContent = strContent.Replace(strFind, strReplace);
        }
        private string GenerateTrdx_getTableCell(BO.TheGridColumn col, int intColIndex, double dblWidth_CM, TheGridReportViewModel v)
        {
            var s = new System.Text.StringBuilder();
            string strW = BO.Code.Bas.GN(dblWidth_CM) + "cm"; string strValue = "Fields." + col.UniqueName; string strFormat = null; string strAlign = "Left";
            switch (col.FieldType)
            {
                case "num":
                    strFormat = "Format='{0:N2}'";
                    strAlign = "Right";
                    if (col.IsHours && Factory.CurrentUser.j02DefaultHoursFormat == "T")
                    {
                        strFormat = null;
                        strValue = $"UI.Code.basTelerikReporting.ShowAsHHMM(Fields.{col.UniqueName})";
                        
                    }
                    break;
                case "num0":
                    strFormat = "Format='{0:N0}'";
                    strAlign = "Right";
                    break;
                case "date":
                    strFormat = "Format='{0:d}'";
                    break;
            }

            s.AppendLine($"<TableCell RowIndex='0' ColumnIndex='{intColIndex}' RowSpan='1' ColumnSpan='1'>");
            s.AppendLine("<ReportItem>");
            s.AppendLine($"<TextBox Width='{strW}' Height='0.5cm' Left='0cm' Top='0cm' Value='= {strValue}' {strFormat} Name='textBox_col{intColIndex}' StyleName='Office.TableBody'>");
            s.AppendLine($"<Style TextAlign='{strAlign}' VerticalAlign='Middle'>");
            s.AppendLine("<Font Size='8pt' />");
            s.AppendLine("<Padding Left='0.1cm' Right='0.1cm' Top='0.1cm' />");
            s.AppendLine("</Style>");
            s.AppendLine("</TextBox>");
            s.AppendLine("</ReportItem>");
            s.AppendLine("</TableCell>");

            if (col.IsShowTotals)
            {
                strValue = $"Value='=sum(Fields.{col.UniqueName})'";
                if (col.IsHours && Factory.CurrentUser.j02DefaultHoursFormat == "T")
                {
                    strValue = $"Value='=UI.Code.basTelerikReporting.ShowAsHHMM(sum(Fields.{col.UniqueName}))'";
                }
            }
            else
            {
                strValue = "";
            }

            s.AppendLine($"<TableCell RowIndex='1' ColumnIndex='{intColIndex}' RowSpan='1' ColumnSpan='1'>");
            s.AppendLine("<ReportItem>");
            s.AppendLine($"<TextBox Width='{strW}' Height='0.5cm' Left='0cm' Top='0cm' {strValue} {strFormat} Name='textBox_sum_col{intColIndex}' StyleName='Office.TableBody'>");
            s.AppendLine($"<Style BackgroundColor='242, 242, 242' TextAlign='{strAlign}' VerticalAlign='Middle'>");
            s.AppendLine("<Font Size='8pt' Bold='True' />");
            s.AppendLine("<Padding Left='0.1cm' Right='0.1cm' Top='0.1cm' />");
            s.AppendLine("</Style>");
            s.AppendLine("</TextBox>");
            s.AppendLine("</ReportItem>");
            s.AppendLine("</TableCell>");

            if (v.GroupByColumn != null)
            {
                s.AppendLine($"<TableCell RowIndex='2' ColumnIndex='{intColIndex}' RowSpan='1' ColumnSpan='1'>");
                s.AppendLine("<ReportItem>");
                s.AppendLine($"<TextBox Width='{strW}' Height='0.5cm' Left='0cm' Top='0cm' {strValue} {strFormat} Name='textBox_groupsum_col{intColIndex}' StyleName='Office.TableBody'>");
                s.AppendLine($"<Style BackgroundColor='217, 217, 217' TextAlign='{strAlign}' VerticalAlign='Middle'>");
                s.AppendLine("<Font Size='8pt' Bold='True' />");
                s.AppendLine("<Padding Left='0.1cm' Right='0.1cm' Top='0.1cm' />");
                s.AppendLine("</Style>");
                s.AppendLine("</TextBox>");
                s.AppendLine("</ReportItem>");
                s.AppendLine("</TableCell>");
            }

            return s.ToString();
        }

        private void InhaleDefaults(TheGridReportViewModel v)
        {
            v.ZoomPercentage = Factory.CBL.LoadUserParamInt("thegridreport-zoom", 70);
            v.MaxTopRecs = Factory.CBL.LoadUserParamInt("thegridreport-toprecs", 2000);
            v.PageOrientation = Factory.CBL.LoadUserParamInt("thegridreport-orientation", 1);
            v.MarginLeft = Factory.CBL.LoadUserParamInt("thegridreport-marginleft", 10);
            v.MarginRight = Factory.CBL.LoadUserParamInt("thegridreport-marginright", 0);
            v.MarginTop = Factory.CBL.LoadUserParamInt("thegridreport-margintop", 0);
            v.MarginBottom = Factory.CBL.LoadUserParamInt("thegridreport-marginbottom", 0);
            v.IsEnableResizer = Factory.CBL.LoadUserParamBool("thegridreport-isenableresizer", false);
        }

        private BO.baseQuery InhaleMyQuery(TheGridReportViewModel v, string pids)
        {
            string myqueryinline = null;
            if (string.IsNullOrEmpty(v.master_prefix) && (v.prefix == "p31" || v.prefix == "j02"))
            {
                var tab = Factory.CBL.LoadUserParam("overgrid-tab-" + v.prefix, "zero", 1);
                if (tab != null && tab != "zero")
                {
                    myqueryinline = "tabquery|string|" + tab;
                }
            }
            if (v.gridState.j72MasterEntity == "approve" && v.p31guid != null)
            {
                myqueryinline = "tempguid|string|" + v.p31guid; //schvalování úkonů - temp tabulka p31worksheet_temp

            }

            var basMyQuerySupport =new BO.InitMyQuery(Factory.CurrentUser);
            var mq = basMyQuerySupport.Load(v.gridState.j72Entity, v.master_prefix, v.master_pid, myqueryinline);

            mq.TopRecordsOnly = v.MaxTopRecs;
            mq.SetPids(pids);

            string strUserParam = $"grid-{v.prefix}-recordbinquery";
            if (v.master_prefix != null)
            {
                strUserParam = $"grid-{v.prefix}-{v.master_prefix}-recordbinquery";
            }

            switch (Factory.CBL.LoadUserParamInt(strUserParam, 1))
            {
                case 1:
                    mq.IsRecordValid = true; break;   //pouze otevřené záznamy
                case 2:
                    mq.IsRecordValid = false; break;  //pouze záznamy v archivu
                default:
                    mq.IsRecordValid = null; break;
            }
            mq.MyRecordsDisponible = true;

            mq.explicit_columns = _colsProvider.ParseTheGridColumns(v.prefix, v.gridState.j72Columns, Factory);

            if (v.gridState.j75GroupDataField != null && v.gridState.j75GroupLastValue != null)
            {
                //ve zdrojovém gridu je nastavený souhrn podle sloupce
                var thegridcol = mq.explicit_columns.Where(p => p.UniqueName == v.gridState.j75GroupDataField).First();
                basMyQuerySupport.Handle_GroupBy_ExplicitSqlWhere(v.gridState, thegridcol, mq);
                
            }
            


            if (!String.IsNullOrEmpty(v.gridState.j75Filter)) //sloupcový filtr
            {
                mq.TheGridFilter = _colsProvider.ParseAdhocFilterFromString(v.gridState.j75Filter, mq.explicit_columns);
                mq.IsNotUseParams_Sql_InGridFilter = true;
            }
            if (v.gridState.j72HashJ73Query)  //vnitřní filtr přehledu
            {
                mq.lisJ73 = this.Factory.j72TheGridTemplateBL.GetList_j73(v.gridState.j72ID, v.gridState.j72Entity.Substring(0, 3),v.j72id_query);
            }

            if (v.p31guid == null)
            {
                if (string.IsNullOrEmpty(v.master_prefix))
                {
                    mq.p31statequery = Factory.CBL.LoadUserParamInt("grid-" + v.prefix + "-p31statequery", 0);
                    mq.p31tabquery = Factory.CBL.LoadUserParam("grid-" + v.prefix + "-p31formatquery");
                }
                else
                {
                    mq.p31statequery = Factory.CBL.LoadUserParamInt("grid-" + v.prefix + "-" + BO.Code.Entity.GetEntity(v.master_prefix) + "-p31statequery", 0);
                    mq.p31tabquery = Factory.CBL.LoadUserParam("grid-" + v.prefix + "-" + BO.Code.Entity.GetEntity(v.master_prefix) + "-p31formatquery");
                }
            }



            var p1 = new myPeriodViewModel() { prefix = v.prefix, IsShowButtonRefresh = true };
            if (string.IsNullOrEmpty(v.master_prefix))
            {
                p1.UserParamKey = $"grid-period-{v.prefix}";
            }
            else
            {
                p1.UserParamKey = $"grid-period-{v.prefix}-{BO.Code.Entity.GetEntity(v.master_prefix)}";
            }
            if (v.gridState.j72MasterEntity == "approve_aio")
            {
                p1.UserParamKey = "approve-index-period";   //schvalovací rozhraní
            }
            

            p1.LoadUserSetting(_pp, Factory);
            mq.period_field = p1.PeriodField;
            mq.global_d1 = p1.d1;
            mq.global_d2 = p1.d2;

            if (v.gridState.j75SortDataField != null && mq.explicit_columns.Any(p => p.UniqueName == v.gridState.j75SortDataField))
            {
                var colSort = mq.explicit_columns.First(p => p.UniqueName == v.gridState.j75SortDataField);
                mq.explicit_orderby = $"{colSort.getFinalSqlSyntax_ORDERBY()} {v.gridState.j75SortOrder}";
            }

            return mq;
        }

        private string FindRepFile(TheGridReportViewModel v)
        {
            string s = "thereport_export_template_portrait.trdx";
            if (v.GroupByColumn != null && !v.IsShowGroupsOnly)
            {
                s = "thereport_export_template_1souhrn_portrait.trdx";
            }
            if (!string.IsNullOrEmpty(v.PageBreakColumn))
            {
                s = "thereport_export_template_portrait_pageby.trdx";
                if (v.GroupByColumn != null)
                {
                    s = "thereport_export_template_1souhrn_portrait_pageby.trdx";
                }
            }

            if (v.IsLandScape)
            {
                s = "thereport_export_template_landscape.trdx";
                if (v.GroupByColumn != null && !v.IsShowGroupsOnly)
                {
                    s = "thereport_export_template_1souhrn_landscape.trdx";
                }
                if (v.PageBreakColumn != null)
                {
                    s = "thereport_export_template_landscape_pageby.trdx";
                    if (v.GroupByColumn != null)
                    {
                        s = "thereport_export_template_1souhrn_landscape_pageby.trdx";
                    }
                }
            }


            return s;
        }


        private string CompletePageBreakBySql(string strSQL, BO.TheGridColumn col)
        {

            int x = strSQL.IndexOf(" FROM ");
            strSQL = BO.Code.Bas.RightString(strSQL, strSQL.Length - x);
            int y = strSQL.IndexOf(" ORDER BY ");
            if (y > 0)
            {
                strSQL = strSQL.Substring(0, y);
            }

            string s = "SELECT DISTINCT";
            switch (col.NormalizedTypeName)
            {
                case "string":
                    s += " ISNULL(" + col.getFinalSqlSyntax_WHERE() + ",'0') AS page_record_pid," + col.getFinalSqlSyntax_WHERE() + " AS page_record_name";
                    break;
                case "num":
                    s += " ISNULL(" + col.getFinalSqlSyntax_WHERE() + ",-9999) AS page_record_pid," + col.getFinalSqlSyntax_WHERE() + " AS page_record_name";
                    break;
                case "date":
                    s += " ISNULL(" + col.getFinalSqlSyntax_WHERE() + ",0) AS page_record_pid," + col.getFinalSqlSyntax_WHERE() + " AS page_record_name";
                    break;
                default:
                    s += " " + col.getFinalSqlSyntax_WHERE() + " AS page_record_pid," + col.getFinalSqlSyntax_WHERE() + " AS page_record_name";
                    break;
            }

            s += strSQL;

            s += " ORDER BY " + col.getFinalSqlSyntax_WHERE();

            return s;
        }


       
    }
}
