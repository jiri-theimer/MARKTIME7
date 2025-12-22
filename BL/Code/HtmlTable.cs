
using System.Data;
using System.Diagnostics;
using ClosedXML.Excel;


namespace BL.Code
{
    public class HtmlTable
    {
        public string HeaderBgColor { get; set; }
        public string ColHeaders { get; set; }
        public string ColTypes { get; set; }    //bool|num0|num|date|datetime|string
        public string ColFlexSubtotals { get; set; }    //0|0|1|11 - 1 je souhrn, 11 je součet, 0 - nic
        public string ColClasses { get; set; }
        public string ColStyles { get; set; }
        public string ColHeaderStyles { get; set; }
        public string ClientID { get; set; }
        public bool IsUseDatatables { get; set; }
        public bool IsFullWidth { get; set; } = true;   //tabulka bude mít auto 100% šířku
        public string Sql { get; set; }
        public bool IsCanExport { get; set; } = true;
        public string MailSubject { get; set; }
        public string TableAfterCaption { get; set; }
        public string TableCaption { get; set; }
    }
    public class BufferDataRow
    {
        public int RowIndex { get; set; }
        public DataRow dbRow { get; set; }
        public int SubTotalLevel { get; set; }
        public string subval1 { get; set; }
        public string subval2 { get; set; }
        public string subval3 { get; set; }

    }
    public class Datatable2Html
    {
        private HtmlTable _def;
        private System.Text.StringBuilder _sb;
        private List<string> _headers;
        private List<string> _types;
        private List<string> _subtotals;
        private List<string> _classes;
        private List<string> _styles;
        private List<string> _headerstyles;
        private List<BufferDataRow> _buffer;
        private bool _isSubtotals { get; set; }
        private int _SubTotalColsCount { get; set; }
        
        private List<string> _backcolors { get; set; }
        private List<string> _forecolors { get; set; }
        public string Guid { get; set; }
        private string _TableClientID { get; set; }

        public string TableClientID { get
            {
                return _TableClientID;
            }
        }

        private BL.Factory _f;
        private BO.Model.Grid.HtmlTableLayout _tablelayout { get; set; }

        public Datatable2Html(HtmlTable def,BL.Factory f)
        {
            _f = f;
            _def = def;
            _tablelayout= new BO.Model.Grid.HtmlTableLayout(_f.CurrentUser.j02GridCssBitStream, _f.CurrentUser.j02FontSizeFlag,false);

            this.Guid = BO.Code.Bas.GetGuid();
            _sb = new System.Text.StringBuilder();
            _buffer = new List<BufferDataRow>();
            _headers = BO.Code.Bas.ConvertString2List(def.ColHeaders, "|");
            _types = BO.Code.Bas.ConvertString2List(def.ColTypes, "|");
            _subtotals = BO.Code.Bas.ConvertString2List(def.ColFlexSubtotals, "|");
            _classes = BO.Code.Bas.ConvertString2List(def.ColClasses, "|");
           
            def.ColStyles += "||||||||||||||||||||||||||||||||||||||||";
            _styles = BO.Code.Bas.ConvertString2List(def.ColStyles, "|");

            def.ColHeaderStyles += "||||||||||||||||||||||||||||||||||||||||";
            _headerstyles = BO.Code.Bas.ConvertString2List(def.ColHeaderStyles, "|");

            if (_subtotals.Contains("1"))
            {
                _isSubtotals = true;
                _SubTotalColsCount = _subtotals.Where(p => p == "1").Count();
            }
            if (string.IsNullOrEmpty(def.ClientID)) def.ClientID = "tab"+this.Guid.Substring(0,10);
            _TableClientID = def.ClientID;
            _backcolors = new List<string> { "#CFE2FF", "#FFF8DC", "#F8D7DA" };
            
            _forecolors = new List<string> { "#4682B4", "#DAA520", "brown" };
        }

