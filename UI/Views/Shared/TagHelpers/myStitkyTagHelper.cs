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
    [HtmlTargetElement("mystitky")]
    public class myStitkyTagHelper : TagHelper
    {
        private const string ForAttributeName = "asp-for";

        [HtmlAttributeName(ForAttributeName)]
        public ModelExpression For { get; set; }

        [HtmlAttributeName("entity")]
        public string Entity { get; set; }

        [HtmlAttributeName("buttontext")]
        public string ButtonText { get; set; }

        [HtmlAttributeName("tagnames")]
        public string SelectedTagNames { get; set; }

        [HtmlAttributeName("taghtml")]
        public string SelectedTagHtml { get; set; }

        private System.Text.StringBuilder _sb;

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {

            _sb = new System.Text.StringBuilder();
            string strSelectedValues = this.For.Model as string;

            
            
            _sb.AppendLine(string.Format("<input type='hidden' id='TagPids' name='TagPids' value='{0}' />", strSelectedValues));
            _sb.AppendLine(string.Format("<input type='hidden' id='TagHtml' name='TagHtml' value=\"{0}\" />", this.SelectedTagHtml));

            _sb.AppendLine("<div class='row'>");
            _sb.AppendLine("<div class='col-auto'>");
            //_sb.AppendLine("<div class='form-group'>");
            _sb.AppendLine(string.Format("<button id='cmdTagging' type='button' tabindex='-1' class='btn btn-sm btn-light' onclick='mystitky_multiselect(event,\"{0}\")'><span class='material-icons-outlined-btn'>local_offer</span> {1}</button>", this.Entity,this.ButtonText));
            //_sb.AppendLine("</div>");
            _sb.AppendLine("</div><div id='divTagHtml' class='col-auto'>");
                      

            //_sb.Append("<div id='divTagHtml'>");
            if (string.IsNullOrEmpty(this.SelectedTagHtml) == false)
            {
                _sb.Append(this.SelectedTagHtml);     
            }
            _sb.Append("</div>");

            _sb.Append("</div>");

            output.Content.AppendHtml(_sb.ToString());
        }
    }
}
