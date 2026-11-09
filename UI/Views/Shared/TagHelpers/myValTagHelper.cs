using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Razor.Runtime.TagHelpers;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using System.Text.Encodings.Web;

namespace UI.Views.Shared.TagHelpers
{
    // You may need to install the Microsoft.AspNetCore.Razor.Runtime package into your project
    [HtmlTargetElement("myval")]

    public class myValTagHelper : TagHelper
    {
        private readonly HtmlEncoder _enc;
        public myValTagHelper(HtmlEncoder enc)
        {
            _enc = enc;
        }

        private string E(object o) => o == null ? "" : _enc.Encode(o.ToString());   //escapování textu i hodnot atributů

        [HtmlAttributeName("value")]
        public object Value { get; set; }

        [HtmlAttributeName("datatype")]     //string | date | datetime | num | html | link
        public string DataType { get; set; } = "string";

        /// <summary>Text za hodnotou (měna, jednotka…) – escapuje se.</summary>
        [HtmlAttributeName("valueafter")]
        public string ValueAfter { get; set; }

        /// <summary>HTML za hodnotou (např. basUI.HtmlColorStrip) – NEescapuje se, nepoužívat pro data od uživatelů.</summary>
        [HtmlAttributeName("valueafter-html")]
        public string ValueAfterHtml { get; set; }

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
                output.Content.AppendHtml(string.Format("<a class=\"cm h4\" onclick=\"_cm(event, '{0}',{1})\">☰</a>", E(this.CmPrefix), this.CmPid));
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
                            output.Content.AppendHtml(string.Format("<a href=\"{0}\">{1}</a>", E(this.LinkUrl), E(this.Value)));
                        }
                        else
                        {
                            output.Content.AppendHtml(string.Format("<a target=\"{2}\" href=\"{0}\">{1}</a>", E(this.LinkUrl), E(this.Value), E(this.LinkTarget)));
                        }
                        
                        break;
                    case "bool":
                    case "boolean":
                        if (this.Value is bool b ? b : (this.Value != null && this.Value.ToString() != "" && this.Value.ToString() != "0" && !"false".Equals(this.Value.ToString(), StringComparison.OrdinalIgnoreCase)))     //dříve se fajfka zobrazovala i pro false
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

            if (!string.IsNullOrEmpty(this.ValueAfter))
            {
                output.Content.AppendHtml($"<span class='myval-after'>{E(this.ValueAfter)}</span>");
            }
            if (!string.IsNullOrEmpty(this.ValueAfterHtml))
            {
                output.Content.AppendHtml(this.ValueAfterHtml);
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
                    output.Content.AppendHtml($"<a class='reczoom' data-rel='{E(this.HoverUrl)}' data-height='{this.HoverHeight}'>{E(this.HoverSymbol)}</a>");
                }
                else
                {
                    output.Content.AppendHtml($"<a class='reczoom' data-title='{E(this.Value)}' data-rel='{E(this.HoverUrl)}' data-height='{this.HoverHeight}'>{E(this.HoverSymbol)}</a>");
                }
                
            }
            if (this.HoverInfo != null)
            {
                //bez alert(): text v data atributu (escapovaný), zobrazí se jako notifikace
                output.Content.AppendHtml($"<a class='valhover_tooltip' href='javascript:void(0)' title='{E(this.HoverInfo)}' data-info='{E(this.HoverInfo)}' onclick=\"_notify_message(this.getAttribute('data-info'),'info')\">{E(this.HoverSymbol)}</a>");
            }
            if (this.IsHoverOnClick && !string.IsNullOrEmpty(this.HoverUrl))
            {
                if (this.HoverUrl.Contains("javascript"))
                {
                    output.Content.AppendHtml(string.Format("<a class='valhover_tooltip' href=\"{0}\">{1}</a>", E(this.HoverUrl), E(this.HoverSymbol)));
                }
                else
                {
                    if (this.LinkTarget == null) this.LinkTarget = "_blank";
                    output.Content.AppendHtml(string.Format("<a target='{0}' class='valhover_tooltip' href=\"{1}\">{2}</a>", E(this.LinkTarget), E(this.HoverUrl), E(this.HoverSymbol)));
                }
            }

        }
    }
}