        private void prepare_buffer(System.Data.DataTable dt)
        {
            int x = 1000; string dbLastVal0 = null; string dbLastVal1 = null; string dbLastVal2 = null; int intLastRowIndex = 0; int y = 0;
            foreach (System.Data.DataRow dbRow in dt.Rows)
            {
                _buffer.Add(new BufferDataRow() { dbRow = dbRow, RowIndex = x }); //zdrojový záznam 
                x += 1000;
            }

            var qry = _buffer.GetRange(0, _buffer.Count());

            for (int i = 0; i < _subtotals.Count(); i++)
            {
                if (_subtotals[i] == "1")
                {
                    //subtotal
                    y = 0;
                    foreach (BufferDataRow row in qry)
                    {

                        if (i == 0 && dbLastVal0 != null && gg(row.dbRow, 0) != dbLastVal0)
                        {
                            var c = new BufferDataRow() {dbRow= dt.NewRow(), SubTotalLevel = 1, subval1 = dbLastVal0, RowIndex = intLastRowIndex + 10 };  //souhrn záznam level1
                            _buffer.Add(c);
                            
                        }

                        if (i == 1 && dbLastVal1 != null && gg(row.dbRow, 0) == dbLastVal0 && gg(row.dbRow, 1) != dbLastVal1)
                        {
                            var c = new BufferDataRow() { dbRow = dt.NewRow(), SubTotalLevel = 2, subval1 = dbLastVal0, subval2 = dbLastVal1, RowIndex = intLastRowIndex + 9 };  //souhrn záznam level2
                            _buffer.Add(c);
                            
                        }
                        if (i == 1 && dbLastVal0 != null && dbLastVal1 !=null && gg(row.dbRow, 0) != dbLastVal0)
                        {
                            var c = new BufferDataRow() { dbRow = dt.NewRow(), SubTotalLevel = 2, subval1 = dbLastVal0, subval2 = dbLastVal1, RowIndex = intLastRowIndex + 9 };  //poslední souhrn záznam level2
                           _buffer.Add(c);
                            
                        }

                        if (i == 2 && dbLastVal2 != null && gg(row.dbRow, 0) == dbLastVal0 && gg(row.dbRow, 1) == dbLastVal1 && gg(row.dbRow, 2) != dbLastVal2)
                        {
                            var c = new BufferDataRow() { dbRow = dt.NewRow(), SubTotalLevel = 3, subval1 = dbLastVal0, subval2 = dbLastVal1, subval3 = dbLastVal2, RowIndex = intLastRowIndex + 8 };  //souhrn záznam level3
                            _buffer.Add(c);
                            
                        }
                        if (i == 2 && dbLastVal2 != null && gg(row.dbRow, 0) == dbLastVal0 && gg(row.dbRow, 1) != dbLastVal1)
                        {
                            var c = new BufferDataRow() { dbRow = dt.NewRow(), SubTotalLevel = 3, subval1 = dbLastVal0, subval2 = dbLastVal1, subval3 = dbLastVal2, RowIndex = intLastRowIndex + 8 };  //poslední souhrn záznam level3
                            _buffer.Add(c);
                            
                        }


                        dbLastVal0 = gg(row.dbRow, 0);
                        if (i >= 1) dbLastVal1 = gg(row.dbRow, 1);
                        if (i >= 2) dbLastVal2 = gg(row.dbRow, 2);
                        
                        intLastRowIndex = row.RowIndex;
                        
                        y += 1;
                        
                    }

                    if (i == 0 && qry.Count() > 0)
                    {
                        var c = new BufferDataRow() { dbRow = dt.NewRow(), SubTotalLevel = 1, subval1 = dbLastVal0, RowIndex = intLastRowIndex + 10 };  //poslední záznam level1
                        _buffer.Add(c);
                        
                    }
                    if (i == 1 && qry.Count() > 0)
                    {
                        var c = new BufferDataRow() { dbRow = dt.NewRow(), SubTotalLevel = 2, subval1 = dbLastVal0, subval2 = dbLastVal1, RowIndex = intLastRowIndex + 9 };  //poslední záznam level2
                        _buffer.Add(c);
                        
                    }
                    if (i == 2 && qry.Count() > 0)
                    {
                        var c = new BufferDataRow() { dbRow = dt.NewRow(), SubTotalLevel = 3, subval1 = dbLastVal0, subval2 = dbLastVal1, subval3 = dbLastVal2, RowIndex = intLastRowIndex + 8 };  //poslední souhrn záznam level3
                        _buffer.Add(c);
                        
                    }

                }

                

            }

            prepare_buffer_sums();
        }

