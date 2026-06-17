using Microsoft.AspNetCore.Razor.Runtime.TagHelpers;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace UI.Views.Shared.TagHelpers
{
    // You may need to install the Microsoft.AspNetCore.Razor.Runtime package into your project
    [HtmlTargetElement("myshowmore")]
    public class myShowMore : TagHelper
    {
        [HtmlAttributeName("classname_target")]
        public string ToggleClassnameTarget { get; set; }


        [HtmlAttributeName("isvertical")]
        public bool IsVertical { get; set; }

        public string Label { get; set; }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            if (string.IsNullOrEmpty(this.ToggleClassnameTarget))
            {
                this.ToggleClassnameTarget = "showmore";
            }
            if (this.IsVertical)
            {
                output.Content.AppendHtml($"<a tabindex='-1' class='cmdshowmore d-inline-flex align-items-center gap-1' href='javascript:_hrefnic()' onclick=\"_showmorevertical_toggle(this,'{this.ToggleClassnameTarget}','{this.Label}')\">");
                if (this.Label != null)
                {
                    output.Content.AppendHtml($"<span>{this.Label}</span>");
                }
                
                output.Content.AppendHtml("<span class='material-icons'>keyboard_arrow_down</span>");
                
                output.Content.AppendHtml("</a>");
            }
            else
            {
                output.Content.AppendHtml($"<a tabindex='-1' title='Více' class='cmdshowmore' href='javascript:_hrefnic()' onclick=\"_showmore_toggle(this,'{this.ToggleClassnameTarget}')\">");
                output.Content.AppendHtml("<span class='material-icons'>keyboard_double_arrow_right</span>");
                output.Content.AppendHtml("</a>");
            }
            
        }
    }
}
