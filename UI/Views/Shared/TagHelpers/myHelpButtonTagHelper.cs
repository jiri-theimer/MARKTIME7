using Microsoft.AspNetCore.Razor.Runtime.TagHelpers;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace UI.Views.Shared.TagHelpers
{
    // You may need to install the Microsoft.AspNetCore.Razor.Runtime package into your project
    [HtmlTargetElement("myhelpbutton")]
    public class myHelpButtonTagHelper : TagHelper
    {
        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            output.Content.AppendHtml($"<a class='btn btn-light' title='Nápověda' href='javascript:_helppage()'><span class='material-icons-outlined-btn'>help_outline</span></a>");
        }
    }
}
