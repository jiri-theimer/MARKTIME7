
using BO;
using ClosedXML.Excel;
using DocumentFormat.OpenXml.Spreadsheet;

//using SQLitePCL;

namespace UI.Code
{
    public class dataExport
    {

        
        public bool ToXLSX(System.Data.DataTable dt, string strFilePath, BO.baseQuery mq)
        {
            
            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Grid");
                int row = 1;
                int col = 1;

                foreach (var c in mq.explicit_columns)
                {
                    worksheet.Cell(row, col).Value = c.Header;
                    worksheet.Cell(row, col).Style.Font.Bold = true;

                    col += 1;
                }
                row += 1;
                string ss = null;
                foreach (System.Data.DataRow dr in dt.Rows)
                {
                    col = 1;
                    foreach (var c in mq.explicit_columns)
                    {

                        if (!Convert.IsDBNull(dr[c.UniqueName]))
                        {
                            switch (dr[c.UniqueName].GetType().ToString())
                            {
                                
                                case "System.DateTime":
                                    worksheet.Cell(row, col).Value = Convert.ToDateTime(dr[c.UniqueName]);
                                    break;
                                case "System.Double":
                                    worksheet.Cell(row, col).Value = Convert.ToDouble(dr[c.UniqueName]);
                                    break;
                                case "System.Int32":
                                    worksheet.Cell(row, col).Value = Convert.ToInt32(dr[c.UniqueName]);
                                    break;
                                case "System.Boolean":
                                    worksheet.Cell(row, col).Value = Convert.ToBoolean(dr[c.UniqueName]);
                                    break;
                                default:
                                    ss = dr[c.UniqueName].ToString();
                                    if (ss.Contains("<") && ss.Contains(">"))
                                    {
                                        ss = BO.Code.Bas.Html2Text(ss);
                                    }                                    
                                    worksheet.Cell(row, col).Value = ss;
                                    break;
                            }

                            


                        }
                        col += 1;
                    }

                    row += 1;

                }
                worksheet.Columns(1, col).AdjustToContents();


                workbook.SaveAs(strFilePath);

            }

            return true;
        }
        public bool ToXLSX(System.Data.DataTable dt, string strFilePath, string strXlsTemplateFullPath)
        {
            //key: název pole, value: záhlaví sloupce
            using (var workbook = new XLWorkbook(strXlsTemplateFullPath))
            {
                var worksheet = workbook.Worksheets.First();
                int row = 2;
                int col = 1;

                

                var coltypes = new List<StringPair>();
                foreach (System.Data.DataColumn c in dt.Columns)
                {
                    coltypes.Add(new BO.StringPair() { Key = c.ColumnName, Value = c.DataType.Name });
                }
               
                foreach (System.Data.DataRow dr in dt.Rows)
                {
                    col = 1;
                    foreach (var c in coltypes)
                    {
                        if (string.IsNullOrEmpty(worksheet.Cell(row, col).Style.NumberFormat.Format) && string.IsNullOrEmpty(worksheet.Cell(row, col).Style.DateFormat.Format))
                        {
                            switch (dr[c.Key].GetType().ToString())
                            {

                                case "System.DateTime":
                                    worksheet.Cell(row, col).Style.DateFormat.Format = "dd.MM.yyyy";                                    
                                    break;
                                case "System.Double":
                                    worksheet.Cell(row, col).Style.NumberFormat.NumberFormatId = 4;                                    
                                    break;
                                case "System.Int32":
                                    worksheet.Cell(row, col).Style.NumberFormat.NumberFormatId = 3;
                                    break;
                                
                                default:                                    
                                    break;
                            }
                        }
                        

                        if (!Convert.IsDBNull(dr[c.Key]))
                        {
                            switch (dr[c.Key].GetType().ToString())
                            {

                                case "System.DateTime":
                                    
                                    worksheet.Cell(row, col).Value = Convert.ToDateTime(dr[c.Key]);
                                    break;
                                case "System.Double":
                                    worksheet.Cell(row, col).Value = Convert.ToDouble(dr[c.Key]);
                                    break;
                                case "System.Int32":
                                    worksheet.Cell(row, col).Value = Convert.ToInt32(dr[c.Key]);
                                    break;
                                case "System.Boolean":
                                    worksheet.Cell(row, col).Value = Convert.ToBoolean(dr[c.Key]);
                                    break;
                                default:
                                    worksheet.Cell(row, col).Value = dr[c.Key].ToString();
                                    break;
                            }
                            

                        }
                        col += 1;
                    }

                    row += 1;

                }
                //worksheet.Columns(1, col).AdjustToContents();
                if (workbook.Worksheets.Where(p => p.Name == "marktime_definition").Count() > 0)
                {
                    workbook.Worksheets.Delete("marktime_definition");
                }
                workbook.SaveAs(strFilePath);
                
                
            }

            return true;
        }
        public bool ToXLSX(System.Data.DataTable dt, string strFilePath, List<BO.StringPair> cols)
        {
            //key: název pole, value: záhlaví sloupce
            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Grid");
                int row = 1;
                int col = 1;
                
                foreach (var c in cols)
                {
                    worksheet.Cell(row, col).Value = c.Value;
                    worksheet.Cell(row, col).Style.Font.Bold = true;

                    col += 1;
                }

                var coltypes = new List<StringPair>();
                foreach (System.Data.DataColumn c in dt.Columns)
                {
                    coltypes.Add(new BO.StringPair() { Key = c.ColumnName, Value = c.DataType.Name });
                }
                //worksheet.Column(1).CellsUsed().SetDataType(XLDataType.Text);
                row += 1;
                foreach (System.Data.DataRow dr in dt.Rows)
                {
                    col = 1;
                    foreach (var c in cols)
                    {
                        
                        if (!Convert.IsDBNull(dr[c.Key]))
                        {
                            worksheet.Cell(row, col).Value =(XLCellValue) dr[c.Key];



                        }
                        col += 1;
                    }

                    row += 1;

                }
                worksheet.Columns(1, col).AdjustToContents();
                workbook.SaveAs(strFilePath);

            }

