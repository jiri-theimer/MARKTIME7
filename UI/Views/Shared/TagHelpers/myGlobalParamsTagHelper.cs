using Microsoft.AspNetCore.Razor.Runtime.TagHelpers;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace UI.Views.Shared.TagHelpers
{
    // You may need to install the Microsoft.AspNetCore.Razor.Runtime package into your project
    [HtmlTargetElement("myglobalparams")]
    public class myGlobalParamsTagHelper : TagHelper
    {
        [HtmlAttributeName("prefix")]
        public string Prefix { get; set; }

        [HtmlAttributeName("pid")]
        public int Pid { get; set; }
        public override void Process(TagHelperContext context, TagHelperOutput output)
        {

            output.Content.AppendHtml($"<button type='button' class='btn bog' onclick=\"_window_open('/GlobalParams/Index?prefix={this.Prefix}&pid={this.Pid}')\">");
            output.Content.AppendHtml("<span class='material-icons-outlined-btn'>settings</span>Doplňující parametry");
            output.Content.AppendHtml("</button>");

        }
    }
}