        private void prepare_buffer_sums()
        {
            for (int i = 0; i < _subtotals.Count(); i++)
            {
                if (_subtotals[i] == "11")
                {
                    foreach (BufferDataRow row in _buffer.Where(p => p.SubTotalLevel == 1))
                    {
                       
                        var qry = _buffer.Where(p => p.SubTotalLevel == 0 && p.dbRow[0] !=System.DBNull.Value && p.dbRow[i] !=System.DBNull.Value && Convert.ToString(p.dbRow[0]) == row.subval1);
                        if (qry.Count() > 0)
                        {
                            row.dbRow[i] = qry.Sum(p => Convert.ToDouble(p.dbRow[i]));
                        }
                        

                    }
                    foreach (BufferDataRow row in _buffer.Where(p => p.SubTotalLevel == 2))
                    {
                        var qry = _buffer.Where(p => p.SubTotalLevel == 0 && p.dbRow[1] != System.DBNull.Value && p.dbRow[i] != System.DBNull.Value && Convert.ToString(p.dbRow[0]) == row.subval1 && Convert.ToString(p.dbRow[1]) == row.subval2);
                        if (qry.Count() > 0)
                        {
                            row.dbRow[i] = qry.Sum(p => Convert.ToDouble(p.dbRow[i]));
                        }
                    }
                    foreach (BufferDataRow row in _buffer.Where(p => p.SubTotalLevel == 3))
                    {
                        var qry = _buffer.Where(p => p.SubTotalLevel == 0 && p.dbRow[2] != System.DBNull.Value && p.dbRow[i] != System.DBNull.Value && Convert.ToString(p.dbRow[0]) == row.subval1 && Convert.ToString(p.dbRow[1]) == row.subval2 && Convert.ToString(p.dbRow[2]) == row.subval3);
                        if (qry.Count() > 0)
                        {
                            row.dbRow[i] = qry.Sum(p => Convert.ToDouble(p.dbRow[i]));
                        }
                    }
                }
            }
        }



        private string gg(DataRow dbRow, int i)
        {
            if (dbRow[i] == System.DBNull.Value) return "";
            return dbRow[i].ToString();
        }

        

        public string CreateHtmlTable(System.Data.DataTable dt, int maxrows = 5000)
        {
            
            prepare_buffer(dt);

            if (_buffer.Count() == 0) return null;
            
            if (_def.IsCanExport)
            {
                var recP85 = new BO.p85Tempbox() { p85GUID = this.Guid, p85FreeText02 = _def.ColTypes, p85FreeText03 = _def.ColFlexSubtotals, p85FreeText04 = _def.ColStyles, p85FreeText05 = _def.ColClasses, p85Message = _def.ColHeaders+"$$$$$$$"+ _def.Sql, p85FreeText06 = _def.HeaderBgColor };
                _f.p85TempboxBL.Save(recP85);
            }

            sb("<div>");

            if (_def.IsCanExport)
            {
                
                sb($"<a title='Zkopírovat do schránky' href=\"javascript:_copy_element_to_clipboard('{_def.ClientID}')\" style='margin-left:10px;'><span class='material-icons-outlined-btn'>content_copy</span></a>");
                sb($"<a title='Odeslat e-mail' href=\"javascript:_html2mail('{_def.ClientID}','{_def.MailSubject}','send_message_template_htmltable.html')\" style='margin-left:10px;'><span class='material-icons-outlined-btn'>alternate_email</span></a>");
                sb($"<a style='margin-left:10px;' title='XLS Export' href='/HtmlTable2Xls/Index?guid={this.Guid}'><span class='material-icons-outlined-btn'>file_download</span>XLS</a>");
                
            }

            sb("</div>");

           
            if (_def.IsUseDatatables)
            {
                sb("<table class='display compact' style='width:100%;'");       //plugin datatables
            }
            else
            {
                if (_def.IsFullWidth)
                {
                    sb("<table class='htmltable' style='border-collapse: collapse; table-layout: auto;width:100%;border: solid 1px #CCCCCC;'");
                    
                }
                else
                {
                    //fixed tabulka
                    sb("<table class='htmltable' style='border-collapse: collapse; table-layout: fixed;border: solid 1px #CCCCCC;'");
                    
                }
                

            }
           
            sb($" id='{_def.ClientID}'>");
           
            if (_def.TableCaption != null)
            {
                sb($"<caption style='caption-side: top'>{_def.TableCaption}</caption>");
            }
            if (_def.TableAfterCaption != null)
            {
                sb($"<caption>{_def.TableAfterCaption}</caption>");
            }

            handle_colgroup();
            handle_headers();
            handle_body(maxrows);
            handle_foot();
            sb("</table>");
           
            

            return _sb.ToString();
        }