            return true;
        }

        public bool ToCSV(System.Data.DataTable dt, string strFilePath, string strXlsTemplateFullPath,string strDelimiter,bool bolUvozovky)
        {
            System.IO.StreamWriter sw = new System.IO.StreamWriter(strFilePath, false, System.Text.Encoding.UTF8);
            
            if (strXlsTemplateFullPath !=null && System.IO.File.Exists(strXlsTemplateFullPath))
            {
                using (var workbook = new XLWorkbook(strXlsTemplateFullPath))   //názvy sloupců z prvního řádku
                {
                    var worksheet = workbook.Worksheets.First();
                    if (!string.IsNullOrEmpty(worksheet.Cell(1, 1).Value.ToString()))   //pokud je v první levé buňce něco vyplněno
                    {
                        int col = 1;
                        foreach (System.Data.DataColumn c in dt.Columns)
                        {
                            string val = worksheet.Cell(1, col).Value.ToString();

                            col += 1;

                            sw.Write(val);
                            sw.Write(strDelimiter);
                        }
                        sw.Write(sw.NewLine);
                    }                    
                }

            }
            
            var coltypes = new List<StringPair>();
            foreach (System.Data.DataColumn c in dt.Columns)
            {
                coltypes.Add(new BO.StringPair() { Key = c.ColumnName, Value = c.DataType.Name });
            }

            foreach (System.Data.DataRow dr in dt.Rows)
            {
                foreach (var c in coltypes)
                {
                    string val = "";


                    if (!Convert.IsDBNull(dr[c.Key]))
                    {
                        val = dr[c.Key].ToString();

                        if (dr[c.Key].GetType().ToString() == "System.String" && bolUvozovky)
                        {                           
                            val = "\"" + val + "\"";
                        }
                    }

                    sw.Write(val);
                    sw.Write(strDelimiter);
                }

                sw.Write(sw.NewLine);

            }

            sw.Close();

            

            return true;
        }

        public bool ToCSV(System.Data.DataTable dt, string strFilePath, BO.baseQuery mq,string strDelimiter, bool bolUvozovky)
        {
            System.IO.StreamWriter sw = new System.IO.StreamWriter(strFilePath, false, System.Text.Encoding.UTF8);
            //headers  
            foreach (var col in mq.explicit_columns)
            {
                sw.Write("\"" + col.Header + "\"");
                sw.Write(strDelimiter);
            }

            sw.Write(sw.NewLine);
            foreach (System.Data.DataRow dr in dt.Rows)
            {
                foreach (var col in mq.explicit_columns)
                {
                    string value = "";

                    if (!Convert.IsDBNull(dr[col.UniqueName]))
                    {
                        value = dr[col.UniqueName].ToString();
                       
                        if (value.Contains("<") && value.Contains(">"))
                        {
                            value = BO.Code.Bas.Html2Text(value);
                        }
                        

                        if (col.FieldType == "string" && bolUvozovky)
                        {
                            value = "\"" + value + "\"";

                        }
                    }
                    sw.Write(value);

                    sw.Write(strDelimiter);


                }

                sw.Write(sw.NewLine);

            }
            sw.Close();

            return true;
        }

        public string ToClipboardHtml(System.Data.DataTable dt, string strFilePath, BO.baseQuery mq, bool bolWithHeaders)
        {
            var sb = new System.Text.StringBuilder();
            sb.AppendLine("<table id='tabHtmlToClipboard'>");

            if (bolWithHeaders)
            {
                sb.AppendLine("<thead>");
                sb.AppendLine("<tr>");
                foreach (var col in mq.explicit_columns)
                {
                    sb.Append($"<th>{col.Header}</th>");
                }
                sb.AppendLine("</tr>");
                sb.AppendLine("</thead>");
            }
            sb.AppendLine("<tbody>");
            foreach (System.Data.DataRow dr in dt.Rows)
            {                
                sb.AppendLine("<tr>");
                foreach (var col in mq.explicit_columns)
                {
                    sb.Append("<td");
                    string strValue = "";

                    if (!Convert.IsDBNull(dr[col.UniqueName]))
                    {
                        switch (col.FieldType)
                        {
                            case "date":                            
                                strValue = BO.Code.Bas.ObjectDate2String(dr[col.UniqueName], "dd.MM.yyyy");
                                break;
                            case "datetime":
                                strValue = BO.Code.Bas.ObjectDate2String(dr[col.UniqueName], "dd.MM.yyyy HH:mm");
                                break;
                            case "int":
                                sb.Append(" class='tdn'");
                                strValue = BO.Code.Bas.Integer2String(Convert.ToInt32(dr[col.UniqueName]));
                                break;
                            case "num":
                                sb.Append(" class='tdn'");
                                strValue = BO.Code.Bas.Num2StringNull(Convert.ToDouble(dr[col.UniqueName]));
                                break;
                            case "bool":
                                strValue = BO.Code.Bas.GB(Convert.ToBoolean(dr[col.UniqueName]));
                                break;
                            default:
                                strValue = dr[col.UniqueName].ToString();
                                break;
                        }


                    }
                    sb.Append(">");
                    sb.Append(strValue);
                    sb.Append("</td>");


                }
                sb.AppendLine("</tr>");

            }
            sb.AppendLine("</tbody>");
            sb.AppendLine("</table>");

            return sb.ToString();
        }

        public string ToClipboardTabs(System.Data.DataTable dt, string strFilePath, BO.baseQuery mq, bool bolWithHeaders)
        {
            var sb = new System.Text.StringBuilder();

            if (bolWithHeaders)
            {
                sb.AppendLine(String.Join("\t", mq.explicit_columns.Select(p => p.Header)));
            }
            foreach (System.Data.DataRow dr in dt.Rows)
            {
                var cells = new List<string>();
                foreach (var col in mq.explicit_columns)
                {
                    string strValue = "";

                    if (!Convert.IsDBNull(dr[col.UniqueName]))
                    {
                        switch (col.FieldType)
                        {
                            case "date":
                                strValue = BO.Code.Bas.ObjectDate2String(dr[col.UniqueName], "dd.MM.yyyy");
                                break;
                            default:
                                strValue = stringToCsvSafe(dr[col.UniqueName].ToString());
                                break;
                        }


                    }
                    cells.Add(strValue);


                }

                sb.AppendLine(String.Join("\t", cells));


            }

            return sb.ToString();
        }

        public string stringToCsvSafe(string str)
        {
            return "\"" + str.Replace("\"", "\"\"") + "\"";
        }
    }
}
