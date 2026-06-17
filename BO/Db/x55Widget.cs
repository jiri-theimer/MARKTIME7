using Microsoft.VisualBasic;
using System.ComponentModel.DataAnnotations;

namespace BO
{
    
    public enum x55DataTablesBtns
    {
        None=0,
        Export=1,
        ExportPrint=2,
        ExportPrintPdf=3
    }
    public class x55Widget : BaseBO
    {
        [Key]
        public int x55ID { get; set; }
       public int x01ID { get; set; }
        public int x04ID { get; set; }
        public string x55Name { get; set; }
        public string x55Code { get; set; }
        public int x55Ordinal { get; set; }
        public string x55Content { get; set; }
        public string x55Description { get; set; }
        public string x55TableSql { get; set; }
        public string x55TableColHeaders { get; set; }
        public string x55TableColTypes { get; set; }
        public string x55Image { get; set; }
        public string x55BoxBackColor { get; set; }
        public string x55HeaderBackColor { get; set; }
        public string x55HeaderForeColor { get; set; }
        public int x55BoxMaxHeight { get; set; }        
        public int x55DataTablesLimit { get; set; }
        public x55DataTablesBtns x55DataTablesButtons { get; set; }
        public string x55Help { get; set; }
        public bool IsUseDatatables { get; set; }   //není db pole - naplní ho incializátor widgetů na stránce
        

        public string x55ChartSql { get; set; }
        public string x55ChartHeaders { get; set; }
        public string x55ChartType { get; set; }
        public int x58ID_Par1 { get; set; }
        public int x58ID_Par2 { get; set; }
        
        public int x55ChartHeight { get; set; }
        public string x55ChartColors { get; set; }
        public string x55Category { get; set; }
        public string x55TableColTotals { get; set; }
        public string Rezerva { get; set; }
        public string x55ReportCodes { get; set; }
        public string NamePlusCategory
        {
            get
            {
                if (this.x55Category == null)
                {
                    return this.x55Name;
                }
                else
                {
                    return $"{this.x55Name} ({this.x55Category})";
                }
            }
        }
        public string CssHeaderDiv { get
            {
                if (this.x55HeaderBackColor==null && this.x55HeaderForeColor == null)
                {
                    return null;
                }
                string s = "";
                if (this.x55HeaderBackColor != null)
                {
                    s += "background-color:" + this.x55HeaderBackColor + ";";
                }
                if (this.x55HeaderForeColor != null)
                {
                    s += "color:" + this.x55HeaderForeColor+";";
                }
                return "style='"+s+"'";
            }
        }
        public string CssContentDiv {
            get
            {
               
                string s = $"max-height: {(this.x55BoxMaxHeight<=0 ? 400 : this.x55BoxMaxHeight )}px;";
                if (this.x55BoxBackColor != null)
                {
                    s += "background-color:" + this.x55BoxBackColor + ";";
                }
                return "style='" + s + "'";
            }
        }
        public string HeaderImage {
            get
            {
                if (this.x55Image == null)
                {
                    return "flag";
                }
                else
                {
                    return this.x55Image;
                }
            } 
        }
    }
}