        private void handle_colgroup()
        {
            sb("<colgroup>");
            for (int i = 0; i < _headers.Count(); i++)
            {
                sb("<col");
                if (_classes.Count() >= i+1)
                {
                    sb($" class='{_classes[i]}'");
                }
                

                sb(">");
            }

            sb("</colgroup>");
        }

        private void handle_headers()
        {
            sb("<thead>");
            if (_def.HeaderBgColor != null)
            {
                sb($"<tr style='background-color:{_def.HeaderBgColor};'");
            }
            else
            {
                sb("<tr");
            }
            if (_tablelayout.Mrizka)
            {
                sb(" class='mrizka'");
            }
            sb(">");

            for (int i = 0; i < _headers.Count(); i++)
            {
                sb("<th style='font-weight:bold");
                if (_types[i] =="N" || _types[i] == "HHMM")
                {
                    sb(";text-align:right;padding-right:10px");
                }
                if (_styles[i] != "" && _styles[i] != "0")
                {
                    sb(";" + _styles[i]);
                }
                if (_headerstyles[i] != "" && _headerstyles[i] != "0")
                {
                    sb(";" + _headerstyles[i]);
                }
                sb("'>");
                
               
                sb(_headers[i]);
                
                sb("</th>");
            }
           
            sb("</tr></thead>");
        }
        private void handle_foot()
        {
            if (_buffer.Count()==0 || !_subtotals.Contains("11")) return;

            sb("<tfoot><tr class='trfoot'>");
            
            for (int i = 0; i < _headers.Count(); i++)
            {
                sb("<td style='font-weight:bold");
                if (_types[i] == "N" || _types[i] == "HHMM")
                {
                    sb(";text-align:right;padding-right:10px");
                }
                if (_styles[i] != "" && _styles[i] != "0")
                {
                    sb(";" + _styles[i]);
                }
               
                sb("'>");
                if (i == 0)
                {
                    sb("∑");
                }
                if (_subtotals[i] == "11")
                {
                    var qry = _buffer.Where(p => p.SubTotalLevel == 0 && p.dbRow[i] != System.DBNull.Value);
                    if (qry.Count() > 0)
                    {
                        sb(string.Format("{0:#,0.00}", qry.Sum(p => Convert.ToDouble(p.dbRow[i]))));
                    }
                }
                

                sb("</td>");
            }

            sb("</tr></tfoot>");
            
        }
        private void handle_body(int intLimitRows = 5000)
        {
            
            sb("<tbody>");

            int x = 0;
            string strVal = "";
            string strLastVal0 = null; string strLastVal1 = null; string strLastVal2 = null;
            
            string strTdVal = null;
            string strDataSort = null;
            List<string> styles;
            var lisBuffer = _buffer.OrderBy(p => p.RowIndex).ThenBy(p => p.SubTotalLevel);
            string strBaseTrClass = null;
            
            if (_tablelayout.Lineheight != null)
            {
                strBaseTrClass = $"{strBaseTrClass} {_tablelayout.Lineheight}";
            }
            if (_tablelayout.Mrizka)
            {
                strBaseTrClass = $"{strBaseTrClass} mrizka";
            }
            
            int x01 = 0;
            bool bolDoubleClick = (_types[0] == "RCM" ? true: false);            

            foreach (var row in lisBuffer)
            {
                x01 += 1;
                string strTrClass = strBaseTrClass;
                if (_tablelayout.Stripped && x01 == 2)
                {
                    strTrClass = $"{strTrClass} stripped";
                    x01 = 0;
                }
                if (strTrClass != null)
                {
                    strTrClass = $"class='{strTrClass.Trim()}'";
                }
               
                if (row.SubTotalLevel == 1)
                {
                    sb($"<tr {strTrClass} style='font-weight:bold;background-color:{_backcolors[0]};color:{_forecolors[0]}'>");
                }
                else
                {
                    if (bolDoubleClick)
                    {
                        sb($"<tr {strTrClass} ondblclick=\"_edit('{row.dbRow["prefix"]}',{row.dbRow[0]})\">");
                    }
                    else
                    {
                        sb($"<tr {strTrClass}>");
                    }
                    
                }
                
                for (int i = 0; i <= _headers.Count - 1; i++)
                {
                    strVal = ""; strTdVal = "";
                    if (row.SubTotalLevel == 0)
                    {
                        strVal = GetVal(row.dbRow, i, _types[i].ToLower());
                        strDataSort = GetDataSort(row.dbRow, i, _types[i].ToLower());
                    }
                    else
                    {
                        if (i == 0 && row.SubTotalLevel > 0) strVal = row.subval1;
                        if (i == 1 && row.SubTotalLevel > 0) strVal = row.subval2;
                        if (i == 2 && row.SubTotalLevel > 0) strVal = row.subval3;


                        if (i == 0 && row.SubTotalLevel == 1)
                        {
                            strVal = "Σ " + row.subval1;
                        }
                        if (i == 1 && row.SubTotalLevel == 2)
                        {
                            strVal = "∑ " + row.subval2;
                        }
                        if (i == 2 && row.SubTotalLevel == 3)
                        {
                            strVal = "∑ " + row.subval3;
                        }

                        if (_subtotals[i] == "11")
                        {
                            strVal = GetVal(row.dbRow, i, _types[i].ToLower());
                        }
                    }

                    strTdVal = strVal;

                    if (_SubTotalColsCount > 0 && i == 0 && strVal == strLastVal0) strTdVal = null;
                    if (_SubTotalColsCount > 1 && i == 1 && strVal == strLastVal1) strTdVal = null;
                    


                    styles = new List<string>();
                    sb("<td");
                    if (_styles[i] != "" && _styles[i] != "0")
                    {
                        styles.Add(_styles[i]);
                    }
                    if (_types[i].ToLower() == "n" || _types[i].ToLower() == "n0" || _types[i].ToLower() == "hhmm")
                    {
                        styles.Add("text-align:right");
                        styles.Add("padding-right:10px");

                    }
                    
                    if (_types[i].ToLower() == "rcm")
                    {
                        styles.Add("width:20px");
                    }
                    if (i > 0 && row.SubTotalLevel == 2)
                    {
                        styles.Add($"font-weight:bold;background-color:{_backcolors[1]};color:{_forecolors[1]}'");
                        

                    }
                    if (i > 1 && row.SubTotalLevel == 3)
                    {
                        styles.Add($"background-color:{_backcolors[2]};color:{_forecolors[2]}'");
                    }
                    if (styles.Count > 0)
                    {
                        sb(" style='");
                        sb(string.Join(";", styles));
                        sb("'");
                    }



                    if (strDataSort != null)
                    {
                        sb(string.Format(" data-sort='{0}'", strDataSort));           //plugin datatables tomuto rozumí                     
                    }

                    sb(">");
                    sb(strTdVal);
                    sb("</td>");

                    if (i == 0) strLastVal0 = strVal;
                    if (i == 1) strLastVal1 = strVal;
                    if (i == 2) strLastVal2 = strVal;
                }
                sb("</tr>");




                x += 1;
                if (x > intLimitRows)
                {
                    break;  //omezovač na extrémně dlouhé tabulky
                }
            }

            sb("</tbody>");
        }

