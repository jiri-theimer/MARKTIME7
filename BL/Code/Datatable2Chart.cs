using System;
using System.Collections.Generic;
using System.Text;
using System.Data;

namespace BL.Code
{
    public class Datatable2Chart
    {
        private System.Text.StringBuilder _sb;
        private List<string> _headers;
        private string _guid { get; set; }

        public Datatable2Chart()
        {
            _sb = new StringBuilder();
            _guid = BO.Code.Bas.GetGuid();
            
            
        }
        public string CreateGoogleChartHtml(System.Data.DataTable dt, BO.x55Widget rec)
        {

            _headers = BO.Code.Bas.ConvertString2List(rec.x55ChartHeaders, "|");
            wr(""); wr("");
            wr($"<div id='myChart{_guid}' style='width:100%; height:400px;'></div>"); wr("");
            wr("<script type='text/javascript'>"); wr("");


            wr("google.charts.load('current', {'packages':['corechart']});");
            wr($"google.charts.setOnLoadCallback(drawChart{_guid});"); wr("");


            wr("function drawChart" + _guid + "() {");
            wr("var data = google.visualization.arrayToDataTable(");
            wr("[");


            wr("['" + string.Join("','", _headers) + "']");

            int x = 0;

            foreach (System.Data.DataRow dbRow in dt.Rows)
            {
                _sb.Append(",[");

                for (int i = 0; i <= _headers.Count - 1; i++)
                {
                    if (i > 0)
                    {
                        _sb.Append(",");
                    }


                    if (i == 0)
                    {
                        if (dbRow[i] == System.DBNull.Value)
                        {
                            _sb.Append("'?'");
                        }
                        else
                        {
                            _sb.Append("'" + BO.Code.Bas.OM2(dbRow[i].ToString(), 25) + "'");
                        }


                    }
                    else
                    {
                        if (dbRow[i] == System.DBNull.Value || dbRow[i] == null)
                        {
                            _sb.Append("0");
                        }
                        else
                        {
                            _sb.Append(dbRow[i].ToString().Replace(",", "."));
                        }

                    }



                }



                wr("]");
                x += 1;
            }


            wr("]");
            wr(");");
            wr("");
            //wr("var options = {title:'Nadpis grafu'};");
            //,chartArea:{left:0,top:0,width:'90%',height:'90%'}
            //, {legend: { position: ['top','right']}}

            string strHeight = "100%";
            string strBarWidth = "60%";
            if (x > 15)
            {
                strHeight = (x * 25).ToString();
                strBarWidth = "95%";
            }
            if (rec.x55ChartHeight > 0) strHeight = rec.x55ChartHeight.ToString();

            string strStacked = (rec.x55ChartType == "StackedBar" || rec.x55ChartType == "StackedColumn" ? "true" : "false");
            string strChartType = rec.x55ChartType;
            if (rec.x55ChartType == "StackedBar")
            {
                strChartType = "Bar";
            }
            if (rec.x55ChartType == "StackedColumn")
            {
                strChartType = "Column";
            }

            if (rec.x55ChartType == "Donut")
            {
                strChartType = "Pie";
            }


            string strSeries = null;
            if (rec.x55ChartColors != null)
            {
                var lis = BO.Code.Bas.ConvertString2List(rec.x55ChartColors, "|");
                if (strChartType == "Pie")
                {
                    strSeries = $",colors: ['#e0440e', '#e6693e', '#ec8f6e', '#f3b49f', '#f6c7b6']";
                    strSeries = $",colors: [{string.Join(",", lis.Select(p => "'" + p + "'"))}]";
                }
                else
                {
                    for (int ii = 0; ii < lis.Count; ii++)
                    {
                        if (ii > 0) strSeries += " ,";
                        strSeries += ii.ToString() + ": {color: '" + lis[ii] + "'}";
                    }
                    strSeries = ",series: {" + strSeries + "}";
                }
            }

            wr("var options = {");
            wr("chartArea: {left:40, top:10, height: '84%',width:'72%'}, legend: { position: ['top','right'],interpolateNulls: true}, bar: { groupWidth: '" + strBarWidth + "' },isStacked: " + strStacked + ", vAxis : { textStyle : { fontSize: 11} },hAxis : { textStyle : { fontSize: 12} }, height:'" + strHeight + "'" + strSeries);
            if (rec.x55ChartType == "Donut")
            {
                wr(",pieHole: 0.4");
            }
            wr("};");
            wr("");
            wr($"var chart = new google.visualization.{strChartType}Chart(document.getElementById('myChart{_guid}'));");
            wr("chart.draw(data, options);");
            wr("}");

            
            wr("");
            wr("</script>");

            //BO.Code.File.LogInfo(_sb.ToString());

            return _sb.ToString();
        }

        private void wr(string s)
        {
            _sb.AppendLine(s);


        }
    }




}
