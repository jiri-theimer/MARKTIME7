using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Razor.Runtime.TagHelpers;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc;

namespace UI.Views.Shared.TagHelpers
{
    // You may need to install the Microsoft.AspNetCore.Razor.Runtime package into your project
    [HtmlTargetElement("mynumber")]
    public class myNumberTagHelper : TagHelper
    {
        
        private const string ForAttributeName = "asp-for";

        [HtmlAttributeName(ForAttributeName)]
        public ModelExpression For { get; set; }


        private const string ForDecimalDigits = "decimal-digits";
        [HtmlAttributeName(ForDecimalDigits)]
        public int DecimalDigits { get; set; } = 2;

        private string _StringValue { get; set; } //tvar hodnoty čísla pro jeho uložení na hostitelské view

        [HtmlAttributeName("elementid-prefix")]
        public string elementidprefix { get; set; } //použitelné v situaci taghelperu v listu, který je umístěn v partial view komponentě

        [HtmlAttributeName("placeholder")]
        public string PlaceHolder { get; set; }

        [HtmlAttributeName("event_after_changevalue")]
        public string Event_After_ChangeValue { get; set; }

        [HtmlAttributeName("cssstyle")]
        public string CssStyle { get; set; }
        [HtmlAttributeName("stringsuffix")]
        public string StringSuffix { get; set; }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            output.TagMode = TagMode.StartTagAndEndTag;
            var sb = new System.Text.StringBuilder();
            
            
            string strStep = "0.01";
            string strPlaceHolder = "0,00";
            string strFormat = "{0:#,0.00}";
            string strFormatted = "0";
            switch (this.DecimalDigits)
            {
                case 0:
                    strStep = "";
                    strPlaceHolder = "000";
                    strFormat = "{0:#,0}";
                    break;
                case 1:
                    strStep = "0.1";
                    strPlaceHolder = "0,0";
                    break;
                case 2:
                    strStep = "0.01";
                    strPlaceHolder = "0,00";
                    break;
                case 3:
                    strStep = "0.001";
                    strPlaceHolder = "0,000";
                    strFormat = "{0:#,0.000}";
                    break;
                case 4:
                    strStep = "0.0001";
                    strPlaceHolder = "0,0000";
                    strFormat = "{0:#,0.0000}";
                    break;
                case 5:
                    strStep = "0.00001";
                    strPlaceHolder = "0,00000";
                    strFormat = "{0:#,0.00000}";
                    break;
                default:
                    strStep = "any";
                    strPlaceHolder = "0,00";                    
                    break;
            }
            if ( this.For.Model != null)
            {
                _StringValue = this.For.Model.ToString().Replace(".",",");
                strFormatted = String.Format(strFormat, this.For.Model);
                
            }
            else
            {
                _StringValue = "0";
                
            }
            string strControlID = this.For.Name;
            string strControlName = this.For.Name;
            if (this.elementidprefix != null)
            {
                strControlID = this.elementidprefix + strControlID;
                strControlName = this.elementidprefix + this.For.Name;
            }
            strControlID=strControlID.Replace(".", "_").Replace("[", "_").Replace("]", "_");

            if (this.PlaceHolder != null) strPlaceHolder = this.PlaceHolder;

            if (this.StringSuffix != null)
            {
                sb.Append("<div class='input-group'>");
            }
            


            sb.Append($"<input type='text' for-id='{strControlID}' id='num{strControlID}' class='form-control' step='{strStep}' placeholder='{strPlaceHolder}' onfocus='mynumber_focus(this)' onblur='mynumber_blur(this,{DecimalDigits})' value='{strFormatted}'");

            if (this.CssStyle != null)
            {
                sb.Append($" style='{this.CssStyle}'");
            }
            if (this.Event_After_ChangeValue != null)
            {
                sb.Append($" onchange='{this.Event_After_ChangeValue}'");
            }
            sb.Append("/>");


            if (this.StringSuffix != null)
            {
                sb.Append($"<span class='input-group-text'>{this.StringSuffix}</span></div>");
            }
            

            sb.Append($"<input type='hidden' value ='{_StringValue}' id ='{strControlID}' name ='{strControlName}'/>");

            //output.Content.AppendHtml(sb.ToString());
            output.Content.SetHtmlContent(sb.ToString());
        }
    }
}