        private string GetVal(DataRow dbRow, int dbIndex, string strType)
        {
            if (dbRow[dbIndex] == System.DBNull.Value) return "";

            switch (strType)
            {
                case "rcm":
                    return string.Format("<a class='cm' onclick=\"_cm(event, '{0}',{1})\">☰</a>", dbRow["prefix"], dbRow[dbIndex]);

                case "b":
                    if (Convert.ToBoolean(dbRow[dbIndex]))
                    {
                        return "&#10004;";
                    }
                    else
                    {
                        return "";
                    }
                case "n":
                    return string.Format("{0:#,0.00}", dbRow[dbIndex]);
                case "hhmm":
                    if (dbRow[dbIndex]== System.DBNull.Value)
                    {
                        return null;
                    }
                    return BO.Code.Time.ShowAssHHMM(Convert.ToDouble(dbRow[dbIndex]));
                case "n0":
                case "i":
                    return string.Format("{0:#,0}", dbRow[dbIndex]);

                case "d":
                    return Convert.ToDateTime(dbRow[dbIndex]).ToString("dd.MM.yyyy");

                case "dt":
                    return Convert.ToDateTime(dbRow[dbIndex]).ToString("dd.MM.yyyy HH:mm");
                case "dtx":
                    return BO.Code.Time.GetTimestamp(Convert.ToDateTime(dbRow[dbIndex]));
                case "dty":
                    string s= BO.Code.Time.DurationFormatted(Convert.ToDateTime(Convert.ToDateTime(dbRow[dbIndex])), DateTime.Now, true);
                    return BO.Code.Bas.ObjectDateTime2String(Convert.ToDateTime(dbRow[dbIndex])) + " (-" + s + ")";

                case "a":
                    return dbRow[dbIndex].ToString().Replace("€€", "\"").Replace("$$", "'");    //link, odkaz
                case "js":
                    string strJS = dbRow[dbIndex].ToString();
                    return $"<a href='#' onclick=\"{strJS}\">Detail</a>";

                default:
                    return dbRow[dbIndex].ToString();

            }
        }
        private string GetDataSort(DataRow dbRow, int dbIndex, string strType)
        {
            if (dbRow[dbIndex] == System.DBNull.Value) return null;
            switch (strType)
            {
                case "b":
                    if (Convert.ToBoolean(dbRow[dbIndex]))
                    {
                        return "1";
                    }
                    else
                    {
                        return null;
                    }
                case "n":
                case "n0":
                case "i":
                    return dbRow[dbIndex].ToString();

                case "d":
                    return Convert.ToDateTime(dbRow[dbIndex]).ToString("yyyy-MM-dd");

                case "dt":
                    return Convert.ToDateTime(dbRow[dbIndex]).ToString("yyyy-MM-dd HH:mm");

            }

            return null;
        }

