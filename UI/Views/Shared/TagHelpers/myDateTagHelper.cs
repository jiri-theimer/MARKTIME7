
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace UI.Views.Shared.TagHelpers
{
    // You may need to install the Microsoft.AspNetCore.Razor.Runtime package into your project
    [HtmlTargetElement("mydate")]
    public class myDateTagHelper : TagHelper
    {        
        private const string ForAttributeName = "asp-for";

        [HtmlAttributeName(ForAttributeName)]
        public ModelExpression For { get; set; }

        [HtmlAttributeName("multidate")]
        public bool multidate { get; set; }

        [HtmlAttributeName("include-time")]
        public bool includetime { get; set; }

        private string _StringValue { get; set; } //tvar hodnoty čísla pro jeho uložení na hostitelské view
        private string _TimeValue { get; set; }

        [HtmlAttributeName("elementid-prefix")]
        public string elementidprefix { get; set; } //použitelné v situaci taghelperu v listu, který je umístěn v partial view komponentě

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            output.TagMode = TagMode.StartTagAndEndTag;

            


            if (this.For.Model != null)
            {
                if (!this.multidate)
                {
                    _StringValue = Convert.ToDateTime(this.For.Model).ToString("dd.MM.yyyy", System.Globalization.CultureInfo.InvariantCulture);
                    if (includetime == true) _TimeValue = Convert.ToDateTime(this.For.Model).ToString("HH:mm", System.Globalization.CultureInfo.InvariantCulture);
                }
                else
                {
                    _StringValue = this.For.Model.ToString();   //multidate nechat jako string
                }
                
            }
            else
            {
                _StringValue = "";
                //_TimeValue = "00:00";
                _TimeValue = "";
            }

            var sb = new System.Text.StringBuilder();
            string strControlID = this.For.Name;
            string strControlName = this.For.Name;
            if (this.elementidprefix != null)
            {
                strControlID = this.elementidprefix + strControlID;
                strControlName = this.elementidprefix + this.For.Name;
            }
            strControlID = strControlID.Replace(".", "_").Replace("[", "_").Replace("]", "_");

            

            sb.AppendLine("<div class='input-group' style='width:100%;'>");
            sb.Append(string.Format("<input type='text' id='{0}' for-id='{1}' class='form-control' placeholder='dd.mm.yyyy' autocomplete='off' value='{2}' onchange='datepicker_change(this)'/>", strControlID+"helper", strControlID, _StringValue));

            if (this.includetime)
            {
                sb.Append(string.Format("<input type='time' class='form-control' placeholder='hh:mm' value='{0}' id='{1}' onchange='datepicker_time_change(this)' for-id='{2}' title='Přesný čas'/>", _TimeValue, strControlID+"_Time",strControlID));
            }
                        
            sb.Append(string.Format("<button id='{0}cmd' type='button' class='btn bog px-1 py-0' tabindex='-1' onclick='datepicker_button_click({1})'><span class='material-icons-outlined-btn' >event</span></button>", strControlID,"\""+strControlID+"helper"+"\""));
            
            sb.AppendLine("</div>");
            if (this.includetime)
            {
                sb.AppendLine(string.Format("<input type='hidden' value ='{0}' id='{1}' name='{2}'/>", _StringValue+" "+_TimeValue, strControlID, strControlName));
            }
            else
            {
                sb.AppendLine(string.Format("<input type='hidden' value ='{0}' id='{1}' name='{2}'/>", _StringValue, strControlID, strControlName));
            }
            

            sb.AppendLine("");


            sb.AppendLine("<script type='text/javascript'>");
            sb.AppendLine("$(document).ready(function () {");

            if (this.multidate)
            {
                sb.AppendLine(string.Format("datepicker_init_multidate('{0}');", strControlID + "helper"));
            }
            else
            {
                sb.AppendLine(string.Format("datepicker_init('{0}');", strControlID + "helper"));
            }
            
            sb.AppendLine("");

            sb.AppendLine("});");
            sb.AppendLine("</script>");


            output.Content.AppendHtml(sb.ToString());
        }
    }
}
