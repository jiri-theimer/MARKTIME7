using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Razor.Runtime.TagHelpers;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace UI.Views.Shared.TagHelpers
{
    // You may need to install the Microsoft.AspNetCore.Razor.Runtime package into your project
    [HtmlTargetElement("myval")]

    public class myValTagHelper : TagHelper
    {        

        [HtmlAttributeName("value")]
        public object Value { get; set; }

        [HtmlAttributeName("datatype")]     //string | date | datetime | num | html | link
        public string DataType { get; set; } = "string";

        [HtmlAttributeName("valueafter")]
        public string ValueAfter { get; set; }

        [HtmlAttributeName("withoutcss")]
        public bool IsWithoutCssClass { get; set; }     //pokud true, pak není podklad šedý box: val-readonly

        [HtmlAttributeName("format")]
        public string Format { get; set; }

        [HtmlAttributeName("linkurl")]
        public string LinkUrl { get; set; }
        [HtmlAttributeName("linktarget")]
        public string LinkTarget { get; set; }

        [HtmlAttributeName("tooltip")]
        public string Tooltip { get; set; }

        [HtmlAttributeName("hoverprefix")]
        public string HoverPrefix { get; set; }

        [HtmlAttributeName("hoverheight")]
        public int HoverHeight { get; set; }

        [HtmlAttributeName("hoverpid")]
        public int HoverPid { get; set; }

        [HtmlAttributeName("hoverurl")]
        public string HoverUrl { get; set; }

        [HtmlAttributeName("hoveronclick")]
        public bool IsHoverOnClick { get; set; }

        [HtmlAttributeName("hoverinfo")]
        public string HoverInfo { get; set; }

        [HtmlAttributeName("hoversymol")]
        public string HoverSymbol { get; set; }

        [HtmlAttributeName("cmprefix")]
        public string CmPrefix { get; set; }
        [HtmlAttributeName("cmdpid")]
        public int CmPid { get; set; }

        [HtmlAttributeName("addcssclass")]
        public string AddCssClass { get; set; }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            output.TagMode = TagMode.StartTagAndEndTag;
            output.TagName = "div";
            if (this.HoverSymbol == null)
            {
                this.HoverSymbol = "ℹ";
                
            }
            string strClass = null;            
            if (this.HoverPrefix != null || this.HoverInfo != null || this.HoverUrl != null)
            {
                if (this.IsWithoutCssClass)
                {
                    strClass = "rowvalhover";
                }
                else
                {
                    strClass = "val-readonly-wrap rowvalhover";
                }                
            }
            else
            {
                if (!this.IsWithoutCssClass)
                {
                    strClass = "val-readonly";
                }                
            }
            if (this.AddCssClass != null) strClass += " " + this.AddCssClass;

            output.Attributes.SetAttribute("class", strClass);

           
            if (this.Tooltip != null)
            {
                output.Attributes.SetAttribute("title", this.Tooltip);
            }
            if (this.CmPid > 0)
            {
                output.Content.AppendHtml(string.Format("<a class=\"cm h4\" onclick=\"_cm(event, '{0}',{1})\">☰</a>", this.CmPrefix, this.CmPid));
            }
            if (this.Value != null)
            {
                
                switch (this.DataType)
                {
                    case "date":
                        if (this.Format == null)
                        {
                            output.Content.Append(BO.Code.Bas.ObjectDate2String(this.Value));
                        }
                        else
                        {
                            output.Content.Append(BO.Code.Bas.ObjectDate2String(this.Value, this.Format));
                        }
                        break;
                    case "datetime":
                        if (this.Format == null)
                        {
                            output.Content.Append(BO.Code.Bas.ObjectDateTime2String(this.Value));
                        }
                        else
                        {
                            output.Content.Append(BO.Code.Bas.ObjectDateTime2String(this.Value, this.Format));
                        }
                        break;
                    case "num":
                        
                        output.Content.AppendHtml(string.Format("<span class=\"numeric_reaodnly_110\">{0}</span>", BO.Code.Bas.Number2String(Convert.ToDouble(this.Value))));
                       
                        break;
                    case "html":
                        output.Content.AppendHtml(this.Value.ToString());
                        break;
                    case "link":
                        if (this.LinkTarget == null)
                        {
                            output.Content.AppendHtml(string.Format("<a href=\"{0}\">{1}</a>", this.LinkUrl, this.Value));
                        }
                        else
                        {
                            output.Content.AppendHtml(string.Format("<a target='{2}' href=\"{0}\">{1}</a>", this.LinkUrl, this.Value,this.LinkTarget));
                        }
                        
                        break;
                    case "bool":
                    case "boolean":
                        if (this.Value !=null || Convert.ToBoolean(this.Value))
                        {
                            output.Content.AppendHtml("<span style='font-size:140%;' class='material-icons'>done</span>");
                        }
                        
                        break;
                    case "string":
                    default:
                        output.Content.Append(this.Value.ToString());

                        break;
                }
                
            }

            if (this.ValueAfter != null)
            {                
                if (this.ValueAfter.Contains("<"))
                {
                    output.Content.AppendHtml(this.ValueAfter);
                }
                else
                {
                    output.Content.AppendHtml($"<span class='myval-after'>{this.ValueAfter}</span>");
                }
                
            }


            if (!this.IsHoverOnClick && (this.HoverPrefix != null || this.HoverPid>0 || this.HoverUrl !=null))
            {
                if (this.HoverUrl == null)
                {
                    this.HoverUrl = $"/{this.HoverPrefix}/Info?pid={this.HoverPid}&hover_by_reczoom=1";
                }
                
                if (this.HoverHeight == 0) this.HoverHeight = 270;
                //output.Content.AppendHtml(string.Format("<a class='valhover_tooltip' onclick=\"_zoom(event,'{0}',{1},900,'Info')\">{2}</a>", this.HoverPrefix, this.HoverPid, this.HoverSymbol));
                if (this.DataType == "html")
                {
                    output.Content.AppendHtml($"<a class='reczoom' data-rel='{this.HoverUrl}' data-height='{this.HoverHeight}'>{this.HoverSymbol}</a>");
                }
                else
                {
                    output.Content.AppendHtml($"<a class='reczoom' data-title='{this.Value}' data-rel='{this.HoverUrl}' data-height='{this.HoverHeight}'>{this.HoverSymbol}</a>");
                }
                
            }
            if (this.HoverInfo != null)
            {
                output.Content.AppendHtml(string.Format("<a class='valhover_tooltip' href=\"javascript: alert({0})\">{1}</a>", BO.Code.Bas.GS(this.HoverInfo),this.HoverSymbol));
            }
            if (this.IsHoverOnClick)
            {
                if (this.HoverUrl.Contains("javascript"))
                {
                    output.Content.AppendHtml(string.Format("<a class='valhover_tooltip' href=\"{0}\">{1}</a>", this.HoverUrl, this.HoverSymbol));
                }
                else
                {
                    if (this.LinkTarget == null) this.LinkTarget = "_blank";
                    output.Content.AppendHtml(string.Format("<a target='{0}' class='valhover_tooltip' href=\"{1}\">{2}</a>",this.LinkTarget,this.HoverUrl, this.HoverSymbol));
                }
                
            }

        }
    }
}
