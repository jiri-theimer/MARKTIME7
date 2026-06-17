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
    [HtmlTargetElement("myautocomplete")]
    public class myAutoCompleteTagHelper : TagHelper
    {
        private const string ForAttributeName = "asp-for";

        [HtmlAttributeName(ForAttributeName)]
        public ModelExpression For { get; set; }

        [HtmlAttributeName("placeholder")]
        public string PlaceHolder { get; set; }
        [HtmlAttributeName("o15flag")]
        public string o15flag { get; set; }

        private System.Text.StringBuilder _sb;
        public override void Process(TagHelperContext context, TagHelperOutput output)
        {

            output.TagMode = TagMode.StartTagAndEndTag;
            


            _sb = new System.Text.StringBuilder();
            var strControlID = this.For.Name.Replace(".", "_").Replace("[", "_").Replace("]", "_");
           
            sb(string.Format("<input id='{0}' class='form-control' placeholder='{1}' autocomplete='on' value='{2}' name='{3}'/>", strControlID, this.PlaceHolder, this.For.Model, this.For.Name));

               
            sb("");
            sb("");


            sb("<script type='text/javascript'>");
            _sb.Append(string.Format("var c{0}=", strControlID));
            _sb.Append("{");
            _sb.Append(string.Format("controlid: '{0}',posturl: '/TheCombo/GetAutoCompleteHtmlItems',o15flag:'{1}'", strControlID,this.o15flag));
            _sb.Append("};");

            sb("");
            sb(string.Format("myautocomplete_init(c{0});", strControlID));

            sb("");
            sb("</script>");

            output.Content.AppendHtml(_sb.ToString());

            




        }



        private void sb(string s)
        {
            _sb.AppendLine(s);

        }
    }
}