        private void sb(string s)
        {
            _sb.Append(s);
        }
        private void sbl(string s)
        {
            _sb.AppendLine(s);
        }



        //generovat xls výstup

        public string CreateXlsFile(System.Data.DataTable dt)
        {

            prepare_buffer(dt);
            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Export");
                int row = 1;
                int col = 1;

                for (int i = 0; i < _headers.Count(); i++)
                {
                    worksheet.Cell(row, col).Value = _headers[i];
                    worksheet.Cell(row, col).Style.Font.Bold = true;
                    col += 1;
                }

                int lastrow=handle_body_xls(worksheet);
                handle_foot_xls(worksheet, lastrow);

                worksheet.Columns(1, col).AdjustToContents();
                workbook.SaveAs(_f.TempFolder+"\\"+this.Guid+".xlsx");
            }

            return this.Guid+".xlsx";
        }

        
        private int handle_body_xls(IXLWorksheet worksheet) //vrací naposledy použité row
        {
            int col = 1;
            int row = 2;

            string strVal = "";
            bool bolValIsEmpty = true;
            XLCellValue cellval;            
            string strLastVal0 = null; string strLastVal1 = null; string strLastVal2 = null;

           
            
            var lisBuffer = _buffer.OrderBy(p => p.RowIndex).ThenBy(p => p.SubTotalLevel);
            foreach (var bufrow in lisBuffer)
            {

                col = 1;
                for (int i = 0; i <= _headers.Count - 1; i++)
                {
                    strVal = ""; cellval = "";
                    if (bufrow.SubTotalLevel == 1)
                    {
                        worksheet.Cell(row, col).Style.Font.Bold = true;
                        worksheet.Cell(row, col).Style.Fill.BackgroundColor = XLColor.LightGray;

                    }

                    
                    if (bufrow.SubTotalLevel == 0)
                    {
                        
                        cellval = GetValXls(bufrow.dbRow, i, _types[i].ToLower());
                        strVal = GetVal(bufrow.dbRow, i, _types[i].ToLower());

                    }
                    else
                    {                        

                        if (i > 0 && bufrow.SubTotalLevel == 2)
                        {
                            worksheet.Cell(row, col).Style.Font.Bold = true;
                            worksheet.Cell(row, col).Style.Fill.BackgroundColor = XLColor.LightBlue;

                        }
                        if (i > 1 && bufrow.SubTotalLevel == 3)
                        {
                            worksheet.Cell(row, col).Style.Fill.BackgroundColor = XLColor.LightYellow;
                            
                        }

                        if (i == 0 && bufrow.SubTotalLevel > 0)
                        {
                            cellval = bufrow.subval1;                            
                            strVal = bufrow.subval1;
                        }
                        if (i == 1 && bufrow.SubTotalLevel > 0)
                        {                            
                            cellval = bufrow.subval2; strVal = bufrow.subval2;
                        }
                        if (i == 2 && bufrow.SubTotalLevel > 0)
                        {
                            cellval = bufrow.subval3; strVal = bufrow.subval3;
                        }


                        if (i == 0 && bufrow.SubTotalLevel == 1)
                        {
                            cellval = "Σ " + bufrow.subval1;
                            if (_types[i] == "HTML") cellval = "Σ " + BO.Code.Bas.Html2Text(bufrow.subval1);
                            strVal = "Σ " + bufrow.subval1;
                        }
                        if (i == 1 && bufrow.SubTotalLevel == 2)
                        {
                            cellval = "∑ " + bufrow.subval2;
                            if (_types[i] == "HTML") cellval = "Σ " + BO.Code.Bas.Html2Text(bufrow.subval2);
                            strVal = "Σ " + bufrow.subval2;
                        }
                        if (i == 2 && bufrow.SubTotalLevel == 3)
                        {
                            cellval = "∑ " + bufrow.subval3;
                            if (_types[i] == "HTML") cellval = "Σ " + BO.Code.Bas.Html2Text(bufrow.subval3);
                            strVal = "Σ " + bufrow.subval3;
                        }

                        if (_subtotals[i] == "11")
                        {
                            cellval = GetValXls(bufrow.dbRow, i, _types[i].ToLower());
                            strVal = GetVal(bufrow.dbRow, i, _types[i].ToLower());
                        }
                    }

                    bolValIsEmpty = false;
                    if (_SubTotalColsCount > 0 && i == 0 && strVal == strLastVal0) bolValIsEmpty = true;
                    if (_SubTotalColsCount > 1 && i == 1 && strVal == strLastVal1) bolValIsEmpty = true;



                    if (!bolValIsEmpty)
                    {
                        worksheet.Cell(row, col).Value = cellval;
                    }
                    
                    if (_types[i] == "N" || _types[i] == "HHMM")
                    {
                        worksheet.Cell(row, col).Style.NumberFormat.Format = "#,##0.00";
                    }

                    if (i == 0) strLastVal0 = strVal;
                    if (i == 1) strLastVal1 = strVal;
                    if (i == 2) strLastVal2 = strVal;
                    col += 1;
                }



                row += 1;
               
            }

            return row;
        }

        private void handle_foot_xls(IXLWorksheet worksheet,int row)
        {
            if (_buffer.Count() == 0 || !_subtotals.Contains("11")) return;

            worksheet.Cell(row, 1).Value = "∑";

            for (int i = 0; i < _headers.Count(); i++)
            {
                worksheet.Cell(row, i+1).Style.Font.Bold = true;
                worksheet.Cell(row, i+1).Style.Fill.BackgroundColor = XLColor.Silver;
               
                if (_subtotals[i] == "11")
                {
                    var qry = _buffer.Where(p => p.SubTotalLevel == 0 && p.dbRow[i] != System.DBNull.Value);
                    if (qry.Count() > 0)
                    {
                        worksheet.Cell(row,i+1).Value= qry.Sum(p => Convert.ToDouble(p.dbRow[i]));
                        worksheet.Cell(row, i+1).Style.NumberFormat.Format = "#,##0.00";
                    }
                }


               
            }

            
        }


        private XLCellValue GetValXls(DataRow dbRow, int dbIndex, string strType)
        {
            if (dbRow[dbIndex] == System.DBNull.Value) return "";

            switch (strType)
            {
                case "rcm":
                    return dbRow[dbIndex].ToString();
                    

                case "b":
                    return Convert.ToBoolean(dbRow[dbIndex]);
                   
                case "n":
                case "hhmm":
                    return Convert.ToDouble(dbRow[dbIndex]);
                    
                case "n0":
                case "i":                    
                    return Convert.ToInt32(dbRow[dbIndex]);

                case "d":
                    return Convert.ToDateTime(dbRow[dbIndex]);

                case "dt":
                    return Convert.ToDateTime(dbRow[dbIndex]);

                case "a":
                    return dbRow[dbIndex].ToString();
                case "html":
                    string s = BO.Code.Bas.Html2Text(dbRow[dbIndex].ToString());
                    return s;
                default:
                    return dbRow[dbIndex].ToString();

            }
        }
    }
}
